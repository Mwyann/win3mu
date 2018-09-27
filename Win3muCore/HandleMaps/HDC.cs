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
    [MappedTypeAttribute]
    public struct HDC
    {
        public IntPtr value;

        public static implicit operator HDC(IntPtr value) { return new HDC() { value = value }; }
        public static HDC Null = IntPtr.Zero;
        public static HandleMap Map = new HandleMap();
        public static HDC To32(ushort hDC) { return new HDC() { value = Map.To32(hDC) }; }
        public static ushort To16(HDC hDC) { return Map.To16(hDC.value); }
        public static void Destroy(ushort hDC) { Map.Destroy16(hDC); }
        public override string ToString()
        {
            return string.Format("HDC(0x{0:X}/0x{1:X})", Map.To16(value), (ulong)value.ToInt64());
        }
    }
}
