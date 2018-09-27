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
using Win3muCore.MessageSemantics;

namespace Win3muCore
{
    public class Messaging
    {
        public Messaging(Machine machine)
        {
            _machine = machine;

            // HWND_BROADCAST Mapping
            HWND.Map.DefineMapping((IntPtr)(0xFFFF), 0xFFFF);
        }

        static Messaging()
        {
        }
            

        Machine _machine;

        #region Message Classes and Semantics
        
        MessageSemantics.MessageMap _map = new MessageSemantics.MessageMap();

        MessageSemantics.Base TryGetMessageSemantics16(IntPtr hWnd32, ushort message16, out uint message32)
        {
            return _map.LookupMessage16(hWnd32, message16, out message32);
        }

        MessageSemantics.Base TryGetMessageSemantics32(IntPtr hWnd32, uint message32, out ushort message16)
        {
            return _map.LookupMessage32(hWnd32, message32, out message16);
        }

        /*
        void ThrowMessageError(IntPtr hWnd32, uint message)
        {
            throw new VirtualException($"Unknown windows message {MessageNames.NameOfMessage(message)} for window class '{User.GetClassName(hWnd32)}' ({WindowClassKind.Get(hWnd32)})");
        }
        */

        #endregion

        #region WNDPROC Mapping
        public ProcMap<Win32.WNDPROC> WndProcMap = new ProcMap<Win32.WNDPROC>();

        /*
        public void ConnectWndProcs(IntPtr wndProc32, uint wndProc16)
        {
            _wndProc16Map.Add(wndProc16, wndProc32);
            _wndProc32Map.Add(wndProc32, wndProc16);
        }

        public IntPtr ConnectWndProcs(Win32.WNDPROC wndProc32, uint wndProc16)
        {
            IntPtr ptr = Marshal.GetFunctionPointerForDelegate(wndProc32);
            _wndProc16Map.Add(wndProc16, ptr);
            _wndProc16MapManaged.Add(wndProc16, wndProc32);
            _wndProc32Map.Add(ptr, wndProc16);
            return ptr;
        }

    */

        // Wrap a 16-bit virtual window procedure in a managed delegate that
        // when invoked will call the virtual proc.  
        public IntPtr GetWndProc32(uint lpfnWndProc, bool dlgProc)
        {
            if (lpfnWndProc == 0)
                return IntPtr.Zero;

            // Check if already wrapped
            IntPtr wndProc32 = WndProcMap.To32(lpfnWndProc);
            if (wndProc32!=IntPtr.Zero)
                return wndProc32;

            // Nope, create one
            Win32.WNDPROC wndProc32Managed = (hWnd, message, wParam, lParam) =>
            {
                return CallWndProc16from32(lpfnWndProc, hWnd, message, wParam, lParam, dlgProc);
            };

            // Connect
            return WndProcMap.Connect(wndProc32Managed, lpfnWndProc);
        }

        public uint GetWndProc16(IntPtr lpfnWndProc32, bool create = true)
        {
            if (lpfnWndProc32 == IntPtr.Zero)
                return 0;

            // Already wrapped?
            uint wndProc16 = WndProcMap.To16(lpfnWndProc32);
            if (wndProc16 != 0)
                return wndProc16;

            if (!create)
                return 0;

            // Need to create a 16-bit thunk that calls it
            wndProc16 = _machine.CreateSystemThunk(() =>
            {
                ushort hWnd16 = _machine.ReadWord(_machine.ss, (ushort)(_machine.sp + 12));
                ushort message16 = _machine.ReadWord(_machine.ss, (ushort)(_machine.sp + 10));
                ushort wParam16 = _machine.ReadWord(_machine.ss, (ushort)(_machine.sp + 8));
                uint lParam16 = _machine.ReadDWord(_machine.ss, (ushort)(_machine.sp + 4));

                _machine.dxax = CallWndProc32from16(lpfnWndProc32, hWnd16, message16, wParam16, lParam16);

            }, 10, false, "WndProc32");      // hWnd = 2, msg = 2, wParam = 2, lParam = 4

            // Update maps
            WndProcMap.Connect(lpfnWndProc32, wndProc16);

            // Done
            return wndProc16;
        }
        #endregion

        #region Bypass Messaging

        class BypassMessage
        {
            public uint id;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public bool bypassed;
            public IntPtr retVal;
        }

        Dictionary<uint, BypassMessage> _activeBypassMessages = new Dictionary<uint, BypassMessage>();
        uint _nextBypassMessageID = 0;

