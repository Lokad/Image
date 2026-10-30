# Current RGBA8 bicubic reduction

The existing 513×259→256×129 Down8 control independently confirms
**1.384× / upper1.400**, **579.250 µs** and
**332,188 allocated B/op**. This is a demonstrated gap. Older independent runs
predate accepted writer changes; the latest zero-tap round retained three
Down8 actuals. Its independent HorizontalUp8 run does not confirm Down8.

Original input/output/native hashes, exact rational/qualified managed pixels,
approved native RGB/alpha bounds, normal/scalar CLI, zero-alpha policy, source
preservation and independent owners pass. Complete public Resize includes
admission, preparation, owned output and disposal; the unchanged zimg reference
includes its approved FFmpeg-zscale packing/halo/filter/normalization/ownership.

One twelve-actual CPU0 confirmation retains filtered/all intervals, every actual,
native brackets and both passing idle-v2 guards. The immutable BRC2 core, BZT1
harness/worker and retained typed Down8 driver require **zero builds**. No new
input/settings, optimization, causal historical gain or successful retiming.
Applicable 5,220 checks per OS, normal/scalar and the same actual Release-package/
governed consumers/three examples are reused. Relevant kernels and benchmark
source are unchanged since the accepted zero-tap round.

[Evidence](current-bicubic-byte-reduction-cost-evidence.json) pins current
producers, exact invalidation and mirrored/member-verified current-bicubic-byte-reduction-cost.tar.xz,
SHA256 35a822c4c73444334601098658926ced20e26685a07b8e3dd33ecc885778f32d. Full 268-path warm single-CPU parity remains open.
