# Warm single-CPU JPEG Huffman lookup round

This round changes one production primitive: canonical Huffman decoding.
Each encoded table generates its own eight-bit prefix lookup, following
T.81 C.2. Each table admits 512 additional bytes and 256 construction work
units before allocation. Valid short codes charge their original logical
bit count together; the original scalar reader handles long or unavailable
codes, markers, truncation and insufficient work budgets.

Cached complete codes inspect no additional byte. Otherwise at most one
unstuffed byte is inspected without mutating reader state. Only a valid
admitted short code commits that state. Near the work limit, the original
per-bit walk preserves partial consumption and format/limit precedence.
Cancellation remains checked per code, with at most eight logical bits
admitted together. Tables are rebuilt on every public Decode; no pixels or
outputs are cached.

It reuses the single [warm JPEG decode pair](WARM-JPEG-DECODE.md). The
[original numeric evidence](warm-jpeg-prefix-evidence.json) retains both
accepted inverse-DCT baseline repeats and lookup repeats, raw statistics,
native brackets, allocations, identities and quietness. Regenerate with
`python benchmarks/render-warm-jpeg-prefix.py`.

| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| baseline | 1 | 16879.56 | 1900.92 / 1900.59 | 8.88x | 10.42x |
| baseline | 2 | 17503.65 | 1898.14 / 1904.64 | 9.22x | 9.90x |
| prefix | 1 | 11550.07 | 1902.50 / 1903.81 | 6.07x | 7.76x |
| prefix | 2 | 11601.04 | 1899.99 / 1898.90 | 6.11x | 6.71x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal native SIMD, optimized managed JIT
with tiering disabled and non-concurrent workstation GC. Two short repeats
guide priority; uniform parity is not established.

Both subjects decode the same resident 324,002-byte baseline 8-bit 4:4:4
JPEG to independent RGBA8, including full-range BT.601 conversion, allocation
and release. Native state reset is timed; contexts/wrappers are reused but
every call decodes and converts all pixels. Padded native output needs no
tight-row copy. Public managed Decode and disposal are timed. Startup, I/O,
IPC, validation, hashing, generation and packing writes are excluded.

No quality enumeration or additional raster is added. Input and both output
hashes remain unchanged. Owned pixels equal the frozen preparation; native
pixels equal the pinned FFmpeg CLI. RGB agreement retains maximum 4/RMSE 1.5
bounds and exact opaque alpha, with independent live output storage.

Managed BDN allocation is 2,594,976 before and
2,596,234 bytes per operation after. Two fresh tables
add 1,024 admitted payload bytes and 512 construction work units to this
pair. Their array/object overhead accounts for 1,088 actual allocated bytes;
raw BDN allocation counts also retain sampling/diagnostic variation.
Exact memory/work boundaries include these costs, including replaced and
unused Huffman definitions.

Windows/Linux full Release/live suites pass 1,855 tests (275 internal,
686 public, 894 FFmpeg comparisons). Each of normal, AVX-512 disabled, AVX
disabled and all intrinsics disabled modes passes 55 internal and 70 public
JPEG controls on Linux. Original canonical/long-code, padding/restart,
partial-work and malformed-code controls pass; three added controls cover
mixed cached short codes, storage admission and marker/error precedence.
Frozen scalar transform output and all 160 encoded JPEG bitstreams pass.
The same actual dependency-free Release package and all three usage examples
pass both OS: SHA256 `29556f99ff827adca071441b24783c533e82fcf4ced7d42f53d8a4ec43d6d7fd`.

Source-byte and pre-run quietness checks pass. Failed post-run rolling-load
checks and later passing cooldowns remain without weakened thresholds.
The separate accepted-inverse workload profile attributed 57.05% inclusively
to entropy reading and 30.55% to ReadBits leaf samples. Profile timing is
excluded from parity evidence; remaining primitives need fresh attribution.

Reproduce sequentially on the quiet dedicated Linux VM using the
[worker instructions](../tools/WarmReference/README.md#jpeg-decode), the
frozen preparation, output smoke and two independent warm JpegDecode repeats
with a passing cooldown between them.
