using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Win3mu Runtime")]
[assembly: AssemblyDescription("Windows 3 Emulator")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Topten Software")]
[assembly: AssemblyProduct("Win3mu")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("cb3255e6-9b13-42bb-9d8d-1c1d7a757e94")]

[assembly: Obfuscation(Feature = "encrypt symbol names with password cantabile_rules", Exclude = false)]
[assembly: Obfuscation(Feature = "debug [secure]", Exclude = false)]
[assembly: Obfuscation(Feature = "merge with Sharp86.dll", Exclude = false)]
[assembly: Obfuscation(Feature = "merge with Sharp86DebuggerCore.dll", Exclude = false)]
[assembly: Obfuscation(Feature = "merge with Sharp86TextGuiDebugger.dll", Exclude = false)]
[assembly: Obfuscation(Feature = "merge with ConFrames.dll", Exclude = false)]
[assembly: Obfuscation(Feature = "merge with Win3muCore.dll", Exclude = false)]
[assembly: Obfuscation(Feature = "merge with PetaJson.dll", Exclude = false)]
[assembly: Obfuscation(Feature = "Apply to type * when has_attribute('Win3muCore.MappedTypeAttribute'): Apply to member To16: renaming", Exclude = true)]
[assembly: Obfuscation(Feature = "Apply to type * when has_attribute('Win3muCore.MappedTypeAttribute'): Apply to member To32: renaming", Exclude = true)]
[assembly: Obfuscation(Feature = "Apply to type * when has_attribute('Win3muCore.MappedTypeAttribute'): Apply to member Destroy: renaming", Exclude = true)]
[assembly: Obfuscation(Feature = "Apply to type *: Apply to member * when has_attribute('Win3muCore.EntryPointAttribute'): renaming", Exclude = true)]
[assembly: Obfuscation(Feature = "Apply to type *: Apply to member * when has_attribute('Sharp86.DebuggerHelpAttribute'): renaming", Exclude = true)]

