# VP8 trellis execution preparation

Retain borrowed trellis execution and non-overlapping DP workspace reuse, with
plain/trellis policies selected at macroblock entry. Complete diagram encoding
at effort6 independently saves **311.23µs/2.72%**. Photograph encoding at effort4
independently costs **30.94µs/0.27% more**; retain that tradeoff and its remaining
gap. Both preserve exact complete packets and paired allocation. No new heap
buffer, owner, pool, cache, quota, public API or numerical policy is introduced.

## Design and scope

Prepare borrowed matrix/rate/importance views, threshold and exact zero-test
vectors once per synchronous luma search or selected reconstruction. Reuse128B
of caller-stack DP workspace after each preceding user finishes. Every reachable
node/predecessor is written before reading; independent signed-fragment digests
now reuse dirty scratch with untouched guard values. Zero blocks leave it alone.
Rate refresh occurs between macroblocks; views end before refresh/disposal.
Cancellation and disposed-owner checks precede zero/nonzero mutation.

Two private value-type policies specialize the real luma drivers. One source
retains prediction, batching, quantization, rate/SSD/texture pruning and strict
tie rules; no interface is called in pixel loops. Source projection verifies all
call arguments and the exact integer DP recurrence/termination/publication.
Statistics counter9 and emitted probability10 retain their distinct pinned roles.
Old private signatures are removed; tests exercise the actual prepared core.

The intermediate shared form confirms328µs/2.87% trellis gain but559µs/8.58%
ordinary-diagram loss, and is rejected before broad/package qualification. Earlier
views-only preparation gives no useful short lead. The completed policy revision
resolves that material loss: ordinary diagram remains uncertain around+0.37%,
interval0.99905–1.00839, without gain/loss/equivalence claim. Keep every stage and
adverse point. No unchanged favorable resampling or permanent family exclusion.

Actual code shows intermediate fixed frames0x7A8/0xB18 versus original0x488/0x7A8.
Final plain frames0x648/0x918 remain larger than original; the prepared constructor
is absent and code shrinks to5534/6735B from5893/8412B. Static code/frame sizes
are leads, not timing causation or complete physical-peak measurements.

## Independent complete-operation effects

Frozen513×259 inputs, quality75, existing effort4/6 settings and exact packets
remain. Twelve adjacent original/candidate pairs alternate order,16complete
operations per subject. Source setup/JIT/validation are outside timing; public
normalization/preparation/admission, fresh output growth, independent ownership
and disposal stay inside. All points are retained without filtering. These
managed-build comparisons establish scoped effects, separate from native parity.

| Complete control | Original µs | Candidate µs | Change µs | Candidate/original interval | Allocated B/op | Result |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Diagram, effort6 | 11,419.82 | 11,108.59 | -311.23 | 0.96520–0.98035 | 754,344, both | Gain |
| Diagram, effort4 | 6,578.14 | 6,602.54 | +24.40 | 0.99905–1.00838 | 740,984, both | Uncertain |
| Photograph, effort4 | 11,279.18 | 11,310.11 | +30.94 | 1.00013–1.00536 | 809,072, both | Loss |

Do not sum different workloads into an assumed workload mix. The current
photograph gap remains a priority. Independent31µs loss does not attribute the
larger old/new standalone movement to this change. Reuse the measured result;
no repeat to seek a favorable point. Lossless behavior and compression quality
are unchanged because complete encoder packets remain exact.

## Current native comparisons

Seven affected encoder comparisons refresh on the final immutable Linux module.
Distortion-only methods0to2 use unchanged code and reuse their qualified cost.
Twelve actuals, before/after native brackets, conservative filtered/all-actual
confidence, source/packet/quality/alpha/ownership checks and quiet guards remain.
Native FFmpeg/libwebp wrappers/scratch retain their stated prepared state;
complete pixel processing and independent output ownership stay timed. Managed
preparation is included. No new workload/settings grid or preparation exclusion.

