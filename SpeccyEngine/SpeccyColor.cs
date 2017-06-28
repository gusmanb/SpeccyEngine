using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public enum SpeccyColor
    {
        Black = 0,
        Blue = unchecked((int)0x000000c0),
        Red = unchecked((int)0x00c00000),
        Magenta = unchecked((int)0x00c000c0),
        Green = unchecked((int)0x0000c000),
        Cyan = unchecked((int)0x0000c0c0),
        Yellow = unchecked((int)0x00c0c000),
        White = unchecked((int)0x00c0c0c0),
        BrightBlack = 0,
        BrightBlue = unchecked((int)0x000000ff),
        BrightRed = unchecked((int)0x00ff0000),
        BrightMagenta = unchecked((int)0x00ff00ff),
        BrightGreen = unchecked((int)0x0000ff00),
        BrightCyan = unchecked((int)0x0000ffff),
        BrightYellow = unchecked((int)0x00ffff00),
        BrightWhite = unchecked((int)0x00ffffff)
    }
}
