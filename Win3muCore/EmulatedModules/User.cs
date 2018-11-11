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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Sharp86;

namespace Win3muCore
{
    [Module("USER", @"C:\WINDOWS\SYSTEM\USER.EXE")]
    public class User : Module32
    {
        [EntryPoint(0x0001)]
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint MessageBox(HWND hWnd, string text, string caption, nint options);

        // 0002 - OLDEXITWINDOWS
        // 0003 - ENABLEOEMLAYER
        // 0004 - DISABLEOEMLAYER

        [EntryPoint(0x0005)]
        public bool InitApp(ushort hInstance)
        {
            return true;
        }

        [EntryPoint(0x0006)]
        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(nint nExitCode);

        // 0006 - POSTQUITMESSAGE
        // 0007 - EXITWINDOWS

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr SetTimer(IntPtr hWnd, IntPtr nIDEvent, uint uElapse, IntPtr lpTimerFunc);

        [EntryPoint(0x000a)]
        public ushort SetTimer(HWND hWnd, short nIDEvent, ushort uElapse, uint pfnTimerProc)
        {
            if (uElapse < 55)
                uElapse = 55;
            else
                uElapse = (ushort)(((int)(uElapse / 55)) * 55);

            var retv = SetTimer(hWnd.value, (IntPtr)nIDEvent, uElapse, _machine.Messaging.RegisterTimerProc(pfnTimerProc));
            var retv16 = retv.Loword();
            System.Diagnostics.Debug.Assert(retv == (IntPtr)retv16);    // Check we didn't lose anything
            return retv16;
        }


        // 000B - BEAR11


        [DllImport("user32.dll")]
        public static extern bool KillTimer(IntPtr hWnd, IntPtr uIDEvent);

        [EntryPoint(0x000c)]
        public bool KillTimer(HWND hWnd, ushort id)
        {
            return KillTimer(hWnd.value, (IntPtr)id);
        }

        [EntryPoint(0x000d)]
        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();


        // 000E - GETTIMERRESOLUTION

        [EntryPoint(0x000f)]
        [DllImport("kernel32.dll", EntryPoint = "GetTickCount")]
        public static extern uint GetCurrentTime();

        [DllImport("user32.dll")]
        static extern bool ClipCursor(IntPtr ptr);
        [DllImport("user32.dll")]
        static extern bool ClipCursor(ref Win32.RECT ptr);

        [EntryPoint(0x0010)]
        public bool ClipCursor(uint prc)
        {
            if (prc == 0)
            {
                return ClipCursor(IntPtr.Zero);
            }
            else
            {
                var rc = _machine.ReadStruct<Win16.RECT>(prc).Convert();
                return ClipCursor(ref rc);
            }
        }

        [EntryPoint(0x0011)]
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Win32.POINT pt);

        [EntryPoint(0x0012)]
        [DllImport("user32.dll")]
        public static extern HWND SetCapture(HWND hWnd);

        [EntryPoint(0x0013)]
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        // 0014 - SETDOUBLECLICKTIME
        // 0015 - GETDOUBLECLICKTIME

        [EntryPoint(0x0016)]
        [DllImport("user32.dll")]
        public static extern HWND SetFocus(HWND hwnd);

        [EntryPoint(0x0017)]
        [DllImport("user32.dll")]
        public static extern HWND GetFocus();

        [DllImport("user32.dll")]
        static extern IntPtr RemoveProp(IntPtr hWnd, IntPtr lpString);

        [EntryPoint(0x0018)]
        public ushort RemoveProp(HWND hWnd, StringOrId name)
        {
            if (name.Name != null)
            {
                unsafe
                {
                    fixed (char* psz = name.Name)
                    {
                        return RemoveProp(hWnd.value, (IntPtr)psz).ToInt32().Loword();
                    }
                }
            }
            else
            {
                return RemoveProp(hWnd.value, (IntPtr)name.ID).ToInt32().Loword();
            }
        }


        [DllImport("user32.dll")]
        static extern IntPtr GetProp(IntPtr hWnd, IntPtr lpString);

