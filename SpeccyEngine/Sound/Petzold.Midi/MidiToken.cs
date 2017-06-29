// MidiToken.cs by Charles Petzold, January 2010
using System;
using System.Collections.Generic;

namespace Petzold.Midi
{
    internal struct MidiToken
    {
        public MidiToken(MidiTokenType tokenType)
            : this()
        {
            TokenType = tokenType;
        }

        public MidiToken(MidiTokenType tokenType, int code)
            : this()
        {
            TokenType = tokenType;
            Code = code;
        }

        public MidiToken(MidiTokenType tokenType, double value)
            : this()
        {
            TokenType = tokenType;
            Value = value;
        }

        public MidiTokenType TokenType { set; get; }
        public int Code { set; get; }
        public double Value { set; get; }
        public List<int> NoteGroup { set; get; }
    }
}
