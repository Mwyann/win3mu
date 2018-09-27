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

namespace Sharp86
{
    public class Disassembler
    {
        public Disassembler(CPU cpu=null)
        {
            if (cpu!= null)
            {
                _memoryBus = cpu.MemoryBus;
                cs = cpu.cs;
                ip = cpu.ip;
            }
        }

        public Disassembler(IMemoryBus memoryBus, ushort cs, ushort ip)
        {
            _memoryBus = memoryBus;
            this.cs = cs;
            this.ip = ip;
        }

        IMemoryBus _memoryBus;
        public ushort cs;
        public ushort ip;

        public IMemoryBus MemoryBus
        {
            get { return _memoryBus; }
            set { _memoryBus = value; }
        }

        public virtual byte ReadByte(ushort seg, ushort offset)
        {
            return _memoryBus.ReadByte(seg, offset);
        }

        public ushort ReadWord(ushort seg, ushort offset)
        {
            return (ushort)(ReadByte(seg, offset) | ReadByte(seg, (ushort)(offset + 1)) << 8);
        }


        // Decode prefixes
        bool _prefixRepEither;
        bool _prefixRepNE;
        RegSeg _prefixSegment;
        bool _haveReadModRM;
        byte _modRM;
        string _modRMSeg;
        string _modRMOffset;
        bool _modRMIsPointer;

        public bool IsCall;

        string FormatDisplacement(ushort displacement)
        {
            short signed = unchecked((short)displacement);
            if (signed >= 0)
                return string.Format("+0x{0:X4}", signed);
            else
                return string.Format("-0x{0:X4}", -signed);
        }

        string FormatDisplacement(byte displacement)
        {
            sbyte signed = unchecked((sbyte)displacement);
            if (signed >= 0)
                return string.Format("+0x{0:X2}", signed);
            else
                return string.Format("-0x{0:X2}", -signed);
        }

        void ReadModRM()
        {
            System.Diagnostics.Debug.Assert(!_haveReadModRM);

            // Remember we've read it
            _haveReadModRM = true;

            // Read the mod RM byte
            _modRM = ReadByte(cs, ip++);
            _modRMIsPointer = true;

            _modRMSeg = "";
            _modRMOffset = "";
            string displacement = "";

            // Read displacement
            switch (_modRM & 0xC0)
            {
                case 0x00:
                    // Mode 0
                    switch (_modRM & 0x7)
                    {
                        case 0:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = "bx+si";
                            break;

                        case 1:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = "bx+di";
                            break;

                        case 2:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = "bp+si";
                            break;

                        case 3:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = "bp+di";
                            break;

                        case 4:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = "si";
                            break;

                        case 5:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = "di";
                            break;

                        case 6:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("0x{0:X4}", ReadWord(cs, ip));
                            ip += 2;
                            break;

                        case 7:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = "bx";
                            break;
                    }
                    break;

                case 0x40:

                    // Mode 1 (1 byte displacement)
                    displacement = FormatDisplacement(ReadByte(cs, ip++));

                    // Mode 1
                    switch (_modRM & 0x7)
                    {
                        case 0:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("bx+si{0}", displacement);
                            break;

                        case 1:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("bx+di{0}", displacement);
                            break;

                        case 2:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = string.Format("bp+si{0}", displacement);
                            break;

                        case 3:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = string.Format("bp+di{0}", displacement);
                            break;

                        case 4:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("si{0}", displacement);
                            break;

                        case 5:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("di{0}", displacement);
                            break;

                        case 6:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = string.Format("bp{0}", displacement);
                            break;

                        case 7:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("bx{0}", displacement);
                            break;
                    }
                    break;

                case 0x80:
                    // Mode 2 (2 byte displacement)
                    displacement = FormatDisplacement(ReadWord(cs, ip));
                    ip += 2;

                    // Mode 1
                    switch (_modRM & 0x7)
                    {
                        case 0:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("bx+si{0}", displacement);
                            break;

                        case 1:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("bx+di{0}", displacement);
                            break;

                        case 2:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = string.Format("bp+si{0}", displacement);
                            break;

                        case 3:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = string.Format("bp+di{0}", displacement);
                            break;

                        case 4:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("si{0}", displacement);
                            break;

                        case 5:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("di{0}", displacement);
                            break;

                        case 6:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.SS);
                            _modRMOffset = string.Format("bp{0}", displacement);
                            break;

                        case 7:
                            _modRMSeg = ResolveSegmentPtr(RegSeg.DS);
                            _modRMOffset = string.Format("bx{0}", displacement);
                            break;
                    }
                    break;

                case 0xC0:
                    // Mode 3 (Register)
                    _modRMIsPointer = false;
                    break;
            }
        }

