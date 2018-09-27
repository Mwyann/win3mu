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
using Sharp86;

namespace Win3muCore
{
    [Module("KERNEL", @"C:\WINDOWS\SYSTEM\KRNL286.EXE")]
    public class Kernel : Module32
    {
        public override uint GetProcAddress(ushort ordinal)
        {
            switch (ordinal)
            {
                case 0x0000:
                case 0x00B2:
                    // __WINFLAGS
                    // Under XP, WOW returns 0x4c29
                    return 0xFFFF0000 | Win16.WF_PMODE | Win16.WF_CPU286 | Win16.WF_STANDARD | Win16.WF_PAGING;

                case 0x0071:
                    // __AHSHIFT
                    return 0xFFFF0003;

                case 0x0072:
                    // __AHINCR
                    return 0xFFFF0008;
            }

            return base.GetProcAddress(ordinal);
        }


        [EntryPoint(0x0001)]
        public void FatalExit(short errorCode)
        {
            throw new NotImplementedException();
        }

        // 0002 - EXITKERNEL

        [EntryPoint(0x0003)]
        public ushort GetVersion()
        {
            return 0x0003;
        }

        // 0004 - LOCALINIT
        [EntryPoint(0x0004)]
        public bool LocalInit(ushort segment, ushort start, ushort end)
        {
            if (segment == 0)
            {
                segment = _machine.ds;
            }

            // Already initialized?
            if (_machine.GlobalHeap.GetLocalHeap(segment) != null)
                return false;

            if (start == 0)
            {
                var size = GlobalSize(segment);
                start = (ushort)(size - end);
                end = (ushort)(size);
            }

            // Create it
            if (_machine.GlobalHeap.CreateLocalHeap(segment, start, (ushort)(end - start)) != null)
                return true;

            return false;
        }

        [EntryPoint(0x0005)]
        public ushort LocalAlloc(ushort flags, ushort bytes)
        {
            // Get the local heap for the current data segment
            var heap = _machine.GlobalHeap.GetLocalHeap(_machine.ds);
            if (heap == null)
                return 0;

            return heap.Alloc(flags, bytes);
        }                      

        [EntryPoint(0x0006)]
        public ushort LocalReAlloc(ushort handle, ushort bytes, ushort flags)
        {
            // Get the local heap for the current data segment
            var heap = _machine.GlobalHeap.GetLocalHeap(_machine.ds);
            if (heap == null)
                return 0;

            return heap.ReAlloc(handle, bytes, flags);
        }

        [EntryPoint(0x0007)]
        public ushort LocalFree(ushort handle)
        {
            // Get the local heap for the current data segment
            var heap = _machine.GlobalHeap.GetLocalHeap(_machine.ds);
            if (heap == null)
                return 0;
                                  
            return heap.Free(handle) ? (ushort)0 : handle;
        }

        [EntryPoint(0x0008)]
        public ushort LocalLock(ushort handle)
        {
            // Get the local heap for the current data segment
            var heap = _machine.GlobalHeap.GetLocalHeap(_machine.ds);
            if (heap == null)
                return 0;

            return heap.Lock(handle);
        }

        // 0009 - LOCALUNLOCK
        [EntryPoint(0x0009)]
        public bool LockUnlock(ushort handle)
        {
            // Get the local heap for the current data segment
            var heap = _machine.GlobalHeap.GetLocalHeap(_machine.ds);
            if (heap == null)
                return false;

            return heap.Unlock(handle);
        }


        [EntryPoint(0x000A)]
        public ushort LocalSize(ushort handle)
        {
            // Get the local heap for the current data segment
            var heap = _machine.GlobalHeap.GetLocalHeap(_machine.ds);
            if (heap == null)
                return 0;

            return heap.Size(handle);
        }

        // 000B - LOCALHANDLE
        // 000C - LOCALFLAGS
        // 000D - LOCALCOMPACT
        // 000E - LOCALNOTIFY

        // 000F - GLOBALALLOC
        [EntryPoint(0x000F)]
        public ushort GlobalAlloc(ushort flags, uint bytes)
        {
            return _machine.GlobalHeap.Alloc("User Alloc", flags, bytes);
        }

        [EntryPoint(0x0010)]
        public ushort GlobalReAlloc(ushort handle, uint bytes, ushort flags)
        {
            return _machine.GlobalHeap.ReAlloc(handle, bytes, flags);
        }

