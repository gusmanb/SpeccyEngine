// ReleaseInfo.cs by Charles Petzold, January 2010

namespace Petzold.Midi
{
    internal class ReleaseInfo
    {
        public ReleaseInfo(int note, double millisecondsElapsed, int channel)
        {
            Note = note;
            MillisecondsElapsed = millisecondsElapsed;
            Channel = channel;
        }

        public int Note { set; get; }
        public int Channel { get; set; }
        public double MillisecondsElapsed { set; get; }
    }
}
