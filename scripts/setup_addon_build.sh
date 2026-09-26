#!/usr/bin/env bash
set -e

# Addons compile their own copy of Dear ImGui and share contexts with cimgui, so build them
# from the same cimgui commit as the cimgui natives (see cmake.yml).
git clone --recursive https://github.com/JunaMeinhold/cimgui.git
git -C cimgui checkout 576040c4bd36894113d1c1c6ae52c4ffd928cf28
git -C cimgui submodule update --init --recursive
