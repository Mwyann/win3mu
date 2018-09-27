/*
Sharp86 - 8086 Emulator
Copyright (C) 2017-2018 Topten Software.

Sharp86 is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Sharp86 is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Sharp86.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public struct FarPointer
    {
        public FarPointer(ushort seg, ushort offset)
        {
            value = (uint)(seg << 16 | offset);
        }

        public FarPointer(uint ptr)
        {
            value = ptr;
        }

        public uint value;

        public ushort Segment { get { return (ushort)(value >> 16); } }
        public ushort Offset { get { return (ushort)(value & 0xFFFF); } }

        public override string ToString()
        {
            return string.Format("0x{0:X4}:0x{1:X4}", value >> 16, value & 0xFFFF);
        }
    }
}
