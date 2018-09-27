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

namespace Win3muCore
{
    public class WinCommon
    {
        public const ushort WM_CREATE = 0x0001;
        public const ushort WM_DESTROY = 0x0002;
        public const ushort WM_SIZE = 0x0005;
        public const ushort WM_SETTEXT = 0x000C;
        public const ushort WM_NCCREATE = 0x0081;
        public const ushort WM_NCCALCSIZE = 0x0083;
        public const ushort WM_KEYDOWN = 0x0100;
        public const ushort WM_SYSKEYDOWN = 0x0104;

        public const ushort WM_CTLCOLORBTN = 0x0135;
        public const ushort WM_CTLCOLORDLG = 0x0136;
        public const ushort WM_CTLCOLORSTATIC = 0x0138;

        public const int DWL_MSGRESULT = 0;

        // GetWindow
        public const uint GW_HWNDFIRST = 0;
        public const uint GW_HWNDLAST = 1;
        public const uint GW_HWNDNEXT = 2;
        public const uint GW_HWNDPREV = 3;
        public const uint GW_OWNER = 4;
        public const uint GW_CHILD = 5;
        public const uint GW_ENABLEDPOPUP = 6;

        public const ushort CS_OWNDC = 0x0020;
        public const ushort CS_CLASSDC = 0x0040;
        public const ushort CS_PARENTDC = 0x0080;


        public const uint WS_OVERLAPPED = 0x00000000;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_CAPTION = 0x00C00000;
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_VSCROLL = 0x00200000;
        public const uint WS_HSCROLL = 0x00100000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_GROUP = 0x00020000;
        public const uint WS_TABSTOP = 0x00010000;
        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;
        public const uint WS_TILED = WS_OVERLAPPED;
        public const uint WS_ICONIC = WS_MINIMIZE;
        public const uint WS_SIZEBOX = WS_THICKFRAME;
        public const uint WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;
        public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
        public const uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;
        public const uint WS_CHILDWINDOW = WS_CHILD;

        public const uint BS_PUSHBUTTON = 0x00000000;
        public const uint BS_DEFPUSHBUTTON = 0x00000001;
        public const uint BS_CHECKBOX = 0x00000002;
        public const uint BS_AUTOCHECKBOX = 0x00000003;
        public const uint BS_RADIOBUTTON = 0x00000004;
        public const uint BS_3STATE = 0x00000005;
        public const uint BS_AUTO3STATE = 0x00000006;
        public const uint BS_GROUPBOX = 0x00000007;
        public const uint BS_USERBUTTON = 0x00000008;
        public const uint BS_AUTORADIOBUTTON = 0x00000009;
        public const uint BS_OWNERDRAW = 0x0000000B;
        public const uint BS_LEFTTEXT = 0x00000020;

        public const uint SWP_ASYNCWINDOWPOS = 0x4000;
        public const uint SWP_DEFERERASE = 0x2000;
        public const uint SWP_DRAWFRAME = 0x0020;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_NOCOPYBITS = 0x0100;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOREPOSITION = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_SHOWWINDOW = 0x0040;

        // Since Win 3 didn't support WM_IME_* messages we'll override 
        // them for our own purposes

        // Used to sneak an unsupported message through the 16-bit code and
        // back out to the 32-bit code.  Relies on well behaved app that's
        // not inadvertantly suppressing that it shouldn't.  (ie: it better
        // call DefWindowProc or the old window proc for subclassed windows)
        public const ushort WM_WIN3MU_BYPASS16 = 0x280;     // lParam = BypassInfo Id

        public const ushort WM_USER = 0x0400;
        public const ushort WM_REGISTEREDBASE = 0xC000;

        public const ushort VK_F5 = 0x74;
        public const ushort VK_F6 = 0x75;
        public const ushort VK_F7 = 0x76;
        public const ushort VK_F8 = 0x77;
        public const ushort VK_F9 = 0x78;
        public const ushort VK_F10 = 0x79;

        public static string[] SystemColorNames = new string[]
        {
            "COLOR_SCROLLBAR",
            "COLOR_BACKGROUND",
            "COLOR_ACTIVECAPTION",
            "COLOR_INACTIVECAPTION",
            "COLOR_MENU",
            "COLOR_WINDOW",
            "COLOR_WINDOWFRAME",
            "COLOR_MENUTEXT",
            "COLOR_WINDOWTEXT",
            "COLOR_CAPTIONTEXT",
            "COLOR_ACTIVEBORDER",
            "COLOR_INACTIVEBORDER",
            "COLOR_APPWORKSPACE",
            "COLOR_HIGHLIGHT", 
            "COLOR_HIGHLIGHTTEXT",
            "COLOR_BTNFACE", 
            "COLOR_BTNSHADOW",
            "COLOR_GRAYTEXT",
            "COLOR_BTNTEXT",
            "COLOR_INACTIVECAPTIONTEXT", 
            "COLOR_BTNHIGHLIGHT", 
        };

