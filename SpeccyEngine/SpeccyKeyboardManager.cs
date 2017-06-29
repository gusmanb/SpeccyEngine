using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeccyEngine
{
    public static class SpeccyKeyboard
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
        
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        static byte[] keyboardState = new byte[256];
        static bool[] status = new bool[256];
        static bool[] clickStatus = new bool[256];

        static string keys = "";
        static IntPtr inputLocaleIdentifier;

        static SpeccyKeyboard()
        {
            inputLocaleIdentifier = GetKeyboardLayout(0);
        }

        public static void Update()
        {
            keys = "";
            for (int buc = 0; buc < 256; buc++)
            {
                short val = GetAsyncKeyState(buc);
                if ((val & 0x7FFF) != 0)
                {
                    clickStatus[buc] = status[buc] == false;
                    status[buc] = true;
                    keyboardState[buc] = (byte)val;

                    keys += KeyCodeToUnicode(buc);
                }
                else
                {
                    clickStatus[buc] = false;
                    status[buc] = false;
                }
            }
            
        }

        public static string PressedKeys()
        {
            return keys;
        }

        public static bool IsPressed(Keys KeyToCheck)
        {
            int value = (int)KeyToCheck;
            byte which = (byte)(value & 0xFF);
            return status[which];
        }

        static string KeyCodeToUnicode(int key)
        {

            uint virtualKeyCode = (uint)key;
            uint scanCode = MapVirtualKey(virtualKeyCode, 0);
           

            StringBuilder result = new StringBuilder();
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, (int)5, (uint)0, inputLocaleIdentifier);

            return result.ToString();
        }

        public static bool IsClicked(Keys KeyToCheck)
        {
            int value = (int)KeyToCheck;
            byte which = (byte)(value & 0xFF);
            return clickStatus[which];
        }

        public static bool AnyClicked()
        {
            return clickStatus.Any(v => v);
        }

        public static bool AnyPressed()
        {
            return status.Any(v => v);
        }
    }
}
