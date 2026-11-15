using System.Linq;

using Microsoft.Xna.Framework.Input;

namespace Engine.MonoGame.Input
{
    public class KeyboardBinding : IInputBinding
    {
        private readonly Keys[] _keys;

        public KeyboardBinding(params Keys[] keys)
        {
            _keys = keys;
        }

        public bool IsPressed(InputState current) => _keys.All(current.Keyboard.IsKeyDown);
        public bool WasPressed(InputState previous) => _keys.All(previous.Keyboard.IsKeyDown);
        public bool IsReleased(InputState current) => _keys.All(current.Keyboard.IsKeyUp);
        public bool WasReleased(InputState previous) => _keys.All(previous.Keyboard.IsKeyUp);
    }
}