| Existing control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| opaque8 | 15,330.32 | 15,505.27 | 0.989 / 0.994 | 1,201,552 | Pass/stop |
| opaque16 | 15,496.33 | 15,577.89 | 0.995 / 0.997 | 1,201,552 | Pass/stop |
| photograph | 11,504.87 | 9,688.60 | 1.187 / 1.192 | 809,225 | Gap |
| diagram | 6,620.62 | 5,812.75 | 1.139 / 1.146 | 741,137 | Gap |
| distortion | 2,987.26 | 2,503.72 | 1.193 / 1.197 | 741,386 | Gap; unchanged path reused |
| trellis | 11,227.37 | 9,796.03 | 1.146 / 1.154 | 754,497 | Gap |
| alpha | 54,454.46 | 53,256.71 | 1.022 / 1.037 | 7,034,392 | Gap |
| alpha16 | 54,962.29 | 53,589.76 | 1.026 / 1.047 | 7,033,984 | Uncertain |

Two pass/stop, five remain gaps and alpha16 is uncertain. Uncertainty is not a
passing result. Old/new standalone clocks do not prove incremental gains/losses;
the independent comparisons above resolve the material caller contradictions.
Heap totals, peak live storage and retention remain separate: existing16KiB
macroblock/2KiB trellis coarse admission covers prepared values and stack scratch;
no new owned heap capacity, and operation scratch retained after return is zero.
Whole-operation physical/admitted peak and native allocation are not newly measured.

## Qualification and retained evidence

Windows/Linux each pass **5,724 full/live checks** (2485internal/2291public/
948differential),110internal/137public affected checks in each normal/scalar/
AVX2-disabled/AVX512-disabled mode. Two added cases verify that a prepared zero or
nonzero call rejects a disposed rate owner before mutation, without new admission.
Existing signed native-level/dequantized/context oracles exercise repeated dirty
workspace and guards; full original packet/alias/overlap/cancel/refusal/async/
source/independent-output contracts remain.

The SAME actual unpublished0.2.1 Release package and all3usage examples pass on
bothOS. Its existing OriginalVp8Controls loop covers frozen513×259 method5/6
diagram and asymmetric packets, shared admission and awaited async output;
no consumer or fixture is added. Published releases remain immutable. Three
coherent core/embedded candidate versions perOS; only final C receives broad/live,
AVX2/AVX512-disabled and package qualification. Reuse unchanged harness/native/
selector/fixture tools.

Only seven displayed costs invalidate;118others, including distortion-only, reuse
relevant unchanged production paths. Inverse/decoder/VP8L/PNG/JPEG/edit/text are
untouched. Existing reader and PNG evidence remains current. This is not a125-row
actual-package release refresh,282-path pass or publication.

[Evidence](vp8-trellis-execution-evidence.json) pins producers, exact inputs/
packets, every comparison, source projections, code, storage and qualification.
Main Windows `376725b0ba0458f1be1c2265b7a20dd50b1d88065f84f678519a2ac55a97320c`;
Linux `cddbdf58abebf24d50e947f53066998c78cb1050cb56613f733a258e09e9bcfc`.
Actual package `1643d044906c7a9bda25de52f4acd30683758a1d288d1ece47a6fe4245911498` (417,338B).
Raw `vp8-trellis-execution-qualified.tar.xz`, `653aa9d1048f3f502a2bbf6db82d921e268b352b48d5fb081e240f57c160a868`
(5,637,656B/487members), mirrored/member-verified with pinned
dependencies and payload-by-hash reuse. All3stages, rejected current costs and
original artifacts remain reproducible.


The [preceding candidate pipeline](VP8-FIXED-VALUE-CANDIDATE-PIPELINE.md) retains
its own independent gain and earlier evidence. Stop this bounded round and
passing cases. Continue from current material photo/alpha/other codec attribution,
resolving uncertainty before tuning. Publication remains postponed.
