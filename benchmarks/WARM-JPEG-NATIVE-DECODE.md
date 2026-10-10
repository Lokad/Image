# Warm decode of the native-prepared JPEG

This screen decodes the existing FFmpeg-produced 513x259 JPEG from identical
resident bytes to independent owned RGBA8. It adds one explicit benchmark
input and changes no production code. Default `Decode8` still selects only
the original owned JPEG; `NativeDecode8` selects only this native input.

The [original numeric evidence](warm-jpeg-native-decode-evidence.json) retains
managed samples, statistics and allocations, native elapsed/count brackets,
source/tool/DLL identities, output hashes, agreement and quietness. Regenerate
with `python benchmarks/render-warm-jpeg-native-decode.py`.

| Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| 1 | 7882.95 | 1717.65 / 1732.39 | 4.59x | 5.70x |
| 2 | 8779.15 | 1726.26 / 1717.73 | 5.11x | 5.98x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal SIMD, tiering disabled and non-concurrent
workstation GC. These short repeats guide ranking; they do not establish
uniform parity or coverage of other JPEG sampling modes.

The frozen qscale-5 JPEG has 133,041 bytes, variable Huffman tables and common
1x2 component factors, which remain 4:4:4 and exercise a padded block layout.
It comes from the same original deterministic RGBA8 raster flattened once
on RGB 0x204060. Preparation and encoder qualities are reused unchanged;
there is no new raster or quality enumeration. Input SHA256:
`c115cd7d17eb2cd3f34e31c439bdd48421e58ea2c0fce264c1cc8ad3eb1f8274`.

Both subjects decode and convert every pixel, acquire independent output
and release it. Native context/conversion/input packet/frame wrappers are
reused, with decoder state reset per operation. Padded native rows need no
tight-row copy. Managed public Decode and disposal are timed. Startup, IPC,
I/O, generation, hashing, checks and file packing are excluded.

RGB agreement is maximum 3, RMSE 0.227168, within
the unchanged maximum 4/RMSE 1.5 bounds, with exact opaque alpha. Managed
pixels reproduce the original frozen preparation. Native pixels exactly
match the pinned public FFmpeg CLI with normal SIMD and the same conversion.
Smoke and setup/cleanup verify repeated pixels, independent live owners,
unchanged source, geometry and single-thread boundaries. The retained fourteen
shared worker rejection controls use the original owned JPEG; they are not
reported as fourteen additional tests of this new input.

A separate workload profile attributes 53.04%
to entropy/Huffman processing and identifies it as the next optimization
target. Its source and trace hash are retained; profile samples are excluded
from the parity rows.

Each output has 531,468 logical bytes.
Managed BDN allocation is 2,660,360 bytes per operation.
Failed rolling-load checks and later passing cooldowns remain in the evidence.
Reproduce sequentially on the quiet dedicated Linux VM following the
[worker instructions](../tools/WarmReference/README.md#jpeg-decode).
