using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeccyEngine
{
    public struct Rect
    {
        public int X;
        public int Y;
        public int W;
        public int H;

        public static bool Intersects(ref Rect A, ref Rect B)
        {
            if (A.X <= B.X + B.W && 
                B.X <= A.X + A.W && 
                A.Y <= B.Y + B.H && 
                B.Y <= A.Y + A.H)
                return true;

            return false;
        }
        
    }
}
