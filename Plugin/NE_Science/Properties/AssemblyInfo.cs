﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("NE Orbital Science Plugin")]
[assembly: AssemblyDescription("KSP Plugin")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Nehemiah Inc")]
[assembly: AssemblyProduct("NE Orbital Science Plugin")]
[assembly: AssemblyCopyright("GNU General Public License V3")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("dc81a219-921d-4a90-9efc-d600c01da454")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("0.9.0.40")]
#if DEBUG
[assembly: AssemblyInformationalVersion("0.9.0 Debug for KSP 1.8+")]
#else
[assembly: AssemblyInformationalVersion("0.8.3 for KSP 1.7+")]
#endif
[assembly: AssemblyFileVersion("0.9.0.40")]
[assembly: KSPAssembly("NE_Science", 0, 90)]
