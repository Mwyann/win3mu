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
    [Module("GDI", @"C:\WINDOWS\SYSTEM\GDI.EXE")]
    public class Gdi : Module32
    {
        [EntryPoint(0x0001)]
        [DllImport("gdi32.dll")]
        public static extern uint SetBkColor(HDC hDC, uint colorref);

        [EntryPoint(0x0002)]
        [DllImport("gdi32.dll")]
        public static extern nint SetBkMode(HDC hDC, nint mode);

        [EntryPoint(0x0003)]
        [DllImport("gdi32.dll")]
        public static extern nint SetMapMode(HDC hDC, nint mode);

        [EntryPoint(0x0004)]
        [DllImport("gdi32.dll")]
        public static extern nint SetROP2(HDC hDC, nint mode);


        // 0005 - SETRELABS
        // 0006 - SETPOLYFILLMODE

        [EntryPoint(0x0007)]
        [DllImport("gdi32.dll")]
        public static extern nint SetStretchBltMode(HDC hDC, nint mode);

        // 0008 - SETTEXTCHARACTEREXTRA

        [EntryPoint(0x0009)]
        [DllImport("gdi32.dll")]
        public static extern uint SetTextColor(HDC hDC, uint color);

        // 000A - SETTEXTJUSTIFICATION

        [DllImport("gdi32.dll")]
        public static extern bool SetWindowOrgEx(HDC hDC, int x, int y, out Win32.SIZE size);

        [EntryPoint(0x000B)]
        public uint SetWindowOrg(HDC hDC, short x, short y)
        {
            Win32.SIZE size;
            SetWindowOrgEx(hDC, x, y, out size);

            return BitUtils.MakeDWord((ushort)(short)size.Width, (ushort)(short)size.Height);
        }


        [DllImport("gdi32.dll")]
        public static extern bool SetWindowExtEx(HDC hDC, int x, int y, out Win32.SIZE size);

        [EntryPoint(0x000C)]
        public uint SetWindowExt(HDC hDC, short x, short y)
        {
            Win32.SIZE size;
            SetWindowExtEx(hDC, x, y, out size);

            return BitUtils.MakeDWord((ushort)(short)size.Width, (ushort)(short)size.Height);
        }

        [DllImport("gdi32.dll")]
        public static extern bool SetViewportOrgEx(HDC hDC, int x, int y, out Win32.POINT pptOld);

        [EntryPoint(0x000d)]
        public uint SetViewportOrg(HDC hDC, nint x, nint y)
        {
            Win32.POINT old32;
            if (!SetViewportOrgEx(hDC, x, y, out old32))
            {
                return 0;
            }
            return old32.ToDWord();
        }

        [DllImport("gdi32.dll")]
        public static extern bool SetViewportExtEx(HDC hDC, int x, int y, out Win32.SIZE size);

        [EntryPoint(0x000e)]
        public uint SetViewportExt(HDC hDC, short x, short y)
        {
            Win32.SIZE size;
            SetViewportExtEx(hDC, x, y, out size);

            return BitUtils.MakeDWord((ushort)(short)size.Width, (ushort)(short)size.Height);
        }


        // 000F - OFFSETWINDOWORG
        // 0010 - SCALEWINDOWEXT
        // 0011 - OFFSETVIEWPORTORG
        // 0012 - SCALEVIEWPORTEXT

        [EntryPoint(0x0013)]
        [DllImport("gdi32.dll")]
        public static extern bool LineTo(HDC hDC, nint x, nint y);

        [DllImport("gdi32.dll")]
        public static extern bool MoveToEx(HDC hDC, int x, int y, IntPtr pptOld);

        [EntryPoint(0x0014)]
        public bool MoveTo(HDC hDC, nint x, nint y)
        {
            return MoveToEx(hDC, x, y, IntPtr.Zero);
        }

        [EntryPoint(0x0015)]
        [DllImport("gdi32.dll")]
        public static extern bool ExcludeClipRect(HDC hDC, nint left, nint top, nint right, nint bottom);

        [EntryPoint(0x0016)]
        [DllImport("gdi32.dll")]
        public static extern bool IntersectClipRect(HDC hDC, nint left, nint top, nint right, nint bottom);

        [EntryPoint(0x0017)]
        [DllImport("gdi32.dll")]
        public static extern bool Arc(HDC hDC, nint left, nint top, nint right, nint bottom, 
                                                    nint xstart, nint ystart, nint xend, nint yend);

        [EntryPoint(0x0018)]
        [DllImport("gdi32.dll")]
        public static extern bool Ellipse(HDC hDC, nint left, nint top, nint right, nint bottom);

        [EntryPoint(0x0019)]
        [DllImport("gdi32.dll")]
        public static extern bool FloodFill(HDC hDC, nint x, nint y, uint colorRef);

        // 001A - PIE

        [EntryPoint(0x001b)]
        [DllImport("gdi32.dll")]
        public static extern bool Rectangle(HDC hDC, nint l, nint t, nint r, nint b);

        [EntryPoint(0x001c)]
        [DllImport("gdi32.dll")]
        public static extern bool RoundRect(HDC hDC, nint l, nint t, nint r, nint b, nint r1, nint r2);

        [EntryPoint(0x001d)]
        [DllImport("gdi32.dll")]
        public static extern bool PatBlt(HDC hDC, nint l, nint t, nint r, nint b, uint rop);

        [EntryPoint(0x001e)]
        [DllImport("gdi32.dll")]
        public static extern nint SaveDC(HDC hDC);

        [EntryPoint(0x001f)]
        [DllImport("gdi32.dll")]
        public static extern uint SetPixel(HDC hDC, nint x, nint y, uint color);

        // 0020 - OFFSETCLIPRGN

        [DllImport("gdi32.dll")]
        public static extern bool TextOut(HDC hDC, int x, int y, string str, int length);

        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ GetCurrentObject(HDC hDC, int objType);

        [DllImport("gdi32.dll")]
        public static extern bool GdiFlush();

        [EntryPoint(0x0021)]
        public bool TextOut(HDC hDC, nint x, nint y, uint pszString, nint cbString)
        {
            var str = _machine.GlobalHeap.ReadCharacters(pszString, cbString);

            bool retv = TextOut(hDC, x, y, str, cbString);

            // This is needed to get stupid Wordzap rendering correctly (text delays appearance with out it because it's
            // doesn't release the DC before spinning a crazy busy loop)
            GdiFlush();

            return retv;

        }


        [EntryPoint(0x0022)]
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(HDC hDC, nint x, nint y, nint width, nint height, HDC hdcSrc, nint x2, nint y2, uint rop);

        [EntryPoint(0x0023)]
        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(HDC hDC, nint x, nint y, nint width, nint height, HDC hdcSrc, nint x2, nint y2, nint width2, nint height2, uint rop);

        [DllImport("gdi32.dll")]
        static extern bool Polygon(HDC hdc, Win32.POINT[] lpPoints, int nCount);

        [EntryPoint(0x0024)]
        public bool Polygon(HDC hDC, uint ppts, nint nCount)
        {
            var pts = new Win32.POINT[nCount];
            for (int i = 0; i < nCount; i++)
            {
                pts[i] = _machine.ReadStruct<Win16.POINT>((uint)(ppts + i * Marshal.SizeOf<Win16.POINT>())).Convert();
            }

            return Polygon(hDC, pts, nCount);
        }

        [DllImport("gdi32.dll")]
        static extern bool Polyline(HDC hdc, Win32.POINT[] lpPoints, int nCount);

        [EntryPoint(0x0025)]
        public bool Polyline(HDC hDC, uint ppts, nint nCount)
        {
            var pts = new Win32.POINT[nCount];
            for (int i = 0; i < nCount; i++)
            {
                pts[i] = _machine.ReadStruct<Win16.POINT>((uint)(ppts + i * Marshal.SizeOf<Win16.POINT>())).Convert();
            }

            return Polyline(hDC, pts, nCount);
        }

        // 0026 - ESCAPE

        [EntryPoint(0x0027)]
        [DllImport("gdi32.dll")]
        public static extern bool RestoreDC(HDC hDC, nint nSavedDC);

        [EntryPoint(0x0028)]                   
        [DllImport("gdi32.dll")]
        public static extern bool FillRgn(HDC hDC, HGDIOBJ hRgn, HGDIOBJ hBrush);

        [EntryPoint(0x0029)]
        [DllImport("gdi32.dll")]
        public static extern bool FrameRgn(HDC hDC, HGDIOBJ hRgn, nint w, nint h);

        [EntryPoint(0x002a)]
        [DllImport("gdi32.dll")]
        public static extern bool InvertRgn(HDC hDC, HGDIOBJ hRgn);

        [EntryPoint(0x002b)]
        [DllImport("gdi32.dll")]
        public static extern bool PaintRgn(HDC hDC, HGDIOBJ hRgn);

        [EntryPoint(0x002c)]
        [DllImport("gdi32.dll")]
        public static extern nint SelectClipRgn(HDC hDC, HGDIOBJ hRgn);

        [EntryPoint(0x002d)]
        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern HGDIOBJ SelectObject(HDC hdc, HGDIOBJ hgdiobj);

        [EntryPoint(0x002f)]
        [DllImport("gdi32.dll")]
        public static extern nint CombineRgn(HGDIOBJ hrgnDest, HGDIOBJ hrgnSrc1, HGDIOBJ hrgnSrc2, nint combineMode);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);

        [EntryPoint(0x0030)]
        public HGDIOBJ CreateBitmap(nint width, nint height, nuint planes, nuint bitcount, uint ptrBits)
        {
            using (var hp = _machine.GlobalHeap.GetHeapPointer(ptrBits, false))
            {
                return CreateBitmap(width, height, planes, bitcount, hp);
            }
        }

        // 0031 - CREATEBITMAPINDIRECT

        [EntryPoint(0x0032)]
        public HGDIOBJ CreateBrushIndirect(ref Win16.LOGBRUSH brush)
        {
            switch (brush.style)
            {
                case Win16.BS_SOLID:
                    return CreateSolidBrush(brush.color);

                case Win16.BS_PATTERN:
                    return CreatePatternBrush(HGDIOBJ.To32((ushort)brush.hatch));

                case Win16.BS_HATCHED:
                    return CreateHatchBrush(brush.style, brush.color);
            }

            throw new NotImplementedException();
        }                                 

        [EntryPoint(0x0033)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreateCompatibleBitmap(HDC hDC, nint width, nint height);

        [EntryPoint(0x0034)]
        [DllImport("gdi32.dll")]
        public static extern HDC CreateCompatibleDC(HDC hdc);

        [EntryPoint(0x0035)]
        [DllImport("gdi32.dll")]
        public static extern HDC CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, [MustBeNull] IntPtr lpdvmInit);

        // 0036 - CREATEELLIPTICRGN
        // 0037 - CREATEELLIPTICRGNINDIRECT

        [DllImport("gdi32.dll", EntryPoint = "CreateFontW", CharSet = CharSet.Unicode)]
        public static extern HGDIOBJ _CreateFont(int nHeight, int nWidth, int nEscapement, int nOrientation, int fnWeight,
            uint fdwItalic, uint fdwUnderline, uint fdwStrikeOut, uint fdwCharSet, uint fdwOutputPrecision, uint fdwClipPrecision, uint fdwQuality,
            uint fdwPitchAndFamily, string faceName);

        [EntryPoint(0x0038)]
        public HGDIOBJ CreateFont(nint nHeight, nint nWidth, nint nEscapement, nint nOrientation, nint fnWeight,
            byte fdwItalic, byte fdwUnderline, byte fdwStrikeOut, byte fdwCharSet, byte fdwOutputPrecision, byte fdwClipPrecision, byte fdwQuality,
            byte fdwPitchAndFamily, string faceName)
        {
            return _CreateFont(nHeight, nWidth, nEscapement, nOrientation, fnWeight,
                fdwItalic, fdwUnderline, fdwStrikeOut, fdwCharSet, fdwOutputPrecision, fdwClipPrecision, fdwQuality, fdwPitchAndFamily,
                faceName);
        }

        [EntryPoint(0x0039)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreateFontIndirect([In] ref Win32.LOGFONT lf);

        [EntryPoint(0x003A)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreateHatchBrush(nint style, uint colorRef);

        [EntryPoint(0x003C)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreatePatternBrush(HGDIOBJ hBrush);

        [EntryPoint(0x003d)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreatePen(nint penStyle, nint width, uint colorRef);

        [EntryPoint(0x003e)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreatePenIndirect(ref Win32.LOGPEN lp);

        // 003F - CREATEPOLYGONRGN

        [EntryPoint(0x0040)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreateRectRgn(nint left, nint top, nint right, nint bottom);

        // 0041 - CREATERECTRGNINDIRECT

        [EntryPoint(0x0042)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreateSolidBrush(uint colorRef);

        [DllImport("gdi32.dll")]
        static extern bool DPtoLP(IntPtr hdc, [In, Out] Win32.POINT[] lpPoints, int nCount);

        [EntryPoint(0x0043)]
        public bool DPtoLP(HDC hDC, uint ppts, nint nCount)
        {
            // Convert to 32
            var pts = new Win32.POINT[nCount];
            for (int i = 0; i < nCount; i++)
            {
                pts[i] = _machine.ReadStruct<Win16.POINT>((uint)(ppts + i * Marshal.SizeOf<Win16.POINT>())).Convert();
            }

            // Calculate
            bool val = DPtoLP(hDC.value, pts, nCount);

            // And back
            for (int i=0; i< nCount; i++)
            {
                _machine.WriteStruct((uint)(ppts + i * Marshal.SizeOf<Win16.POINT>()), pts[i].Convert());
            }

            return val;
        }


        [EntryPoint(0x0044)]
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC([Destroyed] HDC hdc);

        [EntryPoint(0x0045)]
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject([Destroyed] HGDIOBJ hGdiObj);

        public delegate int EnumFontsDelegate(IntPtr pLogFont, IntPtr pTextMetric, uint dwType, IntPtr lParam);

        [DllImport("gdi32.dll", EntryPoint = "EnumFontsW", CharSet = CharSet.Unicode)]
        public static extern int EnumFonts(IntPtr hDC, string faceName, EnumFontsDelegate enumProc, IntPtr lParam);

        [EntryPoint(0x0046)]
        public nint EnumFonts(HDC hDC, string name, uint enumProc, uint lParam)
        {
            return EnumFonts(hDC.value, name, (pLogFont, pTextMetric, dwType, lp) =>
            {
                var lf = Marshal.PtrToStructure<Win32.LOGFONT>(pLogFont);
                var tm = Marshal.PtrToStructure<Win32.TEXTMETRIC>(pTextMetric);

                var plf16 = _machine.SysAlloc(Win32.LOGFONT.To16(lf));
                var ptm16 = _machine.SysAlloc(Win32.TEXTMETRIC.To16(tm));

                _machine.PushDWord(plf16);
                _machine.PushDWord(ptm16);
                _machine.PushWord(dwType.Loword());
                _machine.PushDWord(lParam);

                _machine.CallVM(enumProc, "EnumFontsProc");

                _machine.SysFree(plf16);
                _machine.SysFree(ptm16);

                return _machine.ax;

            }, IntPtr.Zero);
        }

        // 0047 - ENUMOBJECTS
        // 0048 - EQUALRGN
        // 0049 - EXCLUDEVISRECT

        [DllImport("gdi32.dll")]
        public static extern int GetBitmapBits(IntPtr hBitmap, int cbBuffer, IntPtr pBuffer);

        [EntryPoint(0x004a)]
        public int GetBitmapBits(HGDIOBJ hBitmap, int cbBuffer, uint pBuffer)
        {
            using (var hp = _machine.GlobalHeap.GetHeapPointer(pBuffer, true))
            {
                return GetBitmapBits(hBitmap.value, cbBuffer, hp);
            }
        }

        [EntryPoint(0x004b)]
        [DllImport("gdi32.dll")]
        public static extern uint GetBkColor(HDC hDC);

        [EntryPoint(0x004c)]
        [DllImport("gdi32.dll")]
        public static extern nint GetBkMode(HDC hDC);

        [EntryPoint(0x004d)]
        [DllImport("gdi32.dll")]
        public static extern nint GetClipBox(HDC hDC, out Win32.RECT rc);

        [DllImport("gdi32.dll")]
        public static extern bool GetCurrentPositionEx(HDC hdc, out Win32.POINT lpPoint);

        [EntryPoint(0x004e)]
        public uint GetCurrentPosition(HDC hDC)
        {
            Win32.POINT pt;
            GetCurrentPositionEx(hDC, out pt);
            return pt.ToDWord();
        }

        [DllImport("gdi32.dll")]
        public static extern bool GetDCOrgEx(HDC hDC, out Win32.POINT pptOld);

        [EntryPoint(0x004f)]
        public uint GetDCOrg(HDC hDC)
        {
            Win32.POINT pt;
            GetDCOrgEx(hDC, out pt);
            return pt.ToDWord();
        }

        [DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps")]
        public static extern nint _GetDeviceCaps(HDC hDC, nint cap);

        [EntryPoint(0x0050)]
        public nint GetDeviceCaps(ushort hDC, nint cap)
        {
            // Tested on WinXP with 16, 24 and 32-bit color all return
            // 2048 for num colors on 16-bit windows
            // Assumes hDC is a screen DC
            // Also fixes tetris asking for NumColors on a released DC
            if (cap == Win16.NUMCOLORS && (!HDC.Map.IsValid16(hDC) || _GetDeviceCaps(HDC.To32(hDC), Win16.TECHNOLOGY) == Win16.DT_RASDISPLAY))
            {
                return 2048;
            }

            return _GetDeviceCaps(HDC.To32(hDC), cap);
        }

        [EntryPoint(0x0051)]
        [DllImport("gdi32.dll")]
        public static extern nint GetMapMode(HDC hDC);

        [DllImport("gdi32.dll")]
        static extern uint GetObjectType(HGDIOBJ h);

        [DllImport("gdi32.dll")]
        static extern int GetObject(HGDIOBJ hgdiobj, int cbBuffer, IntPtr lpvObject);

        [DllImport("gdi32.dll", EntryPoint = "GetObjectW", CharSet = CharSet.Unicode)]
        static extern int GetObject(HGDIOBJ hgdiobj, int cbBuffer, out Win32.LOGFONT lpvObject);

        [EntryPoint(0x0052)]
        public short GetObject(HGDIOBJ hgdiobj, short cbBuffer, uint ptr)
        {
            unsafe
            {
                var objectType = GetObjectType(hgdiobj);
                switch (objectType)
                {
                    case Win32.OBJ_PEN:
                    {
                        if (ptr == 0)
                            return (short)Marshal.SizeOf<Win16.LOGPEN>();

                        // Check if buffer big enough
                        if (cbBuffer < Marshal.SizeOf<Win16.LOGPEN>())
                            return 0;

                        // Get it
                        var lp32 = new Win32.LOGPEN();
                        var plp32 = &lp32;
                        var size = GetObject(hgdiobj, Marshal.SizeOf<Win32.LOGPEN>(), (IntPtr)plp32);

                        // Convert and write it back
                        _machine.WriteStruct(ptr, Win32.LOGPEN.To16(lp32));

                        return (short)Marshal.SizeOf<Win16.LOGPEN>();
                    }

                    case Win32.OBJ_BRUSH:
                    {
                        if (ptr == 0)
                            return (short)Marshal.SizeOf<Win16.LOGBRUSH>();

                        if (cbBuffer < Marshal.SizeOf<Win16.LOGBRUSH>())
                            return 0;

                        var lb32 = new Win32.LOGBRUSH();
                        var plb32 = &lb32;
                        var size = GetObject(hgdiobj, Marshal.SizeOf<Win32.LOGBRUSH>(), (IntPtr)plb32);

                        _machine.WriteStruct(ptr, Win32.LOGBRUSH.To16(lb32));
                        return (short)Marshal.SizeOf<Win16.LOGBRUSH>();
                    }

                    case Win32.OBJ_BITMAP:
                    {
                        // Just asking for size?
                        if (ptr == 0)
                            return (short)Marshal.SizeOf<Win16.BITMAP>();

                        // Check if buffer big enough
                        if (cbBuffer < Marshal.SizeOf<Win16.BITMAP>())
                            return 0;

                        // Get it
                        var bmp32 = new Win32.BITMAP();
                        var pbmp32 = &bmp32;
                        var size = GetObject(hgdiobj, Marshal.SizeOf<Win32.BITMAP>(), (IntPtr)pbmp32);

                        // Convert and write it back
                        _machine.WriteStruct(ptr, Win32.BITMAP.To16(bmp32));

                        // Return size
                        return (short)Marshal.SizeOf<Win16.BITMAP>();
                    }

                    case Win32.OBJ_FONT:
                    {
                        // Just asking for size?
                        if (ptr == 0)
                            return (short)Marshal.SizeOf<Win16.LOGFONT>();

                        // Check if buffer big enough
                        if (cbBuffer < Marshal.SizeOf<Win16.LOGFONT>())
                            return 0;

                        // Get it
                        var lf32= new Win32.LOGFONT();
                        var size = GetObject(hgdiobj, Marshal.SizeOf<Win32.LOGFONT>(), out lf32);

                        // Convert and write it back
                        _machine.WriteStruct(ptr, Win32.LOGFONT.To16(lf32));

                        // Return size
                        return (short)Marshal.SizeOf<Win16.LOGFONT>();
                    }

                    default:
                        throw new NotImplementedException("Unsupported object type passed to GetObject");
                }
            }
        }

        [EntryPoint(0x0053)]
        [DllImport("gdi32.dll")]
        public static extern uint GetPixel(HDC hDC, nint x, nint y);

        // 0054 - GETPOLYFILLMODE

        [EntryPoint(0x0055)]
        [DllImport("gdi32.dll")]
        public static extern nint GetROP2(HDC hDC);

        // 0056 - GETRELABS

        [EntryPoint(0x0057)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ GetStockObject(nint Object);

        [EntryPoint(0x0058)]
        [DllImport("gdi32.dll")]
        public static extern nint GetStretchBltMode(HDC hDC);

        // 0059 - GETTEXTCHARACTEREXTRA

        [EntryPoint(0x005A)]
        [DllImport("gdi32.dll")]
        public static extern uint GetTextColor(HDC hDC);

        [DllImport("gdi32.dll")]
        static extern bool GetTextExtentPoint(HDC hdc, string lpString, int cbString, out Win32.SIZE lpSize);

        [EntryPoint(0x005b)]
        public uint GetTextExtent(HDC hDC, uint pszString, short cbString)
        {
            var str = _machine.GlobalHeap.ReadCharacters(pszString, cbString);

            Win32.SIZE size;
            if (!GetTextExtentPoint(hDC, str, cbString, out size))
                return 0xFFFFFFFF;

            return BitUtils.MakeDWord((ushort)(short)size.Width, (ushort)(short)size.Height);
        }                          

        // 005C - GETTEXTFACE

        [EntryPoint(0x005d)]
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetTextMetrics(HDC hdc, out Win32.TEXTMETRIC lptm);


        // 005E - GETVIEWPORTEXT
        // 005F - GETVIEWPORTORG
        // 0060 - GETWINDOWEXT
        // 0061 - GETWINDOWORG
        // 0062 - INTERSECTVISRECT

        [DllImport("gdi32.dll")]
        static extern bool LPtoDP(IntPtr hdc, [In, Out] Win32.POINT[] lpPoints, int nCount);

        [EntryPoint(0x0063)]
        public bool LPtoDP(HDC hDC, uint ppts, nint nCount)
        {
            // Convert to 32
            var pts = new Win32.POINT[nCount];
            for (int i = 0; i < nCount; i++)
            {
                pts[i] = _machine.ReadStruct<Win16.POINT>((uint)(ppts + i * Marshal.SizeOf<Win16.POINT>())).Convert();
            }

            // Calculate
            bool val = LPtoDP(hDC.value, pts, nCount);

            // And back
            for (int i = 0; i < nCount; i++)
            {
                _machine.WriteStruct((uint)(ppts + i * Marshal.SizeOf<Win16.POINT>()), pts[i].Convert());
            }

            return val;
        }


        public delegate void LINEDDAPROC(int x, int y, IntPtr lParam);

        [DllImport("gdi32.dll")]
        public static extern bool LineDDA(int x1, int y1, int x2, int y2, LINEDDAPROC callback, IntPtr lParam);

        [EntryPoint(0x0064)]
        public bool LineDDA(nint x1, nint y1, nint x2, nint y2, uint callback, uint lParam)
        {
            return LineDDA(x1, y1, x2, y2, (x, y, data) =>
            {
                _machine.PushWord((ushort)(short)x);
                _machine.PushWord((ushort)(short)y);
                _machine.PushDWord(lParam);
                _machine.CallVM(callback, "LineDDAProc");
            }, IntPtr.Zero);
        }

        // 0065 - OFFSETRGN
        // 0066 - OFFSETVISRGN

        [EntryPoint(0x0067)]
        [DllImport("gdi32.dll")]
        public static extern bool PtVisible(HDC hDC, nint x, nint y);

        [EntryPoint(0x0068)]
        [DllImport("gdi32.dll")]
        public static extern bool RectVisible(HDC hDC, ref Win32.RECT rc);

        // 0069 - SELECTVISRGN
        // 006A - SETBITMAPBITS
        // 0075 - SETDCORG
        // 0077 - ADDFONTRESOURCE
        // 0079 - DEATH
        // 007A - RESURRECTION
 
        [EntryPoint(0x007b)]
        [DllImport("gdi32.dll")]
        public static extern bool PlayMetaFile(HDC hDC, HENHMETAFILE hMetaFile);

        // 007C - GETMETAFILE
        // 007D - CREATEMETAFILE
        // 007E - CLOSEMETAFILE

        [EntryPoint(0x007f)]
        [DllImport("gdi32.dll")]
        public static extern bool DeleteMetaFile([Destroyed] HENHMETAFILE hMetaFile);

        [EntryPoint(0x0080)]
        public short MulDiv(short a, short b, short c)
        {
            return (short)(a * b / c);
        }

        // 0081 - SAVEVISRGN
        // 0082 - RESTOREVISRGN
        // 0083 - INQUIREVISRGN
        // 0084 - SETENVIRONMENT
        // 0085 - GETENVIRONMENT
        // 0086 - GETRGNBOX
        // 0087 - SCANLR
        // 0088 - REMOVEFONTRESOURCE

        [DllImport("gdi32.dll")]
        public static extern bool SetBrushOrgEx(HDC hDC, int x, int y, out Win32.POINT pptOld);

        [EntryPoint(0x0094)]
        public uint SetBrushOrg(HDC hDC, nint x, nint y)
        {
            Win32.POINT old32;
            if (!SetBrushOrgEx(hDC, x, y, out old32))
            {
                return 0;
            }
            return old32.ToDWord();
        }

        // 0095 - GETBRUSHORG

        [EntryPoint(0x0096)]
        [DllImport("gdi32.dll")]
        public static extern bool UnrealizeObject(HGDIOBJ hObj);

        // 0097 - COPYMETAFILE

        [EntryPoint(0x0099)]
        [DllImport("gdi32.dll")]
        public static extern HDC CreateIC(string lpszDriver, string lpszDevice, string lpszOutput, [MustBeNull] IntPtr lpdvmInit);

        [EntryPoint(0x009a)]
        [DllImport("gdi32.dll")]
        public static extern uint GetNearestColor(HDC hDC, uint color);

        // 009B - QUERYABORT

        [EntryPoint(0x009c)]
        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreateDiscardableBitmap(HDC hDC, nint width, nint height);

        // 009F - GETMETAFILEBITS
 
        [DllImport("gdi32.dll")]
        public static extern IntPtr SetMetaFileBitsEx(uint cbBuffer, IntPtr pBuffer);

        [EntryPoint(0x00a0)]
        public ushort SetMetaFileBits(ushort handle)
        {
            if (handle == 0)
                return 0;

            // Get size of global allocation
            uint size = _machine.GlobalHeap.Size(handle);

            // Get pointer
            var hp = _machine.GlobalHeap.GetHeapPointer(BitUtils.MakeDWord(0, handle), false);

            var hEnhMetaFile = SetMetaFileBitsEx(size, hp);

            return HENHMETAFILE.To16(hEnhMetaFile);
        }

        // 00A1 - PTINREGION
        // 00A2 - GETBITMAPDIMENSION
        // 00A3 - SETBITMAPDIMENSION
        // 00A9 - ISDCDIRTY
        // 00AA - SETDCSTATUS


        [EntryPoint(0x00ac)]
        [DllImport("gdi32.dll")]
        public static extern void SetRectRgn(HGDIOBJ hRgn, nint l, nint t, nint r, nint b);


        // 00AD - GETCLIPRGN
        // 00AF - ENUMMETAFILE
        // 00B0 - PLAYMETAFILERECORD
        // 00B3 - GETDCSTATE
        // 00B4 - SETDCSTATE
        // 00B5 - RECTINREGION
        // 00BE - SETDCHOOK
        // 00BF - GETDCHOOK
        // 00C0 - SETHOOKFLAGS
        // 00C1 - SETBOUNDSRECT
        // 00C2 - GETBOUNDSRECT
        // 00C3 - SELECTBITMAP
        // 00C4 - SETMETAFILEBITSBETTER
        // 00C9 - DMBITBLT
        // 00CA - DMCOLORINFO
        // 00CE - DMENUMDFONTS
        // 00CF - DMENUMOBJ
        // 00D0 - DMOUTPUT
        // 00D1 - DMPIXEL
        // 00D2 - DMREALIZEOBJECT
        // 00D3 - DMSTRBLT
        // 00D4 - DMSCANLR
        // 00D5 - BRUTE
        // 00D6 - DMEXTTEXTOUT
        // 00D7 - DMGETCHARWIDTH
        // 00D8 - DMSTRETCHBLT
        // 00D9 - DMDIBBITS
        // 00DA - DMSTRETCHDIBITS
        // 00DB - DMSETDIBTODEV
        // 00DC - DMTRANSPOSE
        // 00E6 - CREATEPQ
        // 00E7 - MINPQ
        // 00E8 - EXTRACTPQ
        // 00E9 - INSERTPQ
        // 00EA - SIZEPQ
        // 00EB - DELETEPQ
        // 00F0 - OPENJOB
        // 00F1 - WRITESPOOL
        // 00F2 - WRITEDIALOG
        // 00F3 - CLOSEJOB
        // 00F4 - DELETEJOB
        // 00F5 - GETSPOOLJOB
        // 00F6 - STARTSPOOLPAGE
        // 00F7 - ENDSPOOLPAGE
        // 00F8 - QUERYJOB
        // 00FA - COPY
        // 00FD - DELETESPOOLPAGE
        // 00FE - SPOOLFILE
        // 012C - ENGINEENUMERATEFONT
        // 012D - ENGINEDELETEFONT
        // 012E - ENGINEREALIZEFONT
        // 012F - ENGINEGETCHARWIDTH
        // 0130 - ENGINESETFONTCONTEXT
        // 0131 - ENGINEGETGLYPHBMP
        // 0132 - ENGINEMAKEFONTDIR
        // 0133 - GETCHARABCWIDTHS
        // 0134 - GETOUTLINETEXTMETRICS
        // 0135 - GETGLYPHOUTLINE
        // 0136 - CREATESCALABLEFONTRESOURCE
        // 0137 - GETFONTDATA
        // 0138 - CONVERTOUTLINEFONTFILE
        // 0139 - GETRASTERIZERCAPS
        // 013A - ENGINEEXTTEXTOUT
        // 014A - ENUMFONTFAMILIES
        // 014C - GETKERNINGPAIRS

        [EntryPoint(0x0159)]
        [DllImport("gdi32.dll")]
        public static extern nuint GetTextAlign(HDC hDC);

        [EntryPoint(0x015A)]
        [DllImport("gdi32.dll")]
        public static extern nuint SetTextAlign(HDC hDC, nuint align);

        // 015C - CHORD
        // 015D - SETMAPPERFLAGS
        // 015E - GETCHARWIDTH

        [DllImport("gdi32.dll")]
        public static extern bool ExtTextOut(IntPtr hDC, int x, int y, uint fuOptions, IntPtr prc, string str, uint cch, IntPtr lpDX);

        [EntryPoint(0x015f)]
        public bool ExtTextOut(HDC hDC, nint x, nint y, nuint fuOptions, uint prcRect, uint lpstr, nuint cch, uint lpDX)
        {
            // Convert the rectangle
            Win32.RECT rc;
            if (prcRect!=0)
            {
                rc = _machine.ReadStruct<Win16.RECT>(prcRect).Convert();
            }

            // Read the string
            var str = _machine.GlobalHeap.ReadCharacters(lpstr, (int)cch.value);

            // Read deltas
            int[] dx = null;
            if (lpDX!=0)
            {
                dx = new int[cch.value];
                for (int i=0; i < cch; i++)
                {
                    dx[i] = _machine.ReadWord((uint)(lpDX + i * 2));
                }
            }

            // Call
            unsafe
            {
                fixed (int* pdx = dx)
                {
                    ExtTextOut(hDC.value, x, y, fuOptions, prcRect == 0 ? IntPtr.Zero : (IntPtr)(&rc), str, cch, lpDX == 0 ? IntPtr.Zero : (IntPtr)pdx);
                }
            }

            return false;
        }

        // 0160 - GETPHYSICALFONTHANDLE
        // 0161 - GETASPECTRATIOFILTER
        // 0162 - SHRINKGDIHEAP
        // 0163 - FTRAPPING0

        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreatePalette(IntPtr ptr);

        [EntryPoint(0x0168)]
        public HGDIOBJ CreatePalette(uint pLogPalette)
        {
            using (var hp = _machine.GlobalHeap.GetHeapPointer(pLogPalette, false))
            {
                return CreatePalette(hp);
            }
        }

        // 0169 - GDISELECTPALETTE
        // 016A - GDIREALIZEPALETTE
        // 016B - GETPALETTEENTRIES
        // 016C - SETPALETTEENTRIES
        // 016D - REALIZEDEFAULTPALETTE
        // 016E - UPDATECOLORS
        // 016F - ANIMATEPALETTE
        // 0170 - RESIZEPALETTE
        // 0172 - GETNEARESTPALETTEINDEX
        // 0174 - EXTFLOODFILL
        // 0175 - SETSYSTEMPALETTEUSE
        // 0176 - GETSYSTEMPALETTEUSE

        [DllImport("gdi32.dll", EntryPoint = "GetSystemPaletteEntries")]
        public static extern uint _GetSystemPaletteEntries(HDC hDC, uint iStartIndex, uint nEntries, IntPtr ptr);

        [EntryPoint(0x0177)]
        public nuint GetSystemPaletteEntries(HDC hDC, nuint iStartIndex, nuint nEntries, uint lppe)
        {
            using (var hp = _machine.GlobalHeap.GetHeapPointer(lppe, true))
            {
                return _GetSystemPaletteEntries(hDC, iStartIndex, nEntries, hp);
            }
        }

        // 0178 - RESETDC
        // 0179 - STARTDOC
        // 017A - ENDDOC
        // 017B - STARTPAGE
        // 017C - ENDPAGE
        // 017D - SETABORTPROC
        // 017E - ABORTDOC
        // 0190 - FASTWINDOWFRAME
        // 0191 - GDIMOVEBITMAP
        // 0193 - GDIINIT2
        // 0195 - FINALGDIINIT
        // 0197 - CREATEUSERBITMAP
        // 0199 - CREATEUSERDISCARDABLEBITMAP
        // 019A - ISVALIDMETAFILE
        // 019B - GETCURLOGFONT
        // 019C - ISDCCURRENTPALETTE

        [DllImport("gdi32.dll")]
        public static extern int StretchDIBits(IntPtr hdc,
               int xDest, int yDest, int destWidth, int destHeight,
               int xSrc, int ySrc, int srcWidth, int srcHeight,
               IntPtr pBits, IntPtr pBitsInfo, uint iUsage, uint rop);

        [EntryPoint(0x01b7)]
        public nint StretchDIBits(HDC hdc,
                    nint xDest, nint yDest, nint destWidth, nint destHeight,
                    nint xSrc, nint ySrc, nint srcWidth, nint srcHeight,
                    uint pBits, uint pBitsInfo, nuint iUsage, uint rop)
        {
            using (var hpBits = _machine.GlobalHeap.GetHeapPointer(pBits, false))
            using (var hpBitsInfo = _machine.GlobalHeap.GetHeapPointer(pBitsInfo, false))
            {
                return StretchDIBits(hdc.value, xDest, yDest, destWidth, destHeight,
                                    xSrc, ySrc, srcWidth, srcHeight,
                                    hpBits, hpBitsInfo,
                                    iUsage, rop);
            }
        }

        // 01B8 - SETDIBITS
        // 01B9 - GETDIBITS

        [DllImport("gdi32.dll")]
        public static extern HGDIOBJ CreateDIBitmap(HDC hdc, IntPtr lpbmih, uint fdwInit, IntPtr lpbInit, IntPtr lpbmi, uint fuUsage);

        [EntryPoint(0x01BA)]
        public HGDIOBJ CreateDIBitmap(HDC hDC, uint lpbmih, uint dwInit, uint lpbInit, uint lpbmi, ushort fuUsage)
        {
            using (var hpbmih = _machine.GlobalHeap.GetHeapPointer(lpbmih, false))
            using (var hpbInit = _machine.GlobalHeap.GetHeapPointer(lpbInit, false))
            using (var hpbmi = _machine.GlobalHeap.GetHeapPointer(lpbmi, false))
            {
                return CreateDIBitmap(hDC, hpbmih, dwInit, hpbInit, hpbmi, fuUsage);
            }
        }

        [DllImport("gdi32.dll")]
        public static extern int SetDIBitsToDevice(HDC hDC, int XDest, int YDest, uint dwWidth, uint dwHeight,
                                                    int XSrc, int YSrc, uint uStartScan, uint cScanLines,
                                                    IntPtr lpvBits, IntPtr lpbmi, uint fuColorUse);

        [EntryPoint(0x01bb)]
        public nint SetDIBitsToDevice(HDC hDC, nint XDest, nint YDest, nuint dwWidth, nuint dwHeight,
                                                    nint XSrc, nint YSrc, nuint uStartScan, nuint cScanLines,
                                                    uint lpvbits, uint lpbmi, nuint fuColorUse)
        {
            using (var hpvbits = _machine.GlobalHeap.GetHeapPointer(lpvbits, false))
            using (var hpbmi = _machine.GlobalHeap.GetHeapPointer(lpbmi, false))
            {
                return SetDIBitsToDevice(hDC, XDest, YDest, dwWidth, dwHeight,
                                        XSrc, YSrc, uStartScan, cScanLines,
                                        hpvbits, hpbmi, fuColorUse);
            }
        }


        // 01BC - CREATEROUNDRECTRGN
        // 01BD - CREATEDIBPATTERNBRUSH
        // 01C1 - DEVICECOLORMATCH
        // 01C2 - POLYPOLYGON
        // 01C3 - CREATEPOLYPOLYGONRGN
        // 01C4 - GDISEEGDIDO
        // 01CC - GDITASKTERMINATION
        // 01CD - SETOBJECTOWNER
        // 01CE - ISGDIOBJECT
        // 01CF - MAKEOBJECTPRIVATE
        // 01D0 - FIXUPBOGUSPUBLISHERMETAFILE
        // 01D1 - RECTVISIBLE_EHH
        // 01D2 - RECTINREGION_EHH
        // 01D3 - UNICODETOANSI
        // 01D4 - GETBITMAPDIMENSIONEX
        // 01D5 - GETBRUSHORGEX
        // 01D6 - GETCURRENTPOSITIONEX
        // 01D7 - GETTEXTEXTENTPOINT
        // 01D8 - GETVIEWPORTEXTEX
        // 01D9 - GETVIEWPORTORGEX
        // 01DA - GETWINDOWEXTEX
        // 01DB - GETWINDOWORGEX
        // 01DC - OFFSETVIEWPORTORGEX
        // 01DD - OFFSETWINDOWORGEX
        // 01DE - SETBITMAPDIMENSIONEX
        // 01DF - SETVIEWPORTEXTEX
        // 01E0 - SETVIEWPORTORGEX
        // 01E1 - SETWINDOWEXTEX
        // 01E2 - SETWINDOWORGEX
        // 01E3 - MOVETOEX
        // 01E4 - SCALEVIEWPORTEXTEX
        // 01E5 - SCALEWINDOWEXTEX
        // 01E6 - GETASPECTRATIOFILTEREX
    }
}
