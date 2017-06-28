using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeccyEngine;

namespace CampoMinado
{
    public class BasicTest : SpeccyBasicProgram
    {
        public override void Main()
        {

            byte[] newChar = new byte[] 
            {
                Bin("00000000"),
                Bin("11001100"),
                Bin("11111111"),
                Bin("11001100"),
                Bin("11111111"),
                Bin("11001100"),
                Bin("11011110"),
                Bin("11001000"),
            };

            DefChar('1', newChar);


            for (int buc = 0; buc < 24; buc++)
            {
                Print("11111111111111111111111111111111");
                SwapFont();
            }

            Pause(1000);

            Ink = SpeccyColor.Cyan;

            for (int buc = 0; buc < 100; buc++)
            {
                //CircleClear(80 + buc * 2, 80, 31);
                Circle(Ink, Paper, 80 + buc, 80, 30);
                Circle(Ink, Paper, 80 + buc, 80, 29);
                Circle(Ink, Paper, 80 + buc, 80, 28);
                Circle(Ink, Paper, 80 + buc, 80, 27);
                //CircleClear(80, 80, 26);
                Beep(0.05, buc / 5);
            }
            Beep(0.05, 2);
            Beep(0.05, 4);
            Beep(0.05, 6);
            Beep(0.05, 8);
            Beep(0.05, 10);



            Finished = true;
            //string nombre = null;

            //while (string.IsNullOrWhiteSpace(nombre))
            //    Input("Saludos!", "Dime tu nombre", out nombre);

            //Print($"Hola {nombre}, encantado de conocerte, soy una especie de spectrum del año 2017!!!");

            //PrintAt(SpeccyColor.Red, SpeccyColor.Yellow, 10, 10, "P");
            //Print("P");
            //Print("Hola");
            //Beep(2, 6);
            //Pause(1000);
            //Circle(10, 10, 5);
            //Pause(1000);
            //Circle(20, 20, 5);
            //Pause(1000);
            //Print("Fin");
        }
    }
}
