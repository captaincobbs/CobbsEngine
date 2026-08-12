using Engine.Core.Utilities;

using MonoGame.Framework.Utilities;

namespace Engine.Core.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GraphicsBackendAttribute : Attribute
    {
        public readonly GraphicsBackend Backend;

        public GraphicsBackendAttribute(GraphicsBackend backend)
        {
            Backend = backend;
        }
    }
}
