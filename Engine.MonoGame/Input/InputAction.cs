using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.MonoGame.Input
{
    public class InputAction
    {
        public string Name { get; }

        private readonly List<IInputBinding> _bindings = new();

        public event Action Pressed;
        public event Action Released;
        public event Action Held;

        public InputAction(string name)
        {
            Name = name;
        }

        public void AddBinding(IInputBinding binding)
        {
            _bindings.Add(binding);
        }

        public void Update(InputState current, InputState previous)
        {
            bool isPressed = _bindings.Any(binding => binding.IsPressed(current));
            bool wasPressed = _bindings.Any(binding => binding.IsPressed(previous));

            if (isPressed && !wasPressed)
            {
                Pressed?.Invoke();
            }

            if (!isPressed && wasPressed)
            {
                Released?.Invoke();
            }
            
            if (isPressed && wasPressed)
            {
                Held?.Invoke();
            }
        }
    }
}
