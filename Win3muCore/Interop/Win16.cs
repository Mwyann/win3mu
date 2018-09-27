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
using System.IO;
using System.Runtime.InteropServices;

namespace Win3muCore
{
    public class Win16 : WinCommon
    {
        // WinFlags
        public const ushort WF_PMODE = 0x0001;
        public const ushort WF_CPU286 = 0x0002;
        public const ushort WF_CPU386 = 0x0004;
        public const ushort WF_CPU486 = 0x0008;
        public const ushort WF_STANDARD = 0x0010;
        public const ushort WF_WIN286 = 0x0010;
        public const ushort WF_ENHANCED = 0x0020;
        public const ushort WF_WIN386 = 0x0020;
        public const ushort WF_CPU086 = 0x0040;
        public const ushort WF_CPU186 = 0x0080;
        public const ushort WF_LARGEFRAME = 0x0100;
        public const ushort WF_SMALLFRAME = 0x0200;
        public const ushort WF_80x87 = 0x0400;
        public const ushort WF_PAGING = 0x0800;
        public const ushort WF_WLO = 0x8000;

        // Global Memory Flags
        public const ushort GMEM_FIXED = 0x0000;
        public const ushort GMEM_MOVEABLE = 0x0002;
        public const ushort GMEM_NOCOMPACT = 0x0010;
        public const ushort GMEM_NODISCARD = 0x0020;
        public const ushort GMEM_ZEROINIT = 0x0040;
        public const ushort GMEM_MODIFY = 0x0080;
        public const ushort GMEM_DISCARDABLE = 0x0100;
        public const ushort GMEM_NOT_BANKED = 0x1000;
        public const ushort GMEM_SHARE = 0x2000;
        public const ushort GMEM_DDESHARE = 0x2000;
        public const ushort GMEM_NOTIFY = 0x4000;
        public const ushort GMEM_LOWER = GMEM_NOT_BANKED;

        // Local Memory Flags
        public const ushort LMEM_FIXED = 0x0000;
        public const ushort LMEM_MOVEABLE = 0x0002;
        public const ushort LMEM_NOCOMPACT = 0x0010;
        public const ushort LMEM_NODISCARD = 0x0020;
        public const ushort LMEM_ZEROINIT = 0x0040;
        public const ushort LMEM_MODIFY = 0x0080;
        public const ushort LMEM_DISCARDABLE = 0x0F00;

        // ShowWindow flags
        public const short SW_HIDE = 0;
        public const short SW_SHOWNORMAL = 1;
        public const short SW_NORMAL = 1;
        public const short SW_SHOWMINIMIZED = 2;
        public const short SW_SHOWMAXIMIZED = 3;
        public const short SW_MAXIMIZE = 3;
        public const short SW_SHOWNOACTIVATE = 4;
        public const short SW_SHOW = 5;
        public const short SW_MINIMIZE = 6;
        public const short SW_SHOWMINNOACTIVE = 7;
        public const short SW_SHOWNA = 8;
        public const short SW_RESTORE = 9;

        // GetWindow(Word|Long)
        public const short GWL_WNDPROC = -4;
        public const short GWW_HINSTANCE = -6;
        public const short GWW_HWNDPARENT = -8;
        public const short GWW_ID = -12;
        public const short GWL_STYLE = -16;
        public const short GWL_EXSTYLE = -20;


        // GetClass(Word|Long)
        public const short GCL_MENUNAME = -8;
        public const short GCW_HBRBACKGROUND = -10;
        public const short GCW_HCURSOR = -12;
        public const short GCW_HICON = -14;
        public const short GCW_HMODULE = -16;
        public const short GCW_CBWNDEXTRA = -18;
        public const short GCW_CBCLSEXTRA = -20;
        public const short GCL_WNDPROC = -24;
        public const short GCW_STYLE = -26;

        public enum ResourceType : uint
        {
            RT_CURSOR = 1,
            RT_BITMAP = 2,
            RT_ICON = 3,
            RT_MENU = 4,
            RT_DIALOG = 5,
            RT_STRING = 6,
            RT_FONTDIR = 7,
            RT_FONT = 8,
            RT_ACCELERATOR = 9,
            RT_RCDATA = 10,
            RT_MESSAGETABLE = 11,
            RT_GROUP_CURSOR = 12,
            RT_GROUP_ICON = 14,
            RT_NAMETABLE = 15,          // REF: http://www.rdos.net/svn/tags/V9.2.5/watcom/bld/sdk/rc/wres/h/resfmt.h
            RT_VERSION = 16,
            RT_DLGINCLUDE = 17,
            RT_PLUGPLAY = 19,
            RT_VXD = 20,
            RT_ANICURSOR = 21,
            RT_ANIICON = 22,
            RT_HTML = 23
        }

