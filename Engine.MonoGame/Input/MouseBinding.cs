using Microsoft.Xna.Framework.Input;

namespace Engine.MonoGame.Input
{
    public class MouseBinding : IInputBinding
    {
        private readonly MouseButton _button;

        public MouseBinding(MouseButton button)
        {
            _button = button;
        }

        public bool IsPressed(InputState current)
        {
            return _button switch
            {
                MouseButton.Left => current.Mouse.LeftButton == ButtonState.Pressed,
                MouseButton.Right => current.Mouse.RightButton == ButtonState.Pressed,
                MouseButton.Middle => current.Mouse.MiddleButton == ButtonState.Pressed,
                MouseButton.XButton1 => current.Mouse.XButton1 == ButtonState.Pressed,
                MouseButton.XButton2 => current.Mouse.XButton2 == ButtonState.Pressed,
                _ => false,
            };
        }

        public bool WasPressed(InputState previous)
        {
            return _button switch
            {
                MouseButton.Left => previous.Mouse.LeftButton == ButtonState.Pressed,
                MouseButton.Right => previous.Mouse.RightButton == ButtonState.Pressed,
                MouseButton.Middle => previous.Mouse.MiddleButton == ButtonState.Pressed,
                MouseButton.XButton1 => previous.Mouse.XButton1 == ButtonState.Pressed,
                MouseButton.XButton2 => previous.Mouse.XButton2 == ButtonState.Pressed,
                _ => false,
            };
        }

        public bool IsReleased(InputState current)
        {
            return _button switch
            {
                MouseButton.Left => current.Mouse.LeftButton == ButtonState.Released,
                MouseButton.Right => current.Mouse.RightButton == ButtonState.Released,
                MouseButton.Middle => current.Mouse.MiddleButton == ButtonState.Released,
                MouseButton.XButton1 => current.Mouse.XButton1 == ButtonState.Released,
                MouseButton.XButton2 => current.Mouse.XButton2 == ButtonState.Released,
                _ => false,
            };
        }

        public bool WasReleased(InputState previous)
        {
            return _button switch
            {
                MouseButton.Left => previous.Mouse.LeftButton == ButtonState.Released,
                MouseButton.Right => previous.Mouse.RightButton == ButtonState.Released,
                MouseButton.Middle => previous.Mouse.MiddleButton == ButtonState.Released,
                MouseButton.XButton1 => previous.Mouse.XButton1 == ButtonState.Released,
                MouseButton.XButton2 => previous.Mouse.XButton2 == ButtonState.Released,
                _ => false,
            };
        }
    }
}
