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
    public struct HGDIOBJ
    {
        public HGDIOBJ(IntPtr handle)
        {
            value = handle;
        }

        public IntPtr value;

        public static implicit operator HGDIOBJ(IntPtr value) { return new HGDIOBJ() { value = value }; }
        public static HGDIOBJ Null = IntPtr.Zero;
        public static HandleMap Map = new HandleMap();
        public static HGDIOBJ To32(ushort hObj) { return new HGDIOBJ() { value = Map.To32(hObj) }; }
        public static ushort To16(HGDIOBJ hObj) { return Map.To16(hObj.value); }
        public static void Destroy(ushort hMenu) { Map.Destroy16(hMenu); }
        public override string ToString()
        {
            return string.Format("HGDIOBJ(0x{0:X}/0x{1:X})", Map.To16(value), (ulong)value.ToInt64());
        }
    }
}