        BypassMessage AllocBypassMessage(uint message, IntPtr wParam, IntPtr lParam)
        {
            // Grab an id
            _nextBypassMessageID++;

            // Allocate info
            var bpm = new BypassMessage()
            {
                id = _nextBypassMessageID,
                message = message,
                wParam = wParam,
                lParam = lParam,
            };

            // Store it
            _activeBypassMessages.Add(bpm.id, bpm);
            return bpm;
        }

        BypassMessage RetrieveBypassMessage(uint id)
        {
            // Get it
            BypassMessage bpm;
            if (_activeBypassMessages.TryGetValue(id, out bpm))
            {
                // Check not already bypassed
                System.Diagnostics.Debug.Assert(!bpm.bypassed);

                // Mark as processed
                bpm.bypassed = true;

                // Return it (called to setup return value)
                return bpm;
            }

            // Hey, what happened?
            System.Diagnostics.Debug.Assert(false);
            return null;
        }

        IntPtr FreeBypassMessage(BypassMessage bpm)
        {
            // Check it made it through
            System.Diagnostics.Debug.Assert(bpm.bypassed);

            // Remove from map
            _activeBypassMessages.Remove(bpm.id);

            // Return result
            return bpm.retVal;
        }

        #endregion

        #region Postable Message Conversion

        // Postable messages are any message that don't contain pointers and can be fully
        // represented by wParam and lParam

        public bool ConvertPostableMessageTo32(ref Win16.MSG msg16, out Win32.MSG msg32)
        {
            // Setup basic message
            msg32 = new Win32.MSG()
            {
                hWnd = HWND.Map.To32(msg16.hWnd),
                message = (ushort)msg16.message,
                time = msg16.time,
                p = msg16.pt.Convert(),
            };

            // Get message semantics
            var sem = TryGetMessageSemantics16(msg32.hWnd, msg16.message, out msg32.message);

            // Postable message?
            var postable = sem as MessageSemantics.Postable;
            if (postable != null)
            {
                postable.To32(_machine, ref msg16, ref msg32);
                return true;
            }

            MessageMap.ThrowMessageError(msg32.hWnd, msg32.message);
            return false;
        }

        public bool ConvertPostableMessageTo16(ref Win32.MSG msg32, out Win16.MSG msg16)
        {
            // Setup basic message
            msg16 = new Win16.MSG()
            {
                message = (ushort)msg32.message,
                hWnd = HWND.Map.To16(msg32.hWnd),
                time = msg32.time,
                pt = msg32.p.Convert(),
            };

            // Get message semantics
            var sem = TryGetMessageSemantics32(msg32.hWnd, msg32.message, out msg16.message);

            // If it should be bypassed, just return false
            if (sem!=null && sem.ShouldBypass(_machine, ref msg32))
                return false;

            // Postable message?
            var postable = sem as MessageSemantics.Postable;
            if (postable != null)
            {
                postable.To16(_machine, ref msg32, ref msg16);
                return true;
            }

            MessageMap.ThrowMessageError(msg32.hWnd, msg32.message);
            return false;
        }

        #endregion

        #region WNDPROC Calling

        public uint CallWndProc32from16(IntPtr pfnProc32, ushort hWnd, ushort message16, ushort wParam16, uint lParam16)
        {
            return CallWndProc32from16((hwnd32, message32, wParam32, lParam32) =>
            {

                return User.CallWindowProc(pfnProc32, hwnd32, message32, wParam32, lParam32);

            }, hWnd, message16, wParam16, lParam16);
       }

        public uint CallWndProc32from16(Win32.WNDPROC pfnProc, ushort hWnd, ushort message16, ushort wParam, uint lParam)
        {
            if (message16 == Win16.WM_WIN3MU_BYPASS16)
            {
                var bpm = RetrieveBypassMessage(lParam);
                bpm.retVal = pfnProc(HWND.Map.To32(hWnd), bpm.message, bpm.wParam, bpm.lParam);
                return 0;
            }

            uint message32;
            var sem = TryGetMessageSemantics16(HWND.Map.To32(hWnd), message16, out message32);

            var msg32 = new Win32.MSG()
            {
                hWnd = HWND.Map.To32(hWnd),
                message = message32,
            };

            var msg16 = new Win16.MSG()
            {
                hWnd = hWnd,
                message = message16,
                wParam = wParam,
                lParam = lParam,
            };

            var postable = sem as MessageSemantics.Postable;
            if (postable!= null)
            {
                // Convert it
                postable.To32(_machine, ref msg16, ref msg32);

                // Call it
                return (uint)pfnProc(msg32.hWnd, msg32.message, msg32.wParam, msg32.lParam);
            }

            // Callable?
            var callable = sem as MessageSemantics.Callable;
            if (callable != null)
            {
                return callable.Call32from16(_machine, false, false, ref msg16, ref msg32, () =>
                {
                    return pfnProc(msg32.hWnd, msg32.message, msg32.wParam, msg32.lParam);
                });
            }
            
            // Unsupported
            MessageMap.ThrowMessageError(HWND.Map.To32(hWnd), message16);
            return 0;
        }

