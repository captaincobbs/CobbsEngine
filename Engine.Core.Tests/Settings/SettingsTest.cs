using System.Text.Json;
using System.Text.Json.Nodes;

using Engine.Core.Settings;

using FluentAssertions;

namespace Engine.Core.Tests.Settings;

public sealed class SettingsTest : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        "CobbsEngine.Tests",
        Guid.NewGuid().ToString("N"));

    private string SettingsPath => Path.Combine(_directory, "settings.json");

    [Fact]
    public async Task LoadAsync_When_File_Is_Missing_Loads_Validated_Defaults()
    {
        SettingsManager manager = CreateManager();

        SettingsLoadResult result = await manager.LoadAsync();

        result.FileExists.Should().BeFalse();
        result.Sections.Should().ContainSingle(section =>
            section.Key == TestSettingsSection.SectionKey &&
            section.LoadedFromFile == false &&
            section.UsedDefaults);

        manager.Get<TestSettings>(TestSettingsSection.SectionKey)
            .Should()
            .Be(new TestSettings(10));
    }

    [Fact]
    public async Task LoadAsync_When_Section_Is_Present_Deserializes_And_Validates_It()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(
            SettingsPath,
            """
            {
              "test": {
                "value": 250
              }
            }
            """);

        SettingsManager manager = CreateManager();

        SettingsLoadResult result = await manager.LoadAsync();

        result.FileExists.Should().BeTrue();
        result.Sections.Should().ContainSingle(section =>
            section.Key == TestSettingsSection.SectionKey &&
            section.LoadedFromFile &&
            !section.UsedDefaults);

        manager.Get<TestSettings>(TestSettingsSection.SectionKey)
            .Should()
            .Be(new TestSettings(100));
    }

    [Fact]
    public async Task LoadAsync_When_Section_Is_Missing_Uses_Only_That_Sections_Defaults()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(
            SettingsPath,
            """
            {
              "unrelated": {
                "enabled": true
              }
            }
            """);

        SettingsManager manager = CreateManager();

        SettingsLoadResult result = await manager.LoadAsync();

        result.Sections.Should().ContainSingle(section =>
            section.Key == TestSettingsSection.SectionKey &&
            !section.LoadedFromFile &&
            section.UsedDefaults);

        manager.Get<TestSettings>(TestSettingsSection.SectionKey)
            .Should()
            .Be(new TestSettings(10));
    }

    [Fact]
    public async Task LoadAsync_When_Section_Is_Malformed_Uses_Defaults_And_Reports_An_Error()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(
            SettingsPath,
            """
            {
              "test": {
                "value": "not a number"
              }
            }
            """);

        SettingsManager manager = CreateManager();

        SettingsLoadResult result = await manager.LoadAsync();

        result.HasErrors.Should().BeTrue();
        result.Sections.Should().ContainSingle(section =>
            section.Key == TestSettingsSection.SectionKey &&
            !section.LoadedFromFile &&
            section.UsedDefaults &&
            section.HasErrors);

        manager.Get<TestSettings>(TestSettingsSection.SectionKey)
            .Should()
            .Be(new TestSettings(10));
    }

    [Fact]
    public async Task SaveAsync_Preserves_Unclaimed_Sections()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(
            SettingsPath,
            """
            {
              "test": {
                "value": 42
              },
              "disabled-mod": {
                "setting": "keep me"
              }
            }
            """);

        SettingsManager manager = CreateManager();

        await manager.LoadAsync();
        SettingsSaveResult save = await manager.SaveAsync();

        save.Saved.Should().BeTrue();

        using JsonDocument document = JsonDocument.Parse(
            await File.ReadAllTextAsync(SettingsPath));

        document.RootElement
            .GetProperty("disabled-mod")
            .GetProperty("setting")
            .GetString()
            .Should()
            .Be("keep me");

        document.RootElement
            .GetProperty(TestSettingsSection.SectionKey)
            .GetProperty("value")
            .GetInt32()
            .Should()
            .Be(42);
    }

    [Fact]
    public async Task Update_Validates_And_Stores_The_Replacement_Value()
    {
        SettingsManager manager = CreateManager();
        await manager.LoadAsync();

        SettingsUpdateResult<TestSettings> update = manager.Update<TestSettings>(
            TestSettingsSection.SectionKey,
            settings => settings with { Value = -5 });

        update.Changed.Should().BeTrue();
        update.Settings.Should().Be(new TestSettings(0));
        manager.Get<TestSettings>(TestSettingsSection.SectionKey)
            .Should()
            .Be(new TestSettings(0));
    }

    [Fact]
    public async Task LoadAsync_Applies_Overrides_After_The_File_Without_Persisting_Them()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(
            SettingsPath,
            """
            {
              "test": {
                "value": 20
              }
            }
            """);

        SettingsManager manager = CreateManager();

        await manager.LoadAsync([
            new SettingsOverride(TestSettingsSection.SectionKey, ["value"], JsonValue.Create(75))
        ]);

        manager.Get<TestSettings>(TestSettingsSection.SectionKey)
            .Should()
            .Be(new TestSettings(75));

        using JsonDocument document = JsonDocument.Parse(await File.ReadAllTextAsync(SettingsPath));
        document.RootElement.GetProperty("test").GetProperty("value").GetInt32().Should().Be(20);
    }

    [Fact]
    public void Register_When_Key_Is_Duplicated_Throws()
    {
        SettingsManager manager = new(SettingsPath);

        manager.Register<TestSettings>(new TestSettingsSection());

        Action act = () => manager.Register<TestSettings>(new TestSettingsSection());

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private SettingsManager CreateManager()
    {
        SettingsManager manager = new(SettingsPath);
        manager.Register<TestSettings>(new TestSettingsSection());
        return manager;
    }

    private sealed record TestSettings(int Value);

    private sealed class TestSettingsSection : SettingsSection<TestSettings>
    {
        public const string SectionKey = "test";

        public override string Key => SectionKey;

        public override TestSettings CreateDefault() => new(10);

        public override SettingsValidationResult<TestSettings> Validate(
            TestSettings settings)
        {
            return new(
                settings with { Value = Math.Clamp(settings.Value, 0, 100) },
                []);
        }

        public override SettingsApplyResult Apply(
            TestSettings settings,
            SettingsApplyContext context)
        {
            return new(SettingsApplyStatus.Applied);
        }
    }
}
