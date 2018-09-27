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
    public class ModuleManager
    {
        public ModuleManager(Machine machine)
        {
            _machine = machine;
            _loadedModules = new Dictionary<string, ModuleBase>(StringComparer.InvariantCultureIgnoreCase);
            _instanceMap = new Dictionary<ushort, ModuleBase>();
        }

        public IEnumerable<ModuleBase> AllModules
        {
            get
            {
                return _loadedModules.Values.OrderBy(x => x.GetModuleName());
            }
        }

        // Get a loaded module by name
        public ModuleBase GetModule(string moduleName)
        {
            ModuleBase module;
            if (!_loadedModules.TryGetValue(moduleName, out module))
                return null;

            return module;
        }

        // Get a loaded module using an instance handle
        public ModuleBase GetModule(ushort hModule)
        {
            // Get the executable module?
            if (hModule == 0)
                return _loadedModules.OfType<Module16>().FirstOrDefault(x => !x.IsDll && x.hModule!=0);

            ModuleBase module;
            if (!_instanceMap.TryGetValue(hModule, out module))
                return null;

            System.Diagnostics.Debug.Assert(module.hModule == hModule);
            return module;
        }

        public ModuleBase LoadModule(string fileOrModuleName)
        {
            return LoadModuleInternal(fileOrModuleName, null);
        }

        public ModuleBase LoadModuleInternal(string fileOrModuleName, string parentPath)
        {
            // Remove trailing '.' - VBRUN100 calls LoadLibrary("gdi.")
            if (fileOrModuleName.EndsWith("."))
                fileOrModuleName = fileOrModuleName.Substring(0, fileOrModuleName.Length - 1);

            // Look for already loaded module with correct name
            ModuleBase module;
            if (_loadedModules.TryGetValue(fileOrModuleName, out module))
            {
                module.LoadCount++;
                return module;
            }

            // Look for module with same filename
            foreach (var kv in _loadedModules)
            {
                if (System.IO.Path.GetFileName(kv.Value.GetModuleFileName()).ToLowerInvariant() == fileOrModuleName.ToLowerInvariant())
                {
                    kv.Value.LoadCount++;
                    return kv.Value;
                }
            }

            // Locate the module
            var locatedModuleGuest = LocateModule(fileOrModuleName, parentPath);
            if (locatedModuleGuest == null)
                throw new VirtualException(string.Format("Can't find module '{0}'", fileOrModuleName));

            // Look for already loaded module
            var existingModule = _loadedModules.Values.FirstOrDefault(x => x.GetModuleFileName() == locatedModuleGuest);
            if (existingModule!= null)
            {
                existingModule.LoadCount++;
                return existingModule;
            }

            // Load it
            var locatedModuleHost = _machine.PathMapper.MapGuestToHost(locatedModuleGuest, false);
            var nativeModule = new Module16(locatedModuleHost);

            // Log the module
            if (_machine.logModules)
            {
                nativeModule.NeFile.Dump(_machine.logRelocations);
            }

            // Setup the guest module filename
            nativeModule.SetGuestFileName(locatedModuleGuest.ToUpper());

            // Load it
            try
            {
                LoadModule(nativeModule);
                return nativeModule;
            }
            catch (VirtualException)
            {
                nativeModule.Unload(_machine);
                throw;
            }
        }

        string _processPath;
        public void SetProcessPath(string path)
        {
            _processPath = path;
        }

        private string LocateModule(string moduleName, string parentPath)
        {
            // Append dll extension
            if (!System.IO.Path.GetFileName(moduleName).Contains('.'))
                moduleName += ".DLL";

            // Fully qualified already?
            if (DosPath.IsFullyQualified(moduleName))
            {
                return moduleName.ToUpper();
            }

            // Check each search location
            foreach (var path in GetSearchPath(parentPath))
            {
                // Must be fully qualified
                if (!DosPath.IsFullyQualified(path))
                    continue;

                // Resolve it
                var resolvedPathGuest = DosPath.ResolveRelativePath(path, moduleName);
                var resolvedPathHost = _machine.PathMapper.TryMapGuestToHost(resolvedPathGuest, false);
                if (resolvedPathHost == null)
                    continue;

                if (System.IO.File.Exists(resolvedPathHost))
                {
                    return resolvedPathGuest.ToUpper();
                }
            }

            // Not found!
            return null;
        }

        IEnumerable<string> GetSearchPath(string parentPath)
        {
            if (parentPath != null)
                yield return parentPath;

            if (_processPath != null)
                yield return _processPath;

            string path;
            if (_machine.Environment.TryGetValue("PATH", out path))
            {
                foreach (var p in path.Split(';'))
                    yield return p;
            }
        }

        int _loadDepth = 0;
        List<ModuleBase> _pendingLink;
        public ModuleBase LoadModule(ModuleBase module)
        {
            if (_loadDepth == 0)
                _pendingLink = new List<ModuleBase>();

            _loadDepth++;

            try
            {
                module.LoadCount++;

                if (module.LoadCount==1)
                {
                    System.Diagnostics.Debug.Assert(!_loadedModules.ContainsKey(module.GetModuleName()));
                    var referencedModules = new List<ModuleBase>();

                    try
                    {
                        // Add this module to map of loaded modules incase a dependent
                        // module circularly references back to it
                        _loadedModules.Add(module.GetModuleName(), module);

                        // Load implicitly references modules
                        foreach (var m in module.GetReferencedModules())
                        {
                            // Load all referenced modules
                            referencedModules.Add(LoadModuleInternal(m, System.IO.Path.GetDirectoryName(module.GetModuleFileName())));
                        }

                        // Load this module
                        module.Load(_machine);

                        // Add to list of modules that still need to be linked
                        _pendingLink.Add(module);
                    }
                    catch (VirtualException)
                    {
                        // Unload referenced modules
                        foreach (var m in referencedModules)
                        {
                            UnloadModule(m);
                        }

                        // Remove this module
                        _loadedModules.Remove(module.GetModuleName());

                        throw;
                    }
                }
            }
            catch (VirtualException)
            {
                _loadDepth--;
                if (_loadDepth == 0)
                    _pendingLink = null;
                throw;
            }

            _loadDepth--;
            if (_loadDepth==0)
            {
                // Link modules
                try
                {
                    // Link modules
                    foreach (var m in _pendingLink)
                    {
                        Log.WriteLine("Linking module '{0}'...", m.GetModuleName());
                        m.Link(_machine);
                    }

                    // If just loaded the native .exe module
                    // then we need to setup the stack before calling
                    // initialization of other native DLLs
                    var nativeModule = module as Module16;
                    if (nativeModule!= null && !nativeModule.IsDll)
                    {
                        nativeModule.PrepareStack(_machine);
                    }

                    // Initialize all modules
                    foreach (var m in _pendingLink)
                    {
                        // Allocate instance handle
                        m.hModule = _machine.GlobalHeap.Alloc(string.Format("Module '{0}' instance handle", m.GetModuleName()), 0, 16);
                        _instanceMap.Add(m.hModule, m);

                        // Initialize
                        Log.WriteLine("Initializing module '{0}'...", m.GetModuleName());
                        m.Init(_machine);
                        m.Initialized = true;
                        System.Diagnostics.Debug.Assert(m.hModule != 0);
                    }
                }
                catch (VirtualException)
                {
                    UnloadModule(module);
                    _pendingLink = null;
                    throw;
                }
            }

            return module;
        }

        public void UnloadModule(ModuleBase module)
        {
            System.Diagnostics.Debug.Assert(module.LoadCount > 0);

            module.LoadCount--;
            if (module.LoadCount==0)
            {
                // Also unload referenced modules too
                foreach (var dep in module.GetReferencedModules())
                {
                    ModuleBase depModule;
                    if (_loadedModules.TryGetValue(dep, out depModule))
                    {
                        // Unload it
                        UnloadModule(depModule);
                    }
                }

                // Save instance handle
                ushort hInstance = module.hModule;

                try
                {
                    // Run exit code
                    if (module.Initialized)
                    {
                        module.Uninit(_machine);

                    }
                }
                finally
                {
                    // Remove from instance map
                    if (hInstance!=0)
                        _instanceMap.Remove(hInstance);
                    _machine.GlobalHeap.Free(hInstance);

                    // Remove from map
                    _loadedModules.Remove(module.GetModuleName());

                    // Unload this module
                    module.Unload(_machine);
                }
            }
        }

        Machine _machine;
        Dictionary<string, ModuleBase> _loadedModules;
        Dictionary<ushort, ModuleBase> _instanceMap;
    }
}
