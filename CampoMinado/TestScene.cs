using SpeccyEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampoMinado
{
    public class TestScene : SpeccyScene
    {
        SpeccyFont fnt;
        SpeccySprite player;
        SpeccyLabel txt;
        Stopwatch sw = new Stopwatch();
        int cnt = 0;
        public TestScene()
        {
            CreateFont();

            SpeccyFrame playerFrame = new SpeccyFrame(1, 1, new string[] { "@" }, SpeccyColor.Red, SpeccyColor.Yellow);
            
            player = new SpeccySprite(1, 1, fnt);
            player.AddFrame(playerFrame);
            player.X = 15;
            player.Y = 23;
            Sprites.Add(player);

            txt = new SpeccyLabel(32, SpeccyColor.BrightGreen, SpeccyColor.Black, fnt);
            txt.Text = "Calculando...";
            Sprites.Add(txt);
            sw.Start();
            FPS = 10000;

            AutoClear = false;
            
        }

        private void CreateFont()
        {
            fnt = new SpeccyFont(new Font("ZX Spectrum", 8, GraphicsUnit.Pixel));
        }
        public override void Dispose()
        {
        }

        int x = 10;
        int y = 10;
        SpeccyColor color = SpeccyColor.BrightWhite;
        bool clean = false;

        int radius = 10;

        public override void Update()
        {
           // Screen.Clear(SpeccyColor.Black, SpeccyColor.Red);
            if (!clean)
            {
                Screen.Clear(SpeccyColor.Red, SpeccyColor.White);
                clean = true;
            }

            cnt++;

            for (int n = 0; n < 256; n++)
            {
                Screen.Plot(n, (int)(80.0 * Math.Sqrt(n / 64.0)));
            }
                //Screen.Circle (x, y, radius, SpeccyColor.BrightMagenta);

            x += radius * 2;

            if (x >= 256)
            {
                x = radius;
                y += radius * 2;
            }

            if (y >= 192)
            {
                radius--;
                x = radius;
                y = radius;
            }

            if (sw.ElapsedMilliseconds > 2000)
            {
                sw.Stop();
                int fps = (int)((double)cnt / (double)(sw.ElapsedMilliseconds / 1000.0));
                txt.Text = "FPS: " + fps;
                cnt = 0;
                sw.Restart();
            }
        }
    }
}
