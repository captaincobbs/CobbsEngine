using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DependsOnAttribute : Attribute
    {
        public readonly Type[] Dependencies;

        public DependsOnAttribute(Type type)
        {
            Dependencies = [type];
        }

        public DependsOnAttribute(params Type[] types)
        {
            Dependencies = types;
        }
    }
}
