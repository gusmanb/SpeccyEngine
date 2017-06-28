using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public static class SpeccyBeeper
    {
        public static void PlayBeepSync(UInt16 frequency, int msDuration)
        {
            Console.Beep(frequency, msDuration);
        }

        public static void PlayBeepAsync(UInt16 frequency, int msDuration)
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                Console.Beep(frequency, msDuration);
            });
        }
    }
}
