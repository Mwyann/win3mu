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

namespace Win3muCore.MessageSemantics
{
    class WM_INITDIALOG : Callable
    {
        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            msg32.wParam = HWND.Map.To32(msg16.wParam);
            msg32.lParam = BitUtils.DWordToIntPtr(msg16.lParam);
            return callback().DWord();
        }

        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            if (!hook)
            {
                if (hInstanceDialog!=0)
                {
                    HWND.RegisterHWndToHInstance(msg32.hWnd, hInstanceDialog);
                }

                var hWndChild = User.GetWindow(msg32.hWnd, Win32.GW_CHILD);
                while (hWndChild.value != IntPtr.Zero)
                {
                    if (hInstanceDialog!=0)
                    {
                        HWND.RegisterHWndToHInstance(hWndChild.value, hInstanceDialog);
                    }

                    uint exStyle = User._GetWindowLong(hWndChild, Win32.GWL_EXSTYLE);
                    uint style = User._GetWindowLong(hWndChild, Win32.GWL_STYLE);
                    if ((exStyle & Win32.WS_EX_CLIENTEDGE) != 0)
                    {
                        User._SetWindowLong(hWndChild.value, Win32.GWL_EXSTYLE, exStyle & ~Win32.WS_EX_CLIENTEDGE);
                        User._SetWindowLong(hWndChild.value, Win32.GWL_STYLE, User._GetWindowLong(hWndChild.value, Win32.GWL_STYLE) | Win32.WS_BORDER);
                        User.SetWindowPos(hWndChild.value, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_FRAMECHANGED | Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOZORDER | Win32.SWP_NOOWNERZORDER);
                    }

                    // Is it a static 
                    if (hInstanceDialog!=0)
                    {
                        if (User.GetClassName(hWndChild).ToLowerInvariant() == "static")
                        {
                            // It it an icon?
                            if ((style & 0x0F) == 0x03)
                            {
                                var iconName = User.GetWindowText(hWndChild);
                                if (!string.IsNullOrEmpty(iconName))
                                {
                                    var icon = machine.User.LoadIcon(hInstanceDialog, new StringOrId(iconName));
                                    if (icon.value != IntPtr.Zero)
                                    {
                                        User._SendMessage(hWndChild.value, Win32.STM_SETICON, icon.value, IntPtr.Zero);
                                    }
                                }
                            }
                        }
                    }


                    // Next
                    hWndChild = User.GetWindow(hWndChild, Win32.GW_HWNDNEXT);
                }

                hInstanceDialog = 0;
            }

            msg16.wParam = HWND.Map.To16(msg32.wParam);
            msg16.lParam = msg32.lParam.DWord();
            return BitUtils.DWordToIntPtr(callback());
        }

        public static ushort hInstanceDialog;
    }
}
