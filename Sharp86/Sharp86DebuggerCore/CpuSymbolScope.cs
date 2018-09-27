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
    class CpuSymbolScope : GenericSymbolScope
    {
        public CpuSymbolScope(CPU cpu)
        {
            _cpu = cpu;
            RegisterSymbol("ax", () => _cpu.ax);
            RegisterSymbol("bx", () => _cpu.bx);
            RegisterSymbol("cx", () => _cpu.cx);
            RegisterSymbol("dx", () => _cpu.dx);
            RegisterSymbol("dxax", () => _cpu.dxax);
            RegisterSymbol("si", () => _cpu.si);
            RegisterSymbol("di", () => _cpu.di);
            RegisterSymbol("bp", () => _cpu.bp);
            RegisterSymbol("sp", () => _cpu.sp);
            RegisterSymbol("ip", () => _cpu.ip);
            RegisterSymbol("ds", () => _cpu.ds);
            RegisterSymbol("ss", () => _cpu.ss);
            RegisterSymbol("es", () => _cpu.es);
            RegisterSymbol("cs", () => _cpu.cs);
            RegisterSymbol("ah", () => _cpu.ah);
            RegisterSymbol("bh", () => _cpu.bh);
            RegisterSymbol("ch", () => _cpu.ch);
            RegisterSymbol("dh", () => _cpu.dh);
            RegisterSymbol("al", () => _cpu.al);
            RegisterSymbol("bl", () => _cpu.bl);
            RegisterSymbol("cl", () => _cpu.cl);
            RegisterSymbol("dl", () => _cpu.dl);
            RegisterSymbol("eflags", () => _cpu.EFlags);
            RegisterSymbol("cputime", () => _cpu.CpuTime);
        }

        CPU _cpu;
    }
}
