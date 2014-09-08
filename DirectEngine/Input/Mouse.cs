using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace DirectEngine.Input
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    public class Mouse : IDisposable
    {
        private SharpDX.DirectInput.Mouse mouse;
        private MouseState previousState;
        private MouseState currentState;
        private int x;
        private int y;

        public Mouse(DirectInput directInput)
        {
            mouse = new SharpDX.DirectInput.Mouse(directInput);
            mouse.Acquire();
            x = 0;
            y = 0;
            previousState = mouse.GetCurrentState();
            currentState = previousState;
        }

        public void Dispose()
        {
            mouse.Unacquire();
        }

        public void UpdateState()
        {
            previousState = currentState;
            currentState = mouse.GetCurrentState();
            x += currentState.X;
            y += currentState.Y;
        }

        public bool IsPressed(MouseButton button)
        {
            return currentState.Buttons[(int)button];
        }
        public bool WasPressed(MouseButton button)
        {
            return previousState.Buttons[(int)button];
        }
        public bool HasJustBeenPressed(MouseButton button)
        {
            return currentState.Buttons[(int)button] && !previousState.Buttons[(int)button];
        }
        public bool HasJustBeenReleased(MouseButton button)
        {
            return !currentState.Buttons[(int)button] && previousState.Buttons[(int)button];
        }

        public int GetX()
        {
            return x;
        }
        public int GetY()
        {
            return y;
        }
        public int GetOffsetX()
        {
            return currentState.X;
        }
        public int GetOffsetY()
        {
            return currentState.Y;
        }
        public int GetOffsetZ()
        {
            return currentState.Z;
        }
    }
}
