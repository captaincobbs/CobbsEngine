using MessageType = Engine.Core.Diagnostics.MessageType;

namespace Engine.Core
{
    public sealed class EngineOptions
    {
        public DebugOptions Debug { get; } = new();
        public DiagnosticsOptions Diagnostics { get; } = new();
    }

    public sealed class DebugOptions
    {
        public bool EnableConsole { get; internal set; }
    }

    public sealed class DiagnosticsOptions
    {
        private readonly HashSet<MessageType> _mirrorBuffered = [MessageType.Error, MessageType.Warning, MessageType.Exception];
        public IReadOnlySet<MessageType> MirroredTypes => _mirrorBuffered;

        internal void SetMirroredMessageTypes(IEnumerable<MessageType> messageTypes)
        {
            _mirrorBuffered.Clear();

            foreach (MessageType type in messageTypes)
            {
                _mirrorBuffered.Add(type);
            }
        }
    }
}
