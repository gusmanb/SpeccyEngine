using NAudio.Midi;
using Petzold.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeccyAudio
{
    public class SpeccyAY : IDisposable
    {
        MidiStringPlayer mps;

        public SpeccyAY()
        {
            mps = new MidiStringPlayer();
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

        public void Dispose()
        {
            if (mps != null)
            {
                mps.Stop();
                mps = null;
            }
        }
    }
}
