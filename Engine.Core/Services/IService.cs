using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core.Services
{
    public interface IService
    {
        int Priority { get; }
        int Weight { get; }

        void Initialize(IServiceProvider services);

        void Shutdown();
    }
}
