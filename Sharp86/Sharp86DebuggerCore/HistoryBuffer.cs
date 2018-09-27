/*
Sharp86 - 8086 Emulator
Copyright (C) 2017-2018 Topten Software.

Sharp86 is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Sharp86 is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Sharp86.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    class HistoryBuffer<T>
    {
        public HistoryBuffer(int length)
        {
            _buffer = new T[length];
        }

        T[] _buffer;
        long _pos;

        public int Count
        {
            get
            {
                if (_pos > _buffer.Length)
                    return _buffer.Length;
                else
                    return (int)_pos;
            }
        }

        public void Write(T val)
        {
            _buffer[_pos % _buffer.Length] = val;
            _pos++;
        }

        public T this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new ArgumentException("Index out of range");
                if (_pos < _buffer.Length)
                    return _buffer[index];

                return _buffer[(_pos + index) % _buffer.Length];
            }
        }

        public int Capacity
        {
            get
            {
                return _buffer.Length;
            }
            set
            {
                var h = new HistoryBuffer<T>(value);
                for (int i = 0; i < Count; i++)
                {
                    h.Write(this[i]);
                }

                _pos = h._pos;
                _buffer = h._buffer;
            }
        }

        public void Clear()
        {
            _pos = 0;
        }
    }
}
