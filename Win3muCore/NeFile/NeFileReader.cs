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

namespace Win3muCore.NeFile
{
    public class NeFileReader : IDisposable
    {
        public NeFileReader(string filename)
        {
            _filename = filename;
            _file = System.IO.File.OpenRead(filename);
            _fileMap.Add(new MapEntry(_file.Length, 0, "End of File"));
            try
            {
                ReadHeaders(_file);
                ReadModuleReferenceTable(_file);
                ReadNonResidentNameTable(_file);
                ReadResidentNameTable(_file);
                ReadSegmentTable(_file);
                ReadEntryTable(_file);
                ReadRelocations(_file);
                ReadResourceTable(_file);
            }
            catch
            {
                _file.Close();
                _file = null;
                throw;
            }
        }

        public string FileName
        {
            get { return _filename; }
        }

        public string ModuleName
        {
            get { return _moduleName; }
        }

        public IList<SegmentEntry> Segments
        {
            get { return _segments; }
        }

        public string[] ModuleReferenceTable
        {
            get { return _moduleReferenceTable; }
        }

        public EntryPoint GetEntryPoint(ushort ordinal)
        {
            return _entryPoints[ordinal];
        }

        public IEnumerable<EntryPoint> GetAllEntryPoints()
        {
            return _entryPoints.Values;
        }

        public int DataSegmentIndex
        {
            get { return _neHeader.AutoDataSegIndex; }
        }

        public NeHeader Header
        {
            get { return _neHeader; }
        }

        public SegmentEntry DataSegment
        {
            get
            {
                if (_neHeader.AutoDataSegIndex == 0)
                    return null;

                if (_neHeader.AutoDataSegIndex < _segments.Count+1)
                    return _segments[_neHeader.AutoDataSegIndex-1];
                else
                    return null;
            }
        }

        public ushort DataSelector
        {
            get
            {
                return DataSegment.globalHandle;
            }
        }

        public void ReadSegment(SegmentEntry seg, byte[] buffer)
        {
            // Read from disk
            _file.Seek(seg.offset, SeekOrigin.Begin);
            _file.Read(buffer, 0, seg.lengthBytes);
        }

        public ushort GetOrdinalFromName(string functionName)
        {
            ushort ordinal;
            if (!_exportedNameToOrdinalMap.TryGetValue(functionName, out ordinal))
                return 0;

            return ordinal;
        }

        public string GetNameFromOrdinal(ushort ordinal)
        {
            string name;
            if (!_exportedOrdinalToNameMap.TryGetValue(ordinal, out name))
                return null;

            return name;
        }

        public IEnumerable<ushort> GetEntryPoints()
        {
            return _entryPoints.Keys;
        }


        public uint GetProcAddress(ushort ordinal)
        {
            // Look up the entry point
            EntryPoint ep;
            if (!_entryPoints.TryGetValue(ordinal, out ep))
                return 0;

            if (ep.segmentNumber == 0xFE)
            {
                return 0xFFFF0000 | ep.segmentOffset;
            }

            if (ep.segmentNumber >= _segments.Count)
                return 0;

            // Get the segment
            var segment = _segments[ep.segmentNumber - 1];
            
            // Convert to segment/offset
            return (uint)(segment.globalHandle << 16) | ep.segmentOffset;
        }

        string _filename;
        FileStream _file;
        string _moduleDescription;
        string _moduleName;
        Dictionary<ushort, EntryPoint> _entryPoints = new Dictionary<ushort, EntryPoint>();
        Dictionary<ushort, string> _exportedOrdinalToNameMap = new Dictionary<ushort, string>();
        Dictionary<string, ushort> _exportedNameToOrdinalMap = new Dictionary<string, ushort>(StringComparer.InvariantCultureIgnoreCase);
        //Dictionary<ushort, string> _importedOrdinalToNameMap = new Dictionary<ushort, string>();
        //Dictionary<string, ushort> _importedNameToOrdinalMap = new Dictionary<string, ushort>(StringComparer.InvariantCultureIgnoreCase);
        MzHeader _mzHeader;
        NeHeader _neHeader;
        List<SegmentEntry> _segments = new List<SegmentEntry>();
        ResourceTable _resourceTable;
        Dictionary<int, string> _importedNameTable = new Dictionary<int, string>();
        string[] _moduleReferenceTable;

