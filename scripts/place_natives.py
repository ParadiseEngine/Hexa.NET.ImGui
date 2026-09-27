#!/usr/bin/env python3
"""Place native build artifacts into the project native/ folders and verify nothing is missing.

Usage: place_natives.py <artifacts-dir> <repo-root>

<artifacts-dir> holds one directory per artifact, named <library>-<os>-<arch> as uploaded by the
native build workflows (e.g. cimgui-linux-x64, cimgui-static-win-x64, cimgui-browser-wasm). The
library part is looked up in hexa-workflows/*/hexa-workflows.json. Afterwards every
native\\<rid>\\*.<ext> pattern the package projects pack must match at least one file, because
an unmatched glob silently yields a package without that runtime.
"""

import glob
import json
import os
import re
import shutil
import sys

PACKED_NATIVE = re.compile(r'<None Include="(native\\[^"]+)"')


def load_destinations(repo_root: str) -> dict:
    destinations = {}
    for manifest in sorted(glob.glob(os.path.join(repo_root, "hexa-workflows", "*", "hexa-workflows.json"))):
        with open(manifest, encoding="utf-8") as f:
            destinations.update(json.load(f))
    return destinations


def place(artifacts_dir: str, repo_root: str) -> int:
    destinations = load_destinations(repo_root)
    placed = 0
    for entry in sorted(os.listdir(artifacts_dir)):
        source = os.path.join(artifacts_dir, entry)
        if not os.path.isdir(source):
            continue
        parts = entry.split("-")
        library, rid = "-".join(parts[:-2]), "-".join(parts[-2:])
        if library not in destinations:
            sys.exit(f"artifact '{entry}' has no destination in hexa-workflows/*/hexa-workflows.json")
        target = os.path.join(repo_root, destinations[library], rid)
        os.makedirs(target, exist_ok=True)
        for name in os.listdir(source):
            shutil.copy2(os.path.join(source, name), os.path.join(target, name))
            placed += 1
    return placed


def verify(repo_root: str) -> list:
    missing = []
    for project in sorted(glob.glob(os.path.join(repo_root, "*", "*.csproj"))):
        with open(project, encoding="utf-8-sig") as f:
            patterns = PACKED_NATIVE.findall(f.read())
        project_dir = os.path.dirname(project)
        for pattern in patterns:
            if not glob.glob(os.path.join(project_dir, *pattern.split("\\"))):
                missing.append(os.path.relpath(os.path.join(project_dir, *pattern.split("\\")), repo_root))
    return missing


def main() -> None:
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    artifacts_dir, repo_root = sys.argv[1:3]
    print(f"placed {place(artifacts_dir, repo_root)} files")
    missing = verify(repo_root)
    if missing:
        sys.exit("packed native patterns with no files:\n  " + "\n  ".join(missing))
    print("every packed native pattern has files")


if __name__ == "__main__":
    main()
