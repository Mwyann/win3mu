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
    public class Win3muDebugger : Sharp86.TextGuiDebugger, IWndProcFilter
    {
        public Win3muDebugger(Machine machine)
        {
            BreakPoint.RegisterBreakPointType("wndproc", typeof(WndProcBreakPoint));
            _machine = machine;
            _wndProcSymbolScope = new WndProcSymbolScope(_machine);
            _machine.Messaging.RegisterFilter(this);
        }

        Machine _machine;
        WndProcSymbolScope _wndProcSymbolScope;

        protected override void PrepareBreakPoints()
        {
            if (BreakPoints.OfType<WndProcBreakPoint>().Any())
            {
                foreach (var wpbp in BreakPoints.OfType<WndProcBreakPoint>())
                {
                    wpbp.Prepare(_wndProcSymbolScope);
                }

                _machine.NotifyCallWndProc16 = () =>
                {
                    foreach (var wpbp in BreakPoints.OfType<WndProcBreakPoint>())
                    {
                        wpbp.Break();
                    }
                };
            }
            else
            {
                if (_machine != null)
                    _machine.NotifyCallWndProc16 = null;
            }

            base.PrepareBreakPoints();
        }

        IntPtr? IWndProcFilter.PreWndProc(uint pfnProc, ref Win32.MSG msg, bool dlgProc)
        {

            if (msg.message == Win32.WM_KEYDOWN && msg.wParam.ToInt32() == Win32.VK_F9)
            {
                Break();
            }

            return null;
        }
    }
}