        List<IWndProcFilter> _wndProcFilters = new List<IWndProcFilter>();

        public void RegisterFilter(IWndProcFilter filter)
        {
            _wndProcFilters.Add(filter);
        }

        public void RevokeFilter(IWndProcFilter filter)
        {
            _wndProcFilters.Remove(filter);
        }

        public IntPtr CallWndProc16from32(uint pfnProc, IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam, bool dlgProc)
        {
            // Package it
            var msg32 = new Win32.MSG()
            {
                hWnd = hWnd,
                message = message,
                wParam = wParam,
                lParam = lParam,
            };

            if (_machine.IsStoppedInDebugger)
            {
                if (dlgProc)
                    return IntPtr.Zero;
                return User.DefWindowProc(hWnd, message, wParam, lParam);
            }

            // Call filters
            for (int i =0; i<_wndProcFilters.Count; i++)
            {
                var f = _wndProcFilters[i];
                var retv = f.PreWndProc(pfnProc, ref msg32, dlgProc);
                if (retv.HasValue)
                    return retv.Value;
            }

            // Log messages?
            if (_machine.logMessages)
            {
                Log.WriteLine("Message: [{4}] hWnd: {0} message:{1} wParam:{2} lParam:{3}", (HWND)hWnd, MessageNames.NameOfMessage(message), wParam, lParam, User.GetTickCount());
            }

            ushort message16;
            var sem = TryGetMessageSemantics32(hWnd, message, out message16);

            var msg16 = new Win16.MSG()
            {
                hWnd = HWND.Map.To16(hWnd),
                message = message16,
            };


            if (sem!=null && sem.ShouldBypass(_machine, ref msg32))
            {
                if (dlgProc)
                    return IntPtr.Zero;

                // Unsupported message - sneak it through
                var bpm = AllocBypassMessage(message, wParam, lParam);
                var ret = _machine.CallWndProc16(pfnProc, HWND.Map.To16(hWnd), Win16.WM_WIN3MU_BYPASS16, 0, bpm.id);
                return FreeBypassMessage(bpm);
            }

            var postable = sem as MessageSemantics.Postable;
            if (postable != null)
            {
                // Convert it
                postable.To16(_machine, ref msg32, ref msg16);

                // Call it
                var x = _machine.CallWndProc16(pfnProc, msg16.hWnd, msg16.message, msg16.wParam, msg16.lParam);

                if (dlgProc)
                    x = x.Loword();

                return BitUtils.DWordToIntPtr(x);
            }

            // Callable?
            var callable = sem as MessageSemantics.Callable;
            if (callable != null)
            {
                var x = callable.Call16from32(_machine, false, dlgProc, ref msg32, ref msg16, () =>
                {
                    return _machine.CallWndProc16(pfnProc, msg16.hWnd, msg16.message, msg16.wParam, msg16.lParam);
                });
                if (dlgProc)
                {
                    x = (IntPtr)(x.ToInt32().Loword());

                    // If not handled by dialog proc and ctlcolor message, switch to white
                    if (x == IntPtr.Zero)
                    {
                        bool recolor = false;
                        switch (message)
                        {
                            case Win32.WM_CTLCOLORDLG:
                            case Win32.WM_CTLCOLORSTATIC:
                            case Win32.WM_CTLCOLORBTN:
                                recolor = true;
                                break;
                        }

                        if (recolor)
                        {
                            Gdi.SetTextColor(wParam, User._GetSysColor(Win32.COLOR_WINDOWTEXT));
                            x = User.GetSysColorBrush(Win32.COLOR_WINDOW);
                        }
                    }

                }
                return x;
            }

            MessageMap.ThrowMessageError(hWnd, message);
            throw new NotImplementedException();
        }

