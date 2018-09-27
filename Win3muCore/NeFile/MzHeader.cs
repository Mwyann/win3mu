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

using System.Runtime.InteropServices;

namespace Win3muCore.NeFile
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MzHeader
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
