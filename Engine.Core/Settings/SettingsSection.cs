using System.Text.Json;

namespace Engine.Core.Settings
{
    public interface ISettingsSection
    {
        string Key { get; }
        Type SettingsType { get; }
        object Value { get; }

        SettingsSectionLoadResult Load(JsonElement? serializedSection, JsonSerializerOptions options);
        void Write(Utf8JsonWriter writer, JsonSerializerOptions options);
        SettingsApplyResult Apply(SettingsApplyContext context);
    }

    public interface ISettingsSection<T> : ISettingsSection where T : class
    {
        new T Value { get; }
        T CreateDefault();
        SettingsValidationResult<T> Validate(T settings);
        SettingsUpdateResult<T> Update(Func<T, T> update);
        SettingsApplyResult Apply(T settings, SettingsApplyContext context);
    }

    public abstract class SettingsSection<T> : ISettingsSection<T> where T : class
    {
        public abstract string Key { get; }
        public Type SettingsType => typeof(T);
        public T Value { get; private set; } = null!;
        object ISettingsSection.Value => Value;

        public abstract T CreateDefault();
        public abstract SettingsValidationResult<T> Validate(T settings);
        public abstract SettingsApplyResult Apply(T settings, SettingsApplyContext context);

        public SettingsUpdateResult<T> Update(Func<T, T> update)
        {
            ArgumentNullException.ThrowIfNull(update);

            if (Value is null)
            {
                throw new InvalidOperationException($"Settings section '{Key}' has not been loaded.");
            }

            T requested = update(Value) ?? throw new InvalidOperationException($"The update for settings section '{Key}' returned null.");
            SettingsValidationResult<T> validation = Validate(requested);
            T validated = validation.Value ?? throw new InvalidOperationException($"The validator for settings section '{Key}' returned null.");
            bool changed = !EqualityComparer<T>.Default.Equals(Value, validated);
            Value = validated;

            return new SettingsUpdateResult<T>(Value, changed, validation.Issues);
        }

        public SettingsSectionLoadResult Load(JsonElement? serializedValue, JsonSerializerOptions options)
        {
            bool loadedFromFile = false;
            List<SettingsIssue> issues = [];
            T value;

            if (serializedValue is JsonElement element)
            {
                try
                {
                    value = JsonSerializer.Deserialize<T>(element, options) ?? throw new JsonException($"Settings section '{Key}' was null.");
                    loadedFromFile = true;
                }
                catch (Exception ex) when (ex is JsonException or NotSupportedException)
                {
                    issues.Add(new SettingsIssue(Key, SettingsIssueSeverity.Error, "Could not deserialize this settings section. Defaults were used.", ex));
                    value = CreateDefault();
                }
            }
            else
            {
                value = CreateDefault();
            }

            SettingsValidationResult<T> validation = Validate(value);
            Value = validation.Value ?? throw new InvalidOperationException($"Settings validator for '{Key}' returned null.");
            issues.AddRange(validation.Issues);

            return new SettingsSectionLoadResult(Key, loadedFromFile, !loadedFromFile, issues);
        }

        public void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WritePropertyName(Key);
            JsonSerializer.Serialize(writer, Value, options);
        }

        SettingsApplyResult ISettingsSection.Apply(SettingsApplyContext context) => Apply(Value, context);
    }
}
