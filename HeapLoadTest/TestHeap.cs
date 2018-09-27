#define NO_CHECKED

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Win3muCore;

namespace Win3muCoreUnitTests
{
    public class TestHeap : RangeAllocator<TestHeap.TestAllocation>.IMoveListener
    {
        public TestHeap(int size)
        {
            _allocator = new RangeAllocator<TestAllocation>(size);
            _allocator.MoveListener = this;

            _buffer = new byte[size];
        }

        public bool Defrag()
        {
            bool retv = _allocator.Defrag();

            CheckedAll();

            return retv;
        }

        public int FreeSpace { get { return _allocator.FreeSpace; } }

        public int FreeSegmentCount
        {
            get
            {
                return _allocator.EntryCount - _allocations.Count;
            }
        }

        public int AllocationCount
        {
            get
            {
                return _allocations.Count;
            }
        }

        public int AddressSpaceSize
        {
            get { return _allocator.AddressSpaceSize; }
            set
            {
                _allocator.AddressSpaceSize = value;
                var newBuf = new byte[_allocator.AddressSpaceSize];
                Buffer.BlockCopy(_buffer, 0, newBuf, 0, _buffer.Length);
                _buffer = newBuf;
            }
        }

        byte[] Backing
        {
            get
            {
                return _buffer;
            }
        }

        uint _nextAllocID;

        bool GrowHeap(int additionalBytes)
        {
            if (AddressSpaceSize <= 0x10000)
            {
                var newAddressSpaceSize = AddressSpaceSize + additionalBytes;
                if (newAddressSpaceSize > 0x10000)
                    newAddressSpaceSize = 0x10000;
                if (newAddressSpaceSize != AddressSpaceSize)
                {
                    _allocator.AddressSpaceSize = newAddressSpaceSize;

                    var newBuffer = new byte[newAddressSpaceSize];
                    Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _buffer.Length);
                    _buffer = newBuffer;

                    return true;
                }
            }

            return false;
        }

        public TestAllocation Alloc(int size, bool moveable, bool defrag)
        {
            var r = _allocator.Alloc(size, moveable, defrag);
            if (r == null)
            {
                if (GrowHeap(size))
                {
                    r = _allocator.Alloc(size, moveable, defrag);
                }

                if (r == null)
                    return null;
            }

            Debug.Assert(r.Size == size);
            Debug.Assert(r.Moveable == moveable);

            var alloc = new TestAllocation()
            {
                size = size,
                moveable = moveable,
                locked = false,
                id = _nextAllocID++,
                position = r.Position,
                range = r,
            };

            r.User = alloc;

            _allocations.Add(alloc);

            for (uint i = 0; i < size; i++)
            {
                _buffer[alloc.position + i] = (byte)((alloc.id + i) & 0xFF);
            }

            Checked(alloc);
            CheckedAll();

            return alloc;
        }


        [Conditional("CHECKED")]
        void Checked(TestAllocation alloc)
        {
            Check(alloc);
        }

        public void Check(TestAllocation alloc)
        {
            // Check it's allocated
            Debug.Assert(_allocations.Contains(alloc));

            // Check data still good
            for (uint i = 0; i < alloc.size; i++)
            {
                Debug.Assert(_buffer[(int)(alloc.position + i)] == (byte)((alloc.id + i) & 0xFF));
            }
        }

        [Conditional("CHECKED")]
        void CheckedAll()
        {
            CheckAll();
        }

        public void CheckAll()
        {
            for (int i = 0; i < _allocations.Count; i++)
            {
                Check(_allocations[i]);
            }
        }

        public bool ReAlloc(TestAllocation alloc, int newSize, bool moveable, bool defrag)
        {
            Checked(alloc);

            if (!_allocator.ReAlloc(alloc.range, newSize, moveable, defrag))
                return false;

            Debug.Assert(alloc.range.Size == newSize);
            Debug.Assert(alloc.range.Moveable == moveable);

            for (uint i = (uint)alloc.size; i < newSize; i++)
            {
                _buffer[(int)(alloc.range.Position + i)] = (byte)((alloc.id + i) & 0xFF);
            }

            alloc.position = alloc.range.Position;
            alloc.size = newSize;
            alloc.moveable = moveable;

            Checked(alloc);
            CheckedAll();
            return true;
        }

        public void Free(TestAllocation alloc)
        {
            Checked(alloc);
            _allocator.Free(alloc.range);
            _allocations.Remove(alloc);
        }

        RangeAllocator<TestAllocation> _allocator;
        List<TestAllocation> _allocations = new List<TestAllocation>();
        byte[] _buffer;

        public class TestAllocation
        {
            public RangeAllocator<TestAllocation>.Range range;
            public int position;
            public int size;
            public bool locked;
            public bool moveable;
            public uint id;

            public override string ToString()
            {
                return string.Format("#{0}: ({1}+{2}={3}) {4} {5}", id, position, size, position + size, locked ? "locked" : "unlocked", moveable ? "moveable" : "fixed");
            }
        }

        object RangeAllocator<TestAllocation>.IMoveListener.Save(RangeAllocator<TestAllocation>.Range entry)
        {
            var save = new byte[entry.Size];
            Buffer.BlockCopy(_buffer, entry.Position, save, 0, entry.Size);
            return save;
        }

        void RangeAllocator<TestAllocation>.IMoveListener.Load(RangeAllocator<TestAllocation>.Range entry, object data)
        {
            Buffer.BlockCopy((byte[])data, 0, _buffer, entry.Position, entry.Size);
        }

        void RangeAllocator<TestAllocation>.IMoveListener.Move(RangeAllocator<TestAllocation>.Range entry, int newPosition)
        {
            Debug.Assert(entry.Position == entry.User.position);
            Buffer.BlockCopy(_buffer, entry.Position, _buffer, newPosition, entry.Size);
            entry.User.position = newPosition;
        }

        public RangeAllocator<TestAllocation> Allocator
        {
            get
            {
                return _allocator;
            }
        }
    }
}
