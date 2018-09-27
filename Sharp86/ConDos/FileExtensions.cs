using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConDos
{
    public static class FileExtensions
    {
        public static T ReadStruct<T>(this System.IO.FileStream This)
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

        public static string ReadLengthPrefixedString(this System.IO.FileStream This)
        {
            // Read the length prefix
            var length = This.ReadByte();
            if (length == 0)
                return null;

            // Convert to string
            return Encoding.GetEncoding(1252).GetString(ReadBytes(This, length));
        }

        public static byte[] ReadBytes(this System.IO.FileStream This, int length)
        {
            var buf = new byte[length];
            This.Read(buf, 0, length);
            return buf;
        }

        public static ushort ReadUInt16(this System.IO.FileStream This)
        {
            var lb = (byte)This.ReadByte();
            var hb = (byte)This.ReadByte();
            return (ushort)(hb << 8 | lb);
        }
    }
}
