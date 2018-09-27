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
using System.Reflection;
using System.Text;
using PetaJson;

namespace Sharp86
{
    public class DebuggerCore : IDebugger, IMemoryBus
    {
        public DebuggerCore()
        {
            BreakPoint.RegisterJson();
            CommandDispatcher = new CommandDispatcher(this);

        }

        public readonly CommandDispatcher CommandDispatcher;

        public CPU CPU
        {
            get
            {
                return _cpu;
            }
            set
            {
                if (_cpu != null)
                {
                    _cpu.Debugger = null;
                }

                _cpu = value;
                _disassembler = new Disassembler(_cpu);

                if (_cpu != null)
                {
                    _cpu.Debugger = this;
                }

                PrepareBreakPoints();
            }
        }

        public IMemoryBus MemoryBus
        {
            get { return _cpu.MemoryBus; }
        }

        CPU _cpu;
        ExpressionContext _expressionContext;

        public ExpressionContext ExpressionContext
        {
            get
            {
                if (_expressionContext == null)
                    _expressionContext = new ExpressionContext(_cpu);
                return _expressionContext;
            }
            set
            {
                _expressionContext = value;
            }
        }


        public virtual string FormatTrace()
        {
            var disasm = _disassembler.Read(_cpu.cs, _cpu.ip);
            return string.Format("{0,12} {1:X4}:{2:X4} {3,-30} {4}",
                _cpu.CpuTime, _cpu.cs, _cpu.ip,
                disasm, ExpressionContext.GenerateDisassemblyAnnotations(disasm, _disassembler.ImplicitParams));
        }

        HistoryBuffer<string> _traceBuffer = null;

        bool IDebugger.OnStep()
        {
            // Capture trace history
            if (_traceBuffer != null)
            {
                _traceBuffer.Write(FormatTrace());
            }

            // Test all break points
            for (int i = 0; i < _allBreakPoints.Count; i++)
            {
                var bp = _allBreakPoints[i];
                try
                {
                    if (bp.Enabled && bp.ShouldBreak(this) && bp.CheckMatchConditions(this))
                    {
                        bp.TripCount++;
                        if (bp.CheckBreakConditions(this))
                        {
                            if (!(bp is CodeBreakPoint) || bp.MatchConditionExpression != null)
                                WriteLine("Break point {0}", _allBreakPoints[i]);
                            _break = true;
                        }
                    }
                }
                catch (Exception x)
                {
                    WriteLine("Exception while evaluating breakpoint #{0} - {1}", bp.Number, x.Message);
                    _break = true;
                }

            }

            if (_tempBreakPoint != null)
            {
                if (_tempBreakPoint.ShouldBreak(this))
                {
                    if (!(_tempBreakPoint is CodeBreakPoint))
                        WriteLine("Break point {0}", _tempBreakPoint);
                    _break = true;
                }
            }

            DebuggerBreak();

            // Carry on
            return true;
        }

        bool IDebugger.OnSoftwareInterrupt(byte interruptNumber)
        {
            bool _breakWas = _break;

            _break = false;

            // Always stop on interrupt 3
            if (interruptNumber == 3)
            {
                _break = true;
            }

            // Test all interrupt break points
            for (int i = 0; i < _allBreakPoints.Count; i++)
            {
                var bp = _allBreakPoints[i] as InterruptBreakPoint;
                if (bp == null)
                    continue;

                try
                {
                    if (bp.Enabled && bp.InterruptNumber == interruptNumber && bp.CheckMatchConditions(this))
                    {
                        bp.TripCount++;
                        if (bp.CheckBreakConditions(this))
                        {
                            WriteLine("Break point {0}", bp);
                            _break = true;
                        }
                    }
                }
                catch (Exception x)
                {
                    WriteLine("Exception while evaluating breakpoint #{0} - {1}", bp.Number, x.Message);
                    _break = true;
                }
            }

            if (_break)
            {
                DebuggerBreak();
                return false;
            }
            else
            {
                _break = _breakWas;
            }

            // Process normally
            return true;
        }

