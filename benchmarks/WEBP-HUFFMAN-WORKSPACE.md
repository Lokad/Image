# WebP Huffman construction workspace

WHS677 replaces fresh tree-building arrays and a priority queue per alphabet with
one admitted operation-owned workspace across an image's five alphabets and
histogram groups. Integer regions share one array; protected-run flags are cleared
on reuse. The BCL queue, smoothing, depth-floor policy, ordering and ties stay
unchanged. Selected canonical lengths/words retain independent storage and charges.
No cross-operation pool, cache, context or codec option is added.

Contemporary complete screens save **553,184 B** per photograph, **37,640 B** per
diagram, **668,512 B** per transparent-alpha encode and **112,632 B** on the existing
authored bars. Their admitted peaks remain **3,470,059 / 3,251,328 / 3,656,309 /
312,716 B**; retained scratch stays zero. The maximum-alphabet workspace remains
fully charged through every actual user, including smaller alphabets. These four
peaks do not establish universal peak savings; physical/native peaks are unmeasured.
Direct screen counters and aggregate benchmark allocation counters are distinct.

| Complete operation / existing reference | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| VP8L RGBA8 / libwebp exact=1 | 107,509.18 | 126,289.11 | 0.851 / 0.910 | 5,948,664 | Pass |
| VP8L opaque / libwebp exact=1 | 74,977.27 | 75,349.65 | 0.995 / 1.013 | 4,823,168 | Uncertain |
| VP8L photograph / libwebp exact=1 | 85,959.95 | 82,521.57 | 1.042 / 1.080 | 5,358,980 | Uncertain |
| VP8L text/diagram / libwebp exact=1 | 7,441.59 | 9,575.22 | 0.777 / 0.791 | 3,721,041 | Pass |
| VP8L RGBA16 / libwebp exact=1 | 114,600.56 | 131,281.45 | 0.873 / 0.887 | 5,948,664 | Pass |
| VP8L nonseekable async / libwebp exact=1 | 112,507.39 | 131,039.53 | 0.859 / 0.869 | 5,949,048 | Pass |
| VP8 + lossless alpha RGBA8 / FFmpeg | 54,544.68 | 53,351.23 | 1.022 / 1.065 | 6,242,608 | Uncertain |
| VP8 + lossless alpha RGBA16 / FFmpeg | 57,605.21 | 58,080.66 | 0.992 / 1.002 | 6,241,792 | Uncertain |

Each comparison has twelve actuals, native brackets, quiet guards and frozen
packet/source/ownership checks on the intended actual-package DLL. Normalization,
preparation, candidate selection, fresh output ownership and disposal stay timed.
References retain their labeled context/input/scratch boundaries. Four passes
stop; photograph, opaque and both alpha cases are uncertain. Earlier passing
photo/alpha16 costs are superseded. This stage claims allocation savings, **no
causal CPU gain, regression or equivalence**, and no complete 283-path parity.

The original short screen's raw elapsed points regress 6–9%. Contemporary photo
shorts normalize to0.972/0.971, but independent photo confirmation is uncertain;
all views remain retained. One two-second capture locates48/1,981 samples in
Huffman-related code, with most elsewhere in unchanged prediction/entropy/color
paths. Locations are not phase times or projected savings. No further tree-kernel
trial or favorable repeat follows this inconclusive CPU evidence.

New tests exercise dirty reuse, empty/single/tied alphabets, independent surviving
codes, context/capacity checks, partial-group refusal, cancellation and recovery.
Windows/Linux each pass **6,026 full/live checks**, plus426internal/239public per
normal/scalar mode. Unchanged SIMD/dispatch evidence is reused. The SAME actual
Release package and all three examples pass both OSes. One immutable main and
embedded module per OS serves the stage; no worker/native/fixture rebuild occurs.
Async managed resource proof refreshes before timing, reusing the unchanged native
proof. Completed profile data was recovered after postprocessing errors without
recapture. Linux package preflight caught a stale preceding certificate document;
current source bytes were synchronized before its consumer, without repacking.

Only Huffman construction callers invalidate earlier CPU/allocation costs:
lossless encoding and transparent lossy encoding's lossless alpha path. All eight
displayed affected comparisons refresh here. Packet correctness remains exact;
decoding, opaque lossy encoding, JPEG, PNG, text and resizing paths are unchanged.
Publication stays postponed.

[Evidence](webp-huffman-workspace-evidence.json) pins sources, complete boundaries,
all actuals and qualification. Mirrored/member-verified raw
`webp-huffman-workspace-qualified.tar.xz` has 13,625,672 B / 305 members,
SHA256 `83757ae5ab3160e288d61f6fc70168a643ec8f7541152275526f3edc10fc0fc1`.
[Clustering handoff](WEBP-CLUSTERING-STORAGE.md) and
[lifetime attribution](ARCHITECTURAL-PREPARATION-AND-STORAGE.md#current-webp-lifetime-follow-up)
retain prior evidence separately.
