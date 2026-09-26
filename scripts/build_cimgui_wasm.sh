#!/usr/bin/env bash
# Builds Hexa.NET.ImGui/native/browser-wasm/cimgui.a with the Emscripten on PATH.
#
# The archive is linked into dotnet.wasm by the .NET wasm workload, so Emscripten must match the
# workload's version (3.1.56 for .NET 9 and 10). Objects built by another version can fail to
# link or break at runtime.
set -euo pipefail

CIMGUI_REPO="${CIMGUI_REPO:-https://github.com/JunaMeinhold/cimgui.git}"
# The cimgui commit the checked-in binding (Generator/cimgui) was generated from (Dear ImGui 1.92.9b
# docking); it only adds *_Construct exports the binding does not use.
CIMGUI_REF="${CIMGUI_REF:-576040c4bd36894113d1c1c6ae52c4ffd928cf28}"

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
work="${CIMGUI_WASM_WORK_DIR:-$root/build/cimgui-wasm}"
out="$root/Hexa.NET.ImGui/native/browser-wasm"

emcc --version | head -n 1

if [ ! -d "$work/cimgui/.git" ]; then
    git clone "$CIMGUI_REPO" "$work/cimgui"
fi
git -C "$work/cimgui" fetch --quiet origin "$CIMGUI_REF" || true
git -C "$work/cimgui" checkout --quiet "$CIMGUI_REF"
git -C "$work/cimgui" submodule update --init --recursive

# Freetype stays off: consumers would have to link an Emscripten freetype port as well, and
# stb_truetype covers the default font loader.
emcmake cmake -S "$work/cimgui" -B "$work/build" \
    -DCMAKE_BUILD_TYPE=Release \
    -DCMAKE_C_FLAGS_RELEASE="-O3 -DNDEBUG" \
    -DCMAKE_CXX_FLAGS_RELEASE="-O3 -DNDEBUG" \
    -DIMGUI_STATIC=ON \
    -DIMGUI_WCHAR32=ON \
    -DCIMGUI_VARGS0=1
cmake --build "$work/build" --parallel

emnm "$work/build/cimgui.a" > "$work/cimgui.nm"
python3 "$root/scripts/wasm/gen_cimgui_exports.py" \
    "$work/cimgui/cimgui.h" "$work/cimgui.nm" "$work/cimgui_wasm_exports.c"
emcc -O3 -DNDEBUG -DIMGUI_USE_WCHAR32 -DCIMGUI_VARGS0 -I "$work/cimgui" \
    -c "$work/cimgui_wasm_exports.c" -o "$work/cimgui_wasm_exports.o"

mkdir -p "$out"
cp "$work/build/cimgui.a" "$out/cimgui.a"
emar rs "$out/cimgui.a" "$work/cimgui_wasm_exports.o"
ls -l "$out/cimgui.a"
