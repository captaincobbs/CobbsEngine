using System.Text.Json;

namespace Engine.Core.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private readonly Lock _lock = new();
        private readonly SemaphoreSlim _fileLock = new(1, 1);

        private readonly string _settingsFilePath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        private readonly Dictionary<string, ISettingsSection> _sections = new(StringComparer.Ordinal);
        private readonly Dictionary<string, JsonElement> _unclaimedSections = new(StringComparer.Ordinal);

        private SettingsState _state = SettingsState.Registering;
        public SettingsApplyPhase Phase { get; private set; }

        public SettingsManager(string settingsFilePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(settingsFilePath);
            _settingsFilePath = Path.GetFullPath(settingsFilePath);
        }

        public void Register(params ISettingsSection[] sections) => Register(sections);

        public void Register(IEnumerable<ISettingsSection> sections)
        {
            ArgumentNullException.ThrowIfNull(sections, nameof(sections));
            ISettingsSection[] registrations = [.. sections];
            foreach (ISettingsSection section in registrations)
            {
                ArgumentNullException.ThrowIfNull(section);
                if (string.IsNullOrEmpty(section.Key))
                {
                    throw new ArgumentException("Settings section keys cannot be empty", nameof(sections));
                }
            }

            lock (_fileLock)
            {
                EnsureState(SettingsState.Registering);
                HashSet<string> seen = new(StringComparer.Ordinal);

                foreach (ISettingsSection section in registrations)
                {
                    if (!seen.Add(section.Key) || _sections.ContainsKey(section.Key))
                    {
                        throw new InvalidOperationException($"Settings section '{section.Key}' is already registered.");
                    }
                }

                foreach (ISettingsSection section in registrations)
                {
                    _sections.Add(section.Key, section);
                }
            }
        }

        public void Register<T>(ISettingsSection<T> section) where T : class
        {
            ArgumentNullException.ThrowIfNull(section, nameof(section));
            ArgumentException.ThrowIfNullOrWhiteSpace(section.Key, nameof(section));

            lock (_lock)
            {
                EnsureState(SettingsState.Registering);

                if (!_sections.TryAdd(section.Key, section))
                {
                    throw new InvalidOperationException($"Settings section '{section.Key}' is already registered.");
                }
            }
        }

        public T Get<T>(string key) where T : class
        {
            lock (_lock)
            {
                EnsureState(SettingsState.Loaded);

                if (_sections.TryGetValue(key, out ISettingsSection? section))
                {
                    throw new KeyNotFoundException($"No settings section registered as '{key}'.");
                }

                if (section is not ISettingsSection<T> typedSection)
                {
                    throw new InvalidOperationException($"Settings section '{key}' is not {typeof(T).Name}.");
                }

                return typedSection.Value;
            }
        }

        public bool TryGet<T>(string key, out T? settings) where T : class
        {
            lock (_lock)
            {
                EnsureState(SettingsState.Loaded);
                if (_sections.TryGetValue(key, out ISettingsSection? section) && section is ISettingsSection<T> typedSection)
                {
                    settings = typedSection.Value;
                    return true;
                }

                settings = null;
                return false;
            }
        }

        public SettingsUpdateResult<T> Update<T>(string key, Func<T, T> update) where T : class
        {
            ArgumentNullException.ThrowIfNull(update);

            lock (_lock)
            {
                EnsureState(SettingsState.Loaded);
                if (!_sections.TryGetValue(key, out ISettingsSection? section))
                {
                    throw new KeyNotFoundException($"No settings section is registered as '{key}'.");
                }

                if (section is not ISettingsSection<T> typedSection)
                {
                    throw new InvalidOperationException($"Settings section '{key}' is not {typeof(T).Name}.");
                }
                return typedSection.Update(update);
            }
        }

        public async Task<SettingsLoadResult> LoadAsync(CancellationToken cancellation = default)
        {
            await _fileLock.WaitAsync(cancellation);
            try
            {
                ISettingsSection[] sections;
                lock (_lock)
                {
                    EnsureState(SettingsState.Registering);
                    _state = SettingsState.Loading;
                    sections = [.. _sections.Values];
                }

                bool fileExists = File.Exists(_settingsFilePath);
                JsonDocument? document = null;
                List<SettingsIssue> allIssues = [];

                try
                {
                    if (fileExists)
                    {
                        string json = await File.ReadAllTextAsync(_settingsFilePath, cancellation);
                        document = JsonDocument.Parse(json);
                        if (document.RootElement.ValueKind != JsonValueKind.Object)
                        {
                            allIssues.Add(new SettingsIssue(Key: null, SettingsIssueSeverity.Error, "The root of settings.json must be a JSON object."));
                            document.Dispose();
                            document = null;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (ex is JsonException or UnauthorizedAccessException)
                {
                    allIssues.Add(new SettingsIssue(null, SettingsIssueSeverity.Error, "Could not read settings.json. Default were used", ex));
                }

                JsonElement? root = document?.RootElement;
                List<SettingsSectionLoadResult> sectionResults = [];
                foreach (ISettingsSection section in sections)
                {
                    JsonElement? serializedValue = null;
                    if (root is JsonElement rootElement && rootElement.TryGetProperty(section.Key, out JsonElement property))
                    {
                        SettingsSectionLoadResult result = section.Load(serializedValue, _jsonOptions);
                        sectionResults.Add(result);
                        allIssues.AddRange(result.Issues);
                    }
                }

                lock (_lock)
                {
                    _unclaimedSections.Clear();
                    if (root is JsonElement rootElement)
                    {
                        foreach (JsonProperty property in rootElement.EnumerateObject())
                        {
                            if (!_sections.ContainsKey(property.Name))
                            {
                                _unclaimedSections[property.Name] = property.Value.Clone();
                            }
                        }
                    }

                    _state = SettingsState.Loaded;
                }

                return new SettingsLoadResult(fileExists, sectionResults, allIssues);
            }
            catch
            {
                lock (_lock)
                {
                    _state = SettingsState.Registering;
                }

                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<SettingsSaveResult> SaveAsync(CancellationToken cancellation = default)
        {
            await _fileLock.WaitAsync(cancellation);
            try
            {
                ISettingsSection[] sections;
                KeyValuePair<string, JsonElement>[] unclaimed;

                lock (_lock)
                {
                    EnsureState(SettingsState.Loaded);
                    sections = [.. _sections.Values];
                    unclaimed = [.. _unclaimedSections];
                }

                string? directory = Path.GetDirectoryName(_settingsFilePath);
                if (string.IsNullOrEmpty(directory))
                {
                    throw new InvalidOperationException("Settings file must have a containing directory.");
                }

                string temporaryPath = _settingsFilePath + "." + Guid.NewGuid() + ".tmp";
                try
                {
                    Directory.CreateDirectory(directory);
                    await using(FileStream stream = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, FileOptions.Asynchronous))
                    {
                        using Utf8JsonWriter writer = new(stream, new()
                        {
                            Indented = true,
                        });

                        writer.WriteStartObject();
                        foreach ((string key, JsonElement value) in unclaimed)
                        {
                            writer.WritePropertyName(key);
                            value.WriteTo(writer);
                        }

                        foreach (ISettingsSection section in sections)
                        {
                            section.Write(writer, _jsonOptions);
                        }

                        writer.WriteEndObject();

                        await writer.FlushAsync(cancellation);
                        await stream.FlushAsync(cancellation);
                    }

                    File.Move(temporaryPath, _settingsFilePath, overwrite: true);
                    return new SettingsSaveResult(_settingsFilePath, Saved: true, Issues: []);
                }
                catch (OperationCanceledException)
                {
                    TryDeleteTemporaryFile(temporaryPath);
                    throw;
                }
                catch (Exception ex)
                {
                    TryDeleteTemporaryFile(temporaryPath);
                    return new SettingsSaveResult(_settingsFilePath, false, Issues: [
                        new SettingsIssue(null, SettingsIssueSeverity.Error, "Could not save settings.", ex)
                    ]);
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public SettingsApplyReport AdvanceTo(SettingsApplyContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            lock (_lock)
            {
                EnsureState(SettingsState.Loaded);
                if (!IsNextPhase(Phase, context.Phase))
                {
                    throw new InvalidOperationException($"Cannot advance settings from {Phase} to {context.Phase}.");
                }

                Phase = context.Phase;
            }

            return Apply(context);
        }

        public SettingsApplyReport Apply(SettingsApplyContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            ISettingsSection[] sections;

            lock (_lock)
            {
                EnsureState(SettingsState.Loaded);
                if (context.Phase != Phase)
                {
                    throw new InvalidOperationException("Settings can only apply using the current phase.");
                }

                sections = [.. _sections.Values];
            }

            Dictionary<string, SettingsApplyResult> results = new(StringComparer.Ordinal);
            foreach (ISettingsSection section in sections)
            {
                try
                {
                    results.Add(section.Key, section.Apply(context));
                }
                catch (Exception ex)
                {
                    results.Add(section.Key, new SettingsApplyResult(SettingsApplyStatus.Failed, "The settings section threw while applying.", ex));
                }
            }

            return new SettingsApplyReport(Phase, results);
        }

        private void EnsureState(SettingsState expected)
        {
            if (_state != expected)
            {
                throw new InvalidOperationException($"Settings manager must be {expected}, but is {_state}.");
            }
        }

        private static bool IsNextPhase(SettingsApplyPhase current, SettingsApplyPhase next)
        {
            return (current, next) switch
            {
                (SettingsApplyPhase.None, SettingsApplyPhase.Engine) => true,
                (SettingsApplyPhase.Engine, SettingsApplyPhase.Host) => true,
                (SettingsApplyPhase.Host, SettingsApplyPhase.Runtime) => true,
                _ => false,
            };
        }

        private static void TryDeleteTemporaryFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // The original exception is more useful than the cleanup failure
            }
        }

        private enum SettingsState
        {
            Registering,
            Loading,
            Loaded,
        }
    }
}
