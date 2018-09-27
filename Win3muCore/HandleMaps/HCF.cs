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
    public struct HCF
    {
        public IntPtr value;

        public static implicit operator HCF(IntPtr value) { return new HCF() { value = value }; }
        public static HCF Null = IntPtr.Zero;
        public static HandleMap Map = new HandleMap();
        public static HCF To32(ushort HCF) { return new HCF() { value = Map.To32(HCF) }; }
        public static ushort To16(HCF HCF) { return Map.To16(HCF.value); }
        public static void Destroy(ushort HCF) { Map.Destroy16(HCF); }
        public override string ToString()
        {
            return string.Format("HCF(0x{0:X}/0x{1:X})", Map.To16(value), (ulong)value.ToInt64());
        }
    }
}
