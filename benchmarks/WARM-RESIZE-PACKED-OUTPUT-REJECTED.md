# Warm resize: packed output trial rejected

Packed RGBA8 output normalization, quantization and storage shows no useful
improvement in one short screen. Its managed mean is 1098.13
microseconds against the retained 1097.32-microsecond baseline.
No longer repeat was run; the production source was restored exactly.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: |
| Retained packed accumulation baseline | 1097.32 | 2.88773x | 2.92525x |
| Rejected short screen | 1098.13 | 2.86866x | 4.59367x |

The trial replaced three scalar RGB divisions, Round calls and byte stores with
packed binary64 division, clamping, exact fractional-half comparisons and byte
packing. Alpha retained its existing rounding and tiny-alpha handling. It made
no change to sample accumulation or tap order. The short screen does not support
a speedup claim; differing native brackets and uncertainty do not establish one.

The sole performance pair remains the original 513x259 RGBA8 to 256x129 bilinear
resize, with complete public Resize, admission, preparation, allocation,
filtering and disposal included. The native FFmpeg graph includes conversion,
widened triangle filtering, encoded-space alpha handling and independent output
acquisition/release. Native contexts are reused. Native samples bracket managed
samples; both use one CPU/filter thread and normal SIMD. Startup, IPC, I/O,
generation, hashing and validation are excluded. No performance image was added.

175 baseline controls per OS and 181 trial controls per OS with intrinsics
enabled/disabled pass. Six original two-pixel controls specify literal RGB
results at and on either side of a half; they remain as regression tests.
Managed pixels retain the exact rational-reference digest. Native alpha is exact
and RGB stays within the existing maximum 1 / RMSE 0.25 bound. The actual
statistical child DLL matches the fresh trial's production IL and flags.

After restoration, all 2128 Release tests and 181 scalar controls pass per OS.
Fresh production IL and flags match the previously verified actual package.
The accepted implementation and its retained 2.89x result remain unchanged.
Pre-run quiet checks pass and post-run failures are retained; independent quiet
confirmation remains open. The next hypothesis is wider filtering across
independent output pixels, retaining each pixel's binary64 arithmetic order.

The [numeric evidence](warm-resize-packed-output-rejected-evidence.json) retains
the original short samples and identities. The immutable short archive SHA-256
is `c2d2c3d4d00dc5666faa9953a21db04ca070488a14ad1e32a7aac888a2dd5e35`;
the rejection/restoration archive SHA-256 is
`1e7d3f974540e271b72fca27460f21a38fddcb997de71834f58eb5193c07b6fa`.
Regenerate with `python benchmarks/render-warm-resize-packed-output-rejected.py`;
use `--check` to verify this report.
