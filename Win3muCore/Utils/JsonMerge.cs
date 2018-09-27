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

namespace Win3muCore
{
    public static class JsonMerge
    {
        public static void Merge(Dictionary<string, object> dest, Dictionary<string, object> merge)
        {
            foreach (var kv in merge)
            {
                if (kv.Key.StartsWith("merge:"))
                {
                    var destKey = kv.Key.Substring(6);
                    if (dest.ContainsKey(destKey))
                    {
                        var childDst = dest[destKey] as Dictionary<string, object>;
                        var childSrc = kv.Value as Dictionary<string, object>;
                        if (childDst != null && childSrc != null)
                        {
                            Merge(childDst, childSrc);
                            continue;
                        }
                    }

                    dest[destKey] = kv.Value;
                    continue;
                }

                if (kv.Key.StartsWith("prepend:"))
                {
                    var destKey = kv.Key.Substring(8);
                    if (dest.ContainsKey(destKey))
                    {
                        var childDst = dest[destKey] as List<object>;
                        var childSrc = kv.Value as List<object>;
                        if (childDst != null && childSrc != null)
                        {
                            childDst.AddRange(childSrc);
                            continue;
                        }
                    }

                    dest[destKey] = kv.Value;
                    continue;
                }

                if (kv.Key.StartsWith("prepend:"))
                {
                    var destKey = kv.Key.Substring(8);
                    if (dest.ContainsKey(destKey))
                    {
                        var childDst = dest[destKey] as List<object>;
                        var childSrc = kv.Value as List<object>;
                        if (childDst != null && childSrc != null)
                        {
                            childDst.InsertRange(0, childSrc);
                            continue;
                        }
                    }

                    dest[destKey] = kv.Value;
                    continue;
                }

                if (kv.Key.StartsWith("delete:"))
                {
                    var destKey = kv.Key.Substring(7);
                    if (dest.ContainsKey(destKey))
                        dest.Remove(destKey);
                    continue;
                }

                // Replace
                dest[kv.Key] = kv.Value;
            }
        }
    }
}
