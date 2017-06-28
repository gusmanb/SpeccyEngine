using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyFontChar
    {
        byte[] data;

        public SpeccyFontChar()
        {
            data = new byte[8];
        }

        public SpeccyFontChar(string[] Data)
        {
            if (Data.Length != 8 || Data[0].Length != 8)
                throw new InvalidCastException();
            
            data = new byte[8];

            for (int y = 0; y < 8; y++)
            {
                string currentLine = Data[y];
                byte currentByte = 0;

                for (int x = 0; x < 8; x++)
                {
                    if (currentLine[x] == '@')
                        currentByte |= (byte)(128 >> x);

                }

                data[y] = currentByte;

            }
            
        }

        public SpeccyFontChar(byte[] Data)
        {
            data = new byte[8];
            Array.Copy(Data, data, 8);
        }

        public byte[] Data { get { return data; } }
    }
}