        // 0019 - GETPROP
        [EntryPoint(0x0019)]
        public ushort GetProp(HWND hWnd, StringOrId name)
        {
            if (name.Name != null)
            {
                unsafe
                {
                    fixed (char* psz = name.Name)
                    {
                        return GetProp(hWnd.value, (IntPtr)psz).ToInt32().Loword();
                    }
                }
            }
            else
            {
                return GetProp(hWnd.value, (IntPtr)name.ID).ToInt32().Loword();
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetProp(IntPtr hWnd, IntPtr lpString, IntPtr handle);

        [EntryPoint(0x001a)]
        public bool SetProp(HWND hWnd, StringOrId name, ushort handle)
        {
            if (name.Name!= null)
            {
                unsafe
                {
                    fixed (char* psz = name.Name)
                    {
                        return SetProp(hWnd.value, (IntPtr)psz, (IntPtr)handle);
                    }
                }
            }
            else
            {
                return SetProp(hWnd.value, (IntPtr)name.ID, (IntPtr)handle);
            }
        }

        // 001B - ENUMPROPS


        [DllImport("user32.dll")]
        [EntryPoint(0x001C)]
        public static extern bool ClientToScreen(HWND hWnd, ref Win32.POINT lpPoint);

        [DllImport("user32.dll")]
        [EntryPoint(0x001D)]
        public static extern bool ScreenToClient(HWND hWnd, ref Win32.POINT lpPoint);

        [EntryPoint(0x001e)]
        [DllImport("user32.dll")]
        public static extern HWND WindowFromPoint(Win32.POINT pt);

        [EntryPoint(0x001f)]
        [DllImport("user32.dll")]
        public static extern bool IsIconic(HWND hWnd);

        [EntryPoint(0x0021)]
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(HWND hWnd, out Win32.RECT lpRect);

        [EntryPoint(0x0020)]
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(HWND hWnd, out Win32.RECT lpRect);

        [EntryPoint(0x0022)]
        [DllImport("user32.dll")]
        public static extern bool EnableWindow(HWND hWnd, bool enable);

        [EntryPoint(0x0023)]
        [DllImport("user32.dll")]
        public static extern bool IsWindowEnabled(HWND hWnd);

        [EntryPoint(0x0024)]
        [DllImport("user32.dll")]
        public static extern int GetWindowText(HWND hWnd, [BufSize(+1)] [Out] StringBuilder sb, nint cch);

        public static string GetWindowText(HWND hWnd)
        {
            var buf = new StringBuilder(GetWindowTextLength(hWnd) + 16);
            GetWindowText(hWnd, buf, buf.Capacity);
            return buf.ToString();
        }

        [EntryPoint(0x0025)]
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern void SetWindowText(HWND hWnd, string text);

        [EntryPoint(0x0026)]
        [DllImport("user32.dll")]
        public static extern nint GetWindowTextLength(HWND hWnd);

        [EntryPoint(0x0027)]
        [DllImport("user32.dll")]
        public static extern HDC BeginPaint(HWND hwnd, out Win32.PAINTSTRUCT lpPaint);

        [DllImport("user32.dll", EntryPoint = "EndPaint")]
        public static extern bool _EndPaint(HWND hWnd, [In] ref Win32.PAINTSTRUCT lpPaint);

        [EntryPoint(0x0028)]
        public static bool EndPaint(HWND hWnd, [In] ref Win32.PAINTSTRUCT lpPaint)
        {
            var classStyle = _GetClassLong(hWnd, Win32.GCL_STYLE);
            if ((classStyle & (Win32.CS_CLASSDC | Win32.CS_OWNDC | Win32.CS_PARENTDC))==0)
                HDC.Map.Destroy32(lpPaint.hdc.value);

            return _EndPaint(hWnd, ref lpPaint);
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW")]
        public static extern IntPtr CreateWindowEx(
           uint dwExStyle,
           string lpClassName,
           string lpWindowName,
           uint dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);


        [EntryPoint(0x0029)]
        public ushort CreateWindow(
            string className,
            string windowName,
            uint style,
            short x,
            short y,
            short width,
            short height,
            ushort hWndParent,
            ushort hMenu,
            ushort hInstance,
            uint lpParam
            )
        {
            return CreateWindowEx(0, className, windowName, style,
                x, y, width, height, hWndParent, hMenu, hInstance, lpParam);
        }


        [EntryPoint(0x002A)]
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(HWND hWnd, nint nCmdShow);

        // 002A - SHOWWINDOW
        // 002B - CLOSEWINDOW
        // 002C - OPENICON

        [EntryPoint(0x002d)]
        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(HWND hWnd);

        [EntryPoint(0x002E)]
        [DllImport("user32.dll")]
        public static extern HWND GetParent(HWND hWnd);

        [EntryPoint(0x002f)]
        [DllImport("user32.dll")]
        public static extern bool IsWindow(HWND hWnd);

        [EntryPoint(0x0030)]
        [DllImport("user32.dll")]
        public static extern bool IsChild(HWND hWndParent, HWND hWnd);

        [EntryPoint(0x0031)]
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(HWND hWnd);

        [EntryPoint(0x0032)]
        [DllImport("user32.dll")]
        public static extern HWND FindWindow(string className, string windowName);

        // 0033 - BEAR51
        // 0034 - ANYPOPUP

        [EntryPoint(0x0035)]
        [DllImport("user32.dll")]
        public static extern bool DestroyWindow([Destroyed] HWND hWnd);

        // 0036 - ENUMWINDOWS
        // 0037 - ENUMCHILDWINDOWS

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(HWND hWnd, int x, int y, int w, int h, bool repaint);

        [EntryPoint(0x0038)]
        public bool MoveWindow(HWND hWnd, short x, short y, short w, short h, bool repaint)
        {
            AdjustWindowSize(_GetWindowLong(hWnd, Win32.GWL_STYLE), _GetWindowLong(hWnd, Win32.GWL_EXSTYLE), ref w, ref h);
            return MoveWindow(hWnd, (int)x, (int)y, (int)w, (int)h, repaint);
        }

        [DllImport("user32.dll", EntryPoint = "RegisterClassW")]
        public static extern ushort RegisterClass([In] ref Win32.WNDCLASS lpWndClass);

        [EntryPoint(0x0039)]
        public ushort RegisterClass(ref Win16.WNDCLASS wc16)
        {
            var wc32 = new Win32.WNDCLASS()
            {
                style = wc16.style,
                cbClsExtra = wc16.cbClsExtra,
                cbWndExtra = wc16.cbWndExtra,
                hInstance = IntPtr.Zero,
                hIcon = HGDIOBJ.To32(wc16.hIcon),
                hCursor = HGDIOBJ.To32(wc16.hCursor),
                lpszClassName = _machine.ReadString(wc16.lpszClassName),
            };

            // Background brush might be a real brush or a (HBRUSH)(COLOR_XXX + 1)
            if (wc16.hbrBackground < 32)
                wc32.hbrBackground = (IntPtr)wc16.hbrBackground;
            else
                wc32.hbrBackground = HGDIOBJ.To32(wc16.hbrBackground).value;

            // Get the module
            var module = _machine.ModuleManager.GetModule(wc16.hInstance) as Module16;
            if (module== null)
            {
                throw new InvalidOperationException();
            }

            // Create interfacing info
            var wndClass = new WindowClass(_machine, module, new StringOrId(_machine, wc16.lpszMenuName), wc32, wc16);

            // Create the window procedure
            wc32.lpfnWndProc = wndClass.WndProc32;

            // Register it
            ushort retv = RegisterClass(ref wc32);
            if (retv!=0)
            {
                wndClass.Atom = retv;
                WindowClass.Register(wndClass);
            }
            return retv;

        }

        [EntryPoint(0x003a)]
        [DllImport("user32.dll")]
        public static extern int GetClassName(HWND hWnd, [BufSize(+1)] [Out] StringBuilder sb, nint cch);

        public static string GetClassName(HWND hWnd)
        {
            var sb = new StringBuilder(512);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        [EntryPoint(0x003b)]
        [DllImport("user32.dll")]
        public static extern HWND SetActiveWindow(HWND hWnd);

        [EntryPoint(0x003c)]
        [DllImport("user32.dll")]
        public static extern HWND GetActiveWindow();

        [DllImport("user32.dll")]
        public static extern void ScrollWindow(HWND hWnd, nint dx, nint dy, IntPtr prcRect, IntPtr prcClip);

        [EntryPoint(0x003d)]
        public void ScrollWindow(HWND hWnd, nint dx, nint dy, uint prcRect16, uint prcClip16)
        {
            unsafe
            {
                Win32.RECT* prcRect32 = null;
                Win32.RECT* prcClip32 = null;
                Win32.RECT rcRect;
                Win32.RECT rcClip;
                if (prcRect16 != 0)
                {
                    rcRect = _machine.ReadStruct<Win16.RECT>(prcRect16).Convert();
                    prcRect32 = &rcRect;
                }

                if (prcClip16 != 0)
                {
                    rcClip = _machine.ReadStruct<Win16.RECT>(prcClip16).Convert();
                    prcClip32 = &rcClip;
                }

                ScrollWindow(hWnd, dx, dy, (IntPtr)prcRect32, (IntPtr)prcClip32);
            }
        }

        [EntryPoint(0x003E)]
        [DllImport("user32.dll")]
        public static extern void SetScrollPos(HWND hWnd, nint bar, nint pos, bool redraw);

        [EntryPoint(0x003F)]
        [DllImport("user32.dll")]
        public static extern int GetScrollPos(HWND hWnd, nint bar);

        [EntryPoint(0x0040)]
        [DllImport("user32.dll")]
        public static extern void SetScrollRange(HWND hWnd, nint bar, nint min, nint max, bool redraw);

        [EntryPoint(0x0041)]
        [DllImport("user32.dll")]
        public static extern void GetScrollRange(HWND hWnd, nint bar, ref nint min, ref nint max);

        [EntryPoint(0x0042)]
        [DllImport("user32.dll")]
        public static extern HDC GetDC(HWND hWnd);

        [EntryPoint(0x0043)]
        [DllImport("user32.dll")]
        public static extern HDC GetWindowDC(HWND hWnd);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern int _ReleaseDC(HWND hWnd, HDC hDC);

        [EntryPoint(0x0044)]
        public int ReleaseDC(HWND hWnd, HDC hDC)
        {
            var classStyle = GetClassLong(hWnd, Win32.GCL_STYLE);
            if ((classStyle & (Win32.CS_CLASSDC | Win32.CS_OWNDC | Win32.CS_PARENTDC)) == 0)
                HDC.Map.Destroy32(hDC.value);

            return _ReleaseDC(hWnd, hDC);
        }

        [EntryPoint(0x0045)]
        [DllImport("user32.dll")]
        public static extern HGDIOBJ SetCursor(HGDIOBJ hCursor);

        [EntryPoint(0x0046)]
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(nint x, nint y);

        [EntryPoint(0x0047)]
        [DllImport("user32.dll")]
        public static extern nint ShowCursor(bool show);

        [EntryPoint(0x0048)]
        [DllImport("user32.dll")]
        public static extern void SetRect(out Win32.RECT rc, nint l, nint t, nint r, nint b);

        [EntryPoint(0x0049)]
        [DllImport("user32.dll")]
        public static extern void SetRectEmpty(out Win32.RECT rc);

        [EntryPoint(0x004A)]
        [DllImport("user32.dll")]
        public static extern void CopyRect(out Win32.RECT rcDest, [In] ref Win32.RECT rcSrc);

        [EntryPoint(0x004B)]
        [DllImport("user32.dll")]
        public static extern bool IsRectEmpty([In] ref Win32.RECT rc);

        [EntryPoint(0x004C)]
        [DllImport("user32.dll")]
        public static extern bool PtInRect([In] ref Win32.RECT rc, Win32.POINT pt);

        [EntryPoint(0x004d)]
        [DllImport("user32.dll")]
        public static extern bool OffsetRect(ref Win32.RECT lprc, nint dx, nint dy);

        [EntryPoint(0x004e)]
        [DllImport("user32.dll")]
        public static extern bool InflateRect(ref Win32.RECT lprc, nint dx, nint dy);

        [EntryPoint(0x004f)]
        [DllImport("user32.dll")]
        public static extern bool IntersectRect(out Win32.RECT dest, [In] ref Win32.RECT a, [In] ref Win32.RECT b);

        [EntryPoint(0x0050)]
        [DllImport("user32.dll")]
        public static extern bool UnionRect(out Win32.RECT dest, [In] ref Win32.RECT a, [In] ref Win32.RECT b);

        [EntryPoint(0x0051)]
        [DllImport("user32.dll")]
        public static extern int FillRect(HDC hDC, [In] ref Win32.RECT rc, HGDIOBJ hBrush);

        [EntryPoint(0x0052)]
        [DllImport("user32.dll")]
        public static extern int InvertRect(HDC hDC, [In] ref Win32.RECT rc);

        [EntryPoint(0x0053)]
        [DllImport("user32.dll")]
        public static extern nint FrameRect(HDC hDC, [In] ref Win32.RECT rc, HGDIOBJ hBrush);

        [EntryPoint(0x0054)]
        [DllImport("user32.dll")]
        public static extern bool DrawIcon(HDC hDC, nint x, nint y, HGDIOBJ hIcon);

        [EntryPoint(0x0055)]
        [DllImport("user32.dll", EntryPoint = "DrawTextW")]
        public static extern int DrawText(HDC hDC, [MarshalAs(UnmanagedType.LPWStr)] string lpString, nint nCount, ref Win32.RECT lpRect, nuint uFormat);

        // 0056 - BEAR86
        // 0057 - DIALOGBOX
        [EntryPoint(0x0057)]
        public short DialogBox(ushort hModule, StringOrId name, HWND hWndParent, uint dlgProc)
        {
            return DialogBoxParam(hModule, name, hWndParent, dlgProc, 0);
        }


        [EntryPoint(0x0058)]
        [DllImport("user32.dll")]
        public static extern void EndDialog(HWND hWnd, nint retv);

        [EntryPoint(0x0059)]
        public HWND CreateDialog(ushort hModule, StringOrId template, HWND hWndParent, uint dlgProc)
        {
            return CreateDialogParam(hModule, template, hWndParent, dlgProc, 0);
        }

        [DllImport("user32.dll")]
        public static extern bool IsDialogMessage(HWND hWnd, ref Win32.MSG msg);

        [EntryPoint(0x005a)]
        public bool IsDialogMessage(HWND hWnd, ref Win16.MSG msg)
        {
            Win32.MSG msg32;
            if (!_machine.Messaging.ConvertPostableMessageTo32(ref msg, out msg32))
                return false;

            return IsDialogMessage(hWnd, ref msg32);
        }

        [EntryPoint(0x005b)]
        [DllImport("user32.dll")]
        public static extern HWND GetDlgItem(HWND hWnd, nint id);
        
        [EntryPoint(0x005c)]
//        [DllImport("user32.dll")]
        public void SetDlgItemText(HWND hWnd, nint id, uint textOrHIcon)
        {
            HWND hWndChild = GetDlgItem(hWnd, id);
            SendMessage(HWND.To16(hWndChild), WinCommon.WM_SETTEXT, 0, textOrHIcon);
        }

        [EntryPoint(0x005D)]
        [DllImport("user32.dll")]
        public static extern int GetDlgItemText(HWND hWnd, nint id, [BufSize(+1)] [Out] StringBuilder sb, nint cch);

        [EntryPoint(0x005E)]
        [DllImport("user32.dll")]
        public static extern int SetDlgItemInt(HWND hWnd, nint id, nuint value, bool signed);

        [DllImport("user32.dll")]
        public static extern uint GetDlgItemInt(HWND hWnd, int id, out bool translated, bool signed);

        [EntryPoint(0x005F)]
        public ushort GetDlgItemInt(HWND hWnd, nint id, out short translated, bool signed)
        {
            bool bTemp;
            var retv = (ushort)GetDlgItemInt(hWnd, id, out bTemp, signed);
            translated = (short)(bTemp ? 1 : 0);
            return retv;
        }

        [EntryPoint(0x0060)]
        [DllImport("user32.dll")]
        public static extern bool CheckRadioButton(HWND hWnd, nint first, nint last, nint check);

        [EntryPoint(0x0061)]
        [DllImport("user32.dll")]
        public static extern void CheckDlgButton(HWND hWnd, nint id, nuint check);

        [EntryPoint(0x0062)]
        [DllImport("user32.dll")]
        public static extern nuint IsDlgButtonChecked(HWND hWnd, nint id);

        // 0063 - DLGDIRSELECT

        IEnumerable<string> DlgDirList(string spec, ushort uFileType)
        {
            // Do we actually need to enumerate folder?
            if ((uFileType & Win16.DDL_ATTRIBUTE_MASK)!=0 || (uFileType & Win16.DDL_EXCLUSIVE)==0)
            {
                // Yes, find files...
                _machine.Dos.FindFiles(spec, (byte)(uFileType & Win16.DDL_ATTRIBUTE_MASK));
                DosApi.FINDFILESTRUCT ffs;
                while (_machine.Dos.FindNextFile(out ffs))
                {
                    // Ignore the current folder
                    if (ffs.name == ".")
                        continue;

                    // Exclusive or attribute match?
                    if ((ffs.attribute & uFileType & Win16.DDL_ATTRIBUTE_MASK)!=0 || 
                        ((uFileType & Win16.DDL_EXCLUSIVE)==0 && (ffs.attribute & Win16.DDL_EXPLICIT_MASK) ==0))
                    {
                        if ((ffs.attribute & Win16.DDL_DIRECTORY)!=0)
                            yield return $"[{ffs.name.ToLowerInvariant()}]";
                        else
                            yield return ffs.name.ToLowerInvariant();
                    }
                }

            }

            // Drive letters?
            if ((uFileType & Win16.DDL_DRIVES) != 0)
            {
                foreach (var d in _machine.Dos.EnumDrives())
                {
                    yield return $"[-{(char)('a' + d)}-]";
                }
            }
        }

        [EntryPoint(0x0064)]
        public short DlgDirList(HWND hWnd, uint lpszPath, short idListBox, short idStatic, ushort uFileType)
        {
            try
            {
                // Implicit Exclusive whene enumerating drives
                if ((uFileType & Win16.DDL_DRIVES) != 0)
                {
                    uFileType |= Win16.DDL_EXCLUSIVE;
                }

                // Get the path
                string strPath = _machine.ReadString(lpszPath);
                if (string.IsNullOrEmpty(strPath))
                    strPath = "*";

                // Qualify it
                strPath = _machine.Dos.QualifyPath(strPath);

                // Is it a directory
                string strFolder;
                string strSpec;
                if (_machine.Dos.IsDirectory(strPath))
                {
                    // Yes, look in the directory
                    strFolder = strPath;
                    strSpec = "*";
                }
                else
                {
                    // No, use filename/spec
                    strFolder = System.IO.Path.GetDirectoryName(strPath);
                    strSpec = System.IO.Path.GetFileName(strPath);
                }

                // Change directory
                if (!_machine.Dos.IsDirectory(strFolder))
                    return 0;

                // Change directory
                _machine.Dos.WorkingDirectory = strFolder;
                if (idStatic != 0)
                {
                    var displayFolder = strFolder.ToLowerInvariant();
                    if (displayFolder.Length == 2)
                        displayFolder += "\\";
                    SetWindowText(GetDlgItem(hWnd, idStatic), displayFolder);
                }

                // Write the spec back
                if (lpszPath!=0)
                {
                    _machine.WriteString(lpszPath, strSpec.ToUpperInvariant(), 0xFFFF);
                }

                // Get the list box
                IntPtr hWndListBox = GetDlgItem(hWnd, idListBox).value;
                if (hWndListBox == IntPtr.Zero)
                    return 0;

                // Clear content
                _SendMessage(hWndListBox, Win32.LB_RESETCONTENT, IntPtr.Zero, IntPtr.Zero);

                // Files first
                foreach (var s in DlgDirList(strSpec, (ushort)(uFileType & ~(Win16.DDL_DIRECTORY | Win16.DDL_DRIVES))))
                {
                    _SendMessage(hWndListBox, Win32.LB_ADDSTRING, IntPtr.Zero, s);
                }

                // Directories and drives second
                if ((uFileType & (Win16.DDL_DIRECTORY | Win16.DDL_DRIVES))!=0)
                {
                    foreach (var s in DlgDirList("*", (ushort)((uFileType & (Win16.DDL_DIRECTORY | Win16.DDL_DRIVES)) | Win16.DDL_EXCLUSIVE)))
                    {
                        _SendMessage(hWndListBox, Win32.LB_ADDSTRING, IntPtr.Zero, s);
                    }
                }
            }
            catch (DosError)
            {
                return 0;
            }

            return 0;
        }


        [EntryPoint(0x0065)]
        public uint SendDlgItemMessage(ushort hWnd, short id, ushort msg, ushort wParam, uint lParam)
        {
            var hWndParent32 = HWND.Map.To32(hWnd);
            var hWndChild32 = GetDlgItem(hWndParent32, id);
            return SendMessage(HWND.Map.To16(hWndChild32.value), msg, wParam, lParam);
        }

        [DllImport("user32.dll", EntryPoint = "AdjustWindowRect")]
        public static extern bool _AdjustWindowRect(ref Win32.RECT rc, uint style, bool bMenu);

        [EntryPoint(0x0066)]
        public bool AdjustWindowRect(ref Win32.RECT rc, uint style, bool bMenu)
        {
            _didCallAdjustWindowRect = true;
            return _AdjustWindowRect(ref rc, style, bMenu);
        }

        // 0067 - MAPDIALOGRECT
        [EntryPoint(0x067)]
        [DllImport("user32.dll")]
        public static extern bool MapDialogRect(HWND hWnd, ref Win32.RECT lpRect);

        [EntryPoint(0x0068)]
        [DllImport("user32.dll")]
        public static extern void MessageBeep(nuint type);

        [EntryPoint(0x0069)]
        [DllImport("user32.dll")]
        public static extern bool FlashWindow(HWND hWnd, bool invert);

        [EntryPoint(0x006a)]
        [DllImport("user32.dll")]
        public static extern nint GetKeyState(nint key);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [EntryPoint(0x006b)]
        public uint DefWindowProc(ushort hWnd, ushort message, ushort wParam, uint lParam)
        {
            return _machine.Messaging.CallWndProc32from16(DefWindowProc, hWnd, message, wParam, lParam);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMessage(out Win32.MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [EntryPoint(0x006c)]
        public bool GetMessage(out Win16.MSG msg16, ushort hWnd, ushort wFilterMin, ushort wFilterMax)
        {
            while (true)
            {
                // Call Win32
                Win32.MSG msg32;
                if (!GetMessage(out msg32, HWND.Map.To32(hWnd), wFilterMin, wFilterMax))
                {
                    msg16 = new Win16.MSG();
                    return false;
                }

                //Log.WriteLine("GetMessage: {0}", msg32.message);

                // Try to convert
                if (!_machine.Messaging.ConvertPostableMessageTo16(ref msg32, out msg16))
                {
                    DispatchMessage(ref msg32);
                    continue;   // Unsupported message, just dispatch it
                }

                return true;
            }
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(out Win32.MSG msg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [EntryPoint(0x006D)]
        public bool PeekMessage(out Win16.MSG msg16, HWND hWnd, ushort wFilterMin, ushort wFilterMax, ushort wRemove)
        {
            while (true)
            {
                Win32.MSG msg32;
                if (!PeekMessage(out msg32, hWnd.value, wFilterMin, wFilterMax, wRemove))
                {
                    msg16 = new Win16.MSG();
                    return false;
                }

                // Try to convert to 16 bit message
                if (_machine.Messaging.ConvertPostableMessageTo16(ref msg32, out msg16))
                    return true;

                if (wRemove == 0)
                {
                    PeekMessage(out msg32, hWnd.value, wFilterMin, wFilterMax, 1);
                }

                // We couldn't convert it so we better dispatch it
                DispatchMessage(ref msg32);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);

        [EntryPoint(0x006e)]
        public uint PostMessage(HWND hWnd, ushort msg, ushort wParam, uint lParam)
        {
            var msg16 = new Win16.MSG()
            {
                hWnd = HWND.Map.To16(hWnd.value),
                message = msg,
                wParam = wParam,
                lParam = lParam,
            };
            var msg32 = new Win32.MSG()
            {
                hWnd = hWnd.value,
                message = msg,
            };

            if (_machine.Messaging.ConvertPostableMessageTo32(ref msg16, out msg32))
            {
                PostMessage(hWnd.value, msg32.message, msg32.wParam, msg32.lParam);
                return 0;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessageW")]
        public static extern IntPtr _SendMessage(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessageW")]
        public static extern IntPtr _SendMessage(IntPtr hWnd, uint message, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string strParam);

        [EntryPoint(0x006f)]
        public uint SendMessage(ushort hWnd, ushort msg, ushort wParam, uint lParam)
        {
            return _machine.Messaging.CallWndProc32from16(_SendMessage, hWnd, msg, wParam, lParam);
        }

        [EntryPoint(0x0070)]
        [DllImport("user32.dll")]
        public static extern bool WaitMessage();

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref Win32.MSG lpMsg);

        [EntryPoint(0x0071)]
        public bool TranslateMessage(ref Win16.MSG msg16)
        {
            Win32.MSG msg32;
            if (!_machine.Messaging.ConvertPostableMessageTo32(ref msg16, out msg32))
                return false;

            return TranslateMessage(ref msg32);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref Win32.MSG lpmsg);

        [EntryPoint(0x0072)]
        public uint DispatchMessage(ref Win16.MSG msg16)
        {
            Win32.MSG msg32;
            if (!_machine.Messaging.ConvertPostableMessageTo32(ref msg16, out msg32))
                return 0;

            if (_machine.logDispatchedMessages)
                Log.WriteLine("Dispatching: {0}", MessageNames.NameOfMessage(msg16.message));

            DispatchMessage(ref msg32);

            return 0;       // TODO: Supposed to be the return value from the WNDPROC 
        }

        // 0073 - REPLYMESSAGE
        // 0074 - POSTAPPMESSAGE
        // 0076 - REGISTERWINDOWMESSAGE -- Check MessageSemantics 0xC000 when we implement this

        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageW")]
        public static extern uint _RegisterWindowMessage(string name);

        [EntryPoint(0x0076)]
        public nuint RegisterWindowMessage(string name)
        {
            var retv = _RegisterWindowMessage(name);
            RegisteredWindowMessages.Register(retv);
            return retv.Loword();
        }

        // 0077 - GETMESSAGEPOS
        // 0078 - GETMESSAGETIME


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int hookType, Win32.HOOKPROC lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhook);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();


        class OldHookInfo
        {
            public IntPtr hhook;
            public short hookType;
            public uint hookProc16;
            public Win32.HOOKPROC hookProc32;
            public uint nextHookProc;
        }


        List<OldHookInfo> _oldHookInfos = new List<OldHookInfo>();
        Dictionary<uint, OldHookInfo> _nextHookProcMap = new Dictionary<uint, OldHookInfo>();
        uint _nextFakeHookProc = 0x010010;

        IntPtr OldHookProcProxy(OldHookInfo info, int code, IntPtr wParam, IntPtr lParam)
        {
            switch (info.hookType)
            {
                case Win32.WH_MSGFILTER:
                {
                    // Get the message
                    var msg = Marshal.PtrToStructure<Win32.MSG>(lParam);

                    // Convert it and call 16-bit hook proc
                    IntPtr? retval = null;
                    _machine.Messaging.Convert32to16(ref msg, (msg16) =>
                    {
                        var saveSP = _machine.sp;
                        uint ptrMsg16 = _machine.StackAlloc(msg16);
                        retval = BitUtils.DWordToIntPtr(_machine.CallHookProc16(info.hookProc16, (short)code, 0, ptrMsg16));
                        _machine.sp = saveSP;
                    });

                    if (retval.HasValue)
                    {
                        // Message was processed by hook, use it's return value
                        return retval.Value;
                    }
                    else
                    {
                        // Couldn't convert message so do default hook processing
                        return CallNextHookEx(info.hhook, code, wParam, lParam);
                    }
                }
            }
            throw new NotImplementedException("Hook Proxy");
        }

        [EntryPoint(0x0079)]
        public uint SetWindowsHook(short hookType, uint hookProc16)
        {
            // Allocate fake hook proc to act as the "next" hook the caller should call
            // (really we just use it as a map to a HHOOK for CallNextHookEx)
            while (_nextHookProcMap.ContainsKey(_nextFakeHookProc))
                _nextFakeHookProc += 4;

            // Create hook info
            var hookInfo = new OldHookInfo();
            hookInfo.hookType = hookType;
            hookInfo.nextHookProc = _nextFakeHookProc;
            hookInfo.hookProc16 = hookProc16;
            hookInfo.hookProc32 = (c, w, l) => OldHookProcProxy(hookInfo, c, w, l);

            // Install the hook
            hookInfo.hhook = SetWindowsHookEx(hookType, hookInfo.hookProc32, IntPtr.Zero, GetCurrentThreadId());

            // Update maps
            _nextHookProcMap[_nextFakeHookProc] = hookInfo;
            _oldHookInfos.Add(hookInfo);

            // Return the fake next hook proc
            return hookInfo.nextHookProc;
        }


        [EntryPoint(0x00EA)]
        public bool UnhookWindowsHook(short hookKind, uint hookProc)
        {
            // Find hook info
            var hookInfo = _oldHookInfos.First(x => x.hookType == hookKind && x.hookProc16 == hookProc);
            if (hookInfo == null)
                return false;

            // Unhook it
            UnhookWindowsHookEx(hookInfo.hhook);

            // Clean up
            _oldHookInfos.Remove(hookInfo);
            _nextHookProcMap.Remove(hookInfo.nextHookProc);
            hookInfo.hookProc32 = null;

            return true;
        }

        [EntryPoint(0x00EB)]
        public uint DefHookProc(short code, ushort wParam16, uint lParam16, uint lplpfnProc)
        {
            // Get the HHOOK
            var fakeNextProc = _machine.ReadDWord(lplpfnProc);
            OldHookInfo hookInfo;
            if (!_nextHookProcMap.TryGetValue(fakeNextProc, out hookInfo))
                return 0;

            // Hook type?
            switch (hookInfo.hookType)
            {
                case Win16.WH_MSGFILTER:
                    var msg16 = _machine.ReadStruct<Win16.MSG>(lParam16);
                    IntPtr? retval = null;
                    _machine.Messaging.Convert16to32(ref msg16, (msg32) =>
                    {
                        unsafe
                        {
                            Win32.MSG* pMsg32 = &msg32;
                            retval = CallNextHookEx(hookInfo.hhook, hookInfo.hookType, IntPtr.Zero, (IntPtr)pMsg32);
                        }
                    });

                    System.Diagnostics.Debug.Assert(retval.HasValue);
                    return (uint)(retval.Value == IntPtr.Zero ? 0 : 1);
            }

            throw new NotImplementedException("DefHookProc");
        }



        [DllImport("user32")]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);

        [EntryPoint(0x007a)]
        public uint CallWindowProc(uint pfnProc, ushort hWnd, ushort message, ushort wParam, uint lParam)
        {
            // Get the wnd proc 32
            var pfnProc32 = _machine.Messaging.GetWndProc32(pfnProc, false);

            // Detect the wndclass kind for superclasses
            if (message == Win16.WM_NCCREATE)
            {
                WindowClassKind.DetectSuperClasses(HWND.To32(hWnd), pfnProc32);
            }

            return _machine.Messaging.CallWndProc32from16((hwnd32, message32, wParam32, lParam32) => {

                return CallWindowProc(pfnProc32, hwnd32, message32, wParam32, lParam32);

            }, hWnd, message, wParam, lParam);
        }


        // 007B - CALLMSGFILTER

        [EntryPoint(0x007c)]
        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(HWND hWnd);

        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, [In] ref Win32.RECT lpRect, bool bErase);
        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, [In] IntPtr lpRect, bool bErase);

        [EntryPoint(0x007d)]
        public void InvalidateRect(HWND hWnd, uint prc, bool eraseBackground)
        {
            if (prc==0)
            {
                InvalidateRect(hWnd.value, IntPtr.Zero, eraseBackground);
            }
            else
            {
                var rc = _machine.ReadStruct<Win16.RECT>(prc).Convert();
                InvalidateRect(hWnd.value, ref rc, eraseBackground);
            }
        }

        // 007E - INVALIDATERGN

        [DllImport("user32.dll")]
        public static extern bool ValidateRect(IntPtr hWnd, [In] ref Win32.RECT lpRect);
        [DllImport("user32.dll")]
        public static extern bool ValidateRect(IntPtr hWnd, [In] IntPtr lpRect);

        [EntryPoint(0x007f)]
        public void ValidateRect(HWND hWnd, uint prc)
        {
            if (prc == 0)
            {
                ValidateRect(hWnd.value, IntPtr.Zero);
            }
            else
            {
                var rc = _machine.ReadStruct<Win16.RECT>(prc).Convert();
                ValidateRect(hWnd.value, ref rc);
            }
        }

        // 0080 - VALIDATERGN

        [DllImport("user32.dll")]
        public static extern ushort SetClassWord(HWND hWnd, int index, ushort value);

        [DllImport("user32.dll")]
        public static extern ushort GetClassWord(HWND hWnd, int index);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint _GetClassLong(HWND hWnd, int index);

        [DllImport("user32.dll")]
        public static extern uint SetClassLong(HWND hWnd, int index, uint value);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtrW", CharSet = CharSet.Unicode)]
        static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

        public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetClassLongPtr64(hWnd, nIndex);
            else
            {
                unsafe
                {
                    return new IntPtr((void*)_GetClassLong(hWnd, nIndex));
                }
            }
        }

        [DllImport("user32.dll", EntryPoint = "SetClassLongPtrW", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
            else
            {
                unsafe
                {
                    return new IntPtr((void*)SetClassLong(hWnd, nIndex, (uint)dwNewLong.ToInt32()));
                }
            }
        }




        [EntryPoint(0x0081)]
        public ushort GetClassWord(HWND hWnd, short index)
        {
            if (index >= 0)
            {
                return GetClassWord(hWnd, (int)index);
            }

            switch (index)
            {
                case Win16.GCW_CBCLSEXTRA:
                case Win16.GCW_CBWNDEXTRA:
                    return (ushort)(short)_GetClassLong(hWnd, index);

                case Win16.GCW_HBRBACKGROUND:
                    return HGDIOBJ.To16(GetClassLongPtr(hWnd.value, index));
            }
                                                                   
            throw new NotImplementedException();
        }

        [EntryPoint(0x0082)]
        public ushort SetClassWord(HWND hWnd, short index, ushort value)
        {
            if (index >= 0)
            {
                return SetClassWord(hWnd, (int)index, value);
            }

            switch (index)
            {
                case Win16.GCW_HBRBACKGROUND:
                case Win16.GCW_HCURSOR:
                case Win16.GCW_HICON:
                    return HGDIOBJ.To16(SetClassLongPtr(hWnd.value, index, HGDIOBJ.To32(value).value));
            }

            throw new NotImplementedException();
        }

        [EntryPoint(0x0083)]
        public uint GetClassLong(HWND hWnd, short index)
        {
            if (index >= 0)
            {
                return _GetClassLong(hWnd, (int)index);
            }

            switch (index)
            {
                case Win16.GCW_CBCLSEXTRA:
                case Win16.GCW_CBWNDEXTRA:
                case Win16.GCW_STYLE:
                    return _GetClassLong(hWnd, index);

                case Win16.GCL_WNDPROC:
                {
                    var oldWndProc = GetClassLongPtr(hWnd.value, Win32.GCL_WNDPROC);
                    return _machine.Messaging.GetWndProc16(oldWndProc);
                }
            }

            throw new NotImplementedException();
        }

        [EntryPoint(0x0084)]
        public uint SetClassLong(HWND hWnd, short index, uint value)
        {
            if (index >= 0)
            {
                return SetClassLong(hWnd, (int)index, value);
            }

            throw new NotImplementedException();
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetWindowWord")]
        public static extern ushort _GetWindowWord(HWND hWnd, int nIndex);

        [EntryPoint(0x0085)]
        public ushort GetWindowWord(HWND hWnd, nint nIndex)
        {
            if (nIndex >= 0)
                return _GetWindowWord(hWnd, nIndex);

            switch (nIndex)
            {
                case Win16.GWW_HWNDPARENT:
                    return HWND.To16(GetWindowLongPtr(hWnd.value, Win32.GWL_P_HWNDPARENT));

                case Win16.GWW_HINSTANCE:
                    return HWND.HInstanaceOfHWnd(hWnd.value);

                case Win16.GWW_ID:
                    return _GetWindowLong(hWnd.value, (int)nIndex).Loword();
            }

            throw new NotImplementedException();
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "SetWindowWord")]
        public static extern ushort _SetWindowWord(HWND hWnd, int nIndex, ushort value);

        [EntryPoint(0x0086)]
        public ushort SetWindowWord(HWND hWnd, nint nIndex, ushort value)
        {
            if (nIndex >= 0)
            {
                var retv = _SetWindowWord(hWnd, nIndex, value);
                System.Diagnostics.Debug.Assert(_GetWindowWord(hWnd, nIndex) == value);
                return retv;
            }

            switch (nIndex)
            {
                case Win16.GWW_HINSTANCE:
                {
                    var old = HWND.HInstanaceOfHWnd(hWnd.value);
                    HWND.RegisterHWndToHInstance(hWnd.value, value);
                    return old;
                }

                case Win16.GWW_ID:
                {
                    return _SetWindowLong(hWnd.value, (int)nIndex, value).Loword();
                }
            }

            throw new NotImplementedException();
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetWindowLongW")]
        public static extern uint _GetWindowLong(HWND hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", CharSet = CharSet.Unicode)]
        static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
            {
                unsafe
                {
                    return new IntPtr((void*)_GetWindowLong(hWnd, nIndex));
                }
            }
        }

        [EntryPoint(0x0087)]
        public uint GetWindowLong(HWND hWnd, short gwl)
        {
            if (gwl>=0)
            {
                return _GetWindowLong(hWnd, (int)gwl);
            }

            switch (gwl)
            {
                case Win16.GWL_STYLE:
                case Win16.GWL_EXSTYLE:
                    return _GetWindowLong(hWnd.value, (int)gwl);

                case Win16.GWL_WNDPROC:
                    {
                        var oldWndProc = GetWindowLongPtr(hWnd.value, Win32.GWL_WNDPROC);
                        return _machine.Messaging.GetWndProc16(oldWndProc);
                    }
            }

            if (gwl < 0)
                return 0;

            throw new NotImplementedException();

            //return GetWindowLong(hWnd.value, (int)gwl);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongW")]
        public static extern uint _SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
            {
                unsafe
                {
                    return new IntPtr((void*)_SetWindowLong(hWnd, nIndex, (uint)dwNewLong.ToInt32()));
                }
            }
        }


        [EntryPoint(0x0088)]
        public uint SetWindowLong(HWND hWnd, short gwl, uint value)
        {
            if (gwl>=0)
            {
                var retv = _SetWindowLong(hWnd.value, (int)gwl, value);
                System.Diagnostics.Debug.Assert(_GetWindowLong(hWnd, (int)gwl) == value);
                return retv;
            }

            switch (gwl)
            {
                case Win16.GWL_STYLE:
                case Win16.GWL_EXSTYLE:
                    return _SetWindowLong(hWnd.value, (int)gwl, value);

                case Win16.GWL_WNDPROC:
                {
                    var wndProc = _machine.Messaging.GetWndProc32(value, false);
                    var oldWndProc = SetWindowLongPtr(hWnd.value, Win32.GWL_WNDPROC, wndProc);
                    return _machine.Messaging.GetWndProc16(oldWndProc);
                }
            }

            if (gwl < 0)
                return 0;

            throw new NotImplementedException();

            //return SetWindowLong(hWnd.value, (int)gwl, value);
        }

        // 0089 - OPENCLIPBOARD
        [EntryPoint(0x0089)]
        [DllImport("user32.dll")]
        public static extern bool OpenClipboard(HWND hWndNewOwner);

        // 008A - CLOSECLIPBOARD
        [EntryPoint(0x008A)]
        [DllImport("user32.dll")]
        public static extern bool CloseClipboard();

        // 008B - EMPTYCLIPBOARD
        [EntryPoint(0x008B)]
        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();

        // 008C - GETCLIPBOARDOWNER
        [EntryPoint(0x008C)]
        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardOwner();

        // 008D - SETCLIPBOARDDATA
        [EntryPoint(0x008D)]
        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardData(uint format);

        // 008E - GETCLIPBOARDDATA
        [EntryPoint(0x008E)]
        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardData(uint format);

        // 008F - COUNTCLIPBOARDFORMATS
        [EntryPoint(0x008F)]
        [DllImport("user32.dll")]
        public static extern int CountClipboardFormats();

        // 0090 - ENUMCLIPBOARDFORMATS
        [EntryPoint(0x0090)]
        [DllImport("user32.dll")]
        public static extern uint EnumClipboardFormats(uint format);

        [EntryPoint(0x0091)]
        [DllImport("user32.dll")]
        public static extern HCF RegisterClipboardFormat(string name);

        // 0092 - GETCLIPBOARDFORMATNAME
        [EntryPoint(0x0092)]
        [DllImport("user32.dll")]
        public static extern int GetClipboardFormatName(uint format, [Out] StringBuilder lpszFormatName, int cchMaxCount);

        // 0093 - SETCLIPBOARDVIEWER
        [EntryPoint(0x0093)]
        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardViewer(HWND hWnd);

        // 0094 - GETCLIPBOARDVIEWER
        [EntryPoint(0x0094)]
        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardViewer();

        // 0095 - CHANGECLIPBOARDCHAIN
        [EntryPoint(0x0095)]
        [DllImport("user32.dll")]
        public static extern bool ChangeClipboardChain(HWND hWndRemove, HWND hWndNewNext);

        [EntryPoint(0x0096)]
        public HMENU LoadMenu(ushort hInstance, StringOrId ridName)
        {
            // Get the module
            var module = _machine.ModuleManager.GetModule(hInstance) as Module16;
            if (module == null)
                return HMENU.Null;

            // Get the stream
            var s = module.NeFile.GetResourceStream(Win16.ResourceType.RT_MENU.ToString(), ridName.ToString());
            if (s == null)
                return HMENU.Null;

            // Load it
            return Resources.LoadMenu(s);
        }

        [EntryPoint(0x0097)]
        [DllImport("user32.dll")]
        public static extern HMENU CreateMenu();

        [EntryPoint(0x0098)]
        [DllImport("user32.dll")]
        public static extern bool DestroyMenu([Destroyed] HMENU hMenu);

        [EntryPoint(0x0099)]
        public bool ChangeMenu(HMENU hMenu, ushort pos, uint ptr, ushort id, ushort flags)
        {
            // Reference: http://source.winehq.org/git/wine.git/blob/refs/heads/master:/dlls/user.exe16/user.c#l1079

            if ((flags & Win16.MF_APPEND) != 0)
                return AppendMenu(hMenu, (uint)(flags & ~Win16.MF_APPEND), id, ptr);

            if ((flags & Win16.MF_DELETE) != 0)
                return DeleteMenu(hMenu, pos, (uint)(flags & ~Win16.MF_DELETE));

            if ((flags & Win16.MF_CHANGE) != 0)
                return ModifyMenu(hMenu, pos, (uint)(flags & ~Win16.MF_CHANGE), id, ptr);

            if ((flags & Win16.MF_REMOVE) != 0)
                return RemoveMenu(hMenu, pos, (uint)(flags & Win16.MF_REMOVE));

            return InsertMenu(hMenu, pos, flags, id, ptr);
        }

        [EntryPoint(0x009A)]
        [DllImport("user32.dll")]
        public static extern bool CheckMenuItem(HMENU hMenu, nuint item, nuint flags);

        [EntryPoint(0x009B)]
        [DllImport("user32.dll")]
        public static extern bool EnableMenuItem(HMENU hMenu, nuint item, nuint flags);

        [EntryPoint(0x009c)]
        [DllImport("user32.dll")]
        public static extern HMENU GetSystemMenu(HWND hWnd, bool revert);

        [EntryPoint(0x009d)]
        [DllImport("user32.dll")]
        public static extern HMENU GetMenu(HWND hWnd);

        [EntryPoint(0x009e)]
        [DllImport("user32.dll")]
        public static extern bool SetMenu(HWND hWnd, HMENU hMenu);

        [EntryPoint(0x009f)]
        [DllImport("user32.dll")]
        public static extern HMENU GetSubMenu(HMENU hMenu, nint pos);

        [EntryPoint(0x00A0)]
        [DllImport("user32.dll")]
        public static extern bool DrawMenuBar(HWND hWnd);

        // 00A1 - GETMENUSTRING
        [EntryPoint(0x00A1)]
        [DllImport("user32.dll")]
        public static extern bool GetMenuString(HMENU hMenu, nuint id, [BufSize(+1)] [Out] StringBuilder sb, nint cch, nuint flag);


        // 00A2 - HILITEMENUITEM
        // 00A3 - CREATECARET
        // 00A4 - DESTROYCARET
        // 00A5 - SETCARETPOS
        // 00A6 - HIDECARET
        // 00A7 - SHOWCARET
        // 00A8 - SETCARETBLINKTIME
        // 00A9 - GETCARETBLINKTIME
        // 00AA - ARRANGEICONICWINDOWS


        [EntryPoint(0x00AB)]
        public bool WinHelp(HWND hwndMain, string lpszHelp, ushort usCommand, uint ulData)
        {
            if (usCommand == Win16.HELP_QUIT)
                return true;

            MessageBox(hwndMain, "WinHelp Not Implemented :(", "Win3Emu", Win32.MB_WARNING | Win32.MB_OK);
            return false;
        }

        // 00AC - SWITCHTOTHISWINDOW



        Dictionary<string, HGDIOBJ> _loadedGdiObjs = new Dictionary<string, HGDIOBJ>();

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, string lpCursorName);

        [EntryPoint(0x00AD)]
        public HGDIOBJ LoadCursor(ushort hInstance, StringOrId name)
        {
            IntPtr hIcon32;
            if (hInstance == 0)
            {
                if (name.Name == null)
                {
                    hIcon32 = LoadCursor(IntPtr.Zero, (IntPtr)name.ID);
                }
                else
                {
                    hIcon32 = LoadCursor(IntPtr.Zero, name.Name);
                }
                return new HGDIOBJ() { value = hIcon32 };
            }

            var key = string.Format("cursor:{0:X4}:{1}", hInstance, name.ToString());
            HGDIOBJ hGdiObj;
            if (_loadedGdiObjs.TryGetValue(key, out hGdiObj))
            {
                return hGdiObj;
            }

            // Get the module
            var module = _machine.ModuleManager.GetModule(hInstance) as Module16;
            if (module == null)
                return HGDIOBJ.Null;

            // Get the cursor directory
            var s = module.NeFile.GetResourceStream(Win16.ResourceType.RT_GROUP_CURSOR.ToString(), name.ToString());
            if (s == null)
                return HGDIOBJ.Null;
            var dir = Resources.LoadIconOrCursorGroup(s);

            // Pick best
            var best = dir.PickBestEntry();
            if (best == null)
                return HGDIOBJ.Null;

            // Load the real cursor
            var cursor = module.NeFile.LoadResource(Win16.ResourceType.RT_CURSOR.ToString(), new StringOrId(best.Value.nId).ToString());

            unsafe
            {
                fixed (byte* pBits = cursor)
                {
                    hGdiObj = CreateIconFromResourceEx((IntPtr)pBits, (uint)cursor.Length, false, 0x00030000, 32, 32, 0);
                    _loadedGdiObjs.Add(key, hGdiObj);
                    return hGdiObj;
                }
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

        [EntryPoint(0x00AE)]
        public HGDIOBJ LoadIcon(ushort hInstance, StringOrId name)
        {
            IntPtr hIcon32;
            if (hInstance == 0)
            {
                if (name.Name == null)
                {
                    hIcon32 = LoadIcon(IntPtr.Zero, (IntPtr)name.ID);
                }
                else
                {
                    hIcon32 = LoadIcon(IntPtr.Zero, name.Name);
                }

                return new HGDIOBJ() { value = hIcon32 };
            }

            var key = string.Format("icon:{0:X4}:{1}", hInstance, name.ToString());
            HGDIOBJ hGdiObj;
            if (_loadedGdiObjs.TryGetValue(key, out hGdiObj))
            {
                return hGdiObj;
            }


            // Get the module
            var module = _machine.ModuleManager.GetModule(hInstance) as Module16;
            if (module == null)
                return HGDIOBJ.Null;

            // Get the icon directory
            var s = module.NeFile.GetResourceStream(Win16.ResourceType.RT_GROUP_ICON.ToString(), name.ToString());
            if (s == null)
                return HGDIOBJ.Null;
            var dir = Resources.LoadIconOrCursorGroup(s);

            // Pick best
            var best = dir.PickBestEntry();
            if (best == null)
                return HGDIOBJ.Null;

            // Load the real icon
            var icon = module.NeFile.LoadResource(Win16.ResourceType.RT_ICON.ToString(), new StringOrId(best.Value.nId).ToString());

            unsafe
            {
                fixed (byte* pBits = icon)
                {
                    hGdiObj = CreateIconFromResourceEx((IntPtr)pBits, (uint)icon.Length, true, 0x00030000, 32, 32, 0);
                    _loadedGdiObjs.Add(key, hGdiObj);
                    return hGdiObj;
                }
            }
        }


        [DllImport("user32.dll")]
        public static extern HGDIOBJ CreateIconFromResourceEx(IntPtr pBits, uint cbBits, bool fIcon, uint version, int cxDesired, int cyDesired, uint flags);

        // 00AF - LOADBITMAP
        [EntryPoint(0x00AF)]
        public HGDIOBJ LoadBitmap(ushort hInstance, StringOrId resId)
        {
            // Crack params
            var module = _machine.ModuleManager.GetModule(hInstance);

            // Get the module
            var nativeModule = module as Module16;
            if (nativeModule == null)
                return HGDIOBJ.Null;

            // Find resource
            var buf = nativeModule.NeFile.LoadResource(Win16.ResourceType.RT_BITMAP.ToString(), resId.ToString());
            if (buf == null)
                return HGDIOBJ.Null;

            var hBitmap = Resources.LoadBitmap(buf);

            if (hBitmap.value == IntPtr.Zero && _machine.logWarnings)
            {
                Log.WriteLine("Failed to load bitmap");
            }

            return hBitmap;
        }

        // 00B0 - LOADSTRING

        [EntryPoint(0x00b0)]
        public ushort LoadString(ushort hModule, ushort stringID, uint pszString, ushort cch)
        {
            // Find the resource entry
            var hRsrc = _machine.Kernel.FindResource(hModule, 
                new StringOrId((ushort)(stringID / 16 + 1)),
                new StringOrId(Win16.ResourceType.RT_STRING.ToString())
                );
            if (hRsrc == 0)
                return 0;


            var hData = _machine.Kernel.LoadResource(hModule, hRsrc);
            var buf = _machine.GlobalHeap.GetBuffer(hData, false);

            ushort p = 0;
            for (ushort i=0; i<(ushort)(stringID & 0x0F); i++)
            {
                p += (ushort)(1 + buf.ReadByte(p));
            }

            int len = buf.ReadByte(p);
            var str = Machine.AnsiEncoding.GetString(buf, p + 1, len);

            _machine.Kernel.FreeResource(hData);

            return _machine.WriteString(pszString, str, cch);
        }


        [DllImport("user32.dll")]
        unsafe static extern HACCEL CreateAcceleratorTable(Win32.ACCEL* pAccel, int cEntries);
        
        [EntryPoint(0x00b1)]
        public HACCEL LoadAccelerators(ushort hModule, StringOrId name)
        {
            // Get the module
            var module = _machine.ModuleManager.GetModule(hModule) as Module16;
            if (module == null)
                return HACCEL.Null;

            // Find the resource and work out how many accelerator entries
            var res = module.NeFile.FindResource(Win16.ResourceType.RT_ACCELERATOR.ToString(), name.ToString());
            int count = res.length / Marshal.SizeOf<Win16.ACCEL>();
            if (count == 0)
                return HACCEL.Null;

            // Get the stream
            var stream = module.NeFile.GetResourceStream(Win16.ResourceType.RT_ACCELERATOR.ToString(), name.ToString());

            // Read and convert all
            var list = new List<Win32.ACCEL>();
            for (int i=0; i< count; i++)
            {
                var a16 = stream.ReadStruct<Win16.ACCEL>();
                list.Add(Win32.ACCEL.To32(a16));
                if ((a16.fFlags & Win16.FENDOFRESOURCE) != 0)
                    break;
            }
            var array = list.ToArray();

            // Create the accelerator table
            unsafe
            {
                fixed (Win32.ACCEL* ptr = array)
                {
                    var result = CreateAcceleratorTable(ptr, list.Count);
                    return result;
                }
            }
        }

        // 00B2 - TRANSLATEACCELERATOR
        [DllImport("user32.dll")]
        static extern int TranslateAccelerator(IntPtr hWnd, IntPtr hAccel, ref Win32.MSG msg);

        [EntryPoint(0x00b2)]
        public short TranslateAccelerator(HWND hWnd, HACCEL hAccel, ref Win16.MSG msg)
        {
            if (msg.message == Win32.WM_KEYDOWN || msg.message == Win32.WM_SYSKEYDOWN)
            {
                Win32.MSG msg32;
                if (!_machine.Messaging.ConvertPostableMessageTo32(ref msg, out msg32))
                    return 0;

                return (short)TranslateAccelerator(hWnd.value, hAccel.value, ref msg32);
            }
            else
            {
                return 0;
            }
        }



        [DllImport("user32.dll", EntryPoint ="GetSystemMetrics")]
        public static extern nint _GetSystemMetrics(nint nIndex);

        [EntryPoint(0x00b3)]
        public nint GetSystemMetrics(nint nIndex)
        {
            if (nIndex>=0 && nIndex<WinCommon.SystemMetricNames.Length)
            {
                var name = WinCommon.SystemMetricNames[nIndex];
                int value;
                if (_machine.SystemMetrics.TryGetValue(name, out value))
                    return value;
            }
            return _GetSystemMetrics(nIndex);
        }
                                                           
        [DllImport("user32.dll", EntryPoint = "GetSysColor")]
        public static extern uint _GetSysColor(nint nIndex);

        [EntryPoint(0x00b4)]
        public uint GetSysColor(nint nIndex)
        {
            if (nIndex >= 0 && nIndex < WinCommon.SystemColorNames.Length)
            {
                var name = WinCommon.SystemColorNames[nIndex];
                int value;
                if (_machine.SystemColors.TryGetValue(name, out value))
                    return (uint)value;
            }
            return _GetSysColor(nIndex);
        }

        [DllImport("user32.dll")]                                   
        public static extern IntPtr GetSysColorBrush(int nIndex);


        // 00B5 - SETSYSCOLORS
        // 00B6 - BEAR182
        // 00B7 - GETCARETPOS
        // 00B8 - QUERYSENDMESSAGE
        // 00B9 - GRAYSTRING
        // 00BA - SWAPMOUSEBUTTON
        // 00BB - ENDMENU
        // 00BC - SETSYSMODALWINDOW

        [EntryPoint(0x00BD)]
        public ushort GetSysModalWindow()
        {
            return 0;
        }
        
        // 00BE - GETUPDATERECT

        [EntryPoint(0x00BF)]
        [DllImport("user32.dll")]
        public static extern HWND ChildWindowFromPoint(HWND hWnd, Win32.POINT pt);

        // 00C0 - INSENDMESSAGE
        // 00C1 - ISCLIPBOARDFORMATAVAILABLE
        // 00C2 - DLGDIRSELECTCOMBOBOX
        // 00C3 - DLGDIRLISTCOMBOBOX
        // 00C4 - TABBEDTEXTOUT

        [DllImport("user32.dll")]
        public static extern uint TabbedTextOut(IntPtr hDC, int X, int Y, string str, int cch, int tabs, [In] int[] tabPositions, int tabOrigin);

        [EntryPoint(0x00c4)]
        public uint TabbedTextOut(HDC hDC, nint X, nint Y, uint psz, nint cch, nint tabs, uint ptabPositions, nint tabOrigin)
        {
            // Widen tab positions
            int[] tabPositions = new int[tabs];
            for (int i = 0; i < tabs; i++)
            {
                tabPositions[i] = (short)_machine.ReadWord((uint)(ptabPositions + i * 2));
            }

            return TabbedTextOut(hDC.value, X, Y, _machine.GlobalHeap.ReadCharacters(psz, cch), cch, tabs, tabPositions, tabOrigin);
        }

        [DllImport("user32.dll")]
        public static extern uint GetTabbedTextExtent(IntPtr hDC, string str, int cch, int tabs, [In] int[] tabPositions);

        [EntryPoint(0x00c5)]
        public uint GetTabbedTextExtent(HDC hDC, uint psz, nint cch, nint tabs, uint ptabPositions)
        {
            // Widen tab positions
            int[] tabPositions = new int[tabs];
            for (int i=0; i<tabs; i++)
            {
                tabPositions[i] = (short)_machine.ReadWord((uint)(ptabPositions + i * 2));
            }

            return GetTabbedTextExtent(hDC.value, _machine.GlobalHeap.ReadCharacters(psz, cch), cch, tabs, tabPositions);
        }


        // 00C6 - CASCADECHILDWINDOWS
        // 00C7 - TILECHILDWINDOWS
        // 00C8 - OPENCOMM
        // 00C9 - SETCOMMSTATE
        // 00CA - GETCOMMSTATE
        // 00CB - GETCOMMERROR
        // 00CC - READCOMM
        // 00CD - WRITECOMM

        [EntryPoint(0x00cd)]
        public int WriteComm(nint nCid, uint pszString, nint cbString)
        {
            return -1;
        }


            /*
        [EntryPoint(0x00cd)]
        public int WriteComm(nint nCid, uint pszString, nint cbString)
        {
            int offset;
            var buf = _machine.GlobalHeap.GetBuffer(pszString, false, out offset);
            if (buf == null)
                return -1;

            var str = Machine.AnsiEncoding.GetString(buf, offset, cbString);

            return 0;
        }
        */

        // 00CE - TRANSMITCOMMCHAR
        // 00CF - CLOSECOMM
        // 00D0 - SETCOMMEVENTMASK
        // 00D1 - GETCOMMEVENTMASK
        // 00D2 - SETCOMMBREAK
        // 00D3 - CLEARCOMMBREAK
        // 00D4 - UNGETCOMMCHAR
        // 00D5 - BUILDCOMMDCB
        // 00D6 - ESCAPECOMMFUNCTION
        // 00D7 - FLUSHCOMM
        // 00D8 - USERSEEUSERDO
        // 00D9 - LOOKUPMENUHANDLE
        // 00DA - DIALOGBOXINDIRECT
        // 00DB - CREATEDIALOGINDIRECT
        // 00DC - LOADMENUINDIRECT
        // 00DD - SCROLLDC

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(IntPtr ptr);

        [EntryPoint(0x00DE)]
        public void GetKeyboardState(uint ptr)
        {
            using (var hp = _machine.GlobalHeap.GetHeapPointer(ptr, true))
            {
                GetKeyboardState(hp);
            }
        }


        [DllImport("user32.dll")]
        public static extern bool SetKeyboardState(IntPtr ptr);

        [EntryPoint(0x00DF)]
        public void SetKeyboardState(uint ptr)
        {
            using (var hp = _machine.GlobalHeap.GetHeapPointer(ptr, true))
            {
                SetKeyboardState(hp);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        // 00E0 - GETWINDOWTASK
        [EntryPoint(0x00E0)]
        public ushort GetWindowTask(HWND hWnd)
        {
            // Get the window's process
            uint processId;
            GetWindowThreadProcessId(hWnd.value, out processId);

            // Is it us?
            if (processId == GetCurrentProcessId())
            {
                // Process module as task handle
                return _machine.ProcessModule.hModule;
            }
            else
            {
                return 0;
            }
        }

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(uint dwThreadId, Win32.ENUMTHREADWNDPROC callback, IntPtr lParam);

        [EntryPoint(0x00e1)]
        public bool EnumTaskWindows(ushort hTask, uint callback, uint lParam)
        {
            if (hTask != _machine.ProcessModule.hModule)
            {
                throw new NotImplementedException("Enumerating windows of other tasks not supported");
            }

            return EnumThreadWindows(GetCurrentThreadId(), (w, lp) =>
            {
                _machine.PushWord(HWND.Map.To16(w));
                _machine.PushDWord(lp.DWord());
                _machine.CallVM(callback, "EnumTaskWndProc");
                return _machine.ax != 0;
            }, BitUtils.DWordToIntPtr(lParam));
        }

        // 00E1 - ENUMTASKWINDOWS
        // 00E2 - LOCKINPUT
        // 00E3 - GETNEXTDLGGROUPITEM
        // 00E4 - GETNEXTDLGTABITEM

        [EntryPoint(0x00e5)]
        [DllImport("user32.dll")]
        public static extern HWND GetTopWindow(HWND hWnd);

        // 00E6 - GETNEXTWINDOW
        // 00E7 - GETSYSTEMDEBUGSTATE

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, nuint uFlags);

        [EntryPoint(0x00e8)]
        public bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, short X, short Y, short cx, short cy, nuint uFlags)
        {
            if ((uFlags & Win32.SWP_NOSIZE)==0)
            {
                AdjustWindowSize(_GetWindowLong(hWnd, Win32.GWL_STYLE), _GetWindowLong(hWnd, Win32.GWL_EXSTYLE), ref cx, ref cy);
            }

            return SetWindowPos(hWnd, hWndInsertAfter, (int)X, (int)Y, (int)cx, (int)cy, uFlags);

        }

        // 00E9 - SETPARENT


        [EntryPoint(0x00ec)]
        [DllImport("user32.dll")]
        public static extern HWND GetCapture();

        // 00ED - GETUPDATERGN
        // 00EE - EXCLUDEUPDATERGN

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr DialogBoxIndirectParam(IntPtr hModule, IntPtr pDialogTemplate, IntPtr hWndParent, IntPtr dlgproc, IntPtr lParam);

        [EntryPoint(0x00ef)]
        public short DialogBoxParam(ushort hModule, StringOrId name, HWND hWndParent, uint dlgProc, uint lParam)
        {
            // Get the module
            var module = hModule == 0 ? _machine.ProcessModule : _machine.ModuleManager.GetModule(hModule) as Module16;
            if (module == null)
                return 0;

            // Open the resource
            var strm = module.NeFile.GetResourceStream(Win16.ResourceType.RT_DIALOG.ToString(), name.ToString());
            if (strm == null)
                return 0;

            // Convert template
            var template = Resources.ConvertDialogTemplate(strm);

            // Get 32-bit shim proc
            IntPtr dlgProc32 = _machine.Messaging.GetWndProc32(dlgProc, true);

            // Run the dialog...
            MessageSemantics.WM_INITDIALOG.hInstanceDialog = hModule;
            unsafe
            {
                fixed (byte* pTemplate = template)
                {
                    return (short)DialogBoxIndirectParam(IntPtr.Zero, (IntPtr)pTemplate, hWndParent.value, dlgProc32, (IntPtr)lParam);
                }
            }
        }


        // 00F0 - DIALOGBOXINDIRECTPARAM
        [EntryPoint(0x00F0)]
        public short DialogBoxIndirectParam(ushort hModule, ushort hGlobal, HWND hWndParent, uint dlgProc, uint lParam)
        {
            // Get the module
            var module = _machine.ModuleManager.GetModule(hModule) as Module16;
            if (module == null)
                return 0;

            // Get template bytes
            var template16 = _machine.GlobalHeap.GetBuffer(hGlobal, false);

            // Convert template
            var template = Resources.ConvertDialogTemplate(new System.IO.MemoryStream(template16));

            // Get 32-bit shim proc
            IntPtr dlgProc32 = _machine.Messaging.GetWndProc32(dlgProc, true);

            // Run the dialog...
            MessageSemantics.WM_INITDIALOG.hInstanceDialog = hModule;
            unsafe
            {
                fixed (byte* pTemplate = template)
                {
                    return (short)DialogBoxIndirectParam(IntPtr.Zero, (IntPtr)pTemplate, hWndParent.value, dlgProc32, (IntPtr)lParam);
                }
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr CreateDialogIndirectParam(IntPtr hModule, IntPtr pDialogTemplate, IntPtr hWndParent, IntPtr dlgproc, IntPtr lParam);

        [EntryPoint(0x00f1)]
        public HWND CreateDialogParam(ushort hModule, StringOrId name, HWND hWndParent, uint dlgProc, uint lParam)
        {
            // Get the module
            var module = _machine.ModuleManager.GetModule(hModule) as Module16;
            if (module == null)
                return IntPtr.Zero;

            // Open the resource
            var strm = module.NeFile.GetResourceStream(Win16.ResourceType.RT_DIALOG.ToString(), name.ToString());
            if (strm == null)
                return IntPtr.Zero;

            // Convert template
            var template = Resources.ConvertDialogTemplate(strm);

            // Get 32-bit shim proc
            IntPtr dlgProc32 = _machine.Messaging.GetWndProc32(dlgProc, true);

            // Run the dialog...
            MessageSemantics.WM_INITDIALOG.hInstanceDialog = hModule;
            unsafe
            {
                fixed (byte* pTemplate = template)
                {
                    return CreateDialogIndirectParam(IntPtr.Zero, (IntPtr)pTemplate, hWndParent.value, dlgProc32, (IntPtr)lParam);
                }
            }
        }

        // 00F2 - CREATEDIALOGINDIRECTPARAM
        // 00F3 - GETDIALOGBASEUNITS
        [EntryPoint(0x00F3)]
        [DllImport("user32.dll")]
        public static extern uint GetDialogBaseUnits();

        [EntryPoint(0x00F4)]
        [DllImport("user32.dll")]
        public static extern bool EqualRect(ref Win32.RECT a, ref Win32.RECT b);

        // 00F5 - ENABLECOMMNOTIFICATION
        // 00F6 - EXITWINDOWSEXEC
        // 00F7 - GETCURSOR
        // 00F8 - GETOPENCLIPBOARDWINDOW

        [EntryPoint(0x00F9)]
        [DllImport("user32.dll")]
        public static extern nint GetAsyncKeyState(nint key);

        [EntryPoint(0x00FA)]
        [DllImport("user32.dll")]
        public static extern nuint GetMenuState(HMENU hMenu, nuint uId, nuint uFlags);

        // 00FB - SENDDRIVERMESSAGE
        // 00FC - OPENDRIVER
        // 00FD - CLOSEDRIVER
        // 00FE - GETDRIVERMODULEHANDLE
        // 00FF - DEFDRIVERPROC
        // 0100 - GETDRIVERINFO
        // 0101 - GETNEXTDRIVER
        // 0102 - MAPWINDOWPOINTS
        // 0103 - BEGINDEFERWINDOWPOS
        // 0104 - DEFERWINDOWPOS
        // 0105 - ENDDEFERWINDOWPOS

        [EntryPoint(0x0106)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern HWND GetWindow(HWND hWnd, nuint uCmd);

        [EntryPoint(0x0107)]
        [DllImport("user32.dll")]
        public static extern nint GetMenuItemCount(HMENU hMenu);

        // 0108 - GETMENUITEMID
        // 0109 - SHOWOWNEDPOPUPS

        [EntryPoint(0x010a)]
        public bool SetMessageQueue(short cMessages)
        {
            return true;
        }

        // 010B - SHOWSCROLLBAR
        [EntryPoint(0x010b)]
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowScrollBar(IntPtr hWnd, int wBar, [MarshalAs(UnmanagedType.Bool)] bool bShow);
        // 010C - GLOBALADDATOM
        [EntryPoint(0x010c)]
        [DllImport("kernel32.dll")]
        public static extern uint GlobalAddAtom(string lpString);

        // 010D - GLOBALDELETEATOM
        [EntryPoint(0x010d)]
        [DllImport("kernel32.dll")]
        public static extern uint GlobalDeleteAtom(string lpString);
        // 010E - GLOBALFINDATOM
        [EntryPoint(0x010e)]
        [DllImport("kernel32.dll")]
        public static extern uint GlobalFindAtom(string lpString);
        // 010F - GLOBALGETATOMNAME
        [EntryPoint(0x010f)]
        [DllImport("kernel32.dll")]
        public static extern uint GlobalGetAtomName(ushort nAtom, StringBuilder lpBuffer, int nSize);

        [EntryPoint(0x0110)]
        [DllImport("user32.dll")]
        public static extern bool IsZoomed(HWND hWnd);

        // 0111 - CONTROLPANELINFO
        // 0112 - GETNEXTQUEUEWINDOW
        // 0113 - REPAINTSCREEN
        // 0114 - LOCKMYTASK

        [EntryPoint(0x0115)]
        [DllImport("user32.dll")]
        public static extern nint GetDlgCtrlID(HWND hWnd);

        // 0116 - GETDESKTOPHWND
        // 0117 - OLDSETDESKPATTERN
        // 0118 - SETSYSTEMMENU

        [EntryPoint(0x011a)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ SelectPalette(HDC hDC, HGDIOBJ hPalette, bool forceBackground);

        [EntryPoint(0x011b)]
        [DllImport("gdi32.dll")]
        public static extern nuint RealizePalette(HDC hDC);

        // 011C - GETFREESYSTEMRESOURCES
        // 011D - BEAR285

        [EntryPoint(0x011e)]
        [DllImport("user32.dll")]
        public static extern HWND GetDesktopWindow();

        [EntryPoint(0x011f)]
        [DllImport("user32.dll")]
        public static extern HWND GetLastActivePopup(HWND hWnd);

        // 0120 - GETMESSAGEEXTRAINFO
        // 0121 - KEYBD_EVENT
        // 0122 - REDRAWWINDOW
        // 0123 - SETWINDOWSHOOKEX
        // 0124 - UNHOOKWINDOWSHOOKEX
        // 0125 - CALLNEXTHOOKEX
        // 0126 - LOCKWINDOWUPDATE
        // 012B - MOUSE_EVENT
        // 012D - BOZOSLIVEHERE
        // 0132 - BEAR306
        // 0134 - DEFDLGPROC
        // 0135 - GETCLIPCURSOR
        // 013A - SIGNALPROC
        // 013F - SCROLLWINDOWEX
        // 0140 - SYSERRORBOX
        // 0141 - SETEVENTHOOK
        // 0142 - WINOLDAPPHACKOMATIC
        // 0143 - GETMESSAGE2
        // 0144 - FILLWINDOW
        // 0145 - PAINTRECT
        // 0146 - GETCONTROLBRUSH
        // 014B - ENABLEHARDWAREINPUT
        // 014C - USERYIELD
        // 014D - ISUSERIDLE
        // 014E - GETQUEUESTATUS
        // 014F - GETINPUTSTATE
        // 0150 - LOADCURSORICONHANDLER
        // 0151 - GETMOUSEEVENTPROC
        // 0155 - _FFFE_FARFRAME
        // 0157 - GETFILEPORTNAME
        // 0164 - LOADDIBCURSORHANDLER
        // 0165 - LOADDIBICONHANDLER
        // 0166 - ISMENU
        // 0167 - GETDCEX
        // 016A - DCHOOK
        // 0170 - COPYICON
        // 0171 - COPYCURSOR
        // 0172 - GETWINDOWPLACEMENT
        // 0173 - SETWINDOWPLACEMENT
        // 0174 - GETINTERNALICONHEADER
        // 0175 - SUBTRACTRECT
        // 0190 - FINALUSERINIT
        // 0192 - GETPRIORITYCLIPBOARDFORMAT


        [DllImport("user32.dll")]
        public static extern bool UnregisterClass(string className, IntPtr hInstance);
        [DllImport("user32.dll")]
        public static extern bool UnregisterClass(IntPtr className, IntPtr hInstance);


        [EntryPoint(0x0193)]
        public bool UnregisterClass(StringOrId name, ushort hInstance)
        {
            if (name.Name!=null)
            {
                if (!UnregisterClass(name.Name, IntPtr.Zero))
                    return false;
            }
            else
            {
                if (!UnregisterClass(BitUtils.MakeIntPtr(name.ID, 0), IntPtr.Zero))
                    return false;
            }

            WindowClass.Unregister(name);
            return true;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetClassInfo(IntPtr hInstance, string className, out Win32.get_WNDCLASS wc);

        [EntryPoint(0x0194)]
        public bool GetClassInfo(ushort hInstance, string className, out Win16.WNDCLASS wc)
        {               
            WindowClass windowClass = WindowClass.Find(className);
            if (windowClass == null)
            {
                if (hInstance==0)
                {
                    int size = Marshal.SizeOf<Win16.WNDCLASS>();
                    Win32.get_WNDCLASS wc32;
                    GetClassInfo(IntPtr.Zero, className, out wc32);

                    wc.cbClsExtra = (short)wc32.cbClsExtra;
                    wc.cbWndExtra = (short)wc32.cbWndExtra;
                    wc.hbrBackground = wc32.hbrBackground.Hiword() == 0 ? wc32.hbrBackground.Loword() : HGDIOBJ.To16(wc32.hbrBackground);
                    wc.hCursor = HGDIOBJ.To16(wc32.hCursor);
                    wc.hIcon = HGDIOBJ.To16(wc32.hIcon);
                    wc.hInstance = 0;
                    wc.style = (ushort)wc32.style;
                    wc.lpszClassName = _machine.StringHeap.GetString(Marshal.PtrToStringUni(wc32.lpszClassName));

                    if (wc32.lpszMenuName.Hiword()!=0)
                    {
                        wc.lpszMenuName = _machine.StringHeap.GetString(Marshal.PtrToStringUni(wc32.lpszMenuName));
                    }
                    else
                    {
                        wc.lpszMenuName = wc32.lpszMenuName.DWord();
                    }

                    wc.lpfnWndProc = _machine.Messaging.GetWndProc16(wc32.lpfnWndProc);
                    return true;
                }
                wc = new Win16.WNDCLASS();
                return false;
            }

            wc = windowClass.wc16;
            return true;
        }

        // 0196 - CREATECURSOR
        // 0197 - CREATEICON


        [DllImport("user32.dll")]
        public static extern IntPtr CreateIcon(IntPtr hInstance, int width, int height, byte planes, byte bitsPixel, IntPtr ptrAndBits, IntPtr ptrXorBits);

        [EntryPoint(0x0197)]
        public HGDIOBJ CreateIcon(ushort hInstance, nint width, nint height, byte planes, byte bitsPixel, uint ptrAndBits, uint ptrXorBits)
        {
            using (var hpAndBits = _machine.GlobalHeap.GetHeapPointer(ptrAndBits, false))
            using (var hpXorBits = _machine.GlobalHeap.GetHeapPointer(ptrXorBits, false))
            {
                return CreateIcon(IntPtr.Zero, width, height, planes, bitsPixel, hpAndBits, hpXorBits);
            }
        }


        // 0198 - CREATECURSORICONINDIRECT

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool InsertMenu(HMENU hmenu, uint position, uint flags, IntPtr uIDNewItem, string item_text);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool InsertMenu(HMENU hmenu, uint position, uint flags, IntPtr uIDNewItem, IntPtr dataOrBitmap);

        [EntryPoint(0x019A)]
        public bool InsertMenu(HMENU hMenu, nuint uPosition, nuint uFlags, ushort idOrHMenu, uint dataBitmapOrString)
        {
            // ID or HMENU?
            IntPtr idOrHMenu32;
            if ((uFlags & Win32.MF_POPUP) != 0)
            {
                idOrHMenu32 = HMENU.Map.To32(idOrHMenu);
            }
            else
            {
                idOrHMenu32 = BitUtils.DWordToIntPtr(idOrHMenu);
            }

            // String, bitmap or user data
            IntPtr dataBitmapOrString32;
            if ((uFlags & Win32.MF_BITMAP) != 0)
            {
                dataBitmapOrString32 = HGDIOBJ.To32(dataBitmapOrString.Loword()).value;
                return InsertMenu(hMenu, uPosition, uFlags, idOrHMenu32, dataBitmapOrString32);
            }
            else if ((uFlags & Win32.MF_OWNERDRAW) != 0)
            {
                dataBitmapOrString32 = BitUtils.DWordToIntPtr(dataBitmapOrString);
                return InsertMenu(hMenu, uPosition, uFlags, idOrHMenu32, dataBitmapOrString32);
            }

            string str = _machine.ReadString(dataBitmapOrString);
            return InsertMenu(hMenu, uPosition, uFlags, idOrHMenu32, str);
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool AppendMenu(HMENU hMenu, uint uFlags, IntPtr uIDNewItem, string lpNewItem);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool AppendMenu(HMENU hMenu, uint uFlags, IntPtr uIDNewItem, IntPtr dataOrBitmap);

        [EntryPoint(0x019B)]
        public bool AppendMenu(HMENU hMenu, nuint uFlags, ushort idOrHMenu, uint dataBitmapOrString)
        {
            // ID or HMENU?
            IntPtr idOrHMenu32;
            if ((uFlags & Win32.MF_POPUP) != 0)
            {
                idOrHMenu32 = HMENU.Map.To32(idOrHMenu);
            }
            else
            {
                idOrHMenu32 = BitUtils.DWordToIntPtr(idOrHMenu);
            }

            // String, bitmap or user data
            IntPtr dataBitmapOrString32;
            if ((uFlags & Win32.MF_BITMAP) != 0)
            {
                dataBitmapOrString32 = HGDIOBJ.To32(dataBitmapOrString.Loword()).value;
                return AppendMenu(hMenu, uFlags, idOrHMenu32, dataBitmapOrString32);
            }
            else if ((uFlags & Win32.MF_OWNERDRAW) != 0)
            {
                dataBitmapOrString32 = BitUtils.DWordToIntPtr(dataBitmapOrString);
                return AppendMenu(hMenu, uFlags, idOrHMenu32, dataBitmapOrString32);
            }

            string str = _machine.ReadString(dataBitmapOrString);
            return AppendMenu(hMenu, uFlags, idOrHMenu32, str);
        }


        [EntryPoint(0x019c)]
        [DllImport("user32.dll")]
        public static extern bool RemoveMenu(HMENU hMenu, nuint uPosition, nuint uFlags);

        [EntryPoint(0x019d)]
        [DllImport("user32.dll")]
        public static extern bool DeleteMenu(HMENU hMenu, nuint uPosition, nuint uFlags);

        [DllImport("user32.dll")]
        public static extern bool ModifyMenu(HMENU hMenu, uint uPosition, uint uFlags, IntPtr idOrHMenu, IntPtr dataOrBitmap);

        [DllImport("user32.dll")]
        public static extern bool ModifyMenu(HMENU hMenu, uint uPosition, uint uFlags, IntPtr idOrHMenu, string str);

        [EntryPoint(0x019e)]
        public bool ModifyMenu(HMENU hMenu, nuint uPosition, nuint uFlags, ushort idOrHMenu, uint dataBitmapOrString)
        {
            // ID or HMENU?
            IntPtr idOrHMenu32;
            if ((uFlags & Win32.MF_POPUP)!=0)
            {
                idOrHMenu32 = HMENU.Map.To32(idOrHMenu);
            }
            else
            {
                idOrHMenu32 = BitUtils.DWordToIntPtr(idOrHMenu);
            }

            // String, bitmap or user data
            IntPtr dataBitmapOrString32;
            if ((uFlags & Win32.MF_BITMAP)!=0)
            {
                dataBitmapOrString32 = HGDIOBJ.To32(dataBitmapOrString.Loword()).value;
                return ModifyMenu(hMenu, uPosition, uFlags, idOrHMenu32, dataBitmapOrString32);
            }
            else if ((uFlags & Win32.MF_OWNERDRAW)!=0)
            {
                dataBitmapOrString32 = BitUtils.DWordToIntPtr(dataBitmapOrString);
                return ModifyMenu(hMenu, uPosition, uFlags, idOrHMenu32, dataBitmapOrString32);
            }

            string str = _machine.ReadString(dataBitmapOrString);
            return ModifyMenu(hMenu, uPosition, uFlags, idOrHMenu32, str);
        }


        [EntryPoint(0x019f)]
        [DllImport("user32.dll")]
        public static extern HMENU CreatePopupMenu();

        // 01A0 - TRACKPOPUPMENU
        // 01A1 - GETMENUCHECKMARKDIMENSIONS
        // 01A2 - SETMENUITEMBITMAPS

        // 01A4 - _WSPRINTF
        [EntryPoint(0x01A4)]
        public ushort _wsprintf()
        {
            var buf = _machine.GlobalHeap.GetBuffer(_machine.ss, false);
            var bp = _machine.sp;

            // Get output buffer pointer
            var ptrOutput = buf.ReadDWord(bp + 4);

            // Get format string
            var format = _machine.ReadString(buf.ReadDWord(bp + 8));

            return wvsprintf(ptrOutput, format, BitUtils.MakeDWord((ushort)(bp + 12), _machine.ss));
        }

        // 01A5 - WVSPRINTF
        [EntryPoint(0x01A5)]
        public ushort wvsprintf(uint lpOutput, string lpFormat, uint lpArglist)
        {
            var tokens = sprintf.Parse(lpFormat);

            var paramCount = tokens.Count(x => x.literal == null);
            var parms = new object[paramCount];
            int parmIndex = 0;

            for (int i=0; i<tokens.Count; i++)
            {
                if (tokens[i].literal != null)
                    continue;

                switch (tokens[i].type)
                {
                    case 's':
                        parms[parmIndex++] = _machine.ReadString(_machine.ReadDWord(lpArglist));
                        break;

                    case 'c':
                        parms[parmIndex++] = (char)(_machine.ReadWord(lpArglist) & 0xFF);
                        break;

                    case 'd':
                    case 'i':
                        if (tokens[i].isLong)
                            parms[parmIndex++] = (int)_machine.ReadDWord(lpArglist);
                        else
                            parms[parmIndex++] = (short)_machine.ReadWord(lpArglist);
                        break;

                    case 'u':
                    case 'x':
                    case 'X':
                        if (tokens[i].isLong)
                            parms[parmIndex++] = (uint)_machine.ReadDWord(lpArglist);
                        else
                            parms[parmIndex++] = (ushort)_machine.ReadWord(lpArglist);
                        break;
                }

                lpArglist = (uint)(lpArglist + tokens[i].StackSize);
            }

            var str = sprintf.Format(tokens, parms);
            return _machine.WriteString(lpOutput, str, (ushort)0xFFFF);
        }

        // 01A6 - DLGDIRSELECTEX
        // 01A7 - DLGDIRSELECTCOMBOBOXEX
        // 01AE - LSTRCMP
        [EntryPoint(0x01AE)]
        public nint lstrcmp(string a, string b)
        {
            return string.Compare(a, b, false);
        }

        [EntryPoint(0x01af)]
        public uint AnsiUpper(uint psz)
        {
            var str = _machine.ReadString(psz);
            _machine.WriteString(psz, str.ToUpperInvariant(), (ushort)0xFFFF);
            return psz;

        }

        [EntryPoint(0x01b0)]
        public uint AnsiLower(uint psz)
        {
            var str = _machine.ReadString(psz);
            _machine.WriteString(psz, str.ToLowerInvariant(), (ushort)0xFFFF);
            return psz;

        }

        // 01B1 - ISCHARALPHA
        // 01B2 - ISCHARALPHANUMERIC
        // 01B3 - ISCHARUPPER
        // 01B4 - ISCHARLOWER

        [EntryPoint(0x01b5)]
        public ushort AnsiUpperBuff(uint psz, ushort len)
        {
            int offset;
            var buf = _machine.GlobalHeap.GetBuffer(psz, true, out offset);

            var str = buf.ReadCharacters(offset, Machine.AnsiEncoding, len == 0 ? 0x10000 : len);
            str = str.ToUpperInvariant();

            buf.WriteString(offset, str);
            return (ushort)str.Length;
        }

        [EntryPoint(0x01b6)]
        public ushort AnsiLowerBuff(uint psz, ushort len)
        {
            int offset;
            var buf = _machine.GlobalHeap.GetBuffer(psz, true, out offset);

            var str = buf.ReadCharacters(offset, Machine.AnsiEncoding, len == 0 ? 0x10000 : len);
            str = str.ToLowerInvariant();

            buf.WriteString(offset, str);
            return (ushort)str.Length;
        }

        // 01B6 - ANSILOWERBUFF
        // 01BD - DEFFRAMEPROC
        // 01BF - DEFMDICHILDPROC
        // 01C3 - TRANSLATEMDISYSACCEL

        bool _didCallAdjustWindowRect;
        void AdjustWindowSize(uint style, uint styleEx, ref short width, ref short height)
        {
            if (_didCallAdjustWindowRect)
                return;

            if ((style & Win32.WS_CAPTION) != 0 &&
                    (style & Win32.WS_SIZEBOX) == 0 &&
                    (style & Win32.WS_BORDER) != 0 &&
                    (style & Win32.WS_CHILD) == 0)
            {
                if (width != Win16.CW_USEDEFAULT)
                    width += 14;
                if (height != Win16.CW_USEDEFAULT)
                    height += 14;
            }
        }

        [EntryPoint(0x01C4)]
        public ushort CreateWindowEx(
            uint styleEx,
            string className,
            string windowName,
            uint style,
            short x,
            short y,
            short width,
            short height,
            ushort hWndParent,
            ushort hMenu,
            ushort hInstance,
            uint lpParam
            )
        {
            if (lpParam != 0)
            {
                if (!WindowClass.IsRegistered(className))
                    throw new NotImplementedException("CreateWindow lpParam not supported");
            }

            uint lpCreateStruct = (uint)((_machine.ss << 16) + _machine.sp + 4);

            AdjustWindowSize(style, styleEx, ref width, ref height);

            var filter = new WindowCreationFilter(hInstance);
            _machine.Messaging.RegisterFilter(filter);

            try
            {
                // Create the window
                IntPtr hWnd = CreateWindowEx(styleEx, className, windowName, style,
                    x == Win16.CW_USEDEFAULT ? Win32.CW_USEDEFAULT : x,
                    y == Win16.CW_USEDEFAULT ? Win32.CW_USEDEFAULT : y,
                    width == Win16.CW_USEDEFAULT ? Win32.CW_USEDEFAULT : width,
                    height == Win16.CW_USEDEFAULT ? Win32.CW_USEDEFAULT : height,
                    HWND.Map.To32(hWndParent),
                    ((style & Win16.WS_CHILD) != 0) ? (IntPtr)hMenu : HMENU.To32(hMenu).value,
                    IntPtr.Zero,
                    BitUtils.DWordToIntPtr(lpParam)
                    );

                // Map the window handle
                return HWND.Map.To16(hWnd);
            }
            finally
            {
                _machine.Messaging.RevokeFilter(filter);
            }
        }



        // 01C6 - ADJUSTWINDOWRECTEX
        // 01C7 - GETICONID
        // 01C8 - LOADICONHANDLER

        [EntryPoint(0x01c9)]
        [DllImport("user32.dll")]
        public static extern bool DestroyIcon([Destroyed] HGDIOBJ hIcon);

        [EntryPoint(0x01ca)]
        [DllImport("user32.dll")]
        public static extern bool DestroyCursor([Destroyed] HGDIOBJ hCursor);

        // 01CB - DUMPICON
        // 01CC - GETINTERNALWINDOWPOS
        // 01CD - SETINTERNALWINDOWPOS
        // 01CE - CALCCHILDSCROLL
        // 01CF - SCROLLCHILDREN
        // 01D0 - DRAGOBJECT
        // 01D1 - DRAGDETECT

        [EntryPoint(0x01D2)]
        [DllImport("user32.dll")]
        public static extern void DrawFocusRect(HDC hdc, [In] ref Win32.RECT rc);

        // 01D6 - STRINGFUNC
        

        [EntryPoint(0x01d7)]
        public nint lstrcmpi(string a, string b)
        {
            return string.Compare(a, b, true);
        }                                     

        [EntryPoint(0x01d8)]
        public uint AnsiNext(uint psz)
        {
            // Hack for now
            return psz + 1;
        }

        [EntryPoint(0x01d9)]
        public uint AnsiPrev(uint psz, uint pszStart)
        {
            // Hack for now
            return psz > pszStart ? psz - 1 : pszStart;
        }

        // 01E0 - GETUSERLOCALOBJTYPE
        // 01E1 - HARDWARE_EVENT
        // 01E2 - ENABLESCROLLBAR
        // 01E3 - SYSTEMPARAMETERSINFO
        // 01F3 - WNETERRORTEXT
        // 01F5 - WNETOPENJOB
        // 01F6 - WNETCLOSEJOB
        // 01F7 - WNETABORTJOB
        // 01F8 - WNETHOLDJOB
        // 01F9 - WNETRELEASEJOB
        // 01FA - WNETCANCELJOB
        // 01FB - WNETSETJOBCOPIES
        // 01FC - WNETWATCHQUEUE
        // 01FD - WNETUNWATCHQUEUE
        // 01FE - WNETLOCKQUEUEDATA
        // 01FF - WNETUNLOCKQUEUEDATA
        // 0200 - WNETGETCONNECTION
        // 0201 - WNETGETCAPS
        [EntryPoint(0x0201)]
        public ushort WNETGETCAPS(ushort arg)
        {
            // WNNC_NET_Multinet | WNNC_SUBNET_WinWorkgroups
            return 0x8004;
        }
        // 0202 - WNETDEVICEMODE
        // 0203 - WNETBROWSEDIALOG
        // 0204 - WNETGETUSER
        // 0205 - WNETADDCONNECTION
        // 0206 - WNETCANCELCONNECTION
        // 0207 - WNETGETERROR
        // 0208 - WNETGETERRORTEXT
        // 0209 - WNETENABLE
        // 020A - WNETDISABLE
        // 020B - WNETRESTORECONNECTION
        // 020C - WNETWRITEJOB
        // 020D - WNETCONNECTDIALOG
        // 020E - WNETDISCONNECTDIALOG
        // 020F - WNETCONNECTIONDIALOG
        // 0210 - WNETVIEWQUEUEDIALOG
        // 0211 - WNETPROPERTYDIALOG
        // 0212 - WNETGETDIRECTORYTYPE
        // 0213 - WNETDIRECTORYNOTIFY
        // 0214 - WNETGETPROPERTYTEXT}

    }
}
