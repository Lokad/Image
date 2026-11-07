# VP8 four-block source histograms

VHB517 accepts one shared histogram boundary for four already transformed luma
blocks. On the frozen complete effort-2 diagram encode, independent original and
candidate confirmations save **101.15 µs (3.69% raw, 3.47% reference-normalized)**.
Both filtered and all-actual confidence intervals separate; the native point moves
−0.23%. Allocation remains **406,705 B/op**. This is a scoped elapsed-time gain,
not a compression change or a causal gain claim for the other controls.

## Design and correctness

`CollectPlane` previously called `AddBlock` four times after a four-block forward
transform. On AVX-512F/BW, `AddFourBlocks` loads all 64 coefficients before recycling
that same 128-byte workspace for bins. Two vector preparations replace four
boundaries; the existing per-16 density policy remains. Eligible zero-bin updates
are combined. All-zero batches avoid workspace stores.

Signed extrema, modulo integer counts and final scores stay exact. Overlapping
populations and non-512 platforms call the original four methods before any
destructive workspace writes, preserving sequential alias feedback. Chroma-8,
live cancellation and portable paths retain their original entry. Predictions,
transforms, candidate order, ties, probability models and packets are unchanged.
No additional scratch payload, owner, cache, pool, quota or retained state exists.

The enclosing source-analysis payload remains 560 bytes within its existing
1,024-byte coarse workspace allowance. Actual production JIT output shows two
512-bit magnitude/bin paths and the intended load-before-store order. Helper
local frames are 40 bytes plus saved registers; the caller retains its existing
128-byte dynamic coefficient workspace. These are code-generation observations,
not physical peak measurements.

## Current complete-operation costs

Each point has 12 actual operations and 12 native samples per bracket on the quiet
single-CPU VM. Normalization, preparation, independent output and disposal remain
timed. FFmpeg/libwebp retains its declared codec/private scratch contexts. Every
complete packet equals the frozen native reference; quality/source/ownership gates
remain unchanged. The first effort-6 point is retained, with its predeclared
four-launch confidence envelope. No additional favorable repeats were taken.

| Control | Managed / native µs | Conservative upper | B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Opaque8 | 13,865.79 / 15,509.82 | 0.898 | 1,201,560 | Pass |
| Opaque16 | 13,882.59 / 15,579.01 | 0.894 | 1,201,560 | Pass |
| Photograph | 9,630.85 / 9,657.98 | 1.001 | 809,233 | Uncertain |
| Diagram | 5,476.34 / 5,803.20 | 0.952 | 741,196 | Pass |
| Selected diagram | 6,396.63 / 6,457.38 | 0.994 | 741,073 | Pass |
| Diagram, effort6 | 9,256.01 / 9,813.99 | 0.998836 | 754,505 | Pass |
| Diagram, effort2 | 2,638.29 / 2,504.45 | 1.058 | 406,705 | Gap |
| Alpha8 | 53,302.41 / 53,807.33 | 1.002 | 6,963,840 | Uncertain |
| Alpha16 | 53,129.08 / 53,549.12 | 0.996 | 6,963,024 | Pass |

Six pass, one gap, two uncertain. Passing cases stop; uncertain points do not
demonstrate an optimization gap. All nine earlier source-analysis costs are
superseded. JPEG, decoding, lossless WebP, PNG, resizing and text evidence remains
valid on unchanged paths. Eight displayed comparisons refresh; 117 reuse valid
evidence. This is not a full actual-package release refresh or all-path parity.

## Qualification and reproduction

Windows/Linux each pass **5,875 full/live checks**, plus **152 internal and 239
public checks per normal/scalar mode**. New tests cover every 16-bit zero mask,
signed bins, overflow, borrowed tails, invalid shapes and overlapping layouts.
The same actual Release package and all three usage examples pass on both OSes.
One core/embedded build per OS is reused throughout; no public harness, native,
selector, fixture or diagnostic rebuilds were needed.

The existing four admission controls, normal/live on each OS, retain peaks
397,205 / 930,669 / 54,821 / 51,200 bytes, zero retained scratch, unchanged borrowed
sources and a surviving borrowed root. These coarse capacities are separate from
GC allocation and unmeasured native/physical peaks.

[Machine-readable evidence](vp8-four-block-histogram-evidence.json) pins sources,
original/current modules, all packets, actuals, guards, comparison intervals,
confidence launches, qualification and the actual package. Raw archive
`vp8-four-block-histogram-qualified.tar.xz` (SHA-256
`4f8b1b05bbfb83c9c4161778befe4d92f05b19330396b9f7cbef1449b3bcfdbc`,
7,777,224 bytes, 293 members) is mirrored and every member verified. Its scripts,
cached-input providers and pinned predecessor archives reproduce the round.

[Per-block preparation](VP8-SMALL-BLOCK-PREPARATION.md) retains earlier gains and
normalized uncertainty as history. The unselected
[dense preparation](VP8-PREPARED-DENSE-EXPERIMENT.md) remains an adverse result,
not a ban on coherent redesign. Older histogram profile shares are historical
after this change. The remaining effort-2 gap is about 134 µs; larger independent
decoder/PNG/resizing opportunities can proceed. Publication remains postponed.
