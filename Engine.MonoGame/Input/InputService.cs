using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Input;

namespace Engine.MonoGame.Input
{
    // Communicates with other services to load / save keybindings for all maps to/from files
    // Handles input events, checking if a specific key is pressed, gamepad/key/mouse states between frames,
    // Interprets bindings in a performant way, allows external subscription of functions to bindings by an enum
    // (Rather than a binding directly mapping to an action)
    // Doesn't handle serialization itself, but it might ask another service to do it for it, or include it with settings
    public class InputService : IInputService
    {
        private InputState _current = new();
        private InputState _previous = new();

        private readonly Dictionary<string, InputAction> _actions = [];

        public void AddAction(InputAction action)
        {
            _actions.Add(action.Name, action);
        }

        public InputAction GetAction(string name)
        {
            return _actions[name];
        }

        public void Update()
        {
            _previous = _current;
            _current = new()
            {
                Keyboard = Keyboard.GetState(),
                Mouse = Mouse.GetState(),
                GamePads = Enumerable.Range(0, 4).ToDictionary(i => i, i => GamePad.GetState(i))
            };

            foreach (InputAction action in _actions.Values)
            {
                action.Update(_current, _previous);
            }
        }
    }
}
