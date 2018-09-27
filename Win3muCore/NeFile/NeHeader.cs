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
    [StructLayout(LayoutKind.Sequential)]
    public struct NeHeader
    {
        public ushort signature;          //"NE"
        public byte MajLinkerVersion;     //The major linker version
        public byte MinLinkerVersion;     //The minor linker version
        public ushort EntryTableOffset;   //Offset of entry table, see below
        public ushort EntryTableLength;   //Length of entry table in bytes
        public uint FileLoadCRC;          //UNKNOWN - PLEASE ADD INFO
        public byte ProgFlags;            //Program flags, bitmapped
        public AppFlags ApplFlags;            //Application flags, bitmapped
        public byte AutoDataSegIndex;     //The automatic data segment index
        public ushort InitHeapSize;       //The intial local heap size
        public ushort InitStackSize;      //The inital stack size
        public uint EntryPoint;           //CS:IP entry point, CS is index into segment table
        public uint InitStack;            //SS:SP inital stack pointer, SS is index into segment table
        public ushort SegCount;           //Number of segments in segment table
        public ushort ModRefs;            //Number of module references (DLLs)
        public ushort NoResNamesTabSiz;   //Size of non-resident names table, in bytes (Please clarify non-resident names table)
        public ushort SegTableOffset;     //Offset of Segment table
        public ushort ResTableOffset;     //Offset of resources table
        public ushort ResidNamTable;      //Offset of resident names table
        public ushort ModRefTable;        //Offset of module reference table
        public ushort ImportNameTable;    //Offset of imported names table (array of counted strings, terminated with string of length 00h)
        public uint OffStartNonResTab;    //Offset from start of file to non-resident names table
        public ushort MovEntryCount;      //Count of moveable entry point listed in entry table
        public ushort FileAlnSzShftCnt;   //File alligbment size shift count (0=9(default 512 byte pages))
        public ushort nResTabEntries;     //Number of resource table entries
        public TargetOS targOS;           //Target OS
        public byte OS2EXEFlags;          //Other OS/2 flags
        public ushort retThunkOffset;     //Offset to return thunks or start of gangload area - what is gangload?
        public ushort segrefthunksoff;    //Offset to segment reference thunks or size of gangload area
        public ushort mincodeswap;        //Minimum code swap area size
        public ushort expctwinver;        //Expected windows version

        public const ushort SIGNATURE = 'N' | 'E' << 8;

    };
}
