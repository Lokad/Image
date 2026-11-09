# RGBA8 compositing on the current package

CCR645 refreshes the complete in-place RGBA8 Composite operation on the already
qualified private Release 0.2.1 package. Twelve actual iterations and bracketing
FFmpeg measurements pass the quiet-VM guards: **558.02 / 452.03 µs**, ratio
**1.234**, conservative interval **1.211–1.265**. The remaining gap is 105.99 µs.
Older 625.41/498.73 µs clocks are superseded; unchanged production code means
this difference is not a causal optimization gain.

The frozen 513×259 source is composed at (17,19) into a borrowed canvas. Both
subjects reset that canvas before every operation, outside timing; invocation
and unroll counts are one. Timing includes the complete public call, admission,
cancellation checks and context cleanup. Source/canvas setup and validation are
excluded equally. FFmpeg uses per-operation clock reads. No output ownership is
transferred: the caller retains its canvas and unchanged source.

Managed pixels remain exact against the rational oracle. The approved native-only
RGB bound is ≤1 and RMSE ≤0.5; alpha, untouched pixels and zero-source-alpha pixels
remain exact. Reference settings, inputs and output identities are unchanged.

Allocation remains **56 B/op**, reused from the CRK93 shared-row allocation-only
refresh using the ALC68 diagnostic (three sets of sixteen complete calls, reset
excluded). The ImageDrawing source hash is unchanged. Extra allocation passes
are deliberately disabled in mutable-canvas timing. No new owned pixel storage
or retained scratch is introduced; this refresh does not measure native or
physical peak capacity.

The timed DLL is 8baf7021159a78bf0f2c2524c9ccb3ec33537b41776c3f977c8c8605697aae1a,
from actual package 1b010016f3d17117ecf476660041322d28411bce03fea43cdc210ac731d4a9eb.
[Existing qualification](BICUBIC-STRIDED-COLUMN-CACHE.md) supplies Windows/Linux
5973 full/live checks each, scalar/ISA checks and the same package's consumers
and three examples. Production inputs are unchanged: zero builds, new fixtures,
profiles, suite repetitions or package verifications were necessary.

[Structured evidence](rgba8-composite-current-package-evidence.json) identifies
the source, current cost, reused allocations and qualification. Mirrored,
member-verified raw archive rgba8-composite-current-package.tar.xz contains
6,862,904 bytes / 66 members, SHA-256
b506782e643515b3c1c46e954332d6e44be6587bb1fab609192d469bad2596a2.
It retains the corrected harness, native worker, exact runtime, receipts,
observations and guards, with the qualified column-workspace archive as a pinned
dependency. The next hypothesis targets three scalar integer color divisions per
nontransparent pixel, preserving exact rounding and all ownership boundaries.
