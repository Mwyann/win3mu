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
using Sharp86;

namespace Win3muCore
{
    /*
    Changes

        - switch to protected memory model
        - selectors are no longer linear
        - selectors for > 64K should increment by 8
        - lower 3 bits of selector for table index and privledge 
        - most allocations should only need 1 selector
        - when allocating > 64, use a separate range of selectors and need consecutive range of 16 to cover 1Mb
        - have a total of 8192 selectors (13 bits)
        
    Selector Map:
        - 0x0000          - Null Selector
        - 0x0008 - 0x7ff8 - 64k Segments (total 256Mb)
        - 0x8000 - 0xFFF8 - > 64k Segments
    */
    public class GlobalHeap : IMemoryBus
    {
        public GlobalHeap(Machine machine)
        {
            _machine = machine;

            // Reserve the first 32 selectors, so first real selector is 0x100.
            _selectorAllocator.Alloc(32, false, false);

            // Allocate the interrupt descriptor table
            _machine.idt = Alloc("Interrupt Descriptor Table", 0, 1024);
        }

        private Machine _machine;
        private RangeAllocator<Selector> _selectorAllocator = new RangeAllocator<Selector>(8192);
        private Selector[] _pageMap = new Selector[8192];

        public class Allocation
        {
            public Allocation(uint bytes, ushort flags)
            {
                this.flags = flags;
                buffer = new byte[bytes];
            }
            public Allocation(byte[] data, ushort flags)
            {
                this.flags = flags;
                buffer = data;
            }
            public ushort flags;
            public byte[] buffer;
            public LocalHeap localHeap;
            public string filename;
            public uint fileoffset;

        }

        public class Selector
        {
            public string name;
            public ushort selectorIndex;
            public Allocation allocation;
            public bool isCode;
            public bool readOnly;

            public ushort selector
            {
                get
                {
                    return (ushort)((selectorIndex << 3) | (isCode ? 0x02 : 0x03));
                }
            }
        }

        public void SetFileSource(ushort selector, string filename, uint offset)
        {
            var sel = GetSelector(selector);
            if (sel == null || sel.allocation == null)
                return;

            sel.allocation.filename = filename;
            sel.allocation.fileoffset = offset;
        }

        public Selector AllocSelector(string name, ushort pages = 1)
        {
            // Create the allocation entry
            var sel = new Selector()
            {
                name = name,
            };

            // Multi segment
            sel.selectorIndex = (ushort)(_selectorAllocator.Alloc(pages, false, false).Position);

            // Update the high memory page map for all selectors
            for (int i = 0; i < pages; i++)
            {
                _pageMap[(sel.selectorIndex + i)] = sel;
            }

            if (_machine.logGlobalAllocations)
            {
                Log.WriteLine("Allocated selector: 0x{0:X4} ({1} pages)", sel.selector, pages);
            }

            return sel;
        }

        public ushort FreeSelector(ushort selector, ushort pages = 1)
        {
            if (selector == 0)
                return 0;

            // Get the selector index
            int selectorIndex = selector >> 3;

            Selector sel;
            sel = _pageMap[selectorIndex];
            if (sel == null)
                return 0;

            // Multi segment
            _selectorAllocator.Free(_selectorAllocator.AllocationFromPosition(selectorIndex));

            // Remove from page map
            for (int i = 0; i < pages; i++)
            {
                _pageMap[sel.selectorIndex + i] = null;
            }

            if (_machine.logGlobalAllocations)
            {
                Log.WriteLine("Freed selector: 0x{0:X4} ({1} pages)", sel.selector, pages);
            }

            return 0;
        }

        public uint GetLargestFreeSpace()
        {
            return BitUtils.MakeDWord(0, (ushort)_selectorAllocator.LargestFreeSpace);
        }

        public uint GetFreeSpace()
        {
            return BitUtils.MakeDWord(0, (ushort)_selectorAllocator.FreeSpace);
        }

