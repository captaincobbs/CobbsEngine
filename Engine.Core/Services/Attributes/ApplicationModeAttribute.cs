using System;
using System.Collections.Generic;
using System.Text;

using Engine.Core.Platform;

namespace Engine.Core.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApplicationModeAttribute : Attribute
    {
        public readonly ApplicationMode Mode;
        public ApplicationModeAttribute(ApplicationMode mode)
        {
            Mode = mode;
        }
    }
}
