# Current RGBA16 bilinear reduction

The existing 513×259→256×129 control independently confirms **1.308× /
upper 1.318**, **544.837 µs** and
**290,124 allocated B/op**. This is a demonstrated gap.
The older independent run predates accepted row reuse; its replacement and the
release refresh had three actuals. This resolves that missing confirmation.

Original pixels, settings and complete boundaries stay fixed: public Resize,
admission, prepared geometry, independently owned output and disposal; native
FFmpeg filtering includes packing/planar conversion, widened triangle filtering,
encoded-space alpha normalization, final RGBA64 and output ownership/release.
Managed pixels match the rational reference exactly. Approved native-only RGB
and alpha maximum1/RMSE≤0.25, zero-alpha RGBzero, frozen hashes, normal/scalar
CLI, source preservation and independent surviving owners pass.

One twelve-actual CPU0 run retains every actual, filtered/unfiltered interval,
native pre/post bracket and passing idle-v2 guard. The immutable VVR1 build,
cached harness/worker and retained typed Down16 confirmation driver require
**zero builds**. Applicable 5,219 checks per OS, normal/scalar controls and the
same actual Release-package/governed consumer/three examples remain valid.
There is no production change, retuning or causal historical gain claim.

[Evidence](current-bilinear-down16-cost-evidence.json) pins producers, the precise
old-evidence invalidation and mirrored/member-verified current-bilinear-down16-cost.tar.xz,
SHA256 49bf3dda3544456bab060147d1df9359207433b3defefbff90baad8caff59573. Other paths retain applicable evidence; the full
268-path warm single-CPU parity objective remains open.

A single six-second current-kernel profile uses the retained public-operation
driver and immutable build, with zero builds and exact pixels/source preserved.
Of 5,264 samples, 91.2% lie in the row-cache kernel, 4.4% in kernel evaluation
and 2.6% in stride access. These are diagnostic sample fractions, not elapsed
costs. Original-order replay contributes under 0.1%; it is not the next target.

The bounded source review identifies duplicate preparation: this control evaluates
vertical geometry across 43 strips (22,317 kernel calls versus 519 if prepared
once) and allocates a 25,792-byte admitted horizontal plan the specialization
does not consume. Preparing vertical geometry once before that unused plan is
the next candidate. The estimated capacity reduction is about 17 KiB; no CPU or
allocation improvement has yet been measured. Existing arithmetic, certificates,
replay, memory refusal, cancellation and ownership stay fixed. Raw diagnostic
evidence is retained in `bilinear-word-geometry-review.tar.xz`, SHA256 1bb9bf3e65bc451fee40dd025c48dce456ded035a350ab85a6d21442c1824a74.
