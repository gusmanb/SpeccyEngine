using SpeccyEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = SpeccyEngine.SpeccyColor;

namespace CampoMinado
{
    public class IntroScene : SpeccyBasicProgram
    {
        public override void Main()
        {
            
            int[] order = new int[] { 2, 3, 4, 5, 6, 5, 4, 3, 2, 1, 2, 3, 4, 5, 6, 7, 7, 6, 5, 4, 3, 2, 1, 2, 3, 4, 5, 6, 5, 4, 3, 2 };

            DefChar('-', new byte[] {
                Bin("........"),
                Bin("........"),
                Bin("........"),
                Bin("........"),
                Bin("..@@@@.."),
                Bin(".@@@@@@."),
                Bin("........"),
                Bin("........"),
            });

            DefChar('?', new byte[] {
                Bin("........"),
                Bin("@@......"),
                Bin("@@......"),
                Bin(".@......"),
                Bin(".@..@@@."),
                Bin(".@..@.@@"),
                Bin(".@.@...."),
                Bin(".@@@...."),
            });

            DefChar('&', new byte[] {
                Bin("...@@..."),
                Bin("...@@..."),
                Bin("..@..@.."),
                Bin("@@....@@"),
                Bin("@@....@@"),
                Bin("..@..@.."),
                Bin("...@@..."),
                Bin("...@@..."),
            });

            Ink = Color.White;
            Paper = Color.Blue;

            Cls();
            
            PrintAt(5, 0, "EL DR. GUSMAN PRESENTA");

            //PlayPercussionAsync($"T280 V127 O3 (3 C C 5 b$ C g e$ g c R) 5 C D E$ D E$ C D C D b$ C b$ C g 7 C");
            //PlayAsync(0, $"T280 O4 I{((int)SpeccyMIDIInstrument.PizzicatoStrings).ToString()} (5 C b$ C g e$ g c R) 5 C D E$ D E$ C D C D b$ C b$ C g 7 C");
            //Pause(60);
            //Play(1, $"T280 O4 I{((int)SpeccyMIDIInstrument.ChoirAahs).ToString()} (5 C b$ C g e$ g c R) 5 C D E$ D E$ C D C D b$ C b$ C g 7 C");

            //PlayPercussionAsync($"T280 V127 O3 (3 C C 5 b$ C g e$ g c R) 5 C D E$ D E$ C D C D b$ C b$ C D 7 E$");
            //PlayAsync(0, $"T280 O5 I{((int)SpeccyMIDIInstrument.PizzicatoStrings).ToString()} (5 C b$ C g e$ g c R) C D E$ D E$ C D C D b$ C b$ C D 7 E$");
            //Pause(60);
            //Play(1, $"T280 O5 I{((int)SpeccyMIDIInstrument.ChoirAahs).ToString()} (5 C b$ C g e$ g c R) C D E$ D E$ C D C D b$ C b$ C D 7 E$");
            
            for (int y = 2; y < 19; y++)
            {
                LockScreen();
                for (int x = 0; x < 32; x++)
                {
                    PrintAt(Ink, SpeccyColorHelper.AllColors[order[x]], x, y, " ");
                }
                UnlockScreen();
                Pause(100);

            }

            Beep(0.2, 30);
            Beep(0.2, 20);
            Pause(10);
            Beep(0.1, 26);
            Beep(0.1, 26);                                                                                           
            Beep(0.1, 18);

            PrintAt(0, 0, "PRUEBA TU ESTRATEGIA Y HABILIDAD");

            PrintAt(Color.White, Color.Black, 9, 10, "CAMPO DE MINAS");
            Pause(100);
            Circle(Color.White, 16 * 8, 10 * 8 + 3, 44);
            Pause(500);
            PrintAt(Color.White, Color.Red, 9, 10, "CAMPO DE MINAS");
            Pause(100);
            Circle(Color.White, 16 * 8, 10 * 8 + 3, 48);
            Pause(500);
            PrintAt(Color.Black, Color.Green, 9, 10, "CAMPO DE MINAS");
            Pause(100);
            Circle(Color.White, 16 * 8, 10 * 8 + 3, 52);
            Pause(500);
            PrintAt(Color.Black, Color.Yellow, 9, 10, "CAMPO DE MINAS");
            Pause(100);
            Circle(Color.White, 16 * 8, 10 * 8 + 3, 56);
            Pause(500);
            PrintAt(Color.White, Color.Black, 9, 10, "CAMPO DE MINAS");

            bool invert = false;

            PrintAt(0, 20, "\"RESCATA A BILL EL GUSANO DE UNA HORRIBLE JUBILACION\"");

            for (int y = 2; y < 19; y++)
            {
                int inverseY = 18 - (y - 2);

                PrintAt(Color.Black, 4, y, "-");
                PrintAt(Color.Black, 3, inverseY, "-");
                PrintAt(Color.Black, 5, inverseY, "-");

                PrintAt(Color.Black, 27, y, "-");
                PrintAt(Color.Black, 26, inverseY, "-");
                PrintAt(Color.Black, 28, inverseY, "-");

                if (invert)
                    PrintAt(Color.White, Color.Black, 9, 10, "CAMPO DE MINAS");
                else
                    PrintAt(Color.Black, Color.White, 9, 10, "CAMPO DE MINAS");

                if (y % 2 == 0)
                    invert = !invert;

                Beep(0.01, 40);
                Pause(90);
            }

            PrintAt(Color.Blue, Color.White, 32 - 15, 23, "PULSA UNA TECLA");

            long val = 0;

            while (!SpeccyKeyboard.IsPressed(System.Windows.Forms.Keys.Enter))
            {
                val++;

                if (invert)
                    PrintAt(Color.White, Color.Black, 9, 10, "CAMPO DE MINAS");
                else
                    PrintAt(Color.Black, Color.White, 9, 10, "CAMPO DE MINAS");

                if (val % 30 == 0)
                    invert = !invert;

                Pause(5);
            }

            Ink = Color.Black;
            Paper = Color.White;

            Cls();

            PrintAt(0, 1, "©");
            PrintAt(Color.White, Color.Blue, 2, 1, "CAMPO DE MINAS!");
            PrintAt(17, 1, " Por Ian Andrew");

            PrintAt(Color.White, Color.Red, 0, 3, "   Recreado por el Dr. Gusman   ");
            PrintAt(Color.Black, Color.Cyan, 0, 5, "TU MISION:");
            PrintAt(10, 5, @" (deberas decidir
si la aceptas) LLEGAR HASTA
BILL EL GUSANO Y RESCATARLE

EL ESTA DURMIENDO EN EL ULTIMO
CAMPO DE MINAS (NIVEL 9)

ESTE ES BILL ""?""


");
            PrintAt(Color.Black, Color.Cyan, 0, 15, @"TU");
            PrintAt(3, 15, @", ""&"", COMIENZAS EN LA PARTE INFERIOR DE LA PANTALLA.");

            PrintAt(Color.Black, Color.Cyan, 0, 18, @"TU OBJETIVO:");
            PrintAt(13, 18, @"ALCANZAR LA PUERTA DE LA PARTE SUPERIOR.");

            Pause(1000);

            PrintAt(Color.White, Color.Black, 32 - 15, 23, "PULSA UNA TECLA");

            while (!SpeccyKeyboard.IsPressed(System.Windows.Forms.Keys.Enter))
            {
                Pause(5);
            }

            FinishData = new EndLevelInfo();
        }
    }
}
