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
    class MemoryChangeBreakPoint : BaseMemoryBreakPoint, IBreakPointMemWrite
    {
        public MemoryChangeBreakPoint()
        {
        }

        public MemoryChangeBreakPoint(ushort segment, ushort offset, ushort length) : 
            base(segment, offset, length)
        {
        }

        bool _tripped;

        public override bool ShouldBreak(DebuggerCore debugger)
        {
            bool retv = _tripped;
            _tripped = false;
            return retv;
        }

        public override string ToString()
        {
            return base.ToString(string.Format("mem 0x{0:X4}:{1:X4} - 0x{2:X4}:{3:X4} ({4} bytes)",
                Segment, Offset,
                Segment, Offset + Length,
                Length
                ));

        }

        void IBreakPointMemWrite.WriteByte(ushort seg, ushort offset, byte oldValue, byte newValue)
        {
            if (oldValue != newValue)
            {
                if (seg == Segment && offset >= Offset && offset < Offset + Length)
                {
                    _tripped = true;
                }
            }
        }

        public override string EditString
        {
            get
            {
                return string.Format("mem 0x{0:X4}:0x{1:X4},{2}", Segment, Offset, Length);
            }
        }
    }
}
