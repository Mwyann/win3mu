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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Sharp86;
using Win3muCore.NeFile;

namespace Win3muCore
{
    public class Module16 : ModuleBase
    {
        public Module16(string filename)
        {
            _neFile = new NeFile.NeFileReader(filename);
        }

        string _guestFileName;

        public void SetGuestFileName(string guestFileName)
        {
            _guestFileName = guestFileName;
        }

        public SegmentEntry DataSegment
        {
            get
            {
                return _neFile.DataSegment;
            }
        }

        public NeFileReader NeFile
        {
            get { return _neFile; }
        }

        public ushort DataSelector
        {
            get
            {
                return DataSegment == null ? (ushort)0 : DataSegment.globalHandle;
            }
        }

        public override ushort GetOrdinalFromName(string functionName)
        {
            return _neFile.GetOrdinalFromName(functionName);
        }

        public override string GetNameFromOrdinal(ushort ordinal)
        {
            return _neFile.GetNameFromOrdinal(ordinal);
        }


        public override uint GetProcAddress(ushort ordinal)
        {
            return _neFile.GetProcAddress(ordinal);
        }

        public override IEnumerable<ushort> GetExports()
        {
            return _neFile.GetEntryPoints();    
        }


        NeFile.NeFileReader _neFile;

        public bool IsDll
        {
            get
            {
                return _neFile.IsDll;
            }
        }

        #region Abstract Methods
        public override string GetModuleName()
        {
            return _neFile.ModuleName;
        }

        public override string GetModuleFileName()
        {
            return _guestFileName;
        }

        public override void Load(Machine machine)
        {
            var segments = _neFile.Segments;
            var neHeader = _neFile.Header;

            for (int i=0; i<segments.Count; i++)
            {
                var seg = segments[i];

                // Work out heap name
                string name = string.Format("Module '{0}' Segment {1} {2} {3}", GetModuleName(), i + 1,
                    seg.flags.HasFlag(SegmentFlags.Data) ? "Data" : "Code",
                    seg.flags.HasFlag(SegmentFlags.ReadOnly) ? "Read-Only" : "Read-Write"
                );

                // How much to allocate?
                uint allocSize = seg.allocationBytes;

                if ((ushort)(i+1) == neHeader.AutoDataSegIndex)
                {
                    name += " [Automatic Data Segment]";
                    allocSize += neHeader.InitHeapSize;
                    if (!IsDll)
                        allocSize += neHeader.InitStackSize;
                }

                // Allocate memory
                seg.globalHandle = machine.GlobalHeap.Alloc(name, 0, allocSize);
                if (seg.globalHandle == 0)
                    throw new VirtualException("Out of Memory");

                seg.globalHandle = machine.GlobalHeap.SetSelectorAttributes(seg.globalHandle, 
                        !seg.flags.HasFlag(SegmentFlags.Data), 
                        seg.flags.HasFlag(SegmentFlags.ReadOnly));

                // Track the origin of the data for this segment
                machine.GlobalHeap.SetFileSource(seg.globalHandle, _neFile.FileName, (uint)seg.offset);

                // Get the buffer
                var bytes = machine.GlobalHeap.GetBuffer(seg.globalHandle, false);      // (Don't mark loaded segments as modified)

                // Read the segment
                _neFile.ReadSegment(seg, bytes);
            }
        }

        public override void Unload(Machine machine)
        {
            if (_neFile != null)
            {
                // Unload all segments
                var segments = _neFile.Segments;
                foreach (var seg in segments)
                {
                    if (seg.globalHandle!=0)
                    {
                        machine.GlobalHeap.Free(seg.globalHandle);
                        seg.globalHandle = 0;
                    }
                }

                _neFile.Dispose();
                _neFile = null;
            }
        }

        public override IEnumerable<string> GetReferencedModules()
        {
            return _neFile.ModuleReferenceTable;
        }

        void ApplyRelocations(byte[] data, ushort offset, ushort value, bool additive, bool log)
        {
            if (additive)
            {
                data.WriteWord(offset, (ushort)(data.ReadWord(offset) + value));
            }
            else
            {
                while (offset != 0xFFFF)
                {
                    if (log)
                        Log.WriteLine("            chain offset: {0:X4}", offset);
                    ushort nextOffset = data.ReadWord(offset);
                    data.WriteWord(offset, value);
                    offset = nextOffset;
                }
            }
        }

