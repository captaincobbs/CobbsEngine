using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HeadlessOnlyAttribute : Attribute
    {
        public HeadlessOnlyAttribute() { }
    }
}
