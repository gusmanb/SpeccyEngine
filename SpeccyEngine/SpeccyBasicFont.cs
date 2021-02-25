using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyBasicFont : SpeccyFont
    {
        static PrivateFontCollection pfc;

        public SpeccyBasicFont()
        {
            if (pfc == null)
            {
                pfc = new PrivateFontCollection();
                var fontStream = typeof(SpeccyBasicFont).Assembly.GetManifestResourceStream("SpeccyEngine.zxsp.ttf");

                byte[] fontdata = new byte[fontStream.Length];
                fontStream.Read(fontdata, 0, (int)fontStream.Length);
                fontStream.Close();

                unsafe
                {
                    fixed (byte* pFontData = fontdata)
                    {
                        pfc.AddMemoryFont((System.IntPtr)pFontData, fontdata.Length);
                    }
                }
            }

            Font fnt = new Font(pfc.Families.First(), 8, GraphicsUnit.Pixel);
            FillFromSystemFont(fnt);               
            
        }
    }
}
