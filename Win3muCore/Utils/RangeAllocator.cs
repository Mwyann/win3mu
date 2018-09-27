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

#define NO_CHECKED

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Win3muCore
{
    public class RangeAllocator<T>
    {
        // Constructor
        public RangeAllocator(int addressSpaceSize)
        {
            _addressSpaceSize = addressSpaceSize;
            _freeSpace = addressSpaceSize;

            // Create one entry covering the entire address space
            _entries.Add(new Entry()
            {
                allocated = false,
                position = 0,
                size = addressSpaceSize,
                User = default(T),
            });

            Check();
        }

        // Get/set the current address space size
        public int AddressSpaceSize
        {
            get
            {
                return _addressSpaceSize;
            }
            set
            {
                // Redundant?
                if (value == _addressSpaceSize)
                    return;

                // Grow or shrink?
                if (value > _addressSpaceSize)
                {
                    // Grow

                    // Get the last entry and if it's allocated, create a new free entry after it
                    // otherwise extend the existing entry
                    var lastEntry = _entries[_entries.Count - 1];
                    if (lastEntry.allocated)
                    {
                        var e = new Entry();
                        {
                            e.position = _addressSpaceSize;
                            e.size = value - _addressSpaceSize;
                            e.User = default(T);
                            e.allocated = false;
                        }
                        _entries.Add(e);
                    }
                    else
                    {
                        lastEntry.size += value - _addressSpaceSize;
                    }

                    // Update free space and total size
                    _freeSpace += value - _addressSpaceSize;
                    _addressSpaceSize = value;
                }
                else
                {
                    // Shrink
                    throw new NotImplementedException();
                }

                Check();
            }
        }

        // Get the available free space
        public int FreeSpace
        {
            get
            {
                Check();
                return _freeSpace;
            }
        }

        public int LargestFreeSpace
        {
            get
            {
                int max = 0;
                for (int i=0; i<_entries.Count; i++)
                {
                    var e = _entries[i];
                    if (e.size > max)
                        max = e.size;
                }
                return max;
            }
        }

        // Get the number o entries currently in play (free and allocated)
        public int EntryCount
        {
            get
            {
                return _entries.Count;
            }
        }

        public IEnumerable<Range> AllAllocations
        {
            get
            {
                for (int i=0; i<_entries.Count; i++)
                {
                    var e = _entries[i];
                    if (e.allocated)
                        yield return e;
                }
            }
        }

        // Defrag everything, returns true if anything moved
        public bool Defrag()
        {
            int index = 0;
            return Defrag(true, false, 0, ref index);
        }

        // Allocate
        public Range Alloc(int size, bool moveable, bool allowDefrag)
        {
            if (size == 0)
                return null;
            if (size > _freeSpace)
                return null;

            Check();

            // Create the new entry
            var e = new Entry()
            {
                allocated = true,
                size = size,
                moveable = moveable,
            };

            // Try to allocate
            if (!Alloc(e, allowDefrag))
                return null;

            // Return it
            return e;                                      
        }

        // Reallocate
        public bool ReAlloc(Range allocation, int newSize, bool moveable, bool allocDefrag)
        {
            // Otherwise we need a move handler
            if (MoveListener == null)
                throw new InvalidOperationException("RangeAllocator needs a move handler");

            // Get the entry
            var e = allocation as Entry;
            int index = IndexOfAllocation(e);

            // If changing moveable type then alway Defrag and move to get it in the best position
            if (moveable != e.moveable)
            {
                // Realloc with a new entry
                if (ReallocWithNewEntry(e, newSize, moveable))
                    return true;

                // Defrag allowed?
                if (!allocDefrag)
                    return false;

                // Try Defraging and creating the new entry
                return DefragAndRealloc(e, index, newSize, moveable);
            }

            // Try to do it in place (this will handle shrink + grow to following free space)
            if (ReAllocInPlace(allocation, newSize))
            {
                return true;
            }

            // We must be growing
            Debug.Assert(newSize > e.size);

            // Try to use preceeding space
            int localSpace = _entries[index].size;
            if (index > 0 && !_entries[index - 1].allocated)
            {
                var eBefore = _entries[index - 1];
                localSpace += eBefore.size;

                // Do we have room?
                if (localSpace >= newSize)
                {
                    // Work out new position (overlap end of preceeding free entry)
                    int newPosition = eBefore.position + localSpace - newSize;

                    // Move data
                    MoveListener.Move(e, newPosition);

                    // Make as allocated
                    eBefore.size = newPosition - eBefore.position;

                    // Update free space
                    _freeSpace -= (newSize - e.size);

                    // Conver the old allocation to a free list entry
                    e.position = newPosition;
                    e.size = newSize;

                    // Remove the preceeding free entry if not used
                    if (eBefore.size == 0)
                        _entries.RemoveAt(index-1);

                    Check();
                    return true;
                }
            }

            // Try to use preceeding and following space
            if (index + 1 < _entries.Count && !_entries[index + 1].allocated)
            {
                localSpace += _entries[index + 1].size;

                if (localSpace > newSize)
                {
                    var eBefore = _entries[index - 1];
                    var eAfter = _entries[index + 1];

                    // We're using all the prior entry and all the current entry and some of the following
                    Debug.Assert(index >= 1);
                    Debug.Assert(newSize > eBefore.size + e.size);

                    // Move the data
                    MoveListener.Move(e, eBefore.position);

                    // Update the following entry
                    eAfter.position += newSize - (eBefore.size + e.size);
                    eAfter.size -= newSize - (eBefore.size + e.size);
                    System.Diagnostics.Debug.Assert(eAfter.size >= 0);
                    if (eAfter.size==0)
                    {
                        _entries.RemoveAt(index + 1);
                    }

                    // Remove the preceeding entry
                    _entries.RemoveAt(index - 1);

                    // Update the free space
                    _freeSpace -= newSize - e.size;

                    // Update the current entry
                    e.size = newSize;
                    e.position = eBefore.position;

                    // Done!
                    Check();
                    return true;
                }
            }

            if (ReallocWithNewEntry(e, newSize, moveable))
                return true;

            // Allow Defragion?
            if (!allocDefrag)
                return false;

            // Do full Defrag and realloc
            return DefragAndRealloc(e, index, newSize, moveable);
        }

        // Try to reallocate an entry in place
        public bool ReAllocInPlace(Range alloc, int newSize)
        {
            var e = alloc as Entry;

            // Find the entry
            var index = IndexOfAllocation(e);
            if (index < 0)
                throw new ArgumentException("Invalid position passed to allocator");

            if (newSize == e.size)
                return true;

            if (newSize < e.size)
            {
                // Shrinking
                if (index + 1 < _entries.Count && !_entries[index + 1].allocated)
                {
                    // Grow the following free entry down
                    var eNext = _entries[index + 1];
                    eNext.position -= e.size - newSize;
                    eNext.size += e.size - newSize;
                }
                else
                {
                    // Create a new free entry after
                    var eNew = new Entry()
                    {
                        position = e.position + newSize,
                        size = e.size - newSize,
                    };
                    _entries.Insert(index + 1, eNew);
                }

                // Update free space
                _freeSpace += e.size - newSize;

                // Update entry
                e.size = newSize;

                Check();
                return true;
            }
            else
            {
                // Try to grow in place
                if (index + 1 < _entries.Count && !_entries[index + 1].allocated)
                {
                    // Get the next entry
                    var eNext = _entries[index + 1];

                    // Check if we have room to grow it?
                    int totalRoom = e.size + eNext.size;
                    if (newSize > totalRoom)
                        return false;

                    // Yep, either remove or update the following entry
                    if (newSize == totalRoom)
                    {
                        _entries.RemoveAt(index + 1);
                    }
                    else
                    {
                        eNext.position += newSize - e.size;
                        eNext.size -= newSize - e.size;
                    }

                    // Update free space
                    _freeSpace -= newSize - e.size;

                    // Grow the allocation
                    e.size = newSize;

                    Check();
                    return true;
                }
            }

            return false;
        }

        // Free an allocation
        public void Free(Range allocation)
        {
            // Find the entry
            var index = IndexOfAllocation(allocation);
            if (index < 0)
                throw new ArgumentException("Invalid position passed to allocator");

            // Update free space
            _freeSpace += _entries[index].size;

            // Remove the entry
            int unused = 0;
            RemoveEntry(index, ref unused);

            Check();
        }

        // Write to state Console output
        public void Dump()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                Console.WriteLine("{0}: {1}", i, _entries[i]);
            }
        }

        // Must be provided to allow move operations
        public IMoveListener MoveListener;

        // Publicly visible view on an allocation
        public abstract class Range
        {
            public abstract int Position { get; }
            public abstract int Size { get; }
            public abstract bool Moveable { get; }
            public T User;
            public int LockCount;
        }

        public interface IMoveListener
        {
            void Move(Range user, int newPosition);
            object Save(Range user);
            void Load(Range user, object data);
        }

        #region Implementation
        // Private members
        private int _addressSpaceSize;
        private int _freeSpace;
        private List<Entry> _entries = new List<Entry>();

        // Check consistency
        [Conditional("CHECKED")]
        void Check()
        {
            int size = 0;
            int free = 0;
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (!e.allocated)
                {
                    Debug.Assert(e.size > 0);

                    // Check for consecutive free entries
                    if (i > 0)
                        Debug.Assert(_entries[i - 1].allocated);
                    if (i + 1 < _entries.Count)
                        Debug.Assert(_entries[i + 1].allocated);

                    free += e.size;
                }

                Debug.Assert(e.position == size);

                size += e.size;
            }
            System.Diagnostics.Debug.Assert(size == _addressSpaceSize);
            System.Diagnostics.Debug.Assert(free == _freeSpace);
        }

        // Insert an allocation entry
        private bool Alloc(Entry e, bool allowDefrag)
        {
            // Try to allocate in existing free space
            if (e.moveable ? AllocFromTop(e, _entries.Count - 1) : AllocFromBottom(e, 0, allowDefrag))
                return true;

            // Defrag allowed
            if (!allowDefrag || MoveListener == null)
                return false;

            // Defrag 
            int foundIndex = 0;
            if (!Defrag(e.moveable, false, e.size, ref foundIndex))
                return false;

            // Try again
            return e.moveable ? AllocFromTop(e, foundIndex) : AllocFromBottom(e, foundIndex, false);
        }

        // Reallocate by creating a allocation
        private bool ReallocWithNewEntry(Entry e, int newSize, bool moveable)
        {
            // Alloc in a new location
            var newAlloc = Alloc(newSize, moveable, false);
            if (newAlloc != null)
            {
                // If the block is being shrunk (which can happen here if changing between moveable and fixed)
                // the make sure we only move the smaller amount
                int oldLength = e.size;
                if (newSize < e.Size)
                    e.size = newSize;

                // Move the data
                MoveListener.Move(e, newAlloc.Position);

                e.size = oldLength;

                // Remove the old entry
                int unused = 0;
                RemoveEntry(IndexOfAllocation(e), ref unused);

                // Claim back the space
                _freeSpace += e.size;

                // Replace the new entry with the old entry
                int newIndex = IndexOfAllocation(newAlloc);
                e.position = newAlloc.Position;
                e.size = newAlloc.Size;
                e.moveable = newAlloc.Moveable;
                _entries[newIndex] = e;

                // Done
                Check();
                return true;
            }

            return false;
        }

        // Defrag the heap and reallocate
        private bool DefragAndRealloc(Entry e, int index, int newSize, bool moveable)
        {
            // Will it work?
            if (TryDefrag(moveable, index, newSize))
            {
                // Don't save more than the new allocation needs
                int oldLength = e.size;
                if (newSize < e.Size)
                    e.size = Math.Min(newSize, oldLength);

                // Yes - first capture the old data
                var savedData = MoveListener.Save(e);

                // Restore length now that we've saved it
                e.size = oldLength;

                // Free it
                Free(e);

                // Update attributes
                e.moveable = moveable;
                e.size = newSize;

                // Allocate again
                Alloc(e, true);

                // Put the data back
                MoveListener.Load(e, savedData);

                // Done - phew!
                return true;
            }

            return false;
        }

        // Allocate from the top of the heap
        private bool AllocFromTop(Entry entry, int startIndex)
        {
            // Scan all entries looking for a first fit from top
            for (int i=startIndex; i>=0;  i--)
            {
                // Get the entry
                var e = _entries[i];

                // Is it free and big enough?
                if (!e.allocated && e.size >= entry.size)
                {
                    entry.position = e.position + e.size - entry.size;

                    // Insert it
                    _entries.Insert(i + 1, entry);

                    // Shrink the split entry
                    e.size -= entry.size;

                    // Remove the old entry if now empty
                    if (e.size == 0)
                    {
                        _entries.RemoveAt(i);
                    }

                    _freeSpace -= entry.size;

                    return true;
                }
            }

            // No room
            return false;
        }

        // Allocate from the top of the heap
        private bool AllocFromBottom(Entry entry, int startIndex, bool allowDefrag)
        {
            // Scan all entries looking for a first fit from bottom
            for (int i=startIndex; i<_entries.Count; i++)
            {
                // Get the entry
                var e = _entries[i];

                // If we find a movable entry then fail the initial search so
                // we can move the moveable entries and get the fixed entries in at a 
                // lower addres
                if (e.allocated)
                {
                    if (allowDefrag && e.CanMove)
                        return false;

                    continue;
                }

                // Is it free and big enough?
                if (!e.allocated && e.size >= entry.size)
                {
                    entry.position = e.position;

                    // Insert it
                    _entries.Insert(i, entry);

                    // Shrink the split entry
                    e.size -= entry.size;
                    e.position += entry.size;

                    // Remove the old entry if now empty
                    if (e.size == 0)
                    {
                        _entries.RemoveAt(i + 1);
                    }

                    _freeSpace -= entry.size;
                    return true;
                }
            }

            // No room
            return false;
        }

        // Remove an entry by either covering it with an adjacent free entry
        // or creating a new free entry
        private int RemoveEntry(int indexToRemove, ref int adjustIndex)
        {
            var entry = _entries[indexToRemove];

            // Remove the entry we're moving
            bool spaceBefore = indexToRemove > 0 && !_entries[indexToRemove - 1].allocated;
            bool spaceAfter = indexToRemove < _entries.Count - 1 && !_entries[indexToRemove + 1].allocated;

            if (spaceBefore && spaceAfter)
            {
                _entries[indexToRemove - 1].size += entry.size + _entries[indexToRemove + 1].size;
                _entries.RemoveAt(indexToRemove + 1);
                _entries.RemoveAt(indexToRemove);
                adjustIndex -= 2;
                return indexToRemove - 1;
            }
            else if (spaceBefore)
            {
                _entries[indexToRemove - 1].size += entry.size;
                _entries.RemoveAt(indexToRemove);
                adjustIndex--;
                return indexToRemove - 1;
            }
            else if (spaceAfter)
            {
                _entries[indexToRemove + 1].size += entry.size;
                _entries[indexToRemove + 1].position -= entry.size;
                _entries.RemoveAt(indexToRemove);
                adjustIndex--;
                return indexToRemove;
            }
            else
            {
                _entries[indexToRemove] = new Entry()
                {
                    allocated = false,
                    position = entry.position,
                    size = entry.size,
                };
                return indexToRemove;
            }
        }

        public Range AllocationFromPosition(int position)
        {
            int index = IndexFromPositionHelper(position);
            if (index >= 0)
                return _entries[index];

            return null;
        }

        // Find the index of a given allocation
        private int IndexOfAllocation(Range a)
        {
            int index = IndexFromPositionHelper(a.Position);
            if (index>=0)
            {
                if (_entries[index] == a)
                {
                    return index;
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            return -1;
        }

        // Given an allocation position, return it's index in the list of entris
        private int IndexFromPositionHelper(int position)
        {
            // Binary search
            int min = 0;
            int max = _entries.Count - 1;
            while (min < max-1)
            {
                int mid = (min + max) / 2;
                if (_entries[mid].position == position)
                    return mid;
                if (_entries[mid].position > position)
                    max = mid;
                else
                    min = mid;
            }

            if (_entries[min].position == position)
                return min;
            if (min!=max && _entries[max].position == position)
                return max;

            System.Diagnostics.Debug.Assert(false);
            return -1;
        }

        // Check if requied space can be achieved by Defraging
        bool TryDefrag(bool fromTop, int excludeEntry, int requiredSpace)
        {
            // Should only be doing this if we know we have room
            Debug.Assert(requiredSpace > 0);

            // Clone the entry list
            var cloneEntries = new List<Entry>();
            for (int i = 0; i < _entries.Count; i++)
            {
                cloneEntries.Add(new Entry(_entries[i]));
            }

            // Save the current state of the world
            var saveEntries = _entries;
            var saveFreeSpace = _freeSpace;

            // Switch
            _entries = cloneEntries;

            // Remove the entry we're trying to reallocate
            if (excludeEntry > 0)
            {
                _freeSpace += _entries[excludeEntry].size;
                int unused = 0;
                RemoveEntry(excludeEntry, ref unused);
            }

            // Try it
            try
            {
                int unused = 0;
                return Defrag(fromTop, true, requiredSpace, ref unused);
            }
            finally
            {
                // Restore
                _entries = saveEntries;
                _freeSpace = saveFreeSpace;
            }

        }

        // Defrag
        // If requiredSpace > 0, will stop when a free entry of required size if found
        //      and will return its index in foundIndex.
        // If Defraging from top, prefer high address movable allocations
        //      otherwise prefer low address moveable allocation
        // Otherwise Defrags everything and returns if anything changed
        bool Defrag(bool fromTop, bool testMode, int requiredSpace, ref int foundIndex)
        {
            if (MoveListener == null)
                throw new InvalidOperationException("RangeAllocator needs a move handler");

            Check();

            // Which direction to scan when looking for moveable blocks
            int scanDelta = fromTop ? -1 : 1;

            // Check if required space is more than the free space
            if (requiredSpace > _freeSpace)
                return false;

            // Start at the top of the address space looking for free entries
            // For each free entry found, look for a lower moveable entry that
            // will either fit exactly (preferred) or will fit.  If found move 
            // it into the free space
            bool anythingMoved = false;
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                // Ignore allocated entries because we're looking for free entries
                // that allocated entries can be moved into
                var e = _entries[i];
                if (e.allocated)
                    continue;

                // Work out the bounds of the scan when looking for a moveable entry
                int scanFrom;
                int scanTo;
                if (fromTop)
                {
                    scanFrom = i - 1;
                    scanTo = -1;
                }
                else
                {
                    scanFrom = 0;
                    scanTo = i;
                }

                // Look for an moveable entry of exactly the same size, or failing
                // that a smaller entry that will fit
                int entryToMove = -1;
                int smallerEntryToMove = -1;
                Entry e2 = null;
                for (int j = scanFrom; j != scanTo; j += scanDelta)
                {
                    e2 = _entries[j];
                    if (e2.allocated && e2.size <= e.size && e2.CanMove)
                    {
                        if (e2.size == e.size)
                        {
                            entryToMove = j;
                            break;
                        }
                        if (smallerEntryToMove == -1)
                            smallerEntryToMove = j;
                    }
                }

                // If we didn't find an exact fit, revert to a smaller item
                if (entryToMove == -1)
                    entryToMove = smallerEntryToMove;

                // Do we have something to move?
                if (entryToMove >= 0)
                {
                    e2 = _entries[entryToMove];

                    int newPosition = e.position + e.size - e2.size;

                    // Remove the entry
                    var freedSpaceEntry = RemoveEntry(entryToMove, ref i);

                    // Get the current entry again incase in got coalesced down into the just removed entry
                    e = _entries[i];

                    // Shrink the current entry
                    e.size -= e2.size;

                    // Move the data
                    if (!testMode)
                    {
                        MoveListener.Move(e2, e.position + e.size);
                    }

                    // Update the entry we're moving
                    e2.position = newPosition;

                    _entries.Insert(i + 1, e2);

                    // If we still have some free space in this entry then try to move
                    // some more in on the next loop
                    if (e.size > 0)
                        i++;
                    else
                        _entries.RemoveAt(i);

                    // Sanity check
                    anythingMoved = true;
                    Check();

                    // Check if we found enough room
                    if (requiredSpace > 0 && requiredSpace < _entries[entryToMove].size)
                    {
                        Debug.Assert(!_entries[freedSpaceEntry].allocated);
                        foundIndex = freedSpaceEntry;
                        return true;
                    }

                    // Carry on
                    continue;
                }

                // See if we can shuffle up the preceeding entry
                if (i > 0)
                {
                    e2 = _entries[i - 1];
                    if (e2.allocated && e2.CanMove)
                    {
                        Debug.Assert(e2.size > e.size);  // Would have been handled above

                        // Work out new position
                        int combinedSize = e.size + e2.size;
                        int newPosition = e2.position + combinedSize - e2.size;
                        int basePosition = e2.position;

                        // Move the data
                        if (!testMode)
                        {
                            MoveListener.Move(e2, newPosition);
                        }

                        // Swap the two entries
                        e2.position = newPosition;
                        e.position = basePosition;
                        _entries.RemoveAt(i);
                        _entries.Insert(i - 1, e);
                        i--;

                        // Check if we can combine the shifted free entry with a preceeding free entry
                        if (i > 0)
                        {
                            e2 = _entries[i - 1];
                            if (!e2.allocated)
                            {
                                Debug.Assert(e2.position + e2.size == e.position);
                                e2.size += e.size;
                                _entries.RemoveAt(i);
                                i--;

                                if (requiredSpace > 0 && requiredSpace < e2.size)
                                {
                                    // Sanity check
                                    Check();
                                    foundIndex = i;
                                    return true;
                                }
                            }
                        }

                        i++;

                        // Sanity check
                        anythingMoved = true;
                        Check();
                    }
                }
            }

            return requiredSpace > 0 ? false : anythingMoved;
        }

        // Private view on an allocation
        class Entry : Range
        {
            public Entry()
            {
            }

            public Entry(Entry other)
            {
                allocated = other.allocated;
                position = other.position;
                size = other.size;
                User = other.User;
            }

            public bool allocated;
            public int position;
            public int size;
            public bool moveable;

            public bool CanMove
            {
                get { return Moveable && LockCount == 0; }
            }

            public override int Position
            {
                get
                {
                    return position;
                }
            }
            public override int Size
            {
                get
                {
                    return size;
                }
            }
            public override bool Moveable
            {
                get
                {
                    return moveable;
                }
            }
            public override string ToString()
            {
                if (allocated)
                    return string.Format("Used: {0} + {1} = {2} ({3})", position, size, position + size, User);
                else
                    return string.Format("Free: {0} + {1} = {2}", position, size, position + size);
            }
        }


        #endregion


    }
}
