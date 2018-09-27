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
    [Module("MMSYSTEM", @"C:\WINDOWS\SYSTEM\MMSYSTEM.DLL")]
    public class MMSystem : Module32
    {
        public string ResolveMediaFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return filename;

            // If plain filename, look in same folder as exe
            if (!filename.Contains('\\'))
            {
                var exeHostPath = System.IO.Path.GetDirectoryName(_machine.ProgramHostPath);
                var testFile = System.IO.Path.Combine(exeHostPath, filename);
                if (System.IO.File.Exists(testFile))
                    return testFile;
            }

            // Qualify path using current directory
            var fullPath = _machine.Dos.QualifyPath(filename);

            // Try to map to host path
            var hostPath = _machine.PathMapper.TryMapGuestToHost(fullPath, false);

            // If couldn't use filename as is
            if (hostPath == null)
                return filename;

            // If file doesn't exist, use filename as is
            if (!System.IO.File.Exists(hostPath))
                return filename;

            return hostPath;
        }


        // 0001 - WEP - 0001

        [DllImport("winmm.dll")]
        public static extern bool sndPlaySound(IntPtr ptr, uint flags);
        [DllImport("winmm.dll")]
        public static extern bool sndPlaySound(string str, uint flags);

        [EntryPoint(0x0002)]
        public bool sndPlaySound(uint pszSound, ushort flags)
        {
            if ((flags & 0x0004) != 0) // SND_MEMORY
            {
                // Very conveniently, Windows 7 copies up to 2mb wave file to its own memory block
                // So ok to pass fixed ptr and unfix immediately
                // See:  https://blogs.msdn.microsoft.com/larryosterman/2009/06/24/windows-7-fixes-the-playsoundxxx-snd_memorysnd_async-anti-pattern/
                using (var hp = _machine.GlobalHeap.GetHeapPointer(pszSound, false))
                {
                    return sndPlaySound(hp, flags);
                }
            }
            else
            {
                var str = ResolveMediaFile(_machine.ReadString(pszSound));
                return sndPlaySound(str, flags);
            }
        }




        // 0005 - MMSYSTEMGETVERSION - 0005
        // 0006 - DRIVERPROC - 0006
        // 001E - OUTPUTDEBUGSTR - 001E
        // 001F - DRIVERCALLBACK - 001F
        // 0020 - STACKENTER - 0020
        // 0021 - STACKLEAVE - 0021
        // 0022 - MMDRVINSTALL - 0022
        // 0065 - JOYGETNUMDEVS - 0065
        // 0066 - JOYGETDEVCAPS - 0066
        // 0067 - JOYGETPOS - 0067
        // 0068 - JOYGETTHRESHOLD - 0068
        // 0069 - JOYRELEASECAPTURE - 0069
        // 006A - JOYSETCAPTURE - 006A
        // 006B - JOYSETTHRESHOLD - 006B
        // 006D - JOYSETCALIBRATION - 006D

        [EntryPoint(0x00c9)]
        [DllImport("winmm.dll")]
        public static extern nuint midiOutGetNumDevs();

        // 00CA - MIDIOUTGETDEVCAPS - 00CA
        // 00CB - MIDIOUTGETERRORTEXT - 00CB
        // 00CC - MIDIOUTOPEN - 00CC
        // 00CD - MIDIOUTCLOSE - 00CD
        // 00CE - MIDIOUTPREPAREHEADER - 00CE
        // 00CF - MIDIOUTUNPREPAREHEADER - 00CF
        // 00D0 - MIDIOUTSHORTMSG - 00D0
        // 00D1 - MIDIOUTLONGMSG - 00D1
        // 00D2 - MIDIOUTRESET - 00D2
        // 00D3 - MIDIOUTGETVOLUME - 00D3
        // 00D4 - MIDIOUTSETVOLUME - 00D4
        // 00D5 - MIDIOUTCACHEPATCHES - 00D5
        // 00D6 - MIDIOUTCACHEDRUMPATCHES - 00D6
        // 00D7 - MIDIOUTGETID - 00D7
        // 00D8 - MIDIOUTMESSAGE - 00D8
        // 012D - MIDIINGETNUMDEVS - 012D
        // 012E - MIDIINGETDEVCAPS - 012E
        // 012F - MIDIINGETERRORTEXT - 012F
        // 0130 - MIDIINOPEN - 0130
        // 0131 - MIDIINCLOSE - 0131
        // 0132 - MIDIINPREPAREHEADER - 0132
        // 0133 - MIDIINUNPREPAREHEADER - 0133
        // 0134 - MIDIINADDBUFFER - 0134
        // 0135 - MIDIINSTART - 0135
        // 0136 - MIDIINSTOP - 0136
        // 0137 - MIDIINRESET - 0137
        // 0138 - MIDIINGETID - 0138
        // 0139 - MIDIINMESSAGE - 0139
        // 015E - AUXGETNUMDEVS - 015E
        // 015F - AUXGETDEVCAPS - 015F
        // 0160 - AUXGETVOLUME - 0160
        // 0161 - AUXSETVOLUME - 0161
        // 0162 - AUXOUTMESSAGE - 0162

        [EntryPoint(0x0191)]
        [DllImport("winmm.dll")]
        public static extern nuint waveOutGetNumDevs();

        // 0192 - WAVEOUTGETDEVCAPS - 0192
        // 0193 - WAVEOUTGETERRORTEXT - 0193
        // 0194 - WAVEOUTOPEN - 0194
        // 0195 - WAVEOUTCLOSE - 0195
        // 0196 - WAVEOUTPREPAREHEADER - 0196
        // 0197 - WAVEOUTUNPREPAREHEADER - 0197
        // 0198 - WAVEOUTWRITE - 0198
        // 0199 - WAVEOUTPAUSE - 0199
        // 019A - WAVEOUTRESTART - 019A
        // 019B - WAVEOUTRESET - 019B
        // 019C - WAVEOUTGETPOSITION - 019C
        // 019D - WAVEOUTGETPITCH - 019D
        // 019E - WAVEOUTSETPITCH - 019E
        // 019F - WAVEOUTGETVOLUME - 019F
        // 01A0 - WAVEOUTSETVOLUME - 01A0
        // 01A1 - WAVEOUTGETPLAYBACKRATE - 01A1
        // 01A2 - WAVEOUTSETPLAYBACKRATE - 01A2
        // 01A3 - WAVEOUTBREAKLOOP - 01A3
        // 01A4 - WAVEOUTGETID - 01A4
        // 01A5 - WAVEOUTMESSAGE - 01A5
        // 01F5 - WAVEINGETNUMDEVS - 01F5
        // 01F6 - WAVEINGETDEVCAPS - 01F6
        // 01F7 - WAVEINGETERRORTEXT - 01F7
        // 01F8 - WAVEINOPEN - 01F8
        // 01F9 - WAVEINCLOSE - 01F9
        // 01FA - WAVEINPREPAREHEADER - 01FA
        // 01FB - WAVEINUNPREPAREHEADER - 01FB
        // 01FC - WAVEINADDBUFFER - 01FC
        // 01FD - WAVEINSTART - 01FD
        // 01FE - WAVEINSTOP - 01FE
        // 01FF - WAVEINRESET - 01FF
        // 0200 - WAVEINGETPOSITION - 0200
        // 0201 - WAVEINGETID - 0201
        // 0202 - WAVEINMESSAGE - 0202
        // 0259 - TIMEGETSYSTEMTIME - 0259
        // 025A - TIMESETEVENT - 025A
        // 025B - TIMEKILLEVENT - 025B
        // 025C - TIMEGETDEVCAPS - 025C
        // 025D - TIMEBEGINPERIOD - 025D
        // 025E - TIMEENDPERIOD - 025E
        // 025F - TIMEGETTIME - 025F

        [DllImport("winmm.dll", CharSet = CharSet.Unicode, EntryPoint = "mciSendCommandW")]
        public static extern uint mciSendCommand(uint IDDevice, uint message, IntPtr fdwCommand, IntPtr dwParam);

        HandleMap _mciDeviceIdMap = new HandleMap();

        ushort DeviceIdTo16(uint deviceId)
        {
            return _mciDeviceIdMap.To16(BitUtils.DWordToIntPtr(deviceId));
        }

        uint DeviceIdTo32(ushort deviceId)
        {
            return _mciDeviceIdMap.To32(deviceId).DWord();
        }

        [EntryPoint(0x02bd)]
        public uint mciSendCommand(ushort uDeviceId, ushort uMessage, uint dwParam1, uint dwParam2)
        {
            switch (uMessage)
            {
                case Win16.MCI_OPEN:
                {
                    using (var ctx = new TempContext(_machine))
                    {
                        var op16 = _machine.ReadStruct<Win16.MCI_OPEN_PARAMS>(dwParam2);
                        var op32 = new Win32.MCI_OPEN_PARAMS();

                        // Convert type
                        if ((dwParam1 & Win16.MCI_OPEN_TYPE) != 0)
                        {
                            if ((dwParam1 & Win16.MCI_OPEN_TYPE_ID) != 0)
                                op32.lpstrDeviceName = BitUtils.DWordToIntPtr(op16.lpstrDeviceName);
                            else
                                op32.lpstrDeviceName = ctx.AllocUnmanagedString(_machine.ReadString(op16.lpstrDeviceName));
                        }

                        // Convert element
                        if ((dwParam1 & Win16.MCI_OPEN_ELEMENT) != 0)
                        {
                            if ((dwParam1 & Win16.MCI_OPEN_ELEMENT_ID) != 0)
                                op32.lpstrElementName = BitUtils.DWordToIntPtr(op16.lpstrElementName);
                            else
                                op32.lpstrElementName = ctx.AllocUnmanagedString(ResolveMediaFile(_machine.ReadString(op16.lpstrElementName)));
                        }

                        // Convert element
                        if ((dwParam1 & Win16.MCI_OPEN_ALIAS) != 0)
                        {
                            op32.lpstrAlias = Marshal.StringToHGlobalUni(_machine.ReadString(op16.lpstrAlias));
                        }

                        // Callback
                        if (HWND.Map.IsValid16(op16.dwCallback.Loword()))
                            op32.dwCallback = HWND.Map.To32(op16.dwCallback.Loword());

                        // Open
                        unsafe
                        {
                            Win32.MCI_OPEN_PARAMS* p = &op32;
                            uint retv = mciSendCommand(uDeviceId, Win32.MCI_OPEN, (IntPtr)dwParam1, (IntPtr)p);
                            if (retv == 0)
                            {
                                op16.wDeviceID = DeviceIdTo16(op32.nDeviceID);
                            }
                            else
                            {
                                op16.wDeviceID = 0;
                            }
                            _machine.WriteStruct(dwParam2, op16);
                            return retv;
                        }
                    }
                }

                case Win16.MCI_CLOSE:
                {
                    if (dwParam2 == 0)
                    {
                        return mciSendCommand(DeviceIdTo32(uDeviceId), Win32.MCI_CLOSE, (IntPtr)dwParam1, IntPtr.Zero);
                    }
                    else
                    {
                        var st16 = _machine.ReadStruct<Win16.MCI_GENERIC_PARAMS>(dwParam2);
                        var st32 = new Win32.MCI_GENERIC_PARAMS();

                        if (HWND.Map.IsValid16(st16.dwCallback.Loword()))
                            st32.dwCallback = HWND.Map.To32(st16.dwCallback.Loword());

                        unsafe
                        {
                            return mciSendCommand(DeviceIdTo32(uDeviceId), Win32.MCI_CLOSE, (IntPtr)dwParam1, (IntPtr)(&st32));
                        }
                    }
                }

                case Win16.MCI_PLAY:
                {
                    var st16 = _machine.ReadStruct<Win16.MCI_PLAY_PARAMS>(dwParam2);
                    var st32 = new Win32.MCI_PLAY_PARAMS();

                    st32.dwFrom = st16.dwFrom;
                    st32.dwTo = st16.dwTo;

                    if (HWND.Map.IsValid16(st16.dwCallback.Loword()))
                        st32.dwCallback = HWND.Map.To32(st16.dwCallback.Loword());

                    unsafe
                    {
                        return mciSendCommand(DeviceIdTo32(uDeviceId), Win32.MCI_PLAY, (IntPtr)dwParam1, (IntPtr)(&st32));
                    }
                }

                case Win16.MCI_STATUS:
                {
                    
                    var st16 = _machine.ReadStruct<Win16.MCI_STATUS_PARAMS>(dwParam2);
                    var st32 = new Win32.MCI_STATUS_PARAMS();

                    st32.dwItem = st16.dwItem;
                    st32.dwTrack = st16.dwTrack;
                                                   
                    if (HWND.Map.IsValid16(st16.dwCallback.Loword()))
                        st32.dwCallback = HWND.Map.To32(st16.dwCallback.Loword());

                    unsafe
                    {
                        uint retv = mciSendCommand(DeviceIdTo32(uDeviceId), Win32.MCI_STATUS, (IntPtr)dwParam1, (IntPtr)(&st32));
                        st16.dwReturn = st32.dwReturn.DWord(); 
                        _machine.WriteStruct(dwParam2, st16);
                        return retv;
                    }
                }
            }

            throw new NotImplementedException($"[mciSendCommand] Unsupported MCI command: 0x{uMessage:X4}");
        }

        // 02BE - MCISENDSTRING - 02BE
        // 02BF - MCIGETDEVICEID - 02BF
        // 02C0 - MCIPARSECOMMAND - 02C0
        // 02C1 - MCILOADCOMMANDRESOURCE - 02C1

        [EntryPoint(0x02c2)]
        public bool mciGetErrorString(uint error, uint buffer, nuint length)
        {
            throw new NotImplementedException("mciGetErrorString");
        }

        // 02C3 - MCISETDRIVERDATA - 02C3
        // 02C4 - MCIGETDRIVERDATA - 02C4
        // 02C6 - MCIDRIVERYIELD - 02C6
        // 02C7 - MCIDRIVERNOTIFY - 02C7
        // 02C8 - MCIEXECUTE - 02C8
        // 02C9 - MCIFREECOMMANDRESOURCE - 02C9
        // 02CA - MCISETYIELDPROC - 02CA
        // 02CB - MCIGETDEVICEIDFROMELEMENTID - 02CB
        // 02CC - MCIGETYIELDPROC - 02CC
        // 02CD - MCIGETCREATORTASK - 02CD
        // 0320 - MIXERGETNUMDEVS - 0320
        // 0321 - MIXERGETDEVCAPS - 0321
        // 0322 - MIXEROPEN - 0322
        // 0323 - MIXERCLOSE - 0323
        // 0324 - MIXERMESSAGE - 0324
        // 0325 - MIXERGETLINEINFO - 0325
        // 0326 - MIXERGETID - 0326
        // 0327 - MIXERGETLINECONTROLS - 0327
        // 0328 - MIXERGETCONTROLDETAILS - 0328
        // 0329 - MIXERSETCONTROLDETAILS - 0329
        // 0384 - MMTASKCREATE - 0384
        // 0386 - MMTASKBLOCK - 0386
        // 0387 - MMTASKSIGNAL - 0387
        // 0388 - MMGETCURRENTTASK - 0388
        // 0389 - MMTASKYIELD - 0389
        // 044C - DRVOPEN - 044C
        // 044D - DRVCLOSE - 044D
        // 044E - DRVSENDMESSAGE - 044E
        // 044F - DRVGETMODULEHANDLE - 044F
        // 0450 - DRVDEFDRIVERPROC - 0450
        // 04BA - MMIOOPEN - 04BA
        // 04BB - MMIOCLOSE - 04BB
        // 04BC - MMIOREAD - 04BC
        // 04BD - MMIOWRITE - 04BD
        // 04BE - MMIOSEEK - 04BE
        // 04BF - MMIOGETINFO - 04BF
        // 04C0 - MMIOSETINFO - 04C0
        // 04C1 - MMIOSETBUFFER - 04C1
        // 04C2 - MMIOFLUSH - 04C2
        // 04C3 - MMIOADVANCE - 04C3
        // 04C4 - MMIOSTRINGTOFOURCC - 04C4
        // 04C5 - MMIOINSTALLIOPROC - 04C5
        // 04C6 - MMIOSENDMESSAGE - 04C6
        // 04C7 - MMIODESCEND - 04C7
        // 04C8 - MMIOASCEND - 04C8
        // 04C9 - MMIOCREATECHUNK - 04C9
        // 04CA - MMIORENAME - 04CA
    }
}
