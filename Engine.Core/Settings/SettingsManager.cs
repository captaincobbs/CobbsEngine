using System.Text.Json;

using System.Text.Json.Nodes;

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
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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

        public void Register(params ISettingsSection[] sections) => Register((IEnumerable<ISettingsSection>)sections);

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

            lock (_lock)
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

                if (!_sections.TryGetValue(key, out ISettingsSection? section))
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
            return await LoadAsync(Array.Empty<SettingsOverride>(), cancellation);
        }

        public async Task<SettingsLoadResult> LoadAsync(IEnumerable<SettingsOverride> overrides, CancellationToken cancellation = default)
        {
            ArgumentNullException.ThrowIfNull(overrides);
            SettingsOverride[] requestedOverrides = [.. overrides];

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
                JsonObject root = [];
                List<SettingsIssue> allIssues = [];

                try
                {
                    if (fileExists)
                    {
                        string json = await File.ReadAllTextAsync(_settingsFilePath, cancellation);
                        JsonNode? parsed = JsonNode.Parse(json);
                        if (parsed is not JsonObject parsedRoot)
                        {
                            allIssues.Add(new SettingsIssue(Key: null, SettingsIssueSeverity.Error, "The root of settings.json must be a JSON object."));
                        }
                        else
                        {
                            root = parsedRoot;
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

                ApplyOverrides(root, requestedOverrides, sections, allIssues);

                using JsonDocument document = JsonDocument.Parse(root.ToJsonString());
                JsonElement rootElement = document.RootElement;
                List<SettingsSectionLoadResult> sectionResults = [];
                foreach (ISettingsSection section in sections)
                {
                    JsonElement? serialized = null;
                    if (rootElement.TryGetProperty(section.Key, out JsonElement property))
                    {
                        serialized = property;
                    }

                    SettingsSectionLoadResult result = section.Load(serialized, _jsonOptions);
                    sectionResults.Add(result);
                    allIssues.AddRange(result.Issues);
                }

                lock (_lock)
                {
                    _unclaimedSections.Clear();
                    foreach (JsonProperty property in rootElement.EnumerateObject())
                    {
                        if (!_sections.ContainsKey(property.Name))
                        {
                            _unclaimedSections[property.Name] = property.Value.Clone();
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

        private static void ApplyOverrides(
            JsonObject root,
            IEnumerable<SettingsOverride> overrides,
            IEnumerable<ISettingsSection> sections,
            ICollection<SettingsIssue> issues)
        {
            HashSet<string> registeredKeys = sections
                .Select(section => section.Key)
                .ToHashSet(StringComparer.Ordinal);

            foreach (SettingsOverride entry in overrides)
            {
                if (string.IsNullOrWhiteSpace(entry.SectionKey))
                {
                    issues.Add(new SettingsIssue(null, SettingsIssueSeverity.Error, "A launch settings override had no section key."));
                    continue;
                }

                if (!registeredKeys.Contains(entry.SectionKey))
                {
                    issues.Add(new SettingsIssue(entry.SectionKey, SettingsIssueSeverity.Error, "The launch settings override targets an unregistered section."));
                    continue;
                }

                if (entry.PropertyPath.Count == 0 || entry.PropertyPath.Any(string.IsNullOrWhiteSpace))
                {
                    issues.Add(new SettingsIssue(entry.SectionKey, SettingsIssueSeverity.Error, "The launch settings override had an invalid property path."));
                    continue;
                }

                if (root[entry.SectionKey] is null)
                {
                    root[entry.SectionKey] = new JsonObject();
                }

                if (root[entry.SectionKey] is not JsonObject current)
                {
                    issues.Add(new SettingsIssue(entry.SectionKey, SettingsIssueSeverity.Error, "The launch settings override cannot be applied because its section is not a JSON object."));
                    continue;
                }

                for (int index = 0; index < entry.PropertyPath.Count - 1; index++)
                {
                    string segment = entry.PropertyPath[index];
                    if (current[segment] is null)
                    {
                        current[segment] = new JsonObject();
                    }

                    if (current[segment] is not JsonObject child)
                    {
                        issues.Add(new SettingsIssue(entry.SectionKey, SettingsIssueSeverity.Error, $"The launch settings override cannot replace '{string.Join('.', entry.PropertyPath)}' because '{segment}' is not a JSON object."));
                        current = null!;
                        break;
                    }

                    current = child;
                }

                if (current is not null)
                {
                    current[entry.PropertyPath[^1]] = entry.Value?.DeepClone();
                }
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