        public void Convert32to16(ref Win32.MSG msg32, Action<Win16.MSG> callback)
        {
            if (_machine.IsStoppedInDebugger)
                return;

            ushort message16;
            var sem = TryGetMessageSemantics32(msg32.hWnd, msg32.message, out message16);

            var msg16 = new Win16.MSG()
            {
                hWnd = HWND.Map.To16(msg32.hWnd),
                message = message16,
            };


            if (sem!=null && sem.ShouldBypass(_machine, ref msg32))
            {
                return;
            }

            var postable = sem as MessageSemantics.Postable;
            if (postable != null)
            {
                // Convert it
                postable.To16(_machine, ref msg32, ref msg16);
                callback(msg16);
                return;
            }

            // Callable?
            var callable = sem as MessageSemantics.Callable;
            if (callable != null)
            {
                callable.Call16from32(_machine, true, false, ref msg32, ref msg16, () =>
                {
                    callback(msg16);
                    return 0;
                });
                return;
            }

            MessageMap.ThrowMessageError(msg32.hWnd, msg32.message);
            throw new NotImplementedException();
        }

        public void Convert16to32(ref Win16.MSG msg16, Action<Win32.MSG> callback)
        {
            if (msg16.message == Win16.WM_WIN3MU_BYPASS16)
            {
                return;
            }

            var msg32 = new Win32.MSG()
            {
                hWnd = HWND.Map.To32(msg16.hWnd),
            };

            var sem = TryGetMessageSemantics16(msg32.hWnd, msg16.message, out msg32.message);

            var postable = sem as MessageSemantics.Postable;
            if (postable != null)
            {
                // Convert it
                postable.To32(_machine, ref msg16, ref msg32);

                // Call it
                callback(msg32);
                return;
            }

            // Callable?
            var callable = sem as MessageSemantics.Callable;
            if (callable != null)
            {
                callable.Call32from16(_machine, false, false, ref msg16, ref msg32, () =>
                {
                    callback(msg32);
                    return IntPtr.Zero;
                });
                return;
            }

            // Unsupported
            MessageMap.ThrowMessageError(msg32.hWnd, msg16.message);
        }

        #endregion

        #region Timer Procs

        class TimerProc
        {
            public TimerProc(Messaging owner, uint proc16)
            {
                this.owner = owner;
                this.refCount = 1;
                this.proc16 = proc16;
                _keepAlive = TimerProc32;
                this.proc32 = Marshal.GetFunctionPointerForDelegate(_keepAlive);
            }

            public void TimerProc32(IntPtr hWnd, uint uMsg, IntPtr nIDEvent, uint dwTime)
            {
                // Not if stopped in debugger
                if (owner._machine.IsStoppedInDebugger)
                    return;

                // NB: Convenient that WNDPROC takes same params as TIMERPROC
                owner._machine.CallWndProc16(proc16, HWND.Map.To16(hWnd), (ushort)uMsg, (ushort)nIDEvent.Loword(), dwTime);
            }

            Messaging owner;
            Win32.TIMERPROC _keepAlive;
            public IntPtr proc32;
            public uint proc16;
            public uint refCount;
        }

        Dictionary<uint, TimerProc> _timerProcs16 = new Dictionary<uint, TimerProc>();
        Dictionary<IntPtr, TimerProc> _timerProcs32 = new Dictionary<IntPtr, TimerProc>();

        public IntPtr GetTimerProc(uint pfn16)
        {
            if (pfn16 == 0)
                return IntPtr.Zero;
            TimerProc tp;
            if (_timerProcs16.TryGetValue(pfn16, out tp))
                return tp.proc32;
            else
                return IntPtr.Zero;
        }

        public uint GetTimerProc(IntPtr pfn32)
        {
            if (pfn32 == IntPtr.Zero)
                return 0;
            TimerProc tp;
            if (_timerProcs32.TryGetValue(pfn32, out tp))
                return tp.proc16;
            else
                return 0;
        }


        public IntPtr RegisterTimerProc(uint proc16)
        {
            if (proc16 == 0)
                return IntPtr.Zero;

            TimerProc tp;
            if (_timerProcs16.TryGetValue(proc16, out tp))
            {
                tp.refCount++;
                return tp.proc32;
            }

            tp = new TimerProc(this, proc16);
            _timerProcs16.Add(proc16, tp);
            _timerProcs32.Add(tp.proc32, tp);
            return tp.proc32;
        }

        #endregion
    }
}