        void DebuggerBreak()
        {
            // Break!
            if (_break)
            {
                // Clear the continue flag
                _shouldContinue = false;

                // Clear the break flag
                _break = false;

                // Clear the temp break point
                _tempBreakPoint = null;

                // Break!
                try
                {
                    _isStopped = true;

                    if (PendingCommands)
                    {
                        CommandDispatcher.ExecuteQueuedCommands();
                    }

                    if (!_shouldContinue)
                    {
                        OnBreak();
                    }
                }
                finally
                {
                    _isStopped = false;
                }

                _shouldContinue = false;
            }
        }

        internal bool PendingCommands;

        bool _isStopped = false;
        public bool IsStopped
        {
            get { return _isStopped; }
        }

        // Called by commands that want execution to continue after
        // they return (eg: step, step over, run etc...)
        public void Continue()
        {
            _shouldContinue = true;
        }

        bool _shouldContinue = false;
        public bool ShouldContinue
        {
            get
            {
                return _shouldContinue;
            }
        }

        public bool IsStepping
        {
            get
            {
                return _break;
            }
        }

        // Break on the next instruction (step into, or break while running)
        public void Break()
        {
            _break = true;
        }

        Disassembler _disassembler;

        // Break after the call (ie: step ever)
        public void BreakAfterCall()
        {
            // Check if the current instruction is a call instruction
            var instruction = _disassembler.Read(_cpu.cs, _cpu.ip);
            if (_disassembler.IsCall)
            {
                _tempBreakPoint = new CodeBreakPoint(_disassembler.cs, _disassembler.ip);
            }
            else
            {
                // Otherwise, just step one instruction
                _break = true;
            }
        }

        // Break when leaving this function (ie: step out)
        public void BreakOnLeaveRoutine()
        {
            _tempBreakPoint = new StepOutBreakPoint(_cpu);
        }

        public void BreakAtTemp(BreakPoint bp)
        {
            _tempBreakPoint = bp;
        }

        public void BreakAt(ushort csStop, ushort ipStop)
        {
            BreakAtTemp(new CodeBreakPoint(csStop, ipStop));
        }

        bool _break;

        protected virtual void PrepareBreakPoints()
        {
            if (_cpu == null)
                return;

            _memWriteBreakPoints = _allBreakPoints.Where(x => x.Enabled).OfType<IBreakPointMemWrite>().ToList();
            _memReadBreakPoints = _allBreakPoints.Where(x => x.Enabled).OfType<IBreakPointMemRead>().ToList();

            // Hook/unhook the CPU's memory bus
            if (_memReadBreakPoints.Count > 0 || _memWriteBreakPoints.Count>0)
            {
                if (_cpu.ActiveMemoryBus != this)
                {
                    _cpu.ActiveMemoryBus = this;
                }
            }
            else
            {
                if (_cpu.ActiveMemoryBus == this)
                {
                    _cpu.ActiveMemoryBus = _cpu.MemoryBus;
                }
            }
        }

        #region IBus

        bool IMemoryBus.IsExecutableSelector(ushort seg)
        {
            return _cpu.MemoryBus.IsExecutableSelector(seg);
        }

        byte IMemoryBus.ReadByte(ushort seg, ushort offset)
        {
            byte b = _cpu.MemoryBus.ReadByte(seg, offset);

            for (int i = _memReadBreakPoints.Count - 1; i >= 0; i--)
            {
                _memReadBreakPoints[i].ReadByte(seg, offset, b);
            }

            return b;
        }

        void IMemoryBus.WriteByte(ushort seg, ushort offset, byte value)
        {
            // Read the old value
            byte oldValue = 0;
            if (_memWriteBreakPoints.Count > 0)
            {
                try
                {
                    // Capture the old value
                    oldValue = _cpu.MemoryBus.ReadByte(seg, offset);
                }
                catch
                {
                    // Ignore
                }
            }

            // Notify memory write break points
            if (_memWriteBreakPoints.Count > 0)
            {
                for (int i=_memWriteBreakPoints.Count-1; i>=0; i--)
                {
                    _memWriteBreakPoints[i].WriteByte(seg, offset, oldValue, value);
                }
            }

            // Do the write
            _cpu.MemoryBus.WriteByte(seg, offset, value);
        }

