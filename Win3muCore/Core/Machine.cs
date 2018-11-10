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
using PetaJson;
using Sharp86;

namespace Win3muCore
{
    public class Machine : CPU, DosApi.ISite, IMemoryBus
    {
        public Machine()
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += UnhandledException;
            }

            _pathMapper = new PathMapper(this);
            _globalHeap = new GlobalHeap(this);
            _stringHeap = new StringHeap(this);
            _moduleManager = new ModuleManager(this);
            _messaging = new Messaging(this);
            _variableResolver = new VariableResolver();
            _expressionContext = new ExpressionContext(this);
            _symbolResolver = new SymbolResolver(this);
            _stackWalker = new StackWalker(this);
            _expressionContext.PushSymbolScope(_symbolResolver);

            this.MemoryBus = _globalHeap;

            RegisterVariables();

            // Create system heaps
            _systemCodeSelector = _globalHeap.Alloc("System Thunks", 0, 0x10000);
            _globalHeap.SetSelectorAttributes(_systemCodeSelector, true, true);
            _systemDataHeap = _globalHeap.CreateLocalHeap("System Local Heap", 0);
            _globalHeap.SetSelectorAttributes(_systemDataHeap.GlobalHandle, false, false);

            // Initialise the system return thunk
            CreateSysRetThunk();

            // Creae DOS Api handler
            _dos = new DosApi(this, this);

            // Load standard modules
            _kernel = _moduleManager.LoadModule(new Kernel()) as Kernel;
            _user = _moduleManager.LoadModule(new User()) as User;
            _moduleManager.LoadModule(new Gdi());
            _moduleManager.LoadModule(new MMSystem());
            _moduleManager.LoadModule(new Keyboard());
//            _moduleManager.LoadModule(new Shell());
            _moduleManager.LoadModule(new DdeML());
            _moduleManager.LoadModule(new Sound());

            _disassembler = new Disassembler(this);

