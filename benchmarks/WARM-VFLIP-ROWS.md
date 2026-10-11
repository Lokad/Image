# Warm vertical flip: reversed row copies

The fixed RGBA16 vertical flip improves 98.58% by screened mean,
from 67.28x FFmpeg to 0.99221x. The short screen passes both
unchanged quiet guards, with conservative upper 1.63597x.
Both twelve-sample repeats clear the numerical 2x bound but fail their post-run
quiet guards. Independent quiet confirmation remains open; a later cooldown
does not certify those intervals. This measures only one original 513x259
RGBA16 raster. Uniform warm parity and other material cases remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Generic mapping baseline | 1221.55 | 67.27758x | 91.04075x | Pass |
| Reversed rows, short screen | 17.29 | 0.99221x | 1.63597x | Pass |
| Twelve-sample repeat, failed postguard | 17.02 | 0.97105x | 0.99415x | Post-run failed |
| Fresh twelve-sample repeat, failed postguard | 17.78 | 1.00175x | 1.03192x | Post-run failed |

One vertical-flip primitive replaces generic per-pixel mapping with contiguous
row copies in reversed row order. It uses the existing complete-overwrite
allocator, whose single idle raster slot retains at most 4 MiB. The allocator
implementation and retention policy are unchanged. Every operation copies
all samples into an independently owned result and disposes it; released
storage may be reused, but output pixels are never cached instead of processed.

Both sample depths preserve full channel bytes, alpha, hidden RGB and source
orientation. Admission still counts both complete buffers, with four logical
samples per output pixel. Work and cancellation checks remain bounded to 1,024
pixels, including partial final chunks. A failed operation disposes its private
result before publication. Direct flips retain orientation metadata; AutoOrient
resets it after applying the same pixel placement.

Eight independent placement controls cover both depths, long partial chunks,
single rows and single columns, exact memory/work, cancellation, source integrity
and simultaneously live outputs with mutation isolation. Two controls poison
returned storage and reject a partially completed copy before verifying the next
complete result. Two EXIF controls verify direct flip and automatic orientation.
Focused tests pass on both OSes with intrinsics enabled/disabled: 149 baseline
and 161 final. All 2,157 Release tests per OS pass (533 internal, 728 public API,
896 live differential), as do all three examples through the same actual
dependency-free Release package. Saved timed and fresh production methods/flags
match that package. Only Orient's dispatch changes among existing production
bodies; VerticalFlip.Apply is the only added method (542 to 543). All other
bodies and remaining flags are equivalent with metadata references resolved.

FFmpeg's vflip filter returns a shared negative-stride view. The original
development adapter allocates an independent frame and copies every reversed
row using public frame APIs inside each timed operation, then releases the
view/output. Graph, source and frame wrappers are reused; padded output rows
need no packing copy. Fourteen native protocol, affinity and input controls
pass. Every UInt16 sample, source, independent live owner and pinned CLI pixel
matches the original integer placement reference; CLI checks use normal and
scalar execution. No performance image, geometry or quality sweep is added.

Runs are sequential Release jobs on the dedicated VM, CPU0, one filter thread,
thread type zero, normal SIMD, tiering off and non-concurrent workstation GC.
Native whole-operation batches use the declared 50 ms calibration target, with
three pre/post samples for screens and twelve for repeats. Managed samples
retain the 200 ms target. Startup, IPC, I/O, generation, hashing and validation
are excluded. Prepared generated children avoid compilation inside scored
windows. Short and confirmation preparation execute their actual fixed jobs
for validation only, with one native operation before/after and null ratios.

The [numeric evidence](warm-vflip-rows-evidence.json) retains managed results,
GC/threading lines, all native samples, unchanged guards, identities and immutable
archives, including both failed longer intervals. Conservative bounds divide
managed confidence upper by the smaller native confidence lower, using the
managed critical value. The pinned BDN parser reproduces retained official
report statistics for three and twelve iterations.
Regenerate with `python benchmarks/render-warm-vflip-rows.py`; add `--check` to verify.
See the [worker/replay recipe](../tools/WarmReference/README.md).
No further tuning of this numerically passing raster is planned while larger
gaps remain to be screened.