        public bool IsDll
        {
            get
            {
                return _neHeader.ApplFlags.HasFlag(AppFlags.DLL);
            }
        }

        private MapEntry EnterRegion(string name)
        {
            var me = new MapEntry(_file.Position, name);
            _fileMap.Add(me);
            return me;
        }

        private void LeaveRegion(MapEntry me)
        {
            me.Length = _file.Position - me.Offset;
        }

        private void ReadHeaders(FileStream file)
        {
            // Read the MZ header
            var me = EnterRegion("MZ Header");
            _mzHeader = file.ReadStruct<MzHeader>();
            LeaveRegion(me);
            if (_mzHeader.signature != MzHeader.SIGNATURE)
            {
                throw new VirtualException("Not a valid executable (MZ signtuare missing)");
            }

            if (_mzHeader.offsetNEHeader == 0)
            {
                throw new VirtualException("Not a valid executable (NE header offset is zero)");
            }


            // Read the NE header
            file.Seek(_mzHeader.offsetNEHeader, SeekOrigin.Begin);
            me = EnterRegion("NE Header");
            _neHeader = file.ReadStruct<NeHeader>();
            LeaveRegion(me);
            if (_neHeader.signature != NeHeader.SIGNATURE)
            {
                throw new VirtualException("Not a valid executable (NE signature missing)");
            }

            // Check
            /*
            if ((_neHeader.ApplFlags & AppFlags.WinPMCompat) == 0)
            {
                throw new VirtualException(string.Format("Unsupported executable (AppFlags={0:X})", (byte)_neHeader.ApplFlags));
            }
            */

            // Check
            if (_neHeader.targOS != TargetOS.Win)
            {
                throw new VirtualException(string.Format("Unsupported executable (TargetOS={0:X})", (ushort)_neHeader.targOS));
            }

        }

        private void ReadEntryTable(FileStream file)
        {
            int entryTableOffset = _mzHeader.offsetNEHeader + _neHeader.EntryTableOffset;
            file.Seek(entryTableOffset, SeekOrigin.Begin);
            var stopPos = entryTableOffset + _neHeader.EntryTableLength;
            ushort ordinal = 1;
            while (file.Position < stopPos)
            {
                byte entryCount = (byte)file.ReadByte();
                if (entryCount == 0)
                    break;
                byte segmentIndex = (byte)file.ReadByte();
                if (segmentIndex == 0)
                {
                    ordinal += entryCount;
                    continue;
                }

                for (int i=0; i<entryCount; i++)
                {
                    var ep = new EntryPoint();

                    ep.ordinal = ordinal;
                    ep.flags = (byte)file.ReadByte();

                    if (segmentIndex == 0xFF)
                    {
                        // Moveable segment
                        ushort int3f = file.ReadUInt16();
                        System.Diagnostics.Debug.Assert(int3f == 0x3fCD);
                        ep.segmentNumber = (byte)file.ReadByte();
                    }
                    else
                    {
                        ep.segmentNumber = segmentIndex;
                    }

                    System.Diagnostics.Debug.Assert(ep.segmentNumber <= _segments.Count);

                    ep.segmentOffset = file.ReadUInt16();

                    _entryPoints.Add(ep.ordinal, ep);
                    ordinal++;
                }
            }
        }

