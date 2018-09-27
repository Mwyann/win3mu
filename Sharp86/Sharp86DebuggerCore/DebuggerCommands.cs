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
using System.IO;
using System.Linq;
using System.Text;

namespace Sharp86
{
    public class DebuggerCommands
    {
        [DebuggerHelp("List all available commands")]
        public void help(DebuggerCore debugger)
        {
            foreach (var mi in debugger.CommandDispatcher._commandHandlers
                    .SelectMany(x => x.GetType().GetMethods())
                    .OrderBy(x => x.Name))

            {
                var help = mi.GetCustomAttributes(true).OfType<DebuggerHelpAttribute>().FirstOrDefault();
                if (help == null)
                    continue;
                debugger.WriteLine("{0,20} - {1}", mi.Name.Replace("_", " "), help.Help);
            }
        }

        [DebuggerHelp("Set a break point at a code location")]
        public void bp(DebuggerCore debugger, FarPointer addr)
        {
            debugger.AddBreakPoint(new CodeBreakPoint(addr.Segment, addr.Offset));
        }

        [DebuggerHelp("List break points")]
        public void bp_list(DebuggerCore debugger)
        {
            foreach (var bp in debugger.BreakPoints)
            {
                debugger.WriteLine(bp.ToString());
            }
        }

        [DebuggerHelp("Delete break point")]
        public void bp_del(DebuggerCore debugger, BreakPoint bp)
        {
            debugger.RemoveBreakPoint(bp);
        }

        [DebuggerHelp("Reset break point trip counts")]
        public void bp_reset(DebuggerCore debugger, BreakPoint bp = null)
        {
            if (bp != null)
            {
                bp.TripCount = 0;
                debugger.WriteLine(bp.ToString());
            }
            else
            {
                foreach (var x in debugger.BreakPoints)
                {
                    x.TripCount = 0;
                }
                bp_list(debugger);
            }
        }
        [DebuggerHelp("Enable break point")]
        public void bp_on(DebuggerCore debugger, BreakPoint bp)
        {
            debugger.EnableBreakPoint(bp, true);
        }

        [DebuggerHelp("Disable break point")]
        public void bp_off(DebuggerCore debugger, BreakPoint bp)
        {
            debugger.EnableBreakPoint(bp, false);
        }

        [DebuggerHelp("Remove all break points")]
        public void bp_clear(DebuggerCore debugger)
        {
            debugger.RemoveAllBreakpoints();
        }

        [DebuggerHelp("Set a memory change break point")]
        public void bp_mem(DebuggerCore debugger, FarPointer addr, ushort length)
        {
            debugger.AddBreakPoint(new MemoryChangeBreakPoint(addr.Segment, addr.Offset, length));
        }

        [DebuggerHelp("Set a memory write break point")]
        public void bp_memw(DebuggerCore debugger, FarPointer addr, ushort length)
        {
            debugger.AddBreakPoint(new MemoryWriteBreakPoint(addr.Segment, addr.Offset, length));
        }

        [DebuggerHelp("Set a memory read break point")]
        public void bp_memr(DebuggerCore debugger, FarPointer addr, ushort length)
        {
            debugger.AddBreakPoint(new MemoryReadBreakPoint(addr.Segment, addr.Offset, length));
        }

        [DebuggerHelp("Set a expression change break point")]
        public void bp_expr(DebuggerCore debugger, Expression expr)
        {
            debugger.AddBreakPoint(new ExpressionBreakPoint(expr));
        }

        [DebuggerHelp("Set a CPU time break point")]
        public void bp_cputime(DebuggerCore debugger, ulong time)
        {
            debugger.AddBreakPoint(new CpuTimeBreakPoint(time));
        }

        [DebuggerHelp("Set a interrupt break point")]
        public void bp_int(DebuggerCore debugger, byte number)
        {
            debugger.AddBreakPoint(new InterruptBreakPoint(number));
        }

        [DebuggerHelp("Attach a match condition to a break point")]
        public void bp_match(DebuggerCore debugger, BreakPoint bp, Expression expr = null)
        {
            debugger.SetBreakPointMatchCondition(bp, expr);
        }

        [DebuggerHelp("Attach a break condition to a break point")]
        public void bp_break(DebuggerCore debugger, BreakPoint bp, Expression expr = null)
        {
            debugger.SetBreakPointBreakCondition(bp, expr);
        }

        [DebuggerHelp("Edit a break point")]
        public void bp_edit(DebuggerCore debugger, BreakPoint bp, [ArgTail] string argtail = null)
        {
            if (string.IsNullOrWhiteSpace(argtail))
            {
                debugger.PromptConsole(string.Format("bp edit {0} {1}", bp.Number, bp.EditString));
            }
            else
            {
                try
                {
                    debugger.EditBreakPoint(bp);
                    debugger.CommandDispatcher.ExecuteCommand("bp " + argtail);
                }
                finally
                {
                    debugger.EditBreakPoint(null);
                }
            }
        }


