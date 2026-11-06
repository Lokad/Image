# VP8 coefficient-local trellis execution

The search now evaluates its mandatory floor and optional following level directly,
using one shared scoring source. The generated code inlines both constant-choice
calls. This removes the inner choice loop and selectors without full-block price
preparation, additional scratch or changed encoder decisions.

The reference is the frozen FFmpeg/libwebp worker, on one CPU.

The stage retains a **1.90% scoped counterbalanced gain**. The actual standalone
comparison saves **349.90µs/3.13% raw**, with separated filtered/all-actual confidence.
These process/loading populations and magnitudes remain separate. Neither proves
native-normalized improvement or parity.

| Complete frozen opaque diagram/Q75 | Managed µs | Native µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Effort6, contemporary original | 11,181.35 | 10,766.67 | 1.039 / 1.046 | 754,497 |
| Effort6, current independent | 10,831.46 | 9,811.63 | 1.104 / 1.111 | 754,497 |
| Effort5, current secondary control | 7,144.08 | 7,000.96 | 1.020 / 1.031 | 741,065 |

The candidate native brackets move 10,511.70→9,811.63 µs; the faster bracket point
is 8.87% below the original's. The normalized point is 6.30% adverse, but its full
comparison interval includes 1 in both views: no normalized gain or loss is
established. Report the measured movement without assuming its cause. The current
standalone reference result remains a gap, upper 1.111. Effort5's +35.96µs/0.506%
raw adverse point is also inconclusive; its upper 1.031 remains a gap. No favorable
standalone resampling is used to seek separation.

One existing isolated-assembly comparison retains 12 alternating pairs of 16 complete
encodes per subject: 10,166.51→9,973.57 µs, ratio 0.981023 with interval
[0.978401,0.983648]. It verifies a scoped source effect while retaining exact packets,
source processing, preparation, fresh output growth/ownership and disposal. Its
754,344 B allocation accounting differs from the standalone 754,497 B boundary; neither
magnitude nor allocation is transplanted into the table. The initial three-actual
screen is retained. A contemporary original twelve resolves its historical-clock
comparison, followed by one independent candidate twelve and the affected effort5.

All inputs/settings and complete packet hashes stay frozen: 6,818/7,026 bytes with
exact reconstructed pixels and alpha. The recorded original raw/normalized scoring
transcript, zero/cancellation/disposed-owner prefix and traceback remain unchanged;
only the fixed choice dispatch differs. Both choices read the same predecessor
scores before context updates; missing choice 1 keeps the original dead sentinel.
Complete operations include admission, source processing, fresh independent packet
ownership and disposal. Native wrappers/private scratch keep their declared reuse.
These are tested diagram/settings results, not broader compression claims.

Existing 320 B candidate arrays,128 B borrowed DP scratch,2 KiB standalone/16 KiB macroblock
allowances and 20,192 B rate-plan capacity stay unchanged. New owned/retained storage
is zero. Search still subtracts 600 B of stack with 112 B nested preparation plus saved
registers, fitting the allowances; its code grows 2,103→2,545 B. Those code observations
are not CPU evidence. GC totals are unchanged. Whole-operation physical/admitted
peaks and native allocations are not newly measured; capacity/ownership paths are
unchanged. Both affected costs refresh; seven unchanged encoder paths reuse valid
proof: three pass, three gaps, three uncertain. One displayed row refreshes; 124 numerical
rows reuse unchanged paths.

Windows/Linux each pass **5,751 full/live checks** (2,495 internal/2,308 public/948 differential)
and 69 internal/140 public per normal/scalar mode. Linux generated-code collection also
passes all 22 complete packet tuples. No new ISA guard; unchanged preparation/transform
AVX2/AVX512-disabled evidence is reused. One immutable main/embedded revision per OS;
no public harness, native, selector or fixture build. Windows main`9bcf058a…`, embedded
`255acdef…`; Linux main`d826d125…`, embedded`951f2243…`.

The SAME actual private Release package/all three examples pass Windows/Linux:
`928688932d537ca1c564ddd17194ce3549fae120a55f6145930fc0c10ddb6046` (419,566B), symbols
`7f8c8ce77ea8121953f1a8ec6ff252a04694d888d8f91fe8978da7f35824c4f7`.
Published packages are unchanged; publication and a full actual-package release
refresh remain postponed. All 283 material paths remain in scope.

[Evidence](vp8-coefficient-local-trellis-evidence.json) pins source proofs, every
actual/bracket/guard, paired scope, qualified modules and actual package. Mirrored,
member-verified raw `vp8-coefficient-local-trellis-qualified.tar.xz`: 2,862,736 B / 148 members,
SHA256 `3a335e5a05f83414b5a7fedf645bacff36b04cd64ac99a5a07dad088b8d619dd`, with pinned
reproducible dependencies. [Earlier candidate preparation](VP8-TRELLIS-CANDIDATE-PREPARATION.md)
and the [unaccepted full-block program](VP8-BLOCK-PRICE-PROGRAM-EXPERIMENT.md) retain
history; the latter supplies no renderer assertions.
