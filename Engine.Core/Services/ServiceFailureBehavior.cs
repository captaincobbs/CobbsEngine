using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core.Services
{
    public enum ServiceFailureBehavior
    {
        StopInitialization,
        ContinueInitialization,
        Retry,
        TryNextService,
    }
}
