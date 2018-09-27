/*
Sharp86 - 8086 Emulator
Copyright (C) 2017-2018 Topten Software.

Sharp86 is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Sharp86 is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Sharp86.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public static class Clipboard
    {
        public static void SetText(string str)
        {
            unsafe
            {
                var hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)((str.Length + 1) * 2));
                var pGlobal = GlobalLock(hGlobal);
                char* pszDest = (char*)pGlobal;
                fixed (char* pszSrc = str)
                {
                    for (int i=0; i<str.Length; i++)
                    {
                        pszDest[i] = pszSrc[i];
                    }
                    pszDest[str.Length + 1] = '\0';
                }
                GlobalUnlock(hGlobal);

                OpenClipboard(IntPtr.Zero);
                SetClipboardData(CF_UNICODETEXT, hGlobal);
                CloseClipboard();
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool CloseClipboard();

        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GlobalUnlock(IntPtr hMem);

        const uint GMEM_MOVEABLE = 0x02;
        const uint CF_UNICODETEXT = 13;

/*
        const char* output = "Test";
        const size_t len = strlen(output) + 1;
        HGLOBAL hMem = GlobalAlloc(GMEM_MOVEABLE, len);
memcpy(GlobalLock(hMem), output, len);
GlobalUnlock(hMem);
OpenClipboard(0);
EmptyClipboard();
SetClipboardData(CF_TEXT, hMem);
CloseClipboard();
*/
    }
}
