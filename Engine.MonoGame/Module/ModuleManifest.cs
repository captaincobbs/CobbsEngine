using System;
using System.Collections.Generic;

namespace Engine.MonoGame.Module
{
    public record ModuleManifest
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }
        public Version[] GameVersions { get; set; }
        public ModuleDependency[] Dependencies { get; set; }
        public Dictionary<string, string> Content { get; set; } // Format: { "items" : "data.items.json", "sprites" : "assets/sprites", ... }
    }

    public record ModuleDependency
    {
        public string ID { get; set; }
        public string[] Versions { get; set; } // Format: [">=1.0.0", "0.9.0-0.9.9", ... ]
    }
}
