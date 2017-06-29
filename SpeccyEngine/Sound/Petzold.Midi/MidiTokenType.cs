// MidiTokenType.cs by Charles Petzold, January 2010

namespace Petzold.Midi
{
    internal enum MidiTokenType
    {
        Null,
        Tempo,
        Octave,
        Channel,
        Instrument,
        Volume,
        Rest,
        Note,
        NoteGroup,
        Duration,
        Length,
        Unknown
    }
}