        [Flags]
        public enum ResourceMemoryType : ushort
        {
            None = 0,
            Moveable = 0x10,
            Pure = 0x20,
            PreLoad = 0x40,
            Unknown = 7168
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public short X;
            public short Y;

            public Win32.POINT Convert()
            {
                return new Win32.POINT()
                {
                    X = X,
                    Y = Y,
                };
            }

            public override string ToString()
            {
                return string.Format("Win16.POINT({0},{1})", X, Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public short Width;
            public short Height;

            public Win32.SIZE Convert()
            {
                return new Win32.SIZE()
                {
                    Width = Width,
                    Height = Height,
                };
            }

            public override string ToString()
            {
                return string.Format("Win16.SIZE({0},{1})", Width, Height);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;

            public Win32.RECT Convert()
            {
                return new Win32.RECT()
                {
                    Left = Left,
                    Right = Right,
                    Top = Top,
                    Bottom = Bottom,
                };
            }

            public override string ToString()
            {
                return string.Format("Win16.RECT({0},{1},{2},{3})", Left, Top, Right, Bottom);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct MSG
        {
            public ushort hWnd;
            public ushort message;
            public ushort wParam;
            public uint lParam;
            public uint time;
            public POINT pt;

            public override string ToString()
            {
                return string.Format("MSG({0:X4},{1:X4})", hWnd, message);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct WNDCLASS
        {
            public ushort style;
            public uint lpfnWndProc;
            public short cbClsExtra;
            public short cbWndExtra;
            public ushort hInstance;
            public ushort hIcon;
            public ushort hCursor;
            public ushort hbrBackground;
            public uint lpszMenuName;
            public uint lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct CREATESTRUCT
        {
            public uint lpCreateParams;
            public ushort hInstance;
            public ushort hMenu;
            public ushort hWndParent;
            public short cy;
            public short cx;
            public short y;
            public short x;
            public uint style;
            public uint lpszName;
            public uint lpszClassName;
            public uint dwExStyle;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct PAINTSTRUCT
        {
            public ushort hdc;
            public ushort fErase;
            public RECT rcPaint;
            public ushort fRestore;
            public ushort fIncUpdate;
            public uint resvd1;
            public uint resvd2;
            public uint resvd3;
            public uint resvd4;

            /*
            public Win32.PAINTSTRUCT Convert(Machine _machine)
            {
                return new Win32.PAINTSTRUCT()
                {
                    hdc = _machine.DCHandles.To32(hdc),
                    fErase = fErase != 0,
                    rcPaint = rcPaint.Convert(),
                    fRestore = fRestore != 0,
                    fIncUpdate = fIncUpdate != 0,
                };
            }
            */
        }

        public const uint DS_SETFONT = 0x40;
        public const short CW_USEDEFAULT = unchecked((short)0x8000);

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;

            public Win32.MINMAXINFO Convert()
            {
                return new Win32.MINMAXINFO()
                {
                    ptReserved = ptReserved.Convert(),
                    ptMaxSize = ptMaxSize.Convert(),
                    ptMaxPosition = ptMaxPosition.Convert(),
                    ptMinTrackSize = ptMinTrackSize.Convert(),
                    ptMaxTrackSize = ptMaxTrackSize.Convert(),
                };
            }
        };

        public static uint MakePoint(short x, short y)
        {
            return (uint)(((ushort)x) << 16 | (ushort)y);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TEXTMETRIC
        {
            public short tmHeight;
            public short tmAscent;
            public short tmDescent;
            public short tmInternalLeading;
            public short tmExternalLeading;
            public short tmAveCharWidth;
            public short tmMaxCharWidth;
            public short tmWeight;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public sbyte tmFirstChar;
            public sbyte tmLastChar;
            public sbyte tmDefaultChar;
            public sbyte tmBreakChar;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
            public short tmOverhang;
            public short tmDigitizedAspectX;
            public short tmDigitizedAspectY;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BITMAP
        {
            public short bmType;
            public short bmWidth;
            public short bmHeight;
            public short bmWidthBytes;
            public byte bmPlanes;
            public byte bmBitsPixel;
            public uint bmBits;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GRPICONDIR
        {
            public ushort idReserved;
            public ushort idType;
            public ushort idCount;
            //GRPICONDIRENTRY idEntries[];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GRPICONDIRENTRY
        {
            public byte bWidth;
            public byte bHeight;
            public byte bColorCount;
            public byte bReserved;
            public ushort wPlanes;
            public ushort wBitCount;
            public uint dwBytesInRes;
            public ushort nId;

            public bool IsPreferredSize()
            {
                return bWidth == 32;
            }
        }

        public class GroupIcon
        {
            public GRPICONDIR Directory;
            public List<GRPICONDIRENTRY> Entries = new List<GRPICONDIRENTRY>();

            public GRPICONDIRENTRY? PickBestEntry()
            {
                if (Entries.Count == 0)
                    return null;

                int best = -1;
                for (int i=0; i<Entries.Count; i++)
                {
                    if (best == -1)
                    {
                        best = 0;
                    }
                    else
                    {
                        if (Entries[i].IsPreferredSize())
                        {
                            if (!Entries[best].IsPreferredSize() || Entries[i].wBitCount > Entries[best].wBitCount)
                                best = i;
                        }
                        else
                        {
                            if (!Entries[best].IsPreferredSize() || Entries[i].wBitCount > Entries[best].wBitCount)
                                best = i;
                        }
                    }
                }

                return Entries[best];
            }
        }



        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACCEL
        {
            public byte fFlags;
            public ushort wAscii;
            public ushort wId;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DLGHEADER
        {
            public uint dwStyle; // dialog style
            public byte cItems;  // number of controls in this dialog
            public ushort x;       // x-coordinate
            public ushort y;       // y-coordinate
            public ushort cx;      // width
            public ushort cy;      // height

            public Win32.DLGHEADER Convert()
            {
                return new Win32.DLGHEADER()
                {
                    cItems = cItems,
                    dwStyle = dwStyle,
                    dwStyleEx = 0,
                    x = x,
                    y = y,
                    cx = cx,
                    cy = cy,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DLGITEMHEADER
        {
            public ushort x;
            public ushort y;
            public ushort cx;
            public ushort cy;
            public ushort wID;
            public uint dwStyle;

            public Win32.DLGITEMHEADER Convert()
            {
                return new Win32.DLGITEMHEADER()
                {
                    dwStyle = dwStyle,
                    x = x,
                    y = y,
                    cx = cx,
                    cy = cy,
                    wID = wID,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DRAWITEMSTRUCT
        {
            public ushort CtlType;
            public ushort CtlID;
            public ushort itemID;
            public ushort itemAction;
            public ushort itemState;
            public ushort hwndItem;
            public ushort hDC;
            public RECT rcItem;
            public uint itemData;

            public Win32.DRAWITEMSTRUCT Convert()
            {
                return new Win32.DRAWITEMSTRUCT()
                {
                    CtlType = CtlType,
                    CtlID = CtlID,
                    itemID = itemID,
                    itemAction = itemAction,
                    itemState = itemState,
                    hwndItem = HWND.Map.To32(hwndItem),
                    hDC = HDC.Map.To32(hDC),
                    rcItem = rcItem.Convert(),
                    itemData = (IntPtr)itemData,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MEASUREITEMSTRUCT
        {
            public ushort CtlType;
            public ushort CtlID;
            public ushort itemID;
            public ushort itemWidth;
            public ushort itemHeight;
            public uint itemData;

            public Win32.MEASUREITEMSTRUCT Convert()
            {
                return new Win32.MEASUREITEMSTRUCT()
                {
                    CtlType = CtlType,
                    CtlID = CtlID,
                    itemID = itemID,
                    itemWidth = itemWidth,
                    itemHeight = itemHeight,
                    itemData = (IntPtr)itemData,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DELETEITEMSTRUCT
        {
            public ushort CtlType;
            public ushort CtlID;
            public ushort itemID;
            public ushort hWnd;
            public uint itemData;

            public Win32.DELETEITEMSTRUCT Convert()
            {
                return new Win32.DELETEITEMSTRUCT()
                {
                    CtlType = CtlType,
                    CtlID = CtlID,
                    itemID = itemID,
                    hWnd = HWND.Map.To32(hWnd),
                    itemData = (IntPtr)itemData,
                };
            }
        }

        public delegate uint WNDPROC(ushort hWnd, ushort message, ushort wParam, uint lParam);

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct OFSTRUCT
        {
            public byte cBytes;
            public byte fFixedDisk;
            public ushort nErrCode;
            public uint reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPathName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LOGBRUSH
        {
            public ushort style;
            public uint color;
            public short hatch;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LOGPEN
        {
            public ushort style;
            public Win16.POINT width;
            public uint colorRef;
        }

        public const ushort OF_READ = 0x0000;
        public const ushort OF_WRITE = 0x0001;
        public const ushort OF_READWRITE = 0x0002;
        public const ushort OF_SHARE_COMPAT = 0x0000;
        public const ushort OF_SHARE_EXCLUSIVE = 0x0010;
        public const ushort OF_SHARE_DENY_WRITE = 0x0020;
        public const ushort OF_SHARE_DENY_READ = 0x0030;
        public const ushort OF_SHARE_DENY_NONE = 0x0040;
        public const ushort OF_PARSE = 0x0100;
        public const ushort OF_DELETE = 0x0200;
        public const ushort OF_VERIFY = 0x0400;
        public const ushort OF_SEARCH = 0x0400;
        public const ushort OF_CANCEL = 0x0800;
        public const ushort OF_CREATE = 0x1000;
        public const ushort OF_PROMPT = 0x2000;
        public const ushort OF_EXIST = 0x4000;
        public const ushort OF_REOPEN = 0x8000;

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct LOGFONT
        {
            public short lfHeight;
            public short lfWidth;
            public short lfEscapement;
            public short lfOrientation;
            public short lfWeight;
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
        }


        public const ushort BM_GETCHECK = WM_USER + 0;
        public const ushort BM_SETCHECK = WM_USER + 1;
        public const ushort BM_GETSTATE = WM_USER + 2;
        public const ushort BM_SETSTATE = WM_USER + 3;
        public const ushort BM_SETSTYLE = WM_USER + 4;
        public const ushort STM_SETICON = WM_USER + 0;
        public const ushort STM_GETICON = WM_USER + 1;
        public const ushort EM_GETSEL = WM_USER + 0;
        public const ushort EM_SETSEL = WM_USER + 1;
        public const ushort EM_GETRECT = WM_USER + 2;
        public const ushort EM_SETRECT = WM_USER + 3;
        public const ushort EM_SETRECTNP = WM_USER + 4;
        public const ushort EM_LINESCROLL = WM_USER + 6;
        public const ushort EM_GETMODIFY = WM_USER + 8;
        public const ushort EM_SETMODIFY = WM_USER + 9;
        public const ushort EM_GETLINECOUNT = WM_USER + 10;
        public const ushort EM_LINEINDEX = WM_USER + 11;
        public const ushort EM_SETHANDLE = WM_USER + 12;
        public const ushort EM_GETHANDLE = WM_USER + 13;
        public const ushort EM_LINELENGTH = WM_USER + 17;
        public const ushort EM_REPLACESEL = WM_USER + 18;
        public const ushort EM_SETFONT = WM_USER + 19;
        public const ushort EM_GETLINE = WM_USER + 20;
        public const ushort EM_LIMITTEXT = WM_USER + 21;
        public const ushort EM_CANUNDO = WM_USER + 22;
        public const ushort EM_UNDO = WM_USER + 23;
        public const ushort EM_FMTLINES = WM_USER + 24;
        public const ushort EM_LINEFROMCHAR = WM_USER + 25;
        public const ushort EM_SETWORDBREAK = WM_USER + 26;
        public const ushort EM_SETTABSTOPS = WM_USER + 27;
        public const ushort EM_SETPASSWORDCHAR = WM_USER + 28;
        public const ushort EM_EMPTYUNDOBUFFER = WM_USER + 29;
        public const ushort EM_GETFIRSTVISIBLELINE = WM_USER + 30;
        public const ushort EM_SETREADONLY = WM_USER + 31;
        public const ushort EM_SETWORDBREAKPROC = WM_USER + 32;
        public const ushort EM_GETWORDBREAKPROC = WM_USER + 33;
        public const ushort EM_GETPASSWORDCHAR = WM_USER + 34;
        public const ushort LB_ADDSTRING = WM_USER + 1;
        public const ushort LB_INSERTSTRING = WM_USER + 2;
        public const ushort LB_DELETESTRING = WM_USER + 3;
        public const ushort LB_RESETCONTENT = WM_USER + 5;
        public const ushort LB_SETSEL = WM_USER + 6;
        public const ushort LB_SETCURSEL = WM_USER + 7;
        public const ushort LB_GETSEL = WM_USER + 8;
        public const ushort LB_GETCURSEL = WM_USER + 9;
        public const ushort LB_GETTEXT = WM_USER + 10;
        public const ushort LB_GETTEXTLEN = WM_USER + 11;
        public const ushort LB_GETCOUNT = WM_USER + 12;
        public const ushort LB_SELECTSTRING = WM_USER + 13;
        public const ushort LB_DIR = WM_USER + 14;
        public const ushort LB_GETTOPINDEX = WM_USER + 15;
        public const ushort LB_FINDSTRING = WM_USER + 16;
        public const ushort LB_GETSELCOUNT = WM_USER + 17;
        public const ushort LB_GETSELITEMS = WM_USER + 18;
        public const ushort LB_SETTABSTOPS = WM_USER + 19;
        public const ushort LB_GETHORIZONTALEXTENT = WM_USER + 20;
        public const ushort LB_SETHORIZONTALEXTENT = WM_USER + 21;
        public const ushort LB_SETCOLUMNWIDTH = WM_USER + 22;
        public const ushort LB_SETTOPINDEX = WM_USER + 24;
        public const ushort LB_GETITEMRECT = WM_USER + 25;
        public const ushort LB_GETITEMDATA = WM_USER + 26;
        public const ushort LB_SETITEMDATA = WM_USER + 27;
        public const ushort LB_SELITEMRANGE = WM_USER + 28;
        public const ushort LB_SETCARETINDEX = WM_USER + 31;
        public const ushort LB_GETCARETINDEX = WM_USER + 32;
        public const ushort LB_SETITEMHEIGHT = WM_USER + 33;
        public const ushort LB_GETITEMHEIGHT = WM_USER + 34;
        public const ushort LB_FINDSTRINGEXACT = WM_USER + 35;
        public const ushort CB_GETEDITSEL = WM_USER + 0;
        public const ushort CB_LIMITTEXT = WM_USER + 1;
        public const ushort CB_SETEDITSEL = WM_USER + 2;
        public const ushort CB_ADDSTRING = WM_USER + 3;
        public const ushort CB_DELETESTRING = WM_USER + 4;
        public const ushort CB_DIR = WM_USER + 5;
        public const ushort CB_GETCOUNT = WM_USER + 6;
        public const ushort CB_GETCURSEL = WM_USER + 7;
        public const ushort CB_GETLBTEXT = WM_USER + 8;
        public const ushort CB_GETLBTEXTLEN = WM_USER + 9;
        public const ushort CB_INSERTSTRING = WM_USER + 10;
        public const ushort CB_RESETCONTENT = WM_USER + 11;
        public const ushort CB_FINDSTRING = WM_USER + 12;
        public const ushort CB_SELECTSTRING = WM_USER + 13;
        public const ushort CB_SETCURSEL = WM_USER + 14;
        public const ushort CB_SHOWDROPDOWN = WM_USER + 15;
        public const ushort CB_GETITEMDATA = WM_USER + 16;
        public const ushort CB_SETITEMDATA = WM_USER + 17;
        public const ushort CB_GETDROPPEDCONTROLRECT = WM_USER + 18;
        public const ushort CB_SETITEMHEIGHT = WM_USER + 19;
        public const ushort CB_GETITEMHEIGHT = WM_USER + 20;
        public const ushort CB_SETEXTENDEDUI = WM_USER + 21;
        public const ushort CB_GETEXTENDEDUI = WM_USER + 22;
        public const ushort CB_GETDROPPEDSTATE = WM_USER + 23;
        public const ushort CB_FINDSTRINGEXACT = WM_USER + 24;

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct MCI_OPEN_PARAMS
        {
            public uint dwCallback;
            public ushort wDeviceID;
            public ushort wReserved;
            public uint lpstrDeviceName;
            public uint lpstrElementName;
            public uint lpstrAlias;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct MCI_STATUS_PARAMS
        {
            public uint dwCallback;
            public uint dwReturn;
            public uint dwItem;
            public uint dwTrack;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct MCI_PLAY_PARAMS
        {
            public uint dwCallback;
            public uint dwFrom;
            public uint dwTo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct MCI_GENERIC_PARAMS
        {
            public uint dwCallback;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct MDINEXTMENU
        {
            public ushort hmenuIn;
            public ushort hmenuNext;
            public ushort hwndNext;

            public Win32.MDINEXTMENU Convert()
            {
                return new Win32.MDINEXTMENU()
                {
                    hmenuIn = HMENU.Map.To32(hmenuIn),
                    hmenuNext = HMENU.Map.To32(hmenuNext),
                    hwndNext = HWND.Map.To32(hwndNext),
                };
            }
        }
    }
}
