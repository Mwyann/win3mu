using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConDos
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MZRELOC
    {
        public ushort offset;
        public ushort segment;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MZHEADER
    {
        public ushort signature;
        public ushort extraBytes;
        public ushort pages;
        public ushort relocationItems;
        public ushort headerSize;
        public ushort minimumAllocation;
        public ushort maximumAllocation;
        public ushort initialSS;
        public ushort initialSP;
        public ushort checkSum;
        public ushort initialIP;
        public ushort initialCS;
        public ushort relocationTable;
        public ushort overlay;
        public ushort res1;
        public ushort res2;
        public ushort res3;
        public ushort res4;
        public ushort res5;
        public ushort res6;
        public ushort res7;
        public ushort res8;
        public ushort res9;
        public ushort res10;
        public ushort res11;
        public ushort res12;
        public ushort res13;
        public ushort res14;
        public ushort res15;
        public ushort res16;
        public ushort offsetNEHeader;

        public const ushort SIGNATURE = 'M' | 'Z' << 8;
    }

}
