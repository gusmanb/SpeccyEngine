using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeccyAudio
{
    public class SpeccyBeeper : IDisposable
    {
        WaveOut output;
        SignalGenerator baseGenerator;

        const double freqBase = 261.63;
        const double aFreq = 1.0594630943592953;

        public SpeccyBeeper()
        {
            output = new WaveOut(WaveCallbackInfo.FunctionCallback());
            output.DesiredLatency = 1;
            output.NumberOfBuffers = 500;
            baseGenerator = new SignalGenerator(44100, 1);
            baseGenerator.Gain /= 2;
            baseGenerator.Type = SignalGeneratorType.Square;
            output.Init(baseGenerator);
        }

        public void Play(int Frequency, int Duration)
        {
            baseGenerator.Frequency = Frequency;
            output.Play();
            Thread.Sleep(Duration);
            output.Stop();
        }

        public void PlayAsync(int Frequency, int Duration)
        {
            output.Stop();
            baseGenerator.Frequency = Frequency;
            output.Play();
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

            Play(freq, msDuration);
        }

        public void Dispose()
        {
            if (output != null)
            {
                output.Stop();
                output.Dispose();
                output = null;
            }
        }
    }
}
