# JPEG dequantization and support attribution

[The subsequent qualified stage](JPEG-SHARED-BLOCK-SUPPORT.md) implements this
lead and owns current costs. This predecessor trace remains motivating evidence.

JDA798 reuses the accepted actual package DLL on the existing sparse Adobe CMYK
control. Its source-valid complete cost remains **471.28 / FFmpeg 432.85 µs**, upper
**1.104: gap**, 20,944 B/op. This adds no statistical timing, allocation saving,
production build, fixture or qualification repeat.

One two-second CPU0 capture, sampled from CPU1, has 1,995 locations: inverse 432,
natural support 258, plain entropy 230, two-row columns 217, baseline scheduling 207,
dequantization 188 and CMYK conversion 91. Inlining/skid prevent treating these as
phase costs, removable percentages or a gain forecast. Instrumented elapsed is
discarded. The exact 22,708 B packet / 531,468 B independent RGBA output, native/normal/
scalar, source/ownership and both idle-v2 checks pass. Native contexts remain
retained as declared; this is not an equivalent fresh-context comparison.

The current 227 B natural dequantizer uses four 16-word batches. A separate 132 B
non-inlined support query reloads the 64 materialized words, with its own span
checks and call boundary. This follows the shared-mask stage; earlier profiles
predate that change. Removing 128 B of logical rereads per block is a source model,
not a measured bandwidth or elapsed saving. Existing column/entropy work remains.

**Next candidate:** publish two 32-word SIMD products and exact support together,
using the same multiplied values before their register lifetimes end. Support
must reflect wrapped AC products, not raw coefficient bits. DC must replace its
mask bit only after the original full-width product, level shift and saturation.
Share that publication between natural and scan order; retain AVX2/scalar conversion,
original inverse arithmetic, stores and cancellation. No heap owner or retained
scratch is needed. Extra live vectors, ABI/register pressure or new branches may
lose; existing complete sparse-CMYK/dense-RGB444 screens decide the lead.

This differs from [unselected entropy/coefficient fusion](JPEG-COEFFICIENT-FUSION-EXPERIMENT.md):
reader grammar, token stores and Int32 coefficient storage remain unchanged.
Extend the existing exhaustive coefficient/quantizer/guard and independent-matrix
checks to the returned mask. Reconcile every actually affected decode/caller path
before an accepted W/L/full/ISA/SAME-package milestone. No new image/settings grid
or broader numerical policy. The candidate is not accepted by this attribution.

[Evidence](jpeg-sparse-current-attribution-evidence.json) pins current sources,
locations, actual-module bodies and frozen checks. Reuse BWF794 6,248 full/live per OS
and SAME actual package/all three examples; package a88e69e4 is unchanged. Mirrored/member-
verified raw `jpeg-current-support-attribution.tar.xz`: 111,936 B / 22 members, SHA256
`fa2b968c102b4faade2d9013287c6d8d56f02c0b1c1ae19cd4dcf4bd52a4ccbc`.
[Earlier attribution](https://github.com/lokad/image/blob/5d2981693f0cedce544d380c40e8d717761db8f8/benchmarks/JPEG-SPARSE-CURRENT-ATTRIBUTION.md)
and raw 3d1d2dcf remain history. No renderer dependency/displayed cost changes;
all 289 paths remain required and publication is postponed.
