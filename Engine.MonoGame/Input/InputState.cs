using System.Collections.Generic;

using Microsoft.Xna.Framework.Input;

namespace Engine.MonoGame.Input
{
    public class InputState
    {
        public KeyboardState Keyboard { get; init; }
        public MouseState Mouse { get; init; }
        public Dictionary<int, GamePadState> GamePads { get; init; } = new();
    }
}
