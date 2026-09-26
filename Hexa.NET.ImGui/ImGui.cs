#nullable disable

namespace Hexa.NET.ImGui
{
    using HexaGen.Runtime;
    using System.Diagnostics;

    public static class ImGuiConfig
    {
        public static bool AotStaticLink;
    }

    public static unsafe partial class ImGui
    {
        static ImGui()
        {
#if NET5_0_OR_GREATER
            if (OperatingSystem.IsBrowser())
            {
                InitApi(new BrowserNativeContext());
                return;
            }
#endif
            if (ImGuiConfig.AotStaticLink)
            {
                InitApi(new NativeLibraryContext(Process.GetCurrentProcess().MainModule!.BaseAddress));
            }
            else
            {
                InitApi(new NativeLibraryContext(LibraryLoader.LoadLibrary(GetLibraryName, null)));
            }
        }

        public static string GetLibraryName()
        {
            return "cimgui";
        }

        public const nint ImDrawCallbackResetRenderState = -8;
    }
}