        void ApplyRelocations(byte[] data, ushort offset, uint value, bool additive, bool log)
        {
            if (additive)
            {
                data.WriteDWord(offset, (uint)(data.ReadDWord(offset) + value));
            }
            else
            {
                while (offset != 0xFFFF)
                {
                    if (log)
                        Log.WriteLine("            chain offset: {0:X4}", offset);

                    ushort nextOffset = data.ReadWord(offset);
                    data.WriteDWord(offset, value);
                    offset = nextOffset;
                }
            }
        }

        // Given the first two bytes of a floating point operation
        // work out the replacement bytes for the appropriate win87em 
        // interrupt. 
        public static ushort MapFPOpCodeToWin87EmInt(ushort opCode, ref byte triByteTable)
        {
            switch (opCode)
            {
                case 0xD89B: return 0x34CD; 
                case 0xD99B: return 0x35CD; 
                case 0xDA9B: return 0x36CD; 
                case 0xDB9B: return 0x37CD; 
                case 0xDC9B: return 0x38CD; 
                case 0xDD9B: return 0x39CD; 
                case 0xDE9B: return 0x3ACD; 
                case 0xDF9B: return 0x3BCD; 
                case 0x269B: return 0x3CCD;
                case 0x2e9B: triByteTable = 0x2e;  return 0x3CCD;
                case 0x369B: triByteTable = 0x36;  return 0x3CCD;
                case 0x9B90: return 0x3DCD;
            }

            return 0;               
        }

        public static byte MapFpOpCodeToWin87TriByte(byte triByteTable, byte opByte)
        {
            switch (triByteTable << 8 | opByte)
            {
                case 0x2ED8: return 0x98;
                case 0x2ED9: return 0x99;
                case 0x2EDB: return 0x9B;
                case 0x2EDC: return 0x9C;
                case 0x2EDD: return 0x9D;
                case 0x2EDF: return 0x9F;
                case 0x36D8: return 0x58;
                case 0x36D9: return 0x59;
                case 0x36DB: return 0x5B;
                case 0x36DD: return 0x5D;
                case 0x36DE: return 0x5E;
                case 0x36DF: return 0x5F;
            }

            throw new NotImplementedException(string.Format("FP OSFixup Tribyte: {0:X2} {1:X2}", triByteTable, opByte));
        }

        /*
        class OSFixInfo
        {
            public int segment;
            public ushort offset;
            public ushort param1;
            public ushort param2;
            public int fileoffset;
            public ushort codeBytes1;
            public ushort codeBytes2;

            public uint Key
            {
                get { return (uint)((codeBytes1 << 16) | (codeBytes2 & 0xFF)); }
            }
        }

        List<OSFixInfo> _fixInfos = new List<OSFixInfo>();

        void RecordOSFixup(int segment, ushort offset, ushort param1, ushort param2, int fileoffset, ushort codeBytes1, ushort codeBytes2)
        {
            _fixInfos.Add(new OSFixInfo()
            {
                segment = segment,
                offset = offset,
                param1 = param1, 
                param2 = param2,
                fileoffset = fileoffset,
                codeBytes1 = codeBytes1,
                codeBytes2 = codeBytes2,
            });
        }

        void DumpOSFixups()
        {
            Log.WriteLine("\nOSFixups:");
            foreach (var i in _fixInfos.OrderBy(x=>x.Key).ThenBy(x=>x.segment).ThenBy(x=>x.offset))
            {
                Log.WriteLine("{0:X2} {1:X2} {2:X2} p1:{3:X4} p2:{4:X4} -- seg: {5} offset:{6:X4} file:{7:X8}",
                        i.codeBytes1 & 0xFF,
                        i.codeBytes1 >> 8,
                        i.codeBytes2 & 0xFF,
                        i.param1,
                        i.param2,
                        i.segment + 1,
                        i.offset,
                        i.fileoffset);
            }
        }
        */