        string Format(Reg8 reg)
        {
            switch (reg)
            {
                case Reg8.AH: return "ah";
                case Reg8.AL: return "al";
                case Reg8.BH: return "bh";
                case Reg8.BL: return "bl";
                case Reg8.CH: return "ch";
                case Reg8.CL: return "cl";
                case Reg8.DH: return "dh";
                case Reg8.DL: return "dl";
            }

            throw new NotImplementedException();
        }

        string Format(Reg16 reg)
        {
            switch (reg)
            {
                case Reg16.AX: return "ax";
                case Reg16.BX: return "bx";
                case Reg16.CX: return "cx";
                case Reg16.DX: return "dx";
                case Reg16.SI: return "si";
                case Reg16.DI: return "di";
                case Reg16.SP: return "sp";
                case Reg16.BP: return "bp";
            }
            throw new NotImplementedException();
        }

        string Format(RegSeg reg)
        {
            switch (reg)
            {
                case RegSeg.CS: return "cs";
                case RegSeg.DS: return "ds";
                case RegSeg.ES: return "es";
                case RegSeg.SS: return "ss";
            }
            throw new NotImplementedException();
        }

        static string[] _ccNames = new string[]
        {
            "o", "no", "b", "nb", "z", "nz", "be", "a",
            "s", "ns", "p", "np", "l", "ge", "le", "g",
        };

        string Read_Eb()
        {
            if (!_haveReadModRM)
                ReadModRM();

            if (_modRMIsPointer)
            {
                return string.Format("byte ptr {0}[{1}]", _modRMSeg, _modRMOffset);
            }
            else
            {
                return Format((Reg8)(_modRM & 0x07));
            }
        }

        string Read_Ev()
        {
            if (!_haveReadModRM)
                ReadModRM();

            if (_modRMIsPointer)
            {
                return string.Format("word ptr {0}[{1}]", _modRMSeg, _modRMOffset);
            }
            else
            {
                return Format((Reg16)(_modRM & 0x07));
            }
        }

        string Read_Gb()
        {
            if (!_haveReadModRM)
                ReadModRM();

            return Format((Reg8)((_modRM >> 3) & 0x07));
        }

        string Read_Gv()
        {
            if (!_haveReadModRM)
                ReadModRM();

            return Format((Reg16)((_modRM >> 3) & 0x07));
        }

        string Read_Sv()
        {
            if (!_haveReadModRM)
                ReadModRM();

            return Format((RegSeg)((_modRM >> 3) & 0x07));
        }

        string Read_Ib()
        {
            return string.Format("0x{0:X2}", ReadByte(cs, ip++));
        }

        string Read_Ib_sx()
        {
            return string.Format("0x{0:X4}", unchecked((ushort)(sbyte)ReadByte(cs, ip++)));
        }

        string Read_Iv()
        {
            var val = ReadWord(cs, ip);
            ip += 2;
            return string.Format("0x{0:X4}", val);
        }

        string Read_Jb()
        {
            byte offset = ReadByte(cs, ip++);
            return string.Format("0x{0:X4}", (ushort)(ip + (ushort)(sbyte)offset), FormatDisplacement(offset));
        }

        string Read_Jv()
        {
            ushort offset = ReadWord(cs, ip);
            ip += 2;
            return string.Format("0x{0:X4}", (ushort)(ip + (ushort)(short)offset), FormatDisplacement(offset));
        }

        string ResolveSegmentPtr(RegSeg seg)
        {
            if (_prefixSegment == RegSeg.None)
                return "";

            return Format(_prefixSegment) + ":";
        }

        string FormatAluOp(string op, byte mode)
        {
            switch (mode & 0x07)
            {
                case 0x00:
                    return string.Format("{0} {1},{2}", op, Read_Eb(), Read_Gb());

                case 0x01:
                    return string.Format("{0} {1},{2}", op, Read_Ev(), Read_Gv());

                case 0x02:
                    return string.Format("{0} {1},{2}", op, Read_Gb(), Read_Eb());

                case 0x03:
                    return string.Format("{0} {1},{2}", op, Read_Gv(), Read_Ev());
                    
                case 0x04:
                    return string.Format("{0} {1},{2}", op, "al", Read_Ib());

                case 0x05:
                    return string.Format("{0} {1},{2}", op, "ax", Read_Iv());
            }

            throw new NotImplementedException();
        }

