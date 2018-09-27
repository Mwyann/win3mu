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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConFrames
{
    public class Interop
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);
        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_INPUT_HANDLE = -10;
        public const int STD_ERROR_HANDLE = -12;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool WriteConsoleOutput(
          IntPtr hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateConsoleScreenBuffer(
             uint dwDesiredAccess,
             uint dwShareMode,
             IntPtr secutiryAttributes,
             UInt32 flags,
             IntPtr screenBufferData
             );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferSize(IntPtr hConsoleOutput, Coord dwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleActiveScreenBuffer(IntPtr hConsoleOutput);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, Coord dwCursorPosition);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorInfo(IntPtr hConsoleOutput, ref CONSOLE_CURSOR_INFO info);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleCursorInfo(IntPtr hConsoleOutput, out CONSOLE_CURSOR_INFO info);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleWindowInfo(IntPtr hConsoleOutput, bool absolute, ref SmallRect rect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO info);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        public static bool SetConsoleCursorVisible(IntPtr hConsole, bool visible)
        {
            CONSOLE_CURSOR_INFO info;
            if (!GetConsoleCursorInfo(hConsole, out info))
                return false;
            if (info.bVisible != visible)
            {
                info.bVisible = visible;
                return SetConsoleCursorInfo(hConsole, ref info);
            }
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CONSOLE_CURSOR_INFO
        {
            public uint dwSize;
            public bool bVisible;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public Coord dwSize;
            public Coord dwCursorPosition;
            public ushort wAttributes;
            public SmallRect srWindow;
            public Coord dwMaximumWindowSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CONSOLE_SCREEN_BUFFER_INFOEX
        {
            public int cbSize;
            public Coord dwSize;
            public Coord dwCursorPosition;
            public ushort wAttributes;
            public SmallRect srWindow;
            public Coord dwMaximumWindowSize;
            public ushort wPopupAttributes;
            public bool bFullscreenSupported;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public uint[] ColorTable;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFOEX info);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFOEX info);


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Coord GetLargestConsoleWindowSize(IntPtr hConsoleOutput);


        public static void SetBufferAndScreenSize(IntPtr hConsoleOutput, short x, short y)
        {
            var largestSize = GetLargestConsoleWindowSize(hConsoleOutput);
            if (x > largestSize.X || y > largestSize.Y)
                throw new ArgumentException(string.Format("requested size is too big (max = {0} x {1})", largestSize.X, largestSize.Y));


            CONSOLE_SCREEN_BUFFER_INFO bi;
            if (!GetConsoleScreenBufferInfo(hConsoleOutput, out bi))
                throw new InvalidOperationException("GetConsoleScreenBufferInfo failed");

            Coord windowSize = new Coord((short)(bi.srWindow.Width + 1), (short)(bi.srWindow.Height + 1));

            if (windowSize.X > x || windowSize.Y > y)
            {
                var shrink = new SmallRect();
                shrink.Right = (short)(x < windowSize.X ? x - 1 : windowSize.X - 1);
                shrink.Bottom = (short)(y < windowSize.Y ? y - 1 : windowSize.Y - 1);

                if (!SetConsoleWindowInfo(hConsoleOutput, true, ref shrink))
                    throw new InvalidOperationException("Failed to shrink window");
            }

            if (!SetConsoleScreenBufferSize(hConsoleOutput, new Coord(x, y)))
                throw new InvalidOperationException("Failed to resize buffer");

            var info = new SmallRect();
            info.Right = (short)(x - 1);
            info.Bottom = (short)(y - 1);
            if (!SetConsoleWindowInfo(hConsoleOutput, true, ref info))
                throw new InvalidOperationException("Failed to resize window");

            CONSOLE_SCREEN_BUFFER_INFO cbi;
            GetConsoleScreenBufferInfo(hConsoleOutput, out cbi);
        }

        public const uint CONSOLE_TEXTMODE_BUFFER = 0x00000001;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_READWRITE = GENERIC_READ | GENERIC_WRITE;

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
            public short Width { get { return (short)(Right - Left); } }
            public short Height { get { return (short)(Bottom - Top); } }

        }

    }
}
