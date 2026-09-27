#!/usr/bin/env bash
set -e

# Addons compile their own copy of Dear ImGui and share contexts with cimgui, so build them
# from the same cimgui commit as the cimgui natives (see cmake.yml).
CIMGUI_REF=576040c4bd36894113d1c1c6ae52c4ffd928cf28

git init --quiet cimgui
git -C cimgui fetch --quiet --depth 1 https://github.com/JunaMeinhold/cimgui.git "$CIMGUI_REF"
git -C cimgui checkout --quiet FETCH_HEAD
git -C cimgui submodule update --init --recursive --depth 1
