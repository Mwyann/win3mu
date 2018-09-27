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
    class WndProcBreakPoint : BreakPoint
    {
        public WndProcBreakPoint()
        {
        }
        public override string EditString
        {
            get
            {
                return "wndproc";
            }
        }

        bool _tripped;

        public void Break()
        {
            _tripped = true;
        }

        public override bool ShouldBreak(DebuggerCore debugger)
        {
            bool t = _tripped;
            _tripped = false;
            return t;
        }

        public override string ToString()
        {
            return base.ToString("wndproc");
        }

        ISymbolScope _symbolScope;
        public override Symbol ResolveSymbol(string name)
        {
            var s = _symbolScope.ResolveSymbol(name);
            if (s != null)
                return s;
            return base.ResolveSymbol(name);
        }

        public void Prepare(ISymbolScope scope)
        {
            _symbolScope = scope;
        }
    }
}
    