using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace DirectEngine.Input
{
    public static class InputManager
    {
        public static Keyboard Keyboard { get; private set; }
        public static Mouse Mouse { get; private set; }

        private static DirectInput directInput;

        public static void Initialize()
        {
            directInput = new DirectInput();
            Keyboard = new Keyboard(directInput);
            Mouse = new Mouse(directInput);
        }

        public static void Unitialize()
        {
            Keyboard.Dispose();
            Mouse.Dispose();
            directInput.Dispose();
        }

        public static void UpdateState()
        {
            Keyboard.UpdateState();
            Mouse.UpdateState();
        }
    }
}
