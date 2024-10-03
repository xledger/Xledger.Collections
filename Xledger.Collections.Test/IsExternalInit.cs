#if NETFRAMEWORK
namespace System.Runtime.CompilerServices; 
/// <summary>
/// Allows compiling <c>init</c> properties for .NET Framework.
/// https://developercommunity.visualstudio.com/t/error-cs0518-predefined-type-systemruntimecompiler/1244809
/// https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
/// https://stackoverflow.com/questions/62648189/testing-c-sharp-9-0-in-vs2019-cs0518-isexternalinit-is-not-defined-or-imported
/// </summary>
[ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
static class IsExternalInit { }
#endif
