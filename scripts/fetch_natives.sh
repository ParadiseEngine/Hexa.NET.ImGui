#!/usr/bin/env bash
# Downloads the native libraries into the project native/ folders for local builds and examples.
#
# Usage: scripts/fetch_natives.sh [run-id]
#
# Without a run id, uses the latest successful "Build Native Libraries" run on HexaGen-Mainline.
# Requires an authenticated GitHub CLI (gh). Artifacts expire; when none is left, dispatch
# natives.yml (gh workflow run natives.yml --ref HexaGen-Mainline) and rerun this script.
set -euo pipefail

repo="${NATIVES_REPO:-ParadiseEngine/Hexa.NET.ImGui}"
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
run_id="${1:-}"

if [ -z "$run_id" ]; then
    run_id="$(gh run list --repo "$repo" --workflow natives.yml --branch HexaGen-Mainline \
        --status success --limit 1 --json databaseId --jq '.[0].databaseId // empty')"
    if [ -z "$run_id" ]; then
        echo "no successful natives.yml run on HexaGen-Mainline in $repo" >&2
        exit 1
    fi
fi

echo "downloading natives from $repo run $run_id"
download="$(mktemp -d)"
trap 'rm -rf "$download"' EXIT
gh run download "$run_id" --repo "$repo" --name natives --dir "$download"
# Replace each native/ tree so files the artifact no longer contains cannot be packed.
for native in "$download"/*/native; do
    project="$(basename "$(dirname "$native")")"
    rm -rf "${root:?}/$project/native"
    cp -R "$native" "$root/$project/native"
done
mkdir -p "$root/build/no-artifacts"
python3 "$root/scripts/place_natives.py" "$root/build/no-artifacts" "$root"
