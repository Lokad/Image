# Warm JPEG rejected MCU-coordinate hoist

Calculating MCU row and column once provided no useful measured gain.
Native-input changes are small and overlap sample variability; the affected
owned-input screen is slightly slower. The production change was discarded.
The retained wrapper implementation remains roughly 2.8x FFmpeg.

The [numeric evidence](warm-jpeg-mcu-rejection-evidence.json) retains actual
samples, uncertainty, native brackets, allocations, input/output hashes, source
identities and controls. Regenerate with
`python benchmarks/render-warm-jpeg-mcu-rejection.py`.

| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native / accepted wrapper | 1 | 4791.91 | 1725.91 / 1718.72 | 2.79x | 3.18x |
| Native / accepted wrapper | 2 | 4807.18 | 1723.78 / 1717.28 | 2.80x | 3.59x |
| Native / rejected hoist | 1 | 4774.10 | 1718.48 / 1717.62 | 2.78x | 3.22x |
| Native / rejected hoist | 2 | 4792.59 | 1725.55 / 1729.04 | 2.78x | 3.89x |
| Owned / accepted wrapper | 1 | 5195.24 | 1898.64 / 1904.97 | 2.74x | 3.22x |
| Owned / rejected screen | 1 | 5256.50 | 1899.53 / 1918.32 | 2.77x | 4.63x |

The change calculated quotient/remainder after the existing MCU work admission
and restart handling, then reused those coordinates for every component block.
Correctness-only original generated-code controls confirm that division moved
out of the inner block loop. All thirty original fixture outputs matched.
Less repeated arithmetic alone did not establish a useful speedup.

All three screens use the same two frozen JPEGs from one existing original
513x259 RGBA8 raster. No new image, preparation or quality enumeration is used.
Runs are sequential on CPU 0 of the dedicated AMD EPYC 9V45 VM, with .NET
10.0.8 / SDK 10.0.204, one codec/conversion thread, normal SIMD, disabled tiering
and non-concurrent workstation GC. Resident identical compressed bytes feed
complete public decode, conversion, independent owned allocation and disposal.
Native reset is timed; startup, IPC, I/O, generation, hashing and validation
are excluded. Frozen output hashes match the accepted wrapper version.

All 2,000 tests pass on Windows and Linux: 420 internal, 686 public and
894 live FFmpeg comparisons. Baseline/progressive sampling, restart, logical
work, memory/output admission and cancellation remain checked. Corpus unchanged.
Inclusive allocation changes: [0, 0] bytes/op native,
0 bytes/op owned.

Failed post-run quietness checks and later passing cooldowns are retained.
Short screens neither certify small speedups nor establish regressions. The
production implementation and verified Release package from the accepted
wrapper checkpoint remain unchanged. Uniform warm parity stays open.
