# Warm JPEG certified separable inverse transform

Horizontal and vertical AVX-512/VL passes reduce inverse-transform work.
A conservative numerical certificate preserves every original output byte;
uncertain rows and extreme blocks use the original direct calculation.
The retained JPEG decode cases remain above the 2x target.

The [numeric evidence](warm-jpeg-separable-evidence.json) retains actual
samples, uncertainty, native brackets, allocations, source/input/output hashes,
primitive/assembly controls and terminal receipts. Regenerate with
`python benchmarks/render-warm-jpeg-separable.py`.

| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native / accepted wrapper | 1 | 4791.91 | 1725.91 / 1718.72 | 2.79x | 3.18x |
| Native / accepted wrapper | 2 | 4807.18 | 1723.78 / 1717.28 | 2.80x | 3.59x |
| Native / initial two-pass screen | 1 | 4634.75 | 1717.24 / 1721.06 | 2.70x | 2.93x |
| Native / fixed spans | 1 | 4618.54 | 1719.84 / 1729.39 | 2.69x | 3.19x |
| Native / fixed spans | 2 | 4609.93 | 1727.65 / 1719.63 | 2.68x | 3.58x |
| Owned / accepted wrapper | 1 | 5195.24 | 1898.64 / 1904.97 | 2.74x | 3.22x |
| Owned / affected screen | 1 | 4990.39 | 1900.38 / 1904.71 | 2.63x | 2.99x |

The [rounding proof](../docs/jpeg-inverse-rounding.md) bounds reassociation
against the frozen direct binary64 evaluation. It retains separate products,
the original basis, half ties, clipping and all 4096 admitted logical work
units. No per-block buffer, image cache, new table, dependency or unsafe code
is introduced. AVX/scalar paths retain the original calculation.

The initial screen prompted generated-code inspection rather than a repeat:
its horizontal pass stored eight intermediate vectors back to the stack on
every iteration. Fixed-size spans and hoisted basis access keep them in
registers. This is a refinement of the same primitive. Original generated-code
and fixture controls are correctness evidence, not timing measurements.

Untimed counters in a private copy of the owned source find no uncertain
rows on either frozen JPEG: 6,630 / 6,435 factored blocks, with matching
decoded output hashes. These controls do not measure certificate cost.

The two frozen JPEGs use one existing original 513x259 RGBA8 raster. No new
image, preparation or quality enumeration is used. Complete public decode,
conversion, independent owned allocation and disposal are timed on identical
resident compressed bytes. Native contexts retain timed reset and all pixel
work. Startup, IPC, I/O, hashing and validation are excluded. Runs are
sequential on CPU 0 of the dedicated AMD EPYC 9V45 VM: one codec/conversion
thread, normal SIMD, .NET 10.0.8 / SDK 10.0.204, tiering disabled and
non-concurrent workstation GC.

All 2,008 tests pass on Windows and Linux: 428 internal, 686 public and
894 live FFmpeg comparisons. Eight new original coefficient families contain
2,947 blocks; their bytes match the previous owned assembly in verified
AVX-512, AVX and scalar configurations. All twelve original transform hashes,
input/destination guards, exact admission and cancellation controls remain.
The original raster fixture corpus and frozen decode output hashes are unchanged.

The same actual dependency-free Release package passes verification and all
three usage examples on both platforms. It includes the rounding proof.
Its method bodies, all thirty original JPEG fixture outputs and the primitive
families match the actual measured DLL. Package SHA256:
`cbdc0eacf1cccf123b8647679c2b78d4c8f4edd08fd10edd9729d94750b0561d`.

Inclusive allocation changes: [0, 0] bytes/op native,
0 bytes/op owned. Managed mean spread:
0.187%.

Failed post-run quietness checks and later passing cooldowns are retained.
Preparatory attempts refused timing after failed pre-run quiet checks;
their terminal receipts remain alongside the later actual screens.
Short screens guide ranking and do not certify uniform parity. The owned
input has one affected screen; both native and managed uncertainty remain
visible. Other measured, unmatched and unknown directions remain open.
