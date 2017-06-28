using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyScreenChar
    {
        
        char currentChar;
        SpeccyColor fore;
        SpeccyColor back;
        
        public char CurrentChar { get { return currentChar; } set { currentChar = value; } }
        public SpeccyColor ForeColor { get { return fore; } set { fore = value; } }
        public SpeccyColor BackColor { get { return back; } set { back = value; } }
       
    }
}