        [EntryPoint(0x0011)]
        public ushort GlobalFree(ushort handle)
        {
            return _machine.GlobalHeap.Free(handle);
        }

        // 0012 - GLOBALLOCK
        [EntryPoint(0x0012)]
        public uint GlobalLock(ushort handle)
        {
            return BitUtils.MakeDWord(0, handle);
        }

        [EntryPoint(0x0013)]
        public bool GlobalUnlock(ushort handle)
        {
            var sel = _machine.GlobalHeap.GetSelector(handle);
            System.Diagnostics.Debug.Assert(sel != null);
            return true;
        }

        [EntryPoint(0x0014)]
        public uint GlobalSize(ushort handle)
        {
            return _machine.GlobalHeap.Size(handle);
        }

        [EntryPoint(0x0015)]
        public uint GlobalHandle(ushort selector)
        {
            var sel = _machine.GlobalHeap.GetSelector(selector);
            if (sel == null)
                return 0;

            System.Diagnostics.Debug.Assert(sel.selector == selector);

            return BitUtils.MakeDWord(selector, selector);
        }

        // 0016 - GLOBALFLAGS

        [EntryPoint(0x0017)]
        public ushort LockSegment(ushort segment)
        {
            return segment;
        }

        [EntryPoint(0x0018)]
        public bool UnlockSegment(ushort segment)
        {
            return true;
        }

        [EntryPoint(0x0019)]
        public uint GlobalCompact(uint dw)
        {
            return _machine.GlobalHeap.GetLargestFreeSpace();
        }

        // 001A - GLOBALFREEALL
        // 001C - GLOBALMASTERHANDLE

        [EntryPoint(0x001d)]
        public void Yield()
        {
            // nop
        }

        // 001E - WAITEVENT
        [EntryPoint(0x001e)]
        public void WaitEvent(ushort unused)
        {
            // NOP
        }

        // 001F - POSTEVENT
        /*
        // http://www.drdobbs.com/windows/inside-the-windows-scheduler/184408818
        [EntryPoint(0x001f, DebugBreak16 = true)]
        public void PostEvent()
        {
        }
        */

        // 0020 - SETPRIORITY
        // 0021 - LOCKCURRENTTASK
        // 0022 - SETTASKQUEUE
        // 0023 - GETTASKQUEUE

        [EntryPoint(0x0024)]
        public ushort GetCurrentTask()
        {
            // See also: WM_ACTIVATEAPP lParam

            // We only ever have one task so we'll just
            // use the process module handle as the task handle
            return _machine.ProcessModule.hModule;
        }

        [EntryPoint(0x0025)]
        public ushort GetCurrentPDB()
        {
            return _machine.Dos.PSP;
        }

        // 0025 - GETCURRENTPDB
        // 0026 - SETTASKSIGNALPROC
        // 0029 - ENABLEDOS
        // 002A - DISABLEDOS
        // 002D - LOADMODULE
        // 002E - FREEMODULE

        [EntryPoint(0x002f)]
        public ushort GetModuleHandle(string moduleName)
        {
            var mod = _machine.ModuleManager.GetModule(moduleName);
            if (mod != null)
                return mod.hModule;

            return 0;
        }

        [EntryPoint(0x0030)]
        public nint GetModuleUsage(ushort module)
        {
            var mod = module == 0 ? _machine.ProcessModule : _machine.ModuleManager.GetModule(module);
            if (mod == null)
                return 0;
            return mod.LoadCount;
        }

        [EntryPoint(0x0031)]
        public ushort GetModuleFileName(ushort module, uint pszPointer, ushort nSize)
        {
            // Get the module
            var mod = module == 0 ? _machine.ProcessModule : _machine.ModuleManager.GetModule(module);

            // Convert to 8.3 file format
            var filename = mod.GetModuleFileName();

            // Get it's path
            return _machine.WriteString(pszPointer.Hiword(), pszPointer.Loword(), filename, nSize);
        }

        // 0032 - GETPROCADDRESS
        [EntryPoint(0x0032)]
        public uint GetProcAddress(ushort module, StringOrId nameOrOrdinal)
        {
            // Get the module
            var mod = module == 0 ? _machine.ProcessModule : _machine.ModuleManager.GetModule(module);
            if (mod == null)
                return 0;

            // Look up by name or ordinal?
            if (nameOrOrdinal.Name != null)
            {
                var ordinal = mod.GetOrdinalFromName(nameOrOrdinal.Name);
                if (ordinal == 0)
                    return 0;
                return mod.GetProcAddress(ordinal);
            }
            else
            {
                return mod.GetProcAddress(nameOrOrdinal.ID);
            }
        }