        string RepPrefix()
        {
            if (_prefixRepEither)
                return "rep ";
            else
                return "";
        }

        string RepCCPrefix()
        {
            if (_prefixRepEither)
            {
                return _prefixRepNE ? "repne " : "repe ";
            }
            return "";
        }

        string Group1Name(int subCode)
        {
            switch (subCode & 0x07)
            {
                case 0: return "add"; 
                case 1: return "or"; 
                case 2: return "adc";
                case 3: return "sbb";
                case 4: return "and";
                case 5: return "sub";
                case 6: return "xor";
                case 7: return "cmp";
            }
            throw new NotImplementedException();
        }

        string Group2Name(int subCode)
        {
            switch (subCode & 0x07)
            {
                case 0: return "rol";
                case 1: return "ror";
                case 2: return "rcl";
                case 3: return "rcr";
                case 4: return "shl";
                case 5: return "shr";
                case 7: return "sar";
            }
            throw new NotImplementedException();
        }

        string Group3Name(int subCode)
        {
            switch ((_modRM >> 3) & 0x07)
            {
                case 0: return "test";
                case 1: throw new NotImplementedException();
                case 2: return "not";
                case 3: return "neg"; 
                case 4: return "mul"; 
                case 5: return "imul";
                case 6: return "div";
                case 7: return "idiv";
            }
            throw new NotImplementedException();
        }

        public string Read(ushort csIn, ushort ipIn)
        {
            cs = csIn;
            ip = ipIn;
            return Read();
        }

        public string ImplicitParams;

