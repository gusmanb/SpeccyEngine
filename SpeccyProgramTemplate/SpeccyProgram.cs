using SpeccyEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyProgramTemplate
{
    public class SpeccyProgram : SpeccyBasicProgram
    {
        public override void Main()
        {
            //Your code here
            Print("Hello World!!!!");
            Pause(500);
            Circle(SpeccyColor.Red, SpeccyColor.White, 256 / 2, 192 / 2, 60);
            Pause(500);
            Circle(SpeccyColor.Yellow, SpeccyColor.White, 256 / 2, 192 / 2, 45);
            Pause(500);
            Circle(SpeccyColor.Blue, SpeccyColor.White, 256 / 2, 192 / 2, 30);
            Pause(500);
            Circle(SpeccyColor.Green, SpeccyColor.White, 256 / 2, 192 / 2, 15);
            Pause(500);
            Plot(SpeccyColor.Magenta, SpeccyColor.White, 256 / 2, 192 / 2);
            Pause(500);
            PrintAt(0, 23, "Done!");
            Beep(0.2, 0);
            Beep(0.2, -4);
        }
    }
}
