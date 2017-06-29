// MidiParsingException.cs by Charles Petzold, January 2010
using System;

namespace Petzold.Midi
{
    internal class MidiParsingException : Exception
    {
        public int Index { set; get; }

        public MidiParsingException(int index, string message) :
            base(message)
        {
            Index = index;
        }

    }
}