        public override void Link(Machine machine)
        {
            var segments = _neFile.Segments;

            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                Log.WriteLine("    Linking segment: {0} selector: 0x{1:X4}", i+1, seg.globalHandle);

                // Get the segment data
                var data = machine.GlobalHeap.GetBuffer(seg.globalHandle, false);

                HashSet<int> reffedSegments = new HashSet<int>();
                Action<int, ushort> refSelector = (segmentIndex, value) =>
                {
                    if (reffedSegments.Contains(segmentIndex))
                        return;
                    reffedSegments.Add(segmentIndex);
                    Log.WriteLine("        - reference to segment {0} found at 0x{1:X4}", segmentIndex, value);
                };

                for (int r=0; r<seg.relocations.Length; r++)
                {
                    var reloc = seg.relocations[r];

                    if (machine.logRelocations)
                    {
                        Log.WriteLine($"     relocation {r} {reloc.type} {reloc.offset:X4} p1: {reloc.param1:X4} {reloc.param2:X4}"); 
                    }

                    bool additive = (reloc.type & RelocationType.Additive)!= 0;
                    switch ((RelocationType)((byte)reloc.type & 0x03))
                    {
                        case RelocationType.InternalReference:
                            switch (reloc.addressType)
                            {
                                case RelocationAddressType.Selector:
                                {
                                    // Work out the selector value
                                    if (reloc.param1 == 0xFF)
                                    {
                                        var ep = _neFile.GetEntryPoint(reloc.param2);
                                        var targetSegment = segments[ep.segmentNumber - 1];
                                        ApplyRelocations(data, reloc.offset, targetSegment.globalHandle, additive, machine.logRelocations);
                                        refSelector(ep.segmentNumber, reloc.offset);
                                    }
                                    else
                                    {
                                        var targetSegment = segments[reloc.param1 - 1];
                                        ApplyRelocations(data, reloc.offset, targetSegment.globalHandle, additive, machine.logRelocations);
                                        refSelector(reloc.param1, reloc.offset);
                                    }
                                    break;
                                }

                                case RelocationAddressType.Pointer32:
                                {
                                        // Work out the selector value
                                        if (reloc.param1 == 0xFF)
                                        {
                                            // Get the entry point
                                            var ep = _neFile.GetEntryPoint(reloc.param2);
                                            var ptr = (uint)(segments[ep.segmentNumber-1].globalHandle << 16 | ep.segmentOffset);
                                            ApplyRelocations(data, reloc.offset, ptr, additive, machine.logRelocations);
                                            refSelector(ep.segmentNumber, reloc.offset);
                                        }
                                        else
                                        {
                                            var targetSegment = segments[reloc.param1 - 1];
                                            ApplyRelocations(data, reloc.offset, (uint)(targetSegment.globalHandle << 16 | reloc.param2), additive, machine.logRelocations);
                                            refSelector(reloc.param1, reloc.offset);
                                        }
                                        break;
                                }

                                case RelocationAddressType.Offset16:
                                {
                                        if (reloc.param1 == 0xFF)
                                        {
                                            var ep = _neFile.GetEntryPoint(reloc.param2);
                                            ApplyRelocations(data, reloc.offset, ep.segmentOffset, additive, machine.logRelocations);
                                        }
                                        else
                                        {
                                            ApplyRelocations(data, reloc.offset, reloc.param2, additive, machine.logRelocations);
                                        }
                                        break;
                                }


                                default:
                                    throw new NotImplementedException(string.Format("Unsupported relocation type: {0}/{1}", reloc.type, reloc.addressType));
                            }
                            break;

                        case RelocationType.ImportedOrdinal:
                        {
                            // Get the module
                            var moduleName = _neFile.ModuleReferenceTable[reloc.param1 - 1];
                            var module = machine.ModuleManager.GetModule(moduleName);

                            switch (reloc.addressType)
                            {
                                case RelocationAddressType.Pointer32:
                                {
                                    // Get the proc address
                                    uint farProc = module.GetProcAddress(reloc.param2);
                                    if (farProc == 0)
                                        throw new VirtualException("Module link failed, function ordinal #{0:X4} not found in module '{1}'", reloc.param2, moduleName);

                                    ApplyRelocations(data, reloc.offset, farProc, additive, machine.logRelocations);
                                    break;
                                }

                                case RelocationAddressType.Selector:
                                {
                                    // Get the proc address
                                    uint farProc = module.GetProcAddress(reloc.param2);
                                    if (farProc == 0)
                                        throw new VirtualException("Module link failed, function ordinal #{0:X4} not found in module '{1}'", reloc.param2, moduleName);

                                    ApplyRelocations(data, reloc.offset, farProc.Hiword(), additive, machine.logRelocations);
                                    break;
                                }

                                case RelocationAddressType.Offset16:
                                {
                                    uint addr = module.GetProcAddress(reloc.param2);
                                    if (addr==0)
                                         throw new VirtualException("Module link failed, function ordinal #{0:X4} not found in module '{1}'", reloc.param2, moduleName);

                                    ApplyRelocations(data, reloc.offset, addr.Loword(), additive, machine.logRelocations);
                                    break;
                                }

                                default:
                                    throw new NotImplementedException(string.Format("Unsupported relocation type: {0}/{1}", reloc.type, reloc.addressType));
                            }

                            break;
                        }

                        case RelocationType.OSFixUp:
                        {
                            var fpOpCode = data.ReadWord(reloc.offset);
                            byte triByteTable = 0;
                            var replace = MapFPOpCodeToWin87EmInt(fpOpCode, ref triByteTable);
                            if (replace == 0)
                            {
                                throw new NotImplementedException(string.Format("Don't know how to apply OS Fixup for FP operation at {0:X4} {1:X2} {2:X2}   [p1={3}, p2={4}]", reloc.offset, fpOpCode >> 8, fpOpCode & 0xFF, reloc.param1, reloc.param2));
                            }
                            data.WriteWord(reloc.offset, replace);

                            if (triByteTable!=0)
                            {
                                var rbyte = MapFpOpCodeToWin87TriByte(triByteTable, data.ReadByte(reloc.offset + 2));
                                data.WriteByte(reloc.offset + 2, rbyte);
                            }

                            //RecordOSFixup(i, reloc.offset, reloc.param1, reloc.param2, seg.offset + reloc.offset, fpOpCode, data.ReadWord(reloc.offset + 2));

                            break;
                        }

                        case RelocationType.ImportedName:
                        {
                            var moduleName = _neFile.ModuleReferenceTable[reloc.param1 - 1];
                            var module = machine.ModuleManager.GetModule(moduleName);
                            var entryPointName = _neFile.GetImportedName(reloc.param2);
                            var entryPointOrdinal = module.GetOrdinalFromName(entryPointName);
                            switch (reloc.addressType)
                            {
                                case RelocationAddressType.Pointer32:
                                    {
                                        // Get the proc address
                                        uint farProc = module.GetProcAddress(entryPointOrdinal);
                                        if (farProc == 0)
                                            throw new VirtualException("Module link failed, function ordinal #{0:X4} not found in module '{1}'", reloc.param2, moduleName);

                                        ApplyRelocations(data, reloc.offset, farProc, additive, machine.logRelocations);
                                        break;
                                    }

                                case RelocationAddressType.Selector:
                                    {
                                        // Get the proc address
                                        uint farProc = module.GetProcAddress(entryPointOrdinal);
                                        if (farProc == 0)
                                            throw new VirtualException("Module link failed, function ordinal #{0:X4} not found in module '{1}'", reloc.param2, moduleName);

                                        ApplyRelocations(data, reloc.offset, farProc.Hiword(), additive, machine.logRelocations);
                                        break;
                                    }

                                case RelocationAddressType.Offset16:
                                    {
                                        uint addr = module.GetProcAddress(entryPointOrdinal);
                                        if (addr == 0)
                                            throw new VirtualException("Module link failed, function ordinal #{0:X4} not found in module '{1}'", reloc.param2, moduleName);

                                        ApplyRelocations(data, reloc.offset, addr.Loword(), additive, machine.logRelocations);
                                        break;
                                    }

                                default:
                                    throw new NotImplementedException(string.Format("Unsupported relocation type: {0}/{1}", reloc.type, reloc.addressType));
                            }
                            break;
                        }

                        default:
                            throw new NotImplementedException(string.Format("Unsupported relocation type: {0}", reloc.type));
                    }
                }
            }

