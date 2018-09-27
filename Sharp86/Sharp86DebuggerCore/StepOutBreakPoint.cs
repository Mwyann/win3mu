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
    public class StepOutBreakPoint : BreakPoint
    {
        public StepOutBreakPoint(CPU cpu)
        {
            // Store the current stack pointer
            _ssBreakOnReturn = cpu.ss;
            _spBreakOnReturn = cpu.sp;
        }

        ushort _ssBreakOnReturn = 0;
        ushort _spBreakOnReturn = 0;

        public override string EditString
        {
            get
            {
                return "";
            }
        }


        public override bool ShouldBreak(DebuggerCore debugger)
        {
            // Break after executing a return instruction when the stack
            // pointer is higher than it currently is.

            var cpu = debugger.CPU;
            return cpu.DidReturn && cpu.ss == _ssBreakOnReturn && cpu.sp > _spBreakOnReturn;
        }
    }
}