            this.InstructionHook = () =>
            {
                if (logExecution)
                {
                    _disassembled = null;
                    Log.WriteLine(_variableResolver.ResolveTokenizedString(_logExecutionFormat));
                }
            };

        }

        void UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            // Dump global heap
            Log.WriteLine("\nGlobal Heap Map:");
            foreach (var sel in GlobalHeap.AllSelectors)
            {
                Log.WriteLine("0x{0:X4} len:0x{1:X4} - {2}", 
                    sel.selector, 
                    (sel.allocation == null || sel.allocation == null) ? 0 : sel.allocation.buffer.Length, 
                    sel.name);
            }

            Log.WriteLine("\nCall Stack:");
            bool first = true;
            foreach (var e in StackWalker.WalkStack())
            {
                Log.WriteLine("{0} 0x{1:X4}:{2:X4} [stack={3:X4}] {4}", first ? "→" : " ", 
                            e.csip.Hiword(), e.csip.Loword(), e.sp, e.name);
                first = false;
            }


            // Dump exception
            Log.WriteLine("\nUnhandled Exception:");
            Log.WriteLine(args.ExceptionObject.ToString());

            // Flush log
            Log.Flush();

            // Prevent re-entrancy while message box is shown
            IsStoppedInDebugger = true;

            // Unwrap useless exception wrapper
            var x = args.ExceptionObject as Exception;
            while (x is System.Reflection.TargetInvocationException)
                x = x.InnerException;

            User.MessageBox(IntPtr.Zero, 
                string.Format("An unrecoverable error has occurred:\n\n{0}", x.Message), 
                string.Format("{0} (via Win3mu)", System.IO.Path.GetFileName(ProgramHostPath)),
                Win32.MB_OK|Win32.MB_ERROR|Win32.MB_TASKMODAL);

            // Kill the process
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        // Fail on anything sus
        public bool StrictMode = true;

        DebuggerCore _debugger;
        StackWalker _stackWalker;
        DosApi _dos;
        Kernel _kernel;
        public Kernel Kernel { get { return _kernel; } }
        User _user;
        public User User { get { return _user; } }

        public string ConfigFile
        {
            get;
            set;
        }

        public string ProgramHostPath;

        public void MergeConfig(string configName, string programName, Dictionary<string, object> config, string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                // Load the file
                var fileConfig = Json.ParseFile<Dictionary<string, object>>(filename);

                // Debug/Release config?
                var configConfig = fileConfig.GetPath<Dictionary<string, object>>("config." + configName);
                fileConfig.Remove("config");
                if (configConfig != null)
                {
                    JsonMerge.Merge(fileConfig, configConfig);
                }
                
                // Per-app config?
                var appConfig = fileConfig.GetPath<Dictionary<string, object>>("programSpecific." + programName);
                fileConfig.Remove("programSpecific");
                if (appConfig!= null)
                {                                      
                    JsonMerge.Merge(fileConfig, appConfig);
                }

                JsonMerge.Merge(config, fileConfig);
            }
        }
                
        public int RunProgram(string programName, string[] commandArgs, int nCmdShow)
        {
            try
            {
                var cl = new Utils.CommandLine(commandArgs);

                // Store the program path
                ProgramHostPath = System.IO.Path.GetFullPath(programName);

                // Work out base name for the program (we'll use this to merge program specific configuration settings)
                var baseProgramName = System.IO.Path.GetFileNameWithoutExtension(programName).ToLowerInvariant();

                // Create merged config
                var config = new Dictionary<string, object>();
                MergeConfig(cl.Config, baseProgramName, config, VariableResolver.Resolve("$(Win3muFolder)\\config.json"));
                MergeConfig(cl.Config, baseProgramName, config, VariableResolver.Resolve("$(AppData)\\Win3mu\\config.json"));
                MergeConfig(cl.Config, baseProgramName, config, System.IO.Path.Combine(System.IO.Path.GetDirectoryName(programName), "config.json"));
                MergeConfig(cl.Config, baseProgramName, config, System.IO.Path.ChangeExtension(programName, ".json"));
                Json.ReparseInto(this, config);

                // Need console?
                if (enableDebugger || consoleLogger)
                {
                    ConsoleHelper.CreateConsole();
                }

                // Initialize logging
                Log.Init(consoleLogger, VariableResolver.Resolve(fileLogger));
                _pathMapper.Prepare(mountPoints);
                _dos.EnableApiLogging = logApiCalls;
                _dos.EnableFileLogging = logFileOperations;

                // Log configuration
                Log.WriteLine("Configuration:");
                Log.WriteLine(Json.Format(config));
                Log.WriteLine("");

                // Connect debugger?
                if (enableDebugger)
                {
                    // Connect debugger
                    _debugger = new Debugging.Win3muDebugger(this);
                    _debugger.CPU = this;
                    _debugger.ExpressionContext = ExpressionContext;
                    _debugger.CommandDispatcher.RegisterCommandHandler(new Win3muCore.Debugging.DebuggerCommandExtensions(this));
                    _debugger.SettingsFile = VariableResolver.Resolve(DebuggerSettingsFile);

                    if (DebuggerCommands!=null)
                    {
                        foreach (var cmd in DebuggerCommands)
                        {
                            _debugger.CommandDispatcher.EnqueueCommand(cmd);
                        }
                    }

                    // Break immediately?
                    if (cl.Break || breakOnLoad || DebuggerCommands!=null && DebuggerCommands.Count > 0)
                        _debugger.Break();
                }

                // Setup execution logging
                _logExecutionFormat = VariableResolver.TokenizeString(logExecutionFormat);

                // Map the program name to 8.3 name
                var programName16 = _pathMapper.MapHostToGuest(programName, false);

                // Set the current directory
                _dos.WorkingDirectory = System.IO.Path.GetDirectoryName(programName16);

                // Look for unqualified dll paths in same folder as the program
                _moduleManager.SetProcessPath(System.IO.Path.GetDirectoryName(programName16));

                // Load the executable module
                ProcessModule = _moduleManager.LoadModule(programName16) as Module16;
                ProcessModule.PrepareRun(this, cl.CommandTail16, nCmdShow);

                Log.WriteLine("Starting program...");

                _stackWalker.EnterTransition("WinMain");

                try
                {
                    // Run until finished
                    while (!_finished)
                    {
                        Run(1000000);
                    }
                }
                finally
                {
                    _stackWalker.LeaveTransition();
                }

                Log.WriteLine("Program finished with exit code {0}.", _exitCode);

                ProcessModule = null;

                Log.Flush(); 

                return _exitCode;
            }
            catch (Exception x)
            {
                Log.WriteLine(x.Message);      
                Log.Flush();
                throw;
            }
        }

        public Module16 ProcessModule
        {
            get;
            private set;
        }

        ushort ReadWordSafe(ushort seg, ushort offset)
        {
            try
            {
                var retv = this.ReadWord(seg, offset);
                return retv;
            }
            catch
            {
                return 0;
            }
        }
                                                                                 

        [Json("env")]
        public Dictionary<string, string> Environment = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        [Json("mountPoints", KeepInstance = true)]
        public Dictionary<string, PathMapper.Mount> mountPoints = new Dictionary<string, PathMapper.Mount>();

        [Json("enableDebugger")]
        public bool enableDebugger;

        [Json("breakOnLoad")]
        public bool breakOnLoad;

        [Json("consoleLogger")]
        public bool consoleLogger;

        [Json("fileLogger")]
        public string fileLogger;

        [Json("logModules")]
        public bool logModules = true;

        [Json("logRelocations")]
        public bool logRelocations = false;

        [Json("logExecution")]
        public bool logExecution = false;

        [Json("logExecutionFormat")]
        public string logExecutionFormat = "$(cputime,12) $(cs):$(ip): $(asm)";
        private object _logExecutionFormat;

        [Json("logMessages")]
        public bool logMessages = false;

        [Json("logApiCalls")]
        public bool logApiCalls = false;

        [Json("logFileOperations")]
        public bool logFileOperations = false;

        [Json("logDispatchedMessages")]
        public bool logDispatchedMessages = false;

        [Json("logWarnings")]
        public bool logWarnings = true;

        [Json("logGlobalAllocations")]
        public bool logGlobalAllocations = false;

        [Json("systemMetrics", KeepInstance = true)]
        public Dictionary<string, int> SystemMetrics = new Dictionary<string, int>();

        [Json("systemColors", KeepInstance = true)]
        public Dictionary<string, int> SystemColors = new Dictionary<string, int>();

        [Json("debuggerSettingsFile")]
        public string DebuggerSettingsFile = "$(AppData)\\Win3mu\\$(AppName).debugger.json";

        [Json("debuggerCommands")]
        public List<string> DebuggerCommands;


        public new DebuggerCore Debugger
        {
            get
            {
                return _debugger;
            }
        }

        public StackWalker StackWalker
        {
            get
            {
                return _stackWalker;
            }
        }
                                                                         
        public DosApi Dos
        {
            get { return _dos; }
        }

        uint _sysRetDepth;
        uint _sysRetThunk;
        void CreateSysRetThunk()
        {
            // Store address
            _sysRetThunk = (uint)(_systemCodeSelector << 16 | _systemCodeGenPos);

            // Get memory buffer
            byte[] mem = _globalHeap.GetBuffer(_systemCodeSelector, true);

            // INT 81h
            mem[_systemCodeGenPos++] = 0xCD;
            mem[_systemCodeGenPos++] = SysRetInterrupt;
        }


        public void CallVM(uint lpfnProc, string name)
        {
            // Calls into the virtual machine are handled by:
            // 
            // 1. saving the machine's current IP
            // 2. setting ip to address of target function
            // 3. pushing onto the stack the address of a "system return thunk" to
            //     act as the return address for the invoked function
            // 4. executing instructions until the system return thunk is executed
            //      (ie: when the virtual function exits)
            // 5. the system return thunk is piece of code that invokes int 81h which
            //      which is hooked in RaiseInterrupt override of the CPU and flags
            //      that the function has returned be decrementing a depth counter
            // 6. restoring the original IP
            // 
            // NB: a depth counter is used as the flag to re-entrancy to this function
            //     works.

            _stackWalker.EnterTransition(string.Format("Call VM 0x{0:X4}:{1:X4} {2}", lpfnProc.Hiword(), lpfnProc.Loword(), name));

            try
            {

                if (logExecution)
                    Log.Write("--> Native to Emulated Transition: {0:X8} (depth={1})\n", lpfnProc, _sysRetDepth);

                // Save the old IP
                var oldCS = cs;
                var oldIP = ip;

                // Setup the new IP
                cs = lpfnProc.Hiword();
                ip = lpfnProc.Loword();

                // Push address of the exit routine
                this.PushDWord(_sysRetThunk);

                // Setup the sys call
                _sysRetDepth++;
                uint sysCallDepthAtCall= _sysRetDepth;

                // Fake FixDS
                this.ax = this.ss;

                // Process until the sys return thunk is invoked
                while (_sysRetDepth >= sysCallDepthAtCall)
                {
                    // Hrm - this isn't too efficient but will do for now.
                    Run(1);
                }

                if (logExecution)
                    Log.Write("<-- Emulated to Native Transition: {0:X8} (depth={1})\n", lpfnProc, _sysRetDepth);

                // Restore stack
                if (oldCS != 0)
                {
                    cs = oldCS;
                    ip = oldIP;
                }
            }
            finally
            {
                _stackWalker.LeaveTransition();
            }
        }

        public void ExitProcess(int code)
        {
            AbortRunFrame();
            _finished = true;
            _exitCode = code;
        }

        bool _finished;
        int _exitCode;

        Disassembler _disassembler;
        string _disassembled;

        #region Path Mapper
        [Json("pathMapper", KeepInstance = true)]
        PathMapper _pathMapper;
        public PathMapper PathMapper
        {
            get { return _pathMapper; }
        }
        #endregion
        
        #region Global Heap
        GlobalHeap _globalHeap;
        public GlobalHeap GlobalHeap
        {
            get { return _globalHeap; }
        }

        // Provide a delegating implementation of IMemoryBus so we
        // can use Machine.ReadByte etc...
        public bool IsExecutableSelector(ushort seg)
        {
            return _globalHeap.IsExecutableSelector(seg);
        }
        public byte ReadByte(ushort seg, ushort offset)
        {
            return _globalHeap.ReadByte(seg, offset);
        }
        public void WriteByte(ushort seg, ushort offset, byte value)
        {
            _globalHeap.WriteByte(seg, offset, value);
        }
        #endregion

        #region System Heap

        ushort _systemCodeSelector;
        ushort _systemCodeGenPos = 0;
        ushort SystemCodeSelector
        {
            get { return _systemCodeSelector; }
        }

        LocalHeap _systemDataHeap;
        LocalHeap SystemDataHeap
        {
            get { return _systemDataHeap; }
        }

        #endregion

        #region String Heap

        StringHeap _stringHeap;
        public StringHeap StringHeap
        {
            get
            {
                return _stringHeap;
            }
        }

        #endregion

        #region Module Manager
        ModuleManager _moduleManager;
        public ModuleManager ModuleManager
        {
            get { return _moduleManager; }
        }

        #endregion

        #region Thunks

        public const byte SysCallInterrupt = 0x80;
        public const byte SysRetInterrupt = 0x81;                    

        public override void RaiseInterrupt(byte interruptNumber)
        {
            // System call?
            switch (interruptNumber)
            {
                case SysCallInterrupt:
                {
                    // Check index is valid
                    if (ax < _systemThunkHanders.Count)
                    {
                        // Invoke it
                        _stackWalker.EnterTransition(_thunkNames[ax]);
                        try
                        {
                            _systemThunkHanders[ax]();
                        }
                        finally
                        {
                            _stackWalker.LeaveTransition();
                        }

                        return;
                    }
                    break;
                }

                case SysRetInterrupt:
                {
                    // Mark the end of the system ret call
                    _sysRetDepth--;
                    return;
                }

                case 0x1A:
                    _dos.DispatchInt1A();
                    return;

                case 0x21:
                    _dos.DispatchInt21();
                    return;

                case 0x2f:
                    _dos.DispatchInt2f();
                    return;
            }

            base.RaiseInterrupt(interruptNumber);
        }

        List<Action> _systemThunkHanders = new List<Action>();
        Dictionary<ushort, string> _thunkNames = new Dictionary<ushort, string>();

        public uint CreateNopThunk(string name)
        {
            // Capture address of this thunk
            ushort address = _systemCodeGenPos;

            ushort thunkIndex = (ushort)_systemThunkHanders.Count;
            _systemThunkHanders.Add(() => { });
            _thunkNames[thunkIndex] = name;

            // Create the thunk
            byte[] mem = _globalHeap.GetBuffer(_systemCodeSelector, true);
            mem[_systemCodeGenPos++] = 0x90;
            return (uint)(_systemCodeSelector << 16 | address);
        }

        public uint CreateSystemThunk(Action handler, ushort popStack, bool preserveAX, string name)
        {
            // Capture address of this thunk
            ushort address = _systemCodeGenPos;

            // Store the handler
            ushort thunkIndex = (ushort)_systemThunkHanders.Count;
            _systemThunkHanders.Add(handler);

            _thunkNames[thunkIndex] = name;

            // Create the thunk
            byte[] mem = _globalHeap.GetBuffer(_systemCodeSelector, true);
            if (preserveAX)
            {
                // PUSH AX
                mem[_systemCodeGenPos++] = 0x50;
            }

            // MOV AX, thunk index
            mem[_systemCodeGenPos++] = 0xb8;
            mem[_systemCodeGenPos++] = (byte)(thunkIndex & 0xFF);
            mem[_systemCodeGenPos++] = (byte)(thunkIndex >> 8);

            // INT 80h
            mem[_systemCodeGenPos++] = 0xCD;
            mem[_systemCodeGenPos++] = SysCallInterrupt;

            // RETF
            if (popStack == 0)
            {
                mem[_systemCodeGenPos++] = 0xCB;
            }
            else
            {
                mem[_systemCodeGenPos++] = 0xCA;
                mem[_systemCodeGenPos++] = (byte)(popStack & 0xFF);
                mem[_systemCodeGenPos++] = (byte)(popStack >> 8);
            }

            // Return seg:offset address of thunk
            return (uint)(_systemCodeSelector << 16 | address);
        }

        List<ushort> _freeProcInstances = new List<ushort>();
        public uint MakeProcInstance(ushort ds, uint targetProc)
        {
            // Allocate some memory out of the system heap
            ushort address = this._systemCodeGenPos;
            if (_freeProcInstances.Count>0)
            {
                address = _freeProcInstances[0];
                _freeProcInstances.RemoveAt(0);
            }

            // Create the thunk
            byte[] mem = _globalHeap.GetBuffer(_systemCodeSelector, true);

            ushort p = address;

            // MOV AX, thunk index
            mem[p++] = 0xb8;
            mem.WriteWord(p, ds);
            p += 2;

            // JMP FAR target address
            mem[p++] = 0xEA;
            mem.WriteDWord(p, targetProc);
            p += 4;

            // Update gen pos if generated a new thunk
            if (address == _systemCodeGenPos)
                _systemCodeGenPos = p;

            // Return address of proc instance
            return (uint)(_systemCodeSelector << 16 | address);
        }

        public void FreeProcInstance(uint ptr)
        {
            if (ptr == 0)
                return;

            if (ptr.Hiword() != _systemCodeSelector)
            {
                Log.WriteLine("Invalid pointer passed to FreeProcInstance: {0:X8}", ptr);
                return;
            }

            _freeProcInstances.Add(ptr.Loword());
        }

        #endregion

        #region Messaging
        Messaging _messaging;
        public Messaging Messaging
        {
            get
            {
                return _messaging;
            }
        }
        #endregion

        #region Variables and Expressions
        VariableResolver _variableResolver;
        public VariableResolver VariableResolver
        {
            get
            {
                return _variableResolver;
            }
        }

        void RegisterVariables()
        {
            _variableResolver.Register("AppName", () => System.IO.Path.GetFileNameWithoutExtension(ProgramHostPath));
            _variableResolver.Register("AppFolder", () => System.IO.Path.GetDirectoryName(ProgramHostPath));
            _variableResolver.Register("Win3muFolder", () =>
            {
                if (System.Reflection.Assembly.GetExecutingAssembly() != null)
                    return System.IO.Path.GetDirectoryName(typeof(Machine).Assembly.Location);
                return null;
            });
            _variableResolver.Register("ax", () => ax.ToString("X4"));
            _variableResolver.Register("bx", () => bx.ToString("X4"));
            _variableResolver.Register("cx", () => cx.ToString("X4"));
            _variableResolver.Register("dx", () => dx.ToString("X4"));
            _variableResolver.Register("si", () => si.ToString("X4"));
            _variableResolver.Register("di", () => di.ToString("X4"));
            _variableResolver.Register("sp", () => sp.ToString("X4"));
            _variableResolver.Register("bp", () => bp.ToString("X4"));
            _variableResolver.Register("ip", () => ip.ToString("X4"));
            _variableResolver.Register("cs", () => cs.ToString("X4"));
            _variableResolver.Register("es", () => es.ToString("X4"));
            _variableResolver.Register("ds", () => ds.ToString("X4"));
            _variableResolver.Register("ss", () => ss.ToString("X4"));
            _variableResolver.Register("eflags", () => EFlags.ToString("X4"));
            _variableResolver.Register("al", () => al.ToString("X2"));
            _variableResolver.Register("ah", () => ah.ToString("X2"));
            _variableResolver.Register("bl", () => bl.ToString("X2"));
            _variableResolver.Register("bh", () => bh.ToString("X2"));
            _variableResolver.Register("cl", () => cl.ToString("X2"));
            _variableResolver.Register("ch", () => ch.ToString("X2"));
            _variableResolver.Register("dl", () => dl.ToString("X2"));
            _variableResolver.Register("dh", () => dh.ToString("X2"));
            _variableResolver.Register("asm", () =>
            {
                if (_disassembled == null)
                    _disassembled = _disassembler.Read(cs, ip);
                return _disassembled;
            });                              
            _variableResolver.Register("annotations", () =>
            {
                if (_disassembled == null)
                    _disassembled = _disassembler.Read(cs, ip);
                return _expressionContext.GenerateDisassemblyAnnotations(_disassembled, _disassembler.ImplicitParams);
            });
            _variableResolver.Register("cputime", () => CpuTime.ToString());
        }

        ExpressionContext _expressionContext;
        public ExpressionContext ExpressionContext
        {
            get { return _expressionContext; }
        }

        SymbolResolver _symbolResolver;
        public SymbolResolver SymbolResolver
        {
            get { return _symbolResolver; } 
        }
        #endregion

        public uint SysAlloc(ushort bytes)
        {
            var offset = _systemDataHeap.Alloc(Win16.LMEM_FIXED, bytes);
            if (offset == 0)
                return 0;
            return (uint)(_systemDataHeap.GlobalHandle << 16 | offset);
        }

        public uint SysAllocString(string str)
        {
            if (str == null)
                return 0;

            var data = AnsiEncoding.GetBytes(str);
            var ptr_near = _systemDataHeap.Alloc(Win16.LMEM_FIXED, (ushort)(data.Length + 1));
            var ptr_far = BitUtils.MakeDWord(ptr_near, _systemDataHeap.GlobalHandle);
            this.WriteString(ptr_far, str, (ushort)(data.Length + 1));
            return ptr_far;
        }

        public uint StackAlloc<T>(T value)
        {
            var saveSP = sp;
            sp -= (ushort)Marshal.SizeOf<T>();
            var ptr = BitUtils.MakeDWord(sp, ss);
            WriteStruct(ptr, ref value);
            return ptr;
        }

        public void SysFree(uint ptr)
        {
            if (ptr == 0)
                return;
            System.Diagnostics.Debug.Assert(ptr.Hiword() == _systemDataHeap.GlobalHandle);
            _systemDataHeap.Free(ptr.Loword());
        }

        public uint SysAlloc<T>(T value)
        {
            var ptr = SysAlloc((ushort)Marshal.SizeOf<T>());
            if (ptr == 0)
                return 0;
            WriteStruct(ptr, ref value);
            return ptr;
        }

        public T SysReadAndFree<T>(uint ptr)
        {
            var t = ReadStruct<T>(ptr);
            SysFree(ptr);
            return t;
        }

        public void WriteStruct<T>(uint ptr, T value)
        {
            WriteStruct(ptr, ref value);
        }

        public void WriteStruct<T>(uint ptr, ref T value)
        {
            if (ptr == 0)
                throw new GeneralProtectionFaultException(ptr.Hiword(), ptr.Loword(), false);

            using (var hp = _globalHeap.GetHeapPointer(ptr, true))
            {
                Marshal.StructureToPtr<T>(value, hp, false);
            }
        }

        public void WriteStruct(uint ptr, object value)
        {
            if (ptr == 0)
                throw new GeneralProtectionFaultException(ptr.Hiword(), ptr.Loword(), false);

            if (value.GetType() == typeof(bool))
            {
                this.WriteWord(ptr.Hiword(), ptr.Loword(), (ushort)((bool)value ? 1 : 0));
                return;
            }

            // Get reference to the machine's memory
            using (var hp = _globalHeap.GetHeapPointer(ptr, true))
            {
                Marshal.StructureToPtr(value, hp, false);
            }
        }

        public T ReadStruct<T>(uint ptr)
        {
            return (T)ReadStruct(typeof(T), ptr);
        }

        public object ReadStruct(Type structType, uint ptr)
        {
            if (ptr == 0)
                throw new GeneralProtectionFaultException(ptr.Hiword(), ptr.Loword(), true);

            // Get reference to the machine's memory
            byte[] memory = _globalHeap.GetBuffer(ptr.Hiword(), false);

            unsafe
            {
                fixed (byte* pBuffer = memory)
                {
                    return System.Runtime.InteropServices.Marshal.PtrToStructure(new IntPtr(pBuffer + ptr.Loword()), structType);
                }
            }
        }

        public ushort ReadPortWord(ushort port)
        {
            throw new NotImplementedException();
        }

        public void WritePortWord(ushort port, ushort value)
        {
            throw new NotImplementedException();
        }


        #region DosApi.ISite
        void DosApi.ISite.ExitProcess(short exitCode)
        {
            ExitProcess(exitCode);
        }
        bool DosApi.ISite.DoesGuestDirectoryExist(string path)
        {
            return _pathMapper.DoesGuestDirectoryExist(path);
        }
        string DosApi.ISite.TryMapGuestPathToHost(string path, bool forWrite)
        {
            return _pathMapper.TryMapGuestToHost(path, forWrite);
        }
        string DosApi.ISite.TryMapHostPathToGuest(string path, bool forWrite)
        {
            return _pathMapper.TryMapHostToGuest(path, forWrite);
        }
        IEnumerable<string> DosApi.ISite.GetVirtualSubFolders(string guestPath)
        {
            return _pathMapper.GetVirtualSubFolders(guestPath);
        }
        uint DosApi.ISite.Alloc(ushort size)
        {
            return SysAlloc(size);
        }

        void DosApi.ISite.Free(uint ptr)
        {
            SysFree(ptr);
        }

        #endregion

        public Action NotifyCallWndProc16;

        // Helper to call a 16-bit WNDPROC
        public uint CallWndProc16(uint lpfnProc, ushort hWnd, ushort message, ushort wParam, uint lParam)
        {
            // Push parameters
            this.PushWord(hWnd);
            this.PushWord(message);
            this.PushWord(wParam);
            this.PushDWord(lParam);

            if (NotifyCallWndProc16 != null)
                NotifyCallWndProc16();

            // Call the VM
            CallVM(lpfnProc, "WndProc");

            // Return value
            return (uint)(dx << 16 | ax);
        }

        public uint CallHookProc16(uint lpfnProc, short code, ushort wParam, uint lParam)
        {
            // Push parameters
            this.PushWord((ushort)code);
            this.PushWord(wParam);
            this.PushDWord(lParam);

            // Call the VM
            CallVM(lpfnProc, "HookProc");

            // Return value
            return (uint)(dx << 16 | ax);
        }

        public static Encoding AnsiEncoding = Encoding.GetEncoding(1252);

        ushort _hEnvironment = 0;
        public ushort GetDosEnvironmentSegment()
        {
            if (_hEnvironment == 0)
            {
                // Build environment strings
                var env = string.Join("\0", Environment.Select(x => x.Key + "\0" + x.Value)) + "\0";

                // Convert to byte array
                var bytes = Encoding.GetEncoding(437).GetBytes(env);

                // Allocate environment string
                _hEnvironment = GlobalHeap.Alloc("DOS Environment Strings", Win16.GMEM_FIXED, (uint)bytes.Length);

                // Write it
                this.WriteBytes(_hEnvironment, 0, bytes);
            }

            return _hEnvironment;
        }

    }
}
