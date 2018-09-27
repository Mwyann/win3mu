/*
Win3mu - Windows 3 Emulator
Copyright (C) 2017 Topten Software.

Win3mu is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Win3mu is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Win3mu.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win3muCore
{
    public static class ByteArrayHelpers
    {
        public static void WriteByte(this byte[] This, int offset, byte value)
        {
            This[offset] = (byte)(value & 0xFF);
        }

        public static byte ReadByte(this byte[] This, int offset)
        {
            return This[offset];
        }

        public static void WriteWord(this byte[] This, int offset, ushort value)
        {
            This[offset] = (byte)(value & 0xFF);
            This[offset + 1] = (byte)(value >> 8);
        }

        public static ushort ReadWord(this byte[] This, int offset)
        {
            return (ushort)(This[offset] | (This[offset + 1] << 8));
        }

        public static void WriteDWord(this byte[] This, int offset, uint value)
        {
            WriteWord(This, offset, (ushort)(value & 0xFFFF));
            WriteWord(This, offset + 2, (ushort)(value >> 16));
        }

        public static uint ReadDWord(this byte[] This, int offset)
        {
            return (uint)(ReadWord(This, offset) | (ReadWord(This, offset + 2) << 16));
        }

        public static byte[] ReadBytes(this byte[] This, int offset, int count)
        {
            byte[] buf = new byte[count];
            Buffer.BlockCopy(This, offset, buf, 0, count);
            return buf;
        }

        public static void WriteBytes(this byte[] This, int offset, byte[] bytes)
        {
            Buffer.BlockCopy(bytes, 0, This, offset, bytes.Length);
        }

        public static int WriteString(this byte[] This, int offset, string str)
        {
            var bytes = Machine.AnsiEncoding.GetBytes(str);
            WriteBytes(This, offset, bytes);
            return bytes.Length;
        }

        public static string ReadString(this byte[] This, int offset)
        {
            int endPos = offset;
            while (This[endPos] != 0)
            {
                endPos++;
            }

            return Machine.AnsiEncoding.GetString(ReadBytes(This, offset, endPos - offset));
        }

        public static string ReadCharacters(this byte[] This, int offset, Encoding enc, int nCharacters)
        {
            int nBytes = nCharacters - 1;
            while (enc.GetCharCount(This, offset, nBytes+1) <= nCharacters)
                nBytes++;

            return enc.GetString(This, offset, nBytes);
        }

    }
}
