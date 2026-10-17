# Current adaptive JPEG confirmations

Historical timing checkpoint: its adaptive encode rows are superseded by
[nonzero AC planning](WARM-JPEG-ADAPTIVE-AC.md). The original packet, quality,
approved reference and complete operation boundaries remain unchanged.

4 existing adaptive JPEG workloads independently confirm within the 2x warm
single-CPU target on the qualified production DLL. This resolves older
confirmations invalidated by the shared AC event-planning change `10ccb31`.
No production optimization is needed in this round. Stop the confirmed pairs;
full required warm coverage remains open.

| Existing source / JPEG sampling | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Compressed bytes, managed / FFmpeg | Actual / selected |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| RGBA16 / 4:2:2 | 3,816.67 | 2,425.80 | 1.573 / 1.588 | 294,116 | 90,253 / 71,210 | 12 / 11 |
| RGBA8 / 4:2:2 | 3,824.38 | 2,344.76 | 1.631 / 1.639 | 294,116 | 90,214 / 71,168 | 12 / 10 |
| RGBA8 / 4:4:4 | 5,957.29 | 3,776.02 | 1.578 / 1.585 | 559,429 | 158,812 / 133,041 | 12 / 11 |
| RGBA16 / 4:4:4 | 5,955.85 | 3,852.95 | 1.546 / 1.552 | 556,286 | 158,827 / 133,043 | 12 / 11 |

Every operation starts with one existing original 513x259 RGBA8/16 raster and
returns a freshly owned complete JPEG packet. Alpha flattening onto RGB 0x204060,
full source normalization, sampling/transforms, adaptive Huffman preparation,
entropy coding and output acquisition/release are timed. Managed MemoryStream
creation/growth/disposal is included. Native codec/frame/packet wrappers are
warm, but every operation converts and encodes all pixels and reacquires/releases
its output. Only released private entropy workspace is reused; no coefficient,
normalized-image or packet cache. Startup, I/O, IPC, preparation and validation
are excluded.

The frozen settings remain managed quality 65 for 4:2:2 or 69 for 4:4:4 and FFmpeg
qscale 5, with adaptive/optimal Huffman coding, no restart markers and background
0x204060. Both complete packets, original/reference pixels, owned mathematical
settings and quality records match their prior anchors exactly. Maximum source-
quality PSNR gap is 0.245991dB within the unchanged 0.25dB gate. Decoder
agreement remains RGB max 4 / RMSE 1.5 with exact opaque alpha. Source preservation
and simultaneously held independent outputs pass before/after timing. Compressed
sizes are reported; these matched-quality timings do not establish compression
efficiency parity. No settings sweep or additional original raster is used.

Each useful passing three-actual screen precedes an independent twelve-actual
confirmation on the same immutable production, benchmark and native worker.
Every actual remains retained. The conservative upper takes selected/all-actual
uncertainty against the faster native bracket's lower confidence bound. Both
versioned quiet guards pass for every scored run. Sequential Release uses CPU0,
one codec/conversion thread, normal SIMD, tiering disabled and workstation
nonconcurrent GC. Native allocations are unmeasured; managed allocations include
harness costs.

The byte 4:2:2 short run was retained after correcting a private receipt check
that incorrectly required a 16-bit-only normalization flag. The original refused
controller record is retained. Production, binaries, raw timings, actual
benchmark receipt, pixel/quality/ownership checks and both quiet guards were
unchanged and independently revalidated; no replacement timing run is used.

Production, harness and native rebuild counts are zero. Only necessary cached
selection drivers change; every child including WarmDry receives the verified
assemblies. Production/package inputs remain identical to the existing
[qualification](WARM-JPEG-AC-PLANNING.md): 3,483 Release tests per OS including
live comparisons and the same actual package's consumer/all three examples on
Windows/Linux. No full-suite/package repeat, whole-library method audit or
profile is introduced. The [numeric evidence](warm-current-jpeg-adaptive-evidence.json)
retains all samples, guards, allocations, sizes, identities, the scoped private
validator correction and qualification reuse. Older measurements remain
historical at their recorded builds.

All four affected adaptive444/422 byte/word cases are now stopped. Continue
the finite checklist with ordinary baseline JPEG decode confirmations after
the changed AC loop/lookahead, one named existing case at a time.
