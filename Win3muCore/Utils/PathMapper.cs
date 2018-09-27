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
using System.Text;
using System.Threading.Tasks;
using PetaJson;

namespace Win3muCore
{
    public class PathMapper
    {
        public class Mount
        {
            [Json("guest")]
            public string guest;

            [Json("host")]
            public string host;

            [Json("hostWrite")]
            public string hostWrite;
        }

        public PathMapper(Machine machine)
        {
            _machine = machine;
        }

        List<Mount> _mountPoints = new List<Mount>();

        public void Prepare(Dictionary<string, Mount> mountPointMap)
        {
            var vr = _machine.VariableResolver;

            // Replace all
            foreach (var kv in mountPointMap)
            {
                kv.Value.guest = vr.Resolve(kv.Value.guest == null ? kv.Key : kv.Value.guest);
                kv.Value.host = vr.Resolve(kv.Value.host);
                kv.Value.hostWrite = vr.Resolve(kv.Value.hostWrite);
            }

            // Sort longest to shortest
            _mountPoints = mountPointMap.Values.OrderByDescending(x => x.guest.Length).ToList();

            // Check all mount points are valid
            foreach (var mp in _mountPoints)
            {
                if (!DosPath.IsValid(mp.guest))
                    throw new InvalidDataException(string.Format("The mounted path '{0}' isn't a valid 8.3 filename", mp.guest));
            }
        }


        public void AddMount(string guest, string host, string hostWrite)
        {
            _mountPoints.Add(new PathMapper.Mount()
            {
                guest = guest,
                host = host,
                hostWrite = hostWrite,
            });
        }


        bool DoesPathPrefixMatch(string prefix, string path)
        {
            if (!path.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (!prefix.EndsWith("\\"))
            {
                var prefixLen = prefix.Length;
                if (prefixLen < path.Length)
                {
                    return path[prefixLen] == '\\';
                }
            }

            return true;
        }

        // Check if the guest path exists - doesn't need to be mappable
        // eg: if "C:\WINDOWS" is mapped then "C:\" does exist even if 
        //     not mapped 
        // eg2: if "C:\WINDOWS" is mapped then "C:\WINDOWS\MAYBE" should only
        //      return true if MAYBE actually exists
        // Host path doesn't need to be writable
        public bool DoesGuestDirectoryExist(string guestPath)
        {
            // Check it's valid
            if (!DosPath.IsValid(guestPath))
                return false;

            // Check if it exists as a parent folder of a mapping (example 1 above)
            for (int i = 0; i < _mountPoints.Count; i++)
            {
                var mp = _mountPoints[i];

                if (DoesPathPrefixMatch(guestPath, mp.guest))
                    return true;
            }

            // Try to map it and check if it exists (example 2 above)
            var hostPath = TryMapGuestToHost(guestPath, false);
            if (hostPath == null)
                return false;
            return System.IO.Directory.Exists(hostPath);
        }

        // Convert a host filename to 8.3 friendly version
        // Returns null if can't
        public string TryMapHostToGuest(string hostPath, bool forWrite)
        {
            // Map special extension
            if (hostPath.EndsWith(".exe16", StringComparison.InvariantCultureIgnoreCase))
                hostPath = hostPath.Substring(0, hostPath.Length - 2);

            for (int i=0; i<_mountPoints.Count; i++)
            {
                var mp = _mountPoints[i];

                string mapped = null;
                if (mp.hostWrite != null && forWrite)
                {
                    if (DoesPathPrefixMatch(mp.hostWrite, hostPath))
                    {
                        mapped = mp.guest + hostPath.Substring(mp.hostWrite.Length);
                    }
                }
                else
                {
                    if (DoesPathPrefixMatch(mp.host, hostPath))
                    {
                        mapped = mp.guest + hostPath.Substring(mp.host.Length);
                    }
                }

                if (mapped!=null)
                {
                    if (!DosPath.IsValid(mapped))
                        break;
                    return mapped;
                }
            }
            return null;
        }

        public string MapHostToGuest(string hostPath, bool forWrite)
        {
            var mapped = TryMapHostToGuest(hostPath, forWrite);
            if (mapped== null)
                throw new InvalidOperationException(string.Format("The host path '{0}' can't be mapped to a valid DOS path", hostPath));
            return mapped;
        }

        // Convert a 8.3 file name to host equivalent
        public string TryMapGuestToHost(string guestPath, bool forWrite)
        {
            if (guestPath == null)
                return null;

            if (DosPath.IsValid(guestPath))
            {
                for (int i = 0; i < _mountPoints.Count; i++)
                {
                    var mp = _mountPoints[i];

                    if (DoesPathPrefixMatch(mp.guest, guestPath))
                    {
                        // Work out the read-only path
                        var readPath = mp.host + guestPath.Substring(mp.guest.Length);

                        if (!forWrite && System.IO.Directory.Exists(readPath))
                        {
                            return readPath;
                        }

                        // If no separate write mapping then use the read path
                        if (mp.hostWrite != null)
                        {
                            // Work out the write path
                            var writePath = mp.hostWrite + guestPath.Substring(mp.guest.Length);

                            // Does it already exist?
                            if (System.IO.Directory.Exists(writePath))
                                return writePath;
                            if (System.IO.File.Exists(writePath))
                                return writePath;

                            // Copy from the read directory?
                            if (forWrite)
                            {
                                // Make sure the mounted directory exists
                                if (!System.IO.Directory.Exists(mp.hostWrite))
                                {
                                    System.IO.Directory.CreateDirectory(mp.hostWrite);
                                }

                                // File already exist?
                                if (System.IO.File.Exists(readPath))
                                {
                                    // Make sure the target folder exists
                                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(writePath));
                            
                                    // Copy the source folder
                                    System.IO.File.Copy(readPath, writePath);
                                }



                                return writePath;
                            }
                        }

                        if (readPath.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) && System.IO.File.Exists(readPath + "16"))
                            return readPath + "16";

                        // Done!
                        return readPath;
                    }
                }
            }
            return null;
        }

        public string MapGuestToHost(string guestPath, bool forWrite)
        {
            var mapped = TryMapGuestToHost(guestPath, forWrite);
            if (mapped == null)
                throw new InvalidOperationException(string.Format("The guest path '{0}' can't be mapped to a valid host path", guestPath));
            return mapped;
        }

        // Given a guest path which will always be fully qualified and ending in backslash
        // return a list of virtual sub-folders
        public IEnumerable<string> GetVirtualSubFolders(string guestPath)
        {
            System.Diagnostics.Debug.Assert(guestPath.EndsWith("\\"));

            for (int i = 0; i < _mountPoints.Count; i++)
            {
                var mp = _mountPoints[i];

                if (!mp.guest.StartsWith(guestPath))
                    continue;

                var tail = mp.guest.Substring(guestPath.Length);
                int slashPos = tail.IndexOf('\\');
                if (slashPos>0)
                {
                    tail = tail.Substring(0, slashPos);
                }
                if (tail.Length > 0)
                    yield return tail;
            }
        }

        Machine _machine;
    }
}
