# WebP color histogram construction

Color-trial counting now uses a verified 256-bin view and direct masked increments.
New nonzero bins are discovered afterward with SIMD/scalar masks, using a 32-byte
prior-support snapshot. This removes the index/zero-test/active-append chain from
each pixel update. Counts and new-bin membership stay exact, including positive
prior populations. Ascending active order preserves all integer costs; candidate
order/ties, signed/modulo arithmetic and complete packets remain unchanged.

| Independent complete photograph, twelve interleaved blocks | Before | Candidate |
| --- | ---: | ---: |
| Reference-normalized ratio / 99.9% interval | 1.0972 / 1.0794–1.1150 | 1.0133 / 1.0046–1.0219 |
| Incremental result | — | 11.32% less elapsed / 7.65% normalized; 10.75 ms saved |

Both raw and normalized confidence separate. The reference is libwebp exact=1,
FFmpeg's encoder backend. The native clock is 3.98% faster in the candidate run;
the normalized result accounts for that drift. All twelve ratios are kept without
filtering, two complete managed calls per block and fastest of six adjacent native
samples. These ratios stay distinct from displayed global-bracket measurements.
The initial three-actual screen suggested 16%; that magnitude is not the claim.

**Strict parity remains open.** The fresh global photograph ratio is 0.9969 with
conservative upper 1.0044; paired upper 1.0219 also exceeds 1. Other current lossless
rows are independently confirmed: original/diagram/depth/async pass and stop;
opaque remains a gap. Six existing lossless boundaries are refreshed once on the
same immutable core. Unchanged ALPH/VP8 paths reuse CTS197 evidence. Existing
original RGBA8 generalization has a 2.3% short point lead, not a separate speed claim.
All preparation, source processing and independent output/disposal remain timed.
Frozen inputs/settings and 160,032 / 183,812 / 121,896 / 7,234 B packets remain exact.

Allocations are unchanged in the matched controls. The bounded snapshot adds
32 B stack; search admission rises 7,296→7,328 B before use. Complete actual-DLL
admitted peaks remain 3,470,059 B/photo and 3,656,309 B/alpha, retained scratch 0.
No extra heap, owner, pool, cache or cross-operation state. Physical peaks/native
allocations are unmeasured. Generated code confirms direct increments; its
histogram body grows 1,078→1,365 B because support preparation is now explicit.
No old sample offsets are applied to changed code and no new CPU capture is run.

Each OS passes 5,541 full/live checks and 812 internal / 194 public WebP checks per
normal/scalar mode. Four added cases cover dense/sparse masks across all word
boundaries, positive priors, guards, repeat accumulation, empty rectangles,
validation and cancellation. Existing signed-floor, zero-allocation, admission/
cleanup and 65,536 frozen scalar cost cases remain exact. Initial private order/
budget expectations are updated; public pixel/packet gates are preserved.
The same actual 0.2.1 development package and all three examples pass both OSes.
Published packages stay immutable; publication is postponed.

[Evidence](webp-color-histogram-separation-evidence.json) pins all comparisons,
the reused baseline attribution, actual code/memory and qualification. Raw
`webp-color-histogram-separation-qualified.tar.xz`: 4,190,704 B / 308 members, SHA256
`3e026e25c70e9ec7ad3ad02c104f150cbdad08e3a828da5917c215e821f9541f`,
mirrored and member-verified. One production build per OS; existing audit/worker/
selector/paired tools reused. All adverse data and reproducibility are retained.
