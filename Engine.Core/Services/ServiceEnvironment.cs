using System;
using System.Collections.Generic;
using System.Text;

using Engine.Core.Configuration;
using Engine.Core.Utilities;

using MonoGame.Framework.Utilities;

namespace Engine.Core.Services
{
    public class ServiceEnvironment
    {
        public Version BackendVersion { get; }
        public RuntimePlatform Platform { get; }
        public ApplicationMode ApplicationMode { get; }
        public GraphicsBackend GraphicsBackend { get; }
    }
}
