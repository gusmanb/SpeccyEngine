using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyMIDI : IDisposable
    {

        MidiOut midiOut;
        
        SpeccyMIDIChannel[] instruments;
        SpeccyPercussionChannel percussion;

        public SpeccyMIDIChannel[] Instruments { get { return instruments; } }
        public SpeccyPercussionChannel Percussion { get { return percussion; } }
        
        public SpeccyMIDI()
        {
            midiOut = new MidiOut(0);
            
            instruments = new SpeccyMIDIChannel[15];

            for (int buc = 0; buc < 15; buc++)
                instruments[buc] = new SpeccyMIDIChannel(midiOut, buc > 9 ? buc + 2 : buc + 1);

            percussion = new SpeccyPercussionChannel(midiOut);
        }
        
        public void Play(string Tune, int Channel)
        {
            var channel = instruments[Channel];
            channel.Play(Tune);

            while (channel.IsPlaying)
                Thread.Sleep(1);
        }

        public void PlayAsync(string Tune, int Channel)
        {
            var channel = instruments[Channel];
            channel.Play(Tune);
        }

        public void PlayPercussion(string Tune)
        {
            percussion.Play(Tune);

            while (percussion.IsPlaying)
                Thread.Sleep(1);
        }

        public void PlayPercussionAsync(string Tune)
        {
            percussion.Play(Tune);
        }

        public void Stop(int Channel)
        {
            var channel = instruments[Channel];
            channel.Stop();
        }

        public void ChangeInstrument(SpeccyMIDIInstrument Instrument, int Channel)
        {
            midiOut.Send(MidiMessage.ChangePatch((int)Instrument, Channel).RawData);
        }
        
        public void StartNote(SpeccyMIDINotes Note, int Volume, int Channel)
        {
           midiOut.Send(MidiMessage.StartNote((int)Note, Volume, Channel).RawData);
        }

        public void StopNote(SpeccyMIDINotes Note, int Volume, int Channel)
        {
            midiOut.Send(MidiMessage.StopNote((int)Note, Volume, Channel).RawData);
        }

        public void StartPercussion(SpeccyMIDIPercussion Instrument, int Volume)
        {
            midiOut.Send(MidiMessage.StartNote((int)Instrument, Volume, 10).RawData);
        }

        public void StopPercussion(SpeccyMIDIPercussion Instrument, int Volume)
        {
            midiOut.Send(MidiMessage.StopNote((int)Instrument, Volume, 10).RawData);
        }

        public void Dispose()
        {
            foreach (var channel in Instruments)
                channel.Stop();
            
            percussion.Stop();

            if (midiOut != null)
            {
                midiOut.Reset();
                midiOut.Close();
                midiOut.Dispose();
                midiOut = null;
            }
        }
    }
    
    public class SpeccyMIDIChannel
    {
        protected MidiOut output;
        int channel;
        SpeccyMIDIInstrument currentInstrument = SpeccyMIDIInstrument.AcousticGrandPiano;

        public SpeccyMIDIInstrument Instrument
        {
            get { return currentInstrument; }
            set { currentInstrument = value; output.Send(MidiMessage.ChangePatch((int)Instrument, channel).RawData); }
        }

        int volume;

        public int Volume
        {
            get { return volume; }
            set { volume = value; output.Send(MidiMessage.ChangeControl(7, volume, channel).RawData); }
        }

        SpeccyMIDINote note;

        public SpeccyMIDINote Note
        {
            get { return note; }
            set
            {
                if(note != null)
                    output.Send(MidiMessage.StopNote((int)note.Note, note.Volume, channel).RawData);

                note = value;

                if (note != null)
                    output.Send(MidiMessage.StartNote((int)note.Note, note.Volume, channel).RawData);
            }
        }

        public void SetNote(SpeccyMIDINotes Note, int Volume)
        {
            this.Note = new SpeccyMIDINote { Note = Note, Volume = Volume };
        }

        public SpeccyMIDIChannel(MidiOut Output, int Channel)
        {
            output = Output;
            channel = Channel;
        }

        public bool IsPlaying { get { return player != null; } }

        private SpeccyMIDIChannelPlayer player;
        
        public void Play(string Tune)
        {
            if (player != null)
            {
                player.Dispose();
                player = null;
            }

            player = new SpeccyMIDIChannelPlayer(this, Tune);

            player.Play();
        }

        public void Stop()
        {
            if (player == null)
                return;

            player.Dispose();
            player = null;
        }

        private void playerFinished()
        {
            player.Dispose();
            player = null;
        }

        internal class SpeccyMIDIChannelPlayer : IDisposable
        {
            string tune;
            SpeccyMIDIChannel channel;
            Timer tickTimer;

            int index, tokenIndex;
            double millisecondsFromStart;
            DateTime startTime;
            int tempo = 60;
            int volume = 127;
            double duration = 1 / 4.0;
            bool noMoreNotes;

            ReleaseInfo toRelease = null;
            
            char currentOctave = '4';
            
            const string noteLookupString = "C D EF G A B";

            public SpeccyMIDIChannelPlayer(SpeccyMIDIChannel Channel, string Tune)
            {
                if (string.IsNullOrWhiteSpace(Tune))
                    throw new InvalidProgramException("Invalid tune");

                tune = ExpandParentheses(Tune);
                channel = Channel;
                tickTimer = new Timer((o) => 
                {
                    Tick();

                    if (tickTimer != null)
                        tickTimer.Change(1, Timeout.Infinite);
                });
            }

            private string ExpandParentheses(string midiString)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder repeatSB = new StringBuilder();

                int repeats = 0;

                for (int buc = 0; buc < midiString.Length; buc++)
                {
                    if (midiString[buc] == '(')
                        repeats++;
                    else if (midiString[buc] == ')')
                    {
                        repeats--;
                        repeatSB.Insert(0, ' ');
                        sb.Append(repeatSB.ToString());
                    }
                    else
                    {
                        if (repeats > 0)
                        {
                            sb.Append(midiString[buc]);
                            repeatSB.Append(midiString[buc]);
                        }
                        else
                            sb.Append(midiString[buc]);
                    }
                }

                return sb.ToString();
            }

            public void Play()
            {
                startTime = DateTime.Now;
                tickTimer.Change(1, Timeout.Infinite);
            }

            public void Stop()
            {
                if (tickTimer == null)
                    return;

                tickTimer.Change(Timeout.Infinite, Timeout.Infinite);
                tickTimer.Dispose();

                tickTimer = null;

                channel.Note = null;
                channel.playerFinished();
            }

            void Tick()
            {
                DateTime tickTime = DateTime.Now;
                
                if (toRelease != null && tickTime >= startTime + TimeSpan.FromMilliseconds(toRelease.MillisecondsElapsed))
                {
                    channel.Note = null;
                    toRelease = null;
                }

                if (noMoreNotes && toRelease == null)
                {
                    Stop();
                    return;
                }
                else if (noMoreNotes)
                    return;

                while (tickTime >= startTime + TimeSpan.FromMilliseconds(millisecondsFromStart))
                {
                    MidiToken midiToken = null;
                    int note = 0;


                    while (midiToken == null || 
                        (midiToken.TokenType != MidiTokenType.Note &&
                         midiToken.TokenType != MidiTokenType.Rest))
                    {
                        midiToken = DecodeNextToken(tune, ref index, out tokenIndex);

                        switch (midiToken.TokenType)
                        {
                            case MidiTokenType.Null:
                                noMoreNotes = true;
                                return;
                            case MidiTokenType.Octave:
                                currentOctave = midiToken.Code.ToString()[0];
                                break;
                                
                            case MidiTokenType.Tempo:
                                tempo = midiToken.Code;
                                break;

                            case MidiTokenType.Instrument:
                                channel.Instrument = (SpeccyMIDIInstrument)midiToken.Code;
                                break;

                            case MidiTokenType.Volume:
                                volume = midiToken.Code;
                                break;

                            case MidiTokenType.Rest:
                                note = -1;
                                break;

                            case MidiTokenType.Note:
                                note = midiToken.Code;
                                break;
                            case MidiTokenType.Duration:
                                duration = midiToken.Value;
                                break;

                            default:
                                throw new MidiParsingException(tokenIndex, "Expecting Tempo, Instrument, Volume, Note, Note Group, or Rest");
                        }
                    }

                    if (note != -1)
                    {
                        if (toRelease != null)
                            channel.Note = null;

                        channel.SetNote((SpeccyMIDINotes)note, volume);
                        
                        toRelease = new ReleaseInfo(note, millisecondsFromStart + 240000.0 * duration / tempo);
                    }
                   
                    millisecondsFromStart += 240000.0 * duration / tempo;
                    
                }
            }

            MidiToken DecodeNextToken(string midiString, ref int index, out int tokenIndex)
            {
                string token = GetNextToken(midiString, ref index, out tokenIndex);
                
                if (token == null)
                    return new MidiToken(MidiTokenType.Null);

                int code = 0;

                // Tempo in quarter notes per second
                if (token[0] == 'T')
                {
                    if (!Int32.TryParse(token.Substring(1), out code) || code <= 0)
                        throw new MidiParsingException(tokenIndex, "Tempo must be number of quarter notes per second");

                    return new MidiToken(MidiTokenType.Tempo, code);
                }

                if (token[0] == 'O' && token.Length == 2)
                {
                    if (!Int32.TryParse(token.Substring(1), out code) || code <= 0)
                        throw new MidiParsingException(tokenIndex, "Invaild octave (1 to 9)");

                    return new MidiToken(MidiTokenType.Octave, code);
                }

                // Instrument number
                if (token[0] == 'I')
                {
                    if (!Int32.TryParse(token.Substring(1), out code) || code < 0 || code > 127)
                        throw new MidiParsingException(tokenIndex, "Instrument code must range from 0 through 127");

                    return new MidiToken(MidiTokenType.Instrument, code);
                }

                // Volume
                if (token[0] == 'V')
                {
                    if (!Int32.TryParse(token.Substring(1), out code) || code < 0 || code > 127)
                        throw new MidiParsingException(tokenIndex, "Volume code must range from 0 through 127");

                    return new MidiToken(MidiTokenType.Volume, code);
                }

                // Rest
                if (token[0] == 'R' && token.Length == 1)
                {
                    return new MidiToken(MidiTokenType.Rest);
                }

                // Note
                if (token[0] >= 'A' && token[0] <= 'G' || token[0] >= 'a' && token[0] <= 'g')
                {
                    return DecodeNoteToken(token);
                }

                // Percussion note
                if (token[0] == 'P')
                {
                    if (!Int32.TryParse(token.Substring(1), out code) || code < 0 || code > 127)
                        throw new MidiParsingException(tokenIndex, "Percussion note code must range from 0 through 127");

                    return new MidiToken(MidiTokenType.Note, code);
                }

                // Duration (until next note) or Length (of note)
                if (token[0] == '/' || token[0] == '-' || Char.IsDigit(token[0]))
                {
                    return DecodeDurationOrLengthToken(token);
                }
                
                return new MidiToken(MidiTokenType.Unknown);
            }
            
            MidiToken DecodeNoteToken(string token)
            {
                int code = GetNoteCode(token);

                if (code == -1)
                    throw new MidiParsingException(tokenIndex, "Improperly formed note");

                return new MidiToken(MidiTokenType.Note, code);
            }

            string GetNoteCode(string token, out int code)
            {
                bool isLower = Char.IsLower(token[0]);
                var note = char.ToUpper(token[0]);

                code = noteLookupString.IndexOf(note);
                int noteLength = token.Length;
                int noteIndex = 1;

                while (noteIndex < noteLength && (token[noteIndex] == '#' || token[noteIndex] == '+'))
                {
                    code++;
                    noteIndex++;
                }

                while (noteIndex < noteLength && (token[noteIndex] == '$' || token[noteIndex] == '-'))
                {
                    code--;
                    noteIndex++;
                }

                char octaveChar;

                if (noteIndex == noteLength)        // ie, no octave number so assume octave above middle C
                    octaveChar = currentOctave;
                else
                {
                    octaveChar = token[noteIndex];

                    if (char.IsNumber(octaveChar))
                        noteIndex++;
                    else
                        octaveChar = currentOctave;
                }

                if (!isLower)
                    octaveChar++;

                if (!Char.IsDigit(octaveChar))
                {
                    code = -1;
                    return null;

                }

                code += 12 * (1 + (int)(octaveChar - '0'));

                return token.Substring(noteIndex);
            }

            int GetNoteCode(string token)
            {
                bool isLower = Char.IsLower(token[0]);
                var note = char.ToUpper(token[0]);

                int code = noteLookupString.IndexOf(note);
                int noteLength = token.Length;
                int noteIndex = 1;

                while (noteIndex < noteLength && (token[noteIndex] == '#' || token[noteIndex] == '+'))
                {
                    code++;
                    noteIndex++;
                }

                while (noteIndex < noteLength && (token[noteIndex] == '$' || token[noteIndex] == '-'))
                {
                    code--;
                    noteIndex++;
                }

                char octaveChar;

                if (noteIndex == noteLength)        // ie, no octave number so assume octave above middle C
                    octaveChar = currentOctave;
                else
                {
                    octaveChar = token[noteIndex];

                    if (char.IsNumber(octaveChar))
                        noteIndex++;
                    else
                        octaveChar = currentOctave;
                }

                if (!isLower)
                    octaveChar++;

                if (!Char.IsDigit(octaveChar))
                    return -1;

                code += 12 * (1 + (int)(octaveChar - '0'));

                return code;
            }

            MidiToken DecodeDurationOrLengthToken(string token)
            {
                int tokenLength = token.Length;
                int tokenOffset = 0;
                bool isLength = token[0] == '-';

                if (isLength)
                    tokenOffset++;

                int numerator = 1;
                double denominator = 1;
                int slashIndex = token.IndexOf('/');

                if (slashIndex == -1)
                {
                    if (!isLength)
                    {
                        double trMinator;

                        if (!double.TryParse(token.Substring(tokenOffset, tokenLength - tokenOffset), out trMinator))
                            throw new MidiParsingException(tokenIndex, "Cannot parse integer");

                        switch (trMinator)
                        {
                            case 1:
                                denominator = 16;
                                break;

                            case 2:
                                denominator = 16 / 1.5;
                                break;

                            case 3:
                                denominator = 8;
                                break;
                            case 4:
                                denominator = 8 / 1.5;
                                break;
                            case 5:
                                denominator = 4;
                                break;
                            case 6:
                                denominator = 4 / 1.5;
                                break;
                            case 7:
                                denominator = 2;
                                break;
                            case 8:
                                denominator = 2 / 1.5;
                                break;
                            case 9:
                                denominator = 1;
                                break;
                        }
                    }
                    else
                    {
                        if (!Int32.TryParse(token.Substring(tokenOffset, tokenLength - tokenOffset), out numerator))
                            throw new MidiParsingException(tokenIndex, "Cannot parse integer");
                    }
                }

                else
                {
                    if (slashIndex != tokenOffset)
                    {
                        if (!Int32.TryParse(token.Substring(tokenOffset, slashIndex - tokenOffset), out numerator))
                            throw new MidiParsingException(tokenIndex, "Cannot parse integer");
                    }

                    if (slashIndex + 1 != tokenLength)
                    {
                        if (!double.TryParse(token.Substring(slashIndex + 1, tokenLength - slashIndex - 1), out denominator))
                            throw new MidiParsingException(tokenIndex, "Cannot parse integer");
                    }
                }

                return new MidiToken(isLength ? MidiTokenType.Length : MidiTokenType.Duration, (double)numerator / denominator);
            }

            string GetNextToken(string strInp, ref int index, out int tokenIndex)
            {
                int endOfString = strInp.Length;

                // Skip white space until index of string or vertical bar
                while (index < endOfString && strInp[index] != '|' && Char.IsWhiteSpace(strInp, index))
                    index++;

                // If end of string or vertical bar, no more tokens
                if (index == endOfString || strInp[index] == '|')
                {
                    tokenIndex = endOfString;
                    return null;
                }

                // Points to token within string (value helps in backing up or reporting errors)
                tokenIndex = index;

                // Special processing for note group in parentheses
                if (strInp[index] == '(')
                {
                    while (index < endOfString && strInp[index] != ')')
                        index++;

                    if (index == endOfString || strInp[index] != ')')
                        throw new MidiParsingException(tokenIndex, "Missing close parenthesis");

                    index++;
                }

                // Normal case
                else
                {
                    while (index < endOfString && !Char.IsWhiteSpace(strInp, index))
                        index++;
                }

                return strInp.Substring(tokenIndex, index - tokenIndex);
            }

            public void Dispose()
            {
                Stop();
                channel = null;
            }

            internal enum MidiTokenType
            {
                Null,
                Tempo,
                Octave,
                Instrument,
                Volume,
                Rest,
                Note,
                Duration,
                Length,
                Unknown
            }

            internal class MidiToken
            {
                public MidiToken(MidiTokenType tokenType)
                {
                    TokenType = tokenType;
                }

                public MidiToken(MidiTokenType tokenType, int code)
                {
                    TokenType = tokenType;
                    Code = code;
                }

                public MidiToken(MidiTokenType tokenType, double value)
                {
                    TokenType = tokenType;
                    Value = value;
                }

                public MidiTokenType TokenType { set; get; }
                public int Code { set; get; }
                public double Value { set; get; }
                public List<int> NoteGroup { set; get; }
            }

            internal class MidiParsingException : Exception
            {
                public int Index { set; get; }

                public MidiParsingException(int index, string message) :
                    base(message)
                {
                    Index = index;
                }

            }

            internal class ReleaseInfo
            {
                public ReleaseInfo(int note, double millisecondsElapsed)
                {
                    Note = note;
                    MillisecondsElapsed = millisecondsElapsed;
                }

                public int Note { set; get; }
                public double MillisecondsElapsed { set; get; }
            }
        }
        
    }

    public class SpeccyPercussionChannel : SpeccyMIDIChannel
    {

        private new SpeccyMIDIInstrument Instrument;

        public SpeccyPercussionChannel(MidiOut Output) : base(Output, 10)
        {

        }
    }

    public class SpeccyMIDINote
    {
        public SpeccyMIDINotes Note { get; set; }
        public int Volume { get; set; }
    }

    public enum SpeccyMIDIPercussion
    {
        BassDrum2 = 34,
        BassDrum1,
        SideStickRimshot,
        SnareDrum1,
        HandClap,
        SnareDrum2,
        LowTom2,
        ClosedHiHat,
        LowTom1,
        PedalHiHat,
        MidTom2,
        OpenHiHat,
        MidTom1,
        HighTom2,
        CrashCymbal1,
        HighTom1,
        RideCymbal1,
        ChineseCymbal,
        RideBell,
        Tambourine,
        SplashCymbal,
        Cowbell,
        CrashCymbal2,
        VibraSlap,
        RideCymbal2,
        HighBongo,
        LowBongo,
        MuteHighConga,
        OpenHighConga,
        LowConga,
        HighTimbale,
        LowTimbale,
        HighAgogô,
        LowAgogô,
        Cabasa,
        Maracas,
        ShortWhistle,
        LongWhistle,
        ShortGüiro,
        LongGüiro,
        Claves,
        HighWoodBlock,
        LowWoodBlock,
        MuteCuíca,
        OpenCuíca,
        MuteTriangle,
        OpenTriangle
    }

    public enum SpeccyMIDIInstrument
    {
        AcousticGrandPiano,
        BrightAcousticPiano,
        ElectricGrandPiano,
        HonkyTonkPiano,
        ElectricPiano1,
        ElectricPiano2,
        Harpsichord,
        Clavinet,
        Celesta,
        Glockenspiel,
        MusicBox,
        Vibraphone,
        Marimba,
        Xylophone,
        TubularBells,
        Dulcimer,
        DrawbarOrgan,
        PercussiveOrgan,
        RockOrgan,
        ChurchOrgan,
        ReedOrgan,
        Accordion,
        Harmonica,
        TangoAccordion,
        AcousticGuitarNylon,
        AcousticGuitarSteel,
        ElectricGuitarJazz,
        ElectricGuitarClean,
        ElectricGuitarMuted,
        OverdrivenGuitar,
        DistortionGuitar,
        GuitarHarmonics,
        AcousticBass,
        ElectricBassFinger,
        ElectricBassPick,
        FretlessBass,
        SlapBass1,
        SlapBass2,
        SynthBass1,
        SynthBass2,
        Violin,
        Viola,
        Cello,
        Contrabass,
        TremoloStrings,
        PizzicatoStrings,
        OrchestralHarp,
        Timpani,
        StringEnsemble1,
        StringEnsemble2,
        SynthStrings1,
        SynthStrings2,
        ChoirAahs,
        VoiceOohs,
        SynthChoir,
        OrchestraHit,
        Trumpet,
        Trombone,
        Tuba,
        MutedTrumpet,
        FrenchHorn,
        BrassSection,
        SynthBrass1,
        SynthBrass2,
        SopranoSax,
        AltoSax,
        TenorSax,
        BaritoneSax,
        Oboe,
        EnglishHorn,
        Bassoon,
        Clarinet,
        Piccolo,
        Flute,
        Recorder,
        PanFlute,
        Blownbottle,
        Shakuhachi,
        Whistle,
        Ocarina,
        Lead1Square,
        Lead2Sawtooth,
        Lead3Calliope,
        Lead4Chiff,
        Lead5Charang,
        Lead6Voice,
        Lead7Fifths,
        Lead8BassLead,
        Pad1Newage,
        Pad2Warm,
        Pad3Polysynth,
        Pad4Choir,
        Pad5Bowed,
        Pad6Metallic,
        Pad7Halo,
        Pad8Sweep,
        FX1Rain,
        FX2Soundtrack,
        FX3Crystal,
        FX4Atmosphere,
        FX5Brightness,
        FX6Goblins,
        FX7Echoes,
        FX8SciFi,
        Sitar,
        Banjo,
        Shamisen,
        Koto,
        Kalimba,
        Bagpipe,
        Fiddle,
        Shanai,
        TinkleBell,
        Agogo,
        SteelDrums,
        Woodblock,
        TaikoDrum,
        MelodicTom,
        SynthDrum,
        ReverseCymbal,
        GuitarFretNoise,
        BreathNoise,
        Seashore,
        BirdTweet,
        TelephoneRing,
        Helicopter,
        Applause,
        Gunshot
    }

    public enum SpeccyMIDINotes
    {
        C0,
        Cs0,
        D0,
        Ds0,
        E0,
        F0,
        Fs0,
        G0,
        Gs0,
        A0,
        As0,
        B0,
        C1,
        Cs1,
        D1,
        Ds1,
        E1,
        F1,
        Fs1,
        G1,
        Gs1,
        A1,
        As1,
        B1,
        C2,
        Cs2,
        D2,
        Ds2,
        E2,
        F2,
        Fs2,
        G2,
        Gs2,
        A2,
        As2,
        B2,
        C3,
        Cs3,
        D3,
        Ds3,
        E3,
        F3,
        Fs3,
        G3,
        Gs3,
        A3,
        As3,
        B3,
        C4,
        Cs4,
        D4,
        Ds4,
        E4,
        F4,
        Fs4,
        G4,
        Gs4,
        A4,
        As4,
        B4,
        C5,
        Cs5,
        D5,
        Ds5,
        E5,
        F5,
        Fs5,
        G5,
        Gs5,
        A5,
        As5,
        B5,
        C6,
        Cs6,
        D6,
        Ds6,
        E6,
        F6,
        Fs6,
        G6,
        Gs6,
        A6,
        As6,
        B6,
        C7,
        Cs7,
        D7,
        Ds7,
        E7,
        F7,
        Fs7,
        G7,
        Gs7,
        A7,
        As7,
        B7,
        C8,
        Cs8,
        D8,
        Ds8,
        E8,
        F8,
        Fs8,
        G8,
        Gs8,
        A8,
        As8,
        B8,
        C9,
        Cs9,
        D9,
        Ds9,
        E9,
        F9,
        Fs9,
        G9,
        Gs9,
        A9,
        As9,
        B9,
        C10,
        Cs10,
        D10,
        Ds10,
        E10,
        F10,
        Fs10,
        G10,
    }

}
