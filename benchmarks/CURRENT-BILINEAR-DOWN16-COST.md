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