        [EntryPoint(0x0033)]
        public uint MakeProcInstance(uint proc, ushort hModule)
        {
            var mod = _machine.ModuleManager.GetModule(hModule);

            return _machine.MakeProcInstance((mod as Module16).DataSelector, proc);
        }

        // 0034 - FREEPROCINSTANCE
        [EntryPoint(0x0034)]
        public void FreeProcInstance(uint ptr)
        {
            _machine.FreeProcInstance(ptr);
        }

        // 0035 - CALLPROCINSTANCE
        // 0036 - GETINSTANCEDATA
        // 0037 - CATCH
        // 0038 - THROW

        [EntryPoint(0x0039)]
        public nint GetProfileInt(string lpAppName, string lpKeyName, nint nDefault)
        {
            return GetPrivateProfileInt(lpAppName, lpKeyName, nDefault, "c:\\windows\\win.ini");
        }

        [EntryPoint(0x003a)]
        public nuint GetProfileString(string lpAppName, string lpKeyName, string lpDefault, [BufSize(+1)] [Out] StringBuilder lpReturnedString, nint nSize)
        {
            return GetPrivateProfileString(lpAppName, lpKeyName, lpDefault, lpReturnedString, nSize, "c:\\windows\\win.ini");
        }

        // 003B - WRITEPROFILESTRING
        [EntryPoint(0x003B)]
        public bool WriteProfileString(string lpAppName, string lpKeyName, string lpString)
        {
            return WritePrivateProfileString(lpAppName, lpKeyName, lpString, "c:\\windows\\win.ini");
        }

        class ResourceInfo
        {
            public Module16 module;
            public NeFile.ResourceEntry resourceEntry;
            public string ridType;
            public string ridName;
            public int refCount;
            public ushort hData;
            public ushort hRsrc;
        }

        //Dictionary<string, ushort> _loadedResources = new Dictionary<string, ushort>();
        Dictionary<NeFile.ResourceEntry, ResourceInfo> _resourceEntryMap = new Dictionary<NeFile.ResourceEntry, ResourceInfo>();
        Dictionary<ushort, ResourceInfo> _hrsrcMap = new Dictionary<ushort, ResourceInfo>();
        ushort _nextHRSRC = 0x100;

        [EntryPoint(0x003c)]
        public ushort FindResource(ushort hInstance, StringOrId ridName, StringOrId ridType)
        {
            if (ridType.Name == null)
            {
                if (ridType.ID >= 1 && ridType.ID <= 23)
                {
                    ridType = new StringOrId(((Win16.ResourceType)ridType.ID).ToString());
                }
            }

            // Crack params
            var module = hInstance == 0 ? _machine.ProcessModule : _machine.ModuleManager.GetModule(hInstance) as Module16;
            if (module == null)
                return 0;

            // Find the resource
            var resourceEntry = module.NeFile.FindResource(ridType.ToString(), ridName.ToString());

            // hRsrc already allocated?
            ResourceInfo resInfo;
            if (_resourceEntryMap.TryGetValue(resourceEntry, out resInfo))
            {
                return resInfo.hRsrc;
            }

            // Create a HRSRC
            while (_hrsrcMap.ContainsKey(_nextHRSRC) && _nextHRSRC < 0x100)
                _nextHRSRC++;

            // Create resinfo
            resInfo = new ResourceInfo()
            {
                hRsrc = _nextHRSRC++,
                module = module,
                refCount = 0,
                resourceEntry = resourceEntry,
                ridName = ridName.ToString(),
                ridType = ridType.ToString(),
                hData = 0,
            };

            // Add to map
            _resourceEntryMap.Add(resourceEntry, resInfo);
            _hrsrcMap.Add(resInfo.hRsrc, resInfo);

            // Return pseudo handle
            return resInfo.hRsrc;

            /*
            // Look for already loaded resource
            var existing = _hrsrcInfo.FirstOrDefault(x => x.Value.resourceEntry == resourceEntry);
            if (existing.HasVa)
            {
                return existing.hInstance;
            }

            // Load the data
            var data = module.NeFile.LoadResource(ridType.ToString(), ridName.ToString());
            if (data == null)
                return 0;

            // Map it into address space
            string heapName = string.Format("Module '{0}' (0x{1:X4}) Resource {2} {3}", module.GetModuleName(), hInstance, ridType.ToString(), ridName.ToString());
            hrsrc = _machine.GlobalHeap.Alloc(heapName, 0, (uint)data.Length);
            if (hrsrc == 0)
                return 0;

            Buffer.BlockCopy(data, 0, _machine.GlobalHeap.GetBuffer(hrsrc, true), 0, data.Length);

            // Cache it
            _loadedResources.Add(key, hrsrc);

            // Cache the params used to find it so that 
            _hrsrcInfo[hrsrc] = new CResourceInfo()
            {
                hInstance = hInstance,
                ridType = ridType,
                ridName = ridName,
            };

            var re = module.NeFile.FindResource(ridType.ToString(), ridName.ToString());
            _machine.GlobalHeap.SetFileSource(hrsrc, module.NeFile.FileName, (uint)re.offset);

            // Done
            return hrsrc;
            */
        }

