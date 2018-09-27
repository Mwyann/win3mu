/*
Win3mu - Windows 3 Emulator
Copyright (C) 2017 Topten Software.

Win3mu is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Win3mu is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Win3mu.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win3muCore
{
    public static class BitUtils
    {
        public static ushort Hiword(this uint dword)
        {
            return (ushort)(dword >> 16);
        }

        public static ushort Loword(this uint dword)
        {
            return (ushort)(dword & 0xFFFF);
        }

        public static ushort Hiword(this int dword)
        {
            return (ushort)(((uint)dword) >> 16);
        }

        public static ushort Loword(this int dword)
        {
            return (ushort)(dword & 0xFFFF);
        }

        public static uint MakeDWord(ushort lo, ushort hi)
        {
            return (uint)(hi << 16 | lo);
        }

        public static ushort Loword(this IntPtr ip)
        {
            return unchecked((uint)ip.ToInt64()).Loword();
        }

        public static ushort Hiword(this IntPtr ip)
        {
            return unchecked((uint)ip.ToInt64()).Hiword();
        }

        public static uint DWord(this IntPtr ip)
        {
            return unchecked((uint)ip.ToInt64());
        }

        public static IntPtr DWordToIntPtr(uint val)
        {
            return (IntPtr)unchecked((int)val);
        }

        public static IntPtr MakeIntPtr(ushort lo, ushort hi)
        {
            return (IntPtr)unchecked((int)MakeDWord(lo, hi));
        }

        public static uint ToUInt32(this IntPtr This)
        {
            return unchecked((uint)This.ToInt64());
        }
    }
}
