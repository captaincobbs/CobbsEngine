namespace Engine.Core.Settings
{
    public sealed class SettingsApplyContext
    {
        private readonly object[] _targets;

        public SettingsApplyPhase Phase { get; }
        public IServiceProvider Services { get; }

        public SettingsApplyContext(SettingsApplyPhase phase, IServiceProvider services, params object[] targets)
        {
            Phase = phase;
            Services = services;
            _targets = targets ?? [];
        }

        public bool TryGetTarget<T>(out T? target) where T : class
        {
            target = _targets.OfType<T>().FirstOrDefault();
            return target is not null;
        }
    }
}
