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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Sharp86;

namespace Win3muCore
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    internal class MappedTypeAttribute : Attribute
    {
        public static bool Is(Type t)
        {
            if (t == null)
                return false;
            return t.GetCustomAttributes(typeof(MappedTypeAttribute), false).Any();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class ModuleAttribute : Attribute
    {
        public ModuleAttribute(string moduleName, string moduleFileName)
        {
            ModuleName = moduleName;
            ModuleFileName = moduleFileName;
        }

        public string ModuleName;
        public string ModuleFileName;
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class EntryPointAttribute : Attribute
    {
        public EntryPointAttribute(ushort ordinal, string name = null)
        {
            Ordinal = ordinal;
            Name = name;
            PreserveAX = false;
            DebugBreak = false;
            DebugBreak16 = false;
        }

        [Obfuscation(Exclude = true)]
        public bool PreserveAX;

        public ushort Ordinal;
        public string Name;
        public bool DebugBreak;
        public bool DebugBreak16;
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal class DestroyedAttribute : Attribute
    {
        public DestroyedAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal class FileNameAttribute : Attribute
    {
        public FileNameAttribute(bool forWrite)
        {
            ForWrite = forWrite;
        }

        public bool ForWrite;
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal class BufSizeAttribute : Attribute
    {
        public BufSizeAttribute(int paramDX)
        {
            ParamDX = paramDX;
        }

        public int ParamDX;
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal class MustBeNullAttribute : Attribute
    {
        public MustBeNullAttribute()
        {
        }
    }

    public class Module32 : ModuleBase
    {
        public Module32()
        {
            _attributes = (ModuleAttribute)GetType().GetCustomAttributes(typeof(ModuleAttribute), true).FirstOrDefault();
            System.Diagnostics.Debug.Assert(_attributes != null);

            // Build ordinal import table
            foreach (var mi in GetType().GetMethods())
            {
                // Get the entry point attribute
                var ma = (EntryPointAttribute)mi.GetCustomAttributes(typeof(EntryPointAttribute), true).FirstOrDefault();
                if (ma == null)
                    continue;

                var ep = new EntryPoint()
                {
                    Attributes = ma,
                    MethodInfo = mi,
                };

                if (ma.Name == null)
                {
                    ma.Name = mi.Name;
                }

                _entryPointsByName.Add(ma.Name, ep);
                _entryPointsByOrdinal.Add(ma.Ordinal, ep);
            }
        }

        class EntryPoint
        {
            public EntryPointAttribute Attributes;
            public MethodInfo MethodInfo;
            public uint procAddress;
            public ushort paramSize;
        }

        protected Machine _machine;
        Dictionary<ushort, EntryPoint> _entryPointsByOrdinal = new Dictionary<ushort, EntryPoint>();
        Dictionary<string, EntryPoint> _entryPointsByName = new Dictionary<string, EntryPoint>();

        ModuleAttribute _attributes;
        #region Abtract Methods
        public override string GetModuleName()
        {
            return _attributes.ModuleName;
        }

        public override string GetModuleFileName()
        {
            return _attributes.ModuleFileName;
        }

        public override IEnumerable<string> GetReferencedModules()
        {
            yield break;
        }

        public override void Link(Machine machine)
        {
        }

        public override void Load(Machine machine)
        {
            _machine = machine;
        }

        public override void Unload(Machine machine)
        {
        }

        public override void Init(Machine machine)
        {
        }

        public override void Uninit(Machine machine)
        {
            throw new NotImplementedException();
        }

        public override ushort GetOrdinalFromName(string functionName)
        {
            EntryPoint ep;
            if (!_entryPointsByName.TryGetValue(functionName, out ep))
                return 0;

            return ep.Attributes.Ordinal;
        }

        public override string GetNameFromOrdinal(ushort ordinal)
        {
            EntryPoint ep;
            if (!_entryPointsByOrdinal.TryGetValue(ordinal, out ep))
                return null;

            return ep.Attributes.Name;
        }

        public override IEnumerable<ushort> GetExports()
        {
            return _entryPointsByOrdinal.Keys;
        }

        Dictionary<ushort, uint> _exceptionThunks = new Dictionary<ushort, uint>();
        public override uint GetProcAddress(ushort ordinal)
        {
            EntryPoint ep;
            if (!_entryPointsByOrdinal.TryGetValue(ordinal, out ep))
            {
                uint thunk;
                if (_exceptionThunks.TryGetValue(ordinal, out thunk))
                    return thunk;

                Log.WriteLine("        Ordinal #{0:X4} in module {1} not supported - creating NOP thunk", ordinal, GetModuleName());
                //thunk = _machine.CreateNopThunk("Unsupported ordinal thunk");
                thunk = _machine.CreateSystemThunk(() =>
                {
                    throw new InvalidOperationException(string.Format("Unsupported ordinal {0} (#{0:X4}) in module {1} invoked", ordinal, GetModuleName()));
                }, 0, false, "Unsupported ordinal thunk");
                _exceptionThunks.Add(ordinal, thunk);
                return thunk;
            }

            if (ep.procAddress==0)
            {
                ep.paramSize = CalculateSizeOfParametersOnStack(ep.MethodInfo);

                ep.procAddress = _machine.CreateSystemThunk(() =>
                {
                    InvokeEntryPoint(ep);
                }, ep.paramSize, ep.Attributes.PreserveAX, ep.MethodInfo.Name);
            }

            return ep.procAddress;
        }


        #endregion

        ushort SizeOfType16(Type pt)
        {
            if (pt == typeof(ushort))
            {
                return 2;
            }
            if (pt == typeof(short))
            {
                return 2;
            }
            if (pt == typeof(byte))
            {
                return 2;
            }
            if (pt == typeof(uint))
            {
                return 4;
            }
            if (pt == typeof(int))
            {
                return 4;
            }
            if (pt == typeof(string))
            {
                return 4;
            }
            if (pt == typeof(StringOrId))
            {
                return 4;
            }
            if (pt.IsByRef)
            {
                return 4;
            }
            if (pt == typeof(bool))
            {
                return 2;
            }
            if (pt == typeof(StringBuilder))
            {
                return 4;
            }
            if (pt == typeof(IntPtr))
            {
                return 4;
            }
            if (pt == typeof(Win32.POINT))
            {
                return 4;
            }
            if (MappedTypeAttribute.Is(pt))
            {
                var convertMethod = pt.GetMethod("To32");
                return SizeOfType16(convertMethod.GetParameters()[0].ParameterType);
            }
            throw new NotImplementedException(string.Format("Type not supported by thunking layer - {0}", pt));
        }

        ushort CalculateSizeOfParametersOnStack(MethodInfo mi)
        {
            var paramInfos = mi.GetParameters();

            ushort paramPos = 0;
            for (int i = 0; i < paramInfos.Length; i++)
            {
                var pi = paramInfos[i];

                paramPos += SizeOfType16(pi.ParameterType);
            }

            return paramPos;
        }

        class InvokeContext
        {
            public InvokeContext(Module32 This, EntryPoint ep)
            {
                _module = This;
                _machine = This._machine;
                _entryPoint = ep;
            }

            public int IndexOfParameter(string name)
            {
                var paramInfos = _entryPoint.MethodInfo.GetParameters();
                for (int i=0; i<paramInfos.Length; i++)
                {
                    if (paramInfos[i].Name == name)
                        return i;
                }
                return -1;
            }

            public void Invoke()
            {
                ushort ss = _machine.ss;
                ushort sp = _machine.sp;
                try
                {
                    var paramInfos = _entryPoint.MethodInfo.GetParameters();
                    _paramValues = new object[paramInfos.Length];

                    if (_entryPoint.Attributes.DebugBreak)
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                    }

                    _paramPos = (ushort)(_machine.sp + 4);
                    for (_currentParameterIndex = paramInfos.Length - 1; _currentParameterIndex >= 0; _currentParameterIndex--)
                    {
                        _paramValues[_currentParameterIndex] = ReadParamFromStack(null, paramInfos[_currentParameterIndex]);
                    }
                                                              
                    // Invoke pre invoke callbacks
                    if (_preInvokeCallbacks != null)
                    {
                        for (int i = 0; i < _preInvokeCallbacks.Count; i++)
                        {
                            _preInvokeCallbacks[i]();
                        }
                    }

                    if (_machine.logApiCalls)
                    {
                        var ip = _machine.ReadWord(_machine.ss, _machine.sp);
                        var cs = _machine.ReadWord(_machine.ss, (ushort)(_machine.sp + 2));
                        Log.Write("Invoking: [{2:X4}:{3:X4}] {0}({1}) = ", _entryPoint.MethodInfo.Name, string.Join(",", _paramValues.Select(x => x == null ? "null" : x.ToString())), cs, ip);
                    }

                    object retValue;
                    retValue = _entryPoint.MethodInfo.Invoke(_module, _paramValues);
                    
                    if (_machine.logApiCalls)
                        Log.WriteLine("{0}", retValue == null ? "<null>" : retValue.ToString());

                    // Invoke all the out/ref handlers
                    if (_postInvokeCallbacks != null)
                    {
                        for (int i = 0; i < _postInvokeCallbacks.Count; i++)
                        {
                            _postInvokeCallbacks[i]();
                        }
                    }

                    var retType = _entryPoint.MethodInfo.ReturnType;

                    SetReturnValue(retType, retValue);

                    if (_entryPoint.Attributes.DebugBreak16)
                    {
                        _machine.Debugger.BreakOnLeaveRoutine();
                    }
                }
                catch
                {
                    Log.WriteLine("Exception while invoking {0}", _entryPoint.MethodInfo);

                    var sb = new StringBuilder();
                    for (int i=0; i<_entryPoint.paramSize; i+=2)
                    {
                        try
                        {
                            sb.AppendFormat("{0:X4} ", _machine.ReadWord(ss, (ushort)(sp + 4 + i)));
                        }
                        catch
                        {
                            sb.Append("????");
                        }
                    }

                    Log.WriteLine("Parameters on stack: {0}", sb.ToString());

                    throw;
                }
            }


            void SetReturnValue(Type retType, object retValue)
            {
                if (retType == typeof(ushort))
                {
                    _machine.ax = (ushort)retValue;
                    return;
                }

                if (retType == typeof(short))
                {
                    _machine.ax = unchecked((ushort)(short)retValue);
                    return;
                }

                if (retType == typeof(uint))
                {
                    _machine.dxax = (uint)retValue;
                    return;
                }

                if (retType == typeof(int))
                {
                    _machine.dxax = unchecked((uint)(int)retValue);
                    return;
                }

                if (retType == typeof(bool))
                {
                    _machine.ax = (ushort)(((bool)retValue) ? 1U : 0U);
                    return;
                }

                if (retType == typeof(void))
                {
                    // nop
                    return;
                }

                if (MappedTypeAttribute.Is(retType))
                {
                    var convertMethod = retType.GetMethod("To16");
                    var retValue16 = convertMethod.Invoke(null, new object[] { retValue });
                    SetReturnValue(convertMethod.ReturnType, retValue16);
                    return;
                }

                throw new NotImplementedException(string.Format("Return type not supported by thunking layer - {0}", retType));
            }

            public void RegisterPostInvokeCallback(Action callback)
            {
                if (_postInvokeCallbacks == null)
                    _postInvokeCallbacks = new List<Action>();
                _postInvokeCallbacks.Add(callback);
            }

            public void RegisterPreInvokeCallback(Action callback)
            {
                if (_preInvokeCallbacks == null)
                    _preInvokeCallbacks = new List<Action>();
                _preInvokeCallbacks.Add(callback);
            }

            bool ShouldDestroy(ParameterInfo pi)
            {
                if (pi == null)
                    return false;
                return pi.GetCustomAttributes<DestroyedAttribute>().FirstOrDefault() != null;
            }

            object ReadParamFromStack(Type pt, ParameterInfo pi)
            {
                if (pt == null)
                    pt = pi.ParameterType;

                if (pt == typeof(ushort))
                {
                    var val = _machine.ReadWord(_machine.ss, _paramPos);
                    _paramPos += 2;
                    return val;
                }
                if (pt == typeof(short))
                {
                    var val = unchecked((short)_machine.ReadWord(_machine.ss, _paramPos));
                    _paramPos += 2;
                    return val;
                }
                if (pt == typeof(byte))
                {
                    var val = _machine.ReadByte(_machine.ss, _paramPos);
                    _paramPos += 2;
                    return val;
                }
                if (pt == typeof(uint))
                {
                    var val = _machine.ReadDWord(_machine.ss, _paramPos);
                    _paramPos += 4;
                    return val;
                }
                if (pt == typeof(int))
                {
                    var val = unchecked((int)_machine.ReadDWord(_machine.ss, _paramPos));
                    _paramPos += 4;
                    return val;
                }
                if (pt == typeof(string))
                {
                    var ptrOffset = _machine.ReadWord(_machine.ss, _paramPos);
                    var ptrSeg = _machine.ReadWord(_machine.ss, (ushort)(_paramPos + 2));
                    var val = _machine.ReadString(ptrSeg, ptrOffset);
                    _paramPos += 4;

                    if (pi!=null)
                    {
                        var fna = pi.GetCustomAttribute<FileNameAttribute>();
                        if (fna!= null)
                        {
                            val = _machine.PathMapper.MapGuestToHost(val, fna.ForWrite);
                        }
                    }
                    return val;
                }
                if (pt == typeof(StringOrId))
                {
                    var ptrOffset = _machine.ReadWord(_machine.ss, _paramPos);
                    var ptrSeg = _machine.ReadWord(_machine.ss, (ushort)(_paramPos + 2));
                    var val = new StringOrId(_machine, BitUtils.MakeDWord(ptrOffset, ptrSeg));
                    _paramPos += 4;
                    return val;
                }
                if (pt == typeof(bool))
                {
                    var val = _machine.ReadWord(_machine.ss, _paramPos) != 0;
                    _paramPos += 2;
                    return val;
                }
                if (pt == typeof(StringBuilder))
                {
                    // Capture stuff, we'll do it later
                    int paramIndex = _currentParameterIndex;

                    // Get input string pointer
                    var ptrOffset = _machine.ReadWord(_machine.ss, _paramPos);
                    var ptrSeg = _machine.ReadWord(_machine.ss, (ushort)(_paramPos + 2));
                    _paramPos += 4;
                    int capacity = 0;
                    bool isOut = pi.GetCustomAttribute<OutAttribute>() != null;
                    bool isIn = pi.GetCustomAttribute<InAttribute>() != null;
                    var fna = pi.GetCustomAttribute<FileNameAttribute>();

                    RegisterPreInvokeCallback(() =>
                    {
                        // Work out buffer size capacity
                        var bufsize = pi.GetCustomAttribute<BufSizeAttribute>();
                        int bufsizeParamIndex = paramIndex + bufsize.ParamDX;

                        var type = _paramValues[bufsizeParamIndex].GetType();
                        if (type == typeof(int))
                        {
                            capacity = (int)_paramValues[bufsizeParamIndex];
                        }
                        else if (type == typeof(ushort))
                        {
                            capacity = (int)(ushort)_paramValues[bufsizeParamIndex];
                        }
                        else if (type == typeof(nint))
                        {
                            capacity = (nint)_paramValues[bufsizeParamIndex];
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        // Create string builder
                        var sb = new StringBuilder(fna!=null ? 512 : capacity);

                        if (isIn)
                        {
                            var str = _machine.ReadString(ptrSeg, ptrOffset, (ushort)capacity);
                            if (fna!=null)
                                _machine.PathMapper.MapGuestToHost(str, fna.ForWrite);
                            sb.Append(str);
                        }

                        // Return the string builder
                        _paramValues[paramIndex] = sb;
                    });

                    if (isOut)
                    {
                        RegisterPostInvokeCallback(() =>
                        {
                            var str = _paramValues[paramIndex].ToString();
                            if (fna!=null)
                                str = _machine.PathMapper.MapHostToGuest(str, fna.ForWrite);

                            _machine.WriteString(ptrSeg, ptrOffset, str, (ushort)capacity);
                        });
                    }

                    return null;        // We'll fill it in later
                }
                if (pt == typeof(Win32.POINT))
                {
                    var x = unchecked((short)_machine.ReadWord(_machine.ss, _paramPos));
                    var y = unchecked((short)_machine.ReadWord(_machine.ss, (ushort)(_paramPos + 2)));
                    _paramPos += 4;
                    return new Win32.POINT(x, y);
                }
                if (pt == typeof(IntPtr))
                {
                    if (pi!=null)
                    {
                        if (pi.GetCustomAttribute<MustBeNullAttribute>() == null)
                        {
                            throw new NotImplementedException("IntPtr parameters must have MustBeNull attribute");
                        }
                    }

                    var ptrOffset = _machine.ReadWord(_machine.ss, _paramPos);
                    var ptrSeg = _machine.ReadWord(_machine.ss, (ushort)(_paramPos + 2));
                    _paramPos += 4;

                    if (ptrOffset!=0 || ptrSeg!=0)
                    {
                        throw new NotImplementedException("Non-null IntPtr parameter passed");
                    }

                    return IntPtr.Zero;
                }
                if (MappedTypeAttribute.Is(pt))
                {
                    var convertMethod = pt.GetMethod("To32");

                    // Read the 16-bit value
                    var val16 = ReadParamFromStack(convertMethod.GetParameters()[0].ParameterType, null);

                    // Convert it
                    var val32 = convertMethod.Invoke(null, new object[] { val16 });

                    if (ShouldDestroy(pi))
                    {
                        RegisterPostInvokeCallback(() =>
                        {
                            var destroyMethod = pt.GetMethod("Destroy");
                                destroyMethod.Invoke(null, new object[] { val16 });
                        });
                    }

                    return val32;
                }
                if (pt.IsByRef)
                {
                    var underlyingType = pt.GetElementType();
                    if (underlyingType.IsValueType)
                    {
                        var ptr = _machine.ReadDWord(_machine.ss, _paramPos);

                        object val;
                        if (pi == null || !pi.IsOut)
                        {
                            if (MappedTypeAttribute.Is(underlyingType))
                            {
                                var convertMethod = underlyingType.GetMethod("To32");
                                val = _machine.ReadStruct(convertMethod.GetParameters()[0].ParameterType, ptr);
                                val = convertMethod.Invoke(null, new object[] { val });
                            }
                            else
                            {
                                val = _machine.ReadStruct(underlyingType, ptr);
                            }
                        }
                        else
                        {
                            val = Activator.CreateInstance(underlyingType);
                        }


                        _paramPos += 4;

                        if (pi.GetCustomAttribute<InAttribute>() == null)
                        {
                            int index = _currentParameterIndex;
                            RegisterPostInvokeCallback(() =>
                            {
                                val = _paramValues[index];
                                if (MappedTypeAttribute.Is(underlyingType))
                                {
                                    var convertMethod = underlyingType.GetMethod("To16");
                                    val = convertMethod.Invoke(null, new object[] { val });
                                }
                                if (ptr!=0)
                                {
                                    _machine.WriteStruct(ptr, val);
                                }
                            });
                        }

                        return val;
                    }
                }

                throw new NotImplementedException(string.Format("Parameter type not supported by thunking layer - {0}", pt));
            }

            object[] _paramValues;
            List<Action> _postInvokeCallbacks;
            List<Action> _preInvokeCallbacks;
            EntryPoint _entryPoint;
            Module32 _module;
            Machine _machine;
            int _currentParameterIndex;
            ushort _paramPos;
        }

        void InvokeEntryPoint(EntryPoint ep)
        {
            if (ep.Attributes.PreserveAX)
            {
                // POP AX
                _machine.ax = _machine.ReadWord(_machine.ss, _machine.sp);
                _machine.sp += 2;
            }

            var ctx = new InvokeContext(this, ep);
            ctx.Invoke();

        }
    }


}


