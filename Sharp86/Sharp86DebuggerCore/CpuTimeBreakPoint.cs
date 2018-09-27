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
    class CpuTimeBreakPoint : BreakPoint
    {
        public CpuTimeBreakPoint()
        {
        }

        public CpuTimeBreakPoint(ulong cputime)
        {
            CpuTime = cputime;
        }

        [Json("cpuTime")]
        public ulong CpuTime;

        public override bool ShouldBreak(DebuggerCore debugger)
        {
            return debugger.CPU.CpuTime == CpuTime;
        }

        public override string ToString()
        {
            return base.ToString(string.Format("cputime {0}", CpuTime));
        }

        public override string EditString
        {
            get
            {
                return string.Format("cputime {0}", CpuTime);
            }
        }
    }
}
