#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$repo_root"
[[ $(git -C external/libjpeg-turbo rev-parse HEAD) == 9217719d3a58633923b096af4c1d50d304768a64 ]]
[[ $(sha256sum external/jpeg-gray-bench-build/libjpeg.a | cut -d' ' -f1) == faeac65984bc945a3acf12abffadf9814ba64b91d49a0b10bc6d2fe191b55d24 ]]
# Reuse the prepared SIMD archive and preserve the frozen Gray8/16 workers.
cc -std=c11 -O3 -ffp-contract=off -fno-math-errno -Wall -Wextra -Werror \
    -Iexternal/libjpeg-turbo/src -Iexternal/jpeg-gray-bench-build \
    tools/WarmReference/libjpeg-gray-restart-stage.c external/jpeg-gray-bench-build/libjpeg.a \
    -lm -o external/warm-reference/libjpeg-gray-restart-stage
