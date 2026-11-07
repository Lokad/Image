# Direct coefficient classes in live VP8 decoding

The live pixel/validation loop now traverses zero, unit and larger coefficients
directly, with one plane-base setup and common sign handling. It avoids producing
a numeric token and reconstructing its coefficient/context class. The obsolete
live `Token` helper is removed. Noncancelable `Token` and the actual live
`Read`/`TokenRead`/`Category` helpers remain; their arithmetic and polling bodies,
the input guard and 272 B window bound are unchanged.

| Complete live opaque VP8 decode | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Original, independent twelve | 5,085.46 | 4,789.01 | 1.062 / 1.067 | 228,864 |
| Accepted, independent twelve | 5,011.41 | 4,793.54 | 1.045 / 1.055 | 228,864 |

The coherent stage saves **74.05 µs / 1.46% raw / 1.55% reference-normalized**.
Filtered/all-actual raw/normalized intervals separate; native moves +0.095%.
The remaining **218 µs** gap is material. This is one scoped gain, without a
general decoder, individual-component or parity claim. The preceding three-actual
screen is retained; no completed job is repeated for favorable confidence.

The original 513×259 opaque input remains 72,380 B and its exact independently
owned RGBA output 531,468 B. Input delivery, parsing/model preparation, entropy,
reconstruction/filter/color, output acquisition and disposal remain timed on one
CPU. Native codec/conversion/input/frame wrappers retain their declared preparation.
This is not a fresh-context comparison. Pixels, probability/category/sign order,
every entry/decision cancellation checkpoint and exact successful cursor remain.
Predictor class precedes sign/dequantization/short wrapping; zero/EOB skipping,
plane-zero start and validation's untouched destination remain explicit.

Live symbol coverage now exercises the actual complete pixel/validation kernels
against the original scalar oracle, witnessing all twelve symbols. Existing
all-plane/context/phase, category/sign/max-quantizer, cursor/following-read,
malformed, cancellation and root/ownership controls remain. Noncancelable helper
coverage is retained. A missing publication in that retained test was corrected
before timing; 928/929 checks initially passed. The production DLL was unchanged
during this test-only correction; the failed source/log remain retained.

Windows/Linux each pass **5,867 full/live checks**, plus **929 internal / 239 public**
per normal/scalar mode. The SAME actual private Release package and all three
examples pass both OSes. One immutable core per OS; two Windows embedded test
builds for the test correction and one Linux embedded build. Zero public/harness/
selector/native/fixture builds. Broad/package qualification runs once; other costs
and unchanged ISA evidence are reused. There is no separate live-validation CPU claim.

No new owner, table, array, pool, session or retained state. GC above is newly
measured; admitted coarse peak 747,020 B and retained result 531,468 B reuse
[output ownership](VP8-OVERWRITE-OUTPUT.md). The capped default pool and governed
bypass are unchanged. Physical/native peaks are unmeasured. Encoder packets,
metadata exceptions, safe managed execution, terminal refusal, cleanup, borrowed
roots, survivors and actual async storage lifetimes remain qualified.

[Evidence](vp8-direct-coefficient-classes-evidence.json) pins sources, complete
actuals, original/candidate modules and same-package qualification. Earlier
[successful publication](VP8-COMPLETED-BLOCK-PUBLICATION.md) retains its gains,
adverse/inconclusive representation studies and the profile that selected this
stage. Those profile shares predate the changed kernel; they are not current
method attribution or predicted savings. No closed-family rule or fixed
continuation threshold applies.

Raw `vp8-direct-coefficient-classes-qualified.tar.xz`, SHA256
`f44881978218e0dc28783698c5eaa03a6c2e88d0fa339e0dac78d269f0ab185a`,
7,462,352 B /161 members, is mirrored/member-verified, including sources, scripts,
gates, test correction, actuals, regression/package logs and pinned providers.
All 125 displayed numeric rows reuse unchanged paths; the live cost appears in
summary prose. This is not a release-wide actual-package performance refresh or
completion of all 283 material paths. Publication remains postponed.
