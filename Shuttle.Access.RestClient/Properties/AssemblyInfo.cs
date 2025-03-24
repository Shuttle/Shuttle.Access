using System.Reflection;
using System.Runtime.InteropServices;

#if NETSTANDARD
[assembly: AssemblyTitle(".NET Standard")]
#endif

#if NET6_0_OR_GREATER
[assembly: AssemblyTitle(".NET Unified Platform")]
#endif

[assembly: AssemblyVersion("7.0.0.0")]
[assembly: AssemblyCopyright("Copyright (c) 2025, Eben Roux")]
[assembly: AssemblyProduct("Shuttle.Access.RestClient")]
[assembly: AssemblyCompany("Eben Roux")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyInformationalVersion("7.0.0")]
[assembly: ComVisible(false)]