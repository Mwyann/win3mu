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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Win3muCore
{
    public partial class Win32 : WinCommon
    {
        public const uint WS_EX_CLIENTEDGE = 0x00000200;

        public const uint WM_WINDOWPOSCHANGING = 0x0046;
        public const uint WM_WINDOWPOSCHANGED = 0x0047;
        public const uint WM_IME_SETCONTEXT = 0x0281;
        public const uint WM_IME_NOTIFY = 0x0282;
        public const uint WM_DWMNCRENDERINGCHANGED = 0x031f;

        public const int GWL_EXSTYLE = -20;
        public const int GWL_P_HINSTANCE = -6;
        public const int GWL_P_HWNDPARENT = -8;
        public const int GWL_ID = -12;
        public const int GWL_STYLE = -16;
        public const int GWL_USERDATA = -21;
        public const int GWL_WNDPROC = -4;

        public const int GCL_STYLE = -26;
        public const int GCL_WNDPROC = -24;
        public const int GCL_CBCLSEXTRA = -20;
        public const int GCL_CBWNDEXTRA = -18;

        [StructLayout(LayoutKind.Sequential)]
        [MappedType]
        public struct POINT
        {
            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
            public int X;
            public int Y;

            public Win16.POINT Convert()
            {
                return new Win16.POINT()
                {
                    X = unchecked((short)X),
                    Y = unchecked((short)Y),
                };
            }

            public override string ToString()
            {
                return string.Format("Win32.POINT({0},{1})", X, Y);
            }

            public static Win16.POINT To16(Win32.POINT pt32)
            {
                return pt32.Convert();
            }

            public static Win32.POINT To32(Win16.POINT pt16)
            {
                return pt16.Convert();
            }

            public uint ToDWord()
            {
                var pt16 = Convert();
                return BitUtils.MakeDWord((ushort)pt16.X, (ushort)pt16.Y);
            }
        }
                                    
        [StructLayout(LayoutKind.Sequential)]
        [MappedType]
        public struct SIZE
        {
            public int Width;
            public int Height;

            public Win16.SIZE Convert()
            {
                return new Win16.SIZE()
                {
                    Width = unchecked((short)Width),
                    Height = unchecked((short)Height),
                };
            }

            public override string ToString()
            {
                return string.Format("Win32.SIZE({0},{1})", Width, Height);
            }

            public static Win16.SIZE To16(Win32.SIZE sz32)
            {
                return sz32.Convert();
            }

            public static Win32.SIZE To32(Win16.SIZE sz16)
            {
                return sz16.Convert();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        [MappedType]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public Win16.RECT Convert()
            {
                return new Win16.RECT()
                {
                    Left = unchecked((short)Left),
                    Right = unchecked((short)Right),
                    Top = unchecked((short)Top),
                    Bottom = unchecked((short)Bottom),
                };
            }

            public override string ToString()
            {
                return string.Format("Win32.RECT({0},{1},{2},{3})", Left, Top, Right, Bottom);
            }

            public static Win16.RECT To16(Win32.RECT rc32)
            {
                return rc32.Convert();
            }

            public static Win32.RECT To32(Win16.RECT rc16)
            {
                return rc16.Convert();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hWnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT p;

            public override string ToString()
            {
                return string.Format("MSG({0:X},{1:X4})", hWnd.ToInt64(), message);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;

            public Win16.MINMAXINFO Convert()
            {
                return new Win16.MINMAXINFO()
                {
                    ptReserved = ptReserved.Convert(),
                    ptMaxSize = ptMaxSize.Convert(),
                    ptMaxPosition = ptMaxPosition.Convert(),
                    ptMinTrackSize = ptMinTrackSize.Convert(),
                    ptMaxTrackSize = ptMaxTrackSize.Convert(),
                };
            }
        };

        public delegate void TIMERPROC(IntPtr hWnd, uint uMsg, IntPtr nIDEvent, uint dwTime);
        public delegate IntPtr WNDPROC(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);
        public delegate IntPtr HOOKPROC(int code, IntPtr wParam, IntPtr lParam);
        public delegate bool ENUMTHREADWNDPROC(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct WNDCLASS
        {
            public uint style;
            //[MarshalAs(UnmanagedType.FunctionPtr)]
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public HGDIOBJ hIcon;
            public HGDIOBJ hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct get_WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public HGDIOBJ hIcon;
            public HGDIOBJ hCursor;
            public IntPtr hbrBackground;
            public IntPtr lpszMenuName;
            public IntPtr lpszClassName;
        }



        [StructLayout(LayoutKind.Sequential)]
        public struct CREATESTRUCT
        {
            public IntPtr lpCreateParams;
            public IntPtr hInstance;
            public IntPtr hMenu;
            public IntPtr hWndParent;
            public int cy;
            public int cx;
            public int y;
            public int x;
            public uint style;
            public IntPtr lpszName;
            public IntPtr lpszClassName;
            public uint dwExStyle;
        }

        public const int CW_USEDEFAULT = unchecked((int)0x80000000);


        [StructLayout(LayoutKind.Sequential)]
        [MappedType]
        public struct PAINTSTRUCT
        {
            public HDC hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rgbReserved;

            public static Win16.PAINTSTRUCT To16(Win32.PAINTSTRUCT ps32)
            {
                return new Win16.PAINTSTRUCT()
                {
                    hdc = HDC.To16(ps32.hdc),
                    fErase = (ushort)(ps32.fErase ? 1 : 0),
                    rcPaint = ps32.rcPaint.Convert(),
                    fRestore = (ushort)(ps32.fRestore ? 1 : 0),
                    fIncUpdate = (ushort)(ps32.fIncUpdate ? 1 : 0),
                };
            }

            public static Win32.PAINTSTRUCT To32(Win16.PAINTSTRUCT ps16)
            {
                return new Win32.PAINTSTRUCT()
                {
                    hdc = HDC.To32(ps16.hdc),
                    fErase = ps16.fErase != 0,
                    rcPaint = ps16.rcPaint.Convert(),
                    fRestore = ps16.fRestore != 0,
                    fIncUpdate = ps16.fIncUpdate != 0,
                };
            }
        }

        public struct ACCEL
        {
            public byte fVirt;
            public ushort key;
            public ushort cmd;

            public static ACCEL To32(Win16.ACCEL a16)
            {
                return new ACCEL()
                {
                    fVirt = (byte)(a16.fFlags & FMASK),
                    key = a16.wAscii,
                    cmd = a16.wId,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        [MappedType]
        public struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;

            public Win16.TEXTMETRIC Convert()
            {
                return new Win16.TEXTMETRIC()
                {
                    tmHeight = (short)tmHeight,
                    tmAscent = (short)tmAscent,
                    tmDescent = (short)tmDescent,
                    tmInternalLeading = (short)tmInternalLeading,
                    tmExternalLeading = (short)tmExternalLeading,
                    tmAveCharWidth = (short)tmAveCharWidth,
                    tmMaxCharWidth = (short)tmMaxCharWidth,
                    tmWeight = (short)tmWeight,
                    tmOverhang = (short)tmOverhang,
                    tmDigitizedAspectX = (short)tmDigitizedAspectX,
                    tmDigitizedAspectY = (short)tmDigitizedAspectY,
                    tmFirstChar = (sbyte)tmFirstChar,
                    tmLastChar = (sbyte)tmLastChar,
                    tmDefaultChar = (sbyte)tmDefaultChar,
                    tmBreakChar = (sbyte)tmBreakChar,
                    tmItalic = tmItalic,
                    tmUnderlined = tmUnderlined,
                    tmStruckOut = tmStruckOut,
                    tmPitchAndFamily = tmPitchAndFamily,
                    tmCharSet = tmCharSet,
                };
            }

            public static Win16.TEXTMETRIC To16(Win32.TEXTMETRIC tm)
            {
                return tm.Convert();
            }

            /*
            public static Win32.TEXTMETRIC To32(Win16.TEXTMETRIC tm)
            {
                return tm.Convert();
            }
            */
        }


        public const uint OBJ_PEN = 1;
        public const uint OBJ_BRUSH = 2;
        public const uint OBJ_DC = 3;
        public const uint OBJ_METADC = 4;
        public const uint OBJ_PAL = 5;
        public const uint OBJ_FONT = 6;
        public const uint OBJ_BITMAP = 7;
        public const uint OBJ_REGION = 8;
        public const uint OBJ_METAFILE = 9;
        public const uint OBJ_MEMDC = 10;
        public const uint OBJ_EXTPEN = 11;
        public const uint OBJ_ENHMETADC = 12;
        public const uint OBJ_ENHMETAFILE = 13;
        public const uint OBJ_COLORSPACE = 14;

        [MappedType]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;

            public static Win16.BITMAP To16(Win32.BITMAP p32)
            {
                return new Win16.BITMAP()
                {
                    bmType = (short)p32.bmType,
                    bmWidth = (short)p32.bmWidth,
                    bmHeight = (short)p32.bmHeight,
                    bmWidthBytes = (short)p32.bmWidthBytes,
                    bmPlanes = (byte)p32.bmPlanes,
                    bmBitsPixel = (byte)p32.bmBitsPixel,
                    bmBits = 0,
                };
            }

            public static Win32.BITMAP To32(Win16.BITMAP p16)
            {
                return new Win32.BITMAP()
                {
                    bmType = p16.bmType,
                    bmWidth = p16.bmWidth,
                    bmHeight = p16.bmHeight,
                    bmWidthBytes = p16.bmWidthBytes,
                    bmPlanes = p16.bmPlanes,
                    bmBitsPixel = p16.bmBitsPixel,
                    bmBits = IntPtr.Zero,
                };
            }
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DLGHEADER
        {
            public uint dwStyle; 
            public uint dwStyleEx;
            public ushort cItems;
            public ushort x;
            public ushort y;
            public ushort cx;
            public ushort cy;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DLGITEMHEADER
        {
            public uint dwStyle;
            public uint dwStyleEx;
            public ushort x;
            public ushort y;
            public ushort cx;
            public ushort cy;
            public ushort wID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DRAWITEMSTRUCT
        {
            public uint CtlType;
            public uint CtlID;
            public uint itemID;
            public uint itemAction;
            public uint itemState;
            public IntPtr hwndItem;
            public IntPtr hDC;
            public RECT rcItem;
            public IntPtr itemData;

            public Win16.DRAWITEMSTRUCT Convert()
            {
                return new Win16.DRAWITEMSTRUCT()
                {
                    CtlType = (ushort)CtlType,
                    CtlID = (ushort)CtlID,
                    itemID = (ushort)itemID,
                    itemAction = (ushort)itemAction,
                    itemState = (ushort)itemState,
                    hwndItem = HWND.Map.To16(hwndItem),
                    hDC = HDC.Map.To16(hDC),
                    rcItem = rcItem.Convert(),
                    itemData = (uint)itemData.ToInt32(),
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEASUREITEMSTRUCT
        {
            public uint CtlType;
            public uint CtlID;
            public uint itemID;
            public uint itemWidth;
            public uint itemHeight;
            public IntPtr itemData;

            public Win16.MEASUREITEMSTRUCT Convert()
            {
                return new Win16.MEASUREITEMSTRUCT()
                {
                    CtlType = (ushort)CtlType,
                    CtlID = (ushort)CtlID,
                    itemID = (ushort)itemID,
                    itemWidth = (ushort)itemWidth,
                    itemHeight = (ushort)itemHeight,
                    itemData = (uint)itemData.ToInt32(),
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DELETEITEMSTRUCT
        {
            public uint CtlType;
            public uint CtlID;
            public uint itemID;
            public IntPtr hWnd;
            public IntPtr itemData;

            public Win16.DELETEITEMSTRUCT Convert()
            {
                return new Win16.DELETEITEMSTRUCT()
                {
                    CtlType = (ushort)CtlType,
                    CtlID = (ushort)CtlID,
                    itemID = (ushort)itemID,
                    hWnd = HWND.Map.To16(hWnd),
                    itemData = (uint)itemData.ToInt32(),
                };
            }
        }

        [MappedType]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LOGFONT
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string lfFaceName;

            public static Win32.LOGFONT To32(Win16.LOGFONT lf16)
            {
                return new Win32.LOGFONT()
                {
                    lfHeight = lf16.lfHeight,
                    lfWidth = lf16.lfWidth,
                    lfEscapement = lf16.lfEscapement,
                    lfOrientation = lf16.lfOrientation,
                    lfWeight = lf16.lfWeight,
                    lfItalic = lf16.lfItalic,
                    lfUnderline = lf16.lfUnderline,
                    lfStrikeOut = lf16.lfStrikeOut,
                    lfCharSet = lf16.lfCharSet,
                    lfOutPrecision = lf16.lfOutPrecision,
                    lfClipPrecision = lf16.lfClipPrecision,
                    lfQuality = lf16.lfQuality,
                    lfPitchAndFamily = lf16.lfPitchAndFamily,
                    lfFaceName = lf16.lfFaceName,
                };
            }
            public static Win16.LOGFONT To16(Win32.LOGFONT lf32)
            {
                return new Win16.LOGFONT()
                {
                    lfHeight = (short)lf32.lfHeight,
                    lfWidth = (short)lf32.lfHeight,
                    lfEscapement = (short)lf32.lfEscapement,
                    lfOrientation = (short)lf32.lfOrientation,
                    lfWeight = (short)lf32.lfWeight,
                    lfItalic = lf32.lfItalic,
                    lfUnderline = lf32.lfUnderline,
                    lfStrikeOut = lf32.lfStrikeOut,
                    lfCharSet = lf32.lfCharSet,
                    lfOutPrecision = lf32.lfOutPrecision,
                    lfClipPrecision = lf32.lfClipPrecision,
                    lfQuality = lf32.lfQuality,
                    lfPitchAndFamily = lf32.lfPitchAndFamily,
                    lfFaceName = lf32.lfFaceName,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LOGBRUSH
        {
            public uint style;
            public uint color;
            public IntPtr hatch;

            public static Win16.LOGBRUSH To16(Win32.LOGBRUSH lb32)
            {
                if (lb32.style == Win32.BS_DIBPATTERN)
                {
                    throw new NotImplementedException("Unsupport log brush conversion BS_DIBPATTERN");
                }
                return new Win16.LOGBRUSH()
                {
                    style = (ushort)lb32.style,
                    color = lb32.color,
                    hatch = (short)lb32.hatch,
                };
            }
        }

        [MappedType]
        [StructLayout(LayoutKind.Sequential)]
        public struct LOGPEN
        {
            public uint style;
            public Win32.POINT width;
            public uint colorRef;

            public static Win16.LOGPEN To16(Win32.LOGPEN lp32)
            {
                return new Win16.LOGPEN()
                {
                    style = (ushort)lp32.style,
                    width = lp32.width.Convert(),
                    colorRef = lp32.colorRef,
                };
            }

            public static Win32.LOGPEN To32(Win16.LOGPEN lp16)
            {
                return new Win32.LOGPEN()
                {
                    style = lp16.style,
                    width = lp16.width.Convert(),
                    colorRef = lp16.colorRef,
                };
            }
        }


        public const ushort BM_GETCHECK = 0x00F0;
        public const ushort BM_SETCHECK = 0x00F1;
        public const ushort BM_GETSTATE = 0x00F2;
        public const ushort BM_SETSTATE = 0x00F3;
        public const ushort BM_SETSTYLE = 0x00F4;
        public const ushort BM_CLICK = 0x00F5;
        public const ushort BM_GETIMAGE = 0x00F6;
        public const ushort BM_SETIMAGE = 0x00F7;

        public const ushort STM_SETICON = 0x0170;
        public const ushort STM_GETICON = 0x0171;
        public const ushort STM_SETIMAGE = 0x0172;
        public const ushort STM_GETIMAGE = 0x0173;

        public const ushort LB_ADDSTRING = 0x0180;
        public const ushort LB_INSERTSTRING = 0x0181;
        public const ushort LB_DELETESTRING = 0x0182;
        public const ushort LB_SELITEMRANGEEX = 0x0183;
        public const ushort LB_RESETCONTENT = 0x0184;
        public const ushort LB_SETSEL = 0x0185;
        public const ushort LB_SETCURSEL = 0x0186;
        public const ushort LB_GETSEL = 0x0187;
        public const ushort LB_GETCURSEL = 0x0188;
        public const ushort LB_GETTEXT = 0x0189;
        public const ushort LB_GETTEXTLEN = 0x018A;
        public const ushort LB_GETCOUNT = 0x018B;
        public const ushort LB_SELECTSTRING = 0x018C;
        public const ushort LB_DIR = 0x018D;
        public const ushort LB_GETTOPINDEX = 0x018E;
        public const ushort LB_FINDSTRING = 0x018F;
        public const ushort LB_GETSELCOUNT = 0x0190;
        public const ushort LB_GETSELITEMS = 0x0191;
        public const ushort LB_SETTABSTOPS = 0x0192;
        public const ushort LB_GETHORIZONTALEXTENT = 0x0193;
        public const ushort LB_SETHORIZONTALEXTENT = 0x0194;
        public const ushort LB_SETCOLUMNWIDTH = 0x0195;
        public const ushort LB_ADDFILE = 0x0196;
        public const ushort LB_SETTOPINDEX = 0x0197;
        public const ushort LB_GETITEMRECT = 0x0198;
        public const ushort LB_GETITEMDATA = 0x0199;
        public const ushort LB_SETITEMDATA = 0x019A;
        public const ushort LB_SELITEMRANGE = 0x019B;
        public const ushort LB_SETANCHORINDEX = 0x019C;
        public const ushort LB_GETANCHORINDEX = 0x019D;
        public const ushort LB_SETCARETINDEX = 0x019E;
        public const ushort LB_GETCARETINDEX = 0x019F;
        public const ushort LB_SETITEMHEIGHT = 0x01A0;
        public const ushort LB_GETITEMHEIGHT = 0x01A1;
        public const ushort LB_FINDSTRINGEXACT = 0x01A2;
        public const ushort LB_SETLOCALE = 0x01A5;
        public const ushort LB_GETLOCALE = 0x01A6;
        public const ushort LB_SETCOUNT = 0x01A7;
        public const ushort LB_INITSTORAGE = 0x01A8;
        public const ushort LB_ITEMFROMPOINT = 0x01A9;
        public const ushort LB_MULTIPLEADDSTRING = 0x01B1;
        public const ushort LB_GETLISTBOXINFO = 0x01B2;
        public const ushort CB_GETEDITSEL = 0x0140;
        public const ushort CB_LIMITTEXT = 0x0141;
        public const ushort CB_SETEDITSEL = 0x0142;
        public const ushort CB_ADDSTRING = 0x0143;
        public const ushort CB_DELETESTRING = 0x0144;
        public const ushort CB_DIR = 0x0145;
        public const ushort CB_GETCOUNT = 0x0146;
        public const ushort CB_GETCURSEL = 0x0147;
        public const ushort CB_GETLBTEXT = 0x0148;
        public const ushort CB_GETLBTEXTLEN = 0x0149;
        public const ushort CB_INSERTSTRING = 0x014A;
        public const ushort CB_RESETCONTENT = 0x014B;
        public const ushort CB_FINDSTRING = 0x014C;
        public const ushort CB_SELECTSTRING = 0x014D;
        public const ushort CB_SETCURSEL = 0x014E;
        public const ushort CB_SHOWDROPDOWN = 0x014F;
        public const ushort CB_GETITEMDATA = 0x0150;
        public const ushort CB_SETITEMDATA = 0x0151;
        public const ushort CB_GETDROPPEDCONTROLRECT = 0x0152;
        public const ushort CB_SETITEMHEIGHT = 0x0153;
        public const ushort CB_GETITEMHEIGHT = 0x0154;
        public const ushort CB_SETEXTENDEDUI = 0x0155;
        public const ushort CB_GETEXTENDEDUI = 0x0156;
        public const ushort CB_GETDROPPEDSTATE = 0x0157;
        public const ushort CB_FINDSTRINGEXACT = 0x0158;
        public const ushort CB_SETLOCALE = 0x0159;
        public const ushort CB_GETLOCALE = 0x015A;
        public const ushort CB_GETTOPINDEX = 0x015b;
        public const ushort CB_SETTOPINDEX = 0x015c;
        public const ushort CB_GETHORIZONTALEXTENT = 0x015d;
        public const ushort CB_SETHORIZONTALEXTENT = 0x015e;
        public const ushort CB_GETDROPPEDWIDTH = 0x015f;
        public const ushort CB_SETDROPPEDWIDTH = 0x0160;
        public const ushort CB_INITSTORAGE = 0x0161;
        public const ushort CB_MULTIPLEADDSTRING = 0x0163;
        public const ushort CB_GETCOMBOBOXINFO = 0x0164;
        public const ushort EM_GETSEL = 0x00B0;
        public const ushort EM_SETSEL = 0x00B1;
        public const ushort EM_GETRECT = 0x00B2;
        public const ushort EM_SETRECT = 0x00B3;
        public const ushort EM_SETRECTNP = 0x00B4;
        public const ushort EM_SCROLL = 0x00B5;
        public const ushort EM_LINESCROLL = 0x00B6;
        public const ushort EM_SCROLLCARET = 0x00B7;
        public const ushort EM_GETMODIFY = 0x00B8;
        public const ushort EM_SETMODIFY = 0x00B9;
        public const ushort EM_GETLINECOUNT = 0x00BA;
        public const ushort EM_LINEINDEX = 0x00BB;
        public const ushort EM_SETHANDLE = 0x00BC;
        public const ushort EM_GETHANDLE = 0x00BD;
        public const ushort EM_GETTHUMB = 0x00BE;
        public const ushort EM_LINELENGTH = 0x00C1;
        public const ushort EM_REPLACESEL = 0x00C2;
        public const ushort EM_GETLINE = 0x00C4;
        public const ushort EM_SETLIMITTEXT = 0x00C5;
        public const ushort EM_CANUNDO = 0x00C6;
        public const ushort EM_UNDO = 0x00C7;
        public const ushort EM_FMTLINES = 0x00C8;
        public const ushort EM_LINEFROMCHAR = 0x00C9;
        public const ushort EM_SETTABSTOPS = 0x00CB;
        public const ushort EM_SETPASSWORDCHAR = 0x00CC;
        public const ushort EM_EMPTYUNDOBUFFER = 0x00CD;
        public const ushort EM_GETFIRSTVISIBLELINE = 0x00CE;
        public const ushort EM_SETREADONLY = 0x00CF;
        public const ushort EM_SETWORDBREAKPROC = 0x00D0;
        public const ushort EM_GETWORDBREAKPROC = 0x00D1;
        public const ushort EM_GETPASSWORDCHAR = 0x00D2;
        public const ushort EM_SETMARGINS = 0x00D3;
        public const ushort EM_GETMARGINS = 0x00D4;
        public const ushort EM_GETLIMITTEXT = 0x00D5;
        public const ushort EM_POSFROMCHAR = 0x00D6;
        public const ushort EM_CHARFROMPOS = 0x00D7;
        public const ushort EM_SETIMESTATUS = 0x00D8;
        public const ushort EM_GETIMESTATUS = 0x00D9;


        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MCI_OPEN_PARAMS
        {
            public IntPtr dwCallback;
            public uint nDeviceID;
            public IntPtr lpstrDeviceName;
            public IntPtr lpstrElementName;
            public IntPtr lpstrAlias;
            public uint nPacking;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MCI_STATUS_PARAMS
        {
            public IntPtr dwCallback;
            public IntPtr dwReturn;
            public uint dwItem;
            public uint dwTrack;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MCI_PLAY_PARAMS
        {
            public IntPtr dwCallback;
            public uint dwFrom;
            public uint dwTo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MCI_GENERIC_PARAMS
        {
            public IntPtr dwCallback;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MDINEXTMENU
        {
            public IntPtr hmenuIn;
            public IntPtr hmenuNext;
            public IntPtr  hwndNext;

            public Win16.MDINEXTMENU Convert()
            {
                return new Win16.MDINEXTMENU()
                {
                    hmenuIn = HMENU.To16(hmenuIn),
                    hmenuNext = HMENU.To16(hmenuNext),
                    hwndNext = HWND.To16(hwndNext),
                };
            }
        }
    }
}
