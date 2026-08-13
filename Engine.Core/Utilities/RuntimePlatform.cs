using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core.Utilities
{
    [Flags]
    public enum RuntimePlatform
    {
        None = 0,
        Windows     = 1 << 0,
        Linux       = 1 << 1,
        MacOS       = 1 << 2,
        Android     = 1 << 3,
        iOS         = 1 << 4,
        WebGL       = 1 << 5,
        XBox        = 1 << 6,
        PlayStation = 1 << 7,
        Switch      = 1 << 8,

        All = Windows | Linux | MacOS | Android | iOS | WebGL | XBox | PlayStation | Switch
    }
}
