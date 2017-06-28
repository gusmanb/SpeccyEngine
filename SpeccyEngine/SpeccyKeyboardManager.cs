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

        static bool[] status = new bool[256];
        static bool[] clickStatus = new bool[256];

        public static void Update()
        {
            for (int buc = 0; buc < 256; buc++)
            {
                if ((GetAsyncKeyState(buc) & 0x7FFF) != 0)
                {
                    clickStatus[buc] = status[buc] == false;
                    status[buc] = true;
                }
                else
                {
                    clickStatus[buc] = false;
                    status[buc] = false;
                }
            }
        }


        public static bool IsPressed(Keys KeyToCheck)
        {
            int value = (int)KeyToCheck;
            byte which = (byte)(value & 0xFF);
            return status[which];
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
