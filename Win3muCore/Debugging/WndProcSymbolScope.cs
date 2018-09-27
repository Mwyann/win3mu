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
using Sharp86;

namespace Win3muCore.Debugging
{
    public class WndProcSymbolScope : GenericSymbolScope
    {
        public WndProcSymbolScope(Machine machine)
        {
            _machine = machine;
            RegisterSymbol("wndproc", () => new FarPointer(_machine.cs, _machine.ip));
            RegisterSymbol("hWnd", () => _machine.ReadWord(_machine.ss, (ushort)(_machine.sp + 4)));
            RegisterSymbol("message", () => _machine.ReadWord(_machine.ss, (ushort)(_machine.sp + 6)));
            RegisterSymbol("wParam", () => _machine.ReadWord(_machine.ss, (ushort)(_machine.sp + 8)));
            RegisterSymbol("lParam", () => _machine.ReadDWord(_machine.ss, (ushort)(_machine.sp + 10)));
        }

        Machine _machine;
    }
}
