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
    public class LocalHeap : RangeAllocator<LocalHeap.LocalAllocation>.IMoveListener
    {
        public LocalHeap(GlobalHeap globalHeap, ushort globalHandle, ushort baseOffset, ushort maxSize)
        {
            _globalHeap = globalHeap;
            _globalHandle = globalHandle;

            // Dont allocate at zero
            if (baseOffset == 0)
                baseOffset = 16;

            // Work out top of heap
            uint topOfHeap = _globalHeap.Size(globalHandle);
            uint heapLimit = baseOffset + (uint)(maxSize == 0 ? 0x10000 : maxSize);
            if (heapLimit > topOfHeap)
                heapLimit = topOfHeap;

            _allocator = new RangeAllocator<LocalAllocation>((int)heapLimit);
            _freeHandles = new List<ushort>();
            _handleOrPtrMap = new Dictionary<ushort, LocalAllocation>();

            var reservedPage = _allocator.Alloc(baseOffset, false, false);
            reservedPage.User = new LocalAllocation()
            {
                flags = Win16.LMEM_FIXED,
                handleOrPtr = 0,
                range = reservedPage,
            };

            _allocator.MoveListener = this;
            _baseOffset = baseOffset;
        }

        class LocalAllocation
        {
            public ushort handleOrPtr;                          // Handle if moveable, else address
            public ushort flags;                                // Flags
            public RangeAllocator<LocalAllocation>.Range range; // Allocation

            public bool Moveable
            {
                get { return (flags & Win16.LMEM_MOVEABLE) != 0; }
            }
        }

        GlobalHeap _globalHeap;
        ushort _globalHandle;
        ushort _baseOffset;
        RangeAllocator<LocalAllocation> _allocator;
        List<ushort> _freeHandles;
        Dictionary<ushort, LocalAllocation> _handleOrPtrMap;

        ushort AllocHandle(ushort growSize)
        {
            // Do we have any free handles?
            if (_freeHandles.Count==0)
            {
                // No, allocate a fixed block of memory for the handles
                // Each handle in the block points to the actual location of the allocation elsewhere in the heap
                int handlesPerBlock = 32;

                // Allocate range
                var r = _allocator.Alloc(handlesPerBlock * 2, false, true);
                if (r == null)
                {
                    GrowHeap(handlesPerBlock * 2 + growSize + 1024);

                    r = _allocator.Alloc(handlesPerBlock * 2, false, true);
                    if (r == null)
                        return 0;
                }

                r.User = new LocalAllocation()
                {
                    flags = Win16.LMEM_FIXED,
                    handleOrPtr = 0,
                    range = r,
                };

                // Create handles
                for (int i=0; i<handlesPerBlock; i++)
                {
                    _freeHandles.Add((ushort)(r.Position + i * 2));
                }
            }

            // Remove from list
            ushort handle = _freeHandles[0];
            _freeHandles.RemoveAt(0);
            return handle;
        }

        void FreeHandle(ushort handle)
        {
            if (handle!=0)
            {
                _freeHandles.Add(handle);
            }
        }

        public ushort GlobalHandle
        {
            get
            {
                return _globalHandle;
            }
        }

        public byte[] GetBuffer(bool forWrite)
        {
            return _globalHeap.GetBuffer(_globalHandle, forWrite);
        }

        public ushort GetSelector()
        {
            return _globalHandle;
        }

        bool GrowHeap(int additionalBytes)
        {
            if (_allocator.AddressSpaceSize <= 0x10000)
            {
                var newAddressSpaceSize = _allocator.AddressSpaceSize + additionalBytes;
                if (newAddressSpaceSize > 0x10000)
                    newAddressSpaceSize = 0x10000;
                if (newAddressSpaceSize != _allocator.AddressSpaceSize)
                {
                    // Reallocate global memory
                    ushort newHandle = _globalHeap.ReAlloc(_globalHandle, (uint)newAddressSpaceSize, 0);
                    if (newHandle == 0)
                        return false;

                    // Should never be moved!
                    System.Diagnostics.Debug.Assert(newHandle == _globalHandle);

                    // Update the allocator with new limit
                    _allocator.AddressSpaceSize = newAddressSpaceSize;
                    return true;
                }
            }

            return false;
        }



        public ushort Alloc(ushort flags, ushort length)
        {
            // Round up
            if (length == 0)
                return 0;
            if (length < 8)
            {
                // Min size = 8 bytes
                length = 8;
            }
            else
            {
                // Granularity = 4 bytes
                length = (ushort)((length + 3) & 0xFFFC);
            }

            // Allocate handle if moveable
            ushort handle = 0;
            if ((flags & Win16.LMEM_MOVEABLE)!=0)
            {
                handle = AllocHandle(length);
                if (handle == 0)
                    return 0;
            }

            // Alocate memory
            var range = _allocator.Alloc(length, (flags & Win16.LMEM_MOVEABLE) != 0, (flags & Win16.LMEM_NOCOMPACT) == 0);
            if (range == null)
            {
                // Try to grow the heap
                if (GrowHeap(length))
                {
                    range = _allocator.Alloc(length, (flags & Win16.LMEM_MOVEABLE) != 0, (flags & Win16.LMEM_NOCOMPACT) == 0);
                }

                // Still failed?
                if (range == null)
                {
                    FreeHandle(handle);
                    return 0;
                }
            }

            // Create the allocation
            var alloc = new LocalAllocation()
            {
                handleOrPtr = handle == 0 ? (ushort)range.Position : handle,
                flags = flags,
                range = range,
            };

            range.User = alloc;

            // Store it
            _handleOrPtrMap.Add(alloc.handleOrPtr, alloc);

            // Setup the handle to point to the actual memory location
            if (alloc.Moveable)
            {
                GetBuffer(true).WriteWord(handle, (ushort)alloc.range.Position);
            }

            // Zero init?
            if ((flags & Win16.LMEM_ZEROINIT)!=0)
            {
                try
                {
                    var buffer = GetBuffer(true);
                    for (int i=0; i< length; i++)
                    {
                        buffer[alloc.range.Position + i] = 0;
                    }
                }
                catch
                {
                    throw;
                }
            }

            // Return handle
            return alloc.handleOrPtr;
        }

        LocalAllocation FromHandle(ushort handle)
        {
            // Get the allocation
            LocalAllocation alloc;
            if (!_handleOrPtrMap.TryGetValue(handle, out alloc))
                return null;

            return alloc;
        }

        public bool Free(ushort handle)
        {
            // Get allocation from handle or pointer
            var alloc = FromHandle(handle);
            if (alloc==null)
                return false;

            // Remove from allocations table
            _handleOrPtrMap.Remove(alloc.handleOrPtr);

            // Release the handle
            if (alloc.Moveable)
            {
                FreeHandle(alloc.handleOrPtr);
            }

            // Remove from allocator
            _allocator.Free(alloc.range);
            return true;
        }

        public ushort ReAlloc(ushort handle, ushort newSize, ushort flags)
        {
            // Get allocation from handle or pointer
            var alloc = FromHandle(handle);

            // Save the old size
            // Round up
            if (newSize == 0)
                return 0;
            if (newSize < 8)
            {
                // Min size = 8 bytes
                newSize = 8;
            }
            else
            {
                // Granularity = 4 bytes
                newSize = (ushort)((newSize + 3) & 0xFFFC);
            }

            var oldSize = alloc.range.Size;
            bool newMoveable = (flags & Win16.LMEM_MOVEABLE) != 0;

            // If we're making it moveable, the we'll need a real handle
            ushort newHandle = 0;
            if (newMoveable && !alloc.Moveable)
            {
                newHandle = AllocHandle(newSize);
                if (newHandle == 0)
                    return 0;
            }

            // Reallocate
            if (!_allocator.ReAlloc(alloc.range, newSize, (flags & Win16.LMEM_MOVEABLE) != 0, (flags & Win16.LMEM_NOCOMPACT) == 0))
            {
                // Grow heap if failed
                if (GrowHeap(newSize))
                {
                    // Realloc again
                    if (!_allocator.ReAlloc(alloc.range, newSize, (flags & Win16.LMEM_MOVEABLE) != 0, (flags & Win16.LMEM_NOCOMPACT) == 0))
                    {
                        FreeHandle(newHandle);
                        return 0;
                    }
                }
                FreeHandle(newHandle);
                return 0;
            }

            if (newMoveable != alloc.Moveable)
            {
                if (newMoveable)
                {
                    // Store the new handle
                    _handleOrPtrMap.Remove(handle);
                    _handleOrPtrMap.Add(newHandle, alloc);

                    // Update the allocation to hold the handle
                    alloc.handleOrPtr = newHandle;

                    // Update the in-heap handle to point to the allocation
                    GetBuffer(true).WriteWord(newHandle, (ushort)alloc.range.Position);
                }
                else
                {
                    // Ditch the handle and store the pointer
                    FreeHandle(handle);
                    _handleOrPtrMap.Remove(handle);
                    _handleOrPtrMap.Add((ushort)alloc.range.Position, alloc);
                    alloc.handleOrPtr = (ushort)alloc.range.Position;
                }
            }

            alloc.flags = flags;

            // Zero init?
            if ((flags & Win16.LMEM_ZEROINIT) != 0 && newSize > oldSize)
            {
                var buffer = GetBuffer(true);
                for (int i = oldSize; i < alloc.range.Size; i++)
                {
                    buffer[alloc.range.Position + i] = 0;
                }
            }

            // Return the new position/handle
            return (ushort)alloc.handleOrPtr;
        }

        public ushort Lock(ushort handle)
        {
            // Get allocation from handle or pointer
            var alloc = FromHandle(handle);

            // Bump the lock count
            alloc.range.LockCount++;

            // Return its current position
            return (ushort)alloc.range.Position;
        }

        public bool Unlock(ushort handle)
        {
            // Get allocation from handle or pointer
            var alloc = FromHandle(handle);
            if (alloc.range.LockCount == 0)
                return false;

            // Bump the lock count
            alloc.range.LockCount--;
            return true;
        }

        public ushort Size(ushort handle)
        {
            // Get allocation from handle or pointer
            var alloc = FromHandle(handle);
            if (alloc == null)
                return 0;

            return (ushort)alloc.range.Size;
        }

        public ushort HeapSize()
        {
            return (ushort)(_allocator.AddressSpaceSize - _baseOffset);
        }

        void RangeAllocator<LocalAllocation>.IMoveListener.Move(RangeAllocator<LocalAllocation>.Range range, int newPosition)
        {
            var bytes = _globalHeap.GetBuffer(_globalHandle, true);
            Buffer.BlockCopy(bytes, range.Position, bytes, newPosition, range.Size);

            // Update the local handle to point to the new location
            if (range.User.Moveable)
            {
                bytes.WriteWord(range.User.handleOrPtr, (ushort)newPosition);
            }
        }

        object RangeAllocator<LocalAllocation>.IMoveListener.Save(RangeAllocator<LocalAllocation>.Range range)
        {
            var bytes = _globalHeap.GetBuffer(_globalHandle, false);

            var save = new byte[range.Size];
            Buffer.BlockCopy(bytes, range.Position, save, 0, range.Size);
            return save;
        }

        void RangeAllocator<LocalAllocation>.IMoveListener.Load(RangeAllocator<LocalAllocation>.Range range, object data)
        {
            var bytes = _globalHeap.GetBuffer(_globalHandle, true);
            Buffer.BlockCopy((byte[])data, 0, bytes, range.Position, range.Size);

            if (range.User.Moveable)
            {
                bytes.WriteWord(range.User.handleOrPtr, (ushort)range.Position);
            }
        }
    }
}
