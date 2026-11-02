# Exact compositing with shared row preparation

One typed row/frame implementation replaces per-channel Sample/SetSample geometry,
disposal and depth checks for all four source/canvas depth pairs. Alpha weights
are prepared once per pixel. Equal depths cancel the common sample maximum from
numerator/denominator; the rational value and positive half-up rounding remain
identical. Mixed depths keep the original full weights. No new workspace,
reservation, owner, retained cache or duplicate depth-specific kernel.

| Existing complete warm composite | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Raw reduction |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 / FFmpeg overlay | 625.41 | 498.73 | 1.254 / 1.270 | 56 | 87.7% / 4.46 ms |
| RGBA16 / ImageSharp 3.1.12 Rgba64 SrcOver | 627.24 | 696.25 | 0.901 / 0.922 | 56 | 88.0% / 4.62 ms |

Both independent twelve-actual confirmations retain exact managed frames and all
original gates. RGBA16 passes scoped parity and stops; RGBA8 has a confirmed gap.
The latter reference is explicitly ImageSharp, not FFmpeg. Native-only RGBA8
RGB≤1/RMSE≤.5 with exact alpha/untouched/zero-alpha pixels, and RGBA16
RGB/alpha≤1/RMSE≤.25 each, remain unchanged. Managed samples remain exact.

Valid prior confirmations are reused: 5,086.39 / 5,247.40 µs. Both raw/all-sample
and normalized confidence separate. Native clocks shift +1.23% / +0.03%;
reference-normalized gains are 87.9% / 88.1%. Two short screens precede confirmation;
their wide confidence is retained without a parity claim. One immutable production
build per OS, zero harness/driver/native builds and no new performance controls.

Complete public Composite mutates the borrowed canvas at17,19 using the original
513×259 resident rasters. Reset is outside each timer on both sides, before every
operation; managed invocation/unroll is one, with exact full-frame validation after
each iteration. Native per-operation clock reads remain included. Admission,
cancellation and context cleanup are timed; setup, checks, IPC and I/O are excluded.
Claims cover these two controls; mixed-depth/overlap performance is unmeasured.

Current allocation-only diagnostics reuse the existing frozen tool, with three
sixteen-operation samples: 56 B/op for each, unchanged. No elapsed claim comes
from this diagnostic. Incremental/retained compositing workspace is zero; B/op is
not a measured physical/root peak and native allocation remains unmeasured.

Windows/Linux each pass 5,423 full/live checks and 76 affected checks in both
normal/scalar modes. Four new controls compare 262,144 pixels / 1,048,576 samples
against the independent BigInteger oracle, covering full sixteen-bit source alpha,
all byte alpha pairs and mixed depths. Existing clipped, bidirectional cross-chunk
self-overlap, zero-source-alpha/hidden RGB, cancellation/admission and ownership
checks pass. The same actual 0.2.1 Release package and all three examples pass on
both OSes; its consumer verifies every depth pair and shared owner lifetime.
Published packages stay immutable; publication is postponed.

[Evidence](compositing-shared-rows-evidence.json) retains all actuals, references,
source/producer pins, independent allocation diagnostics and qualification.
Other codecs, resizing, fill/copy, font/layout and text raster paths are unchanged.
Raw archive identity and dependency pins are recorded in the evidence.
