using Engine.Core.Utilities;

namespace Engine.Core.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RuntimePlatformAttribute : Attribute
    {
        public readonly RuntimePlatform Platforms;

        public RuntimePlatformAttribute(RuntimePlatform platform)
        {
            Platforms = platform;
        }
    }
}