        public ushort Alloc(string name, ushort Flags, byte[] data)
        {
            if (data == null)
                return 0;

            // How many selectors?
            var bytes = (uint)((data.Length + 31) & ~0x1F);
            var pages = (ushort)((bytes >> 16) + ((bytes & 0xFFFF) == 0 ? 0 : 1));

            // Allocate a selector
            var sel = AllocSelector(name, pages);

            // Create the allocation entry
            sel.allocation = new Allocation(data, Flags);

            // Return the handle
            return sel.selector;
        }

        // Allocate a global handle
        public ushort Alloc(string name, ushort Flags, uint bytes)
        {
            // At least one byte and round to 16 bytes
            if (bytes == 0)
                bytes = 1;
            bytes = (uint)((bytes + 31) & ~0x1F);

            // How many selectors?
            var pages = (ushort)((bytes >> 16) + ((bytes & 0xFFFF) == 0 ? 0 : 1));

            // Allocate a selector
            var sel = AllocSelector(name, pages);

            // Create the allocation entry
            sel.allocation = new Allocation(bytes, Flags);

            // Return the handle
            return sel.selector;
        }

        public ushort ReAlloc(ushort handle, uint newSize, ushort flags)
        {
            if (newSize == 0)
                newSize = 1;
            newSize = (uint)((newSize + 31) & ~0x1F);

            // Get the old allocation
            var sel = GetSelector(handle);
            if (sel == null)
                return 0;

            // Both less than 64K?
            if (newSize <= 0x10000 && sel.allocation.buffer.Length <= 0x10000)
            {
                // Create new buffer
                var newBuffer = new byte[newSize];
                Buffer.BlockCopy(sel.allocation.buffer, 0, newBuffer, 0, (int)Math.Min(newSize, sel.allocation.buffer.Length));
                sel.allocation.flags = flags;
                sel.allocation.buffer = newBuffer;

                return handle;
            }

            // Create a new allocation
            var newHandle = Alloc(sel.name, flags, newSize);
            if (newHandle==0)
            {
                Log.WriteLine("Global: Realloc failed");
                return 0;
            }
            var allocNew = GetSelector(newHandle);

            // Copy data
            Buffer.BlockCopy(sel.allocation.buffer, 0, allocNew.allocation.buffer, 0, (int)Math.Min(newSize, sel.allocation.buffer.Length));

            // Release the old handle
            Free(handle);

            // Return the new handle
            return newHandle;
        }

        // Free a global handle
        public ushort Free(ushort handle)
        {
            // Get the selector
            var sel = GetSelector(handle);
            if (sel == null)
                return handle;

            System.Diagnostics.Debug.Assert(sel.allocation != null);

            // Release memory
            /*
            if (sel.allocation!= null)
            {
                sel.allocation.flags = 0;
                sel.allocation.buffer = null;
            }
            */

            // How many selectors?
            int bytes = sel.allocation.buffer.Length;
            var pages = (ushort)((bytes >> 16) + ((bytes & 0xFFFF) == 0 ? 0 : 1));

            // Free the selector
            FreeSelector(handle, pages);

            // Success
            return 0;
        }

        public uint Size(ushort handle)
        {
            // Get the selector
            var sel = GetSelector(handle);
            if (sel == null)
                return 0;

            return (uint)sel.allocation.buffer.Length;
        }

        public Selector GetSelector(ushort handle)
        {
            int selectorIndex = handle >> 3;
            return _pageMap[selectorIndex];
        }

        public ushort SetSelectorAttributes(ushort handle, bool code, bool readOnly)
        {
            var sel = GetSelector(handle);
            if (sel == null)
                return 0;

            sel.isCode = code;
            sel.readOnly = readOnly;

            return sel.selector;
        }

        public HeapPointer GetHeapPointer(uint ptr, bool forWrite)
        {
            return new HeapPointer(this, ptr, forWrite);
        }

