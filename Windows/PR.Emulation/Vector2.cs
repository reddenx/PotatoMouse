using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Emulation
{
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Empty
        {
            get { return new Vector2(0, 0); }
        }

        public static Vector2 operator -(Vector2 rhs, Vector2 lhs)
        {
            return new Vector2(rhs.X - lhs.X, rhs.Y - lhs.Y);
        }

        public static Vector2 operator +(Vector2 rhs, Vector2 lhs)
        {
            return new Vector2(rhs.X + lhs.X, rhs.Y + lhs.Y);
        }
    }
}
