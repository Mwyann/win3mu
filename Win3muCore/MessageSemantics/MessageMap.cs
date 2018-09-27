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
    class MessageMap
    {
        public MessageMap()
        {
            Load();

            // Build wnd class kind message maps
            _wndClassKindMessages = new MessageLookup[(int)WndClassKind.Count];
            for (int i=0; i < (int)WndClassKind.Count; i++)
            {
                _wndClassKindMessages[i] = new MessageLookup(_messageInfos.Where(x => x.WndClassKind == (WndClassKind)i));
            }
        }

        public static void ThrowMessageError(IntPtr hWnd32, uint message)
        {
            throw new VirtualException($"Unknown windows message {MessageNames.NameOfMessage(message)} for window class '{User.GetClassName(hWnd32)}' ({WindowClassKind.Get(hWnd32)})");
        }

        public Base LookupMessage32(IntPtr hWnd, uint message32, out ushort message16)
        {
            // Look up standard messages first
            var mi = _wndClassKindMessages[(int)WndClassKind.Standard].Lookup32((ushort)message32);
            if (mi != null)
            {
                message16 = mi.message16;
                return mi.semantics;
            }

            // Pack and post?
            message16 = (ushort)message32;
            if (message32 == packAndPost.WM_PACKANDPOST)
            {
                return packAndPost.Instance;
            }

            // Registered windows message?
            if (message32 >= Win32.WM_REGISTEREDBASE && message32 <= 0xFFFF)
            {
                if (RegisteredWindowMessages.IsRegistered((ushort)message32))
                    return copy.Instance;
                else
                    return bypass.Instance;
            }

            // Get window class
            var wk = WindowClassKind.Get(hWnd);

            // Look up window class
            mi = _wndClassKindMessages[(int)wk].Lookup32((ushort)message32);
            if (mi != null)
            {
                message16 = mi.message16;
                return mi.semantics;
            }

            if (wk > WndClassKind.Unknown)
            {
                ThrowMessageError(hWnd, message32);
            }

            // Unknown message!
            message16 = (ushort)message32;
            return null;
        }

        public Base LookupMessage16(IntPtr hWnd, ushort message16, out uint message32)
        {
            // Look up standard messages first
            var mi = _wndClassKindMessages[(int)WndClassKind.Standard].Lookup16(message16);
            if (mi != null)
            {
                message32 = mi.message32;
                return mi.semantics;
            }

            // Registered windows message?
            if (message16 >= Win32.WM_REGISTEREDBASE && message16 <= 0xFFFF)
            {
                message32 = message16;
                return copy.Instance;
            }

            // Get window class
            var wk = WindowClassKind.Get(hWnd);


            // Look up window class
            mi = _wndClassKindMessages[(int)wk].Lookup16(message16);
            if (mi != null)
            {
                message32 = mi.message32;
                return mi.semantics;
            }

            /*
            if (wk > WndClassKind.Unknown)
            {
                ThrowMessageError(hWnd, message16);
            }
            */

            // Unknown message!
            message32 = message16;
            return packAndPost.Instance;
        }

        MessageLookup[] _wndClassKindMessages;
        List<MessageInfo> _messageInfos = new List<MessageInfo>();

        void Add(ushort message, Base semantics)
        {
            Add(WndClassKind.Standard, message, message, semantics);
        }

        void Add(WndClassKind wk, ushort message, Base semantics)
        {
            Add(wk, message, message, semantics);
        }

        void Add(WndClassKind wk, ushort msg32, ushort msg16, Base semantics)
        {
            /*
            if (wk == WndClassKind.Standard && (msg32 >= Win32.WM_USER || msg16 >= Win32.WM_USER))
            {
                System.Diagnostics.Debug.Assert(false);
            }
            */

            _messageInfos.Add(new MessageInfo()
            {
                WndClassKind = wk,
                message32 = msg32,
                message16 = msg16,
                semantics = semantics,
            });
        }

        // Info about one message type
        class MessageInfo
        {
            public WndClassKind WndClassKind;
            public ushort message32;
            public ushort message16;
            public Base semantics;
        }

        class MessageLookup
        {
            public MessageLookup(IEnumerable<MessageInfo> content)
            {
                if (!content.Any())
                {
                    _min32 = 1;
                    _max32 = 0;
                    _min16 = 1;
                    _max16 = 0;
                    return;
                }

                // Work out ranges
                _min32 = content.Min(x => x.message32);
                _max32 = content.Max(x => x.message32);
                _min16 = content.Min(x => x.message16);
                _max16 = content.Max(x => x.message16);

                // Allocate arrays
                _map16 = new MessageInfo[_max16 - _min16 + 1];
                _map32 = new MessageInfo[_max32 - _min32 + 1];

                // Initialize maps
                foreach (var mi in content)
                {
                    _map16[mi.message16 - _min16] = mi;
                    _map32[mi.message32 - _min32] = mi;
                }
            }

            public MessageInfo Lookup32(ushort message32)
            {
                if (message32 >= _min32 && message32 <= _max32)
                    return _map32[message32 - _min32];
                else
                    return null;
            }

            public MessageInfo Lookup16(ushort message16)
            {
                if (message16 >= _min16 && message16 <= _max16)
                    return _map16[message16 - _min16];
                else
                    return null;
            }

            ushort _min32;
            ushort _max32;
            ushort _min16;
            ushort _max16;
            MessageInfo[] _map32;
            MessageInfo[] _map16;
        }


        void Load()
        {
            // Standard messages
            Add(0x0000, new unused());             // WM_NULL
            Add(0x0001, new WM_NC_OR_CREATE(false));// WM_CREATE
            Add(0x0002, new WM_DESTROY());         // WM_DESTROY
            Add(0x0003, new copy());               // WM_MOVE
            Add(0x0005, new copy());               // WM_SIZE
            Add(0x0006, new WM_ACTIVATE());            // WM_ACTIVATE
            Add(0x0007, new hwnd_copy());          // WM_SETFOCUS
            Add(0x0008, new hwnd_copy());          // WM_KILLFOCUS
            Add(0x000A, new copy_unused());        // WM_ENABLE
            Add(0x000B, new copy_unused());        // WM_SETDRAW
            Add(0x000C, new WM_SETTEXT());         // WM_SETTEXT
            Add(0x000D, new WM_GETTEXT());         // WM_GETTEXT
            Add(0x000E, new unused());             // WM_GETTEXTLENGTH
            Add(0x000F, new unused());             // WM_PAINT
            Add(0x0010, new unused());             // WM_CLOSE
            Add(0x0012, new copy_unused());        // WM_QUIT
            Add(0x0013, new unused());             // WM_NULL
            Add(0x0014, new hdc_unused());         // WM_ERASEBKGND
            Add(0x0018, new copy());               // WM_SHOWWINDOW
            Add(0x0019, new WM_CTLCOLOR());        // WM_CTLCOLOR
            Add(0x001A, new unused_string());      // WM_WININICHANGE
            Add(0x001C, new copy_htask());         // WM_ACTIVATEAPP lParam is supposed to be task handle - hopefully DefWindowProce doesn't need it
            Add(0x001E, new unused());             // WM_TIMECHANGE
            Add(0x001F, new unused());             // WM_CANCELMODE
            Add(0x0020, new hwnd_copy());          // WM_SETCURSOR
            Add(0x0021, new hwnd_copy());          // WM_MOUSEACTIVATE
            Add(0x0022, new unused());             // WM_CHILDACTIVATE
            Add(0x0024, new WM_GETMINMAXINFO());   // WM_GETMINMAXINFO
            Add(0x002B, new WM_DRAWITEM());        // WM_DRAWITEM
            Add(0x002C, new WM_MEASUREITEM());     // WM_MEASUREITEM
            Add(0x002D, new WM_DELETEITEM());      // WM_DELETEITEM
            Add(0x0030, new hgdiobj_copy());       // WM_SETFONT
            Add(0x0031, new WM_GETFONT());         // WM_GETFONT
            Add(0x003D, new bypass());             // WM_GETOBJECT
            Add(0x0046, new bypass());             // WM_WINDOWPOSCHANGED
            Add(0x0047, new bypass());             // WM_WINDOWPOSCHANGING
            Add(0x0048, new copy());               // WM_POWER
            Add(0x004D, new bypass());             // ??? (press F1 on checkers)
            Add(0x0053, new bypass());             // WM_HELP
            Add(0x0060, new bypass());             // ??
            Add(0x007b, new bypass());             // WM_CONTEXTMENU
            Add(0x007c, new bypass());             // WM_STYLECHANGING
            Add(0x007d, new bypass());             // WM_STYLECHANGED
            Add(0x007e, new bypass());             // WM_DISPLAYCHANGE
            Add(0x007F, new bypass());             // WM_GETICON
            Add(0x0081, new WM_NC_OR_CREATE(true));// WM_NCCREATE
            Add(0x0082, new WM_NCDESTROY());       // WM_NCDESTROY
            Add(0x0083, new WM_NCCALCSIZE());      // WM_NCCALCSIZE
            Add(0x0084, new copy());               // WM_NCHITTEST
            Add(0x0085, new hgdiobj_unused());     // WM_NCPAINT
            Add(0x0086, new copy());               // WM_NCACTIVATE
            Add(0x0087, new unused());             // WM_GETDLGCODE
            Add(0x0088, new bypass());             // WM_SYNCPAINT
            Add(0x0090, new bypass());             // ??
            Add(0x0091, new bypass());             // ??
            Add(0x0092, new bypass());             // ??
            Add(0x0093, new bypass());             // ??
            Add(0x0094, new bypass());             // ??
            Add(0x00A0, new copy());               // WM_NCMOUSEMOVE
            Add(0x00a1, new copy());               // WM_NCLBUTTONDOWN
            Add(0x00a2, new copy());               // WM_NCLBUTTONUP
            Add(0x00a3, new copy());               // WM_NCLBUTTONDBLCLK
            Add(0x00a4, new copy());               // WM_NCRBUTTONDOWN
            Add(0x00a5, new copy());               // WM_NCRBUTTONUP
            Add(0x00a6, new copy());               // WM_NCRBUTTONDBLCLK
            Add(0x00a7, new copy());               // WM_NCMBUTTONDOWN
            Add(0x00a8, new copy());               // WM_NCMBUTTONUP
            Add(0x00a9, new copy());               // WM_NCMBUTTONDBLCLK
            Add(0x00ae, new bypass());             // ??
            Add(0x00e0, new bypass());             // SBM_SETPOS
            Add(0x00e1, new bypass());             // SBM_GETPOS
            Add(0x00e2, new bypass());             // SBM_SETRANGE
            Add(0x00e3, new bypass());             // SBM_GETRANGE
            Add(0x00e4, new bypass());             // SBM_ENABLE_ARROWS
            Add(0x00e6, new bypass());             // SBM_SETRANGEREDRAW
            Add(0x00e9, new bypass());             // SBM_SETSCROLLINFO
            Add(0x00ea, new bypass());             // SBM_GETSCROLLINFO
            Add(0x00eb, new bypass());             // SBM_GETSCROLLBARINFO
            Add(0x0100, new copy());               // WM_KEYDOWN
            Add(0x0101, new copy());               // WM_KEYUP
            Add(0x0102, new copy());               // WM_CHAR
            Add(0x0104, new copy());               // WM_SYSKEYDOWN
            Add(0x0105, new copy());               // WM_SYSKEYUP
            Add(0x0106, new copy());               // WM_SYSCHAR
            Add(0x0110, new WM_INITDIALOG());      // WM_INITDIALOG
            Add(0x0111, new WM_COMMAND());         // WM_COMMAND
            Add(0x0112, new copy());               // WM_SYSCOMMAND
            Add(0x0113, new WM_TIMER());           // WM_TIMER
            Add(0x0114, new WM_XSCROLL());         // WM_HSCROLL
            Add(0x0115, new WM_XSCROLL());         // WM_VSCROLL
            Add(0x0116, new hmenu_copy());         // WM_INITMENU
            Add(0x0117, new hmenu_copy());         // WM_INITMENUPOPUP
            Add(0x0118, new bypass());             // WM_SYSTIMER
            Add(0x011f, new WM_MENUSELECT());      // WM_MENUSELECT
            Add(0x0120, new WM_MENUCHAR());        // WM_MENUCHAR
            Add(0x0127, new bypass());             // WM_CHANGEUISTATE
            Add(0x0128, new bypass());             // WM_UPDATEUISTATE
            Add(0x0129, new bypass());             // WM_QUERYUISTATE
            Add(0x0121, new copy_hwnd());          // WM_ENTERIDLE
            Add(0x0125, new bypass());             // WM_UNINITMENUPOPUP
            Add(0x0131, new bypass());             // ???
            Add(0x0132, new WM_CTLCOLOR());        // WM_CTLCOLORMSGBOX
            Add(0x0133, new WM_CTLCOLOR());        // WM_CTLCOLOREDIT
            Add(0x0134, new WM_CTLCOLOR());        // WM_CTLCOLORLISTBOX
            Add(0x0135, new WM_CTLCOLOR());        // WM_CTLCOLORBTN
            Add(0x0136, new WM_CTLCOLOR());        // WM_CTLCOLORDLG
            Add(0x0137, new WM_CTLCOLOR());        // WM_CTLCOLORSCROLLBAR
            Add(0x0138, new WM_CTLCOLOR());        // WM_CTLCOLORSTATIC
            Add(0x0200, new copy());               // WM_MOUSEMOVE
            Add(0x0201, new copy());               // WM_LBUTTONDOWN
            Add(0x0202, new copy());               // WM_LBUTTONUP
            Add(0x0203, new copy());               // WM_LBUTTONDBLCLK
            Add(0x0204, new copy());               // WM_RBUTTONDOWN
            Add(0x0205, new copy());               // WM_RBUTTONUP
            Add(0x0206, new copy());               // WM_RBUTTONDBLCLK
            Add(0x0207, new copy());               // WM_MBUTTONDOWN
            Add(0x0208, new copy());               // WM_MBUTTONUP
            Add(0x0209, new copy());               // WM_MBUTTONDBLCLK
            Add(0x020A, new bypass());             // WM_MOUSEWHEEL
            Add(0x0210, new WM_PARENTNOTIFY());    // WM_PARENTNOTIFY
            Add(0x0211, new bypass());             // WM_ENTERMENULOOP
            Add(0x0212, new bypass());             // WM_EXITMENULOOP
            Add(0x0213, new WM_NEXTMENU());        // WM_NEXTMENU
            Add(0x0214, new bypass());             // WM_SIZING
            Add(0x0215, new bypass());             // WM_CAPTURECHANGED
            Add(0x0216, new bypass());             // WM_MOVING
            Add(0x0218, new bypass());             // WM_POWERBROADCAST
            Add(0x0219, new bypass());             // WM_DEVICECHANGE
            Add(0x0231, new bypass());             // WM_ENTERSIZEMOVE
            Add(0x0232, new bypass());             // WM_EXITSIZEMOVE
            Add(0x0281, new bypass());             // WM_IME_SETCONTEXT
            Add(0x0282, new bypass());             // WM_IME_NOTIFY            
            Add(0x02a2, new bypass());             // WM_NCMOUSELEAVE
            Add(0x02a3, new bypass());             // WM_MOUSELEAVE
            Add(0x031e, new bypass());             // WM_DWMCOMPOSITIONCHANGED
            Add(0x031f, new bypass());             // WM_DWMNCRENDERINGCHANGED   
            Add(0x0317, new bypass());             // WM_PRINT
            Add(0x0318, new bypass());             // WM_PRINTCLIENT
            Add(0x0320, new bypass());             // WM_DWMCOLORIZATIONCOLORCHANGED   
            Add(0x0321, new bypass());             // WM_DWMWINDOWMAXIMIZEDCHANGE   
            Add(0x0323, new bypass());             // WM_DWMSENDICONICTHUMBNAIL   
            Add(0x0326, new bypass());             // WM_DWMSENDICONICLIVEPREVIEWBITMAP   
            Add(0x03B9, new copy());               // MM_MCINOTIFY

            Add(0x0737, new bypass());             // ??
            Add(0x0738, new bypass());             // ??

            // Dialog
            Add(WndClassKind.Dialog, 0x0400, new copy());      // DM_GETDEFID
            Add(WndClassKind.Dialog, 0x0401, new copy());      // DM_SETDEFID

            // Button
            Add(WndClassKind.Button, Win32.BM_GETCHECK, Win16.BM_GETCHECK, new unused());
            Add(WndClassKind.Button, Win32.BM_SETCHECK, Win16.BM_SETCHECK, new copy_unused());
            Add(WndClassKind.Button, Win32.BM_SETSTATE, Win16.BM_SETSTATE, new copy_unused());
            Add(WndClassKind.Button, Win32.BM_SETSTYLE, Win16.BM_SETSTYLE, new copy());

            // Edit
            Add(WndClassKind.Edit, Win32.EM_SETSEL, Win16.EM_SETSEL, new cracked_lparam16());
            Add(WndClassKind.Edit, Win32.EM_SETLIMITTEXT, Win16.EM_LIMITTEXT, new copy());

            // Listbox
            Add(WndClassKind.Listbox, Win32.LB_ADDSTRING, Win16.LB_ADDSTRING, new ClassListBox.LB_ADDSTRING());
            Add(WndClassKind.Listbox, Win32.LB_INSERTSTRING, Win16.LB_INSERTSTRING, new ClassListBox.LB_ADDSTRING());
            Add(WndClassKind.Listbox, Win32.LB_SETCURSEL, Win16.LB_SETCURSEL, new copy_unused());
            Add(WndClassKind.Listbox, Win32.LB_GETCURSEL, Win16.LB_GETCURSEL, new unused());
            Add(WndClassKind.Listbox, Win32.LB_GETCARETINDEX, Win16.LB_GETCARETINDEX, new unused());
            Add(WndClassKind.Listbox, Win32.LB_FINDSTRING, Win16.LB_FINDSTRING, new copy_string());
            Add(WndClassKind.Listbox, Win32.LB_RESETCONTENT, Win16.LB_RESETCONTENT, new unused());
            Add(WndClassKind.Listbox, Win32.LB_GETTEXT, Win16.LB_GETTEXT, new copy_outstring());
            Add(WndClassKind.Listbox, Win32.LB_SETTOPINDEX, Win16.LB_SETTOPINDEX, new copy_unused());
            Add(WndClassKind.Listbox, Win32.LB_GETTOPINDEX, Win16.LB_GETTOPINDEX, new unused());

            // ComboBox
            Add(WndClassKind.Combobox, Win32.CB_ADDSTRING, Win16.CB_ADDSTRING, new ClassComboBox.CB_ADDSTRING());
            Add(WndClassKind.Combobox, Win32.CB_INSERTSTRING, Win16.CB_INSERTSTRING, new ClassComboBox.CB_ADDSTRING());
            Add(WndClassKind.Combobox, Win32.CB_SETCURSEL, Win16.CB_SETCURSEL, new copy_unused());
            Add(WndClassKind.Combobox, Win32.CB_GETCURSEL, Win16.CB_GETCURSEL, new unused());
            Add(WndClassKind.Combobox, Win32.CB_SETITEMDATA, Win16.CB_SETITEMDATA, new copy());
            Add(WndClassKind.Combobox, Win32.CB_GETITEMDATA, Win16.CB_GETITEMDATA, new copy());
            Add(WndClassKind.Combobox, Win32.CB_RESETCONTENT, Win16.CB_RESETCONTENT, new unused());
            Add(WndClassKind.Combobox, Win32.CB_GETCOUNT, Win16.CB_GETCOUNT, new unused());
            Add(WndClassKind.Combobox, Win32.CB_GETLBTEXT, Win16.CB_GETLBTEXT, new copy_outstring());
            Add(WndClassKind.Combobox, Win32.CB_FINDSTRING, Win16.CB_FINDSTRING, new copy_string());

            /*
            Add(WndClassKind.Button, Win32.BM_GETCHECK, Win16.BM_GETCHECK, new notimpl());
            Add(WndClassKind.Button, Win32.BM_SETCHECK, Win16.BM_SETCHECK, new notimpl());
            Add(WndClassKind.Button, Win32.BM_GETSTATE, Win16.BM_GETSTATE, new notimpl());
            Add(WndClassKind.Button, Win32.BM_SETSTATE, Win16.BM_SETSTATE, new notimpl());
            Add(WndClassKind.Button, Win32.BM_SETSTYLE, Win16.BM_SETSTYLE, new notimpl());
//            Add(WndClassKind.Button, Win32.BM_CLICK, Win16.BM_CLICK, new notimpl());
//            Add(WndClassKind.Button, Win32.BM_GETIMAGE, Win16.BM_GETIMAGE, new notimpl());
//            Add(WndClassKind.Button, Win32.BM_SETIMAGE, Win16.BM_SETIMAGE, new notimpl());
            Add(WndClassKind.Static, Win32.STM_SETICON, Win16.STM_SETICON, new notimpl());
            Add(WndClassKind.Static, Win32.STM_GETICON, Win16.STM_GETICON, new notimpl());
//            Add(WndClassKind.Static, Win32.STM_SETIMAGE, Win16.STM_SETIMAGE, new notimpl());
//            Add(WndClassKind.Static, Win32.STM_GETIMAGE, Win16.STM_GETIMAGE, new notimpl());

            Add(WndClassKind.Listbox, Win32.LB_INSERTSTRING, Win16.LB_INSERTSTRING, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_DELETESTRING, Win16.LB_DELETESTRING, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_SELITEMRANGEEX, Win16.LB_SELITEMRANGEEX, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SETSEL, Win16.LB_SETSEL, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SETCURSEL, Win16.LB_SETCURSEL, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETSEL, Win16.LB_GETSEL, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETCURSEL, Win16.LB_GETCURSEL, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETTEXT, Win16.LB_GETTEXT, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETTEXTLEN, Win16.LB_GETTEXTLEN, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETCOUNT, Win16.LB_GETCOUNT, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SELECTSTRING, Win16.LB_SELECTSTRING, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETSELCOUNT, Win16.LB_GETSELCOUNT, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETSELITEMS, Win16.LB_GETSELITEMS, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SETTABSTOPS, Win16.LB_SETTABSTOPS, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETHORIZONTALEXTENT, Win16.LB_GETHORIZONTALEXTENT, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SETHORIZONTALEXTENT, Win16.LB_SETHORIZONTALEXTENT, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SETCOLUMNWIDTH, Win16.LB_SETCOLUMNWIDTH, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_ADDFILE, Win16.LB_ADDFILE, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SETTOPINDEX, Win16.LB_SETTOPINDEX, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETITEMRECT, Win16.LB_GETITEMRECT, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETITEMDATA, Win16.LB_GETITEMDATA, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SETITEMDATA, Win16.LB_SETITEMDATA, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SELITEMRANGE, Win16.LB_SELITEMRANGE, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_SETANCHORINDEX, Win16.LB_SETANCHORINDEX, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_GETANCHORINDEX, Win16.LB_GETANCHORINDEX, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SETCARETINDEX, Win16.LB_SETCARETINDEX, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_SETITEMHEIGHT, Win16.LB_SETITEMHEIGHT, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_GETITEMHEIGHT, Win16.LB_GETITEMHEIGHT, new notimpl());
            Add(WndClassKind.Listbox, Win32.LB_FINDSTRINGEXACT, Win16.LB_FINDSTRINGEXACT, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_SETLOCALE, Win16.LB_SETLOCALE, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_GETLOCALE, Win16.LB_GETLOCALE, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_SETCOUNT, Win16.LB_SETCOUNT, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_INITSTORAGE, Win16.LB_INITSTORAGE, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_ITEMFROMPOINT, Win16.LB_ITEMFROMPOINT, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_MULTIPLEADDSTRING, Win16.LB_MULTIPLEADDSTRING, new notimpl());
//            Add(WndClassKind.Listbox, Win32.LB_GETLISTBOXINFO, Win16.LB_GETLISTBOXINFO, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETEDITSEL, Win16.CB_GETEDITSEL, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_LIMITTEXT, Win16.CB_LIMITTEXT, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_SETEDITSEL, Win16.CB_SETEDITSEL, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_ADDSTRING, Win16.CB_ADDSTRING, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_DELETESTRING, Win16.CB_DELETESTRING, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_DIR, Win16.CB_DIR, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETCOUNT, Win16.CB_GETCOUNT, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETCURSEL, Win16.CB_GETCURSEL, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETLBTEXT, Win16.CB_GETLBTEXT, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETLBTEXTLEN, Win16.CB_GETLBTEXTLEN, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_INSERTSTRING, Win16.CB_INSERTSTRING, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_RESETCONTENT, Win16.CB_RESETCONTENT, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_FINDSTRING, Win16.CB_FINDSTRING, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_SELECTSTRING, Win16.CB_SELECTSTRING, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_SETCURSEL, Win16.CB_SETCURSEL, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_SHOWDROPDOWN, Win16.CB_SHOWDROPDOWN, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETITEMDATA, Win16.CB_GETITEMDATA, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_SETITEMDATA, Win16.CB_SETITEMDATA, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETDROPPEDCONTROLRECT, Win16.CB_GETDROPPEDCONTROLRECT, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_SETITEMHEIGHT, Win16.CB_SETITEMHEIGHT, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETITEMHEIGHT, Win16.CB_GETITEMHEIGHT, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_SETEXTENDEDUI, Win16.CB_SETEXTENDEDUI, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETEXTENDEDUI, Win16.CB_GETEXTENDEDUI, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_GETDROPPEDSTATE, Win16.CB_GETDROPPEDSTATE, new notimpl());
            Add(WndClassKind.Combobox, Win32.CB_FINDSTRINGEXACT, Win16.CB_FINDSTRINGEXACT, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_SETLOCALE, Win16.CB_SETLOCALE, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_GETLOCALE, Win16.CB_GETLOCALE, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_GETTOPINDEX, Win16.CB_GETTOPINDEX, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_SETTOPINDEX, Win16.CB_SETTOPINDEX, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_GETHORIZONTALEXTENT, Win16.CB_GETHORIZONTALEXTENT, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_SETHORIZONTALEXTENT, Win16.CB_SETHORIZONTALEXTENT, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_GETDROPPEDWIDTH, Win16.CB_GETDROPPEDWIDTH, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_SETDROPPEDWIDTH, Win16.CB_SETDROPPEDWIDTH, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_INITSTORAGE, Win16.CB_INITSTORAGE, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_MULTIPLEADDSTRING, Win16.CB_MULTIPLEADDSTRING, new notimpl());
//            Add(WndClassKind.Combobox, Win32.CB_GETCOMBOBOXINFO, Win16.CB_GETCOMBOBOXINFO, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_GETSEL, Win16.EM_GETSEL, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_SETSEL, Win16.EM_SETSEL, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_GETRECT, Win16.EM_GETRECT, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_SETRECT, Win16.EM_SETRECT, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_SETRECTNP, Win16.EM_SETRECTNP, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_SCROLL, Win16.EM_SCROLL, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_LINESCROLL, Win16.EM_LINESCROLL, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_SCROLLCARET, Win16.EM_SCROLLCARET, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_GETMODIFY, Win16.EM_GETMODIFY, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_SETMODIFY, Win16.EM_SETMODIFY, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_GETLINECOUNT, Win16.EM_GETLINECOUNT, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_LINEINDEX, Win16.EM_LINEINDEX, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_SETHANDLE, Win16.EM_SETHANDLE, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_GETHANDLE, Win16.EM_GETHANDLE, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_GETTHUMB, Win16.EM_GETTHUMB, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_LINELENGTH, Win16.EM_LINELENGTH, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_REPLACESEL, Win16.EM_REPLACESEL, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_GETLINE, Win16.EM_GETLINE, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_SETLIMITTEXT, Win16.EM_SETLIMITTEXT, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_CANUNDO, Win16.EM_CANUNDO, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_UNDO, Win16.EM_UNDO, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_FMTLINES, Win16.EM_FMTLINES, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_LINEFROMCHAR, Win16.EM_LINEFROMCHAR, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_SETTABSTOPS, Win16.EM_SETTABSTOPS, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_SETPASSWORDCHAR, Win16.EM_SETPASSWORDCHAR, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_EMPTYUNDOBUFFER, Win16.EM_EMPTYUNDOBUFFER, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_GETFIRSTVISIBLELINE, Win16.EM_GETFIRSTVISIBLELINE, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_SETREADONLY, Win16.EM_SETREADONLY, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_SETWORDBREAKPROC, Win16.EM_SETWORDBREAKPROC, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_GETWORDBREAKPROC, Win16.EM_GETWORDBREAKPROC, new notimpl());
            Add(WndClassKind.Edit, Win32.EM_GETPASSWORDCHAR, Win16.EM_GETPASSWORDCHAR, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_SETMARGINS, Win16.EM_SETMARGINS, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_GETMARGINS, Win16.EM_GETMARGINS, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_GETLIMITTEXT, Win16.EM_GETLIMITTEXT, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_POSFROMCHAR, Win16.EM_POSFROMCHAR, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_CHARFROMPOS, Win16.EM_CHARFROMPOS, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_SETIMESTATUS, Win16.EM_SETIMESTATUS, new notimpl());
//            Add(WndClassKind.Edit, Win32.EM_GETIMESTATUS, Win16.EM_GETIMESTATUS, new notimpl());
            */
        }

    }
}
