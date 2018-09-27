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
    public class EFlag
    {
        // Flag bits
        public const uint CF = 1 << 0;              // 0x0001
        public const uint PF = 1 << 2;              // 0x0004
        public const uint AF = 1 << 4;              // 0x0010
        public const uint ZF = 1 << 6;              // 0x0040
        public const uint SF = 1 << 7;              // 0x0080
        public const uint TF = 1 << 8;              // 0x0100
        public const uint IF = 1 << 9;              // 0x0200
        public const uint DF = 1 << 10;             // 0x0400
        public const uint OF = 1 << 11;             // 0x0800

        //FFFF => 4ed7 = ? OF|DF|IF | SF|ZF|AF | PF|FIXED|CF
        //0    => 0202 =         IF                 FIXED

        // On 8086 these bits are always one
        public const uint SupportedBits = CF | PF | AF | ZF | SF | TF | IF | DF | OF;
        public const uint FixedBits = 1 << 1;// | 1 << 12 | 1 << 13 | 1 << 14 | 1 << 15;
    }

    public class ALU
    {
        uint _aluResult;
        uint _aluOperA;
        uint _aluOperB;

        public ushort EFlags
        {
            get
            {
                return (ushort)(
                    EFlag.FixedBits |
                    (FlagC ? EFlag.CF : 0) |
                    (FlagP ? EFlag.PF : 0) |
                    (FlagA ? EFlag.AF : 0) |
                    (FlagZ ? EFlag.ZF : 0) |
                    (FlagS ? EFlag.SF : 0) |
                    (FlagT ? EFlag.TF : 0) |
                    (FlagD ? EFlag.DF : 0) |
                    (FlagI ? EFlag.IF : 0) |
                    (FlagO ? EFlag.OF : 0)
                    );
            }
            set
            {
                FlagC = (value & EFlag.CF) != 0;
                FlagP = (value & EFlag.PF) != 0;
                FlagA = (value & EFlag.AF) != 0;
                FlagZ = (value & EFlag.ZF) != 0;
                FlagS = (value & EFlag.SF) != 0;
                FlagT = (value & EFlag.TF) != 0;
                FlagD = (value & EFlag.DF) != 0;
                FlagI = (value & EFlag.IF) != 0;
                FlagO = (value & EFlag.OF) != 0;
            }
        }

        public byte Flags8
        {
            get
            {
                unchecked
                {
                    return (byte)(
                        EFlag.FixedBits |
                        (FlagC ? EFlag.CF : 0) |
                        (FlagP ? EFlag.PF : 0) |
                        (FlagA ? EFlag.AF : 0) |
                        (FlagZ ? EFlag.ZF : 0) |
                        (FlagS ? EFlag.SF : 0)
                        );
                }
            }
            set
            {
                FlagC = (value & EFlag.CF) != 0;
                FlagP = (value & EFlag.PF) != 0;
                FlagA = (value & EFlag.AF) != 0;
                FlagZ = (value & EFlag.ZF) != 0;
                FlagS = (value & EFlag.SF) != 0;
            }
        }

        [Flags]
        enum fm
        {
            Bit8        = 0x0000001,        // Last operation was 8 bit
            ZFromResult = 0x0000002,        // Get the Z flag from result
            ZFlag       = 0x0000004,        // Otherwise from this bit
            CFromResult = 0x0000008,        // Get the C flag from result
            CFlag       = 0x0000010,        // Otherwise from this bit
            PFromResult = 0x0000020,        // Parity flag from result
            PFlag       = 0x0000040,        // Otherwise from this bit
            SFromResult = 0x0000080,        // Sign flag from result
            SFlag       = 0x0000100,        // Otherwise from this bit
            OFromAdd    = 0x0000200,        // Overflow flag from result/opera/operb   (by add)
            OFromSub    = 0x0000400,        // Overflow flag from result/opera/operb   (by sub)          
            OFlag       = 0x0000800,        // O flag value
            AFromResult = 0x0001000,        // Overflow flag from result/opera/operb
            AFlag       = 0x0002000,        // A flag value
        }

        fm _fm;

        public bool FlagO
        {
            get
            {
                if ((_fm & fm.OFromAdd) != 0)
                {
                    if ((_fm & fm.Bit8)!=0)
                    {
                        return ((_aluResult ^ _aluOperA) & (_aluResult ^ _aluOperB) & 0x80) != 0;
                    }
                    else
                    {
                        return ((_aluResult ^ _aluOperA) & (_aluResult ^ _aluOperB) & 0x8000) != 0;
                    }
                }
                else if ((_fm & fm.OFromSub) != 0)
                {
                    if ((_fm & fm.Bit8) != 0)
                    {
                        return ((_aluResult ^ _aluOperA) & (_aluOperA ^ _aluOperB) & 0x80) != 0;
                    }
                    else
                    {
                        return ((_aluResult ^ _aluOperA) & (_aluOperA ^ _aluOperB) & 0x8000) != 0;
                    }
                }
                else
                {
                    return (_fm & fm.OFlag)!= 0;
                }
            }
            set
            {
                if (value)
                {
                    _fm |= fm.OFlag;
                    _fm &= ~(fm.OFromAdd | fm.OFromSub);
                }
                else
                {
                    _fm &= ~(fm.OFlag | fm.OFromAdd | fm.OFromSub);
                }
            }

        }
        public bool FlagA
        {
            get
            {
                if ((_fm & fm.AFromResult) != 0)
                {
                    return ((_aluOperA ^ _aluOperB ^ _aluResult) & 0x10) != 0;
                }
                else
                {
                    return (_fm & fm.AFlag) != 0;
                }
            }
            set
            {
                if (value)
                {
                    _fm |= fm.AFlag;
                    _fm &= ~fm.AFromResult;
                }
                else
                {
                    _fm &= ~(fm.AFlag | fm.AFromResult);
                }
            }
        }

        public bool FlagC
        {
            get
            {
                if ((_fm & fm.CFromResult)!=0)
                {
                    if ((_fm & fm.Bit8)!=0)
                         return (_aluResult & 0x100) != 0;
                    else
                        return (_aluResult & 0x10000) != 0;
                }
                else
                {
                    return (_fm & fm.CFlag)!= 0;
                }
            }
            set
            {
                if (value)
                {
                    _fm |= fm.CFlag;
                    _fm &= ~fm.CFromResult;
                }
                else
                {
                    _fm &= ~(fm.CFlag | fm.CFromResult);
                }
            }
        }

        public bool FlagZ
        {
            get
            {
                if ((_fm & fm.ZFromResult) != 0)
                {
                    if ((_fm & fm.Bit8) != 0)
                        return (_aluResult & 0xFF) == 0;
                    else
                        return (_aluResult & 0xFFFF) == 0;
                }
                else
                {
                    return (_fm & fm.ZFlag) != 0;
                }
            }
            set
            {
                if (value)
                {
                    _fm |= fm.ZFlag;
                    _fm &= ~fm.ZFromResult;
                }
                else
                {
                    _fm &= ~(fm.ZFlag | fm.ZFromResult);
                }
            }
        }

        public bool FlagS
        {
            get
            {
                if ((_fm & fm.SFromResult) != 0)
                {
                    if ((_fm & fm.Bit8) != 0)
                        return (_aluResult & 0x80) != 0;
                    else
                        return (_aluResult & 0x8000) != 0;
                }
                else
                {
                    return (_fm & fm.SFlag) != 0;
                }
            }
            set
            {
                if (value)
                {
                    _fm |= fm.SFlag;
                    _fm &= ~fm.SFromResult;
                }
                else
                {
                    _fm &= ~(fm.SFlag | fm.SFromResult);
                }
            }
        }

        public bool FlagP
        {
            get
            {
                if ((_fm & fm.PFromResult) != 0)
                {
                    return _parityTable[_aluResult & 0xFF] != 0;
                }
                else
                {
                    return (_fm & fm.PFlag) != 0;
                }
            }
            set
            {
                if (value)
                {
                    _fm |= fm.PFlag;
                    _fm &= ~fm.PFromResult;
                }
                else
                {
                    _fm &= ~(fm.PFlag | fm.PFromResult);
                }
            }
        }

        public bool FlagD;
        public bool FlagT;
        public bool FlagI = true;

        public ushort Add16(ushort a, ushort b)
        {
            unchecked
            {
                _aluResult = (uint)a + (uint)b;

                _aluOperA = a;
                _aluOperB = b;
                _fm = fm.AFromResult | fm.CFromResult | fm.OFromAdd | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (ushort)_aluResult;
            }
        }

        public byte Add8(byte a, byte b)
        {
            unchecked
            {
                _aluResult = (uint)a + (uint)b;

                _aluOperA = a;
                _aluOperB = b;
                _fm = fm.Bit8 | fm.AFromResult | fm.CFromResult | fm.OFromAdd | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (byte)_aluResult;
            }
        }

        public ushort Adc16(ushort a, ushort b)
        {
            unchecked
            {
                _aluResult = (uint)a + (uint)b + (FlagC ? 1U : 0);

                _aluOperA = a;
                _aluOperB = b;
                _fm = fm.AFromResult | fm.CFromResult | fm.OFromAdd | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (ushort)_aluResult;
            }
        }

        public byte Adc8(byte a, byte b)
        {
            unchecked
            {
                _aluResult = (uint)a + (uint)b + (FlagC ? 1U : 0);

                _aluOperA = a;
                _aluOperB = b;
                _fm = fm.Bit8 | fm.AFromResult | fm.CFromResult | fm.OFromAdd | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (byte)_aluResult;
            }
        }

        public ushort Sub16(ushort a, ushort b)
        {
            unchecked
            {
                _aluResult = ((uint)a - (uint)b) & 0x1FFFF;

                _aluOperA = a;
                _aluOperB = b;
                _fm = fm.AFromResult | fm.CFromResult | fm.OFromSub | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (ushort)_aluResult;
            }
        }

        public byte Sub8(byte a, byte b)
        {
            unchecked
            {
                _aluResult = ((uint)a - (uint)b) & 0x1FFFF;

                _aluOperA = a;
                _aluOperB = b;
                _fm = fm.Bit8 | fm.AFromResult | fm.CFromResult | fm.OFromSub | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (byte)_aluResult;
            }
        }

        public ushort Sbb16(ushort a, ushort b)
        {
            unchecked
            {
                _aluResult = ((uint)a - ((uint)b + (FlagC ? 1U : 0))) & 0x1FFFF;

                _aluOperA = a;
                _aluOperB = b;
                _fm = fm.AFromResult | fm.CFromResult | fm.OFromSub | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (ushort)_aluResult;
            }
        }

        public byte Sbb8(byte a, byte b)
        {
            unchecked
            {
                _aluResult = ((uint)a - ((uint)b + (FlagC ? 1U : 0)) & 0x1FFFF);

                _aluOperA = a;
                _aluOperB = b;
                _fm = fm.Bit8 | fm.AFromResult | fm.CFromResult | fm.OFromSub | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (byte)_aluResult;
            }
        }

        public ushort And16(ushort a, ushort b)
        {
            unchecked
            {
                _aluResult = (uint)a & (uint)b;
                _fm = fm.CFromResult | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                return (ushort)_aluResult;
            }
        }

        public byte And8(byte a, byte b)
        {
            unchecked
            {
                _aluResult = (uint)a & (uint)b;
                _fm = fm.Bit8 | fm.CFromResult | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                return (byte)_aluResult;
            }
        }

        public ushort Or16(ushort a, ushort b)
        {
            unchecked
            {
                _aluResult = (uint)a | (uint)b;
                _fm = fm.CFromResult | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                return (ushort)_aluResult;
            }
        }

        public byte Or8(byte a, byte b)
        {
            unchecked
            {
                _aluResult = (uint)a | (uint)b;
                _fm = fm.Bit8 | fm.CFromResult | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                return (byte)_aluResult;
            }
        }

        public ushort Xor16(ushort a, ushort b)
        {
            unchecked
            {
                _aluResult = (uint)a ^ (uint)b;
                _fm = fm.CFromResult | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                return (ushort)_aluResult;
            }
        }

        public byte Xor8(byte a, byte b)
        {
            unchecked
            {
                _aluResult = (uint)a ^ (uint)b;
                _fm = fm.Bit8 | fm.CFromResult | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                return (byte)_aluResult;
            }
        }

        public ushort Inc16(ushort a)
        {
            unchecked
            {
                _aluOperA = a;
                _aluOperB = 1;
                _fm = fm.AFromResult | (FlagC ? fm.CFlag : 0) | fm.OFromAdd | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                _aluResult = (uint)a + 1;
                return (ushort)_aluResult;
            }
        }

        public byte Inc8(byte a)
        {
            unchecked
            {
                _aluOperA = a;
                _aluOperB = 1;
                _fm = fm.Bit8 | fm.AFromResult | (FlagC ? fm.CFlag : 0) | fm.OFromAdd | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                _aluResult = (uint)a + 1;
                return (byte)_aluResult;
            }
        }

        public ushort Dec16(ushort a)
        {
            unchecked
            {
                _aluOperA = a;
                _aluOperB = 1;
                _fm = fm.AFromResult | (FlagC ? fm.CFlag : 0) | fm.OFromSub | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                _aluResult = (uint)a - 1;
                return (ushort)_aluResult;
            }
        }

        public byte Dec8(byte a)
        {
            unchecked
            {
                _aluOperA = a;
                _aluOperB = 1;
                _fm = fm.Bit8 | fm.AFromResult | (FlagC ? fm.CFlag : 0) | fm.OFromSub | fm.PFromResult | fm.SFromResult | fm.ZFromResult;
                _aluResult = (uint)a - 1;
                return (byte)_aluResult;
            }
        }

        public ushort Rol16(ushort a, byte b)
        {
            if (b == 0)
                return a;

            b %= 16;

            _aluResult = (uint)((a << b) | (a >> (16 - b)));

            bool o;
            if (b == 1)
                o = ((_aluResult ^ (_aluResult << 1)) & 0x10000) != 0;
            else
            {
                if (((a >> 1) & 0x4000) == (a & 0x4000))
                    o = false;
                else
                    o = true;
            }

            _fm =
                ((_aluResult & 0x0001) != 0 ? fm.CFlag : 0) |
                (o ? fm.OFlag : 0) |
                (FlagS ? fm.SFlag : 0) |
                (FlagZ ? fm.ZFlag : 0) |    
                (FlagP ? fm.PFlag : 0) |
                (FlagA ? fm.AFlag : 0);

            return (ushort)_aluResult;
        }
        
        public byte Rol8(byte a, byte b)
        {
            if (b == 0)
                return a;

            b %= 8;

            _aluResult = (uint)((a << b) | (a >> (8 - b)));

            bool o;
            if (b == 1)
                o = ((_aluResult ^ (_aluResult << 1)) & 0x100) != 0;
            else
            {
                if (((a >> 1) & 0x40) == (a & 0x40))
                    o = false;
                else
                    o = true;
            }

            _fm =
                ((_aluResult & 0x0001) != 0 ? fm.CFlag : 0) |
                (o ? fm.OFlag : 0) |
                (FlagS ? fm.SFlag : 0) |
                (FlagZ ? fm.ZFlag : 0) |
                (FlagP ? fm.PFlag : 0) |
                (FlagA ? fm.AFlag : 0);

            return (byte)_aluResult;
        }

        public ushort Ror16(ushort a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                b %= 16;

                _aluResult = (uint)((a >> b) | (a << (16 - b)));

                bool o;
                if (b == 1)
                    o = ((_aluResult ^ (_aluResult >> 1)) & 0x4000) != 0;
                else
                {
                    if ((a & 0x0001) == ((a >> 15) & 0x0001))
                        o = false;
                    else
                        o = true;
                }

                _fm = 
                    ((_aluResult & 0x8000) != 0 ? fm.CFlag : 0) |  
                    (o ? fm.OFlag : 0) |
                    (FlagS ? fm.SFlag : 0) |
                    (FlagZ ? fm.ZFlag : 0) |
                    (FlagP ? fm.PFlag : 0) |
                    (FlagA ? fm.AFlag : 0);

                return (ushort)_aluResult;
            }
        }

        public byte Ror8(byte a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                b %= 8;

                _aluResult = (uint)((a >> b) | (a << (8 - b)));

                bool o;
                if (b == 1)
                    o = ((_aluResult ^ (_aluResult >> 1)) & 0x40) != 0;
                else
                {
                    if ((a & 0x0001) == ((a >> 7) & 0x0001))
                        o = false;
                    else
                        o = true;
                }

                _fm =
                    ((_aluResult & 0x80) != 0 ? fm.CFlag : 0) |
                    (o ? fm.OFlag : 0) |
                    (FlagS ? fm.SFlag : 0) |
                    (FlagZ ? fm.ZFlag : 0) |
                    (FlagP ? fm.PFlag : 0) |
                    (FlagA ? fm.AFlag : 0);

                return (byte)_aluResult;
            }
        }

        public ushort Rcr16(ushort a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                b %= 17;

                _aluResult = (uint)((FlagC ? 0x10000 : 0) | (a & 0xFFFF));
                _aluResult = (_aluResult >> b) | (_aluResult << (17 - b));

                bool o;
                if (b == 1)
                    o = ((_aluResult ^ (_aluResult >> 1)) & 0x4000) != 0;
                else
                {
                    if ((a & 0x8000) == 0)
                        o = FlagO;
                    else
                        o = !FlagO;
                }

                _fm = fm.CFromResult | 
                    (o ? fm.OFlag : 0) |
                    (FlagS ? fm.SFlag : 0) |
                    (FlagZ ? fm.ZFlag : 0) | 
                    (FlagP ? fm.PFlag : 0) | 
                    (FlagA ? fm.AFlag : 0);

                return (ushort)_aluResult;
            }
        }

        public ushort Rcl16(ushort a, byte b)
        {
            if (b == 0)
                return a;

            b %= 17;

            _aluResult = (uint)((FlagC ? 0x10000 : 0) | (a & 0xFFFF));
            _aluResult = (_aluResult << b) | (_aluResult >> (17 - b));

            bool o;
            if (b == 1)
                o = ((_aluResult ^ (_aluResult << 1)) & 0x10000) != 0;
            else
            {
                if (((a>>1) & 0x4000) == (a & 0x4000))
                    o = false;
                else
                    o = true;
            }

            _fm = fm.CFromResult |
                (o ? fm.OFlag : 0) |
                (FlagS ? fm.SFlag : 0) |
                (FlagZ ? fm.ZFlag : 0) |
                (FlagP ? fm.PFlag : 0) |
                (FlagA ? fm.AFlag : 0);

            return (ushort)_aluResult;
        }

        public byte Rcl8(byte a, byte b)
        {
            if (b == 0)
                return a;

            b %= 9;

            _aluResult = (uint)((FlagC ? 0x100 : 0) | (a & 0xFF));
            _aluResult = (_aluResult << b) | (_aluResult >> (9 - b));

            bool o;
            if (b == 1)
                o = ((_aluResult ^ (_aluResult << 1)) & 0x100) != 0;
            else
            {
                if (((a >> 1) & 0x40) == (a & 0x40))
                    o = false;
                else
                    o = true;
            }

            _fm = fm.Bit8 | fm.CFromResult |
                (o ? fm.OFlag : 0) |
                (FlagS ? fm.SFlag : 0) |
                (FlagZ ? fm.ZFlag : 0) |
                (FlagP ? fm.PFlag : 0) |
                (FlagA ? fm.AFlag : 0);

            return (byte)_aluResult;
        }

        public byte Rcr8(byte a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                b %= 9;

                _aluResult = (uint)((FlagC ? 0x100 : 0) | (a & 0xFF));
                _aluResult = (_aluResult >> b) | (_aluResult << (9 - b));

                bool o;
                if (b == 1)
                    o = ((_aluResult ^ (_aluResult >> 1)) & 0x40) != 0;
                else
                {
                    if ((a & 0x80) == 0)
                        o = FlagO;
                    else
                        o = !FlagO;
                }

                _fm = fm.Bit8 | fm.CFromResult |
                    (o ? fm.OFlag : 0) |
                    (FlagS ? fm.SFlag : 0) |
                    (FlagZ ? fm.ZFlag : 0) |
                    (FlagP ? fm.PFlag : 0) |
                    (FlagA ? fm.AFlag : 0);

                return (byte)_aluResult;
            }
        }

        public ushort Shl16(ushort a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                _aluResult = (uint)(a << (b & 0x1f));

                _fm = fm.CFromResult | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                // Single bit shifts update FlagO to the xor of 
                // the carry flag and the msb of the result
                if (b == 1)
                {
                    if (((_aluResult ^ (_aluResult << 1)) & 0x10000) != 0)
                    {
                        _fm |= fm.OFlag;
                    }
                }
                else
                {
                    if (((_aluResult ^ (_aluResult << 1)) & (0x8000 << b)) != 0)
                        _fm |= fm.OFlag;
                }

                return (ushort)_aluResult;
            }
        }

        public byte Shl8(byte a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                _aluResult = (uint)(a << (b & 0x1f));

                _fm = fm.Bit8 | fm.CFromResult | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                // Single bit shifts update FlagO to the xor of 
                // the carry flag and the msb of the result
                if (b == 1)
                {
                    if (((_aluResult ^ (_aluResult << 1)) & 0x100) != 0)
                    {
                        _fm |= fm.OFlag;
                    }
                }
                else
                {
                    if (((_aluResult ^ (_aluResult << 1)) & (0x80 << b)) != 0)
                        _fm |= fm.OFlag;
                }

                return (byte)_aluResult;
            }
        }

        public ushort Shr16(ushort a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                b &= 0x1f;

                _aluResult = (uint)(a >> b);

                _fm = fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                if (((a >> (b - 1)) & 0x01) != 0)
                    _fm |= fm.CFlag;

                if (((_aluResult ^ a) & 0x8000) != 0)
                    _fm |= fm.OFlag;

                return (ushort)_aluResult;
            }
        }

        public byte Shr8(byte a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                b &= 0x1f;

                _aluResult = (uint)(a >> b);

                _fm = fm.Bit8 | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                if (((a >> (b - 1)) & 0x01) != 0)
                    _fm |= fm.CFlag;

                if (((_aluResult ^ a) & 0x80) != 0)
                    _fm |= fm.OFlag;

                return (byte)_aluResult;
            }
        }

        public ushort Sar16(ushort a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                b &= 0x1f;

                _aluResult = (uint)((int)(short)a >> b);

                _fm = fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                if (((a >> (b - 1)) & 0x01) != 0)
                    _fm |= fm.CFlag;

                if (((_aluResult ^ a) & 0x8000) != 0)
                    _fm |= fm.OFlag;

                return (ushort)_aluResult;
            }
        }

        public byte Sar8(byte a, byte b)
        {
            unchecked
            {
                if (b == 0)
                    return a;

                b &= 0x1f;

                _aluResult = (uint)((int)(sbyte)a >> b);

                _fm = fm.Bit8 | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                if (((a >> (b - 1)) & 0x01) != 0)
                    _fm |= fm.CFlag;

                if (((_aluResult ^ a) & 0x80) != 0)
                    _fm |= fm.OFlag;

                return (byte)_aluResult;
            }
        }

        public ushort Cbw(byte a)
        {
            unchecked
            {
                return (ushort)(sbyte)a;
            }
        }

        public uint Cwd(ushort a)
        {
            unchecked
            {
                return (uint)(short)a;
            }
        }

        public uint Div16(uint a, ushort b)
        {
            var remainder = (ushort)(a % b);
            return (uint)(checked((ushort)(a / b)) | (remainder << 16));
        }

        public ushort Div8(ushort a, byte b)
        {
            var remainder = (byte)(a % b);
            return (ushort)(checked((byte)(a / b)) | (remainder << 8));
        }

        public uint IDiv16(uint a, ushort b)
        {
            int ai = (int)a;
            int bi = (int)(short)b;

            var remainder = ai % bi;
            var quotient = ai / bi;
            return (uint)((ushort)checked((short)quotient) | ((ushort)checked((short)remainder) << 16));
        }

        public ushort IDiv8(ushort a, byte b)
        {
            int ai = (int)(short)a;
            int bi = (int)(sbyte)b;

            var remainder = ai % bi;
            var quotient = ai / bi;
            return (ushort)((byte)checked((sbyte)quotient) | ((byte)checked((sbyte)remainder) << 8));
        }

        public uint Mul16(ushort a, ushort b)
        {
            unchecked
            {
                _aluResult = (uint)a * (uint)b;

                _fm = fm.PFromResult | fm.SFromResult;

                FlagC = FlagO = (_aluResult & 0xFFFF0000) != 0;

                return _aluResult;
            }
        }

        public ushort Mul8(byte a, byte b)
        {
            unchecked
            {
                _aluResult = (uint)a * (uint)b;

                _fm = fm.Bit8 | fm.PFromResult | fm.SFromResult;

                FlagC = FlagO = (_aluResult & 0xFFFFFF00) != 0;

                return (ushort)_aluResult;
            }
        }

        public uint IMul16(ushort a, ushort b)
        {
            unchecked
            {
                _aluResult = (uint)((int)(short)a * (int)(short)b);

                _fm = fm.PFromResult | fm.SFromResult;

                FlagC = FlagO = (int)(short)(_aluResult & 0xFFFF) != (int)_aluResult;

                return (uint)_aluResult;
            }
        }

        public ushort IMul8(byte a, byte b)
        {
            unchecked
            {
                _aluResult = (uint)((int)(sbyte)a * (int)(sbyte)b);

                _fm = fm.Bit8 | fm.PFromResult | fm.SFromResult;

                FlagC = FlagO = (int)(sbyte)(_aluResult & 0xFF) != (int)_aluResult;

                return (ushort)_aluResult;
            }
        }

        public ushort Neg16(ushort b)
        {
            unchecked
            {
                _aluResult = ((uint)0 - (uint)b) & 0x1FFFF;

                _aluOperA = 0;
                _aluOperB = b;
                _fm = fm.AFromResult | fm.CFromResult | fm.OFromSub | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (ushort)_aluResult;
            }
        }

        public byte Neg8(byte b)
        {
            unchecked
            {
                _aluResult = ((uint)0 - (uint)b) & 0x1FFFF;

                _aluOperA = 0;
                _aluOperB = b;
                _fm = fm.Bit8 | fm.AFromResult | fm.CFromResult | fm.OFromSub | fm.PFromResult | fm.SFromResult | fm.ZFromResult;

                return (byte)_aluResult;
            }
        }

        public ushort Not16(ushort a)
        {
            unchecked
            {
                return (ushort)(~a);
            }
        }

        public byte Not8(byte a)
        {
            unchecked
            {
                return (byte)(~a);
            }
        }

        public ushort Aaa(ushort a)
        {
            unchecked
            {
                if (((a & 0x0F) > 9) || FlagA)
                {
                    FlagA = true;
                    FlagC = true;
                    return (ushort)((a + 0x106) & 0xFF0F);
                }
                else
                {
                    FlagA = false;
                    FlagC = false;
                    return (ushort)(a & 0xFF0F);
                }
            }
        }

        public ushort Aad(ushort a, byte b)
        {
            unchecked
            {
                var result = (ushort)(((a & 0xFF) + ((a >> 8) & 0xFF) * b) & 0xFF);
                _aluResult = (uint)(result << 8 | result & 0xFF);
                return result;
            }
        }

        public ushort Aam(byte a, byte b)
        {
            unchecked
            {
                var h = (byte)(a / b);
                var l = (byte)(a % b);
                _aluResult = (uint)((l << 8) | l);
                return (ushort)((h << 8) | l);
            }
        }

        public ushort Aas(ushort a)
        {
            unchecked
            {
                if ((a & 0x0f) > 9 || FlagA)
                {
                    a -= 6;
                    a = (ushort)(((byte)(a >> 8) - 1) << 8);
                    FlagA = true;
                    FlagC = true;
                }
                else
                {
                    FlagA = false;
                    FlagC = false;
                }

                a = (ushort)(a & 0xFF0F);

                var l = a & 0xFF;
                _aluResult = (ushort)(l << 8 | l);

                return a;
            }
        }

        public byte Daa(byte a)
        {
            unchecked
            {
                bool carry = a > 0x99 || FlagC;

                if ((a & 0x0F) > 9 || FlagA)
                {
                    var a2 = (ushort)a + 6;
                    a = (byte)a2;
                    FlagA = true;
                }
                else
                {
                    FlagA = false;
                }

                if (carry)
                    a += 0x60;

                FlagC = carry;

                _aluResult = (ushort)(a << 8 | a);

                return a;
            }
        }

        public byte Das(byte a)
        {
            unchecked
            {
                bool carry = a > 0x99 || FlagC;

                if ((a & 0x0F) > 9 || FlagA)
                {
                    var a2 = (ushort)a - 6;
                    a = (byte)a2;
                    FlagA = true;
                    FlagC = FlagC | (a2 & 0x100)!= 0;
                }
                else
                {
                    FlagA = false;
                    FlagC = false;
                }

                if (carry)
                {
                    a -= 0x60;
                    FlagC = true;
                }

                return a;
            }
        }

        #region Lookup Tables
        static byte[] _parityTable = new byte[]
        {
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x04,0x00,0x00,0x04,0x00,0x04,0x04,0x00,
	        0x00,0x04,0x04,0x00,0x04,0x00,0x00,0x04,
        };
        #endregion
    }
}