        #endregion

        public CodeBreakPoint FindCodeBreakPoint(ushort cs, ushort ip)
        {
            return _allBreakPoints.OfType<CodeBreakPoint>().FirstOrDefault(x => x.Segment == cs && x.Offset == ip);
        }

        public bool ToggleCodeBreakPoint(ushort cs, ushort ip)
        {
            var existing = FindCodeBreakPoint(cs, ip);
            if (existing!=null)
            {
                RemoveBreakPoint(existing);
                return false;
            }
            else
            {
                AddBreakPoint(new CodeBreakPoint(cs, ip));
                return true;
            }
        }

        public void AddBreakPoint(BreakPoint bp)
        {
            if (_editingBreakPoint!=null)
            {
                // Copy state (number, condition, trip count etc...)
                bp.CopyState(_editingBreakPoint);

                // Replace it
                var index = _allBreakPoints.IndexOf(_editingBreakPoint);
                _allBreakPoints[index] = bp;

                PrepareBreakPoints();

                // Save
                OnSettingsChanged();
                WriteLine("Edited {0}", bp);
                return;
            }

            if (_allBreakPoints.Count>0)
            {
                bp.Number = _allBreakPoints.Max(x => x.Number) + 1;
            }
            else
            {
                bp.Number = 1;
            }

            BreakPoints.Add(bp);

            PrepareBreakPoints();
            OnSettingsChanged();

            WriteLine("Added {0}", bp.ToString());
        }

        BreakPoint _editingBreakPoint;
        public void EditBreakPoint(BreakPoint bp)
        {
            _editingBreakPoint = bp;
        }

        public void RemoveBreakPoint(BreakPoint bp)
        {
            _allBreakPoints.Remove(bp);
            PrepareBreakPoints();
            OnSettingsChanged();
            WriteLine("Removed {0}", bp);
        }

        public void RemoveAllBreakpoints()
        {
            _allBreakPoints.Clear();
            PrepareBreakPoints();
            OnSettingsChanged();
            WriteLine("Cleared all breakpoints");
        }

        public void EnableBreakPoint(BreakPoint bp, bool enable)
        {
            bp.Enabled = enable;
            PrepareBreakPoints();
            OnSettingsChanged();
            WriteLine("{0} - {1}", enable ? "Enabled" : "Disabled", bp.ToString());
        }

        public void SetBreakPointMatchCondition(BreakPoint bp, Expression condition)
        {
            bp.MatchConditionExpression = condition;
            OnSettingsChanged();
            WriteLine(bp.ToString());
        }

        public void SetBreakPointBreakCondition(BreakPoint bp, Expression condition)
        {
            bp.BreakConditionExpression = condition;
            OnSettingsChanged();
            WriteLine(bp.ToString());
        }

        public void AddWatchExpression(WatchExpression w)
        {
            if (_allWatchExpressions.Count>0)
            {
                w.Number = _allWatchExpressions.Max(x => x.Number) + 1;
            }
            else
            {
                w.Number = 1;
            }

            WatchExpressions.Add(w);

            OnSettingsChanged();

            WriteLine("Added {0}", w.ToString());
        }

        public void EditWatchExpression(WatchExpression w, Expression expression)
        {
            w.Expression = expression;
            OnSettingsChanged();
            WriteLine("Edited {0}", w.ToString());
        }



        public void RemoveWatchExpression(WatchExpression bp)
        {
            _allWatchExpressions.Remove(bp);
            OnSettingsChanged();
            WriteLine("Removed {0}", bp);
        }

        public void RemoveAllWatchExpressions()
        {
            _allWatchExpressions.Clear();
            OnSettingsChanged();
            WriteLine("Cleared all Watch Expressions");
        }


