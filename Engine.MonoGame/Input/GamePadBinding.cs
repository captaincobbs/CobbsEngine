using System.Linq;

using Microsoft.Xna.Framework.Input;

namespace Engine.MonoGame.Input
{
    public class GamePadBinding : IInputBinding
    {
        private readonly int _playerIndex;
        private readonly Buttons[] _buttons;

        public GamePadBinding(int playerIndex, params Buttons[] buttons)
        {
            _playerIndex = playerIndex;
            _buttons = buttons;
        }

        public bool IsPressed(InputState current)
        {
            if (!current.GamePads.TryGetValue(_playerIndex, out GamePadState state))
            {
                return false;
            }

            return _buttons.All(button => state.IsButtonDown(button));
        }

        public bool WasPressed(InputState previous)
        {
            if (!previous.GamePads.TryGetValue(_playerIndex, out GamePadState state))
            {
                return false;
            }

            return _buttons.All(button => state.IsButtonDown(button));
        }

        public bool IsReleased(InputState current)
        {
            if (!current.GamePads.TryGetValue(_playerIndex, out GamePadState state))
            {
                return false;
            }

            return _buttons.All(button => state.IsButtonUp(button));
        }

        public bool WasReleased(InputState previous)
        {
            if (!previous.GamePads.TryGetValue(_playerIndex, out GamePadState state))
            {
                return false;
            }

            return _buttons.All(button => state.IsButtonUp(button));
        }
    }
}
