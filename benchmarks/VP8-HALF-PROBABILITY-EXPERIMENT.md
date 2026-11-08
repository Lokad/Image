# Fixed-half live VP8 sign experiment

VRA585/VHP586, unselected. A fixed-probability sign primitive replaces generic
boolean range arithmetic with equivalent half-range formulas. Normalization,
refill, canonical state, decision order and cancellation checkpoints remain exact.

The existing complete opaque cancellable decode screen costs 4,985.63µs versus
4,783.84µs for FFmpeg, with 228,864 managed B/op and a separately owned 531,468B
output. The qualified original costs 4,979.43/4,789.64µs. Three actuals establish
no useful gain or parity; no independent confirmation or variants followed.

Windows/Linux each pass 16 internal and 239 public controls per normal/scalar
mode. These include 40,800 range/cache/cutoff states with following reads and
305 cancellation ordinals against both the original and half primitive. No
new owned capacity or retained state is introduced; physical peaks were not
remeasured. Retained source-valid profile locations were reused, not recaptured.

Qualified sources, modules and bindings are restored byte for byte. Prototype
code/tests are removed; the existing 5,907 checks per OS and actual-package
qualification remain applicable. The current benchmark summary is unchanged.
This failed implementation does not close other live-decoder designs.

[Evidence](vp8-half-probability-experiment-evidence.json) retains source, test,
measurement and restoration identities. Raw `vp8-half-probability-experiment.tar.xz`,
SHA256 `2e2d2e7287320f236eae5a5b84688dd8a72fa6e91c899f04f5ce10270952d895`,
4,774,444B/97 members, is mirrored and member-verified. Publication is postponed.
