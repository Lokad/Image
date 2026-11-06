# Complete VP8 prediction preparation

Expand the encoder's ten small-prediction candidates from one boundary snapshot,
sharing DC and interpolation preparation. Complete photograph encoding independently
saves **682.12 µs / 6.23%**, with separated raw and reference-normalized confidence,
unchanged809,225 B/op and the exact17,688 B packet. Other current costs below own
their parity status; this is a scoped gain.

## Preparation and execution

The existing noncancelled batch produced all ten predictions before ordered mode
decisions, but invoked the single-mode predictor ten times. Each call snapshotted
the same thirteen samples and recomputed DC. Across those modes,58two/three-tap
expressions have22unique values after two-tap commutation and three-tap endpoint
symmetry. Prepare13border samples,9half averages and13smoothed values once, then
write all ten compact blocks in the original search order.

The implementation derives from the existing owned RFC6386section12 formulas and
shares elementary averages/clamping/packing. Single-mode decoder/live/refinement
source remains unchanged and retains real production callers.
Different complete-set and single-mode output scheduling justifies this bounded
specialization. No generic pixel-loop abstraction, new heap allocation, owner,
cache, pool, session, table, quota or numerical policy.

Keep the existing batch eligibility, ten-to-twelve forward padding, mode order,
strict ties, rate/flatness/pruning rules, selected pixels/levels and live checkpoints.
All borders are snapshotted before output writes, including aliased views. The140 B
geometric stack data fits the existing16KiB macroblock admission; trellis retains
its existing2KiB coarse workspace. Synchronous scratch ends with its users; retained
operation scratch is zero. Complete physical/admitted peaks and native allocation
are not newly measured. No CPU inference follows from capacity or code size.

The preceding current-module2second capture contains1963process/1933managed
samples,423in Luma4 and148in single-mode prediction entries. These are instruction
locations/skid, not phase times or projected savings. External libwebp supplies
insight only: its encoder expands a complete candidate set from an immutable border.
No upstream implementation/constants are copied or used as build inputs.

## Actual complete-operation effects

Frozen513×259photograph, Q75/effort4, actual standalone/default-context worker,
quiet pinned CPU0. Setup/JIT remain outside timing; public normalization, preparation,
admission, pixel processing, fresh output growth, independent ownership and disposal
stay timed. Native wrappers/private scratch retain their declared lifetime; every
pixel is processed. No ALC-only acceptance or new image/settings grid.

| Complete control | Original µs | Current µs | Original / current B/op | Interpretation |
| --- | ---: | ---: | ---: | --- |
| Photograph, independent12actuals each | 10,951.93 | 10,269.81 | 809,225 / 809,225 | Gain, raw and normalized |
| RGBA16 + alpha, contemporary control | 56,678.70 | 57,673.00 | 7,033,984 / 7,033,984 | Adverse point; inconclusive |

Photograph's independent raw reduction is6.23%, reference-normalized6.16%; faster
native bracket changes−0.074%. Both filtered/all-actual confidence views separate.
The initial three-actual screen had an adverse raw point (+2.41%) alongside an
8.60%native movement; it was inconclusive. Retain it and the independent confirmation.
Do not combine these gains with prior rate/score-publication comparisons.

The transparent word refresh has an adverse point, prompting one contemporary
original-build control. Candidate costs994.30µs/1.75%more by point, but raw intervals
overlap (original54.61–58.75ms, current56.42–58.93ms), as do normalized intervals.
No causal loss, gain or equivalence is established. Retain that uncertainty without
another candidate measurement for favorable numbers. Source/packet/quality/alpha/
ownership/quiet gates pass on every point.

## Current reference boundaries

Seven affected costs refresh with12actuals each; unchanged distortion-only uses
its separate decision path. Passing opaque cases are requalified for changed
production and stop. No broader incremental gain is inferred from older clocks.

| Existing complete control | Lokad µs | Reference µs | Ratio / upper | Lokad B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Opaque8 | 14,427.73 | 15,489.57 | 0.931 / 0.938 | 1,201,669 | Pass/stop |
| Opaque16 | 14,502.89 | 15,678.51 | 0.925 / 0.945 | 1,201,552 | Pass/stop |
| Photograph | 10,269.81 | 9,692.49 | 1.060 / 1.068 | 809,225 | Gap |
| Diagram | 6,118.83 | 5,820.81 | 1.051 / 1.066 | 741,222 | Gap |
| Trellis | 10,708.53 | 9,838.60 | 1.088 / 1.120 | 754,497 | Gap |
| Alpha | 56,148.06 | 54,903.24 | 1.023 / 1.053 | 7,034,392 | Uncertain |
| Alpha16 | 57,673.00 | 53,478.66 | 1.078 / 1.103 | 7,033,984 | Uncertain |
| Distortion-only, unchanged | 2,987.26 | 2,503.72 | 1.193 / 1.197 | 741,386 | Gap |

Eight controls:2pass/4gaps/2uncertain. Uncertainty is neither a pass nor an
optimization mandate. Actual standalone allocation differs by+117 B/op for opaque8
and+85for diagram versus prior receipts; five other refreshed totals are unchanged.
No new allocation site/capacity exists. Report these GC/batch totals separately from
live/retained storage, without attributing minor differences to a new owner.

## Qualification and retention

Windows/Linux each pass **5,735 full/live checks** (2488internal/2299public/948live),
142internal/137public per normal/scalar/AVX2-disabled/AVX512-disabled mode, and the
SAME actual private Release package/all three examples. Three new internal tests
check independent per-pixel geometry across all ten modes and128rounding contexts,
border aliasing/input/tail ownership, invalid spans before writes and zero allocation.
Existing native hashes and complete-packet/cancellation/admission oracles remain.
The package's22large original VP8 tuples retain exact sync/async packets, methods5/6,
both depths, borrowed roots, terminal refusal and independent surviving outputs.

One immutable core/embedded build perOS; no harness/native/selector/fixture build.
Seven displayed costs refresh;118other relevant paths reuse valid evidence. JPEG,
VP8L, decoding, resizing and text remain unchanged. This is not a125-row actual-package
release refresh or282-path pass. Published releases remain immutable; publication
is postponed.

[Evidence](vp8-complete-prediction-preparation-evidence.json) retains source projection,
all actuals/native brackets, adverse/uncertain controls, attribution and qualification.
Raw `vp8-complete-prediction-preparation-qualified.tar.xz`:2,606,896 B/229members,
`3aa49c6208945ab6e7de97f9884a64e4dbfaf3b4cc69ee8d407b807264227937`;
both mirrors and all members verified. [Rate preparation](VP8-CANDIDATE-SCORE-PUBLICATION.md)
retains prior independent gains and experiments. Continue substantial independent
codec/resize work, preserving uncertainty and avoiding unchanged favorable repeats.