        [EntryPoint(0x003d)]
        public ushort LoadResource(ushort hInstance, ushort hRsrc)
        {
            // Get the resource info
            ResourceInfo resInfo;
            if (!_hrsrcMap.TryGetValue(hRsrc, out resInfo) || resInfo.module.hModule != hInstance)
                return 0;

            // Load it
            if (resInfo.refCount == 0)
            {
                // Load the data
                var data = resInfo.module.NeFile.LoadResource(resInfo.resourceEntry);
                if (data == null)
                    return 0;

                // Map it into address space
                string heapName = string.Format("Module '{0}' (0x{1:X4}) Resource {2} {3}", resInfo.module.GetModuleName(), hInstance, resInfo.ridType, resInfo.ridName);
                resInfo.hData = _machine.GlobalHeap.Alloc(heapName, 0, (uint)data.Length);
                if (resInfo.hData == 0)
                    return 0;
                Buffer.BlockCopy(data, 0, _machine.GlobalHeap.GetBuffer(resInfo.hData, true), 0, data.Length);
            }

            // Bump reference count and return 
            resInfo.refCount++;
            return resInfo.hData;
        }

        [EntryPoint(0x003e)]
        public uint LockResource(ushort hGlobal)
        {
            return GlobalLock(hGlobal);
        }

        [EntryPoint(0x003f)]
        public bool FreeResource(ushort hGlobal)
        {
            // Find the resource entry
            var resourceEntry = _hrsrcMap.Values.FirstOrDefault(x => x.hData == hGlobal);
            if (resourceEntry == null || resourceEntry.refCount == 0)
                return false;

            // Drop reference
            resourceEntry.refCount--;
            if (resourceEntry.refCount != 0)
                return true;

            // Release it
            _machine.GlobalHeap.Free(resourceEntry.hData);
            resourceEntry.hData = 0;

            // Nop...
            return true;
        }

        [EntryPoint(0x0040)]
        public short AccessResource(ushort hInstance, ushort hRsrc)
        {
            // Look up hrsrc
            ResourceInfo resinfo;
            if (!_hrsrcMap.TryGetValue(hRsrc, out resinfo) || resinfo.module.hModule != hInstance)
                return 0;

            // Get the module
            var module = hInstance == 0 ? _machine.ProcessModule : _machine.ModuleManager.GetModule(hInstance) as Module16;
            if (module == null)
                return 0;

            // Open the file
            short hFile = _lopen(module.GetModuleFileName(), (short)(DosApi.FileAccessMode.Read | DosApi.FileAccessMode.ShareDenyNone));
            if (hFile < 0)
                return -1;

            // Seek to location
            _llseek((ushort)hFile, resinfo.resourceEntry.offset, (short)SeekOrigin.Begin);

            // Return it;
            return hFile;
        }

        [EntryPoint(0x0041)]
        public ushort SizeofResource(ushort hInstance, ushort hRsrc)
        {
            // Get the resource info
            ResourceInfo resInfo;
            if (!_hrsrcMap.TryGetValue(hRsrc, out resInfo) || resInfo.module.hModule != hInstance)
                return 0;

            return (ushort)resInfo.resourceEntry.length;
        }

