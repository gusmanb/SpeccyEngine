using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyFrame
    {
        SpeccyScreenChar[,] chars;
        int width;
        int height;
        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public SpeccyFrame(int Width, int Height, string[] InitialContent = null, SpeccyColor ForeColor = SpeccyColor.Black, SpeccyColor BackColor = SpeccyColor.White)
        {
            width = Width;
            height = Height;

            chars = new SpeccyScreenChar[Width, Height];

            if (InitialContent != null)
            {
                for (int y = 0; y < Height; y++)
                {
                    string line = InitialContent[y];

                    for (int x = 0; x < Width; x++)
                    {
                        chars[x, y] = new SpeccyScreenChar { CurrentChar = line[x], ForeColor = ForeColor, BackColor = BackColor };
                    }
                }
            }
            else
            {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)                 
                        chars[x, y] = new SpeccyScreenChar { CurrentChar = ' ', ForeColor = ForeColor, BackColor = BackColor };
                   
            }
        }

        public SpeccyScreenChar this[int X, int Y] { get { return chars[X, Y]; } }
    }
    
}
