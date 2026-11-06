# Shared VP8 coefficient windows

Eligible macroblocks retain one borrowed arithmetic reader across24or25blocks.
One unchanged coefficient loop serves this window and the actual one-block tail
path; short-input and cancellable readers remain. A conservative272bytes/block
selection preserves declared partitions. Contexts, zero runs, EOB, signed16
dequantization, pixels, following-read states and cancellation remain exact.

Independent12alternating pairs×32complete coding validations save
**114.56µs / 1.73%**,
6,623.48→6,508.92µs;
ratio0.982707, interval[0.979306,0.986113].
554,232B/op is unchanged. A separate three-pair screen precedes confirmation.
Fresh-stream decoding's short is inconclusive: ratio.996915,
interval[.963821,1.031143]; no broader incremental gain claim. All pairs remain.

The subjects borrow separate identical resident encoded input. Complete metadata
validation includes independent output ownership. Stream decoding includes fresh
non-exposed MemoryStream acquisition/copies/EOF, all decoding/chroma/RGBA output
and disposal. Source setup/JIT/checks are excluded equally. The managed-build
increment does not establish new native parity; standalone current costs follow.

| Existing complete513×259 operation | Lokad µs | FFmpeg reference µs | Ratio / upper | B/op |
| --- | ---: | ---: | ---: | ---: |
| Complete VP8+ALPH coding validation, span | 6,677.08 | 5,447.15 | 1.226 / 1.230 | 554,478 |
| VP8+ALPH resident decode | 7,041.53 | 5,961.94 | 1.181 / 1.186 | 1,434,731 |
| VP8+ALPH fresh seekable Stream decode | 7,063.58 | 5,980.11 | 1.181 / 1.185 | 1,434,827 |
| Opaque normal-filter decode | 5,626.09 | 4,792.43 | 1.174 / 1.180 | 760,564 |
| Opaque normal-filter decode, live token | 5,829.95 | 4,790.25 | 1.217 / 1.221 | 760,564 |
| Opaque simple-filter decode | 5,775.64 | 4,889.02 | 1.181 / 1.187 | 760,564 |

All six remain confirmed gaps. Each retains12actuals, both native brackets,
filtered/all confidence and quiet guards. Original packets/settings, exact frozen
pixels/alpha or36-byte descriptor, source/owners and reference gates pass. Coding
validation uses the approved natural-frame FFmpeg decode plus original source-
metadata adapter. Full decoders retain their declared normalization and fresh
input/output boundaries. Reference contexts are reused; these are warm boundaries,
not CLI startup. Historical/current clock differences are not causal gains.

This also corrects omitted invalidation in the [preceding encoder stage](VP8-FIXED-VALUE-CANDIDATE-PIPELINE.md):
its changed inverse has real full-decoder callers. One displayed and four further
covered decoder costs required reconciliation; all five are now current. Coding-
only validation skipped reconstruction and was valid before this new parser
change. Two displayed comparisons refresh; other123reuse unchanged paths.

No new heap capacity, owner, pool, session, cache or public API. The32-byte borrowed
window fits existing8KiB coarse header/scratch admission; a default window and
original fallback borrower can overlap. Operation scratch retention is zero.
Standalone GC/batch readings differ from paired allocation. Physical/admitted
peaks and native allocation are not newly measured; no CPU claim follows from
allocation. [Attribution/applicability](VP8-VALIDATION-READER-ATTRIBUTION.md) counts
justify the design, not a predicted saving.

Both OSes pass **5,721full/live checks** (2482internal/2291public/948differential),
45internal and27focused public checks in normal/scalar modes, plus58public corpus
cases per mode covering all164frozen frames, authored partitions/filters and
malformed syntax. Affected AVX512-disabled45internal/58public checks pass. New
24/25block tests compare every coefficient/cursor checkpoint and following reads
against the independent bitwise reader; tight partitions, live-token exclusion,
source/tails and borrowed zero-capacity root remain. The SAME actual private0.2.1
Releasepackage/all3examples passes both OSes, including existing513×259 compressed
alpha/shared-memory decoding. Published packages remain unchanged.

Independent tests caught an extra entry in the moved band literal before timing;
the original literal was restored. Both failed builds/logs and corrected immutable
builds remain. A metadata assertion also confused164fixture frames within two
theory loops with testcase count; existing58-case passing logs were reused without
rerunning tests. No completed measurement was restarted. One private paired tool
build serves the stage; native/harness/selectors/fixtures are reused.

[Evidence](vp8-shared-coefficient-window-evidence.json) pins sources/modules,
all points/guards, failures and package proofs. Raw `vp8-shared-coefficient-window-qualified.tar.xz`:
44,283,220B/659members,
SHA256`9d2daa4dd124156d34c90e4d48a3ea08bfdfc1f3fee44f6551dfd5c8c6cdead3`, mirrored/member-verified with qualified-base/applicability
dependencies. Full282-path parity and a complete125-row actual-package release
refresh remain open; publication is postponed.
