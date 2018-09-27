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
using System.Runtime.CompilerServices;

namespace Sharp86
{
    public enum Reg8
    {
        AL,
        CL,
        DL,
        BL,
        AH,
        CH,
        DH,
        BH,
    }

    public enum Reg16
    {
        AX,
        CX,
        DX,
        BX,
        SP,
        BP,
        SI,
        DI,
    }

    public enum RegSeg
    {
        None = -1,
        ES,
        CS,
        SS,
        DS,
    }

    public class CPUException : Exception
    {
        public CPUException(int interruptNo, string message = null) : base(message)
        {
            RestoreIP = true;
            InterruptNo = interruptNo;
        }
        public bool RestoreIP;
        public int InterruptNo;
        public ushort cs;
        public ushort ip;
        public bool ipSet;

        public override string Message
        {
            get
            {
                return string.Format("{0} at {1:X4}:{2:X4}", this.GetType().Name, cs, ip);
            }
        }
    }

    public class InvalidOpCodeException : CPUException
    {
        public InvalidOpCodeException() : base(6)
        {
        }
    }

    public class SegmentNotPresentException : CPUException
    {
        public SegmentNotPresentException(ushort seg) : base(11)
        {
            _seg = seg;
        }

        ushort _seg;

        public override string Message
        {
            get
            {
                return string.Format("Segment not preset fault at {0:X4}:{1:X4} segment 0x{2:X4} ", cs, ip, _seg);
            }
        }
    }

    public class GeneralProtectionFaultException : CPUException
    {
        public GeneralProtectionFaultException(ushort seg, ushort offset, bool read) : base(13)
        {
            _seg = seg;
            _offset = offset;
            _read = read;
        }
        ushort _seg;
        ushort _offset;
        bool _read;

        public override string Message
        {
            get
            {
                return string.Format("General protection fault at {1:X4}:{2:X4} {3} address {4:X4}:{5:X4}", this.GetType().Name, 
                    cs, ip, _read ? "reading" : "writing", _seg, _offset);
            }
        }
    }

    public class NonExecutableSegmentException : CPUException
    {
        public NonExecutableSegmentException() : base(13, "Attempt to execute code from a non-executable segment")
        {
        }
    }

    public abstract class CPU : ALU
    {
        public CPU()
        {
        }

        public virtual void Reset()
        {
            ax = 0;
            bx = 0;
            cx = 0;
            dx = 0;
            si = 0;
            di = 0;
            sp = 0;
            bp = 0;
            cs = 0;
            ds = 0;
            ss = 0;
            es = 0;
            EFlags = 0;
        }

        IMemoryBus _memoryBus;
        public IMemoryBus MemoryBus
        {
            get { return _memoryBus; }
            set
            {
                System.Diagnostics.Debug.Assert(_activeMemoryBus == null);
                _memoryBus = value;
                _activeMemoryBus = value;
            }
        }

        IMemoryBus _activeMemoryBus;
        public IMemoryBus ActiveMemoryBus
        {
            get { return _activeMemoryBus; }
            set { _activeMemoryBus = value; }
        }

        IPortBus _portBus;
        public IPortBus PortBus
        {
            get { return _portBus; }
            set { _portBus = value; }
        }

        #region Segment Registers
        public ushort ss;
        public ushort cs
        {
            get { return _cs; }
            set
            {
                // Verify code selector
                if (_activeMemoryBus != null && !_activeMemoryBus.IsExecutableSelector(value))
                    throw new NonExecutableSegmentException();

                // Store it 
                _cs = value;
            }
        }
        ushort _cs;
        public ushort es;
        public ushort ds;
        public ushort ReadReg(RegSeg reg)
        {
            switch (reg)
            {
                case RegSeg.ES: return es;
                case RegSeg.DS: return ds;
                case RegSeg.SS: return ss;
                case RegSeg.CS: return cs;
            }
            throw new ArgumentException("Invalid register index");
        }
        public void WriteReg(RegSeg reg, ushort value)
        {
            switch (reg)
            {
                case RegSeg.ES: es = value; return;
                case RegSeg.DS: ds = value; return;
                case RegSeg.SS: ss = value; return;
                case RegSeg.CS: cs = value; return;
            }
            throw new ArgumentException("Invalid register index");
        }
        #endregion

        #region 16-bit registers
        public uint dxax
        {
            get
            {
                return (uint)(dx << 16 | ax);
            }
            set
            {
                dx = (ushort)(value >> 16);
                ax = (ushort)(value & 0xFFFF);
            }
        }
        public ushort ax;
        public ushort bx;
        public ushort cx;
        public ushort dx;
        public ushort si;
        public ushort di;
        public ushort ip;
        public ushort sp;
        public ushort bp;
        public ushort ReadReg(Reg16 reg)
        {
            switch (reg)
            {
                case Reg16.AX: return ax;
                case Reg16.BX: return bx;
                case Reg16.CX: return cx;
                case Reg16.DX: return dx;
                case Reg16.DI: return di;
                case Reg16.SI: return si;
                case Reg16.SP: return sp;
                case Reg16.BP: return bp;
            }
            throw new ArgumentException("Invalid register index");
        }
        public void WriteReg(Reg16 reg, ushort value)
        {
            switch (reg)
            {
                case Reg16.AX: ax = value; return;
                case Reg16.BX: bx = value; return;
                case Reg16.CX: cx = value; return;
                case Reg16.DX: dx = value; return;
                case Reg16.DI: di = value; return;
                case Reg16.SI: si = value; return;
                case Reg16.SP: sp = value; return;
                case Reg16.BP: bp = value; return;
            }
            throw new ArgumentException("Invalid register index");
        }
        #endregion

        #region 8-bit registers
        public byte al
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(ax & 0xFF); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { ax = (ushort)((ax & 0xFF00) | (value & 0x00FF)); }
        }