        public string Read()
        {
            ImplicitParams = null;
            IsCall = false;
            ushort ipInstruction = ip;
            try
            {
                // Decode prefixes
                _prefixRepEither = false;
                _prefixRepNE = false;

                _prefixSegment = RegSeg.None;
                _haveReadModRM = false;

                ushort temp;

                byte opCode = ReadByte(cs, ip++);
                bool haveOpCode = false;
                while (!haveOpCode)
                {
                    switch (opCode)
                    {
                        case 0xF3:
                            _prefixRepEither = true;
                            _prefixRepNE = false;
                            opCode = ReadByte(cs, ip++);
                            break;

                        case 0xF2:
                            _prefixRepEither = true;
                            _prefixRepNE = true;
                            opCode = ReadByte(cs, ip++);
                            break;

                        case 0xF0:
                            opCode = ReadByte(cs, ip++);
                            break;

                        case 0x2E:
                            _prefixSegment = RegSeg.CS;
                            opCode = ReadByte(cs, ip++);
                            break;

                        case 0x3E:
                            _prefixSegment = RegSeg.DS;
                            opCode = ReadByte(cs, ip++);
                            break;

                        case 0x26:
                            _prefixSegment = RegSeg.ES;
                            opCode = ReadByte(cs, ip++);
                            break;

                        case 0x36:
                            _prefixSegment = RegSeg.SS;
                            opCode = ReadByte(cs, ip++);
                            break;

                        default:
                            haveOpCode = true;
                            break;
                    }
                }

                switch (opCode)
                {
                    case 0x00:
                    case 0x01:
                    case 0x02:
                    case 0x03:
                    case 0x04:
                    case 0x05:
                        return FormatAluOp("add", opCode);

                    case 0x06:
                    case 0x0E:
                    case 0x16:
                    case 0x1E:
                        ImplicitParams = "sp";
                        return string.Format("push {0}", Format((RegSeg)((opCode >> 3) & 0x03)));

                    case 0x07:
                    case 0x17:
                    case 0x1F:
                        ImplicitParams = "word ptr ss:[sp]";
                        return string.Format("pop {0}", Format((RegSeg)((opCode >> 3) & 0x03)));

                    case 0x0F:
                        // No pop cs instruction
                        throw new InvalidOpCodeException();

                    case 0x08:
                    case 0x09:
                    case 0x0A:
                    case 0x0B:
                    case 0x0C:
                    case 0x0D:
                        return FormatAluOp("or", opCode);

                    case 0x10:
                    case 0x11:
                    case 0x12:
                    case 0x13:
                    case 0x14:
                    case 0x15:
                        return FormatAluOp("adc", opCode);

                    case 0x18:
                    case 0x19:
                    case 0x1A:
                    case 0x1B:
                    case 0x1C:
                    case 0x1D:
                        return FormatAluOp("sbb", opCode);

                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x25:
                        return FormatAluOp("and", opCode);

                    case 0x26:
                        // ES: prefix
                        throw new InvalidOpCodeException();

                    case 0x27:
                        return "daa";

                    case 0x28:
                    case 0x29:
                    case 0x2A:
                    case 0x2B:
                    case 0x2C:
                    case 0x2D:
                        return FormatAluOp("sub", opCode);

                    case 0x2E:
                        // CS:
                        throw new InvalidOpCodeException();

                    case 0x2F:
                        return "das";

                    case 0x30:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                    case 0x34:
                    case 0x35:
                        return FormatAluOp("xor", opCode);

                    case 0x36:
                        // SS: prefix
                        throw new InvalidOpCodeException();

                    case 0x37:
                        return "aaa";

                    case 0x38:
                    case 0x39:
                    case 0x3A:
                    case 0x3B:
                    case 0x3C:
                    case 0x3D:
                        return FormatAluOp("cmp", opCode);

                    case 0x3E:
                        // DS:
                        throw new InvalidOpCodeException();

                    case 0x3F:
                        // AAS
                        return "aas";

                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                    case 0x44:
                    case 0x45:
                    case 0x46:
                    case 0x47:
                        return string.Format("inc {0}", Format((Reg16)(opCode & 0x07)));

                    case 0x48:
                    case 0x49:
                    case 0x4A:
                    case 0x4B:
                    case 0x4C:
                    case 0x4D:
                    case 0x4E:
                    case 0x4F:
                        return string.Format("dec {0}", Format((Reg16)(opCode & 0x07)));

                    case 0x50:
                    case 0x51:
                    case 0x52:
                    case 0x53:
                    case 0x54:
                    case 0x55:
                    case 0x56:
                    case 0x57:
                        ImplicitParams = "sp";
                        return string.Format("push {0}", Format((Reg16)(opCode & 0x07)));

                    case 0x58:
                    case 0x59:
                    case 0x5A:
                    case 0x5B:
                    case 0x5C:
                    case 0x5D:
                    case 0x5E:
                    case 0x5F:
                        ImplicitParams = "word ptr ss:[sp]";
                        return string.Format("pop {0}", Format((Reg16)(opCode & 0x07)));

                    case 0x60:
                        ImplicitParams = "sp";
                        return "pusha";

                    case 0x61:
                        ImplicitParams = "sp";
                        return "popa";

                    case 0x62:
                        // BOUND r16,m16
                        ReadModRM();
                        if (!_modRMIsPointer)
                            throw new InvalidOpCodeException();
                        return string.Format("Bound {0},{1}", Read_Gv(), Read_Ev());

                    case 0x63:
                    case 0x64:
                    case 0x65:
                    case 0x66:
                    case 0x67:
                        throw new InvalidOpCodeException();

                    case 0x68:
                        // Push Iv
                        ImplicitParams = "sp";
                        return string.Format("push {0:X4}", Read_Iv());

                    case 0x69:
                        ReadModRM();
                        return string.Format("imul {0},{1},{2}", Read_Gv(), Read_Ev(), Read_Iv());

                    case 0x6A:
                        // PUSH Ib
                        ImplicitParams = "sp";
                        return string.Format("push byte {0}", Read_Ib_sx());

                    case 0x6B:
                        // imul Gv,Ev,Ib
                        ReadModRM();
                        return string.Format("imul {0},{1},{2}", Read_Gv(), Read_Ev(), Read_Ib_sx());

                    case 0x6C:
                        // INSB
                        ImplicitParams = "es:di";
                        return string.Format("{0} ins", RepPrefix());

                    case 0x6D:
                        // INSW
                        ImplicitParams = "es:di";
                        return string.Format("{0} insw", RepPrefix());

                    case 0x6E:
                        // OUTSB
                        ImplicitParams = "byte ptr ds:[si]";
                        return string.Format("{0} outsb", RepPrefix());

                    case 0x6F:
                        // OUTSW
                        ImplicitParams = "word ptr ds:[si]";
                        return string.Format("{0} outsw", RepPrefix());

                    case 0x70:
                    case 0x71:
                    case 0x72:
                    case 0x73:
                    case 0x74:
                    case 0x75:
                    case 0x76:
                    case 0x77:
                    case 0x78:
                    case 0x79:
                    case 0x7A:
                    case 0x7B:
                    case 0x7C:
                    case 0x7D:
                    case 0x7E:
                    case 0x7F:
                        return string.Format("j{0} {1}", _ccNames[opCode & 0x0f], Read_Jb());

                    case 0x80:
                    case 0x82:
                        // GRP1 Eb Ib
                        ReadModRM();
                        return string.Format("{0} {1},{2}", Group1Name(_modRM >> 3), Read_Eb(), Read_Ib());

                    case 0x81:
                        // GRP1 Ev Iv 
                        ReadModRM();
                        return string.Format("{0} {1},{2}", Group1Name(_modRM >> 3), Read_Ev(), Read_Iv());

                    case 0x83:
                        // GRP1 Ev Ib 
                        ReadModRM();
                        return string.Format("{0} {1},{2}", Group1Name(_modRM >> 3), Read_Ev(), Read_Ib_sx());

                    case 0x84:
                        // Test Gb, Eb
                        return string.Format("test {0},{1}", Read_Gb(), Read_Eb());

                    case 0x85:
                        // Test Gv, Ev
                        return string.Format("and {0},{1}", Read_Gv(), Read_Ev());

                    case 0x86:
                        // XCHG Gb, Eb
                        return string.Format("xchg {0},{1}", Read_Gb(), Read_Eb());

                    case 0x87:
                        // XCHG Gv, Ev
                        return string.Format("xchg {0},{1}", Read_Gv(), Read_Ev());

                    case 0x88:
                        // MOV Eb, Gb 
                        return string.Format("mov {0},{1}", Read_Eb(), Read_Gb());

                    case 0x89:
                        // MOV Ev, Gv
                        return string.Format("mov {0},{1}", Read_Ev(), Read_Gv());

                    case 0x8A:
                        // MOV Gb, Eb
                        return string.Format("mov {0},{1}", Read_Gb(), Read_Eb());

                    case 0x8B:
                        // MOV Gv Ev
                        return string.Format("mov {0},{1}", Read_Gv(), Read_Ev());

                    case 0x8C:
                        // MOV Ew Sw
                        return string.Format("mov {0},{1}", Read_Ev(), Read_Sv());

                    case 0x8D:
                        // LEA Gv M
                        ReadModRM();
                        if (!_modRMIsPointer)
                            throw new InvalidOpCodeException();
                        return string.Format("lea {0},{1}", Read_Gv(), Read_Eb());

                    case 0x8E:
                        // MOV Sw Ew
                        return string.Format("mov {0},{1}", Read_Sv(), Read_Ev());

                    case 0x8F:
                        // POP Ev
                        ImplicitParams = "word ptr ss:[sp]";
                        return string.Format("pop {0}", Read_Ev());

                    case 0x90:
                        // NOP
                        return "nop";

                    case 0x91:
                    case 0x92:
                    case 0x93:
                    case 0x94:
                    case 0x95:
                    case 0x96:
                    case 0x97:
                        return string.Format("xchg ax,{0}", Format((Reg16)(opCode & 0x07)));

                    case 0x98:
                        return "cbw";

                    case 0x99:
                        return "cwd";

                    case 0x9A:
                    {
                        // Read target address
                        temp = ReadWord(cs, ip);
                        ip += 2;

                        var newcs = ReadWord(cs, ip);
                        ip += 2;

                        IsCall = true;
                        ImplicitParams = "sp,bp";
                        return string.Format("call 0x{0:X4}:0x{1:X4}", newcs, temp);
                    }

                    case 0x9B:
                        return "wait";

                    case 0x9C:
                        ImplicitParams = "sp";
                        return "pushf";

                    case 0x9D:
                        ImplicitParams = "sp";
                        return "popf";

                    case 0x9E:
                        return "sahf";

                    case 0x9F:
                        return "lahf";

                    case 0xA0:
                        // MOV al, [Ob]
                        temp = ReadWord(cs, ip);
                        ip += 2;
                        return string.Format("mov al,byte ptr {0}[0x{1:X4}]", ResolveSegmentPtr(RegSeg.DS), temp);

                    case 0xA1:
                        // MOV ax, [Ov]
                        temp = ReadWord(cs, ip);
                        ip += 2;
                        return string.Format("mov ax,word ptr {0}[0x{1:X4}]", ResolveSegmentPtr(RegSeg.DS), temp);

                    case 0xA2:
                        // MOV [Ob], al
                        temp = ReadWord(cs, ip);
                        ip += 2;
                        return string.Format("mov byte ptr {0}[0x{1:X4}],al", ResolveSegmentPtr(RegSeg.DS), temp);

                    case 0xA3:
                        // MOV [Ob], ax
                        temp = ReadWord(cs, ip);
                        ip += 2;
                        return string.Format("mov word ptr {0}[0x{1:X4}],ax", ResolveSegmentPtr(RegSeg.DS), temp);

                    case 0xA4:
                        // MOVSB
                        if (_prefixSegment != RegSeg.None)
                        {
                            ImplicitParams = "es:di";
                            return string.Format("{0}movsb byte ptr {1}[si]", RepPrefix(), ResolveSegmentPtr(RegSeg.DS));
                        }
                        else
                        {
                            ImplicitParams = "ds:si,es:di";
                            return string.Format("{0}movsb", RepPrefix());
                        }


                    case 0xA5:
                        // MOVSW
                        if (_prefixSegment != RegSeg.None)
                        {
                            ImplicitParams = "es:di";
                            return string.Format("{0}movsw word ptr {1}[si]", RepPrefix(), ResolveSegmentPtr(RegSeg.DS));
                        }
                        else
                        {
                            ImplicitParams = "ds:si,es:di";
                            return string.Format("{0}movsw", RepPrefix());
                        }

                    case 0xA6:
                        // CMPSB
                        if (_prefixSegment != RegSeg.None)
                        {
                            ImplicitParams = "es:di";
                            return string.Format("{0}cmpsb byte ptr {1}[si]", RepPrefix(), ResolveSegmentPtr(RegSeg.DS));
                        }
                        else
                        {
                            ImplicitParams = "ds:si,es:di";
                            return string.Format("{0}cmpsb", RepPrefix());
                        }

                    case 0xA7:
                        // CMPSW
                        if (_prefixSegment != RegSeg.None)
                        {
                            ImplicitParams = "es:di";
                            return string.Format("{0}cmpsw word ptr {1}[si]", RepPrefix(), ResolveSegmentPtr(RegSeg.DS));
                        }
                        else
                        {
                            ImplicitParams = "word ptr ds:[si],es:di";
                            return string.Format("{0}cmpsw", RepPrefix());
                        }

                    case 0xA8:
                        // TEST AL Ib 
                        return string.Format("test al,{0}", Read_Ib());

                    case 0xA9:
                        // TEST eAX Iv
                        return string.Format("test ax,{0}", Read_Iv());

                    case 0xAA:
                        // STOSB
                        return string.Format("{0}stosb byte ptr es:[di]", RepPrefix());

                    case 0xAB:
                        // STOSW
                        return string.Format("{0}stosw word ptr es:[di]", RepPrefix());

                    case 0xAC:
                        // LODSB
                        return string.Format("{0}lodsb byte ptr {1}:[si]", RepPrefix(), ResolveSegmentPtr(RegSeg.DS));

                    case 0xAD:
                        // LODSW
                        return string.Format("{0}lodsw word ptr {1}:[si]", RepPrefix(), ResolveSegmentPtr(RegSeg.DS));

                    case 0xAE:
                        // SCASB
                        return string.Format("{0}scasb byte ptr es:[di]", RepCCPrefix());

                    case 0xAF:
                        // SCASW
                        return string.Format("{0}scasw word ptr es:[di]", RepCCPrefix());

                    case 0xB0:
                    case 0xB1:
                    case 0xB2:
                    case 0xB3:
                    case 0xB4:
                    case 0xB5:
                    case 0xB6:
                    case 0xB7:
                        return string.Format("mov {0},{1}", Format((Reg8)(opCode & 0x07)), Read_Ib());

                    case 0xB8:
                    case 0xB9:
                    case 0xBA:
                    case 0xBB:
                    case 0xBC:
                    case 0xBD:
                    case 0xBE:
                    case 0xBF:
                        return string.Format("mov {0},{1}", Format((Reg16)(opCode & 0x07)), Read_Iv());

                    case 0xC0:
                        // GRP2 Eb, Ib
                        ReadModRM();
                        return string.Format("{0} {1},{2}", Group2Name((_modRM >> 3) & 0x07), Read_Eb(), Read_Ib());

                    case 0xC1:
                        // GRP2 Ev, Ib
                        ReadModRM();
                        return string.Format("{0} {1},{2}", Group2Name((_modRM >> 3) & 0x07), Read_Ev(), Read_Ib());

                    case 0xC2:
                        // RET Iw
                        ImplicitParams = "sp,bp";
                        return string.Format("ret {0}", Read_Iv());

                    case 0xC3:
                        // RET
                        ImplicitParams = "sp,bp";
                        return "ret";

                    case 0xC4:
                        // LES Gv Mp
                        ReadModRM();
                        if (!_modRMIsPointer)
                            throw new InvalidOpCodeException();

                        return string.Format("les {0},d{1}", Read_Gv(), Read_Ev());

                    case 0xC5:
                        // LDS Gv Mp
                        ReadModRM();
                        if (!_modRMIsPointer)
                            throw new InvalidOpCodeException();

                        return string.Format("lds {0},d{1}", Read_Gv(), Read_Ev());

                    case 0xC6:
                        // MOV Eb Ib
                        ReadModRM();
                        return string.Format("mov {0},{1}", Read_Eb(), Read_Ib());

                    case 0xC7:
                        // MOV Ev Iv
                        ReadModRM();
                        return string.Format("mov {0},{1}", Read_Ev(), Read_Iv());

                    case 0xC8:
                        // ENTER Iw, Ib
                        string storage = Read_Iv();
                        string nestingLevel = Read_Ib();
                        return string.Format("enter {0},{1}", storage, nestingLevel);

                    case 0xC9:
                        // LEAVE
                        return "leave";

                    case 0xCA:
                        // RETF Iv
                        ImplicitParams = "sp,bp";
                        return string.Format("retf {0}", Read_Iv());

                    case 0xCB:
                        // RETF
                        ImplicitParams = "sp,bp";
                        return "retf";

                    case 0xCC:
                        // INT 3
                        return "int 3";

                    case 0xCD:
                        // Int Ib
                        return string.Format("int {0}", Read_Ib());

                    case 0xCE:
                        // INTO
                        return "into";

                    case 0xCF:
                        // IRET
                        return "iret";

                    case 0xD0:
                        // GRP2 Eb 1 
                        ReadModRM();
                        return string.Format("{0} {1},1", Group2Name(_modRM >> 3), Read_Eb()); 

                    case 0xD1:
                        // GRP2 Ev 1 
                        ReadModRM();
                        return string.Format("{0} {1},1", Group2Name(_modRM >> 3), Read_Ev());

                    case 0xD2:
                        // GRP2 Eb CL
                        ReadModRM();
                        return string.Format("{0} {1},cl", Group2Name(_modRM >> 3), Read_Eb());

                    case 0xD3:
                        // GRP2 Ev CL
                        ReadModRM();
                        return string.Format("{0} {1},cl", Group2Name(_modRM >> 3), Read_Ev());

                    case 0xD4:
                        // AAM I0 
                        return string.Format("aam {0}", Read_Ib());

                    case 0xD5:
                        // AAD I0
                        return string.Format("aad {0}", Read_Ib());

                    case 0xD6: 
                        // -
                        throw new InvalidOpCodeException();

                    case 0xD7:
                        // XLAT
                        return "xlat";

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
                        return string.Format("loopnz {0}", Read_Jb());
                    case 0xE1:
                        // LOOPZ Jb
                        return string.Format("loopz {0}", Read_Jb());

                    case 0xE2:
                        // LOOP Jb
                        return string.Format("loop {0}", Read_Jb());

                    case 0xE3:
                        // JCXZ Jb
                        return string.Format("jcxz {0}", Read_Jb());

                    case 0xE4:
                        // IN AL Ib
                        return string.Format("in al,{0}", Read_Ib());

                    case 0xE5:
                        // IN eAX Ib
                        return string.Format("in ax,{0}", Read_Ib());

                    case 0xE6:
                        // OUT Ib AL
                        return string.Format("out {0},al", Read_Ib());

                    case 0xE7:
                        // OUT Ib eAX
                        return string.Format("out {0},ax", Read_Ib());

                    case 0xE8:
                        // CALL Jv
                        IsCall = true;
                        ImplicitParams = "sp,bp";
                        return string.Format("call {0}", Read_Jv());

                    case 0xE9:
                        // JMP Jv
                        return string.Format("jmp {0}", Read_Jv());

                    case 0xEA:
                    {
                        // JMP Ap
                        var strIp= Read_Iv();
                        return string.Format("jmp {0}:{1}", Read_Iv(), strIp);
                    }

                    case 0xEB:
                        // JMP Jb
                        return string.Format("jmp {0}", Read_Jb());

                    case 0xEC:
                        // IN AL DX
                        return "in al,dx";

                    case 0xED:
                        // IN eAX DX
                        return "in ax,dx";

                    case 0xEE:
                        // OUT DX AL
                        return "out dx,al";

                    case 0xEF:
                        // OUT DX eAX
                        return "out dx,ax";

                    case 0xF0:
                        // LOCK (Ignore)
                        throw new InvalidOpCodeException();

                    case 0xF1: 
                        // -
                        throw new InvalidOpCodeException();

                    case 0xF2:
                        // REPNZ (Prefix)
                        throw new InvalidOpCodeException();

                    case 0xF3: 
                        // REPZ (Prefix)
                        throw new InvalidOpCodeException();

                    case 0xF4:
                        // HLT
                        return "hlt";

                    case 0xF5:
                        // CMC
                        return "cmc";

                    case 0xF6:
                        // GRP3a Eb
                        ReadModRM();
                        switch ((_modRM >> 3) & 0x07)
                        {
                            case 0:
                                return string.Format("{0} {1},{2}", Group3Name((_modRM >> 3) & 0x07), Read_Eb(), Read_Ib());

                            default:
                                return string.Format("{0} {1}", Group3Name((_modRM >> 3) & 0x07), Read_Eb());
                        }

                    case 0xF7:
                        // GRP3b Ev
                        ReadModRM();
                        switch ((_modRM >> 3) & 0x07)
                        {
                            case 0:
                                return string.Format("{0} {1},{2}", Group3Name((_modRM >> 3) & 0x07), Read_Ev(), Read_Iv());

                            default:
                                return string.Format("{0} {1}", Group3Name((_modRM >> 3) & 0x07), Read_Ev());
                        }

                    case 0xF8:
                        return "clc";

                    case 0xF9:
                        return "stc";

                    case 0xFA:
                        return "cli";

                    case 0xFB:
                        return "sti";

                    case 0xFC:
                        return "cld";

                    case 0xFD:
                        return "std";

                    case 0xFE:
                        // GRP4 Eb
                        ReadModRM();
                        switch ((_modRM >> 3) & 0x07)
                        {
                            case 0: return string.Format("inc {0}", Read_Eb());
                            case 1: return string.Format("dec {0}", Read_Eb());
                            default:
                                throw new InvalidOpCodeException();
                        }

                    case 0xFF:
                        // GRP5 Ev
                        ReadModRM();
                        switch ((_modRM >> 3) & 0x07)
                        {
                            case 0: return string.Format("inc {0}", Read_Ev());
                            case 1: return string.Format("dec {0}", Read_Ev());
                            case 2:
                            {
                                IsCall = true;
                                ImplicitParams = "sp,bp";
                                return string.Format("call {0}", Read_Ev());
                            }
                            case 3: 
                            {
                                if (!_modRMIsPointer)
                                    throw new InvalidOpCodeException();
                                IsCall = true;
                                ImplicitParams = "sp,bp";
                                return string.Format("call d{0}", Read_Ev());
                            }
                            case 4: return string.Format("jmp {0}", Read_Ev());
                            case 5:
                            {
                                if (!_modRMIsPointer)
                                    throw new InvalidOpCodeException();
                                return string.Format("jmp d{0}", Read_Ev());
                            }

                            case 6:
                                ImplicitParams = "sp";
                                return string.Format("push {0}", Read_Ev());

                            case 7: throw new InvalidOpCodeException();
                        }
                        break;
                }

                throw new NotImplementedException();
            }
            catch (NotImplementedException)
            {
                ip = (ushort)(ipInstruction + 1);
                return "??";
            }
            catch (CPUException)
            {
                ip = (ushort)(ipInstruction + 1);
                return "??";
            }
        }
    }
}
