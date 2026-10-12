# Packed gray4 with shared PNG storage

This check uses the existing opaque gray4 PNG, noninterlaced/filter None/zlib
level 6, projected from the original 513x259 RGBA8 raster: index=red>>4,
RGB=index*17, alpha=255. Samples are packed MSB-first, unused low bits zero.
Coded precision is 4 and output is RGBA8. No new original image is added.

Both subjects decode identical resident bytes into independent owned output.
Native timing includes unpacking to GRAY8, one-thread conversion to RGBA,
allocation/copy/release; wrappers/contexts are reused. Managed public Decode and
disposal now use the existing complete-overwrite storage policy. Startup, I/O,
IPC, preparation and validation are excluded. Native SIMD/scalar samples,
source, padding/tails and held owners are exact.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Prior gap, before shared storage | 420.42 | 159.85 | 2.630 / 2.775 | 545,302 |
| New three-actual screen | 244.10 | 163.34 | 1.494 / 3.258 | 13,544 |
| Independent twelve-actual confirmation | 220.61 | 146.67 | 1.504 / 1.532 | 13,544 |

This fixed pair is **confirmed: 1.504x / upper 1.532x**. The independent repeat
retains 12 actuals / 10 filtered results. Every quiet guard passes; the upper takes
the larger filtered/all-actual bound. Allocations fall 97.5%, to 13,544 B/op.
Stop tuning this pair. Allocation includes harness overhead; native totals remain
unmeasured. Retained and first-use owners are distinct performance boundaries.

Production is unchanged from the [PNG storage round](WARM-PNG-DECODE-REUSE.md).
After synchronization, forced fresh builds and both timed children match all 582
method bodies and flags of the same accepted Release package. Its 2,646 tests
per OS, poisoned-storage/held-owner controls for all 72 authored PNG profiles,
scalar controls and all three package examples remain the validation. Full QA
and pack are correctly reused for this evidence-only change.

The [numeric evidence](warm-pnggray4-reuse-evidence.json) retains actuals,
brackets, source/input/raster hashes, four actual child identities and raw/package
proofs. The earlier [above-2x repeat](WARM-PNG-GRAY4.md), including its unnecessary
confirmation launch, is preserved unchanged. Palette8 remains the largest
reported gap at 2.433x; gray8/RGB16/JPEG420 and other material warm scope remain
open. Passing this pair does not establish uniform warm parity.
