# Exact alpha accumulation with Int128

The existing tight-memory RGBA8 enlargement is **7.26% faster** independently,
with **360,288 fewer allocated bytes/op (14.45%)** and identical pixels.
Its comparison improves1.565×→**1.473× / upper1.481**. This is an accepted
improvement;the ≤1× objective remains open.

ResizeAlpha.Round now avoids BigInteger objects where a proved Int128 range
suffices. For d≤2^14 and≤2^16 tap pairs,cubic|w|≤2*d³ gives|total|≤2^102,
|alpha|<2^118 and a rounding numerator<2^120,below signedInt1282^127.
Larger arithmetic retains BigInteger. Exact signed terms,rounding/clamping,
cancellation and the same4KiB admission/refusal/cleanup remain. This is an
arithmetic overflow guard,not an input or work limit. No approximation,new
buffer/owner/capacity,public API or quality tolerance is introduced.

| Existing complete warm one-CPU resize | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| bicubic-TightUp8 | 4,472.999 | 1.473 / 1.481 | 2,132,668 |
| bicubic-TightDown8 | 659.877 | 1.584 / 1.591 | 132,306 |
| bicubic-Wide8 | 529.180 | 1.518 / 1.534 | 231,239 |
| bilinear-Up8 | 2,914.617 | 1.347 / 1.358 | 2,235,512 |
| bicubic-TightHorizontalUp8 | 1,644.062 | 1.078 / 1.094 | 1,064,526 |
| bicubic-TightVerticalUp8 | 2,165.541 | 1.238 / 1.256 | 1,065,523 |

The primary remains513×259RGBA8→1027×519 with2,667,616B local working memory
(source531,468+output2,132,052+scratch4,096). The fixed native FFmpeg-zscale/zimg
boundary includes halo/packing/premultiplication,filtering,independent output,
unrounded-alpha quantization and release. Native allocation is unmeasured.
All six managed frames/input/native hashes,existing native bounds,source/owned
output and quiet-v2 gates pass;the vertical case retains its one previously
certified rational RGBhalf. No new input/settings/workload grid is introduced.

The short4.61%raw/4.68%normalized gain meets the predeclared exception for removing
at least64KiB/op. Independent7.26%filtered/7.46%all/5.89%normalized clears the
default5%gate. Raw and normalized confidence envelopes separate;candidate upper
1.481 is below original lower1.546. The valid original twelve is reused after
a stable fresh original short(−1.25%filtered,−1.46%all,native−1.48%). No score
is repeated. Follow-up rows are current costs,without causal speedup claims
against their older producers.

**5,267 checks per OS** pass(2,134internal+2,185public+948live),plus22new analytic
controls and179focused public controls normal/scalar and LinuxAVX512off.
Coverage includes8/16bits,bilinear/bicubic,exact halves,negative lobes,arithmetic
guard edges,BigInteger fallback,unchanged4KiB refusal,borrowed roots,cancellation,
subsequent reuse and zero heap arithmetic. The **same actual Windows Release
package/DLL** passes both OS consumers,three examples and a new exact-alpha/
ownership check. One core/internal-test build per OS;zero public/differential/
harness/native/selector builds. Published releases remain immutable.

The callee is shared: other bilinear/bicubic costs are prior-producer context
until their frozen fixture is proved not to enter it or receives current timing.
Seven displayed cases and the previously passing VerticalUp16 are named in the
numeric record. Do not infer every caller is active. Codecs,Lanczos3,drawing,text
and geometric copies are unchanged;their applicable evidence is reused. The
full268-path objective remains unchanged;this primitive is closed to variants.

[Numeric evidence](warm-exact-alpha-int128-evidence.json) records every actual,
confidence view,proof,producer,scope and qualification. Raw
exact-alpha-int128-qualified.tar.xz,SHA256
639fe3a36755b6f78153f8f4a96e1a2c37fe43cbadcde184b449441113ffdeab,
is mirrored/member-verified with source,actual builds/package,tests,selectors,
workers,scores and guards. Reproduce the named existing cases with the
[reference tooling](../tools/WarmReference/README.md).
