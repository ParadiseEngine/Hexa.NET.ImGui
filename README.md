[![Build Native Libraries](https://github.com/ParadiseEngine/Paradise.ImGui/actions/workflows/natives.yml/badge.svg)](https://github.com/ParadiseEngine/Paradise.ImGui/actions/workflows/natives.yml)
[![Publish NuGet Packages](https://github.com/ParadiseEngine/Paradise.ImGui/actions/workflows/push-nuget.yml/badge.svg)](https://github.com/ParadiseEngine/Paradise.ImGui/actions/workflows/push-nuget.yml)

# Paradise.ImGui

> **Not an official Hexa.NET package.** Paradise.ImGui is a ParadiseEngine fork of [Hexa.NET.ImGui](https://github.com/HexaEngine/Hexa.NET.ImGui) by Juna Meinhold. It is not affiliated with or endorsed by HexaEngine. Report issues with these packages [here](https://github.com/ParadiseEngine/Paradise.ImGui/issues), not upstream.

This README covers only what the fork changes. For the library itself (features, API usage, safe/unsafe and UTF-8 string overloads, examples, screenshots, credits) read the **[upstream README](https://github.com/HexaEngine/Hexa.NET.ImGui#readme)**. Everything there applies here, except that you install the package IDs below and the backends packages are not available.

## Differences from upstream

- **Package names**: published as `Paradise.ImGui*`, not `Hexa.NET.*`; see [Naming](#naming).
- **Native libraries**: built in CI at `-O3` from pinned sources; none are committed. See [Native libraries](#native-libraries).
- **Browser**: `Paradise.ImGui` supports `browser-wasm`. See [Browser (WebAssembly)](#browser-webassembly).
- **ImPlot3D**: the binding is regenerated for the ImPlot3D 0.4 natives it ships with.
- **ImPlot/ImPlot3D specs**: calls that omit the spec pass the C++ default (`ImPlotSpec.Default`/`ImPlot3DSpec.Default`) instead of an all-zero struct.
- **Backends**: upstream's `Hexa.NET.ImGui.Backends*` are neither built nor published; bring your own renderer (Paradise Engine renders ImGui through WebGPU itself).

## Naming

Upstream's [LICENSE.txt](https://github.com/HexaEngine/Hexa.NET.ImGui/blob/main/LICENSE.txt) (copied unchanged as [LICENSE.txt](LICENSE.txt)) is MIT plus a **Naming Clause**:

> The use of the "Hexa.NET" name, prefix, or any derivative thereof in the naming of derivative projects, redistributed packages, or modified versions is not permitted without explicit written permission from the original author (Juna Meinhold). Any usage of the Hexa.NET name outside of the official projects must clearly indicate that it is not an official Hexa.NET package.

This fork is a modified, redistributed derivative without that permission, so:

- **Repository**: `ParadiseEngine/Paradise.ImGui`, with no `Hexa.NET` in the name.
- **Package IDs**: `Hexa.NET.ImGui` becomes `Paradise.ImGui` and each addon `Hexa.NET.ImX` becomes `Paradise.ImGui.X`. The IDs are set in [`Directory.Build.targets`](Directory.Build.targets) so upstream's project files stay unchanged and merge cleanly; any other `Hexa.NET.*` project is marked non-packable there, so nothing can ship under upstream's name.

  | Upstream package | This fork |
  |---|---|
  | `Hexa.NET.ImGui` | [`Paradise.ImGui`](https://www.nuget.org/packages/Paradise.ImGui) |
  | `Hexa.NET.ImGuizmo` | [`Paradise.ImGui.Guizmo`](https://www.nuget.org/packages/Paradise.ImGui.Guizmo) |
  | `Hexa.NET.ImNodes` | [`Paradise.ImGui.Nodes`](https://www.nuget.org/packages/Paradise.ImGui.Nodes) |
  | `Hexa.NET.ImPlot` | [`Paradise.ImGui.Plot`](https://www.nuget.org/packages/Paradise.ImGui.Plot) |
  | `Hexa.NET.ImPlot3D` | [`Paradise.ImGui.Plot3D`](https://www.nuget.org/packages/Paradise.ImGui.Plot3D) |
  | `Hexa.NET.ImGui.Backends*` | not published |

- **Assemblies and namespaces keep `Hexa.NET.*`** (`using Hexa.NET.ImGui;`), so code written against upstream compiles unchanged. These are identifiers inside upstream's source, not the name of this project or its packages; as the clause's second sentence requires, every package description and this README state that the packages are not official Hexa.NET packages.
- **Unlisted IDs**: 3.1.0 was published as `Paradise.Hexa.NET.*`, a derivative of the Hexa.NET prefix that the clause forbids; 3.1.1 used `Paradise.ImGuizmo`, `Paradise.ImNodes`, `Paradise.ImPlot`, `Paradise.ImPlot3D` and `Paradise.ImGui.Backends*`. Both are unlisted; use the IDs above at 3.1.2 or later.

Upstream's official packages are [Hexa.NET.ImGui](https://www.nuget.org/packages/Hexa.NET.ImGui/), [Hexa.NET.ImGuizmo](https://www.nuget.org/packages/Hexa.NET.ImGuizmo/), [Hexa.NET.ImNodes](https://www.nuget.org/packages/Hexa.NET.ImNodes/) and [Hexa.NET.ImPlot](https://www.nuget.org/packages/Hexa.NET.ImPlot/).

## Installation

```bash
dotnet add package Paradise.ImGui          # core
dotnet add package Paradise.ImGui.Guizmo   # ImGuizmo
dotnet add package Paradise.ImGui.Nodes    # ImNodes
dotnet add package Paradise.ImGui.Plot     # ImPlot
dotnet add package Paradise.ImGui.Plot3D   # ImPlot3D
```

```csharp
using Hexa.NET.ImGui;
```

From here, follow the [upstream README](https://github.com/HexaEngine/Hexa.NET.ImGui#readme) and the [examples](Examples/).

## Browser (WebAssembly)

`Paradise.ImGui` (the core package only) runs on `browser-wasm` with the Mono interpreter and with `RunAOTCompilation`. Referencing the package from a `Microsoft.NET.Sdk.WebAssembly` app links `native/browser-wasm/cimgui.a` into `dotnet.wasm` through a `NativeFileReference`, so the app build needs the `wasm-tools` workload and relinks the runtime. No rendering backend is included; see [ExampleBrowserWasm](Examples/ExampleBrowserWasm/) for a headless smoke check of the binding.

- `scripts/build_cimgui_wasm.sh` builds the archive with the Emscripten on `PATH`, which must match the workload's version (3.1.56 for .NET 9 and 10). It appends a generated name table (`scripts/wasm/gen_cimgui_exports.py`) because the browser runtime cannot look up native symbols by name.
- After regenerating the binding, rerun `dotnet run scripts/wasm/GenerateInterpToNativeSignatures.cs -- Hexa.NET.ImGui/bin/Release/net10.0/Hexa.NET.ImGui.dll Hexa.NET.ImGui Hexa.NET.ImGui/Browser/InterpToNativeSignatures.cs`. The interpreter aborts on a native call whose signature that file lacks.
- The archive is single-threaded and built without FreeType, so `ImGuiFreeType` functions are unavailable in the browser.

## Native libraries

Native libraries are not committed. `.github/workflows/natives.yml` builds every published one (cimgui and the addons, shared and static, and `browser-wasm`) at `-O3` from the source commits pinned in the `cmake*.yml` workflows and `scripts/`, then uploads the project `native/` folders as one `natives` artifact. It runs on pull requests and pushes that change those inputs, and `push-nuget.yml` runs it before packing.

- For local builds, examples, and tests, run `scripts/fetch_natives.sh` (needs an authenticated `gh`). It downloads the latest successful `natives` artifact from `HexaGen-Mainline`, or pass a run id. Artifacts expire; if none is left, dispatch `natives.yml`.
- `scripts/place_natives.py` maps artifacts to projects through `hexa-workflows/*/hexa-workflows.json`. It fails if any `native\<rid>\*` pattern a project packs has no files, because an empty glob would silently drop that runtime from the package.
- When syncing from upstream, keep `*/native/` deleted. Upstream still commits binaries, so merges conflict on those files.

## Publishing

Pushing a `v*` tag runs `push-nuget.yml`, which builds the natives, packs with the tag's version, and publishes through nuget.org Trusted Publishing as the `NUGET_USER` repository variable.

## License

MIT with the Hexa.NET Naming Clause, inherited unchanged from upstream; see [LICENSE.txt](LICENSE.txt). Copyright (c) 2022 Juna Meinhold. Dear ImGui, cimgui and the addon libraries are under their own licenses.
