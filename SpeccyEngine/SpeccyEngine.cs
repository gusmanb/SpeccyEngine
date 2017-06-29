using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeccyEngine
{
    public class Speccy : IDisposable
    {
        SpeccyScene scene;
        public SpeccyScene CurrentScene => scene;

        public event EventHandler NextScene;

        SpeccyScreen screen;
        Graphics g;
        Thread engineThread;
        Control c;
        bool running = false;
        int mult = 1;

        public Speccy(int ScreenWidth, int ScreenHeight, int RenderMultiplier, Control RenderTarget)
        {
            c = RenderTarget;

            RenderTarget.GetType().InvokeMember("SetStyle",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, RenderTarget, 
                new object[] { ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true });

            RenderTarget.ClientSize = new Size(ScreenWidth * 8 * RenderMultiplier, ScreenHeight * 8 * RenderMultiplier);

            g = RenderTarget.CreateGraphics();
            screen = new SpeccyScreen(ScreenWidth, ScreenHeight, g);
            mult = Math.Max(1, RenderMultiplier);
        }

        bool disableInput;

        public bool DisableInput { get { return disableInput; } set { disableInput = value; } }
        
        public void SetScene(SpeccyScene Scene)
        {
            if (scene != null)
                scene.Dispose();

            scene = Scene;

            if (scene != null)
            {
                scene.Screen = screen;
                scene.Engine = this;
            }
        }

        public void Start()
        {
            if (engineThread != null)
                return;

            engineThread = new Thread(Run);
            engineThread.IsBackground = true;
            running = true;
            engineThread.Start();
        }

        public void Stop()
        {
            if (engineThread == null)
                return;

            running = false;
            engineThread.Join();
            engineThread = null;
        }

        private void Run(object state)
        {
            int cnt = 0;
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif

            while (running)
            {
                if (!disableInput)
                    SpeccyKeyboard.Update();
#if DEBUG
                cnt++;
#endif
                if (scene != null)
                {
                    if (scene.Finished)
                    {
                        if (NextScene != null)
                            NextScene(scene.FinishData, EventArgs.Empty);
                    }
                    else
                        scene.Frame(mult, screen, g);
                }
                else
                    Thread.Sleep(10);
#if DEBUG
                if (sw.ElapsedMilliseconds > 2000)
                {
                    Debug.WriteLine("FPS: " + (cnt * 1000.0 / 2000));
                    cnt = 0;
                    sw.Restart();
                }
#endif
            }

        }
        
        public void Invoke(Action ActionToInvoke)
        {
            c.Invoke(ActionToInvoke);
        }

        public void Dispose()
        {
            Stop();

            SetScene(null);

            if (screen != null)
            {
                screen.Dispose();
                screen = null;
            }

            if (g != null)
            {
                g.Dispose();
                g = null;
            }
        }
        

    }
}
