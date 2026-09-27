#if NET5_0_OR_GREATER
#nullable disable

namespace Hexa.NET.ImGui
{
    using HexaGen.Runtime;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Resolves cimgui exports on browser-wasm, where the runtime cannot look up native symbols.
    /// </summary>
    /// <remarks>
    /// Mono on browser-wasm links native code statically into dotnet.wasm and resolves only
    /// functions declared with [DllImport], so <see cref="LibraryLoader"/> and
    /// NativeLibrary.GetExport cannot find cimgui's exports. The browser-wasm cimgui.a carries a
    /// generated name table behind <c>cimgui_wasm_get_proc_address</c>, and this single
    /// DllImport is the only symbol the runtime needs to know. The app must link that archive
    /// (the package's buildTransitive targets add it as a NativeFileReference).
    /// </remarks>
    public sealed unsafe class BrowserNativeContext : INativeContext
    {
        [DllImport("cimgui", EntryPoint = "cimgui_wasm_get_proc_address")]
        private static extern nint GetProcAddressNative(byte* name);

        [DynamicDependency(nameof(InterpToNativeSignatures.Keep), typeof(InterpToNativeSignatures))]
        public BrowserNativeContext()
        {
        }

        public nint GetProcAddress(string procName)
        {
            return TryGetProcAddress(procName, out nint address)
                ? address
                : throw new EntryPointNotFoundException($"cimgui export '{procName}' is not linked.");
        }

        public bool TryGetProcAddress(string procName, out nint procAddress)
        {
            int length = Encoding.UTF8.GetByteCount(procName);
            byte* name = stackalloc byte[length + 1];
            Encoding.UTF8.GetBytes(procName, new Span<byte>(name, length));
            name[length] = 0;
            procAddress = GetProcAddressNative(name);
            return procAddress != 0;
        }

        public bool IsExtensionSupported(string extensionName)
        {
            return false;
        }

        public void Dispose()
        {
        }
    }
}
#endif