        [DebuggerHelp("Run")]
        public void r(DebuggerCore debugger)
        {
            debugger.Continue();
        }

        [DebuggerHelp("Step")]
        public void s(DebuggerCore debugger)
        {
            debugger.Break();
            debugger.Continue();
        }

        [DebuggerHelp("Step Over")]
        public void o(DebuggerCore debugger)
        {
            debugger.BreakAfterCall();
            debugger.Continue();
        }

        [DebuggerHelp("Step Out")]
        public void t(DebuggerCore debugger)
        {
            debugger.BreakOnLeaveRoutine();
            debugger.Continue();
        }

        [DebuggerHelp("Run to address")]
        public void r_to(DebuggerCore debugger, FarPointer addr)
        {
            debugger.BreakAt(addr.Segment, addr.Offset);
            debugger.Continue();
        }

        [DebuggerHelp("Run to CPU time")]
        public void r_time(DebuggerCore debugger, ulong counter)
        {
            if (counter > debugger.CPU.CpuTime)
            {
                debugger.BreakAtTemp(new CpuTimeBreakPoint(counter));
                debugger.Continue();
            }
            else
            {
                debugger.WriteLine("{0} is in the past!", counter);
            }
        }

        [DebuggerHelp("Run for CPU cycles")]
        public void r_for(DebuggerCore debugger, ulong instructions)
        {
            debugger.BreakAtTemp(new CpuTimeBreakPoint(debugger.CPU.CpuTime + instructions));
            debugger.Continue();
        }

        [DebuggerHelp("Evaluate an expression")]
        public void e(DebuggerCore debugger, Expression expr)
        {
            try
            {
                debugger.WriteLine(debugger.ExpressionContext.EvalAndFormat(expr));
            }
            catch (Exception x)
            {
                debugger.WriteLine("Error: {0}", x.Message);
            }
        }

        [DebuggerHelp("Add a watch expression")]
        public void w(DebuggerCore debugger, Expression expr, string name = "")
        {
            debugger.AddWatchExpression(new WatchExpression(expr) { Name = name });
        }

        [DebuggerHelp("Remove a watch expression")]
        public void w_del(DebuggerCore debugger, int number)
        {
            var bp = debugger.WatchExpressions.FirstOrDefault(x => x.Number == number);
            if (bp != null)
            {
                debugger.RemoveWatchExpression(bp);
            }
            else
            {
                debugger.WriteLine("Watch expression #{0} doesn't exist", number);
            }
        }

        [DebuggerHelp("Remove all watch expression")]
        public void w_clear(DebuggerCore debugger)
        {
            debugger.RemoveAllWatchExpressions();
        }

        [DebuggerHelp("Edit a watch point")]
        public void w_edit(DebuggerCore debugger, int number, Expression expression = null)
        {
            var w = debugger.WatchExpressions.FirstOrDefault(x => x.Number == number);
            if (w == null)
            {
                debugger.WriteLine("Watch expression #{0} doesn't exist", number);
                return;
            }

            if (expression == null)
            {
                debugger.PromptConsole(string.Format("w edit {0} {1}", w.Number, w.ExpressionText));
            }
            else
            {
                debugger.EditWatchExpression(w, expression);
            }
        }

        [DebuggerHelp("Disassemble (format=b,w,dw,i,l)")]
        public void disasm(DebuggerCore debugger, FarPointer addr, ushort length = 16)
        {
            var dis = new Disassembler(debugger.CPU);
            dis.cs = addr.Segment;
            dis.ip = addr.Offset;
            while (dis.ip < addr.Offset + length)
            {
                debugger.WriteLine("{0:X4}:{1:X4} {2}", dis.cs, dis.ip, dis.Read());
            }
        }

