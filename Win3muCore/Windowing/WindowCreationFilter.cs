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
    class WindowCreationFilter : IWndProcFilter
    {
        public WindowCreationFilter(ushort hInstance)
        {
            _hInstance = hInstance;
        }

        ushort _hInstance;
        IntPtr _hWnd;
        bool _createMessageReceived;
        
        public IntPtr? PreWndProc(uint pfnProc, ref Win32.MSG msg, bool dlgProc)
        {
            // Wait for WM_NCCREATE message
            if (msg.message == Win32.WM_NCCREATE && _hWnd == IntPtr.Zero)
            {
                _hWnd = msg.hWnd;

                // Give window class early notification
                WindowClass.OnNcCreate(msg.hWnd);
            }

            // Correct window?
            if (msg.hWnd != _hWnd)
                return null;

            // Setup window instance handle
            if (_hInstance!=0 && HWND.HInstanaceOfHWnd(msg.hWnd) == 0)
            {
                HWND.RegisterHWndToHInstance(msg.hWnd, _hInstance);
                _hInstance = 0;
            }

            // Suppress WM_SIZE messages until WM_CREATE has been received (fixes for crash in Solitaire)
            if (msg.message == Win32.WM_SIZE && !_createMessageReceived)
            {
                return IntPtr.Zero;
            }

            // Flag receipt of WM_CREATE
            if (msg.message == Win32.WM_CREATE)
            {
                _createMessageReceived = true;
            }

            return null;            
        }
    }
}
