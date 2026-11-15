using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
