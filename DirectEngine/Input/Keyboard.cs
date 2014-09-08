using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace DirectEngine.Input
{
    public enum Key 
    {
         D0,
         D1,
         D2,
         D3,
         D4,
         D5,
         D6,
         D7,
         D8,
         D9,
         A,
         AbntC1,
         AbntC2,
         Add,
         Apostrophe,
         Applications,
         AT,
         AX,
         B,
         Back,
         Backslash,
         C,
         Calculator,
         Capital,
         Colon,
         Comma,
         Convert,
         D,
         Decimal,
         Delete,
         Divide,
         Down,
         E,
         End,
         Equals,
         Escape,
         F,
         F1,
         F2,
         F3,
         F4,
         F5,
         F6,
         F7,
         F8,
         F9,
         F10,
         F11,
         F12,
         F13,
         F14,
         F15,
         G,
         Grave,
         H,
         Home,
         I,
         Insert,
         J,
         K,
         Kana,
         Kanji,
         L,
         LeftBracket,
         LeftControl,
         Left,
         LeftMenu,
         LeftShift,
         LeftWindowsKey,
         M,
         Mail,
         MediaSelect,
         MediaStop,
         Minus,
         Multiply,
         Mute,
         MyComputer,
         N,
         Next,
         NextTrack,
         NoConvert,
         NumberLock,
         NumberPad0,
         NumberPad1,
         NumberPad2,
         NumberPad3,
         NumberPad4,
         NumberPad5,
         NumberPad6,
         NumberPad7,
         NumberPad8,
         NumberPad9,
         NumberPadComma,
         NumberPadEnter,
         NumberPadEquals,
         O,
         Oem102,
         P,
         Pause,
         Period,
         PlayPause,
         Power,
         PreviousTrack,
         Prior,
         Q,
         R,
         RightBracket,
         RightControl,
         Return,
         Right,
         RightMenu,
         RightShift,
         RightWindowsKey,
         S,
         Scroll,
         Semicolon,
         Slash,
         Sleep,
         Space,
         Stop,
         Substract,
         PrintScreen,
         T,
         Tab,
         U,
         Underline,
         Unlabeled,
         Up,
         V,
         VolumeDown,
         VolumeUp,
         W,
         Wake,
         WebBack,
         WebFavorites,
         WebForward,
         WebHome,
         WebRefresh,
         WebSearch,
         WebStop,
         X,
         Y,
         Yen,
         Z
    };

    public class Keyboard : IDisposable
    {
        private SharpDX.DirectInput.Keyboard keyboard;
        private KeyboardState previousState;
        private KeyboardState currentState;

        public Keyboard(DirectInput directInput)
        {
            keyboard = new SharpDX.DirectInput.Keyboard(directInput);
            keyboard.Acquire();
            previousState = keyboard.GetCurrentState();
            currentState = previousState;
        }

        public void Dispose()
        {
            keyboard.Unacquire();
        }

        public void UpdateState()
        {
            previousState = currentState;
            currentState = keyboard.GetCurrentState();
            int i =0;
            if (currentState.PressedKeys.Count > 0)
                i++;
        }

        public bool IsPressed(Key key)
        {
            return currentState.IsPressed(ConvertKeyEnum(key));
        }
        public bool WasPressed(Key key)
        {
            return previousState.IsPressed(ConvertKeyEnum(key));
        }
        public bool HasJustBeenPressed(Key key)
        {
            return currentState.IsPressed(ConvertKeyEnum(key)) && !previousState.IsPressed(ConvertKeyEnum(key));
        }
        public bool HasJustBeenReleased(Key key)
        {
            return !currentState.IsPressed(ConvertKeyEnum(key)) && previousState.IsPressed(ConvertKeyEnum(key));
        }

        private static SharpDX.DirectInput.Key ConvertKeyEnum(Key key)
        {
            return (SharpDX.DirectInput.Key)Enum.Parse(typeof(SharpDX.DirectInput.Key), key.ToString());
        }
    }
}
