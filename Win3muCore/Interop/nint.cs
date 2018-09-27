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
    [MappedType]
    public struct nint : IFormattable
    {
        public nint(int value)
        {
            this.value = value;
        }

        public int value;

        public static nint To32(short value)
        {
            return new nint(value);
        }

        public static short To16(nint value)
        {
            return (short)value.value;
        }

        public static implicit operator nint(int value)
        {
            return new nint(value);
        }

        public static implicit operator int(nint value)
        {
            return value.value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return value.ToString(format, formatProvider);
        }
    }
}
