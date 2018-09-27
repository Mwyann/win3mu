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

using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Win3muCore
{
    public static class FileExtensions
    {
        public static T ReadStruct<T>(this System.IO.Stream This)
        {
            // Allocate buffer
            var buf = new byte[Marshal.SizeOf<T>()];

            // Read it
            This.Read(buf, 0, buf.Length);

            // Convert to struct
            var handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
            var t = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            // Return ti
            return t;
        }

        public static void WriteStruct<T>(this System.IO.Stream This, T value)
        {
            // Allocate buffer
            var buf = new byte[Marshal.SizeOf<T>()];

            // Convert to struct
            var handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();

            // Write it
            This.Write(buf, 0, buf.Length);
        }

        public static string ReadLengthPrefixedString(this System.IO.Stream This)
        {
            // Read the length prefix
            var length = This.ReadByte();
            if (length == 0)
                return null;

            // Convert to string
            return Machine.AnsiEncoding.GetString(ReadBytes(This, length));
        }

        public static string ReadNullTerminatedString(this System.IO.Stream This, byte lead = 0)
        {
            var s = new System.IO.MemoryStream();

            if (lead != 0)
                s.WriteByte(lead);

            while (true)
            {
                var b = (byte)This.ReadByte();
                if (b == 0)
                    break;
                s.WriteByte(b);
            }

            return Machine.AnsiEncoding.GetString(s.ToArray());
        }

        public static byte[] ReadBytes(this System.IO.Stream This, int length)
        {
            var buf = new byte[length];
            This.Read(buf, 0, length);
            return buf;
        }

        public static ushort ReadUInt16(this System.IO.Stream This)
        {
            var lb = (byte)This.ReadByte();
            var hb = (byte)This.ReadByte();
            return (ushort)(hb << 8 | lb);
        }

        public static void WriteUInt16(this System.IO.Stream This, ushort value)
        {
            This.WriteByte((byte)(value & 0xFF));
            This.WriteByte((byte)(value >> 8));
        }

        public static short ReadInt16(this System.IO.Stream This)
        {
            var lb = (byte)This.ReadByte();
            var hb = (byte)This.ReadByte();
            return (short)(hb << 8 | lb);
        }

        public static StringOrId ReadResourceString16(this System.IO.Stream This, bool weirdness = false)
        {
            var lead = This.ReadByte();

            // Null
            if (lead == 0)
                return null;

            // Ordinal?
            if (lead == 0xFF)
            {
                var id = This.ReadUInt16();
                return new StringOrId(id);
            }

            // Special check for win16 dialog template window class names
            if (weirdness && lead >= 0x80 && lead <=0x85)
            {
                return new StringOrId((ushort)lead);
            }

            // Ansi
            var str = ReadNullTerminatedString(This, (byte)lead);
            return new StringOrId(str);
        }

        public static void WriteUnicodeString(this System.IO.Stream This, string str)
        {
            var buf = Encoding.Unicode.GetBytes(str);
            This.Write(buf, 0, buf.Length);
            This.WriteUInt16(0);
        }

        public static void WriteResourceString32(this System.IO.Stream This, StringOrId value, bool extraWeirdness = false)
        {
            if (value == null)
            {
                This.WriteUInt16(0);
                return;
            }

            if (value.Name == null)
            {
                if (extraWeirdness)
                    This.WriteUInt16(0xFFFF);
                else
                    This.WriteUInt16(0x00FF);
                This.WriteUInt16(value.ID);
                return;
            }

            WriteUnicodeString(This, value.Name);
        }

        public static void Pad(this System.IO.Stream This, int multiple)
        {
            while ((This.Position % multiple)!=0)
            {
                This.WriteByte(0);
            }
        }
    }
}