            foreach (var ep in _neFile.GetAllEntryPoints().Where(x=>(x.flags & Win3muCore.NeFile.EntryPoint.FLAG_EXPORTED)!=0))
            {
                // Get the segment
                var segment = segments[ep.segmentNumber - 1].globalHandle;
                var data = machine.GlobalHeap.GetBuffer(segment, false);

                if (data[ep.segmentOffset] != 0x1e || data[ep.segmentOffset+1] != 0x58 || data[ep.segmentOffset+2] !=0x90)
                {
                    /*
                    Log.Write("WARNING: Patching exported entry point prolog, existing code looks wrong at 0x{0:X4}:{1:X4}", ep.segmentNumber, ep.segmentOffset);
                    if (machine.StrictMode)
                        throw new InvalidDataException("Module entry point prolog wrong - see log");
                        */
                }

                if ((ep.flags & Win3muCore.NeFile.EntryPoint.FLAG_SHAREDDS)!=0)
                {
                    data[ep.segmentOffset] = 0xb8;      // MOV AX,xxxx
                    data.WriteWord(ep.segmentOffset + 1, this.DataSelector);
                }
                else
                {
                    if (!this.IsDll)
                    {
                        data[ep.segmentOffset] = 0x90;        // NOP
                        data[ep.segmentOffset+1] = 0x90;      // NOP
                    }
                }
            }

            //DumpOSFixups();
        }

