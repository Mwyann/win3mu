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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Win3muCore
{
    public static class Resources
    {
        private const ushort MF_END = 0x0080;
        private const ushort MF_POPUP = 0x0010;
        private const ushort MF_BITMAP = 0x0004;
        private const ushort MF_MENUBREAK = 0x0040;
        private const ushort MF_HELP = 0x4000;

        private static bool LoadAppendMenu(HMENU hMenuParent, Stream stream)
        {
            // Read flags
            ushort flags = stream.ReadUInt16();

            if ((flags & MF_POPUP)==0)
            {
                // Regular menu item
                ushort id = stream.ReadUInt16();
                string text = stream.ReadNullTerminatedString();
                if (string.IsNullOrEmpty(text))
                    text = null;
                User.AppendMenu(hMenuParent, (uint)(flags & ~MF_END), (IntPtr)id, text);
            }
            else
            {
                // Popup menu item
                string text = stream.ReadNullTerminatedString();

                // Load all items
                var popup = User.CreatePopupMenu();
                while (LoadAppendMenu(popup, stream))
                {
                    ;
                }

                // Add it
                User.AppendMenu(hMenuParent, (uint)(flags & ~MF_END), popup.value, text);
            }

            return (flags & MF_END) == 0;
        }

        private static bool LoadAppendMenu_old(HMENU hMenuParent, Stream stream)
        {
            // Read flags
            ushort flags = (ushort)stream.ReadByte();

            if ((flags & MF_POPUP) == 0)
            {
                // Regular menu item
                ushort id = stream.ReadUInt16();
                string text = stream.ReadNullTerminatedString();
                if (text.StartsWith("\u0008"))
                {
                    flags |= MF_HELP;
                    text = text.Substring(1);
                }
                if (string.IsNullOrEmpty(text))
                    text = null;
                User.AppendMenu(hMenuParent, (uint)(flags & ~(MF_END | MF_BITMAP)), (IntPtr)id, text);
            }
            else
            {
                // Popup menu item
                string text = stream.ReadNullTerminatedString();

                // Load all items         
                var popup = User.CreatePopupMenu();
                while (LoadAppendMenu_old(popup, stream))
                {
                    ;
                }

                // Add it
                User.AppendMenu(hMenuParent, (uint)(flags & ~MF_END), popup.value, text);
            }

            return (flags & MF_END) == 0;
        }

        public static HMENU LoadMenu(Stream stream)
        {
            // Read version
            var wVersion = stream.ReadUInt16();
            if (wVersion!=0)
            {
                // Old menu format

                // Rewind
                stream.Seek(-2, SeekOrigin.Current);

                // Create the root menu
                var menu = User.CreateMenu();

                // Load all items
                while (LoadAppendMenu_old(menu, stream))
                {
                    ;
                }

                return menu;
            }
            else
            {
                // Read header size
                var wHeaderSize = stream.ReadUInt16();
                if (wHeaderSize!=0)
                {
                    Log.WriteLine("Failed to load menu - incorrect header size");
                    return HMENU.Null;
                }

                // Create the root menu
                var menu = User.CreateMenu();

                // Load all items
                while (LoadAppendMenu(menu, stream))
                {
                    ;
                }

                return menu;
            }
        }

        public static HGDIOBJ LoadBitmap(byte[] data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    uint size = *(uint*)ptr;
                    if (size == Marshal.SizeOf<Win16.BITMAPCOREHEADER>())
                    {
                        Win16.BITMAPCOREHEADER* pbmi = (Win16.BITMAPCOREHEADER*)ptr;
                        int numColors = pbmi->bcBitCount > 8 ? 0 : (1 << pbmi->bcBitCount);
                        byte* ptrColors = ptr + pbmi->bcSize + numColors * 4 - numColors;               // WTF Is this "- numColors"

                        var hdc = Gdi.CreateIC("display", null, null, IntPtr.Zero);
                        var ret = Gdi.CreateDIBitmap(hdc, (IntPtr)ptr, Win32.CBM_INIT, (IntPtr)ptrColors, (IntPtr)ptr, (uint)Win32.DIB_RGB_COLORS);
                        Gdi.DeleteDC(hdc);
                        return ret;
                    }
                    else
                    {
                        Win16.BITMAPINFOHEADER* pbmi = (Win16.BITMAPINFOHEADER*)ptr;
                        int numColors = (int)pbmi->biClrUsed;
                        if (numColors == 0 && pbmi->biBitCount<=8)
                        {
                            numColors = 1 << pbmi->biBitCount;
                        }
                        byte* ptrColors = ptr + pbmi->biSize + numColors * 4;

                        var hdc = Gdi.CreateIC("display", null, null, IntPtr.Zero);
                        var ret = Gdi.CreateDIBitmap(hdc, (IntPtr)ptr, Win32.CBM_INIT, (IntPtr)ptrColors, (IntPtr)ptr, (uint)Win32.DIB_RGB_COLORS);
                        Gdi.DeleteDC(hdc);
                        return ret;
                    }
                }
            }
        }

        public static Win16.GroupIcon LoadIconOrCursorGroup(Stream strm)
        {
            var g = new Win16.GroupIcon();
            g.Directory = strm.ReadStruct<Win16.GRPICONDIR>();
            for (int i=0; i<g.Directory.idCount; i++)
            {
                var de = strm.ReadStruct<Win16.GRPICONDIRENTRY>();
                g.Entries.Add(de);
            }
            return g;
        }

        // Ref 16 bit:  https://blogs.msdn.microsoft.com/oldnewthing/20040618-00/?p=38803
        // Ref 32 bit: https://blogs.msdn.microsoft.com/oldnewthing/20040621-00/?p=38793
        public static byte[] ConvertDialogTemplate(Stream strm)
        {
            var ms = new MemoryStream();

            // Convert header
            var header16 = strm.ReadStruct<Win16.DLGHEADER>();
            header16.x = (ushort)(header16.x * 118 / 100);
            header16.cx = (ushort)(header16.cx * 118 / 100);
            ms.WriteStruct(header16.Convert());

            // Convert strings
            ms.WriteResourceString32(strm.ReadResourceString16());      // Menu name
            ms.WriteResourceString32(strm.ReadResourceString16());      // Class name
            ms.WriteResourceString32(strm.ReadResourceString16());      // Title

            // Font?
            if ((header16.dwStyle & Win16.DS_SETFONT)!=0)
            {
                ms.WriteUInt16(strm.ReadUInt16());                      // Point size
                ms.WriteUnicodeString(strm.ReadNullTerminatedString()); // Font name
            }
            
            // Convert all controls
            for (int i=0; i<header16.cItems; i++)
            {
                ms.Pad(4);

                // Common header
                var item16 = strm.ReadStruct<Win16.DLGITEMHEADER>();
                item16.x = (ushort)(item16.x * 118 / 100);
                item16.cx = (ushort)(item16.cx * 118 / 100);
                ms.WriteStruct(item16.Convert());

                // Class name
                ms.WriteResourceString32(strm.ReadResourceString16(true), true);

                // Control text
                ms.WriteResourceString32(strm.ReadResourceString16(), true);

                // Extra data
                var byteCount = strm.ReadByte();
                ms.WriteUInt16((ushort)byteCount);
                if (byteCount>0)
                {
                    var buf = new byte[byteCount];
                    strm.Read(buf, 0, buf.Length);
                    ms.Write(buf, 0, buf.Length);
                }
            }

            var data = ms.ToArray();
            System.IO.File.WriteAllBytes("dlg.bin", data);
            return data;
        }
    }
}
