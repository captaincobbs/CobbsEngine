namespace Engine.Core
{
    public static partial class Diagnostics
    {
        public enum MessageType
        {
            Message =   1 << 0,
            Warning =   1 << 1,
            Error =     1 << 2,
            Exception = 1 << 3,
            Debug =     1 << 4,
            Assert =    1 << 5,
        }
    }
}