        string _settingsFile;
        public string SettingsFile
        {
            get { return _settingsFile; }
            set
            {
                // Save it
                if (value != null)
                    _settingsFile = System.IO.Path.GetFullPath(value);
                else
                    _settingsFile = null;

                // Load it specified
                if (_settingsFile != null && System.IO.File.Exists(_settingsFile))
                {
                    Json.ParseFileInto(_settingsFile, this);
                    CreateTraceBuffer();
                }
            }
        }

        public event Action SettingsChanged;

        void OnSettingsChanged()
        {
            if (SettingsChanged != null)
                SettingsChanged.Invoke();

            if (_settingsFile != null)
                Json.WriteFile(_settingsFile, this);
        }

        [Json("watchExpressions")]
        public List<WatchExpression> WatchExpressions   
        {
            get { return _allWatchExpressions; }
            set { _allWatchExpressions = value; }
        }

        [Json("breakpoints")]
        public List<BreakPoint> BreakPoints
        {
            get
            {
                return _allBreakPoints;
            }
            set
            {
                _allBreakPoints.Clear();
                _allBreakPoints.AddRange(value);
                PrepareBreakPoints();
            }
        }

        bool _breakImmediately;
        [Json("breakImmediately")]
        public bool BreakImmediately
        {
            get
            {
                return _breakImmediately;
            }
            set
            {
                _breakImmediately = value;

                if (_breakImmediately)
                    Break();
            }
        }

        List<WatchExpression> _allWatchExpressions = new List<WatchExpression>();
        List<BreakPoint> _allBreakPoints = new List<BreakPoint>();
        List<IBreakPointMemRead> _memReadBreakPoints;
        List<IBreakPointMemWrite> _memWriteBreakPoints;
        BreakPoint _tempBreakPoint;

        // Code break - return true to continue exection, or false to return from 
        // CPU step() to caller...
        // BreakPoint can be null if user initiated break
        protected virtual bool OnBreak()
        {
            return true;
        }

        TextWriter _redirect;

        public void Redirect(TextWriter tw)
        {
            _redirect = tw;
        }

        public virtual void WriteConsole(string output)
        {
        }

        public virtual void PromptConsole(string str)
        {
        }

        public void Write(string output)
        {
            if (_redirect != null)
                _redirect.Write(output);
            else
                WriteConsole(output);
        }

        public void WriteLine(string output = "")
        {
            Write(output);
            Write("\n");
        }

        public void WriteLine(string fmt, params object[] parms)
        {
            WriteLine(string.Format(fmt, parms));
        }

        void CreateTraceBuffer()
        {
            int newSize = _enableTrace ? _traceBufferSize : 0;

            if (newSize == 0)
            {                                               
                _traceBuffer = null;
            }
            else
            {
                _traceBuffer = new HistoryBuffer<string>(newSize);
            }
        }

        [Json("enableTrace")]
        bool _enableTrace = false;
        public bool EnableTrace
        {
            get { return _enableTrace; }
            set
            {
                if (_enableTrace != value)
                {
                    _enableTrace = value;
                    CreateTraceBuffer();
                    OnSettingsChanged();
                }
            }
        }

        [Json("traceBufferSize")]
        int _traceBufferSize = 1000;
        public int TraceBufferSize
        {
            get { return _traceBufferSize; }
            set
            {
                if (_traceBufferSize != value)
                {
                    _traceBufferSize = value;
                    CreateTraceBuffer();
                    OnSettingsChanged();
                }
            }
        }

        public void DumpTraceBuffer(int items)
        {
            if (_traceBuffer == null)
            {
                WriteLine("Trace buffer disabled");
                return;
            }
            if (items > _traceBuffer.Count)
                items = _traceBuffer.Count;

            for (int i=_traceBuffer.Count - items; i<_traceBuffer.Count; i++)
            {
                WriteLine(_traceBuffer[i]);
            }
        }

        public void ClearTraceBuffer()
        {
            _traceBuffer.Clear();
        }


    }
}
