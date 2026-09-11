using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core.Services
{
    public enum ServiceFailureBehavior
    {
        /// <summary>Stop the initialization process if the service fails</summary>
        StopInitialization,
        /// <summary>Ignore failure and continue initialization</summary>
        ContinueInitialization,
        /// <summary>Try the next compatible registration of the same contract</summary>
        TryNextService,
        /// <summary>Retry the service initialization</summary>
        Retry,
    }
}