        // 0042 - ALLOCRESOURCE
        // 0043 - SETRESOURCEHANDLER
        // 0044 - INITATOMTABLE
        // 0045 - FINDATOM
        // 0046 - ADDATOM
        // 0047 - DELETEATOM
        // 0048 - GETATOMNAME
        // 0049 - GETATOMHANDLE

        [EntryPoint(0x004a)]
        public short OpenFile(string filename, ref Win16.OFSTRUCT reopenBuf, ushort style)
        {
            try
            {
                // Use filename?
                if ((style & Win16.OF_REOPEN) == 0)
                {
                    // Get the fully qualified path
                    reopenBuf.cBytes = (byte)Marshal.SizeOf<Win16.OFSTRUCT>();
                    reopenBuf.fFixedDisk = 1;
                    reopenBuf.szPathName = _machine.Dos.QualifyPath(filename);
                }

                // Parse only?
                if ((style & Win16.OF_PARSE) != 0)
                    return 0;

                // Delete
                if ((style & Win16.OF_DELETE) != 0)
                {
                    _machine.Dos.DeleteFile(reopenBuf.szPathName);
                    return 0;
                }

                // Create/Open file
                FileMode mode = (style & Win16.OF_CREATE) != 0 ? FileMode.Create : FileMode.Open;
                FileAccess access = (FileAccess)((style & 3) + 1);
                FileShare share;
                switch (style & 0x00F0)
                {
                    case Win16.OF_SHARE_EXCLUSIVE:
                        share = FileShare.None;
                        break;

                    case Win16.OF_SHARE_DENY_WRITE:
                        share = FileShare.Read;
                        break;
                    case Win16.OF_SHARE_DENY_READ:
                        share = FileShare.Write;
                        break;

                    default:
                        share = FileShare.ReadWrite;
                        break;
                }

                if (mode == FileMode.Create || mode == FileMode.CreateNew)
                {
                    access = FileAccess.ReadWrite;
                }

                // Open it
                var file = _machine.Dos.OpenFileHandle(reopenBuf.szPathName, mode, access, share);

                // Just testing?
                if ((style & Win16.OF_EXIST) != 0)
                {
                    _machine.Dos.CloseFile(file.handle);
                    return 0;
                }

                return (short)file.handle;

            }
            catch (DosError e)
            {
                reopenBuf.nErrCode = e.errorCode;
                return -1;
            }
        }

        // 004B - OPENPATHNAME
        // 004C - DELETEPATHNAME
        // 004D - RESERVED1
        // 004E - RESERVED2
        // 004F - RESERVED3
        // 0050 - RESERVED4

        [EntryPoint(0x0051)]
        public short _lclose(ushort hFile)
        {
            try
            {
                _machine.Dos.CloseFile(hFile);
                return 0;
            }
            catch (DosError)
            {
                return -1;
            }
        }

        [EntryPoint(0x0052)]
        public short _lread(ushort hFile, uint ptr, ushort bytes)
        {
            try
            {
                return (short)_machine.Dos.ReadFile(hFile, bytes, ptr.Hiword(), ptr.Loword());
            }
            catch (DosError)
            {
                return -1;
            }
        }

        [EntryPoint(0x0053)]
        public short _lcreate(string filename, short attributes)
        {
            try
            {
                // Map attributes
                byte fa = 0;
                switch (attributes)
                {
                    case 1: fa = DosApi.DosFileAttributes.ReadOnly; break;
                    case 2: fa = DosApi.DosFileAttributes.Hidden; break;
                    case 3: fa = DosApi.DosFileAttributes.System; break;
                }

                return (short)_machine.Dos.CreateFile(filename, (ushort)fa);
            }
            catch (DosError)
            {
                return -1;
            }
        }

        [EntryPoint(0x0054)]
        public int _llseek(ushort hFile, int offset, short origin)
        {
            try
            {
                return _machine.Dos.SeekFile(hFile, offset, origin);
            }
            catch (DosError)
            {
                return -1;
            }
        }

        [EntryPoint(0x0055)]
        public short _lopen(string filename, short readWrite)
        {
            try
            {
                return (short)_machine.Dos.OpenFile(filename, (byte)readWrite);
            }
            catch (DosError)
            {
                return -1;
            }
        }

        [EntryPoint(0x0056)]
        public short _lwrite(ushort hFile, uint ptr, ushort bytes)
        {
            try
            {
                return (short)_machine.Dos.WriteFile(hFile, bytes, ptr.Hiword(), ptr.Loword());
            }
            catch (DosError)
            {
                return -1;
            }
        }

