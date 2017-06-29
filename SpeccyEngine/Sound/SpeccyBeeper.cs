using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyBeeper
    {
        const double freqBase = 261.63;
        const double aFreq = 1.0594630943592953;
        
        public void Play(int Frequency, int Duration)
        {
            Console.Beep(Frequency, Duration);
        }

        public void PlayAsync(int Frequency, int Duration)
        {
            ThreadPool.QueueUserWorkItem((o) => Play(Frequency, Duration));
        }

        public void Beep(double Duration, int Pitch)
        {
            int msDuration = (int)(Duration * 1000);
            int freq = (int)(Math.Round(freqBase * Math.Pow(aFreq, Pitch)));

            Play(freq, msDuration);
        }

        public void BeepAsync(double Duration, int Pitch)
        {
            int msDuration = (int)(Duration * 1000);
            int freq = (int)(Math.Round(freqBase * Math.Pow(aFreq, Pitch)));

            PlayAsync(freq, msDuration);
        }
        
    }
}
