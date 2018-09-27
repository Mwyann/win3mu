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
using Win3muCore;
using Win3muCore.NeFile;

namespace Win3muRuntime
{
    public static class API
    {
        public static int Run(string program, string[] commandLine, int showWindow)
        {
            var machine = new Machine();
            return machine.RunProgram(program, commandLine, Win16.SW_SHOWNORMAL);
        }

        public static Func<string, string[], int, int> GetRunMethod()
        {
            return Run;
        }

        public static void Register()
        {
            // Store location of Win3muCore.dll
            var location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var win3muKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey("SOFTWARE\\Win3mu");
            win3muKey.SetValue("Location", location);

            // Register Win3muTool shell common
            var key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey("exefile\\shell\\win3mu_repack");
            key.SetValue("", "Convert with Win3mu");
            var command = key.CreateSubKey("command");
            command.SetValue("", string.Format("\"{0}\" \"%1\"", System.IO.Path.Combine(location, "Win3muTool.Exe")));
        }

        public static void Unregister()
        {
            // Store location of Win3muCore.dll
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\Win3mu");
            Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree("exefile\\shell\\win3mu_repack");
        }

        public static void BuildStub(string win3muExe, string win16Exe, string targetExe, bool renameWin16Exe)
        {
            unsafe
            {
                string createdTemp = null;
                try
                {
                    // Open the source exe and locate the first icon
                    using (var neFile = new NeFileReader(win16Exe))
                    {
                        // Copy stub to target directory as a temp file
                        var tmp = targetExe + ".tmp";
                        if (System.IO.File.Exists(tmp))
                        {
                            System.IO.File.Delete(tmp);
                        }
                        System.IO.File.Copy(win3muExe, tmp);
                        createdTemp = tmp;


                        // Get the icon
                        var rtIconGroup = neFile.FindResourceType(Win16.ResourceType.RT_GROUP_ICON.ToString());
                        if (rtIconGroup != null)
                        {
                            // Get the first one
                            var rIconGroup = rtIconGroup.resources[0];

                            var iconDir = Resources.LoadIconOrCursorGroup(neFile.GetResourceStream(rtIconGroup.name, rIconGroup.name));

                            // Load it
                            var rIconGroupData = neFile.LoadResource(rtIconGroup.name, rIconGroup.name);

                            // Start up 
                            var hUpdate = ResourceUtils.BeginUpdateResource(createdTemp, false);

                            // Copy the group
                            fixed (byte* p = rIconGroupData)
                            {
                                ResourceUtils.UpdateResource(hUpdate, (IntPtr)Win16.ResourceType.RT_GROUP_ICON, (IntPtr)rIconGroup.id, 0, (IntPtr)p, (uint)rIconGroupData.Length);
                            }

                            // Copy each icon from the group
                            foreach (var e in iconDir.Entries)
                            {
                                var rIconData = neFile.LoadResource(Win16.ResourceType.RT_ICON.ToString(), string.Format("#{0}", e.nId));
                                fixed (byte* p = rIconData)
                                {
                                    ResourceUtils.UpdateResource(hUpdate, (IntPtr)Win16.ResourceType.RT_ICON, (IntPtr)e.nId, 0, (IntPtr)p, (uint)rIconData.Length);
                                }
                            }

                            // Delete the old version resource so that "Win3muProxy" doesn't appear in task manager
                            ResourceUtils.UpdateResource(hUpdate, (IntPtr)Win16.ResourceType.RT_VERSION, (IntPtr)1, 0, IntPtr.Zero, 0);

                            /*
                            var rVersion = neFile.LoadResource(Win16.ResourceType.RT_VERSION.ToString(), "#1");
                            if (rVersion!=null)
                            {
                                fixed (byte* p = rVersion)
                                {
                                    ResourceUtils.UpdateResource(hUpdate, (IntPtr)Win16.ResourceType.RT_VERSION, (IntPtr)1, 0, (IntPtr)p, (uint)rVersion.Length);
                                }
                            }
                            */

                            // Update!
                            ResourceUtils.EndUpdateResource(hUpdate, false);

                        }
                    }

                    if (renameWin16Exe)
                    {
                        // Rename .exe to .exe16
                        System.IO.File.Move(win16Exe, win16Exe + "16");
                    }

                    // Rename the target
                    if (System.IO.File.Exists(targetExe))
                        System.IO.File.Delete(targetExe);
                    System.IO.File.Move(createdTemp, targetExe);
                }
                catch
                {
                    if (createdTemp != null)
                        System.IO.File.Delete(createdTemp);
                    throw;
                }
            }
        }


    }
}
