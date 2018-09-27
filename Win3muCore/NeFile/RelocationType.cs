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
    public enum RelocationType : byte
    {
        InternalReference = 0,      // If segment fixed:    p1 = segment number, p2 = segment offset
                                    // If segment moveable: p1 = 0xff p2 = ordinal in segment entry table
        ImportedOrdinal = 1,        // p1 = module index, p2 = ordinal
        ImportedName = 2,           // p1 = module index, p2 = offset to imported name table
        OSFixUp = 3,

        Additive = 0x04,
    }

}