        public byte ah
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(ax >> 8); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { ax = (ushort)((ax & 0x00FF) | ((value & 0x00FF) << 8)); }
        }

        public byte bl
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(bx & 0xFF); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { bx = (ushort)((bx & 0xFF00) | (value & 0x00FF)); }
        }

        public byte bh
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(bx >> 8); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { bx = (ushort)((bx & 0x00FF) | ((value & 0x00FF) << 8)); }
        }

        public byte cl
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(cx & 0xFF); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { cx = (ushort)((cx & 0xFF00) | (value & 0x00FF)); }
        }

        public byte ch
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(cx >> 8); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { cx = (ushort)((cx & 0x00FF) | ((value & 0x00FF) << 8)); }
        }

        public byte dl
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(dx & 0xFF); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { dx = (ushort)((dx & 0xFF00) | (value & 0x00FF)); }
        }

        public byte dh
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(dx >> 8); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { dx = (ushort)((dx & 0x00FF) | ((value & 0x00FF) << 8)); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadReg(Reg8 reg)
        {
            switch (reg)
            {
                case Reg8.AL: return (byte)(ax & 0xFF);
                case Reg8.AH: return (byte)(ax >> 8);
                case Reg8.BL: return (byte)(bx & 0xFF);
                case Reg8.BH: return (byte)(bx >> 8);
                case Reg8.CL: return (byte)(cx & 0xFF);
                case Reg8.CH: return (byte)(cx >> 8);
                case Reg8.DL: return (byte)(dx & 0xFF);
                case Reg8.DH: return (byte)(dx >> 8);
            }
            throw new ArgumentException("Invalid register index");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteReg(Reg8 reg, byte value)
        {
            switch (reg)
            {
                case Reg8.AL: ax = (ushort)((ax & 0xFF00) | (value & 0x00FF)); return;
                case Reg8.AH: ax = (ushort)((ax & 0x00FF) | ((value & 0x00FF) << 8)); return;
                case Reg8.BL: bx = (ushort)((bx & 0xFF00) | (value & 0x00FF)); return;
                case Reg8.BH: bx = (ushort)((bx & 0x00FF) | ((value & 0x00FF) << 8)); return;
                case Reg8.CL: cx = (ushort)((cx & 0xFF00) | (value & 0x00FF)); return;
                case Reg8.CH: cx = (ushort)((cx & 0x00FF) | ((value & 0x00FF) << 8)); return;
                case Reg8.DL: dx = (ushort)((dx & 0xFF00) | (value & 0x00FF)); return;
                case Reg8.DH: dx = (ushort)((dx & 0x00FF) | ((value & 0x00FF) << 8)); return;
            }
            throw new ArgumentException("Invalid register index");
        }
        #endregion

        #region Debugger
        IDebugger _debugger;
        public IDebugger Debugger
        {
            get
            {
                return _debugger;
            }
            set
            {
                _debugger = value;
            }
        }
        #endregion

        #region Instruction Execution

        // Decode prefixes
        //bool _prefixLock;
        bool _prefixRepEither;
        bool _prefixRepNE;
        RegSeg _prefixSegment;
        bool _halt;
        bool _haveReadModRM;
        byte _modRM;
        ushort _modRMSeg;
        ushort _modRMOffset;
        bool _modRMIsPointer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReadModRM()
        {
            System.Diagnostics.Debug.Assert(!_haveReadModRM);

            // Remember we've read it
            _haveReadModRM = true;

            // Read the mod RM byte
            _modRM = _activeMemoryBus.ReadByte(cs, ip++);
            _modRMIsPointer = true;

            ushort displacement;

            // Read displacement
            switch (_modRM & 0xC0)
            {
                case 0x00:
                    // Mode 0
                    switch (_modRM & 0x7)
                    {
                        case 0:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(bx + si);
                            break;

                        case 1:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(bx + di);
                            break;

                        case 2:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = (ushort)(bp + si);
                            break;

                        case 3:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = (ushort)(bp + di);
                            break;

                        case 4:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(si);
                            break;

                        case 5:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(di);
                            break;

                        case 6:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = _activeMemoryBus.ReadWord(cs, ip);
                            ip += 2;
                            break;

                        case 7:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = bx;
                            break;
                    }
                    break;

                case 0x40:

                    // Mode 1 (1 byte displacement)
                    displacement = (ushort)(sbyte)_activeMemoryBus.ReadByte(cs, ip++);

                    // Mode 1
                    switch (_modRM & 0x7)
                    {
                        case 0:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(bx + si + displacement);
                            break;

                        case 1:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(bx + di + displacement);
                            break;

                        case 2:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = (ushort)(bp + si + displacement);
                            break;

                        case 3:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = (ushort)(bp + di + displacement);
                            break;

                        case 4:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(si + displacement);
                            break;

                        case 5:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(di + displacement);
                            break;

                        case 6:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = (ushort)(bp + displacement);
                            break;

                        case 7:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(bx + displacement);
                            break;
                    }
                    break;

                case 0x80:
                    // Mode 2 (2 byte displacement)
                    displacement = _activeMemoryBus.ReadWord(cs, ip);
                    ip += 2;

                    // Mode 1
                    switch (_modRM & 0x7)
                    {
                        case 0:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(bx + si + displacement);
                            break;

                        case 1:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(bx + di + displacement);
                            break;

                        case 2:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = (ushort)(bp + si + displacement);
                            break;

                        case 3:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = (ushort)(bp + di + displacement);
                            break;

                        case 4:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(si + displacement);
                            break;

                        case 5:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(di + displacement);
                            break;

                        case 6:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = (ushort)(bp + displacement);
                            break;

                        case 7:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = (ushort)(bx + displacement);
                            break;
                    }
                    break;

                case 0xC0:
                    // Mode 3 (Register)
                    _modRMIsPointer = false;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte Read_Eb()
        {
            if (!_haveReadModRM)
                ReadModRM();

            if (_modRMIsPointer)
            {
                return _activeMemoryBus.ReadByte(_modRMSeg, _modRMOffset);
            }
            else
            {
                return ReadReg((Reg8)(_modRM & 0x07));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ushort Read_Ev()
        {
            if (!_haveReadModRM)
                ReadModRM();

            if (_modRMIsPointer)
            {
                return _activeMemoryBus.ReadWord(_modRMSeg, _modRMOffset);
            }
            else
            {
                return ReadReg((Reg16)(_modRM & 0x07));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Write_Eb(byte value)
        {
            if (!_haveReadModRM)
                ReadModRM();

            if (_modRMIsPointer)
            {
                _activeMemoryBus.WriteByte(_modRMSeg, _modRMOffset, value);
            }
            else
            {
                WriteReg((Reg8)(_modRM & 0x07), value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Write_Ev(ushort value)
        {
            if (!_haveReadModRM)
                ReadModRM();

            if (_modRMIsPointer)
            {
                _activeMemoryBus.WriteWord(_modRMSeg, _modRMOffset, value);
            }
            else
            {
                WriteReg((Reg16)(_modRM & 0x07), value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte Read_Gb()
        {
            if (!_haveReadModRM)
                ReadModRM();

            return ReadReg((Reg8)((_modRM >> 3) & 0x07));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ushort Read_Gv()
        {
            if (!_haveReadModRM)
                ReadModRM();

            return ReadReg((Reg16)((_modRM >> 3) & 0x07));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Write_Gb(byte value)
        {
            if (!_haveReadModRM)
                ReadModRM();

            WriteReg((Reg8)((_modRM >> 3) & 0x07), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Write_Gv(ushort value)
        {
            if (!_haveReadModRM)
                ReadModRM();

            WriteReg((Reg16)((_modRM >> 3) & 0x07), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ushort Read_Sv()
        {
            if (!_haveReadModRM)
                ReadModRM();

            return ReadReg((RegSeg)((_modRM >> 3) & 0x07));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Write_Sv(ushort value)
        {
            if (!_haveReadModRM)
                ReadModRM();

            WriteReg((RegSeg)((_modRM >> 3) & 0x07), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte Read_Ib()
        {
            return _activeMemoryBus.ReadByte(cs, ip++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ushort Read_Iv()
        {
            var val = _activeMemoryBus.ReadWord(cs, ip);
            ip += 2;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ushort Read_Jb()
        {
            byte offset = Read_Ib();
            return (ushort)(ip + (ushort)(sbyte)offset);
        }

        bool IsInterruptHandlerInstalled(byte interruptNumber)
        {
            // Read new location
            try
            {
                return _activeMemoryBus.ReadWord(idt, (ushort)(interruptNumber * 4 + 2)) != 0;
            }
            catch
            {
                return false;
            }
        }

        byte? _pendingHardwareInterrupt;

        public void RaiseHardwareInterrupt(byte interruptNumber)
        {
            // Put it in the queue, we'll raise it at the end of the next instruction
            _pendingHardwareInterrupt = interruptNumber;
        }

        public virtual void RaiseInterrupt(byte interruptNumber)
        {
            // Read new location
            ushort newcs = _activeMemoryBus.ReadWord(idt, (ushort)(interruptNumber * 4 + 2));
            ushort newip = _activeMemoryBus.ReadWord(idt, (ushort)(interruptNumber * 4));

            if (newcs == 0 && newip == 0)
            {
                throw new InvalidOperationException(string.Format("No handler for interrupt 0x{0:X2} at {1:X4}:{2:X4}", interruptNumber, cs, ip));
            }

            // Save state
            sp -= 2;
            _activeMemoryBus.WriteWord(ss, sp, EFlags);
            sp -= 2;
            _activeMemoryBus.WriteWord(ss, sp, cs);
            sp -= 2;
            _activeMemoryBus.WriteWord(ss, sp, ip);

            // Clear interrupt flag
            FlagI = false;

            cs = newcs;
            ip = newip;
        }

        public virtual void RaiseInterruptInternal(byte interruptNumber)
        {
            // Notify debugger
            if (_debugger != null)
            {
                var saveIP = ip;
                ip = _ipInstruction;

                if (!_debugger.OnSoftwareInterrupt(interruptNumber))
                {
                    ip = saveIP;
                    return;
                }

                ip = saveIP;
            }

            RaiseInterrupt(interruptNumber);
        }

        public ushort idt
        {
            get;
            set;
        }

        public virtual void RaiseHalt()
        {
            _halt = true;
            AbortRunFrame();
        }

        public bool Halted
        {
            get { return _halt; }
            set
            {
                _halt = value;
                if (value)
                    AbortRunFrame();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ushort ResolveSegmentPtr(RegSeg seg)
        {
            if (_prefixSegment != RegSeg.None)
                seg = _prefixSegment;

            return ReadReg(seg);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool RepTest()
        {
            if (!_prefixRepEither)
                return false;

            cx--;
            return cx != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool RepTestCC()
        {
            if (!_prefixRepEither)
                return false;

            if (_prefixRepNE)
            {
                cx--;
                if (!FlagZ)
                {
                    return cx != 0;
                }
            }
            else
            {
                cx--;
                if (FlagZ)
                {
                    return cx != 0;
                }
            }
            return false;
        }

        //bool _executing = false;


        // was the last instruction a return?
        bool _didReturn = false;
        public bool DidReturn
        {
            get
            {
                return _didReturn;
            }
        }

        public bool IsStoppedInDebugger
        {
            get { return _inDebugger; }
            set { _inDebugger = value; }
        }

        bool _inDebugger;
        ushort _ipInstruction;

        public ulong CpuTime = 0;//0xFFFFFFFFFFFFFFFFUL;

        int _instructions;
        bool _runFrameAborted;

        // Stop processing instructions in the current run frame and return to caller
        public void AbortRunFrame()
        {
            _instructions = 0;
            _runFrameAborted = true;
        }

        public bool Run(int instructions)
        {
            _instructions = instructions;
            _runFrameAborted = false;
            while (_instructions > 0)
            {
                RunInternal();
            }
            return _runFrameAborted;
        }

        // True if reading instruction opcode from memory bus
        public bool M1
        {
            get { return _m1; }
        }

        bool _m1;

        Action _instructionHook;
        public Action InstructionHook
        {
            get { return _instructionHook; }
            set { _instructionHook = value; }
        }

        public void RunInternal()
        {
            // Not if halted
            if (_halt)
                return;

            //            _executing = true;
            try
            {
                while (_instructions > 0)
                {
                    _instructions--;

                    // Update CPU time
                    CpuTime++;

                    _instructionHook?.Invoke();

                    // Notify debugger
                    if (_debugger != null)
                    {
                        System.Diagnostics.Debug.Assert(!_inDebugger);

                        _inDebugger = true;
                        if (!_debugger.OnStep())
                        {
                            _inDebugger = false;
                            //                    _executing = false;
                            return;
                        }
                        _inDebugger = false;
                    }

                    _didReturn = false;

                    // Save the address of the current instruction
                    _ipInstruction = ip;

                    // Decode prefixes
                    //                _prefixLock = false;
                    _prefixRepEither = false;
                    _prefixRepNE = false;
                    _prefixSegment = RegSeg.None;
                    _haveReadModRM = false;
                    ushort temp;


                    prefixHandled:      // will jump back to here after decoding an instruction prefix

                    _m1 = true;
                    byte opCode = _activeMemoryBus.ReadByte(cs, ip++);
                    _m1 = false;

                    switch (opCode)
                    {
                        case 0x00:
                            // ADD Eb, Gb
                            Write_Eb(Add8(Read_Eb(), Read_Gb()));
                            break;

                        case 0x01:
                            // ADD Ev, Gv
                            Write_Ev(Add16(Read_Ev(), Read_Gv()));
                            break;

                        case 0x02:
                            // ADD Gb, Eb
                            Write_Gb(Add8(Read_Gb(), Read_Eb()));
                            break;

                        case 0x03:
                            // ADD Gv, Ev
                            Write_Gv(Add16(Read_Gv(), Read_Ev()));
                            break;

                        case 0x04:
                            // ADD AL, Ib
                            al = Add8(al, Read_Ib());
                            break;

                        case 0x05:
                            // ADD eAX Iv
                            ax = Add16(ax, Read_Iv());
                            break;

                        case 0x06:
                            // PUSH ES
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, es);
                            break;

                        case 0x07:
                            // POP ES
                            es = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x08:
                            // OR Eb, Gb
                            Write_Eb(Or8(Read_Eb(), Read_Gb()));
                            break;

                        case 0x09:
                            // OR Ev, Gv
                            Write_Ev(Or16(Read_Ev(), Read_Gv()));
                            break;

                        case 0x0A:
                            // OR Gb, Eb
                            Write_Gb(Or8(Read_Gb(), Read_Eb()));
                            break;

                        case 0x0B:
                            // OR Gv, Ev
                            Write_Gv(Or16(Read_Gv(), Read_Ev()));
                            break;

                        case 0x0C:
                            // OR AL, Ib
                            al = Or8(al, Read_Ib());
                            break;

                        case 0x0D:
                            // OR eAX, Iv
                            ax = Or16(ax, Read_Iv());
                            break;

                        case 0x0E:
                            // PUSH cs
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, cs);
                            break;

                        case 0x0F:
                            throw new InvalidOpCodeException();

                        case 0x10:
                            // ADC Eb, Gb
                            Write_Eb(Adc8(Read_Eb(), Read_Gb()));
                            break;

                        case 0x11:
                            // ADC Ev, Gv
                            Write_Ev(Adc16(Read_Ev(), Read_Gv()));
                            break;

                        case 0x12:
                            // ADC Gb, Eb
                            Write_Gb(Adc8(Read_Gb(), Read_Eb()));
                            break;

                        case 0x13:
                            // ADC Gv, Ev
                            Write_Gv(Adc16(Read_Gv(), Read_Ev()));
                            break;

                        case 0x14:
                            // ADC AL, Ib
                            al = Adc8(al, Read_Ib());
                            break;

                        case 0x15:
                            // ADC eAX Iv
                            ax = Adc16(ax, Read_Iv());
                            break;

                        case 0x16:
                            // PUSH SS
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, ss);
                            break;

                        case 0x17:
                            // POP SS
                            ss = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x18:
                            // SBB Eb, Gb
                            Write_Eb(Sbb8(Read_Eb(), Read_Gb()));
                            break;

                        case 0x19:
                            // SBB Ev, Gv
                            Write_Ev(Sbb16(Read_Ev(), Read_Gv()));
                            break;

                        case 0x1A:
                            // SBB Gb, Eb
                            Write_Gb(Sbb8(Read_Gb(), Read_Eb()));
                            break;

                        case 0x1B:
                            // SBB Gv, Ev
                            Write_Gv(Sbb16(Read_Gv(), Read_Ev()));
                            break;

                        case 0x1C:
                            // SBB AL, Ib
                            al = Sbb8(al, Read_Ib());
                            break;

                        case 0x1D:
                            // SBB eAX, Iv
                            ax = Sbb16(ax, Read_Iv());
                            break;

                        case 0x1E:
                            // PUSH ds
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, ds);
                            break;

                        case 0x1F:
                            // POP ds
                            ds = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x20:
                            // AND Eb, Gb
                            Write_Eb(And8(Read_Eb(), Read_Gb()));
                            break;

                        case 0x21:
                            // AND Ev, Gv
                            Write_Ev(And16(Read_Ev(), Read_Gv()));
                            break;

                        case 0x22:
                            // AND Gb, Eb
                            Write_Gb(And8(Read_Gb(), Read_Eb()));
                            break;

                        case 0x23:
                            // AND Gv, Ev
                            Write_Gv(And16(Read_Gv(), Read_Ev()));
                            break;

                        case 0x24:
                            // AND AL, Ib
                            al = And8(al, Read_Ib());
                            break;

                        case 0x25:
                            // AND eAX Iv
                            ax = And16(ax, Read_Iv());
                            break;

                        case 0x26:
                            // ES: prefix
                            _prefixSegment = RegSeg.ES;
                            goto prefixHandled;

                        case 0x27:
                            al = Daa(al);
                            break;

                        case 0x28:
                            // SUB Eb, Gb
                            Write_Eb(Sub8(Read_Eb(), Read_Gb()));
                            break;

                        case 0x29:
                            // SUB Ev, Gv
                            Write_Ev(Sub16(Read_Ev(), Read_Gv()));
                            break;

                        case 0x2A:
                            // SUB Gb, Eb
                            Write_Gb(Sub8(Read_Gb(), Read_Eb()));
                            break;

                        case 0x2B:
                            // SUB Gv, Ev
                            Write_Gv(Sub16(Read_Gv(), Read_Ev()));
                            break;

                        case 0x2C:
                            // SUB AL, Ib
                            al = Sub8(al, Read_Ib());
                            break;

                        case 0x2D:
                            // SUB eAX, Iv
                            ax = Sub16(ax, Read_Iv());
                            break;

                        case 0x2E:
                            // CS:
                            _prefixSegment = RegSeg.CS;
                            goto prefixHandled;

                        case 0x2F:
                            al = Das(al);
                            break;


                        case 0x30:
                            // XOR Eb, Gb
                            Write_Eb(Xor8(Read_Eb(), Read_Gb()));
                            break;

                        case 0x31:
                            // XOR Ev, Gv
                            Write_Ev(Xor16(Read_Ev(), Read_Gv()));
                            break;

                        case 0x32:
                            // XOR Gb, Eb
                            Write_Gb(Xor8(Read_Gb(), Read_Eb()));
                            break;

                        case 0x33:
                            // XOR Gv, Ev
                            Write_Gv(Xor16(Read_Gv(), Read_Ev()));
                            break;

                        case 0x34:
                            // XOR AL, Ib
                            al = Xor8(al, Read_Ib());
                            break;

                        case 0x35:
                            // XOR eAX Iv
                            ax = Xor16(ax, Read_Iv());
                            break;

                        case 0x36:
                            // SS: prefix
                            _prefixSegment = RegSeg.SS;
                            goto prefixHandled;

                        case 0x37:
                            ax = Aaa(ax);
                            break;

                        case 0x38:
                            // CMP Eb, Gb
                            Sub8(Read_Eb(), Read_Gb());
                            break;

                        case 0x39:
                            // CMP Ev, Gv
                            Sub16(Read_Ev(), Read_Gv());
                            break;

                        case 0x3A:
                            // CMP Gb, Eb
                            Sub8(Read_Gb(), Read_Eb());
                            break;

                        case 0x3B:
                            // CMP Gv, Ev
                            Sub16(Read_Gv(), Read_Ev());
                            break;

                        case 0x3C:
                            // CMP AL, Ib
                            Sub8(al, Read_Ib());
                            break;

                        case 0x3D:
                            // CMP eAX, Iv
                            Sub16(ax, Read_Iv());
                            break;

                        case 0x3E:
                            // DS:
                            _prefixSegment = RegSeg.DS;
                            goto prefixHandled;

                        case 0x3F:
                            // AAS
                            ax = Aas(ax);
                            break;

                        case 0x40:
                            // INC AX
                            ax = Inc16(ax);
                            break;

                        case 0x41:
                            // INC CX
                            cx = Inc16(cx);
                            break;

                        case 0x42:
                            // INC DX
                            dx = Inc16(dx);
                            break;

                        case 0x43:
                            // INC BX
                            bx = Inc16(bx);
                            break;

                        case 0x44:
                            // INC SP
                            sp = Inc16(sp);
                            break;

                        case 0x45:
                            // INC BP
                            bp = Inc16(bp);
                            break;

                        case 0x46:
                            // INC SI
                            si = Inc16(si);
                            break;

                        case 0x47:
                            // INC DI
                            di = Inc16(di);
                            break;

                        case 0x48:
                            // DEC AX
                            ax = Dec16(ax);
                            break;

                        case 0x49:
                            // DEC CX
                            cx = Dec16(cx);
                            break;

                        case 0x4A:
                            // DEC DX
                            dx = Dec16(dx);
                            break;

                        case 0x4B:
                            // DEC BX
                            bx = Dec16(bx);
                            break;

                        case 0x4C:
                            // DEC SP
                            sp = Dec16(sp);
                            break;

                        case 0x4D:
                            // DEC BP
                            bp = Dec16(bp);
                            break;

                        case 0x4E:
                            // DEC SI
                            si = Dec16(si);
                            break;

                        case 0x4F:
                            // DEC DI
                            di = Dec16(di);
                            break;

                        case 0x50:
                            // PUSH AX
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, ax);
                            break;

                        case 0x51:
                            // PUSH CX
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, cx);
                            break;

                        case 0x52:
                            // PUSH DX
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, dx);
                            break;

                        case 0x53:
                            // PUSH BX
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, bx);
                            break;

                        case 0x54:
                            // PUSH SP
                            temp = sp;
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, temp);
                            break;

                        case 0x55:
                            // PUSH BP
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, bp);
                            break;

                        case 0x56:
                            // PUSH SI
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, si);
                            break;

                        case 0x57:
                            // PUSH DI
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, di);
                            break;

                        case 0x58:
                            // POP AX
                            ax = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x59:
                            cx = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x5A:
                            dx = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x5B:
                            bx = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x5C:
                            sp = _activeMemoryBus.ReadWord(ss, sp);
                            break;

                        case 0x5D:
                            bp = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x5E:
                            si = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x5F:
                            di = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x60:
                            // PUSHA
                            _activeMemoryBus.WriteWord(ss, (ushort)(sp - 2), ax);
                            _activeMemoryBus.WriteWord(ss, (ushort)(sp - 4), cx);
                            _activeMemoryBus.WriteWord(ss, (ushort)(sp - 6), dx);
                            _activeMemoryBus.WriteWord(ss, (ushort)(sp - 8), bx);
                            _activeMemoryBus.WriteWord(ss, (ushort)(sp - 10), sp);
                            _activeMemoryBus.WriteWord(ss, (ushort)(sp - 12), bp);
                            _activeMemoryBus.WriteWord(ss, (ushort)(sp - 14), si);
                            _activeMemoryBus.WriteWord(ss, (ushort)(sp - 16), di);
                            sp -= 16;
                            break;

                        case 0x61:
                            // PUSHA
                            sp += 16;
                            ax = _activeMemoryBus.ReadWord(ss, (ushort)(sp - 2));
                            cx = _activeMemoryBus.ReadWord(ss, (ushort)(sp - 4));
                            dx = _activeMemoryBus.ReadWord(ss, (ushort)(sp - 6));
                            bx = _activeMemoryBus.ReadWord(ss, (ushort)(sp - 8));
                            //sp = _activeMemoryBus.ReadWord(ss, (ushort)(sp - 10));
                            bp = _activeMemoryBus.ReadWord(ss, (ushort)(sp - 12));
                            si = _activeMemoryBus.ReadWord(ss, (ushort)(sp - 14));
                            di = _activeMemoryBus.ReadWord(ss, (ushort)(sp - 16));
                            break;

                        case 0x62:
                            // BOUND r16,m16
                            ReadModRM();
                            if (!_modRMIsPointer)
                                throw new InvalidOpCodeException();

                            // Read bounds
                            short lowerBound = (short)_activeMemoryBus.ReadWord(_modRMSeg, _modRMOffset);
                            short upperBound = (short)_activeMemoryBus.ReadWord(_modRMSeg, (ushort)(_modRMOffset + 2));

                            // Read array index
                            short arrayIndex = (short)Read_Gv();
                            if (arrayIndex < lowerBound || arrayIndex > upperBound)
                            {
                                // Raise exception
                                ip = _ipInstruction;
                                RaiseInterruptInternal(5);
                            }
                            break;

                        case 0x63:
                        case 0x64:
                        case 0x65:
                        case 0x66:
                        case 0x67:
                            throw new InvalidOpCodeException();

                        case 0x68:
                            // Push Iv
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, Read_Iv());
                            break;

                        case 0x69:
                            ReadModRM();
                            Write_Gv((ushort)(IMul16(Read_Ev(), Read_Iv()) & 0xFFFF));
                            break;

                        case 0x6A:
                            // PUSH Ib
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, (ushort)(sbyte)Read_Ib());
                            break;

                        case 0x6B:
                            // imul Gv,Ev,Ib
                            ReadModRM();
                            Write_Gv((ushort)(IMul16(Read_Ev(), (ushort)(sbyte)Read_Ib()) & 0xFFFF));
                            break;

                        case 0x6C:
                            // INSB
                            if (cx != 0 || !_prefixRepEither)
                            {
                                do
                                {
                                    _activeMemoryBus.WriteByte(es, di, _portBus.ReadPortByte(dx));
                                    if (FlagD)
                                    {
                                        di--;
                                    }
                                    else
                                    {
                                        di++;
                                    }
                                }
                                while (RepTest());
                            }
                            break;

                        case 0x6D:
                            if (cx != 0 || !_prefixRepEither)
                            {
                                // INSW
                                do
                                {
                                    _activeMemoryBus.WriteWord(es, di, _portBus.ReadPortWord(dx));
                                    if (FlagD)
                                    {
                                        di -= 2;
                                    }
                                    else
                                    {
                                        di += 2;
                                    }
                                }
                                while (RepTest());
                            }
                            break;

                        case 0x6E:
                            // OUTSB
                            if (cx != 0 || !_prefixRepEither)
                            {
                                do
                                {
                                    _portBus.WritePortByte(dx, _activeMemoryBus.ReadByte(ds, si));
                                    if (FlagD)
                                    {
                                        si--;
                                    }
                                    else
                                    {
                                        si++;
                                    }
                                }
                                while (RepTest());
                            }
                            break;

                        case 0x6F:
                            // OUTSW
                            if (cx != 0 || !_prefixRepEither)
                            {
                                do
                                {
                                    _portBus.WritePortWord(dx, _activeMemoryBus.ReadWord(ds, si));
                                    if (FlagD)
                                    {
                                        si -= 2;
                                    }
                                    else
                                    {
                                        si += 2;
                                    }
                                }
                                while (RepTest());
                            }
                            break;

                        case 0x70:
                            // JO Jb
                            if (FlagO)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x71:
                            // JNO Jb
                            if (!FlagO)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x72:
                            // JB Jb
                            // JNAE Jb
                            // JC Jb
                            if (FlagC)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x73:
                            // JNB Jb
                            // JAE Jb
                            // JNC Jb
                            if (!FlagC)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x74:
                            // JZ Jb
                            // JE Jb
                            if (FlagZ)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x75:
                            // JNZ Jb
                            // JNE Jb
                            if (!FlagZ)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x76:
                            // JBE Jb
                            // JNA Jb
                            if (FlagC || FlagZ)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x77:
                            // JA Jb
                            // JNBE Jb
                            if (!FlagC && !FlagZ)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x78:
                            // JS Jb
                            if (FlagS)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x79:
                            // JNS Jb
                            if (!FlagS)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x7A:
                            // JP Jb
                            // JPE Jb
                            if (FlagP)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x7B:
                            // JPO Jb
                            // JNP Jb
                            if (!FlagP)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x7C:
                            // JL Jb
                            // JNGE Jb
                            if (FlagS != FlagO)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x7D:
                            // JGE Jb
                            // JNL Jb
                            if (FlagS == FlagO)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x7E:
                            // JLE Jb
                            // JNG Jb
                            if (FlagZ || (FlagS != FlagO))
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x7F:
                            // JG Jb
                            // JNLE Jb
                            if (!FlagZ && (FlagS == FlagO))
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0x80:
                        case 0x82:
                            // GRP1 Eb Ib
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Eb(Add8(Read_Eb(), Read_Ib())); break;
                                case 1: Write_Eb(Or8(Read_Eb(), Read_Ib())); break;
                                case 2: Write_Eb(Adc8(Read_Eb(), Read_Ib())); break;
                                case 3: Write_Eb(Sbb8(Read_Eb(), Read_Ib())); break;
                                case 4: Write_Eb(And8(Read_Eb(), Read_Ib())); break;
                                case 5: Write_Eb(Sub8(Read_Eb(), Read_Ib())); break;
                                case 6: Write_Eb(Xor8(Read_Eb(), Read_Ib())); break;
                                case 7: Sub8(Read_Eb(), Read_Ib()); break;
                            }
                            break;

                        case 0x81:
                            // GRP1 Ev Iv 
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Ev(Add16(Read_Ev(), Read_Iv())); break;
                                case 1: Write_Ev(Or16(Read_Ev(), Read_Iv())); break;
                                case 2: Write_Ev(Adc16(Read_Ev(), Read_Iv())); break;
                                case 3: Write_Ev(Sbb16(Read_Ev(), Read_Iv())); break;
                                case 4: Write_Ev(And16(Read_Ev(), Read_Iv())); break;
                                case 5: Write_Ev(Sub16(Read_Ev(), Read_Iv())); break;
                                case 6: Write_Ev(Xor16(Read_Ev(), Read_Iv())); break;
                                case 7: Sub16(Read_Ev(), Read_Iv()); break;
                            }
                            break;

                        case 0x83:
                            // GRP1 Ev Ib 
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Ev(Add16(Read_Ev(), (ushort)(sbyte)Read_Ib())); break;
                                case 1: Write_Ev(Or16(Read_Ev(), (ushort)(sbyte)Read_Ib())); break;
                                case 2: Write_Ev(Adc16(Read_Ev(), (ushort)(sbyte)Read_Ib())); break;
                                case 3: Write_Ev(Sbb16(Read_Ev(), (ushort)(sbyte)Read_Ib())); break;
                                case 4: Write_Ev(And16(Read_Ev(), (ushort)(sbyte)Read_Ib())); break;
                                case 5: Write_Ev(Sub16(Read_Ev(), (ushort)(sbyte)Read_Ib())); break;
                                case 6: Write_Ev(Xor16(Read_Ev(), (ushort)(sbyte)Read_Ib())); break;
                                case 7: Sub16(Read_Ev(), (ushort)(sbyte)Read_Ib()); break;
                            }
                            break;

                        case 0x84:
                            // Test Gb, Eb
                            And8(Read_Gb(), Read_Eb());
                            break;

                        case 0x85:
                            // Test Gv, Ev
                            And16(Read_Gv(), Read_Ev());
                            break;

                        case 0x86:
                            // XCHG Gb, Eb
                            temp = Read_Gb();
                            Write_Gb(Read_Eb());
                            Write_Eb((byte)temp);
                            break;

                        case 0x87:
                            // XCHG Gv, Evb
                            temp = Read_Gv();
                            Write_Gv(Read_Ev());
                            Write_Ev(temp);
                            break;

                        case 0x88:
                            // MOV Eb, Gb 
                            Write_Eb(Read_Gb());
                            break;

                        case 0x89:
                            // MOV Ev, Gv
                            Write_Ev(Read_Gv());
                            break;

                        case 0x8A:
                            // MOV Gb, Eb
                            Write_Gb(Read_Eb());
                            break;

                        case 0x8B:
                            // MOV Gv Ev
                            Write_Gv(Read_Ev());
                            break;

                        case 0x8C:
                            // MOV Ew Sw
                            Write_Ev(Read_Sv());
                            break;

                        case 0x8D:
                            // LEA Gv M
                            ReadModRM();
                            if (!_modRMIsPointer)
                                throw new InvalidOpCodeException();
                            Write_Gv(_modRMOffset);
                            break;

                        case 0x8E:
                            // MOV Sw Ew
                            Write_Sv(Read_Ev());
                            break;

                        case 0x8F:
                            // POP Ev
                            Write_Ev(_activeMemoryBus.ReadWord(ss, sp));
                            sp += 2;
                            break;

                        case 0x90:
                            // NOP
                            break;

                        case 0x91:
                            // XCHG eCX eAX
                            temp = ax;
                            ax = cx;
                            cx = temp;
                            break;

                        case 0x92:
                            // XCHG eDX eAX
                            temp = ax;
                            ax = dx;
                            dx = temp;
                            break;

                        case 0x93:
                            // XCHG eBX eAX
                            temp = ax;
                            ax = bx;
                            bx = temp;
                            break;

                        case 0x94:
                            // XCHG eSP eAX
                            temp = ax;
                            ax = sp;
                            sp = temp;
                            break;

                        case 0x95:
                            // XCHG eBP eAX
                            temp = ax;
                            ax = bp;
                            bp = temp;
                            break;

                        case 0x96:
                            // XCHG eSI eAX
                            temp = ax;
                            ax = si;
                            si = temp;
                            break;

                        case 0x97:
                            // XCHG eDI eAX
                            temp = ax;
                            ax = di;
                            di = temp;
                            break;

                        case 0x98:
                            // CBW
                            ax = Cbw(al);
                            break;

                        case 0x99:
                            {
                                // CWD
                                uint val = Cwd(ax);
                                dx = (ushort)(val >> 16);
                                ax = (ushort)(val & 0xFFFF);
                                break;
                            }

                        case 0x9A:
                            {
                                // CALL Ap

                                // Read target address
                                temp = _activeMemoryBus.ReadWord(cs, ip);
                                ip += 2;

                                var newcs = _activeMemoryBus.ReadWord(cs, ip);
                                ip += 2;

                                // Push current ip
                                sp -= 2;
                                _activeMemoryBus.WriteWord(ss, sp, cs);
                                sp -= 2;
                                _activeMemoryBus.WriteWord(ss, sp, ip);

                                // Jump
                                ip = temp;
                                cs = newcs;

                                break;
                            }

                        case 0x9B:
                            // WAIT
                            break;

                        case 0x9C:
                            // PUSHF
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, EFlags);
                            break;

                        case 0x9D:
                            // POPF
                            EFlags = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0x9E:
                            // SAHF
                            Flags8 = ah;
                            break;

                        case 0x9F:
                            // LAHF
                            ah = Flags8;
                            break;

                        case 0xA0:
                            // MOV al, [Ob]
                            temp = _activeMemoryBus.ReadWord(cs, ip);
                            ip += 2;
                            al = _activeMemoryBus.ReadByte(ResolveSegmentPtr(RegSeg.DS), temp);
                            break;

                        case 0xA1:
                            // MOV ax, [Ov]
                            temp = _activeMemoryBus.ReadWord(cs, ip);
                            ip += 2;
                            ax = _activeMemoryBus.ReadWord(ResolveSegmentPtr(RegSeg.DS), temp);
                            break;

                        case 0xA2:
                            // MOV [Ob], al
                            temp = _activeMemoryBus.ReadWord(cs, ip);
                            ip += 2;
                            _activeMemoryBus.WriteByte(ResolveSegmentPtr(RegSeg.DS), temp, al);
                            break;

                        case 0xA3:
                            // MOV [Ob], ax
                            temp = _activeMemoryBus.ReadWord(cs, ip);
                            ip += 2;
                            _activeMemoryBus.WriteWord(ResolveSegmentPtr(RegSeg.DS), temp, ax);
                            break;

                        case 0xA4:
                            // MOVSB
                            if (cx != 0 || !_prefixRepEither)
                            {
                                temp = ResolveSegmentPtr(RegSeg.DS);
                                do
                                {
                                    _activeMemoryBus.WriteByte(es, di, _activeMemoryBus.ReadByte(temp, si));
                                    if (FlagD)
                                    {
                                        di--;
                                        si--;
                                    }
                                    else
                                    {
                                        di++;
                                        si++;
                                    }
                                }
                                while (RepTest());
                            }
                            break;

                        case 0xA5:
                            if (cx != 0 || !_prefixRepEither)
                            {
                                // MOVSB
                                temp = ResolveSegmentPtr(RegSeg.DS);
                                do
                                {
                                    _activeMemoryBus.WriteWord(es, di, _activeMemoryBus.ReadWord(temp, si));
                                    if (FlagD)
                                    {
                                        di -= 2;
                                        si -= 2;
                                    }
                                    else
                                    {
                                        di += 2;
                                        si += 2;
                                    }
                                }
                                while (RepTest());
                            }
                            break;

                        case 0xA6:
                            if (cx != 0 || !_prefixRepEither)
                            {
                                // CMPSB
                                temp = ResolveSegmentPtr(RegSeg.DS);
                                do
                                {
                                    Sub8(_activeMemoryBus.ReadByte(ResolveSegmentPtr(RegSeg.DS), si), _activeMemoryBus.ReadByte(es, di));
                                    if (FlagD)
                                    {
                                        di -= 1;
                                        si -= 1;
                                    }
                                    else
                                    {
                                        di += 1;
                                        si += 1;
                                    }
                                }
                                while (RepTestCC());
                            }
                            break;

                        case 0xA7:
                            // CMPSW
                            // CMPSB
                            if (cx != 0 || !_prefixRepEither)
                            {
                                temp = ResolveSegmentPtr(RegSeg.DS);
                                do
                                {
                                    Sub16(_activeMemoryBus.ReadWord(ResolveSegmentPtr(RegSeg.DS), si), _activeMemoryBus.ReadWord(es, di));
                                    if (FlagD)
                                    {
                                        di -= 2;
                                        si -= 2;
                                    }
                                    else
                                    {
                                        di += 2;
                                        si += 2;
                                    }
                                }
                                while (RepTestCC());
                            }
                            break;

                        case 0xA8:
                            // TEST AL Ib 
                            And8(al, Read_Ib());
                            break;

                        case 0xA9:
                            // TEST eAX Iv
                            And16(ax, Read_Iv());
                            break;

                        case 0xAA:
                            if (cx != 0 || !_prefixRepEither)
                            {
                                // STOSB
                                do
                                {
                                    _activeMemoryBus.WriteByte(es, di, al);
                                    if (FlagD)
                                    {
                                        di--;
                                    }
                                    else
                                    {
                                        di++;
                                    }
                                }
                                while (RepTest());
                            }
                            break;

                        case 0xAB:
                            if (cx != 0 || !_prefixRepEither)
                            {
                                // STOSW
                                do
                                {
                                    _activeMemoryBus.WriteWord(es, di, ax);
                                    if (FlagD)
                                    {
                                        di -= 2;
                                    }
                                    else
                                    {
                                        di += 2;
                                    }
                                }
                                while (RepTest());
                            }
                            break;


                        case 0xAC:
                            if (cx != 0 || !_prefixRepEither)
                            {
                                // LODSB
                                temp = ResolveSegmentPtr(RegSeg.DS);
                                do
                                {
                                    al = _activeMemoryBus.ReadByte(temp, si);
                                    if (FlagD)
                                    {
                                        si--;
                                    }
                                    else
                                    {
                                        si++;
                                    }
                                }
                                while (RepTest());
                            }
                            break;

                        case 0xAD:
                            if (cx != 0 || !_prefixRepEither)
                            {
                                // LODSW
                                temp = ResolveSegmentPtr(RegSeg.DS);
                                do
                                {
                                    ax = _activeMemoryBus.ReadWord(temp, si);
                                    if (FlagD)
                                    {
                                        si -= 2;
                                    }
                                    else
                                    {
                                        si += 2;
                                    }
                                }
                                while (RepTest());
                            }
                            break;

                        case 0xAE:
                            if (cx != 0 || !_prefixRepEither)
                            {
                                // SCASB
                                do
                                {
                                    Sub8(al, _activeMemoryBus.ReadByte(es, di));
                                    if (FlagD)
                                    {
                                        di -= 1;
                                    }
                                    else
                                    {
                                        di += 1;
                                    }
                                }
                                while (RepTestCC());
                            }
                            break;

                        case 0xAF:
                            // SCASW
                            if (cx != 0 || !_prefixRepEither)
                            {
                                do
                                {
                                    Sub16(ax, _activeMemoryBus.ReadWord(es, di));
                                    if (FlagD)
                                    {
                                        di -= 2;
                                    }
                                    else
                                    {
                                        di += 2;
                                    }
                                }
                                while (RepTestCC());
                            }
                            break;

                        case 0xB0:
                            // MOV AL Ib
                            al = Read_Ib();
                            break;

                        case 0xB1:
                            // MOV CL Ib
                            cl = Read_Ib();
                            break;

                        case 0xB2:
                            // MOV DL Ib
                            dl = Read_Ib();
                            break;

                        case 0xB3:
                            // MOV BL Ib
                            bl = Read_Ib();
                            break;

                        case 0xB4:
                            // MOV AH Ib
                            ah = Read_Ib();
                            break;

                        case 0xB5:
                            // MOV CH Ib
                            ch = Read_Ib();
                            break;

                        case 0xB6:
                            // MOV DH Ib
                            dh = Read_Ib();
                            break;

                        case 0xB7:
                            // MOV BH Ib
                            bh = Read_Ib();
                            break;


                        case 0xB8:
                            // MOV AX, Iv
                            ax = Read_Iv();
                            break;

                        case 0xB9:
                            // MOV CX, Iv
                            cx = Read_Iv();
                            break;

                        case 0xBA:
                            // MOV DX, Iv
                            dx = Read_Iv();
                            break;

                        case 0xBB:
                            // MOV BX, Iv
                            bx = Read_Iv();
                            break;

                        case 0xBC:
                            // MOV SP, Iv
                            sp = Read_Iv();
                            break;

                        case 0xBD:
                            // MOV BP, Iv
                            bp = Read_Iv();
                            break;

                        case 0xBE:
                            // MOV SI, Iv
                            si = Read_Iv();
                            break;

                        case 0xBF:
                            // MOV DI, Iv
                            di = Read_Iv();
                            break;

                        case 0xC0:
                            // GRP2 Eb, Ib
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Eb(Rol8(Read_Eb(), Read_Ib())); break;
                                case 1: Write_Eb(Ror8(Read_Eb(), Read_Ib())); break;
                                case 2: Write_Eb(Rcl8(Read_Eb(), Read_Ib())); break;
                                case 3: Write_Eb(Rcr8(Read_Eb(), Read_Ib())); break;
                                case 4: Write_Eb(Shl8(Read_Eb(), Read_Ib())); break;
                                case 5: Write_Eb(Shr8(Read_Eb(), Read_Ib())); break;
                                case 6: throw new InvalidOpCodeException();
                                case 7: Write_Eb(Sar8(Read_Eb(), Read_Ib())); break;
                            }
                            break;

                        case 0xC1:
                            // GRP2 Ev Ib 
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Ev(Rol16(Read_Ev(), Read_Ib())); break;
                                case 1: Write_Ev(Ror16(Read_Ev(), Read_Ib())); break;
                                case 2: Write_Ev(Rcl16(Read_Ev(), Read_Ib())); break;
                                case 3: Write_Ev(Rcr16(Read_Ev(), Read_Ib())); break;
                                case 4: Write_Ev(Shl16(Read_Ev(), Read_Ib())); break;
                                case 5: Write_Ev(Shr16(Read_Ev(), Read_Ib())); break;
                                case 6: throw new InvalidOpCodeException();
                                case 7: Write_Ev(Sar16(Read_Ev(), Read_Ib())); break;
                            }
                            break;

                        case 0xC2:
                            // RET Iw
                            temp = Read_Iv();
                            ip = _activeMemoryBus.ReadWord(ss, sp);
                            sp += (ushort)(temp + 2);
                            _didReturn = true;
                            break;

                        case 0xC3:
                            // RET
                            ip = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            _didReturn = true;
                            break;

                        case 0xC4:
                            // LES Gv Mp
                            ReadModRM();
                            if (!_modRMIsPointer)
                                throw new InvalidOpCodeException();

                            Write_Gv(Read_Ev());
                            es = _activeMemoryBus.ReadWord(_modRMSeg, (ushort)(_modRMOffset + 2));
                            break;

                        case 0xC5:
                            // LDS Gv Mp
                            ReadModRM();
                            if (!_modRMIsPointer)
                                throw new InvalidOpCodeException();

                            Write_Gv(Read_Ev());
                            ds = _activeMemoryBus.ReadWord(_modRMSeg, (ushort)(_modRMOffset + 2));
                            break;

                        case 0xC6:
                            // MOV Eb Ib
                            ReadModRM();
                            Write_Eb(Read_Ib());
                            break;

                        case 0xC7:
                            // MOV Ev Iv
                            ReadModRM();
                            Write_Ev(Read_Iv());
                            break;

                        case 0xC8:
                            // ENTER Iw, Ib
                            ushort storage = Read_Iv();
                            byte nestingLevel = (byte)(Read_Ib() % 32);

                            // Push bp
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, bp);

                            if (nestingLevel == 0)
                            {
                                bp = sp;
                            }
                            else
                            {
                                temp = sp;
                                for (byte i = 0; i < nestingLevel - 1; i++)
                                {
                                    bp -= 2;
                                    sp -= 2;
                                    _activeMemoryBus.WriteWord(ss, sp, _activeMemoryBus.ReadWord(ss, bp));
                                }

                                sp -= 2;
                                _activeMemoryBus.WriteWord(ss, sp, temp);

                                bp = temp;
                            }

                            sp -= storage;
                            break;

                        case 0xC9:
                            // LEAVE
                            sp = bp;
                            bp = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            break;

                        case 0xCA:
                            // RETF Iv
                            temp = Read_Iv();

                            ip = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            cs = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;

                            sp += temp;
                            _didReturn = true;
                            break;

                        case 0xCB:
                            // RETF
                            ip = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            cs = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            _didReturn = true;
                            break;

                        case 0xCC:
                            // INT 3
                            RaiseInterruptInternal(3);
                            break;

                        case 0xCD:
                            // Int Ib
                            {
                                byte intNo = Read_Ib();
                                RaiseInterruptInternal(intNo);
                            }
                            break;

                        case 0xCE:
                            // INTO
                            if (FlagO)
                            {
                                RaiseInterruptInternal(4);
                            }
                            break;

                        case 0xCF:
                            // IRET
                            ip = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            cs = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            EFlags = _activeMemoryBus.ReadWord(ss, sp);
                            sp += 2;
                            _didReturn = true;
                            break;

                        case 0xD0:
                            // GRP2 Eb 1 
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Eb(Rol8(Read_Eb(), 1)); break;
                                case 1: Write_Eb(Ror8(Read_Eb(), 1)); break;
                                case 2: Write_Eb(Rcl8(Read_Eb(), 1)); break;
                                case 3: Write_Eb(Rcr8(Read_Eb(), 1)); break;
                                case 4: Write_Eb(Shl8(Read_Eb(), 1)); break;
                                case 5: Write_Eb(Shr8(Read_Eb(), 1)); break;
                                case 6: throw new InvalidOpCodeException();
                                case 7: Write_Eb(Sar8(Read_Eb(), 1)); break;
                            }
                            break;

                        case 0xD1:
                            // GRP2 Ev 1 
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Ev(Rol16(Read_Ev(), 1)); break;
                                case 1: Write_Ev(Ror16(Read_Ev(), 1)); break;
                                case 2: Write_Ev(Rcl16(Read_Ev(), 1)); break;
                                case 3: Write_Ev(Rcr16(Read_Ev(), 1)); break;
                                case 4: Write_Ev(Shl16(Read_Ev(), 1)); break;
                                case 5: Write_Ev(Shr16(Read_Ev(), 1)); break;
                                case 6: throw new InvalidOpCodeException();
                                case 7: Write_Ev(Sar16(Read_Ev(), 1)); break;
                            }
                            break;

                        case 0xD2:
                            // GRP2 Eb CL
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Eb(Rol8(Read_Eb(), cl)); break;
                                case 1: Write_Eb(Ror8(Read_Eb(), cl)); break;
                                case 2: Write_Eb(Rcl8(Read_Eb(), cl)); break;
                                case 3: Write_Eb(Rcr8(Read_Eb(), cl)); break;
                                case 4: Write_Eb(Shl8(Read_Eb(), cl)); break;
                                case 5: Write_Eb(Shr8(Read_Eb(), cl)); break;
                                case 6: throw new InvalidOpCodeException();
                                case 7: Write_Eb(Sar8(Read_Eb(), cl)); break;
                            }
                            break;

                        case 0xD3:
                            // GRP2 Ev CL
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Ev(Rol16(Read_Ev(), cl)); break;
                                case 1: Write_Ev(Ror16(Read_Ev(), cl)); break;
                                case 2: Write_Ev(Rcl16(Read_Ev(), cl)); break;
                                case 3: Write_Ev(Rcr16(Read_Ev(), cl)); break;
                                case 4: Write_Ev(Shl16(Read_Ev(), cl)); break;
                                case 5: Write_Ev(Shr16(Read_Ev(), cl)); break;
                                case 6: throw new InvalidOpCodeException();
                                case 7: Write_Ev(Sar16(Read_Ev(), cl)); break;
                            }
                            break;

                        case 0xD4:
                            // AAM I0 
                            ax = Aam(al, Read_Ib());
                            break;

                        case 0xD5:
                            // AAD I0
                            ax = Aad(ax, Read_Ib());
                            break;

                        case 0xD6:
                            // -
                            throw new InvalidOpCodeException();

                        case 0xD7:
                            // XLAT
                            al = _activeMemoryBus.ReadByte(ResolveSegmentPtr(RegSeg.DS), (ushort)(bx + al));
                            break;

                        case 0xD8:
                        case 0xD9:
                        case 0xDA:
                        case 0xDB:
                        case 0xDC:
                        case 0xDD:
                        case 0xDE:
                        case 0xDF:
                            // -
                            throw new InvalidOpCodeException();

                        case 0xE0:
                            // LOOPNZ Jb
                            cx--;
                            if (cx != 0 && !FlagZ)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0xE1:
                            // LOOPZ Jb
                            cx--;
                            if (cx != 0 && FlagZ)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0xE2:
                            // LOOP Jb
                            cx--;
                            if (cx != 0)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0xE3:
                            // JCXZ Jb
                            if (cx == 0)
                                ip = Read_Jb();
                            else
                                ip++;
                            break;

                        case 0xE4:
                            // IN AL Ib
                            al = _portBus.ReadPortByte(Read_Ib());
                            break;

                        case 0xE5:
                            // IN eAX Ib
                            ax = _portBus.ReadPortWord(Read_Ib());
                            break;

                        case 0xE6:
                            // OUT Ib AL
                            _portBus.WritePortByte(Read_Ib(), al);
                            break;

                        case 0xE7:
                            // OUT Ib eAX
                            _portBus.WritePortWord(Read_Ib(), ax);
                            break;

                        case 0xE8:
                            // CALL Jv

                            // Read target address offset
                            temp = Read_Iv();

                            // Push current ip
                            sp -= 2;
                            _activeMemoryBus.WriteWord(ss, sp, ip);

                            // Jump
                            ip += temp;
                            break;

                        case 0xE9:
                            // JMP Jv
                            temp = Read_Iv();
                            ip += temp;
                            break;

                        case 0xEA:
                            // JMP Ap
                            temp = Read_Iv();
                            cs = Read_Iv();
                            ip = temp;
                            break;

                        case 0xEB:
                            // JMP Jb
                            ip = Read_Jb();
                            break;

                        case 0xEC:
                            // IN AL DX
                            al = _portBus.ReadPortByte(dx);
                            break;

                        case 0xED:
                            // IN eAX DX
                            ax = _portBus.ReadPortWord(dx);
                            break;

                        case 0xEE:
                            // OUT DX AL
                            _portBus.WritePortByte(dx, al);
                            break;

                        case 0xEF:
                            // OUT DX eAX
                            _portBus.WritePortWord(dx, ax);
                            break;

                        case 0xF0:
                            // LOCK (Ignore)
                            //                        _prefixLock = true;
                            goto prefixHandled;

                        case 0xF1:
                            // -
                            throw new InvalidOpCodeException();

                        case 0xF2:
                            // REPNZ (Prefix)
                            _prefixRepEither = true;
                            _prefixRepNE = true;
                            goto prefixHandled;

                        case 0xF3:
                            // REPZ (Prefix)
                            _prefixRepEither = true;
                            _prefixRepNE = false;
                            goto prefixHandled;

                        case 0xF4:
                            // HLT
                            RaiseHalt();
                            break;

                        case 0xF5:
                            // CMC
                            FlagC = !FlagC;
                            break;

                        case 0xF6:
                            // GRP3a Eb
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: And8(Read_Eb(), Read_Ib()); break;
                                case 1: throw new NotImplementedException();
                                case 2: Write_Eb(Not8(Read_Eb())); break;
                                case 3: Write_Eb(Neg8(Read_Eb())); break;
                                case 4: ax = Mul8(al, Read_Eb()); break;
                                case 5: ax = IMul8(al, Read_Eb()); break;
                                case 6:
                                    {
                                        ax = Div8(ax, Read_Eb());
                                        break;
                                    }
                                case 7:
                                    {
                                        ax = IDiv8(ax, Read_Eb());
                                        break;
                                    }
                            }
                            break;

                        case 0xF7:
                            // GRP3b Ev
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: And16(Read_Ev(), Read_Iv()); break;
                                case 1: throw new NotImplementedException();
                                case 2: Write_Ev(Not16(Read_Ev())); break;
                                case 3: Write_Ev(Neg16(Read_Ev())); break;
                                case 4: dxax = Mul16(ax, Read_Ev()); break;
                                case 5: dxax = IMul16(ax, Read_Ev()); break;
                                case 6: dxax = Div16(dxax, Read_Ev()); break;
                                case 7: dxax = IDiv16(dxax, Read_Ev()); break;
                            }
                            break;

                        case 0xF8:
                            // CLC
                            FlagC = false;
                            break;

                        case 0xF9:
                            // STC
                            FlagC = true;
                            break;

                        case 0xFA:
                            // CLI
                            FlagI = false;
                            break;

                        case 0xFB:
                            // STI
                            FlagI = true;
                            break;

                        case 0xFC:
                            // CLD
                            FlagD = false;
                            break;

                        case 0xFD:
                            // STD
                            FlagD = true;
                            break;

                        case 0xFE:
                            // GRP4 Eb
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Eb(Inc8(Read_Eb())); break;
                                case 1: Write_Eb(Dec8(Read_Eb())); break;
                                default:
                                    throw new InvalidOpCodeException();
                            }
                            break;

                        case 0xFF:
                            // GRP5 Ev
                            ReadModRM();
                            switch ((_modRM >> 3) & 0x07)
                            {
                                case 0: Write_Ev(Inc16(Read_Ev())); break;
                                case 1: Write_Ev(Dec16(Read_Ev())); break;
                                case 2:
                                    {
                                        // NEAR CALL Ev
                                        ushort proc = Read_Ev();
                                        sp -= 2;
                                        _activeMemoryBus.WriteWord(ss, sp, ip);
                                        ip = proc;
                                        break;
                                    }
                                case 3:
                                    {
                                        // FAR CALL M
                                        if (!_modRMIsPointer)
                                            throw new InvalidOpCodeException(); ;
                                        ushort proc = _activeMemoryBus.ReadWord(_modRMSeg, _modRMOffset);
                                        ushort seg = _activeMemoryBus.ReadWord(_modRMSeg, (ushort)(_modRMOffset + 2));
                                        sp -= 2;
                                        _activeMemoryBus.WriteWord(ss, sp, cs);
                                        sp -= 2;
                                        _activeMemoryBus.WriteWord(ss, sp, ip);
                                        ip = proc;
                                        cs = seg;
                                        break;
                                    }
                                case 4:
                                    {
                                        // NEAR JMP Ev
                                        ip = Read_Ev();
                                        break;
                                    }
                                case 5:
                                    {
                                        // FAR JMP M
                                        if (!_modRMIsPointer)
                                            throw new InvalidOpCodeException(); ;
                                        ip = _activeMemoryBus.ReadWord(_modRMSeg, _modRMOffset);
                                        cs = _activeMemoryBus.ReadWord(_modRMSeg, (ushort)(_modRMOffset + 2));
                                        break;
                                    }

                                case 6:
                                    {
                                        // PUSH
                                        temp = Read_Ev();
                                        sp -= 2;
                                        _activeMemoryBus.WriteWord(ss, sp, temp);
                                        break;
                                    }

                                case 7: throw new InvalidOpCodeException();
                            }
                            break;
                    }

                    // If interrupts are enabled and we have a pending hardward interrupt, then raise it now
                    if (FlagI && _pendingHardwareInterrupt.HasValue)
                    {
                        var intNo = _pendingHardwareInterrupt.Value;
                        _pendingHardwareInterrupt = null;
                        RaiseInterruptInternal(intNo);
                    }
                }
            }
            catch (DivideByZeroException)
            {
                ip = _ipInstruction;
                if (IsInterruptHandlerInstalled((byte)0))
                    RaiseInterruptInternal((byte)0);
                else
                    throw;
            }
            catch (CPUException x)
            {
                if (x.RestoreIP)
                    ip = _ipInstruction;

                x.ipSet = true;
                x.cs = cs;
                x.ip = ip;

                if (IsInterruptHandlerInstalled((byte)x.InterruptNo))
                    RaiseInterruptInternal((byte)x.InterruptNo);
                else
                    throw;
            }
        }

        #endregion
    }
}