        public const int COLOR_SCROLLBAR = 0;
        public const int COLOR_BACKGROUND = 1;
        public const int COLOR_ACTIVECAPTION = 2;
        public const int COLOR_INACTIVECAPTION = 3;
        public const int COLOR_MENU = 4;
        public const int COLOR_WINDOW = 5;
        public const int COLOR_WINDOWFRAME = 6;
        public const int COLOR_MENUTEXT = 7;
        public const int COLOR_WINDOWTEXT = 8;
        public const int COLOR_CAPTIONTEXT = 9;
        public const int COLOR_ACTIVEBORDER = 10;
        public const int COLOR_INACTIVEBORDER = 11;
        public const int COLOR_APPWORKSPACE = 12;
        public const int COLOR_HIGHLIGHT = 13;
        public const int COLOR_HIGHLIGHTTEXT = 14;
        public const int COLOR_BTNFACE = 15;
        public const int COLOR_BTNSHADOW = 16;
        public const int COLOR_GRAYTEXT = 17;
        public const int COLOR_BTNTEXT = 18;
        public const int COLOR_INACTIVECAPTIONTEXT = 19;
        public const int COLOR_BTNHIGHLIGHT = 20;

        public const int DRIVERVERSION = 0;
        public const int TECHNOLOGY = 2;
        public const int HORZSIZE = 4;
        public const int VERTSIZE = 6;
        public const int HORZRES = 8;
        public const int VERTRES = 10;
        public const int BITSPIXEL = 12;
        public const int PLANES = 14;
        public const int NUMBRUSHES = 16;
        public const int NUMPENS = 18;
        public const int NUMMARKERS = 20;
        public const int NUMFONTS = 22;
        public const int NUMCOLORS = 24;
        public const int PDEVICESIZE = 26;
        public const int CURVECAPS = 28;
        public const int LINECAPS = 30;
        public const int POLYGONALCAPS = 32;
        public const int TEXTCAPS = 34;
        public const int CLIPCAPS = 36;
        public const int RASTERCAPS = 38;
        public const int ASPECTX = 40;
        public const int ASPECTY = 42;
        public const int ASPECTXY = 44;

        public const int DT_PLOTTER = 0;
        public const int DT_RASDISPLAY = 1;
        public const int DT_RASPRINTER = 2;
        public const int DT_RASCAMERA = 3;
        public const int DT_CHARSTREAM = 4;
        public const int DT_METAFILE = 5;
        public const int DT_DISPFILE = 6;


