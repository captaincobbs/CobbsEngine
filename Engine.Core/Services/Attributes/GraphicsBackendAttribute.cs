using Engine.Core.Platform;

namespace Engine.Core.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GraphicsBackendAttribute : Attribute
    {
        public readonly GraphicsAPI Backend;

        public GraphicsBackendAttribute(GraphicsAPI backend)
        {
            Backend = backend;
        }
    }
}
