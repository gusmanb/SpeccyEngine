// MidiStringPlayer.cs by Charles Petzold, January 2010
using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading;
using NAudio.Midi;

namespace Petzold.Midi
{
    public class MidiStringPlayer
    {
        public MidiOut midiOut;
        Timer tmr;
        string midiString;
        List<MidiChannel> midiChannels = new List<MidiChannel>();
        List<MidiChannel> removeChannelList = new List<MidiChannel>();
        DateTime shutDownTime;
        
        public event EventHandler Ended;
        public event MidiStringPlayerFailedEventHandler Failed;
        
        public MidiStringPlayer(MidiOut Output)
        {
            midiOut = Output;
        }

        public string MidiString { get; set; }

        public bool IsPlaying { get; set; }

        public bool Play()
        {
            if (IsPlaying)
                return false;

            if (String.IsNullOrEmpty(MidiString))
                return false;

            // Set private fields to default values
            midiString = MidiString;
            midiChannels.Clear();
            shutDownTime = DateTime.MaxValue;

            int startIndex = 0;
            int channel = 0;
            bool hasPercussionTrack = false;
            
            DateTime startTime = DateTime.Now;

            do
            {
                if (HasContent(midiString, startIndex))
                {
                    // Check for percussion track
                    int endIndex = midiString.IndexOf('|', startIndex);

                    if (endIndex == -1)
                        endIndex = midiString.Length;

                    if (midiString.IndexOf('P', startIndex, endIndex - startIndex) != -1)
                    {
                        if (hasPercussionTrack)
                        {
                            OnFail(startIndex, "Already has percussion track");
                            return false;
                        }

                        hasPercussionTrack = true;
                        midiChannels.Add(new MidiChannel(this, midiOut, midiString, startIndex, 9, startTime));
                    }
                    else
                    {
                        if (channel == 16)
                        {
                            OnFail(startIndex, "Only 16 channels are allowed");
                            return false;
                        }

                        midiChannels.Add(new MidiChannel(this, midiOut, midiString, startIndex, channel, startTime));
                    }
                    channel += channel == 8 ? 2 : 1;
                }
                else
                {
                    OnFail(startIndex, "Channel has no content");
                    return false;
                }

                startIndex = 1 + midiString.IndexOf('|', startIndex);
            }
            while (startIndex > 0);

            if (midiChannels.Count == 0)
            {
                OnFail(startIndex, "No channels to play");
                return false;
            }

            tmr = new Timer(OnTick);
            tmr.Change(1, 1);

            IsPlaying = true;

            return true;
        }

        public void Stop()
        {
            if (IsPlaying)
            {
                Cleanup();
            }
        }

        void OnTick(object args)
        {

#if DEBUG
            tmr.Change(Timeout.Infinite, Timeout.Infinite);
#endif

            removeChannelList.Clear();

            foreach (MidiChannel midiChannel in midiChannels)
            {
                bool channelStillGoing = true;

                try
                {
                    channelStillGoing = midiChannel.Tick();
                }
                catch (MidiParsingException exc)
                {
                    OnFail(exc.Index, exc.Message);
                    return;
                }

                if (!channelStillGoing)
                {
                    removeChannelList.Add(midiChannel);
                }
            }

            foreach (MidiChannel midiChannel in removeChannelList)
                midiChannels.Remove(midiChannel);

            if (midiChannels.Count == 0 && shutDownTime == DateTime.MaxValue)
            {
                shutDownTime = DateTime.Now + TimeSpan.FromSeconds(1);
            }

            if (DateTime.Now > shutDownTime)
            {
                OnEnded();
            }
            else
            {
#if DEBUG
                tmr.Change(1, 1);
#endif
            }
        }

        protected void OnEnded()
        {
            tmr.Change(Timeout.Infinite, Timeout.Infinite);

            Cleanup();

            if (Ended != null)
                Ended(this, EventArgs.Empty);
        }

        protected void OnFail(int index, string error)
        {
            Cleanup();

            if (Failed != null)
                Failed(this, new MidiStringPlayerFailedEventArgs(index, error));
        }

        bool HasContent(string str, int index)
        {
            int length = str.Length;

            while (index < length && str[index] != '|')
            {
                if (!Char.IsWhiteSpace(str, index))
                    return true;

                index++;
            }

            return false;
        }
        
        void OnWindowClosed(object sender, EventArgs args)
        {
            Stop();
        }

        void Cleanup()
        {
            if (tmr != null)
            {
                tmr.Change(Timeout.Infinite, Timeout.Infinite);

            }

            IsPlaying = false;
        }
    }
}
