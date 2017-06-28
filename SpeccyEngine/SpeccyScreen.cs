using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public unsafe class SpeccyScreen : IDisposable
    {
        #region Interop
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        [DllImport("kernel32.dll", EntryPoint = "MoveMemory", SetLastError = false)]
        static extern void MoveMemory(IntPtr destination, IntPtr source, uint length);

        [DllImport("kernel32.dll", EntryPoint = "RtlFillMemory", SetLastError = false)]
        static extern void FillMemory(IntPtr destination, int length, byte fill);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
        int nWidthDest, int nHeightDest,
        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, TernaryRasterOperations dwRop);

        [DllImport("msimg32.dll")]
        public static extern int TransparentBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint clr);

        public enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }
        #endregion

        byte[,] pixels;
        SpeccyColor[,,] attributes;

        Bitmap backBuffer;
        BitmapData backBufferData;

        SpeccyColor* backPointer;

        int width;
        int height;

        int bufferWidth;
        int bufferHeight;
        int bufferStride;

        IntPtr srcDc;

        ManualResetEvent vSync;

        bool skipBuffer = false;

        public ManualResetEvent VSync { get { return vSync; } }

        public bool ScreenLock { get { return skipBuffer; } set { VSync.WaitOne(); skipBuffer = value; } }

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public byte[,] Pixels { get { return pixels; } }
        public SpeccyColor[,,] Attributes { get { return attributes; } }

        public SpeccyScreen(int Width, int Height, Graphics BaseGraphics)
        {

            width = Width;
            height = Height;

            var dc = BaseGraphics.GetHdc();
            srcDc = CreateCompatibleDC(dc);
            BaseGraphics.ReleaseHdc(dc);

            pixels = new byte[width, height * 8];
            attributes = new SpeccyColor[width, height, 2];
            backBuffer = new Bitmap(Width * 8, Height * 8, PixelFormat.Format32bppRgb);
            backBufferData = backBuffer.LockBits(new Rectangle(0, 0, Width * 8, Height * 8), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            backPointer = (SpeccyColor*)backBufferData.Scan0.ToPointer();


            bufferWidth = backBufferData.Width;
            bufferHeight = backBufferData.Height;
            bufferStride = backBufferData.Stride;

            vSync = new ManualResetEvent(true);
        }

        public void Clear(SpeccyColor Foreground, SpeccyColor Background)
        {
            Array.Clear(pixels, 0, width * height * 8);
            
            fixed (SpeccyColor* attrPtr = &attributes[0, 0, 0])
            {
                int max = width * height * 2;

                for (int buc = 0; buc < max; buc++)
                {
                    if (buc % 2 == 0)
                        attrPtr[buc] = Background;
                    else
                        attrPtr[buc] = Foreground;
                }
            }
            
        }

        public void PrintChar(SpeccyFontChar Char, SpeccyColor ForeColor, SpeccyColor BackColor, int X, int Y, SpeccyMode Mode)
        {
            if (X > width || Y > height)
                return;

            int baseY = Y * 8;
            var map = Char.Data;

            int row = 0;

            switch (Mode)
            {
                case SpeccyMode.AND:

                    for (int y = baseY; y < baseY + 8; y++)
                        pixels[X, y] &= map[row++];

                    break;

                case SpeccyMode.OR:

                    for (int y = baseY; y < baseY + 8; y++)
                        pixels[X, y] |= map[row++];

                    break;

                case SpeccyMode.XOR:

                    for (int y = baseY; y < baseY + 8; y++)
                        pixels[X, y] ^= map[row++];

                    break;

                default:

                    for (int y = baseY; y < baseY + 8; y++)
                        pixels[X, y] = map[row++];

                    break;

            }

            

            attributes[X, Y, 0] = BackColor;
            attributes[X, Y, 1] = ForeColor;
        }

        public void PrintChar(SpeccyFontChar Char, SpeccyColor ForeColor, SpeccyColor BackColor, int X, int Y, SpeccyMode Mode, int XShift, int YShift, bool ExpandAttributes)
        {
            if (X > width || Y > height)
                return;

            int baseY = Y * 8 + YShift;
            int maxY = Math.Min(height * 8, baseY + 8);

            var map = Char.Data;

            int row = 0;

            bool xFits = X + 1 < width || XShift == 0;
            bool yFits = Y < height - 1 || YShift == 0;
            
            switch (Mode)
            {
                case SpeccyMode.AND:

                    if (XShift != 0)
                    {
                        if (xFits)
                        {

                            for (int y = baseY; y < maxY; y++)
                            {
                                pixels[X, y] &= (byte)(map[row] >> XShift);
                                pixels[X + 1, y] &= (byte)(map[row++] << 8 - XShift);
                            }
                        }
                        else
                        {
                            for (int y = baseY; y < maxY; y++)
                                pixels[X, y] &= (byte)(map[row++] >> XShift);
                        }
                    }
                    else
                    {
                        for (int y = baseY; y < maxY; y++)
                            pixels[X, y] &= map[row++];
                    }

                    break;

                case SpeccyMode.OR:

                    if (XShift != 0)
                    {
                        if (xFits)
                        {

                            for (int y = baseY; y < maxY; y++)
                            {
                                pixels[X, y] |= (byte)(map[row] >> XShift);
                                pixels[X + 1, y] |= (byte)(map[row++] << 8 - XShift);
                            }
                        }
                        else
                        {
                            for (int y = baseY; y < maxY; y++)
                                pixels[X, y] |= (byte)(map[row++] >> XShift);
                        }
                    }
                    else
                    {
                        for (int y = baseY; y < maxY; y++)
                            pixels[X, y] |= map[row++];
                    }

                    break;

                case SpeccyMode.XOR:

                    if (XShift != 0)
                    {
                        if (xFits)
                        {

                            for (int y = baseY; y < maxY; y++)
                            {
                                pixels[X, y] ^= (byte)(map[row] >> XShift);
                                pixels[X + 1, y] ^= (byte)(map[row++] << 8 - XShift);
                            }
                        }
                        else
                        {
                            for (int y = baseY; y < maxY; y++)
                                pixels[X, y] ^= (byte)(map[row++] >> XShift);
                        }
                    }
                    else
                    {
                        for (int y = baseY; y < maxY; y++)
                            pixels[X, y] ^= map[row++];
                    }

                    break;

                default:

                    if (XShift != 0)
                    {
                        if (xFits)
                        {

                            for (int y = baseY; y < maxY; y++)
                            {
                                pixels[X, y] = (byte)(map[row] >> XShift);
                                pixels[X + 1, y] = (byte)(map[row++] << 8 - XShift);
                            }
                        }
                        else
                        {
                            for (int y = baseY; y < maxY; y++)
                                pixels[X, y] = (byte)(map[row++] >> XShift);
                        }
                    }
                    else
                    {
                        for (int y = baseY; y < maxY; y++)
                            pixels[X, y] = map[row++];
                    }

                    break;

            }
            
            attributes[X, Y, 0] = BackColor;
            attributes[X, Y, 1] = ForeColor;

            if (ExpandAttributes)
            {
                if (XShift != 0 && xFits)
                {
                    attributes[X + 1, Y, 0] = BackColor;
                    attributes[X + 1, Y, 1] = ForeColor;
                }

                if (YShift != 0 && yFits)
                {
                    attributes[X , Y + 1, 0] = BackColor;
                    attributes[X, Y + 1, 1] = ForeColor;
                }

                if (XShift != 0 && YShift != 0 && xFits && yFits)
                {
                    attributes[X + 1, Y + 1, 0] = BackColor;
                    attributes[X + 1, Y + 1, 1] = ForeColor;
                }
            }

        }

        public void Plot(int X, int Y)
        {

            if (X >= bufferWidth || Y >= bufferHeight || X < 0 || Y < 0)
                return;

            int byteX = X / 8;
            int bit = X % 8;

            pixels[byteX, Y] |= (byte)(128 >> bit); 
        }

        public void Plot(int X, int Y, SpeccyColor ForeColor, SpeccyColor BackColor)
        {
            if (X >= bufferWidth || Y >= bufferHeight || X < 0 || Y < 0)
                return;

            int byteX = X / 8;
            int attrY = Y / 8;
            int bit = X % 8;

            pixels[byteX, Y] |= (byte)(128 >> bit);
            attributes[byteX, attrY, 0] = BackColor;
            attributes[byteX, attrY, 1] = ForeColor;
        }

        public void PlotInverse(int X, int Y)
        {

            if (X >= bufferWidth || Y >= bufferHeight || X < 0 || Y < 0)
                return;

            int byteX = X / 8;
            int bit = X % 8;

            pixels[byteX, Y] ^= (byte)(128 >> bit);
        }

        public void PlotClear(int X, int Y)
        {
            int byteX = X / 8;
            int bit = X % 8;

            pixels[byteX, Y] &= (byte)((128 >> bit) ^ 0xFF);
        }

        public void Line(int StartX, int StartY, int EndX, int EndY)
        {
            int w = EndX - StartX;
            int h = EndY - StartY;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                Plot(StartX, StartY);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    StartX += dx1;
                    StartY += dy1;
                }
                else
                {
                    StartX += dx2;
                    StartY += dy2;
                }
            }
        }

        public void LineInverse(int StartX, int StartY, int EndX, int EndY)
        {
            int w = EndX - StartX;
            int h = EndY - StartY;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                PlotInverse(StartX, StartY);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    StartX += dx1;
                    StartY += dy1;
                }
                else
                {
                    StartX += dx2;
                    StartY += dy2;
                }
            }
        }

        public void LineClear(int StartX, int StartY, int EndX, int EndY)
        {
            int w = EndX - StartX;
            int h = EndY - StartY;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                PlotClear(StartX, StartY);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    StartX += dx1;
                    StartY += dy1;
                }
                else
                {
                    StartX += dx2;
                    StartY += dy2;
                }
            }
        }

        public void Line(int StartX, int StartY, int EndX, int EndY, SpeccyColor ForeColor, SpeccyColor BackColor)
        {
            int w = EndX - StartX;
            int h = EndY - StartY;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                Plot(StartX, StartY, ForeColor, BackColor);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    StartX += dx1;
                    StartY += dy1;
                }
                else
                {
                    StartX += dx2;
                    StartY += dy2;
                }
            }
        }
        
        public void Circle(int X, int Y, int Radius)
        {
            var f = 1 - Radius;
            var ddF_x = 1;
            var ddF_y = -2 * Radius;
            var x = 0;
            var y = Radius;

            //Bottom middle
            Plot(X, Y + Radius);

            //Top Middle
            Plot(X, Y - Radius);

            //Right Middle
            Plot(X + Radius, Y);

            //Left Middle
            Plot(X - Radius, Y);

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                //Lower Right 
                Plot(X + x, Y + y);
                Plot(X + y, Y + x);

                //Lower Left
                Plot(X - x, Y + y);
                Plot(X - y, Y + x);

                //Top Right
                Plot(X + x, Y - y);
                Plot(X + y, Y - x);

                //Top Left
                Plot(X - x, Y - y);
                Plot(X - y, Y - x);
            }
        }

        public void CircleInverse(int X, int Y, int Radius)
        {
            var f = 1 - Radius;
            var ddF_x = 1;
            var ddF_y = -2 * Radius;
            var x = 0;
            var y = Radius;

            //Bottom middle
            PlotInverse(X, Y + Radius);

            //Top Middle
            PlotInverse(X, Y - Radius);

            //Right Middle
            PlotInverse(X + Radius, Y);

            //Left Middle
            PlotInverse(X - Radius, Y);

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                //Lower Right 
                PlotInverse(X + x, Y + y);
                PlotInverse(X + y, Y + x);

                //Lower Left
                PlotInverse(X - x, Y + y);
                PlotInverse(X - y, Y + x);

                //Top Right
                PlotInverse(X + x, Y - y);
                PlotInverse(X + y, Y - x);

                //Top Left
                PlotInverse(X - x, Y - y);
                PlotInverse(X - y, Y - x);
            }
        }

        public void CircleClear(int X, int Y, int Radius)
        {
            var f = 1 - Radius;
            var ddF_x = 1;
            var ddF_y = -2 * Radius;
            var x = 0;
            var y = Radius;

            //Bottom middle
            PlotClear(X, Y + Radius);

            //Top Middle
            PlotClear(X, Y - Radius);

            //Right Middle
            PlotClear(X + Radius, Y);

            //Left Middle
            PlotClear(X - Radius, Y);

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                //Lower Right 
                PlotClear(X + x, Y + y);
                PlotClear(X + y, Y + x);

                //Lower Left
                PlotClear(X - x, Y + y);
                PlotClear(X - y, Y + x);

                //Top Right
                PlotClear(X + x, Y - y);
                PlotClear(X + y, Y - x);

                //Top Left
                PlotClear(X - x, Y - y);
                PlotClear(X - y, Y - x);
            }
        }

        public void Circle(int X, int Y, int Radius, SpeccyColor ForeColor, SpeccyColor BackColor)
        {
            var f = 1 - Radius;
            var ddF_x = 1;
            var ddF_y = -2 * Radius;
            var x = 0;
            var y = Radius;

            //Bottom middle
            Plot(X, Y + Radius, ForeColor, BackColor);

            //Top Middle
            Plot(X, Y - Radius, ForeColor, BackColor);

            //Right Middle
            Plot(X + Radius, Y, ForeColor, BackColor);

            //Left Middle
            Plot(X - Radius, Y, ForeColor, BackColor);

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                //Lower Right 
                Plot(X + x, Y + y, ForeColor, BackColor);
                Plot(X + y, Y + x, ForeColor, BackColor);

                //Lower Left
                Plot(X - x, Y + y, ForeColor, BackColor);
                Plot(X - y, Y + x, ForeColor, BackColor);

                //Top Right
                Plot(X + x, Y - y, ForeColor, BackColor);
                Plot(X + y, Y - x, ForeColor, BackColor);

                //Top Left
                Plot(X - x, Y - y, ForeColor, BackColor);
                Plot(X - y, Y - x, ForeColor, BackColor);
            }
        }
        
        public void Shift(int Pixels)
        {
            int h = height * 8;

            for (int y = 0; y < height * 8; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int fromCopy = y + Pixels;

                    if (fromCopy >= h)
                        pixels[x, y] = 0;
                    else
                        pixels[x, fromCopy] = 0;
                }
            }
        }

        public void ShiftChars(int Chars, SpeccyColor ForeColor, SpeccyColor BackColor)
        {
            int h = height * 8;
            int Pixels = Chars * 8;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int fromCopy = y + Pixels;

                    if (fromCopy >= h)
                        pixels[x, y] = 0;
                    else
                        pixels[x, fromCopy] = 0;
                }
            }

            h = height - 1;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    attributes[x, y, 0] = attributes[x, y + 1, 0];
                    attributes[x, y, 1] = attributes[x, y + 1, 1];
                }
            }

            for (int x = 0; x < width; x++)
            {
                attributes[x, h, 0] = BackColor;
                attributes[x, h, 1] = ForeColor;
            }
        }

        public void Render(Graphics g)
        {
            vSync.Reset();

            if(!skipBuffer)
                ComposeBuffer();

            var targetDc = g.GetHdc();
            var backB = backBuffer.GetHbitmap();
            SelectObject(srcDc, backB);
            BitBlt(targetDc, 0, 0, bufferWidth, bufferHeight, srcDc, 0, 0, TernaryRasterOperations.SRCCOPY);
            SelectObject(srcDc, IntPtr.Zero);
            DeleteObject(backB);
            g.ReleaseHdc(targetDc);

            vSync.Set();
        }

        public void Render(int Width, int Height, Graphics g)
        {
            vSync.Reset();

            if (!skipBuffer)
                ComposeBuffer();

            var targetDc = g.GetHdc();
            var backB = backBuffer.GetHbitmap();
            SelectObject(srcDc, backB);
            StretchBlt(targetDc, 0, 0, Width, Height, srcDc, 0, 0, bufferWidth, bufferHeight, TernaryRasterOperations.SRCCOPY);
            SelectObject(srcDc, IntPtr.Zero);
            DeleteObject(backB);
            g.ReleaseHdc(targetDc);

            vSync.Set();
        }

        private void ComposeBuffer()
        {
            int bufH = height * 8;

            for (int y = 0; y < bufH; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte currentChar = pixels[x, y];

                    int charPos = y * width * 8 + x * 8;

                    backPointer[charPos] = attributes[x, y / 8, (currentChar & 128) >> 7];
                    backPointer[charPos + 1] = attributes[x, y / 8, (currentChar & 64) >> 6];
                    backPointer[charPos + 2] = attributes[x, y / 8, (currentChar & 32) >> 5];
                    backPointer[charPos + 3] = attributes[x, y / 8, (currentChar & 16) >> 4];
                    backPointer[charPos + 4] = attributes[x, y / 8, (currentChar & 8) >> 3];
                    backPointer[charPos + 5] = attributes[x, y / 8, (currentChar & 4) >> 2];
                    backPointer[charPos + 6] = attributes[x, y / 8, (currentChar & 2) >> 1];
                    backPointer[charPos + 7] = attributes[x, y / 8, (currentChar & 1)];
                }
            }
        }

        public void Dispose()
        {
            if (backBuffer != null)
            {
                backBuffer.UnlockBits(backBufferData);
                backBuffer.Dispose();
                DeleteDC(srcDc);
                backBuffer.Dispose();
            }
        }
    }

    public enum SpeccyMode
    {
        COPY,
        AND,
        OR,
        XOR
        
    }
}
