# Bounded word refills for VP8L

Complete resident VP8L decode independently uses **8.32% less filtered / 8.26%
less all elapsed time**, with unchanged **684,377 allocated B/op**. Raw 99.9%
intervals and conservative normalized intervals separately separate. Eight
existing companions have current independent costs, without paired gain claims.

| Existing complete operation | Lokad µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| VP8L resident, native input | 3,523.534 | 1.164 / 1.177 | 684,377 | Gap |
| VP8L literal/run | 1,567.627 | 0.729 / 0.732 | 542,502 | Pass |
| VP8L sixteen colors | 548.456 | 1.009 / 1.012 | 820,194 | Gap |
| VP8L live token | 3,542.067 | 1.173 / 1.181 | 684,377 | Gap |
| VP8L fresh seekable stream | 3,510.168 | 1.159 / 1.171 | 684,422 | Gap |
| VP8+ALPH resident | 7,069.515 | 1.187 / 1.196 | 1,434,731 | Gap |
| VP8+ALPH fresh seekable stream | 7,047.567 | 1.177 / 1.187 | 1,434,827 | Gap |
| VP8L complete coding validation | 3,558.644 | 1.169 / 1.176 | 685,421 | Gap |
| VP8+ALPH complete coding validation | 7,261.076 | 1.332 / 1.339 | 770,092 | Gap |

The literal/run control independently passes; stop it unchanged. The other eight
remain gaps. This qualifies nine named paths, without a general decoder-parity
claim. The full 268-path objective stays open.

The reader appends 40 bits with one safe, bounded UInt64 load and a mask, instead
of refilling one byte at a time. Refills begin with fewer than 24 cached bits,
so valid storage never exceeds 63 bits. The three unused loaded bytes remain
inside the declared payload and available for later reads; short tails keep
byte refills. No field, allocation, owner, table or admission mechanism is added.
All cancellation-before-commit checkpoints and public input delivery stay fixed.

Eleven new controls compare every field width/phase and exact EOF against an
independent bit reference, test long-code tails and canceled speculative word
reads. Two earlier controls retain symbols/following fields without forbidding
private in-payload lookahead. Existing canceled-state/single-leaf checks stay.
A separate deterministic proof checks 52,507 field/remaining-bit transitions.
No encoder packets, pixels, metadata or supported public behavior changes.

The original resident packet is 183,812 B. Companions retain the frozen 533,100 B
literal/run, 11,810 B indexed and 98,590 B VP8+ALPH inputs; validators retain their
existing 183,310/115,542 B metadata controls. Exact RGBA, alpha and hidden RGB,
or exact nine-field descriptors, source/independent owners, native boundaries
and both quiet guards pass. Streams include acquisition/copies/EOF; validators
include required coding storage and cleanup without RGBA output. Native allocation
is unmeasured. No input/settings grid or quality change.

Both OSes pass **5,210 checks each** (2,108 internal, 2,154 public, 948 live),
60 internal/272 public focused checks in normal/scalar modes, the same actual
Release package, governed async and two-frame-alpha consumers and three examples.
The separate governed lossless control remains 1,217,192 allocated B/op and
1,063,480 B shared peak. Capacity, cancellation and ownership remain unchanged.

One immutable core per OS and cached workers/harnesses/selectors: zero additional
tool builds, profiles or variants. A fresh three-actual pair clears the fixed 5%
gate; a stable original screen permits reuse of the prior independent original
twelve, followed by one candidate twelve. Eight existing companion twelves follow
once. No successful timing repeats. Other affected boundaries need current costs;
unchanged metadata/opaque-VP8/encoder/other-codec/edit/text paths retain proof.

[Evidence](warm-vp8l-word-refill-evidence.json) pins decisions, producers and the
mirrored/member-verified vp8l-word-refill-qualified.tar.xz.
Close this primitive without further width or prefetch variants.

Final review strengthens the eight mixed-prefix cases with an explicit following
field. They pass normal/scalar on both OSes after test-only builds; production,
package and timing checks are reused. The verified supplemental archive is
vp8l-word-refill-mixed-tests.tar.xz, SHA256 c11ee320ee5296cf41701945d017e248ad8f1e7e1b341386f7b767220ca3f9e5.
