namespace Engine.MonoGame.Input
{
    public interface IInputBinding
    {
        bool IsPressed(InputState current);
        bool WasPressed(InputState previous);
        bool IsReleased(InputState current);
        bool WasReleased(InputState previous);
    }
}