        public string GetImportedName(ushort index)
        {
            // Already loaded
            string name;
            if (_importedNameTable.TryGetValue(index, out name))
                return name;

            // Save current file position
            var save = _file.Position;

            // Seek and read
            _file.Seek(_mzHeader.offsetNEHeader + _neHeader.ImportNameTable + index, SeekOrigin.Begin);
            var str = _file.ReadLengthPrefixedString();

            // Cache it
            _importedNameTable.Add(index, str);

            // Restore position
            _file.Seek(save, SeekOrigin.Begin);

            return str;
        }

        private void ReadModuleReferenceTable(FileStream file)
        {
            // Read the module reference table
            file.Seek(_mzHeader.offsetNEHeader + _neHeader.ModRefTable, SeekOrigin.Begin);
            _moduleReferenceTable = new string[_neHeader.ModRefs];
            for (int i = 0; i < _neHeader.ModRefs; i++)
            {
                _moduleReferenceTable[i] = GetImportedName(file.ReadUInt16());
            }
        }

        private void ReadNonResidentNameTable(FileStream file)
        {
            file.Seek(_neHeader.OffStartNonResTab, SeekOrigin.Begin);
            var me = EnterRegion("Non-resident name table");
            var stopPos = _neHeader.OffStartNonResTab + _neHeader.NoResNamesTabSiz;
            while (file.Position < stopPos)
            {
                var str = file.ReadLengthPrefixedString();
                if (str == null)
                    continue;

                var ordinal = file.ReadUInt16();

                if (ordinal == 0)
                {
                    _moduleDescription = str;
                }
                else
                {
                    _exportedOrdinalToNameMap.Add(ordinal, str);
                    _exportedNameToOrdinalMap.Add(str, ordinal);
                }
            }
            LeaveRegion(me);
        }

        private void ReadResidentNameTable(FileStream file)
        {
            if (_neHeader.ResidNamTable == 0)
                return;

            file.Seek(_mzHeader.offsetNEHeader + _neHeader.ResidNamTable, SeekOrigin.Begin);
            var me = EnterRegion("Resident name table");
            while (true)
            {
                var str = file.ReadLengthPrefixedString();
                if (str == null)
                    break;

                var ordinal = file.ReadUInt16();

                if (ordinal == 0)
                {
                    _moduleName = str;
                }
                else
                {
                    _exportedOrdinalToNameMap.Add(ordinal, str);
                    _exportedNameToOrdinalMap.Add(str, ordinal);
                }
            }
            LeaveRegion(me);
        }

        private void ReadSegmentTable(FileStream file)
        {
            // Read the segment table
            file.Seek(_mzHeader.offsetNEHeader + _neHeader.SegTableOffset, SeekOrigin.Begin);
            var me = EnterRegion("Segment Table");
            for (int i = 0; i < _neHeader.SegCount; i++)
            {
                var seg = new SegmentEntry();
                seg.Read(file);
                seg.offset = (1 << _neHeader.FileAlnSzShftCnt) * seg.offset;
                _segments.Add(seg);
                if (seg.lengthBytes > 0)
                {
                    _fileMap.Add(new MapEntry(seg.offset, seg.lengthBytes, string.Format("Segment {0}", i)));
                }
            }
            LeaveRegion(me);
        }

        private void ReadRelocations(FileStream file)
        {
            // Read the relocations table
            for (int si=0; si<_segments.Count; si++)
            {
                var seg = _segments[si];
                if (!seg.flags.HasFlag(SegmentFlags.HasRelocations))
                {
                    seg.relocations = new RelocationEntry[0];
                    continue;
                }

                // Seek to the end of the segment
                file.Seek(seg.offset + seg.lengthBytes, SeekOrigin.Begin);
                var me = EnterRegion(string.Format("Relocations for segment {0}", si));

                // Read the number of relocation entries...
                seg.relocations = new RelocationEntry[file.ReadUInt16()];
                for (int i = 0; i < seg.relocations.Length; i++)
                {
                    seg.relocations[i] = new RelocationEntry(file);
                }
                LeaveRegion(me);
            }
        }