        public string ReadCharacters(uint ptr, int cch)
        {
            if (ptr == 0)
                return null;

            int offset;
            var buf = GetBuffer(ptr, false, out offset);

            return buf.ReadCharacters(offset, Machine.AnsiEncoding, cch);
        }

        // Lock a handle and return it's segment selector
        public byte[] GetBuffer(ushort handle, bool forWrite)
        {
            // Get the allocation
            var sel = GetSelector(handle);
            if (sel == null)
                return null;

            // Return the buffer
            return sel.allocation.buffer;
        }

        public byte[] GetBuffer(uint ptr, bool forWrite, out int offset)
        {
            // Get the allocation
            var sel = GetSelector(ptr.Hiword());
            if (sel == null)
            {
                offset = 0;
                return null;
            }

            // Work out offset in buffer
            offset = ((ptr.Hiword() >> 3) - sel.selectorIndex) << 16 | ptr.Loword();

            // Return the buffer
            return sel.allocation.buffer;
        }

        public LocalHeap GetLocalHeap(ushort globalHandle)
        {
            var sel = GetSelector(globalHandle);
            if (sel == null)
                return null;

            return sel.allocation.localHeap;
        }

        public LocalHeap CreateLocalHeap(ushort handle, ushort baseOffset, ushort maxSize)
        {
            // Get the allocation and check it doesn't already have a local heap manager
            var sel = GetSelector(handle);
            if (sel == null)
                throw new ArgumentException("Invalid global handle");
            if (sel.allocation.localHeap != null)
                throw new ArgumentException("Global handle already has a local heap manager");

            // Create and associate the local heap manager
            sel.allocation.localHeap = new LocalHeap(this, handle, baseOffset, maxSize);
            return sel.allocation.localHeap;
        }

        public LocalHeap CreateLocalHeap(string name, ushort maxSize)
        {
            // Allocate the global handle
            var globalHandle = Alloc(name, 0, maxSize == 0 ? 0x10000U : maxSize);
            if (globalHandle == 0)
                return null;

            // Create the heap
            return CreateLocalHeap(globalHandle, 0, maxSize);
        }

        #region IMemoryBus
        public bool IsExecutableSelector(ushort seg)
        {
            var alloc = GetSelector(seg);
            if (alloc == null)
                return false;

            return alloc.isCode;
        }

        public byte ReadByte(ushort seg, ushort offset)
        {
            var sel = GetSelector(seg);

            try
            {
                return sel.allocation.buffer[((seg >> 3) - sel.selectorIndex) << 16 | offset];
            }
            catch (NullReferenceException)
            {
                throw new Sharp86.SegmentNotPresentException(seg);
            }
            catch (IndexOutOfRangeException)
            {
                throw new Sharp86.GeneralProtectionFaultException(seg, offset, true);
            }
        }

        public void WriteByte(ushort seg, ushort offset, byte value)
        {
            var sel = GetSelector(seg);

            try
            {
                if (sel.readOnly || sel.isCode)
                    throw new Sharp86.GeneralProtectionFaultException(seg, offset, false);

                // Write byte
                sel.allocation.buffer[((seg >> 3) - sel.selectorIndex) << 16 | offset] = value;
            }
            catch (NullReferenceException)
            {
                throw new Sharp86.SegmentNotPresentException(seg);
            }
            catch (IndexOutOfRangeException)
            {
                throw new Sharp86.GeneralProtectionFaultException(seg, offset, false);
            }
        }
        #endregion


        public IEnumerable<Selector> AllSelectors
        {
            get
            {
                foreach (var r in _selectorAllocator.AllAllocations)
                {
                    var selIndex = r.Position;
                    var sel = _pageMap[selIndex];
                    if (sel != null && sel.selectorIndex == selIndex)       // Ignore multi-selectors
                    {
                        yield return sel;
                    }
                }
            }
        }
    }
}
