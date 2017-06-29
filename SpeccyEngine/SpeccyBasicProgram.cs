using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeccyEngine
{
    public abstract class SpeccyBasicProgram : SpeccyScene
    {
        int cursorX;
        int cursorY;

        int plotX = 0;
        int plotY = 0;

        protected SpeccyColor Ink = SpeccyColor.Black; 
        protected SpeccyColor Paper = SpeccyColor.White;

        protected bool Over = false;

        protected SpeccyColor[,,] Attribs { get { return Screen.Attributes; } }
        protected byte[,] Pixels { get { return Screen.Pixels; } }

        protected Random Rnd = new Random();

        protected SpeccyBasicFont FontA = new SpeccyBasicFont();
        protected SpeccyBasicFont FontB = new SpeccyBasicFont();

        const double freqBase = 261.63;
        const double aFreq = 1.0594630943592953;

        private SpeccyBasicFont currentFont;

        public SpeccyBasicProgram()
        {
            currentFont = FontA;
            AutoClear = false;
            FPS = 60;
        }

        protected void LockScreen()
        {
            Screen.ScreenLock = true;
        }

        protected void UnlockScreen()
        {
            Screen.ScreenLock = false;
        }

        protected void Cls()
        {
            Screen.Clear(Ink, Paper);
        }

        protected void Pause(int Milliseconds)
        {
            Thread.Sleep(Milliseconds);
        }

        protected void Print(string Value)
        {
            PrintAt(cursorX, cursorY, Value);
        }

        protected void Print(SpeccyColor Ink, string Value)
        {
            PrintAt(Ink, cursorX, cursorY, Value);
        }

        protected void Print(SpeccyColor Ink, SpeccyColor Paper, string Value)
        {
            PrintAt(Ink, Paper, cursorX, cursorY, Value);
        }

        protected void PrintAt(int X, int Y, string Value)
        {
            if (Over)
            {
                int len = Value.Length;
                cursorX = X;
                cursorY = Y;
                bool lastInc = false;
                
                for (int buc = 0; buc < len; buc++)
                {
                    if (Value[buc] == '\r')
                        continue;

                    lastInc = false;

                    if (Value[buc] != '\n')
                    {
                       
                        if (cursorY >= Screen.Height)
                        {
                            cursorY--;
                            Screen.ShiftChars(1, Ink, Paper);
                        }

                        Screen.PrintChar(currentFont[Value[buc]], cursorX, cursorY, Over ? SpeccyMode.OR : SpeccyMode.COPY);
                        cursorX++;

                        if (cursorX >= Screen.Width)
                        {
                            cursorX = 0;
                            cursorY++;
                            lastInc = true;

                        }
                    }
                    else
                    {
                        cursorX = 0;
                        cursorY++;
                        lastInc = true;
                    }
                }

                cursorX = 0;
                if (!lastInc)
                    cursorY++;
            }
            else
                PrintAt(Ink, Paper, X, Y, Value);
        }

        protected void PrintAt(SpeccyColor Ink, int X, int Y, string Value)
        {
            int len = Value.Length;
            cursorX = X;
            cursorY = Y;
            bool lastInc = false;

            for (int buc = 0; buc < len; buc++)
            {
                if (Value[buc] == '\r')
                    continue;

                lastInc = false;

                if (Value[buc] != '\n')
                {
                    
                    if (cursorY >= Screen.Height)
                    {
                        cursorY--;
                        Screen.ShiftChars(1, Ink, Paper);
                    }

                    Screen.PrintChar(currentFont[Value[buc]], Ink, cursorX, cursorY, Over ? SpeccyMode.OR : SpeccyMode.COPY);
                    cursorX++;

                    if (cursorX >= Screen.Width)
                    {
                        cursorX = 0;
                        cursorY++;
                        lastInc = true;

                    }
                }
                else
                {
                    cursorX = 0;
                    cursorY++;
                    lastInc = true;
                }
            }

            cursorX = 0;
            if (!lastInc)
                cursorY++;
        }

        protected void PrintAt(SpeccyColor Ink, SpeccyColor Paper, int X, int Y, string Value)
        {
            int len = Value.Length;
            cursorX = X;
            cursorY = Y;
            bool lastInc = false;

            for (int buc = 0; buc < len; buc++)
            {
                if (Value[buc] == '\r')
                    continue;

                lastInc = false;

                if (Value[buc] != '\n')
                {
                    if (cursorY >= Screen.Height)
                    {
                        cursorY--;
                        Screen.ShiftChars(1, Ink, Paper);
                    }

                    Screen.PrintChar(currentFont[Value[buc]], Ink, Paper, cursorX, cursorY, Over ? SpeccyMode.OR : SpeccyMode.COPY);
                    cursorX++;

                    if (cursorX >= Screen.Width)
                    {
                        cursorX = 0;
                        cursorY++;
                        lastInc = true;

                    }
                }
                else
                {
                    cursorX = 0;
                    cursorY++;
                    lastInc = true;
                }
                
            }

            cursorX = 0;
            if(!lastInc)
                cursorY++;
        }

        protected void SwapFont()
        {
            if (currentFont == FontA)
                currentFont = FontB;
            else
                currentFont = FontA;
        }

        protected void DefChar(char Character, byte[] Data)
        {
            currentFont.SetChar(Character, Data);
        }

        protected void Plot(int X, int Y)
        {
            plotX = X;
            plotY = Y;
            Screen.Plot(X, Y);
        }

        protected void PlotInverse(int X, int Y)
        {
            plotX = X;
            plotY = Y;
            Screen.PlotInverse(X, Y);
        }

        protected void PlotClear(int X, int Y)
        {
            plotX = X;
            plotY = Y;
            Screen.PlotClear(X, Y);
        }

        protected void Plot(SpeccyColor Ink, int X, int Y)
        {
            plotX = X;
            plotY = Y;
            Screen.Plot(X, Y, Ink);
        }

        protected void Plot(SpeccyColor Ink, SpeccyColor Paper, int X, int Y)
        {
            plotX = X;
            plotY = Y;
            Screen.Plot(X, Y, Ink, Paper);
        }

        protected void Draw(int X, int Y)
        {
            Screen.Line(plotX, plotY, X, Y);
            plotX = X;
            plotY = Y;
        }

        protected void DrawInverse(int X, int Y)
        {
            Screen.LineInverse(plotX, plotY, X, Y);
            plotX = X;
            plotY = Y;
        }

        protected void DrawClear(int X, int Y)
        {
            Screen.LineClear(plotX, plotY, X, Y);
            plotX = X;
            plotY = Y;
        }

        protected void Draw(SpeccyColor Ink, int X, int Y)
        {
            Screen.Line(plotX, plotY, X, Y, Ink);
            plotX = X;
            plotY = Y;
        }

        protected void Draw(SpeccyColor Ink, SpeccyColor Paper, int X, int Y)
        {
            Screen.Line(plotX, plotY, X, Y, Ink, Paper);
            plotX = X;
            plotY = Y;
        }

        protected void Circle(int X, int Y, int Radius)
        {
            Screen.Circle(X, Y, Radius);
            plotX = X;
            plotY = Y;
        }

        protected void CircleInverse(int X, int Y, int Radius)
        {
            Screen.CircleInverse(X, Y, Radius);
            plotX = X;
            plotY = Y;
        }

        protected void CircleClear(int X, int Y, int Radius)
        {
            Screen.CircleClear(X, Y, Radius);
            plotX = X;
            plotY = Y;
        }

        protected void Circle(SpeccyColor Ink, int X, int Y, int Radius)
        {
            Screen.Circle(X, Y, Radius, Ink);
            plotX = X;
            plotY = Y;
        }

        protected void Circle(SpeccyColor Ink, SpeccyColor Paper, int X, int Y, int Radius)
        {
            Screen.Circle(X, Y, Radius, Ink, Paper);
            plotX = X;
            plotY = Y;
        }

        protected void AdvancedBeep(int Frequency, int MsDuration)
        {
            SpeccyBeeper.PlayBeepSync((ushort)Frequency, MsDuration);
        }

        protected void Beep(double Duration, int Pitch)
        {
            int msDuration = (int)(Duration * 1000);
            int freq = (int)(Math.Round(freqBase * Math.Pow(aFreq, Pitch)));

            SpeccyBeeper.PlayBeepSync((ushort)freq, msDuration);
        }
        
        string inputResult;
        bool inputSuccess;

        protected bool Input(string Title, string Prompt, out string Result)
        {
            Result = null;

            var me = this;

            Engine.Invoke(() => 
            {
                string value = SpeccyInputBox.ShowDialog(Prompt, Title);

                if (value == null)
                    me.inputSuccess = false;
                else
                {
                    me.inputSuccess = true;
                    me.inputResult = value;
                }
            });

            if (inputSuccess)
                Result = inputResult;

            return inputSuccess;

        }

        protected bool Input(string Title, string Prompt, out int Result)
        {
            Result = 0;

            var me = this;

            Engine.Invoke(() =>
            {
                string value = SpeccyInputBox.ShowDialog(Prompt, Title);

                if (value == null)
                    me.inputSuccess = false;
                else
                {
                    me.inputSuccess = true;
                    me.inputResult = value;
                }
            });

            if (inputSuccess)
            {
                if (!int.TryParse(inputResult, out Result))
                    return false;
            }

            return inputSuccess;
        }

        protected bool Input(string Title, string Prompt, out double Result)
        {
            Result = 0;

            var me = this;

            Engine.Invoke(() =>
            {
                string value = SpeccyInputBox.ShowDialog(Prompt, Title);

                if (value == null)
                    me.inputSuccess = false;
                else
                {
                    me.inputSuccess = true;
                    me.inputResult = value;
                }
            });

            if (inputSuccess)
            {
                if (!double.TryParse(inputResult, out Result))
                    return false;
            }

            return inputSuccess;
        }

        protected byte Bin(string Value)
        {
            if (Value.Length != 8)
                throw new InvalidCastException();

            byte val = 0;

            for (int buc = 0; buc < 8; buc++)
            {
                char c = Value[buc];

                if (c == '1' || c == '@')
                    val |= (byte)(128 >> buc);
                else if (c != '0' && c != '.')
                    throw new InvalidCastException();

            }

            return val;
        }

        bool running = false;
        
        public override void Update()
        {
            if (!running)
            {
                ThreadPool.QueueUserWorkItem(Run);
                running = true;
            }
        }

        void Run(object state)
        {
            Cls();
            Main();
            Finished = true;
        }

        public override void Dispose()
        {
            
        }

        public abstract void Main();
    }
}
