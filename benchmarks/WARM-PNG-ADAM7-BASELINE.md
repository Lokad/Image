# Warm Adam7 RGBA16 baseline

The independent quiet confirmation reaches 2.009x FFmpeg,
conservative upper 2.076x. This fixed pair remains open.
The earlier screen fails its post-load guard (0.251465 against the unchanged
0.25 limit); later quietness does not certify it. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Adam7 coverage screen | 7688.08 | 1.90105x | 1.96429x | FAIL |
| Independent Adam7 coverage confirmation | 8206.74 | 2.00934x | 2.07570x | Pass |

One original authoring of the existing 513x259 RGBA16 raster uses Adam7's seven
passes, filter None, BCL Optimal level6 and one IDAT. The 937,465-byte input is
identical on both sides. All pass rows and 531,468 sixteen-bit samples match the
analytical raster, including alpha and hidden RGB. Normal and scalar FFmpeg CLI
outputs are exact. Source preservation, independent held outputs, eight negative
native controls and twenty prior PNG controls pass. DecodeAdam7_16 is explicit;
the default PNG selection is now one Decode8 case. No raster, geometry, filter,
quality or compression-effort sweep is added.

Public resident-input decode includes validation, allocation and disposal.
Native context/input packet/frame wrappers are reused; complete decode, output
acquisition/release and RGBA16 big-to-little-endian conversion with a second owned
frame are timed. Startup, I/O, IPC, generation, hashing and validation checks are
excluded. CPU0, one codec/conversion thread, thread type zero, normal SIMD,
tiering off and nonconcurrent workstation GC remain fixed. Managed iterations
target200ms and native brackets50ms. Prepared children validate one operation
per native bracket outside statistical windows.

The independent run retains all twelve actual measurements and twelve native
samples per bracket. BenchmarkDotNet also selects all twelve. The conservative
upper takes the larger of filtered and all-actual confidence bounds, using each
managed confidence critical value for its native bracket lower bounds. Windows
normal/scalar decoded bytes match the retained actual package. Observed public
Stream allocations are 5,058,912 bytes per operation; no allocation improvement
is claimed. Both actual timing libraries retain all 562 production methods and
implementation flags from that package. No production package is changed.

One six-second CPU profile contains 5,904 samples. Generic
sparse-pixel conversion and repeated stride/pixel access account for
18.2% of sampled CPU. The next
substantial-win hypothesis copies packed RGBA pixels directly into their Adam7
positions, avoiding generic scaling and repeated per-channel destination calls.
Logical work, admission, precision, output ownership and cancellation remain
requirements. Diagnostic sample shares do not certify a speedup.

The [numeric evidence](warm-pngadam7-baseline-evidence.json) retains every actual
measurement, native bracket, guard, input/source/actual-child identity, Windows
controls and immutable timing/analysis/profile archives. Regenerate with
`python benchmarks/render-warm-pngadam7-baseline.py`; `--check` verifies this report.
See the [reference recipe](../tools/WarmReference/README.md). Broader required,
unmatched and unconfirmed warm coverage remains open.
