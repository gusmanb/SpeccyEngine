using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyLabel : SpeccySprite
    {
        string text;
        SpeccyLabelAlignment alignment;

        public string Text { get { return text; } set { text = value; CreateFrame(); } }
        public SpeccyLabelAlignment Alignment { get { return alignment; } set { alignment = value; CreateFrame(); } }

        public SpeccyColor ForeColor { get; set; }
        public SpeccyColor BackColor { get; set; }

        private void CreateFrame()
        {
            int toPad = pos.W - text.Length;

            int padLeft = 0;
            int padRight = 0;

            switch (alignment)
            {
                case SpeccyLabelAlignment.Right:
                    padLeft = toPad;
                    break;
                case SpeccyLabelAlignment.Left:
                    padRight = toPad;
                    break;
                case SpeccyLabelAlignment.Center:
                    padLeft = toPad / 2;
                    padRight = toPad - padLeft;
                    break;
            }

            if (toPad < 0)
            {
                padLeft *= -1;
                padRight *= -1;
                string finalString = text.Substring(padRight, text.Length - (padRight + padLeft));
                frames.Clear();
                AddFrame(new SpeccyFrame(pos.W, pos.H, new string[] { finalString }, ForeColor, BackColor));
            }
            else
            {
                string finalString = text.PadLeft(text.Length + padLeft, ' ').PadRight(text.Length + padLeft + padRight, ' ');
                frames.Clear();
                AddFrame(new SpeccyFrame(pos.W, pos.H, new string[] { finalString }, ForeColor, BackColor));
            }
        }

        public SpeccyLabel(int Width, SpeccyColor ForegroundColor, SpeccyColor BackgroundColor, SpeccyFont Font) : base(Width, 1, Font)
        {
            this.ForeColor = ForegroundColor;
            this.BackColor = BackgroundColor;
            this.TranspatentChar = '\0';
            this.Text = "";
        }

        public override void Render(SpeccyScreen Screen)
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
                                Screen.PrintChar(Font[cA.CurrentChar], BackColor, ForeColor, x, y, mode, xShift, yShift, expands);
                            else
                                Screen.PrintChar(Font[cA.CurrentChar], BackColor, ForeColor, x, y, mode);
                        }
                        else
                        {

                            if (xShift != 0 || yShift != 0)
                                Screen.PrintChar(Font[cA.CurrentChar], ForeColor, BackColor, x, y, mode, xShift, yShift, expands);
                            else
                                Screen.PrintChar(Font[cA.CurrentChar], ForeColor, BackColor, x, y, mode);
                        }
                    }
                }
            }
        }
    }

    public enum SpeccyLabelAlignment
    {
        Left,
        Center,
        Right
    }
}
