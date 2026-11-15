using System.Text.Json.Nodes;

using Engine.Core.Settings;

using FluentAssertions;

namespace Engine.Core.Tests;

public sealed class LaunchArgumentHandlerTests
{
    [Fact]
    public void Parse_Known_Flags_Create_The_Expected_Bootstrap_And_Settings_Overrides()
    {
        LaunchArgumentHandler.LaunchArgumentParseResult result = LaunchArgumentHandler.Parse([
            "--headless",
            "--console",
            "--no-mods",
            "--log-mirror=error,warning"
        ]);

        result.HasErrors.Should().BeFalse();
        result.Bootstrap.Headless.Should().BeTrue();
        result.SettingsOverrides.Should().Contain(overrideSetting =>
            overrideSetting.SectionKey == "engine" &&
            overrideSetting.PropertyPath.SequenceEqual(new[] { "debug", "enableConsole" }) &&
            overrideSetting.Value!.GetValue<bool>());
        result.SettingsOverrides.Should().Contain(overrideSetting =>
            overrideSetting.SectionKey == "engine" &&
            overrideSetting.PropertyPath.SequenceEqual(new[] { "modules", "enabled" }) &&
            !overrideSetting.Value!.GetValue<bool>());
        result.SettingsOverrides.Should().Contain(overrideSetting =>
            overrideSetting.SectionKey == "engine" &&
            overrideSetting.PropertyPath.SequenceEqual(new[] { "diagnostics", "mirroredTypes" }));
    }

    [Fact]
    public void Parse_Set_Separates_The_Section_Path_And_Json_Value()
    {
        LaunchArgumentHandler.LaunchArgumentParseResult result = LaunchArgumentHandler.Parse([
            "--set",
            "engine.debug.enableConsole=true"
        ]);

        result.HasErrors.Should().BeFalse();
        result.SettingsOverrides.Should().ContainSingle();

        SettingsOverride setting = result.SettingsOverrides[0];
        setting.SectionKey.Should().Be("engine");
        setting.PropertyPath.Should().Equal("debug", "enableConsole");
        setting.Value!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void Parse_Set_Reports_A_Missing_Value()
    {
        LaunchArgumentHandler.LaunchArgumentParseResult result = LaunchArgumentHandler.Parse(["--set"]);

        result.HasErrors.Should().BeTrue();
        result.Issues.Should().ContainSingle(issue => issue.IsError);
    }
}
