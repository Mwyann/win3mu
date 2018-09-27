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
    [MappedTypeAttribute]
    public struct HMENU
    {
        public IntPtr value;

        public static implicit operator HMENU(IntPtr value) { return new HMENU() { value = value }; }
        public static HMENU Null = new HMENU() { value = IntPtr.Zero };
        public static HandleMap Map = new HandleMap();
        public static HMENU To32(ushort hMenu) { return new HMENU() { value = Map.To32(hMenu) }; }
        public static ushort To16(HMENU hMenu) { return Map.To16(hMenu.value); }
        public static void Destroy(ushort hMenu) { Map.Destroy16(hMenu); }
        public static void Trim()
        {
            foreach (var x in Map.GetAll32().Where(x => !IsMenu(x)).ToList())
            {
                Map.Destroy32(x);
            }
        }
        static bool IsMenu(IntPtr hMenu)
        {
            if (hMenu == IntPtr.Zero)
                return false;
            int count = User.GetMenuItemCount(hMenu);
            return count >= 0;
        }
    }
}