        // 0057 - RESERVED5

        [EntryPoint(0x0058)]
        public uint lstrcpy(uint destPtr, string srcPtr)
        {
            _machine.WriteString(destPtr, srcPtr, 0xFFFF);
            return destPtr;
        }

        [EntryPoint(0x0059)]
        public uint lstrcat(uint destPtr, string srcPtr)
        {
            return lstrcpy((uint)(destPtr + lstrlen(destPtr)), srcPtr);
        }

        [EntryPoint(0x005a)]
        public nint lstrlen(uint srcPtr)
        {
            ushort seg = srcPtr.Hiword();
            ushort off = srcPtr.Loword();
            ushort p = off;
            while (_machine.ReadByte(seg, p) != 0)
                p++;

            return p - off; 
        }

        [EntryPoint(0x005B)]
        public ushort InitTask()
        {
            // Get the module
            var module = _machine.ModuleManager.GetModule(_machine.di) as Module16;
            if (module == null)
                return 0;

            return module.InitTask(_machine);
        }


        // 005C - GETTEMPDRIVE
        // 005D - GETCODEHANDLE
        // 005E - DEFINEHANDLETABLE

        [EntryPoint(0x005F)]
        public ushort LoadLibrary(string strDllFile)
        {
            var module = _machine.ModuleManager.LoadModule(strDllFile);
            if (module == null)
                return 0;

            return module.hModule;
        }

        // 0060 - FREELIBRARY
        [EntryPoint(0x0060)]
        public void FreeLibrary(ushort hInstance)
        {
            // Get the module
            var module = _machine.ModuleManager.GetModule(hInstance);
            if (module == null)
                return;

            // Free it
            _machine.ModuleManager.UnloadModule(module);
        }

        // 0061 - GETTEMPFILENAME
        // 0062 - GETLASTDISKCHANGE
        // 0063 - GETLPERRMODE
        // 0064 - VALIDATECODESEGMENTS
        // 0065 - NOHOOKDOSCALL

        [EntryPoint(0x0066, PreserveAX = true)]
        public void Dos3Call()
        {
            _machine.Dos.DispatchInt21();
        }

        // 0067 - NETBIOSCALL
        // 0068 - GETCODEINFO
        // 0069 - GETEXEVERSION
        // 006A - SETSWAPAREASIZE

        [EntryPoint(0x006b)]
        public nuint SetErrorMode(nuint mode)
        {
            return 0;
        }

        // 006C - SWITCHSTACKTO
        // 006D - SWITCHSTACKBACK
        // 006E - PATCHCODEHANDLE
        // 006F - GLOBALWIRE
        // 0070 - GLOBALUNWIRE

        [EntryPoint(0x0073)]
        [DllImport("kernel32.dll")]
        public static extern void OutputDebugString(string str);

        // 0074 - INITLIB
        // 0075 - OLDYIELD
        // 0076 - GETTASKQUEUEDS
        // 0077 - GETTASKQUEUEES
        // 0078 - UNDEFDYNLINK

        [EntryPoint(0x0079)]
        public ushort LocalShrink(ushort hSeg, ushort newSize)
        {
            var localHeap = _machine.GlobalHeap.GetLocalHeap(hSeg);
            if (localHeap == null)
                return 0;

            return localHeap.HeapSize();
        }

        // 007A - ISTASKLOCKED
        // 007B - KBDRST
        // 007C - ENABLEKERNEL
        // 007D - DISABLEKERNEL
        // 007E - MEMORYFREED

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetPrivateProfileIntW")]
        public static extern int _GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        [EntryPoint(0x007F)]
        public nint GetPrivateProfileInt(string lpAppName, string lpKeyName, nint nDefault, string lpFileName)
        {
            // Check for unqualified path and map to windows folder
            if (!lpFileName.Contains('\\') && !lpFileName.Contains(':'))
                lpFileName = @"C:\WINDOWS\" + lpFileName;

            // Map it
            lpFileName = _machine.PathMapper.TryMapGuestToHost(lpFileName, false);
            if (lpFileName == null)
                return nDefault;

            // Really read it
            return _GetPrivateProfileInt(lpAppName, lpKeyName, nDefault, lpFileName);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetPrivateProfileStringW")]
        public static extern uint _GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, [BufSize(+1)] [Out] StringBuilder lpReturnedString, int nSize, string lpFileName);

