using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public class SpeccyAnimation
    {
        int currentFrame = 0;
        int maxFrame = 0;
        Dictionary<int, SpeccyKeyFrame> keyFrames = new Dictionary<int, SpeccyKeyFrame>();

        public bool Finished { get { return currentFrame == maxFrame; } }

        public SpeccyAnimation(int FrameCount)
        {
            maxFrame = FrameCount;
        }

        public void AddKeyFrame(SpeccyKeyFrame KeyFrame)
        {
            keyFrames[KeyFrame.Frame] = KeyFrame;
        }

        public void RemoveKeyFrame(int Frame)
        {
            keyFrames.Remove(Frame);
        }

        public void Play()
        {
            if (currentFrame == maxFrame)
                return;

            currentFrame++;
            SpeccyKeyFrame play;

            if (keyFrames.TryGetValue(currentFrame, out play))
                play.FrameAction();

        }
    }

    public class SpeccyKeyFrame
    {
        public int Frame { get; set; }
        public Action FrameAction { get; set; }
    }
}
