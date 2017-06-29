// MidiStringPlayerFailedEventArgs.cs by Charles Petzold, January 2010
using System;

namespace Petzold.Midi
{
    public class MidiStringPlayerFailedEventArgs : EventArgs
    {
        public int Index { set; get; }
        public string Error { set; get; }

        public MidiStringPlayerFailedEventArgs(int index, string error)
        {
            Index = index;
            Error = error;
        }
    }
}