        [EntryPoint(0x0080)]
        public nuint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, [BufSize(+1)] [Out] StringBuilder lpReturnedString, nint nSize, string lpFileName)
        {
            // Check for unqualified path and map to windows folder
            if (!lpFileName.Contains('\\') && !lpFileName.Contains(':'))
                lpFileName = @"C:\WINDOWS\" + lpFileName;

            // Map it
            lpFileName = _machine.PathMapper.TryMapGuestToHost(lpFileName, false);
            if (lpFileName == null)
            {
                lpReturnedString.Append(lpDefault);
                return (uint)lpDefault.Length;
            }

            // Really read it
            return _GetPrivateProfileString(lpAppName, lpKeyName, lpDefault, lpReturnedString, nSize, lpFileName);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WritePrivateProfileStringW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool _WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [EntryPoint(0x0081)]
        public bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName)
        {
            // Check for unqualified path and map to windows folder
            if (!lpFileName.Contains('\\') && !lpFileName.Contains(':'))
                lpFileName = @"C:\WINDOWS\" + lpFileName;

            // Map it
            lpFileName = _machine.PathMapper.TryMapGuestToHost(lpFileName, true);
            if (lpFileName == null)
            {
                return false;
            }

            // Really read it
            return _WritePrivateProfileString(lpAppName, lpKeyName, lpString, lpFileName);
        }

        // 0082 - FILECDR


        [EntryPoint(0x0083)]
        public uint GetDosEnvironment()
        {
            return BitUtils.MakeDWord(0, _machine.GetDosEnvironmentSegment());
        }

        // 0084 - GETWINFLAGS
        [EntryPoint(0x0084)]
        public uint GetWinFlags()
        {
            return GetProcAddress(0x00B2) & 0x0000FFFF;
        }

        // 0085 - GETEXEPTR

        [EntryPoint(0x0086)]
        public static nint GetWindowDirectory([Out] [BufSize(+1)] StringBuilder sb, nint nSize)
        {
            sb.Append("C:\\WINDOWS");
            return sb.Length;
        }


        // 0087 - GETSYSTEMDIRECTORY

        [EntryPoint(0x0088)]
        public ushort GetDriveType(short drive)
        {
            return (ushort)_machine.Dos.GetDriveType(drive);
        }

        [EntryPoint(0x0089)]
        public void FatalAppExit(ushort reserved, string messageText)
        {
            throw new NotImplementedException();
        }

        // 008A - GETHEAPSPACES
        // 008B - DOSIGNAL
        // 008C - SETSIGHANDLER
        // 008D - INITTASK1
        // 0096 - DIRECTEDYIELD
        // 0097 - WINOLDAPCALL
        // 0098 - GETNUMTASKS
        // 009A - GLOBALNOTIFY
        // 009B - GETTASKDS
        // 009C - LIMITEMSPAGES
        // 009D - GETCURPID
        // 009E - ISWINOLDAPTASK
        // 009F - GLOBALHANDLENORIP
        // 00A0 - EMSCOPY
        // 00A1 - LOCALCOUNTFREE
        // 00A2 - LOCALHEAPSIZE
        // 00A3 - GLOBALLRUOLDEST
        // 00A4 - GLOBALLRUNEWEST
        // 00A5 - A20PROC

        [EntryPoint(0x00a6)]
        public ushort WinExec(string cmd, ushort show)
        {
            User.MessageBox(User.GetActiveWindow(), string.Format("Application tried to launch:\n\n'{0}'\n\nNot currently supported", cmd), "Win3mu", 0x10);
            return 0;
        }

        // 00A7 - GETEXPWINVER
        // 00A8 - DIRECTRESALLOC

        [EntryPoint(0x00a9)]
        public uint GetFreeSpace(ushort flags)
        {
            return _machine.GlobalHeap.GetFreeSpace();
        }

        // 00AA - ALLOCCSTODSALIAS

        [EntryPoint(0x00AA)]
        public ushort AllocCStoDSAlias(ushort wSelector)
        {
            var sel = _machine.GlobalHeap.GetSelector(wSelector);
            if (sel == null || sel.allocation == null)
                return 0;
            if (!sel.isCode)
                return 0;

            var newSel = _machine.GlobalHeap.AllocSelector(string.Format("CS to DS alias for 0x{0:X4}", wSelector), 1);
            newSel.allocation = sel.allocation;
            newSel.isCode = false;
            newSel.readOnly = false;

            return newSel.selector;
        }