        private void ReadResourceTable(FileStream file)
        {
            // _neHeader.nResTabEntries is unreliable for number of resource entries (it's usually zero)
            // but _neHeader.ResTableOffset is often set when no resources (eg: win87em.dll)
            // Comparing the ResidNameTable offset seems to be the only way to detect no resources
            if (_neHeader.ResTableOffset == _neHeader.ResidNamTable)
            {
                _resourceTable = new ResourceTable();
                return;
            }

            // Resource the resource table
            file.Seek(_mzHeader.offsetNEHeader + _neHeader.ResTableOffset, SeekOrigin.Begin);

            var me = EnterRegion("Resource table");
            _resourceTable = new ResourceTable(file);

            foreach (var rt in _resourceTable.types)
            {
                foreach (var r in rt.resources)
                {
                    _fileMap.Add(new MapEntry(r.offset, r.length, string.Format("Resource: {0}/{1}", rt.name, r.name)));
                }
            }
            LeaveRegion(me);

            // Find the resource name table
            var nte = FindResourceType(Win16.ResourceType.RT_NAMETABLE.ToString());
            if (nte!= null && nte.resources.Length>0)
            {
                _file.Seek(nte.resources[0].offset, SeekOrigin.Begin);

                while (true)
                {
                    // Read length prefix
                    var pos = _file.Position;
                    short entryLength = _file.ReadInt16();
                    if (entryLength == 0)
                        break;

                    // Read the entry
                    var resourceType = (Win16.ResourceType)_file.ReadUInt16();
                    var resourceOrd = _file.ReadUInt16();
                    _file.ReadByte();   // Unused?
                    string resourceName = _file.ReadNullTerminatedString();

                    // Find the resource and update it's name
                    var rt = FindResourceType(resourceType.ToString());
                    if (rt != null)
                    {
                        foreach (var re in rt.resources)
                        {
                            if (re.id == resourceOrd)
                            {
                                re.nameTableName = resourceName;
                            }
                        }
                    }

                    // Seek to next entry
                    _file.Seek(pos + entryLength, SeekOrigin.Begin);
                }
            }
        }

