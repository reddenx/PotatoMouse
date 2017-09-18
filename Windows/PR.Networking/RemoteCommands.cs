using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Networking
{
    internal static class PhoneRemoteCommands //protocol to adhere to
    {
        public const int INVALID_COMMAND = 0;
        public const int MOUSE_POSITION_MOVE = 1;
        public const int MOUSE_POSITION_SET = 2;

        public const int MOUSE_LEFT_CLICK = 10;
        public const int MOUSE_LEFT_DOUBLE_CLICK = 11;
        public const int MOUSE_LEFT_PRESS = 12;
        public const int MOUSE_LEFT_RELEASE = 13;

        public const int MOUSE_RIGHT_CLICK = 20;
        public const int MOUSE_RIGHT_DOUBLE_CLICK = 21;
        public const int MOUSE_RIGHT_PRESS = 22;
        public const int MOUSE_RIGHT_RELEASE = 23;

        public const int MOUSE_SCROLLWHEEL_CLICK = 30;
        public const int MOUSE_SCROLLWHEEL_SCROLL_MOVE = 31;

        public const int KEYBOARD_KEY_CLICK = 40;
        public const int KEYBOARD_KEY_PRESS = 41;
        public const int KEYBOARD_KEY_RELEASE = 42;
        public const int KEYBOARD_KEY_STRING = 43;
    }
}
