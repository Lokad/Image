# Current tight horizontal RGBA16 bicubic cost

One independent current confirmation resolves the earlier short measurement:
**739.572 µs**, **1.267× / conservative upper 1.326**, **530,838 B/op**.
The conservative lower is **1.201×**: this is a confirmed CPU gap, with no causal
gain/regression claim against the older unmatched screen.

This existing control resizes original 513×259 RGBA16 to 256×259, with local
working limit input + output + 4 KiB. Complete public/native operations include
normalization, allocation, independently owned output and release. The reference
is zimg, FFmpeg's zscale backend, with frozen settings, packing and clamped halos.
Exact managed rational/frozen pixels, native RGB/alpha max1 and RMSE≤0.25,
normal/scalar bounds, source/output ownership and quiet-v2 guards pass.
All twelve actuals and both twelve-sample native brackets are retained.

No production change, build, new profile, input or settings grid. The current
immutable JPEG-round core and cached harness/selector/workers are reused.
Current 5,291 checks per OS and the same actual Release package/three-example
qualification apply without repetition. The resize kernel, rounding, operation
context and image owner text remain unchanged since output-cache acceptance;
subsequent shared-alpha changes use their current qualification, not an obsolete
unchanged-source claim. A preflight assertion identifies that distinction before
timing. One postcheck normalizes only a public native-library prefix; the completed
score is recovered without rescoring, with all worker/build/pixel gates preserved.

[Numeric evidence](current-tight-horizontal-word-bicubic-evidence.json) retains
all actuals, brackets, bounds, identities, recovery and reused qualification.
Raw current-tight-horizontal-word-bicubic.tar.xz SHA256
a9913abdc9057bb274913240f4dec51b2efb6300d42478d59fe4a76c15a7cf37
(1,433,264 bytes, 31 members) is mirrored/member-verified; dependencies retain the
[qualified current package](WARM-JPEG-COMPLETED-ROW-REUSE.md) and
[original output-cache controls](WARM-BICUBIC-OUTPUT-ROW-CACHE.md).
No new parity pass or workload is added; the full 268-path objective stays open.
