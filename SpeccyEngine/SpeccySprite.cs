using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccySprite
    {
        protected Rect pos = new Rect();

        public int X { get { return pos.X; } set { pos.X = value; } }
        public int Y { get { return pos.Y; } set { pos.Y = value; } }

        protected int xShift;
        protected int yShift;

        public int XShift { get { return xShift; } set { xShift = value; } }
        public int YShift { get { return yShift; } set { yShift = value; } }

        public int PixelX { get { return pos.X * 8 + xShift; } set { xShift = value % 8; pos.X = value / 8; } }
        public int PixelY { get { return pos.Y * 8 + yShift; } set { yShift = value % 8; pos.Y = value / 8; } }

        public bool BounceAnimation { get; set; }
        public SpeccyFrame Frame { get { return frames[frame]; } }
        public int CurrentFrame { get { return frame; } set { frame = value; } }
        public List<SpeccyFrame> Frames { get { return frames; } }

        protected bool expands;

        public bool ExpandsAttributes { get { return expands; } set { expands = value; } }

        protected List<SpeccyFrame> frames = new List<SpeccyFrame>();
        public SpeccyFont Font { get; set; }
        protected int frame = 0;
        protected int sense = 1;

        public bool Visible { get; set; } = true;
        public char TranspatentChar { get; set; } = ' ';

        public bool Flash { get; set; }
        public int FlashInterval { get; set; } = 10;
        protected int flashCount;

        protected SpeccyMode mode;

        public SpeccyMode RenderMode { get { return mode; } set { mode = value; } }

        public SpeccyScreenChar this[int X, int Y]
        {
            get
            {
                int x = X - pos.X;
                int y = Y - pos.Y;

                if (x < 0 || y < 0 || x >= pos.W || y >= pos.H)
                    return null;

                return Frame[x, y];
            }
        }


        public SpeccySprite(int Width, int Height, SpeccyFont Font)
        {
            pos.W = Width;
            pos.H = Height;
            this.Font = Font;
        }

        public bool AddFrame(SpeccyFrame Frame)
        {
            if (Frame.Width != pos.W || Frame.Height != pos.H)
                return false;

            frames.Add(Frame);

            return true;
        }

        public bool RemoveFrame(SpeccyFrame Frame)
        {
            return frames.Remove(Frame);
        }
        
        public void NextFrame()
        {
            frame += sense;

            if (frame >= frames.Count)
            {
                if (BounceAnimation)
                {
                    frame = Math.Max(0, frames.Count - 2);
                    sense *= -1;
                }
                else
                {
                    frame = 0;
                    sense = 1;
                }

            }
            else if (frame < 0)
            {
                frame = Math.Min(1, frames.Count - 1);
                sense *= -1;
            }
        }

        public void PreviousFrame()
        {
            frame -= sense;

            if (frame >= frames.Count)
            {
                if (BounceAnimation)
                {
                    frame = Math.Max(0, frames.Count - 2);
                    sense *= -1;
                }
                else
                {
                    frame = 0;
                    sense = 1;
                }

            }
            else if (frame < 0)
            {
                frame = Math.Min(1, frames.Count - 1);
                sense *= -1;
            }
        }

        public void MovePixels(int X, int Y)
        {
            xShift += X;
            yShift += Y;

            int xPlaces = xShift / 8;
            int yPlaces = yShift / 8;

            pos.X += xPlaces;
            pos.Y += yPlaces;

            xShift %= 8;
            yShift %= 8;
        }

        protected bool invert = false;

        public virtual void Render(SpeccyScreen Screen)
        {
            if (Flash)
            {
                flashCount++;
                if (flashCount >= FlashInterval)
                {
                    flashCount = 0;
                    invert = !invert;
                }
            }
            else
            {
                invert = false;
                flashCount = 0;
            }

            if (!Visible)
                return;

            if (0 <= pos.X + pos.W &&
                pos.X <= Screen.Width &&
                0 <= pos.Y + pos.H &&
                pos.Y <= Screen.Height)
            {
                int minX = Math.Max(0, pos.X);
                int maxX = Math.Min(Screen.Width, pos.X + pos.W);

                int minY = Math.Max(0, pos.Y);
                int maxY = Math.Min(Screen.Height, pos.Y + pos.H);
                
                for (int x = minX; x < maxX; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        var cA = this[x, y];

                        if (invert)
                        {
                            if (xShift != 0 || yShift != 0)
                                Screen.PrintChar(Font[cA.CurrentChar], cA.BackColor, cA.ForeColor, x, y, mode, xShift, yShift, expands);
                            else
                                Screen.PrintChar(Font[cA.CurrentChar], cA.BackColor, cA.ForeColor, x, y, mode);
                        }
                        else
                        {

                            if (xShift != 0 || yShift != 0)
                                Screen.PrintChar(Font[cA.CurrentChar], cA.ForeColor, cA.BackColor, x, y, mode, xShift, yShift, expands);
                            else
                                Screen.PrintChar(Font[cA.CurrentChar], cA.ForeColor, cA.BackColor, x, y, mode);
                        }
                    }
                }
            }
        }
    }
}
