[![Build ImGui and Addons Libraries](https://github.com/HexaEngine/Hexa.NET.ImGui/actions/workflows/cmake.yml/badge.svg)](https://github.com/HexaEngine/Hexa.NET.ImGui/actions/workflows/cmake.yml)
[![Build ImGui Backends](https://github.com/HexaEngine/Hexa.NET.ImGui/actions/workflows/cmake-backends.yml/badge.svg)](https://github.com/HexaEngine/Hexa.NET.ImGui/actions/workflows/cmake-backends.yml)
[![Publish NuGet Packages](https://github.com/HexaEngine/Hexa.NET.ImGui/actions/workflows/push-nuget.yml/badge.svg)](https://github.com/HexaEngine/Hexa.NET.ImGui/actions/workflows/push-nuget.yml)

# Hexa.NET.ImGui

Welcome to Hexa.NET.ImGui! This custom wrapper is designed to be a high-performance, API-compatible alternative to ImGuiNET, offering enhanced speed, additional functionality, and comprehensive access to ImGui's internal structures. With optimizations that bring near C performance and significantly reduced startup times, Hexa.NET.ImGui provides the best of both worlds: the power of C and the productivity of C#.

#### The code generator in use [HexaGen](https://github.com/HexaEngine/HexaGen).

## Features

- **Comprehensive Wrapper**: Integrates the core Dear ImGui library along with essential addons such as ImGuizmo, ImNodes, and ImPlot.
- **Backends**: Includes a collection of native backends in the Hexa.NET.ImGui.Backends package. (incl. Android, Win32, OSX, D3D9, D3D10, D3D11, D3D12, Metal, OpenGL2, OpenGL3, Vulkan and extra packages for SDL2 and GLFW)
- **FreeType Fonts**: Allows better text rendering and colored fonts to be loaded.
- **Docking Support**: Full docking branch integration with access to DockBuilder APIs (via ImGuiP for internals)
- **Multi Viewport Support**: Enables seamless multi-viewport rendering for advanced UI scenarios.
- **Active Development**: Regular updates and improvements to ensure compatibility with the latest Dear ImGui features and .NET advancements.
- **Trustworthy Builds**: Native libraries are built using GitHub Actions for added trustworthiness and can be found [here](https://github.com/HexaEngine/Hexa.NET.ImGui/actions).
- **Open Source**: The source code for the native libraries is public and can be reviewed by anyone.
- **Access to Internals**: Allows users to access Dear ImGui internals for advanced customization and functionality.
- **Drop in replacement for ImGuiNET** Adapt to the library with minimal effort by simply changing the namespace.
- **High performance** Using a static function table, all API calls are faster and startup time is reduced.
- **Optimized String Handling**: Overloads that bypass UTF-8 encoding and avoid allocations.
- **Wide range .NET support**: Supported versions net8.0, net7.0, netstandard2.1, netstandard2.0

## Community
- Discord: [https://discord.gg/VawN5d8HMh](https://discord.gg/VawN5d8HMh)

## Packages

Hexa.NET.ImGui is divided into four different packages to provide modularity and flexibility. You can choose to install only the packages you need:

- [Hexa.NET.ImGui](https://www.nuget.org/packages/Hexa.NET.ImGui/)
- [Hexa.NET.ImGuizmo](https://www.nuget.org/packages/Hexa.NET.ImGuizmo/)
- [Hexa.NET.ImNodes](https://www.nuget.org/packages/Hexa.NET.ImNodes/)
- [Hexa.NET.ImPlot](https://www.nuget.org/packages/Hexa.NET.ImPlot/)

## Releated Projects

- [Hexa.NET.ImGui.Widgets](https://github.com/HexaEngine/Hexa.NET.ImGui.Widgets)
  - A small framework making it easier working with ImGui in C# build on top of Hexa.NET.ImGui. It allows to capsulate Widgets in classes and manage them centralized, supporting dialogs that block other windows (incl. OpenFileDialog SaveFileDialog and more)

## Getting Started

To get started with Hexa.NET.ImGui, follow these steps:

1. **Install the NuGet packages**:

    For the core library:
    ```bash
    dotnet add package Hexa.NET.ImGui
    ```

    For ImGuizmo addon:
    ```bash
    dotnet add package Hexa.NET.ImGuizmo
    ```

    For ImNodes addon:
    ```bash
    dotnet add package Hexa.NET.ImNodes
    ```

    For ImPlot addon:
    ```bash
    dotnet add package Hexa.NET.ImPlot
    ```

2. **Initialize the library** in your project:
    ```csharp
    using Hexa.NET.ImGui;
    // Your initialization code here
    ```

3. **Explore the demos** to see the library in action.

### Usage Example

For a comprehensive example of how to use the library, refer to the [ExampleGFWLD3D11 project](https://github.com/HexaEngine/Hexa.NET.ImGui/tree/main/Examples/ExampleGLFWD3D11) [ExampleSDL3OpenGL3 project](https://github.com/HexaEngine/Hexa.NET.ImGui/tree/main/Examples/ExampleSDL3OpenGL3/).

### Browser (WebAssembly)

`Hexa.NET.ImGui` (the core package only) runs on `browser-wasm` with the Mono interpreter and with `RunAOTCompilation`. Referencing the package from a `Microsoft.NET.Sdk.WebAssembly` app links `native/browser-wasm/cimgui.a` into `dotnet.wasm` through a `NativeFileReference`, so the app build needs the `wasm-tools` workload and relinks the runtime. No rendering backend is included; see [ExampleBrowserWasm](Examples/ExampleBrowserWasm/) for a headless smoke check of the binding.

- `scripts/build_cimgui_wasm.sh` builds the archive with the Emscripten on `PATH`, which must match the workload's version (3.1.56 for .NET 9 and 10). It appends a generated name table (`scripts/wasm/gen_cimgui_exports.py`) because the browser runtime cannot look up native symbols by name.
- After regenerating the binding, rerun `dotnet run scripts/wasm/GenerateInterpToNativeSignatures.cs -- Hexa.NET.ImGui/bin/Release/net10.0/Hexa.NET.ImGui.dll Hexa.NET.ImGui Hexa.NET.ImGui/Browser/InterpToNativeSignatures.cs`. The interpreter aborts on a native call whose signature that file lacks.
- The archive is single-threaded and built without FreeType, so `ImGuiFreeType` functions are unavailable in the browser.

### Native Libraries

Native libraries are not committed. `.github/workflows/natives.yml` builds all of them (cimgui, the addons, the backends, shared and static, and `browser-wasm`) at `-O3` from the source commits pinned in the `cmake*.yml` workflows and `scripts/`, then uploads the project `native/` folders as one `natives` artifact. It runs on pull requests and pushes that change those inputs, and `push-nuget.yml` runs it before packing.

- For local builds, examples, and tests, run `scripts/fetch_natives.sh` (needs an authenticated `gh`). It downloads the latest successful `natives` artifact from `HexaGen-Mainline`, or pass a run id. Artifacts expire; if none is left, dispatch `natives.yml`.
- `scripts/place_natives.py` maps artifacts to projects through `hexa-workflows/*/hexa-workflows.json`. It fails if any `native\<rid>\*` pattern a project packs has no files, because an empty glob would silently drop that runtime from the package.
- When syncing from upstream, keep `*/native/` deleted. Upstream still commits binaries, so merges conflict on those files.

### Using the Flexible and Optimized API

Hexa.NET.ImGui supports both safe and unsafe API calls, along with optimized string handling to bypass UTF-8 encoding and avoid allocations. Here are some examples:

 1. Safe API Call:

```cs
ImGui.Text("A normal C# string");
```

 2. Unsafe API Call:
```cs
unsafe
{
    byte* pText = (byte*)Marshal.StringToHGlobalAnsi("A string from an unsafe pointer").ToPointer();
    ImGui.Text(pText);
    Marshal.FreeHGlobal((IntPtr)pText);
}
```

 3. Optimized String Handling:
```cs
ImGui.Text("A C# string"u8);
```
**This overload bypasses UTF-8 encoding and avoids allocations, providing a highly optimized way to render text.**

## Projects Using Hexa.NET.ImGui

[HexaEngine](https://github.com/HexaEngine/HexaEngine)

<img src="https://github.com/user-attachments/assets/b54145fe-5bd5-4998-b36f-24efe8345aba" alt="HexaEngine Editor" width="400"/>

## Screenshots

### Main Interface
![Screenshot 2023-05-23 163105](https://github.com/JunaMeinhold/HexaEngine.ImGui/assets/46632782/e15288c5-e0f1-4feb-8589-abd2ca92fffb)

### Multi Viewport Support
![Screenshot 2023-07-07 153108](https://github.com/JunaMeinhold/HexaEngine.ImGui/assets/46632782/efb715f8-2dee-4bd2-8fa5-d1bc2195129a)

## Contributing

Contributions are welcome! If you have ideas for improvements or new features, feel free to submit a pull request or open an issue.

## Credits

- [ImGui.NET](https://github.com/ImGuiNET/ImGui.NET/) for the JSON parsing of the cimgui metadata. 
- [cimgui](https://github.com/cimgui/cimgui) for the c interface. 
- [Dear ImGui](https://github.com/ocornut/imgui) 

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/HexaEngine/Hexa.NET.ImGui/blob/main/LICENSE.txt) file for more details.

-----
<div align="center" id="sponsor-section">

## Thanks to our sponsors!

[![Sponsors](https://raw.githubusercontent.com/HexaEngine/Sponsors/refs/heads/main/sponsors.svg)](https://ko-fi.com/junameinhold)

</div>
