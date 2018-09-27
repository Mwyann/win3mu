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

namespace Win3muCore.NeFile
{
    [Flags]
    public enum AppFlags : byte
    {
        None = 0,
        FullScreeen = 1,    //fullscreen (not aware of Windows/P.M. API)
        WinPMCompat = 2,    //compatible with Windows/P.M. API
        WinPMUses = 3,      //uses Windows/P.M. API

        OS2APP = 1 << 3,          //OS/2 family application
        IMAGEERROR = 1 << 5,      //errors in image/executable
        NONCONFORM = 1 << 6,      //non-conforming program?
        DLL = 1 << 7,             //DLL or driver (SS:SP invalid, CS:IP->Far INIT routine AX=HMODULE,returns AX==0 success, AX!=0 fail)
    };
}
