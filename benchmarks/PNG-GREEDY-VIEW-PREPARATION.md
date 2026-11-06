# PNG greedy view preparation

Preparing stable history, dictionary and token views removes repeated field
acquisition and coordinate work from greedy PNG encoding. The sweep uses bounded
32-bit window positions, publishing the original 64-bit logical coordinates before
callbacks and on exceptional exits. One insertion core consumes the prepared
views; history slides modify existing arrays. Candidate order, depth 4 / nice 8
selection, skip/short-match insertion, token/block boundaries, final lookahead,
cancellation checkpoints and exact packets are preserved. Rare slide/block
branches remain in the same execution core; no extra scheduler or duplicate path.

## Independent complete-operation gains

Twelve alternating original/candidate pairs per control independently confirm
the short-screen leads; every point is retained. This paired managed comparison
establishes the scoped gain, independently of older standalone reference clocks.

| Frozen 513×259 Fastest encode | Original µs | Prepared µs | Saving | Ratio [interval] |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 | 4,835.61 | 4,566.36 | 269.24µs / 5.57% | 0.944323 [0.941806, 0.946843] |
| RGBA16 | 19,504.49 | 18,668.85 | 835.64µs / 4.28% | 0.957162 [0.948231, 0.966130] |

Each isolated module owns identical resident source samples. Fresh public row
packing, history/dictionary/token preparation and admission, complete encode,
MemoryStream growth, independent output and disposal remain timed. Setup/JIT and
validation are excluded equally. Byte pairs batch 64 operations; word pairs 16.
Complete 236,828/968,216 B packets, source preservation and independent outputs pass.

## Current FFmpeg boundaries

Four affected complete one-shot controls refresh once on the quiet single-CPU
VM. Both sides time fresh codec/zlib/packet state, necessary conversion, independent
output and release. Word endian conversion remains timed; live-token and forward
output boundaries are retained. Twelve actuals, both native brackets and filtered/
unfiltered confidence remain. Native clock movement is not an incremental gain.

| Existing control | Lokad µs | FFmpeg µs | Ratio / upper | Lokad B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8 | 4,543.94 | 4,356.87 | 1.043 / 1.059 | 820,916 | gap |
| RGBA16 | 18,449.19 | 18,610.40 | 0.991 / 1.019 | 2,437,660 | uncertain |
| RGBA16, live token | 18,727.28 | 18,564.11 | 1.009 / 1.016 | 2,437,660 | uncertain |
| RGBA16, forward-only output | 18,557.49 | 18,594.14 | 0.998 / 1.004 | 2,437,692 | uncertain |

The byte case remains a demonstrated gap. Three word boundaries are uncertain,
not passing or evidence that they need further optimization. Eleven other PNG
costs reuse unchanged production paths: only ProcessFast and its exclusive Insert
helper change, and the remaining class text/fields match exactly after line-ending
normalization. The 15-case PNG inventory now has 3 passes, 4 gaps and 8 uncertain cases.
Passing paths stop. Other 121 displayed costs, including these 11 PNG cases, reuse
valid evidence. The [previous output milestone](PNG-COMPLETE-TOKEN-OUTPUT.md)
retains its timings, attribution and adverse/uncertain observations.

## Storage and qualification

Standalone allocations remain 820,916 B/op for byte, 2,437,660 B/op for ordinary/live
word and 2,437,692 B/op for forward word. Paired readings have small subject-position
variation, retained without a causal allocation explanation. No new array, owner,
pool, cache, session or API. The existing 384 KiB admission stays charged through
actual span users and callbacks; disposal releases it after unwinding. Scratch
retained after the operation is zero. Whole-operation physical/admitted peak and
native allocations are not newly measured; allocation is not CPU evidence.

Windows/Linux each pass 5,721 full/live checks (2,482 internal / 2,291 public / 948 differential),
581 internal / 211 public PNG checks in normal/scalar modes, and 43 internal / 16 public
affected AVX512-disabled checks. Frozen noise/overlap packets across 1/262/65,521 byte
writes and 0/2^40 logical origins, independent BCL decoding, output-failure prefixes,
cancellation, admission refusal, surviving ownership and subsequent root reuse pass.
One immutable core/embedded-test build per OS serves the whole experiment.

The SAME actual Release 0.2.1 package and all 3 examples pass both OSes. Its existing
public consumer explicitly exercises Fastest 8/16 frozen packets through awaited
async encoding with a shared root and cleanup; no new consumer or repeated package
proof is needed. Package `bddf776b32ec1a75a752af894215ca9b6ad928b4aec5ebb4ab2327f496388f86` (416,423 B);
Windows main `dd3adf0f5406b7ca9792f3b453fc5921121ecb42e0d8c20dbba493fba4e773ed`;
Linux main `ecc7d746ebfdd49843d84a99a7046f22504bed021662119b2795c7ca1294766f`.
Published releases are unchanged; publication is postponed. This is a development
milestone, not a 125-row actual-package release refresh or 282-path parity claim.

[Evidence](png-greedy-view-preparation-evidence.json) pins sources, complete points,
source applicability, packet/ownership gates, checks and actual package. Raw
`png-greedy-view-preparation-qualified.tar.xz`: 5,983,852 B / 284 members,
SHA256 `3ab99fc03dfb475a049da76686e1355fc394daab36358459853dab1d04011bb5`, mirrored/member-verified. Unchanged runtime payloads
are pinned to retained dependencies instead of duplicated. The current attribution
capture is also retained, including recovered valid samples after the attached
short worker exited; no completed capture or measurement is replayed.

Next select a larger demonstrated codec gap from current attribution. Preserve
these gains and resolve uncertain cases before assuming they need optimization.
The complete warm objective remains open.