        // 00AB - ALLOCDSTOCSALIAS
        // 00AC - ALLOCALIAS
        // 00AD - __ROMBIOS
        // 00AE - __A000H

        [EntryPoint(0x00AF)]
        public ushort AllocSelector(ushort src)
        {
            return _machine.GlobalHeap.FreeSelector(src);
        }

        [EntryPoint(0x00B0)]
        public ushort FreeSelector(ushort sel)
        {
            return _machine.GlobalHeap.FreeSelector(sel);
        }

        [EntryPoint(0x00B1)]
        public ushort PrestoChangoSelector(ushort dest, ushort src)
        {
            // Get both selectors
            var selSrc = _machine.GlobalHeap.GetSelector(src);
            var selDest = _machine.GlobalHeap.GetSelector(dest);

            // Make sure both valid
            if (selSrc == null || selDest == null)
                return 0;
            if (selSrc.allocation == null || selDest.allocation == null)
                return 0;

            // Presto Chango
            selDest.isCode = !selSrc.isCode;
            selDest.readOnly = !selSrc.isCode;

            // Return the new selector
            return selDest.selector;
        }
        
        // 00B2 - __WINFLAGS


        // 00B3 - __D000H
        // 00B4 - LONGPTRADD
        // 00B5 - __B000H
        // 00B6 - __B800H
        // 00B7 - __0000H
        // 00B8 - GLOBALDOSALLOC
        // 00B9 - GLOBALDOSFREE
        // 00BA - GETSELECTORBASE
        // 00BB - SETSELECTORBASE
        // 00BC - GETSELECTORLIMIT
        // 00BD - SETSELECTORLIMIT
        // 00BE - __E000H
        // 00BF - GLOBALPAGELOCK
        // 00C0 - GLOBALPAGEUNLOCK
        // 00C1 - __0040H
        // 00C2 - __F000H
        // 00C3 - __C000H
        // 00C4 - SELECTORACCESSRIGHTS
        // 00C5 - GLOBALFIX
        // 00C6 - GLOBALUNFIX
        // 00C7 - SETHANDLECOUNT
        // 00C8 - VALIDATEFREESPACES
        // 00C9 - REPLACEINST
        // 00CA - REGISTERPTRACE
        // 00CB - DEBUGBREAK
        // 00CC - SWAPRECORDING
        // 00CD - CVWBREAK
        // 00CE - ALLOCSELECTORARRAY
        // 00CF - ISDBCSLEADBYTE
        // 0136 - LOCALHANDLEDELTA
        // 0137 - GETSETKERNELDOSPROC
        // 013A - DEBUGDEFINESEGMENT
        // 013B - WRITEOUTPROFILES
        // 013C - GETFREEMEMINFO
        // 013E - FATALEXITHOOK
        // 013F - FLUSHCACHEDFILEHANDLE
        // 0140 - ISTASK
        // 0143 - ISROMMODULE
        // 0144 - LOGERROR
        // 0145 - LOGPARAMERROR
        // 0146 - ISROMFILE
        // 0147 - K327
        // 0148 - _DEBUGOUTPUT
        // 0149 - K329
        // 014C - THHOOK
        // 014E - ISBADREADPTR
        // 014F - ISBADWRITEPTR
        // 0150 - ISBADCODEPTR
        // 0151 - ISBADSTRINGPTR
        // 0152 - HASGPHANDLER
        // 0153 - DIAGQUERY
        // 0154 - DIAGOUTPUT
        // 0155 - TOOLHELPHOOK
        // 0157 - REGISTERWINOLDAPHOOK
        // 0158 - GETWINOLDAPHOOKS
        // 0159 - ISSHAREDSELECTOR
        // 015A - ISBADHUGEREADPTR
        // 015B - ISBADHUGEWRITEPTR
        // 015C - HMEMCPY
        // 015D - _HREAD
        // 015E - _HWRITE
        // 015F - BUNNY_351
        // 0161 - LSTRCPYN
        // 0162 - GETAPPCOMPATFLAGS
        // 0163 - GETWINDEBUGINFO
        // 0164 - SETWINDEBUGINFO
        // 0193 - K403
        // 0194 - K404
    }
}
