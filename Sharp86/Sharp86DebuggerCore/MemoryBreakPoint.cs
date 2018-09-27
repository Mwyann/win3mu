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
using PetaJson;

namespace Sharp86
{
    public abstract class BaseMemoryBreakPoint : BreakPoint
    {
        public BaseMemoryBreakPoint()
        {
        }
        public BaseMemoryBreakPoint(ushort segment, ushort offset, ushort length)
        {
            Segment = segment;
            Offset = offset;
            Length = length;
        }

        [Json("address")]
        public uint Address
        {
            get
            {
                return (uint)(Segment << 16 | Offset);
            }
            set
            {
                Segment = (ushort)(value >> 16);
                Offset = (ushort)(value & 0xFFFF);
            }
        }

        public ushort Segment;
        public ushort Offset;

        [Json("length")]
        public uint Length;

    }
}
