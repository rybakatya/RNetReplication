using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RapidNet.Replication.Culling
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Cell : IEquatable<Cell>
    {
        internal sbyte _x;
        internal sbyte _z;
        internal Rect rect;


        public bool Equals(Cell other)
        {
            if (_x == other._x && _z == other._z)
                return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return UniqueNumber(_x, _z);
        }
        private int UniqueNumber(int x, int y)
        {
            // Shift the range from [-20, 20] to [0, 40]
            int xShifted = x + 20;
            int yShifted = y + 20;

            // Map the shifted coordinates to a unique number
            int uniqueNum = xShifted * 41 + yShifted;

            return uniqueNum;
        }

        public override string ToString()
        {
            return "Cell { x: " + _x + ", y:" + _z + "}";
        }
    }
}
