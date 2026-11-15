using System.Reflection;

namespace Engine.Core
{
    public readonly record struct FilePaths(
        string Executable,
        string UserData,
        string Settings,
        string Diagnostics
    )
    {
        public static FilePaths FromMetadata(AssemblyMetadata metadata)
        {
            string executable = AppDomain.CurrentDomain.BaseDirectory;
            string userData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), metadata.Company, metadata.Name);
            string settings = Path.Combine(userData, "settings.json");
            string diagnostics = Path.Combine(userData, "Diagnostics");

            return new(executable, userData, settings, diagnostics);
        }
    }

    public readonly record struct AssemblyMetadata(string Name, string Company, string Version)
    {
        public static AssemblyMetadata FromAssembly(Assembly assembly)
        {
            return new(
                assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Unknown Assembly",
                assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Unknown Company",
                assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "0.0.0.0"
            );
        }
    }
}
