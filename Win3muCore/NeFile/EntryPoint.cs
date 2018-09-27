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

namespace Win3muCore.NeFile
{
    public class EntryPoint
    {
        public ushort ordinal;
        public byte flags;
        public byte segmentNumber;
        public ushort segmentOffset;

        public const byte FLAG_EXPORTED = 0x01;
        public const byte FLAG_SHAREDDS = 0x02;
    }

}
