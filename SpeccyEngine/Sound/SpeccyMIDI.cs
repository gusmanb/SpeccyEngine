using NAudio.Midi;
using Petzold.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyMIDI : IDisposable
    {

        MidiOut midiOut;

        MidiStringPlayer mps;

        SpeccyInstrumentChannel[] instruments;
        SpeccyPercussionChannel percussion;

        public SpeccyInstrumentChannel[] Instruments { get { return instruments; } }
        public SpeccyPercussionChannel Percussion { get { return percussion; } }

        public bool IsPlaying { get { return mps.IsPlaying; } }

        public SpeccyMIDI()
        {
            midiOut = new MidiOut(0);

            mps = new MidiStringPlayer(midiOut);

            instruments = new SpeccyInstrumentChannel[15];

            for (int buc = 0; buc < 15; buc++)
                instruments[buc] = new SpeccyInstrumentChannel(midiOut, buc > 9 ? buc + 2 : buc + 1);

            percussion = new SpeccyPercussionChannel(midiOut);
        }
        
        public void Play(string PlayString)
        {
            mps.MidiString = PlayString;
            mps.Play();
            while (mps.IsPlaying)
                Thread.Sleep(1);
        }

        public void PlayAsync(string PlayString)
        {
            mps.MidiString = PlayString;
            mps.Play();
        }
        
        public void ChangeInstrument(SpeccyMIDIInstrument Instrument, int Channel)
        {
            midiOut.Send(MidiMessage.ChangePatch((int)Instrument, Channel).RawData);
        }

        public void ChangePercussion(SpeccyMIDIPercussion Instrument)
        {
            midiOut.Send(MidiMessage.ChangePatch((int)Instrument, 10).RawData);
        }

        public void StartNote(SpeccyMIDINotes Note, int Volume, int Channel)
        {
           midiOut.Send(MidiMessage.StartNote((int)Note, Volume, Channel).RawData);
        }

        public void StopNote(SpeccyMIDINotes Note, int Volume, int Channel)
        {
            midiOut.Send(MidiMessage.StopNote((int)Note, Volume, Channel).RawData);
        }

        public void StartPercussion(SpeccyMIDINotes Pitch, int Volume)
        {
            midiOut.Send(MidiMessage.StartNote((int)Pitch, Volume, 10).RawData);
        }

        public void StopPercussion(SpeccyMIDINotes Note, int Volume)
        {
            midiOut.Send(MidiMessage.StopNote((int)Note, Volume, 10).RawData);
        }

        public void Dispose()
        {
            if (mps != null)
            {
                mps.Stop();
                mps = null;
            }

            if (midiOut != null)
            {
                midiOut.Reset();
                midiOut.Close();
                midiOut.Dispose();
                midiOut = null;
            }
        }
    }

    public class SpeccyInstrumentChannel
    {
        MidiOut output;
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

        public SpeccyInstrumentChannel(MidiOut Output, int Channel)
        {
            output = Output;
            channel = Channel;
        }
    }

    public class SpeccyPercussionChannel
    {
        MidiOut output;
        int channel;
        SpeccyMIDIPercussion currentInstrument = SpeccyMIDIPercussion.BassDrum2;

        public SpeccyMIDIPercussion Instrument
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
                if (note != null)
                    output.Send(MidiMessage.StopNote((int)note.Note, note.Volume, channel).RawData);
            }
        }

        public SpeccyPercussionChannel(MidiOut Output)
        {
            output = Output;
            channel = 10;
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
