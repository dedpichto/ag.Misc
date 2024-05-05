using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ShayX.Hotkeys
{
    internal static class Hotkeys
    {
        //private const int VK_OEM_1 = 0xBA;
        //private const int VK_OEM_PLUS = 0xBB;
        //private const int VK_OEM_COMMA = 0xBC;
        //private const int VK_OEM_MINUS = 0xBD;
        //private const int VK_OEM_PERIOD = 0xBE;
        //private const int VK_OEM_2 = 0xBF;
        //private const int VK_OEM_3 = 0xC0;
        //private const int VK_OEM_4 = 0xDB;
        //private const int VK_OEM_5 = 0xDC;
        //private const int VK_OEM_6 = 0xDD;
        //private const int VK_OEM_7 = 0xDE;
        //private const int VK_OEM_8 = 0xDF;
        //private const int VK_BACK = 0x08;
        //private const int VK_RETURN = 0x0D;
        //private const int VK_SHIFT = 0x10;
        //private const int VK_CONTROL = 0x11;
        //private const int VK_MENU = 0x12;
        //private const int VK_PAUSE = 0x13;
        //private const int VK_ESCAPE = 0x1B;
        //private const int VK_TAB = 0x09;
        //private const int VK_SPACE = 0x20;
        //private const int VK_PRIOR = 0x21;
        //private const int VK_NEXT = 0x22;
        //private const int VK_END = 0x23;
        //private const int VK_HOME = 0x24;
        //private const int VK_LEFT = 0x25;
        //private const int VK_UP = 0x26;
        //private const int VK_RIGHT = 0x27;
        //private const int VK_DOWN = 0x28;
        //private const int VK_INSERT = 0x2D;
        //private const int VK_DELETE = 0x2E;
        //private const int VK_SCROLL = 0x91;
        private const int VK_0 = 0x30;
        private const int VK_1 = 0x31;
        private const int VK_2 = 0x32;
        private const int VK_3 = 0x33;
        private const int VK_4 = 0x34;
        private const int VK_5 = 0x35;
        private const int VK_6 = 0x36;
        private const int VK_7 = 0x37;
        private const int VK_8 = 0x38;
        private const int VK_9 = 0x39;
        private const int VK_A = 0x41;
        private const int VK_B = 0x42;
        private const int VK_C = 0x43;
        private const int VK_D = 0x44;
        private const int VK_E = 0x45;
        private const int VK_F = 0x46;
        private const int VK_G = 0x47;
        private const int VK_H = 0x48;
        private const int VK_I = 0x49;
        private const int VK_J = 0x4A;
        private const int VK_K = 0x4B;
        private const int VK_L = 0x4C;
        private const int VK_M = 0x4D;
        private const int VK_N = 0x4E;
        private const int VK_O = 0x4F;
        private const int VK_P = 0x50;
        private const int VK_Q = 0x51;
        private const int VK_R = 0x52;
        private const int VK_S = 0x53;
        private const int VK_T = 0x54;
        private const int VK_U = 0x55;
        private const int VK_V = 0x56;
        private const int VK_W = 0x57;
        private const int VK_X = 0x58;
        private const int VK_Y = 0x59;
        private const int VK_Z = 0x5A;
        //private const int VK_LWIN = 0x5B;
        //private const int VK_RWIN = 0x5C;
        //private const int VK_NUMPAD0 = 0x60;
        //private const int VK_NUMPAD1 = 0x61;
        //private const int VK_NUMPAD2 = 0x62;
        //private const int VK_NUMPAD3 = 0x63;
        //private const int VK_NUMPAD4 = 0x64;
        //private const int VK_NUMPAD5 = 0x65;
        //private const int VK_NUMPAD6 = 0x66;
        //private const int VK_NUMPAD7 = 0x67;
        //private const int VK_NUMPAD8 = 0x68;
        //private const int VK_NUMPAD9 = 0x69;
        //private const int VK_MULTIPLY = 0x6A;
        //private const int VK_ADD = 0x6B;
        //private const int VK_SUBTRACT = 0x6D;
        //private const int VK_DIVIDE = 0x6F;
        private const int VK_F1 = 0x70;
        private const int VK_F2 = 0x71;
        private const int VK_F3 = 0x72;
        private const int VK_F4 = 0x73;
        private const int VK_F5 = 0x74;
        private const int VK_F6 = 0x75;
        private const int VK_F7 = 0x76;
        private const int VK_F8 = 0x77;
        private const int VK_F9 = 0x78;
        private const int VK_F10 = 0x79;
        private const int VK_F11 = 0x7A;
        private const int VK_F12 = 0x7B;
        private const int VK_F13 = 0x7C;
        private const int VK_F14 = 0x7D;
        private const int VK_F15 = 0x7E;
        private const int VK_F16 = 0x7F;
        private const int VK_F17 = 0x80;
        private const int VK_F18 = 0x81;
        private const int VK_F19 = 0x82;
        private const int VK_F20 = 0x83;
        private const int VK_F21 = 0x84;
        private const int VK_F22 = 0x85;
        private const int VK_F23 = 0x86;
        private const int VK_F24 = 0x87;
        //private const int VK_LSHIFT = 0xA0;
        //private const int VK_RSHIFT = 0xA1;
        //private const int VK_LCONTROL = 0xA2;
        //private const int VK_RCONTROL = 0xA3;
        //private const int VK_LMENU = 0xA4;
        //private const int VK_RMENU = 0xA5;

        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int MOD_WIN = 0x0008;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static byte getModifier(string key)
        {
            return key switch
            {
                "Shift" => MOD_SHIFT,
                "Ctrl" => MOD_CONTROL,
                "Alt" => MOD_ALT,
                "Win" => MOD_WIN,
                _ => 0
            };
        }

        private static ushort getKey(string key)
        {
            return key switch
            {
                "0" => VK_0,
                "1" => VK_1,
                "2" => VK_2,
                "3" => VK_3,
                "4" => VK_4,
                "5" => VK_5,
                "6" => VK_6,
                "7" => VK_7,
                "8" => VK_8,
                "9" => VK_9,
                "A" => VK_A,
                "B" => VK_B,
                "C" => VK_C,
                "D" => VK_D,
                "E" => VK_E,
                "F" => VK_F,
                "G" => VK_G,
                "H" => VK_H,
                "I" => VK_I,
                "J" => VK_J,
                "K" => VK_K,
                "L" => VK_L,
                "M" => VK_M,
                "N" => VK_N,
                "O" => VK_O,
                "P" => VK_P,
                "Q" => VK_Q,
                "R" => VK_R,
                "S" => VK_S,
                "T" => VK_T,
                "U" => VK_U,
                "V" => VK_V,
                "W" => VK_W,
                "X" => VK_X,
                "Y" => VK_Y,
                "Z" => VK_Z,
                "F1" => VK_F1,
                "F2" => VK_F2,
                "F3" => VK_F3,
                "F4" => VK_F4,
                "F5" => VK_F5,
                "F6" => VK_F6,
                "F7" => VK_F7,
                "F8" => VK_F8,
                "F9" => VK_F9,
                "F10" => VK_F10,
                "F11" => VK_F11,
                "F12" => VK_F12,
                "F13" => VK_F13,
                "F14" => VK_F14,
                "F15" => VK_F15,
                "F16" => VK_F16,
                "F17" => VK_F17,
                "F18" => VK_F18,
                "F19" => VK_F19,
                "F20" => VK_F20,
                "F21" => VK_F21,
                "F22" => VK_F22,
                "F23" => VK_F23,
                "F24" => VK_F24,
                _ => 0
            };
        }

        private static ushort loWord(uint l) => (ushort)(l & 0xffff);

        private static ushort hiWord(uint l) => (ushort)(l >> 16);

        private static ushort makeWord(byte low, byte high) => (ushort)((high << 8) | low);

        private static uint makeDWord(ushort low, ushort high) => (uint)(low << 16) | high;

        internal static uint BuildHotKey(string gesture)
        {
            ushort key = 0;
            var arr = gesture.Split('+');
            var bytes = new List<byte>();
            foreach (var k in arr)
            {
                if (k.In("Shift", "Ctrl", "Alt", "Win"))
                {
                    if (bytes.Count < 2)
                    {
                        bytes.Add(getModifier(k));
                    }
                }
                else
                {
                    key = getKey(k);
                }
            }
            var modifiers = bytes.Count switch
            {
                0 => (ushort)0,
                1 => makeWord(bytes[0], 0),
                2 => makeWord(bytes[0], bytes[1]),
                _ => makeWord(bytes[0], bytes[1])
            };
            var hotKey = makeDWord(modifiers, key);
            return hotKey;
        }
    }

    public static class Extentions
    {
        public static bool In<T>(this T obj, params T[] values) => values.Contains(obj);
    }
}
