using SpeccyAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeccyAudioTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SpeccyBeeper bp = new SpeccyBeeper();
            Thread.Sleep(2000);

            SpeccyAY ay = new SpeccyAY();

            ay.Play("12 a C E a D G  C C# C$");

            bp.Beep(0.2, 30);
            bp.Beep(0.2, 20);
            Thread.Sleep(10);
            bp.Beep(0.1, 26);
            bp.Beep(0.1, 26);
            bp.Beep(0.1, 18);

            for (int bu1c = 0; bu1c < 10; bu1c++)
            {

                for (int buc = 0; buc < 50; buc++)
                {
                    bp.Beep(0.01, buc);
                    //Thread.Sleep(90);
                }
            }
            Console.ReadKey();
        }
    }

}
