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
                typeof(SpeccyBasicFont).Assembly.GetManifestResourceStream("SpeccyEngine.zxsp.ttf");

            }

            Font fnt = new Font("ZX Spectrum", 8, GraphicsUnit.Pixel);
            FillFromSystemFont(fnt);               
            
        }
    }
}