        public override void Init(Machine machine)
        {
            var autoSeg = DataSegment;

            if (IsDll)
            {
                if (_neFile.Header.EntryPoint!=0)
                {
                    var saveds = machine.ds;

                    // Call Library entry point
                    machine.di = hModule;
                    machine.ds = autoSeg == null ? (ushort)0 : autoSeg.globalHandle;
                    machine.cx = _neFile.Header.InitHeapSize;

                    // Find entry point
                    var ip = (ushort)(_neFile.Header.EntryPoint & 0xFFFF);
                    var cs = _neFile.Segments[(int)((_neFile.Header.EntryPoint >> 16) - 1)].globalHandle;

                    // Call it
                    machine.CallVM(BitUtils.MakeDWord(ip, cs), "LibMain");

                    // Restore DS
                    machine.ds = saveds;
                }
            }
            else
            {            
                // Create the local heap
                if (autoSeg != null)
                {
                    var heapBaseAddress = (ushort)(autoSeg.allocationBytes + (IsDll ? 0 : _neFile.Header.InitStackSize));
                    var heapSize = _neFile.Header.InitHeapSize;
                    machine.GlobalHeap.CreateLocalHeap(autoSeg.globalHandle, heapBaseAddress, heapSize);
                }
            }
        }


        public override void Uninit(Machine machine)
        {
        }

        #endregion


        // Instance Data
        //LocalHeap _localHeap;
        ushort _psp;
        short _nCmdShow;

        public void PrepareStack(Machine machine)
        {
            var dataSeg = DataSegment;
            if (dataSeg == null)
                throw new VirtualException("Executable has no data segment");

            machine.sp = (ushort)(dataSeg.allocationBytes + _neFile.Header.InitStackSize);
            machine.ss = dataSeg.globalHandle;
        }


        public void PrepareRun(Machine machine, string commandTail, int nCmdShow)
        {
            if (IsDll)
                throw new VirtualException("Can't run a DLL");

            if (DataSegment == null)
                throw new VirtualException("Executable has no data segment");

            // Create the program segement prefix
            _psp = machine.GlobalHeap.Alloc("Program Segment Prefix", 0, 256);
            var pspBuf = machine.GlobalHeap.GetBuffer(_psp, true);
            pspBuf.WriteByte(0, 0xCD);
            pspBuf.WriteByte(0, 0x20);

            // Setup environment ptr
            pspBuf.WriteWord(0x002C, machine.GetDosEnvironmentSegment());

            // Update the PSP with the command tail
            int commandLen = commandTail == null ? 0 : commandTail.Length;
            if (commandLen > 0x7f)
                commandLen = 0x7f;
            pspBuf[0x80] = (byte)commandLen;
            if (commandLen > 0)
                pspBuf.WriteString(0x81, commandTail.Substring(0, commandLen));

            // Store show command
            _nCmdShow = (short)nCmdShow;

            // Setup machine state
            var dataSeg = DataSegment;
            machine.sp = (ushort)(dataSeg.allocationBytes + _neFile.Header.InitStackSize);
            machine.ss = dataSeg.globalHandle;
            machine.ds = dataSeg.globalHandle;
            machine.ip = (ushort)(_neFile.Header.EntryPoint & 0xFFFF);
            machine.cs = _neFile.Segments[(int)((_neFile.Header.EntryPoint>>16)-1)].globalHandle;
            machine.bx = _neFile.Header.InitStackSize;
            machine.cx = _neFile.Header.InitHeapSize;
            machine.di = hModule;
            machine.si = 0; // Previous instance
            machine.es = _psp; // Program segment prefix

            machine.Dos.PSP = _psp;
        }

        public ushort InitTask(Machine machine)
        {
            var autoSeg = DataSegment;
            if (autoSeg== null)
                return 0;

            // See https://blogs.msdn.microsoft.com/oldnewthing/20071203-00/?p=24323
            machine.bx = 0x81;     // Offset to command line in PSP
            machine.es = _psp;    // Segment of command line  
            machine.cx = (ushort)(autoSeg == null ? 0 : autoSeg.allocationBytes);       // Stack limit
            machine.dx = (ushort)_nCmdShow;
            machine.di = hModule;
            machine.bp = machine.sp;        
            machine.ds = autoSeg.globalHandle;

            // Real imple seems to return with SP decremented by 2 and 0 on the stack
            // Fake it but look after the return address
            var retAddr = machine.ReadDWord(machine.ss, machine.sp);
            machine.sp += 4;
            machine.sp -= 2;
            machine.WriteWord(machine.ss, machine.sp, 0);
            machine.sp -= 4;
            machine.WriteDWord(machine.ss, machine.sp, retAddr);

            return machine.es;     // Return the PSP segment in ax
        }
    }
}
