/*
ConFrames - Gui Stuff for Console Windows
Copyright (C) 2017-2018 Topten Software.

ConFrames is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ConFrames is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ConFrames.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Runtime.InteropServices;

namespace ConFrames
{
    // Character info (matches format used by Windows Console API's)
    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Unicode)]
    public struct CharInfo
    {
        public char Char;
        public ushort Attributes;
    }

    // Rectangle
    public struct Rect
    {
        public Rect(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public int Left;
        public int Top;
        public int Width;
        public int Height;

        public int Right { get { return Left + Width; } }
        public int Bottom { get { return Top + Height; } }
        public Size Size { get { return new Size(Width, Height); } }

        public bool IntersectsWith(Rect other)
        {
            // Does it intersect
            if (other.Right < Left)
                return false;
            if (other.Left > Right)
                return false;
            if (other.Top > Bottom)
                return false;
            if (other.Bottom < Top)
                return false;

            return true;
        }

        public bool Intersect(Rect rectOther)
        {
            if (!IntersectsWith(rectOther))
            {
                Width = 0;
                Height = 0;
                return false;
            }

            var right = Right;
            var bottom = Bottom;
            Left = Math.Max(Left, rectOther.Left);
            Width = Math.Min(right, rectOther.Right) - Left;
            Top = Math.Max(Top, rectOther.Top);
            Height = Math.Min(bottom, rectOther.Bottom) - Top;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Rect))
                return false;

            return this == (Rect)obj;
        }

        public override int GetHashCode()
        {
            return Left.GetHashCode() ^ Top.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
        }

        public static bool operator ==(Rect a, Rect b)
        {
            return a.Left == b.Left && a.Top == b.Top && a.Width == b.Width && a.Height == b.Height;
        }

        public static bool operator !=(Rect a, Rect b)
        {
            return !(a == b);
        }
    }

    // Point
    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X;
        public int Y;
    }

    // Size
    public struct Size
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width;
        public int Height;


        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Size))
                return false;

            return this == (Size)obj;
        }

        public override int GetHashCode()
        {
            return Width.GetHashCode() ^ Height.GetHashCode();
        }

        public static bool operator ==(Size a, Size b)
        {
            return a.Width == b.Width && a.Height == b.Height;
        }

        public static bool operator !=(Size a, Size b)
        {
            return !(a == b);
        }

    }

    // View mode
    public enum ViewMode
    {
        StdOut,
        Desktop,
    }

    // Helper for working with attributes
    public static class Attribute
    {
        public static ushort Make(ConsoleColor foreground, ConsoleColor background)
        {
            return (ushort)(((byte)background << 4) | (ushort)foreground);
        }
    }

}
