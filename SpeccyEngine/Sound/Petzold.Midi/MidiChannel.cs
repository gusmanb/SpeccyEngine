// MidiChannel.cs by Charles Petzold, January 2010
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Midi;

namespace Petzold.Midi
{
    internal class MidiChannel
    {
        const string noteLookupString = "C D EF G A B";
        MidiStringPlayer player;
        MidiOut midiOut;
        string midiString;
        int channel;
        int index, tokenIndex;
        double millisecondsFromStart;
        DateTime startTime;
        double defaultDuration = 0.25;      // ie, one quarter note
        int tempo = 60;                     // ie, 60 quarter notes per minute
        int volume = 127;
        bool noMoreNotes;
        List<ReleaseInfo> releaseInfoList = new List<ReleaseInfo>();
        List<ReleaseInfo> removeInfoList = new List<ReleaseInfo>();
        char currentOctave = '4';

        public MidiChannel(MidiStringPlayer player, MidiOut midiOut, string midiString, int index, int channel, DateTime startTime)
        {
            this.player = player;
            this.midiOut = midiOut;
            this.midiString = midiString;
            this.channel = 1;
            this.index = index;
            this.startTime = startTime;
            index = 0;
        }

        public bool Tick()
        {
            DateTime tickTime = DateTime.Now;

            removeInfoList.Clear();

            foreach (ReleaseInfo releaseInfo in releaseInfoList)
            {
                if (tickTime >= startTime + TimeSpan.FromMilliseconds(releaseInfo.MillisecondsElapsed))
                {
                    // Release note with StopNote message
                    midiOut.Send(MidiMessage.StopNote(releaseInfo.Note, 0, channel).RawData);
                    removeInfoList.Add(releaseInfo);
                }
            }

            foreach (ReleaseInfo releaseInfo in removeInfoList)
            {
                releaseInfoList.Remove(releaseInfo);
            }

            if (noMoreNotes)
            {
                return releaseInfoList.Count > 0;
            }

            while (tickTime >= startTime + TimeSpan.FromMilliseconds(millisecondsFromStart))
            {
                MidiToken midiToken;
                int note = 0;
                List<int> noteGroup = null;
                double length = -1;
                double duration = -1;

                // Loop to get everything through note or rest
                do
                {
                    midiToken = DecodeNextToken(midiString, ref index, out tokenIndex);

                    if (midiToken.TokenType == MidiTokenType.Null)
                    {
                        noMoreNotes = true;
                        return releaseInfoList.Count > 0;
                    }

                    switch (midiToken.TokenType)
                    {
                        case MidiTokenType.Octave:
                            currentOctave = midiToken.Code.ToString()[0];
                            break;

                        case MidiTokenType.Channel:
                            channel = midiToken.Code;
                            break;

                        case MidiTokenType.Tempo:
                            tempo = midiToken.Code;
                            break;

                        case MidiTokenType.Instrument:
                            midiOut.Send(MidiMessage.ChangePatch(midiToken.Code, channel).RawData);
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

                        case MidiTokenType.NoteGroup:
                            note = -1;
                            noteGroup = midiToken.NoteGroup;
                            break;
                        case MidiTokenType.Duration:
                            defaultDuration = midiToken.Value;
                            break;

                        default:
                            throw new MidiParsingException(tokenIndex, "Expecting Tempo, Instrument, Volume, Note, Note Group, or Rest");
                    }
                } 
                while (midiToken.TokenType != MidiTokenType.NoteGroup && 
                       midiToken.TokenType != MidiTokenType.Note && 
                       midiToken.TokenType != MidiTokenType.Rest);

                do
                {
                    midiToken = DecodeNextToken(midiString, ref index, out tokenIndex);

                    switch (midiToken.TokenType)
                    {
                        case MidiTokenType.Duration:
                            if (duration != -1)
                            {
                                throw new MidiParsingException(tokenIndex, "Duration between notes has already been specified");
                            }

                            duration = midiToken.Value;
                            break;

                        case MidiTokenType.Length:
                            if (length != -1)
                            {
                                throw new MidiParsingException(tokenIndex, "Length between notes has already been specified");
                            }

                            length = midiToken.Value;
                            break;

                        default:
                            index = tokenIndex;
                            break;
                    }
                }
                while (midiToken.TokenType == MidiTokenType.Duration || midiToken.TokenType == MidiTokenType.Length);

                if (duration == -1)
                    duration = defaultDuration;
                else
                    defaultDuration = duration;

                if (length == -1)
                    length = duration;

                // For a plain old note
                if (note != -1)
                {
                    midiOut.Send(MidiMessage.StartNote(note, volume, channel).RawData);
                    releaseInfoList.Add(new ReleaseInfo(note, millisecondsFromStart + 240000.0 * length / tempo, channel));
                }

                // For a note group
                else if (noteGroup != null)
                {
                    foreach (int code in noteGroup)
                    {
                        midiOut.Send(MidiMessage.StartNote(code, volume, channel).RawData);
                        releaseInfoList.Add(new ReleaseInfo(note, millisecondsFromStart + 240000.0 * length / tempo, channel));
                    }

                    noteGroup = null;
                }

                millisecondsFromStart += 240000.0 * duration / tempo;
            }
            return true;
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

            if (token[0] == '(')
            {
                // Strip close parenenthesis off token
                return DecodeNoteGroup(token.Substring(0, token.Length - 1));
            }

            if (token[0] == 'M')
            {
                if (!Int32.TryParse(token.Substring(1, token.Length - 1), out code) || code < 1 || code > 16)
                    throw new MidiParsingException(tokenIndex, "Channels must go from 1 to 16");

                return new MidiToken(MidiTokenType.Channel, code);
            }

            return new MidiToken(MidiTokenType.Unknown);
        }

        MidiToken DecodeNoteGroup(string token)
        {
            MidiToken midiToken = new MidiToken(MidiTokenType.NoteGroup);
            midiToken.NoteGroup = new List<int>();
            int groupIndex = 1;                     // skip open parenthesis
            int groupTokenIndex;
            string tokenInGroup;

            while (null != (tokenInGroup = GetNextToken(token, ref groupIndex, out groupTokenIndex)))
            {
                int code = GetNoteCode(tokenInGroup);

                if (code == -1)
                    throw new MidiParsingException(tokenIndex + groupTokenIndex, "Improperly formed note");

                midiToken.NoteGroup.Add(code);
            }

            return midiToken;
        }

        MidiToken DecodeNoteToken(string token)
        {
            int code = GetNoteCode(token);

            if (code == -1)
                    throw new MidiParsingException(tokenIndex, "Improperly formed note");

            return new MidiToken(MidiTokenType.Note, code);
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
                noteIndex++;
            }

            if (!isLower)
                octaveChar++;

            if (noteIndex != noteLength || !Char.IsDigit(octaveChar))
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
            int denominator = 1;
            int slashIndex = token.IndexOf('/');

            if (slashIndex == -1)
            {
                if (!Int32.TryParse(token.Substring(tokenOffset, tokenLength - tokenOffset), out denominator))
                    throw new MidiParsingException(tokenIndex, "Cannot parse integer");
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
                    if (!Int32.TryParse(token.Substring(slashIndex + 1, tokenLength - slashIndex - 1), out denominator))
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
    }
}
