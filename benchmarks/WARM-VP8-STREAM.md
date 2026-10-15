# Fresh VP8 Stream decoding

Complete VP8-with-alpha stream decoding independently confirms **1.825x FFmpeg /
conservative upper 1.833x**, at **2,327,699 managed allocated B/op**.
Production is unchanged; uniform current-source coverage remains open.

| Measurement | Lokad.Image ms | FFmpeg ms | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 10.964 | 5.988 | 1.831 / 1.934 | 2,327,699 |
| Independent twelve-actual confirmation | 10.915 | 5.980 | 1.825 / 1.833 | 2,327,699 |

Both subjects reacquire the existing 98,590-byte native VP8/ALPH packet through
fresh seekable memory streams. Public managed `Decode(Stream)` includes default
buffering/growth/copies and never receives an exposed input buffer. Native fresh
AVIO reads into a newly owned packet and checks EOF every operation, without
demuxing. Complete input acquisition/copies, decode/state reset, centered chroma
expansion, limited BT.601 conversion through RGBA16, no-dither reduction, exact
alpha merge and input/scratch/output release are timed. Native context/frame/
packet wrappers may be reused; input packet storage is fresh each operation.

The input and each decoder's pixels exactly match the
[resident alpha anchor](WARM-VP8-FILTER8.md). Normal/scalar CLI pixels agree
exactly; native worker pixels equal that reference. Managed RGB agreement stays
maximum 1 / RMSE 0.132879, including hidden RGB, within unchanged maximum 4 / RMSE 1.5.
Alpha is exact. Complete reading, caller lifetime, unchanged source and two live
independent outputs pass before/after timing. No image, encoding, quality search
or comparison allowance is added.

Every versioned quiet guard passes. All twelve confirmation actuals are retained
and selected. The upper uses the larger filtered/all-actual interval against the
faster native bracket's lower bound. Sequential Release runs use one pinned CPU,
one codec/conversion thread, normal SIMD, disabled tiering and workstation
nonconcurrent GC. Startup, file I/O, IPC, generation, hashing and validation are
excluded; no decoded output is cached. Native allocations are unmeasured;
managed figures include harness/GC bookkeeping.

The separate original adapter preserves resident-VP8 and other stream workers.
Screen and confirmation reuse the same verified immutable production DLL/harness.
Only two benchmark files and an original adapter/build script change. Existing
3,308 Release tests per OS, including 896 live FFmpeg comparisons, and consumer
plus three examples through the same actual Release package are reused. No
production rebuild, full suite, package proof or whole-library method audit repeats.

The [numeric evidence](warm-vp8stream-evidence.json) retains every sample,
allocation, guard, actual job/assembly, input packet and qualification link.
Stop this confirmed pair; remaining material public/codec/current-source
coverage stays open.
