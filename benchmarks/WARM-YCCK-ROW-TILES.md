# YCCK conversion over rows

Current affected CPU costs are in [baseline DC/AC fusion](WARM-JPEG-BASELINE-DC-AC-FUSION.md); the measurements below are historical.

The existing YCCK420 control falls from **3,420.79 to 462.26 µs**: **86.5% less
elapsed time**, with every pixel unchanged. Its independently confirmed
**0.653× / upper 0.660** cost passes the one-CPU warm target. Stop this control.

| Complete owned RGBA8 decode, 513×259 | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original, independent | 3,420.79 | 707.48 | 4.835 / 4.864 | 895,458 | 6,704 |
| Rows, independent | 462.26 | 708.38 | 0.653 / 0.660 | 895,545 | 6,704 |

Owned YCCK with full Y/K and half-resolution CbCr now samples and adjusts black
over 64-pixel tiles, feeding the existing SIMD color kernel whole rows. Previously
each pixel repeated four-component sampling and invoked color conversion with
one pixel. Horizontal byte rounding still precedes vertical rounding; signed Q16
black products and the unchanged Q13 color conversion preserve exact pixels.
Portable and tail arithmetic remain. Other sampling and caller sample APIs keep
their existing general paths.

Each row uses **192 bytes of bounded stack scratch** and no new owned heap
buffer, frame copy, cache, budget or owner. This minor temporary follows the
cooperative integration brief's approximation allowance. Source planes, retained
output, admission refusal and cleanup keep their existing owners and contracts.
Cancellation is checked every 64 pixels. Full-operation allocation measures
+87 B/op; this is not an allocation improvement.

The original 6,704-byte DC-only packet, authored data, settings, native worker,
normalization and complete-operation boundary remain frozen. Independent authored-
block pixels and SIMD/scalar native RGBA are exact. This is one tested content/
sampling scope, without a general JPEG CPU-pass claim.

A fresh three-actual pair clears the predeclared screen. One candidate twelve
reuses the valid original twelve: **86.4867% filtered / 86.5019% all elapsed**,
**86.5038% native-normalized** reduction. Raw 99.9% intervals and conservative
normalized envelopes separately separate. All actuals, native brackets and both
idle-v2 guards are retained; no job, variant or profile is repeated.

Both OS pass **5,180 checks each** (2,090 internal, 2,142 public, 948 live), plus
336 internal/339 public focused checks normal/scalar. Two new kernel cases cover
byte ranges, rounding, 16 boundary widths, tile tails, source/guards, cancellation
and zero heap allocation. Existing varying/progressive/restart/native/root tests
remain. The same actual Release package, governed async consumer and three
examples pass on both OSes. Its existing YCCK array/sync/async controls exercise
this new path and exact native pixels with retained shared charges. One core
build per OS; zero harness/native/selector builds. A test-only missing import was
fixed before scoring without rebuilding production.

Other affected owned YCCK boundaries need applicable costs before CPU-pass claims;
unchanged caller, quarter-sampling and other codec/editing evidence is reused.
The next largest demonstrated gaps are 4:1:0 at 4.703× and 4:1:1 at 2.807×.
The full **268-path ≤1×** objective remains open.

[Evidence](warm-ycck-row-tiles-evidence.json),
[original current costs](WARM-EXTENDED-JPEG-CURRENT-COSTS.md).
Mirrored/member-verified ycck-row-tiles-qualified.tar.xz, 141 members,
SHA256 27fd94cd28c25931381991c36ac8d0e947f83550d55c96ae32cbb2358fca73e5; reference/build dependencies are pinned.