        [DebuggerHelp("Dump memory (format=b,w,dw,i,l)")]
        public void dump_mem(DebuggerCore debugger, FarPointer addr, ushort length = 16, string format = "b")
        {
            try
            {
                ushort seg = addr.Segment;
                ushort ofs = addr.Offset;
                switch (format)
                {
                    case "b":
                        {
                            char[] chBuf = new char[16];
                            for (int i = 0; i < length; i++)
                            {
                                if ((i % 16 == 0))
                                {
                                    if (i > 0)
                                    {
                                        debugger.Write("|");
                                        debugger.Write(new string(chBuf));
                                        debugger.WriteLine("|");
                                    }
                                    debugger.Write(string.Format("{0:X4}:{1:X4} ", seg, ofs + i));
                                }

                                byte b = debugger.CPU.MemoryBus.ReadByte(seg, (ushort)(ofs + i));
                                chBuf[i % 16] = (b >= 32 && b < 128) ? (char)b : ' ';
                                debugger.Write(string.Format("{0:X2} ", b));
                            }

                            if ((length % 16) != 0)
                                debugger.Write(new string(' ', (16 - length % 16) * 3));
                            debugger.Write("|");
                            debugger.Write(new string(chBuf, 0, (length % 16) == 0 ? 16 : length % 16));
                            debugger.WriteLine("|");
                            break;
                        }

                    case "w":
                        for (int i = 0; i < length; i++)
                        {
                            if ((i % 8 == 0))
                            {
                                if (i > 0)
                                {
                                    debugger.WriteLine();
                                }
                                debugger.Write(string.Format("{0:X4}:{1:X4} ", seg, ofs + i * 2));
                            }

                            var val = debugger.CPU.MemoryBus.ReadWord(seg, (ushort)(ofs + i * 2));
                            debugger.Write(string.Format("{0:X4} ", val));
                        }

                        debugger.WriteLine();
                        break;

                    case "dw":
                        for (int i = 0; i < length; i++)
                        {
                            if ((i % 4 == 0))
                            {
                                if (i > 0)
                                {
                                    debugger.WriteLine();
                                }
                                debugger.Write(string.Format("{0:X4}:{1:X4} ", seg, ofs + i * 4));
                            }

                            var val = debugger.CPU.MemoryBus.ReadDWord(seg, (ushort)(ofs + i * 4));
                            debugger.Write(string.Format("{0:X8} ", val));
                        }

                        debugger.WriteLine();
                        break;

                    case "i":
                        for (int i = 0; i < length; i++)
                        {
                            if ((i % 8 == 0))
                            {
                                if (i > 0)
                                {
                                    debugger.WriteLine();
                                }
                                debugger.Write(string.Format("{0:X4}:{1:X4} ", seg, ofs + i * 2));
                            }

                            var val = debugger.CPU.MemoryBus.ReadWord(seg, (ushort)(ofs + i * 2));
                            debugger.Write(string.Format("{0,6} ", unchecked((short)val)));
                        }

                        debugger.WriteLine();
                        break;

                    case "l":
                        for (int i = 0; i < length; i++)
                        {
                            if ((i % 4 == 0))
                            {
                                if (i > 0)
                                {
                                    debugger.WriteLine();
                                }
                                debugger.Write(string.Format("{0:X4}:{1:X4} ", seg, ofs + i * 4));
                            }

                            var val = debugger.CPU.MemoryBus.ReadDWord(seg, (ushort)(ofs + i * 4));
                            debugger.Write(string.Format("{0,11} ", unchecked((int)val)));
                        }

                        debugger.WriteLine();
                        break;
                }
            }
            catch (CPUException)
            {
                debugger.WriteLine("#err");
            }
        }

        [DebuggerHelp("Dump execution trace")]
        public void trace(DebuggerCore debugger, int length = 0)
        {
            debugger.DumpTraceBuffer(length == 0 ? debugger.TraceBufferSize : length);
        }

        [DebuggerHelp("Clear execution trace")]
        public void trace_clear(DebuggerCore debugger)
        {
            debugger.ClearTraceBuffer();
        }

        [DebuggerHelp("Set the execution trace buffer size")]
        public void trace_set(DebuggerCore debugger, int size = -1)
        {
            if (size > 0)
            {
                debugger.TraceBufferSize = size;
            }


            debugger.WriteLine("Trace buffer size = {0}, tracing {1}", debugger.TraceBufferSize, debugger.EnableTrace ? "on" : "off");
            return;
        }

        [DebuggerHelp("Enable execution tracing")]
        public void trace_on(DebuggerCore debugger)
        {
            debugger.EnableTrace = true;
            trace_set(debugger);
        }

        [DebuggerHelp("Disable execution tracing")]
        public void trace_off(DebuggerCore debugger)
        {
            debugger.EnableTrace = false;
            trace_set(debugger);
        }

        TextWriter _logger;

        [DebuggerHelp("Log output to a file")]
        public void log(DebuggerCore debugger, string filename)
        {
            if (_logger != null)
            {
                _logger.Dispose();
                _logger = null;
            }
            _logger = new StreamWriter(filename, false, Encoding.UTF8);
            debugger.WriteLine("Logging to file {0}", filename);
            debugger.Redirect(_logger);
        }

        [DebuggerHelp("Close log file")]
        public void close_log(DebuggerCore debugger)
        {
            if (_logger != null)
            {
                debugger.Redirect(null);
                _logger.Dispose();
            }
            debugger.WriteLine("Logger closed");
        }
    }

}