        public static string[] SystemMetricNames = new string[]
        {
            "SM_CXSCREEN", 
            "SM_CYSCREEN", 
            "SM_CXVSCROLL",
            "SM_CYHSCROLL",
            "SM_CYCAPTION",
            "SM_CXBORDER",
            "SM_CYBORDER",
            "SM_CXDLGFRAME",
            "SM_CYDLGFRAME",
            "SM_CYVTHUMB", 
            "SM_CXHTHUMB",
            "SM_CXICON", 
            "SM_CYICON", 
            "SM_CXCURSOR",
            "SM_CYCURSOR",
            "SM_CYMENU", 
            "SM_CXFULLSCREEN",
            "SM_CYFULLSCREEN",
            "SM_CYKANJIWINDOW",
            "SM_MOUSEPRESENT", 
            "SM_CYVSCROLL",
            "SM_CXHSCROLL",
            "SM_DEBUG", 
            "SM_SWAPBUTTON", 
            "SM_RESERVED1", 
            "SM_RESERVED2", 
            "SM_RESERVED3", 
            "SM_RESERVED4",  
            "SM_CXMIN", 
            "SM_CYMIN", 
            "SM_CXSIZE", 
            "SM_CYSIZE", 
            "SM_CXFRAME",
            "SM_CYFRAME",
            "SM_CXMINTRACK",
            "SM_CYMINTRACK",
            "SM_CXDOUBLECLK",
            "SM_CYDOUBLECLK",
            "SM_CXICONSPACING",
            "SM_CYICONSPACING",
            "SM_MENUDROPALIGNMENT",
            "SM_PENWINDOWS", 
            "SM_DBCSENABLED", 
            "SM_CMETRICS", 
        };

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        public const int SM_CXVSCROLL = 2;
        public const int SM_CYHSCROLL = 3;
        public const int SM_CYCAPTION = 4;
        public const int SM_CXBORDER = 5;
        public const int SM_CYBORDER = 6;
        public const int SM_CXDLGFRAME = 7;
        public const int SM_CYDLGFRAME = 8;
        public const int SM_CYVTHUMB = 9;
        public const int SM_CXHTHUMB = 10;
        public const int SM_CXICON = 11;
        public const int SM_CYICON = 12;
        public const int SM_CXCURSOR = 13;
        public const int SM_CYCURSOR = 14;
        public const int SM_CYMENU = 15;
        public const int SM_CXFULLSCREEN = 16;
        public const int SM_CYFULLSCREEN = 17;
        public const int SM_CYKANJIWINDOW = 18;
        public const int SM_MOUSEPRESENT = 19;
        public const int SM_CYVSCROLL = 20;
        public const int SM_CXHSCROLL = 21;
        public const int SM_DEBUG = 22;
        public const int SM_SWAPBUTTON = 23;
        public const int SM_RESERVED1 = 24;
        public const int SM_RESERVED2 = 25;
        public const int SM_RESERVED3 = 26;
        public const int SM_RESERVED4 = 27;
        public const int SM_CXMIN = 28;
        public const int SM_CYMIN = 29;
        public const int SM_CXSIZE = 30;
        public const int SM_CYSIZE = 31;
        public const int SM_CXFRAME = 32;
        public const int SM_CYFRAME = 33;
        public const int SM_CXMINTRACK = 34;
        public const int SM_CYMINTRACK = 35;
        public const int SM_CXDOUBLECLK = 36;
        public const int SM_CYDOUBLECLK = 37;
        public const int SM_CXICONSPACING = 38;
        public const int SM_CYICONSPACING = 39;
        public const int SM_MENUDROPALIGNMENT = 40;
        public const int SM_PENWINDOWS = 41;
        public const int SM_DBCSENABLED = 42;
        public const int SM_CMETRICS = 43;

        public const ushort MF_POPUP = 0x0010;
        public const ushort MF_SYSMENU = 0x2000;
        public const ushort MF_BITMAP = 0x0004;
        public const ushort MF_OWNERDRAW = 0x0100;

        public const ushort MF_INSERT = 0x0000;
        public const ushort MF_CHANGE = 0x0080;
        public const ushort MF_APPEND = 0x0100;
        public const ushort MF_DELETE = 0x0200;
        public const ushort MF_REMOVE = 0x1000;

        public const uint CBM_INIT = 0x00000004;

        public const ushort DIB_RGB_COLORS = 0;
        public const ushort DIB_PAL_COLORS = 1;

        public const ushort BS_SOLID = 0;
        public const ushort BS_NULL = 1;
        public const ushort BS_HOLLOW = BS_NULL;
        public const ushort BS_HATCHED = 2;
        public const ushort BS_PATTERN = 3;
        public const ushort BS_INDEXED = 4;
        public const ushort BS_DIBPATTERN = 5;


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BITMAPCOREHEADER
        {
            public uint bcSize;
            public short bcWidth;
            public short bcHeight;
            public ushort bcPlanes;
            public ushort bcBitCount;
        }

        public const byte FVIRTKEY = 0x01;
        public const byte FNOINVERT = 0x02;
        public const byte FSHIFT = 0x04;
        public const byte FCONTROL = 0x08;
        public const byte FALT = 0x10;
        public const byte FMASK = 0x01F;
        public const byte FENDOFRESOURCE = 0x80;

        public const ushort HELP_CONTEXT = 0x0001;
        public const ushort HELP_QUIT = 0x0002;
        public const ushort HELP_INDEX = 0x0003;
        public const ushort HELP_CONTENTS = 0x0003;
        public const ushort HELP_HELPONHELP = 0x0004;
        public const ushort HELP_SETINDEX = 0x0005;
        public const ushort HELP_SETCONTENTS = 0x0005;
        public const ushort HELP_CONTEXTPOPUP = 0x0008;
        public const ushort HELP_FORCEFILE = 0x0009;
        public const ushort HELP_KEY = 0x0101;
        public const ushort HELP_COMMAND = 0x0102;
        public const ushort HELP_PARTIALKEY = 0x0105;
        public const ushort HELP_MULTIKEY = 0x0201;
        public const ushort HELP_SETWINPOS = 0x0203;