        public void Dump(bool includeRelocations)
        {
            Log.WriteLine("MODULE DUMP: {0}", _filename);
            Log.WriteLine("App Flags: {0}", EnumUtils.FormatFlags(_neHeader.ApplFlags));
            Log.WriteLine("Auto Data Segment: {0}", _neHeader.AutoDataSegIndex);
            Log.WriteLine("CS:IP: {0:X4}:{1:X4}", _neHeader.EntryPoint >> 16, _neHeader.EntryPoint & 0xFFFF);
            Log.WriteLine("SS:SP: {0:X4}:{1:X4}", _neHeader.InitStack >> 16, _neHeader.InitStack & 0xFFFF);
            Log.WriteLine("Stack Size: 0x{0:X4}", _neHeader.InitStackSize);
            Log.WriteLine("Heap Size: 0x{0:X4}", _neHeader.InitHeapSize);

            Log.WriteLine("\nImported Name Table:");
            foreach (var kv in _importedNameTable)
            {
                Log.WriteLine("    {0}: {1}", kv.Key, kv.Value);
            }

            Log.WriteLine("\nModule Reference Table:");
            for (int i=0; i<_moduleReferenceTable.Length; i++)
            {
                Log.WriteLine("    {0}: {1}", i+1, _moduleReferenceTable[i]);
            }

            Log.WriteLine("\nSegments:");
            int index = 1;
            foreach (var seg in _segments)
            {
                Log.WriteLine("    {0}: offset: {1:X8} length: {2:X4} allocation: {3:X4} flags: {4:X4} relocs: {5}",
                    index++, seg.offset, seg.lengthBytes, seg.allocationBytes, EnumUtils.FormatFlags((SegmentFlags)seg.flags), seg.relocations.Length);

                if (includeRelocations)
                {
                    foreach (var r in seg.relocations)
                    {
                        Log.WriteLine("  reloc: {0} {1} offset:{2:X4} p1:{3:X4} p2: {4:X4}", r.TypeString, r.addressType, r.offset, r.param1, r.param2);
                    }
                }
            }

            Log.WriteLine("\nResources: (table starts at {0:X8})", _mzHeader.offsetNEHeader + _neHeader.ResTableOffset);
            foreach (var r in _resourceTable.types)
            {
                foreach (var e in r.resources)
                {
                    Log.WriteLine("    {0,20} - offset: {1:X8} length: {2:X4} flags: {3:X4}",
                        string.Format("{0}/{1}", r.name, e.nameTableName==null ? e.name : e.nameTableName), e.offset, e.length, e.flags);
                }
            }

            Log.WriteLine("\nExports:");
            foreach (var kv in _exportedOrdinalToNameMap)
            {
                Log.WriteLine("    {0:X4} - {1} - {2:X4}", kv.Key, kv.Value, _exportedNameToOrdinalMap[kv.Value]);
            }

            Log.WriteLine("\nEntry Points:");
            foreach (var ep in _entryPoints.Values)
            {
                string name = "n/a";
                _exportedOrdinalToNameMap.TryGetValue(ep.ordinal, out name);
                Log.WriteLine("    {0} - {1:X2} segNo: {2} segOffset: {3:X4} ({4})",
                            ep.ordinal, ep.flags, ep.segmentNumber, ep.segmentOffset, name);
            }

            Log.WriteLine("\nFile Map:");
            _fileMap = _fileMap.OrderBy(x => x.Offset).ToList();
            long pos = 0;
            for (int i=0; i<_fileMap.Count; i++)
            {
                var me = _fileMap[i];
                if (me.Offset != pos)
                {
                    Log.WriteLine("{0:X8} - {1:X8} [{2:X4}] -", pos, me.Offset, me.Offset - pos);
                }
                Log.WriteLine("{0:X8} - {1:X8} [{2:X4}] {3}", me.Offset, me.Offset + me.Length, me.Length, me.Name);
                pos = me.Offset + me.Length;
            }


            Log.WriteLine();
            Log.WriteLine();
        }

        Dictionary<string, ResourceTypeTable> _resourceTypeMap;
        public ResourceTypeTable FindResourceType(string name)
        {
            if (_resourceTypeMap == null)
            {
                _resourceTypeMap = new Dictionary<string, ResourceTypeTable>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var t in _resourceTable.types)
                {
                    _resourceTypeMap.Add(t.name, t);
                }
            }

            ResourceTypeTable tt;
            if (!_resourceTypeMap.TryGetValue(name, out tt))
                return null;

            return tt;
        }

        public ResourceEntry FindResource(string type, string entry)
        {
            var tt = FindResourceType(type);
            if (tt == null)
                return null;

            return tt.FindEntry(entry);
        }

        public byte[] LoadResource(string type, string entry)
        {
            var e = FindResource(type, entry);
            if (e == null)
                return null;

            return LoadResource(e);
        }

        public byte[] LoadResource(ResourceEntry e)
        {
            // Seek to it
            _file.Seek(e.offset, SeekOrigin.Begin);

            // Read it
            var buf = new byte[e.length];
            _file.Read(buf, 0, e.length);
            return buf;
        }

        public Stream GetResourceStream(string type, string entry)
        {
            var e = FindResource(type, entry);
            if (e == null)
                return null;

            // Seek to it
            _file.Seek(e.offset, SeekOrigin.Begin);

            return _file;
        }

        #region IDisposable Support

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }
        #endregion

        class MapEntry
        {
            public MapEntry(long offset, string name)
            {
                Offset = offset;
                Length = -1;
                Name = name;
            }

            public MapEntry(long offset, long length, string name)
            {
                Offset = offset;
                Length = length;
                Name = name;
            }

            public long Offset;
            public long Length;
            public string Name;
        }

        List<MapEntry> _fileMap = new List<MapEntry>();
    }
}
