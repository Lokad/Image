# Exact packed VP8 reconstruction

Certified coefficient blocks stay in signed16 SIMD through both inverse passes,
removing repeated widening, multiply-low/shift and first-pass wrap instructions.
For all16 inputs within±2047, first-pass values are bounded by7877 and second-pass
values by30310; final+4 fits signed16. The signed multiply-high sine identity,
floors, order, rounding and pixel clamp are exact. The original full-range signed32
and portable kernels remain. Inputs load before publication; existing pair
snapshots preserve aliased buffers. There is no new owner, capacity, pool or API.

The earlier [short trial](VP8-PACKED-INVERSE-RECONSTRUCTION-REJECTED.md) stopped
under a now-retired5% rule despite a485µs lead. Current source matched its baseline,
so its arithmetic/eligibility proof and two boundary/alias controls could be reused.
This bounded reconsideration runs one current short pair and one independent pair
on each existing photograph/alpha control, without variants or favorable resampling.

The photograph independently saves **337.10µs /2.27% filtered raw elapsed**
(14,819.38→14,482.28µs). Filtered/all-actual raw and conservative bracket-normalized
confidence separate. Normalized gain is2.05%; native clock is0.23% faster.
Alpha saves978.19µs/1.38% by raw point, but normalized0.89% confidence overlaps;
native clock is0.50% faster. **No independent alpha CPU gain is claimed.** Its
adverse short (raw7.90%, normalized0.64%, native7.22% slower) remains alongside the
photograph's2.10%raw/2.35%normalized short lead. No actual or confirmation repeats.

| Complete encode,513×259 | Managed µs | FFmpeg µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Photograph, effort4 | 14,482.28 | 10,521.42 | 1.376 /1.391 | 809,072 | 17,688 |
| RGBA8 + lossless alpha | 69,657.13 | 57,733.16 | 1.207 /1.217 | 7,165,880 | 98,590 |
| RGBA16 + alpha, normalization included | 70,165.60 | 57,652.08 | 1.217 /1.229 | 7,165,472 | 98,590 |
| Opaque RGBA8 | 20,366.51 | 16,737.93 | 1.217 /1.225 | 1,201,552 | 72,380 |
| Text/diagram, effort4 | 8,123.43 | 6,270.92 | 1.295 /1.304 | 741,137 | 7,100 |
| Text/diagram, effort2 | 3,522.60 | 2,709.10 | 1.300 /1.311 | 741,375 | 7,330 |
| Text/diagram, effort6 | 14,550.71 | 10,511.03 | 1.384 /1.402 | 754,344 | 6,818 |
| Opaque RGBA16, normalization included | 21,738.20 | 16,813.65 | 1.293 /1.304 | 1,201,552 | 72,380 |

All eight existing costs are current on one immutable producer and retain CPU
gaps. Only photograph has independently separated raw AND normalized gain;
the other six refreshes are not incremental gains or causal-regression claims.
Allocation is unchanged at each boundary. No extra owned capacity or retained
operation scratch is introduced; physical/admitted peaks are not remeasured.
Existing output/alpha/coefficient owners and storage admission remain unchanged.
Arithmetic eligibility and logical instruction counts are not CPU-time attribution.

The complete public operation includes normalization, preparation, encoding,
independent packet acquisition and disposal. The existing FFmpeg/libwebp reference
retains its documented codec/frame/packet wrappers; repacking and complete encoding
stay timed. Both sides run sequentially on one quiet-VM CPU. Frozen sources,
settings, complete packets, reconstruction, alpha and ownership gates remain.
Equal numerical quality settings alone do not establish equivalence.

Both OSes pass **5,625 full/live checks** (2,390internal,2,287public,948live) and
844internal/357public affected checks per normal/scalar mode. The two retained
cases cover certificate limits, every out-of-range lane, stride/padding and aliased
single/pair outputs; existing signed16-domain checks remain. Retained arithmetic
proof includes65,536 sine identities and4,176 synthetic blocks. These prove
arithmetic/eligibility, not speed. The **same actual Release package/all three
examples** pass Windows/Linux. Published releases remain unchanged.

One core/internal-test version per OS is reused throughout; no public-test,
harness, native or selector builds. One coherent broad/package milestone follows
selection. A stale final-CGE-test manifest entry stops before any target; only
that verified test identity is corrected. An alpha postcheck searched the wrong
receipt family: its completed three actuals are recovered unchanged, completed
photograph jobs reused, and only the unexecuted alpha candidate launched.
Both failure/recovery records remain; neither causes repeated sampling.

[Evidence](vp8-packed-reconstruction-evidence.json) pins current costs, all short/
independent decisions, source/producer/package identities and retained proof.
Mirrored/member-verified `vp8-packed-reconstruction-qualified.tar.xz`: 4,503,016B /436members, SHA256
`52792940e0c22b1ce0b114e5a0624fdc79e6a8b17c86f1d6c04251e183e4495d`.
Original/candidate sources and modules, every actual/native bracket/guard, failure
records, full qualification, actual package and reproduction scripts remain, with
dependencies pinned. VP8L/ALPH, other codecs, resizing and text/drawing reuse
unchanged-path evidence. Full282-path parity remains open; publication is postponed.
