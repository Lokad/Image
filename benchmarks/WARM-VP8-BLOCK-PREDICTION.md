# Reuse complete VP8 block prediction

The five existing VP8decode cases are **2.5-4.8% faster**, with unchanged pixels
and allocations. Live opaque improves from1.663x to1.586x FFmpeg. These are small
qualified improvements from one decoder call-site change; allfive remain gaps.

The decoder uses P58's already-qualified complete4x4prediction helpers instead
of dispatching the mode for each pixel. Thirteen temporary samples use52Bstack
within the existing8192Bfixed workspace. No new math, table, heap, state machine,
mandatory admission or test machinery is added. Entropy, logical work/cancellation,
transform/filter/output bodies and owners remain. The original per-pixel Small
oracle and all10mode/default/right-edge tests remain independent and unchanged.

| Existing selector | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Managed time improvement |
| --- | ---: | ---: | ---: | ---: |
| Opaque / live token | 7,613.64 / 4,801.66 | 1.586 / 1.664 | 760,548 | 4.75% |
| Opaque | 7,568.84 / 4,798.45 | 1.577 / 1.672 | 760,548 | 3.31% |
| Opaque / simple filter | 7,807.89 / 4,893.93 | 1.595 / 1.621 | 760,548 | 3.20% |
| Alpha | 9,647.14 / 5,967.97 | 1.616 / 1.841 | 1,966,616 | 2.52% |
| Alpha / fresh Stream | 9,741.05 / 5,982.30 | 1.628 / 1.682 | 2,327,699 | 3.29% |

All5shorts retain all3actuals at200ms, native50ms brackets and both
`exclusive-vm-idle-v2` guards. Means remain above1, so no long confirmations.
The same two513x259rasters and frozen packets remain:72,380Bopaque,76,128Bsimple,
98,590Balpha. Full acquisition/parsing/entropy/reconstruction/prediction/filter/
conversion, independent output and disposal remain timed; Stream includes fresh
acquisition, reads, copies and EOF. Frozen managed hashes, exactalpha, native
RGBmax4/RMSE1.5, normalCLI, source and owners pass. Optional scalarCLI checks
retain their original case policy. B/op includes harness; native allocation is
unmeasured. No reference, quality, tolerance or timed boundary changes.

**3,720 Release tests perOS** pass:1,386internal+1,436public+898liveFFmpeg, plus
38internal+251public scalar controls. Existing697,120formula comparisons and
all-mode oracle checks are reused; no new tests or profiles. The same actual
Release package's consumer and allthree examples pass on Windows/Linux. One
producer build perOS, zero harness/selector/native builds, all control means
read before qualification, no new workload/image/settings grid.

Only5decode rows change. Other202comparisons/all60passing proofs remain valid,
including all4P58encode results: their methods/helpers are unchanged. Current
totals stay60passes/143gaps/4confidence,127independent+80qualified short. No
prediction/entropy tuning grid follows this small reuse. The0.1.0release tag and
packages are preserved. [Numeric evidence](warm-vp8-block-prediction-evidence.json)
pins sources, allocations, actuals/guards, conservative bounds and actual package.
The full207-case <=1x objective remains open.
