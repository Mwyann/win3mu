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
using ConFrames;

namespace Sharp86
{
    public class RegistersWindow : Window
    {
        public RegistersWindow(TextGuiDebugger debugger) 
            : base("Registers", new Rect(100, 0, 20, 18))
        {
            _debugger = debugger;
        }

        TextGuiDebugger _debugger;

        void WriteReg(PaintContext ctx, string name, ushort value, ushort prevValue)
        {
            if (value != prevValue)
                ctx.ForegroundColor = ConsoleColor.White;
            else
                ctx.ForegroundColor = ConsoleColor.Gray;

            ctx.Write("{0}: {1:X4}", name, value);
        }

        void WriteFlag(PaintContext ctx, ushort value, ushort prevValue, ushort mask, string nameOn, string nameOff)
        {
            if ((value & mask) != (prevValue & mask))
                ctx.ForegroundColor = ConsoleColor.White;
            else
                ctx.ForegroundColor = ConsoleColor.Gray;

            if ((value & mask) != 0)
                ctx.Write(nameOn);
            else
                ctx.Write(nameOff);
        }

        public override void OnPaint(PaintContext ctx)
        {
            var cpu = _debugger.CPU;
            ctx.Write("        ");            WriteReg(ctx, " AX", cpu.ax, _ax); ctx.WriteLine();
            ctx.Write("        ");            WriteReg(ctx, " BX", cpu.bx, _bx); ctx.WriteLine();
            ctx.Write("        ");            WriteReg(ctx, " CX", cpu.cx, _cx); ctx.WriteLine();
            ctx.Write("        ");            WriteReg(ctx, " DX", cpu.dx, _dx); ctx.WriteLine();
            WriteReg(ctx, "DS", cpu.ds, _ds); WriteReg(ctx, " SI", cpu.si, _si); ctx.WriteLine();
            WriteReg(ctx, "ES", cpu.es, _es); WriteReg(ctx, " DI", cpu.di, _di); ctx.WriteLine();
            WriteReg(ctx, "SS", cpu.ss, _ss); WriteReg(ctx, " SP", cpu.sp, _sp); ctx.WriteLine();
            ctx.Write("        ");            WriteReg(ctx, " BP", cpu.bp, _bp); ctx.WriteLine();
            WriteReg(ctx, "CS", cpu.cs, _cs); WriteReg(ctx, " IP", cpu.ip, _ip); ctx.WriteLine();
            WriteReg(ctx, "FL", cpu.EFlags, _EFlags);

            ctx.WriteLine();
            ctx.WriteLine();

            WriteFlag(ctx, cpu.EFlags, _EFlags, (ushort)EFlag.ZF, "Z  ", "NZ ");
            WriteFlag(ctx, cpu.EFlags, _EFlags, (ushort)EFlag.CF, "C  ", "NC ");
            WriteFlag(ctx, cpu.EFlags, _EFlags, (ushort)EFlag.OF, "O  ", "NO ");
            WriteFlag(ctx, cpu.EFlags, _EFlags, (ushort)EFlag.SF, "S  ", "NS ");
            WriteFlag(ctx, cpu.EFlags, _EFlags, (ushort)EFlag.DF, "UP ", "DN");
            ctx.WriteLine();
            ctx.WriteLine();

            ctx.WriteLine("CT: {0}", cpu.CpuTime);
            ctx.WriteLine("DT: {0}", cpu.CpuTime - _cpuTime);
        }

        public void OnResume()
        {
            var cpu = _debugger.CPU;
            _ax = cpu.ax; 
            _bx = cpu.bx; 
            _cx = cpu.cx; 
            _dx = cpu.dx; 
            _si = cpu.si; 
            _di = cpu.di; 
            _bp = cpu.bp; 
            _sp = cpu.sp; 
            _ds = cpu.ds; 
            _es = cpu.es; 
            _ss = cpu.ss; 
            _cs = cpu.cs; 
            _ip = cpu.ip;
            _EFlags = cpu.EFlags;
            _cpuTime = cpu.CpuTime;
        }


        ushort _ax;
        ushort _bx;
        ushort _cx;
        ushort _dx;
        ushort _si;
        ushort _di;
        ushort _bp;
        ushort _sp;
        ushort _ds;
        ushort _es;
        ushort _ss;
        ushort _cs;
        ushort _ip;
        ushort _EFlags;
        ulong _cpuTime;
    }
}
