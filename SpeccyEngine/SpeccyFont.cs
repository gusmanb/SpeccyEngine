using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyFont
    {
        SpeccyFontChar[] chars = new SpeccyFontChar[256];

        protected SpeccyFont() { }

        public SpeccyFont(Font BaseFont)
        {
            if (BaseFont != null)
                FillFromSystemFont(BaseFont);
            else
            {
                for (int buc = 0; buc < 256; buc++)
                    chars[buc] = new SpeccyFontChar();
            }
        }

        public void SetChar(char Which, string[] Data)
        {
            chars[(int)Which] = new SpeccyFontChar(Data);
        }

        public void SetChar(char Which, byte[] Data)
        {
            chars[(int)Which] = new SpeccyFontChar(Data);
        }

        public void FillFromSystemFont(Font SystemFont)
        {
            Bitmap bmp = new Bitmap(8, 8);
            Graphics g = Graphics.FromImage(bmp);
            g.PageUnit = GraphicsUnit.Pixel;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            for (int buc = 0; buc < 256; buc++)
            {

                g.Clear(Color.Black);
                g.DrawString(new string((char)buc, 1), SystemFont, Brushes.White, new RectangleF(0, 0, 8, 8), StringFormat.GenericTypographic);
                g.Flush();

                List<string> lines = new List<string>();

                for (int y = 0; y < 8; y++)
                {
                    string line = "";

                    for (int x = 0; x < 8; x++)
                    {
                        var value = bmp.GetPixel(x, y);

                        if (value.ToArgb() != Color.Black.ToArgb())
                            line += "@";
                        else
                            line += " ";
                    }

                    lines.Add(line);
                }

                chars[buc] = new SpeccyFontChar(lines.ToArray());
            }

            g.Dispose();
            bmp.Dispose();

        }

        public SpeccyFontChar this[char C]
        {
            get { return chars[(int)C]; }
            set { chars[(int)C] = value; }
        }
    }
}