        public const ushort MB_OK = 0X00000000;
        public const ushort MB_OKCANCEL = 0X00000001;
        public const ushort MB_ABORTRETRYIGNORE = 0x00000002;
        public const ushort MB_YESNOCANCEL = 0x00000003;
        public const ushort MB_YESNO = 0X00000004;
        public const ushort MB_RETRYCANCEL = 0x00000005;
        public const ushort MB_ERROR = 0X00000010;
        public const ushort MB_WARNING = 0X00000030;
        public const ushort MB_QUESTION = 0X00000020;
        public const ushort MB_INFORMATION = 0x00000040;
        public const ushort MB_TASKMODAL = 0x00002000;

        // Windows hook types
        public const short WH_MSGFILTER = -1;
        public const short WH_JOURNALRECORD = 0;
        public const short WH_JOURNALPLAYBACK = 1;
        public const short WH_KEYBOARD = 2;
        public const short WH_GETMESSAGE = 3;
        public const short WH_CALLWNDPROC = 4;
        public const short WH_CBT = 5;
        public const short WH_SYSMSGFILTER = 6;
        public const short WH_MOUSE = 7;
        public const short WH_HARDWARE = 8;
        public const short WH_DEBUG = 9;
        public const short WH_SHELL = 10;

        // MCI Commands
        public const ushort MCI_OPEN = 0x0803;
        public const ushort MCI_CLOSE = 0x0804;
        public const ushort MCI_ESCAPE = 0x0805;
        public const ushort MCI_PLAY = 0x0806;
        public const ushort MCI_SEEK = 0x0807;
        public const ushort MCI_STOP = 0x0808;
        public const ushort MCI_PAUSE = 0x0809;
        public const ushort MCI_INFO = 0x080A;
        public const ushort MCI_GETDEVCAPS = 0x080B;
        public const ushort MCI_SPIN = 0x080C;
        public const ushort MCI_SET = 0x080D;
        public const ushort MCI_STEP = 0x080E;
        public const ushort MCI_RECORD = 0x080F;
        public const ushort MCI_SYSINFO = 0x0810;
        public const ushort MCI_BREAK = 0x0811;
        public const ushort MCI_SAVE = 0x0813;
        public const ushort MCI_STATUS = 0x0814;
        public const ushort MCI_CUE = 0x0830;
        public const ushort MCI_REALIZE = 0x0840;
        public const ushort MCI_WINDOW = 0x0841;
        public const ushort MCI_PUT = 0x0842;
        public const ushort MCI_WHERE = 0x0843;
        public const ushort MCI_FREEZE = 0x0844;
        public const ushort MCI_UNFREEZE = 0x0845;
        public const ushort MCI_LOAD = 0x0850;
        public const ushort MCI_CUT = 0x0851;
        public const ushort MCI_COPY = 0x0852;
        public const ushort MCI_PASTE = 0x0853;
        public const ushort MCI_UPDATE = 0x0854;
        public const ushort MCI_RESUME = 0x0855;
        public const ushort MCI_DELETE = 0x0856;

        public const uint MCI_OPEN_SHAREABLE = 0x00000100;
        public const uint MCI_OPEN_ELEMENT = 0x00000200;
        public const uint MCI_OPEN_ALIAS = 0x00000400;
        public const uint MCI_OPEN_ELEMENT_ID = 0x00000800;
        public const uint MCI_OPEN_TYPE_ID = 0x00001000;
        public const uint MCI_OPEN_TYPE = 0x00002000;

        public const short DRIVE_REMOVABLE = 2;
        public const short DRIVE_FIXED = 3;
        public const short DRIVE_REMOTE = 4;

        public const ushort DDL_READWRITE = 0x0000;
        public const ushort DDL_READONLY = 0x0001;
        public const ushort DDL_HIDDEN = 0x0002;
        public const ushort DDL_SYSTEM = 0x0004;
        public const ushort DDL_DIRECTORY = 0x0010;
        public const ushort DDL_ARCHIVE = 0x0020;
        public const ushort DDL_POSTMSGS = 0x2000;
        public const ushort DDL_DRIVES = 0x4000;
        public const ushort DDL_EXCLUSIVE = 0x8000;

        // These DDL_xxx flags match DosFileAttributes
        public const ushort DDL_ATTRIBUTE_MASK = DDL_READONLY | DDL_HIDDEN | DDL_SYSTEM | DDL_ARCHIVE | DDL_DIRECTORY;
        public const ushort DDL_EXPLICIT_MASK = DDL_READONLY | DDL_HIDDEN | DDL_SYSTEM | DDL_DIRECTORY;

    }
}
