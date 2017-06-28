using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public abstract class SpeccyScene : IDisposable
    {
        List<SpeccySprite> sprites = new List<SpeccySprite>();

        public List<SpeccySprite> Sprites => sprites;

        SpeccyColor fg = SpeccyColor.Black;
        SpeccyColor bg = SpeccyColor.White;
        int delay = 33;

        public int FPS { get { return 1000 / delay; } set { delay = 1000 / value; } }

        public SpeccyColor ForeColor { get { return fg; } set { fg = value; } }
        public SpeccyColor BackColor { get { return bg; } set { bg = value; } }

        bool clear = true;

        public bool AutoClear { get { return clear; } set { clear = value; } }

        public SpeccyScreen Screen { get; set; }
        public Speccy Engine { get; set; }
        Stopwatch sw = new Stopwatch();

        internal void Frame(int RenderMultiplier, SpeccyScreen Screen, Graphics G)
        {
            sw.Restart();

            SpeccyKeyboard.Update();

            if(clear)
                Screen.Clear(fg, bg);

            Update();

            foreach (var spr in sprites)
                spr.Render(Screen);

            if(RenderMultiplier == 1)
                Screen.Render(G);
            else
                Screen.Render(Screen.Width * 8 * RenderMultiplier, Screen.Height * 8 * RenderMultiplier, G);

            sw.Stop();

            if (sw.ElapsedMilliseconds < delay)
                Thread.Sleep(delay - (int)sw.ElapsedMilliseconds);
        }

        public abstract void Update();

        public bool Finished { get; protected set; }
        public object FinishData { get; protected set; }

        public abstract void Dispose();

    }
}
