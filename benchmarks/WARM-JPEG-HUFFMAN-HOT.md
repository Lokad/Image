# Warm JPEG short-code wrapper

One production primitive moves long/canonical fallback out of the common
Huffman wrapper, allowing the caller to inline its short-code probe.
Warm JPEG decode improves, while both retained gaps remain above 2x.

The [numeric evidence](warm-jpeg-huffman-hot-evidence.json) retains actual
samples, uncertainty, native brackets, allocations, input/output hashes,
source identities, controls and quietness observations. Regenerate with
`python benchmarks/render-warm-jpeg-huffman-hot.py`.

| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native / retained baseline | 1 | 5019.99 | 1734.75 / 1726.15 | 2.91x | 4.25x |
| Native / retained baseline | 2 | 5001.46 | 1723.06 / 1716.30 | 2.91x | 3.31x |
| Native / short wrapper | 1 | 4791.91 | 1725.91 / 1718.72 | 2.79x | 3.18x |
| Native / short wrapper | 2 | 4807.18 | 1723.78 / 1717.28 | 2.80x | 3.59x |
| Owned / retained baseline | 1 | 5400.91 | 1899.30 / 1897.88 | 2.85x | 2.93x |
| Owned / affected screen | 1 | 5195.24 | 1898.64 / 1904.97 | 2.74x | 3.22x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0, one
codec/conversion thread, normal SIMD, tiering disabled and non-concurrent
workstation GC. Runs are sequential. Native-input managed mean spread:
0.319%. The owned input has one affected screen only.

Two frozen JPEGs use one existing original 513x259 RGBA8 raster. No new image,
preparation or quality enumeration is used. Each pair receives identical
resident compressed bytes. Complete public decode, conversion, independent
owned output allocation and disposal are timed. Reused native contexts retain
timed reset and every decoded/converted pixel. Startup, IPC, I/O, generation,
hashing and correctness checks are excluded.

The existing short-code probe now has a small wrapper marked for inlining.
Its long-code lookup and canonical traversal move unchanged into a helper.
Current baseline generated code showed a saved-register prologue on every
Read call. The candidate baseline-block caller contains the short probe and
fallback helper call directly, with no separate Read-wrapper call. Both actual
package and measured DLL retain the inlining flag and matching method bodies.
Code generation controls use original fixture inputs and make no timing claim.

Tables remain independently derived under
[T.81 C.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf). Allocation, table
construction/admission, logical work, cancellation, source/error positions,
padding/restarts and scalar fallback are unchanged. No storage, dependency,
unsafe code or decoded cache is added.

A preceding complete canonical-symbol candidate was discarded after one
5097.54 µs screen (2.97x). It had no useful gain;
its repeat and affected-input screen were omitted. Original evidence remains.

All 2,000 tests pass on Windows and Linux: 420 internal, 686 public and
894 live FFmpeg comparisons. Thirty-three canonical cases include sixteen
mixed cached phases, 288 partial short-code budgets with long tables and all
800 partial nine-to-sixteen-bit budget/phase combinations. Complete long symbols,
marker/truncation/work precedence, invalid prefixes, source positions, exact
table admission and cancellation remain checked. Output smoke and native CLI
checks pass. Frozen outputs match the retained baseline; fixture corpus unchanged.

The same actual dependency-free Release package passes verification and all
three usage examples on both platforms. Its method-body fingerprint and every
pixel of thirty original JPEG fixtures match the actual measured DLL. SHA256:
`8fe7024c44bac9771329e6c8d27f05a9ea733ed02f1f876f29b16f975a3e3822`.

Inclusive BDN allocation changes: [0, 0] bytes/op for native repeats,
0 bytes/op for the owned screen. Full diagnostics remain visible.

Failed post-run quietness observations and later passing cooldowns are retained.
The separate baseline CPU profile precedes this wrapper change. Its workload
includes warmup/calibration; it is not parity timing or candidate attribution.
Short screens guide ranking and retain both managed and native uncertainty.
Uniform warm parity remains open.

Reproduce sequentially with the [worker instructions](../tools/WarmReference/README.md#jpeg-decode).
