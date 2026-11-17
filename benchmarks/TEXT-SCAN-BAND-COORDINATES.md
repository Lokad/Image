# Shared text scan-band coordinates

Accepted: prepare top/midpoint/bottom edge coordinates together, retaining exact
ordered arithmetic, crossing ties, winding and pixel blending. Final-DLL controlled
ordinary-RGBA8 pairs improve **47.950 ->45.395 µs**,
**5.33% /2.555 µs**, paired interval
**0.939408–0.954021**. This incremental effect is separate
from the standalone comparisons below; historical clocks are never subtracted.

| Complete public operation / reference | Lokad /reference µs | Conservative ratio interval /status | Public GC B/op |
| --- | ---: | ---: | ---: |
| Ordinary8 / stock ImageSharp | 43.817 /44.962 | 0.958566–0.983139 /pass | 83,008 /48,576 |
| Ordinary16 / stock ImageSharp | 44.603 /46.171 | 0.852923–1.023865 /uncertain | 83,008 /48,576 |
| Clipped Render8 / corrected ImageSharp | 37.325 /225.481 | 0.098756–0.289477 /pass | 44,136 /417,912 |
| Clipped Render16 / corrected ImageSharp | 34.925 /226.992 | 0.092013–0.265146 /pass | 44,136 /417,912 |

The frozen faces, text, options, canvases, exact full-depth managed frames and
approved visual gates are unchanged. Ordinary controls retain font/layout;
ImageSharp retains positioned paths while managed drawing prepares geometry per
call. Both reset and check every frame outside two clocks. Each ordinary capture keeps
all twelve 16,384-operation actuals, three reference groups and the conservative
confidence rule. CPU0, optimized JIT, natural nonconcurrent GC and idle-v2 pass.
Ordinary16's paired interval is below one, but shared phase variation broadens
the conservative marginal interval above one. It remains uncertain, not a proven
regression or optimization target. No filtering or favorable replay obtains a pass.

Clipped controls retain their explicitly source-corrected Fonts2.1.3/Drawing2.1.7
public adapters, nine layout calls/two tab rounds, twelve single-invocation actuals
per side/bracket and owned cleanup. Stock ordinary
references remain ImageSharp3.1.12/Drawing2.1.7/Fonts2.1.3. No FFmpeg/font-engine
equivalence claim follows. See [reference corrections](AUTHORED-FONT-COMPARISONS.md)
and [symmetric sampling](ORDINARY-DRAW-SYMMETRIC-CHECKS.md).
The authored managed BDN raw GC includes 736 B of scaffolding; the table reports
separate public-operation allocation diagnostics. Current raw counters and
reference allocation samples are reconciled from the retained final-stage logs.

Complete one-shot is **1130.556 /
6205.590 µs**, upper
**0.211854**; preparation plus eight draws is
**1309.267 /
6914.695 µs**, upper
**0.219622**. Every fitting trial, canvas,
independent output and disposal stays timed. These public-API integrations include
multiple layouts and the complete fitting search; stage timings are diagnostics.
Validation eagerness and retained preparation differ and are disclosed in the
[integration boundary](TEXT-COMPLETE-PUBLIC-PIPELINES.md).

Crossings grow16 ->32 bytes: ordinary temporary allocation rises **7,168 B**
(448 edges), to **83,008 B** plus24B adapter. Complete-fit admitted peak grows
**742,840 ->748,984 B** (384 edges); retained fit/Latin plans stay **4,160 /2,112 B**.
Reservation precedes allocation and lasts through actual users. No cache/pool/API
or retained workspace is added. GC allocations, admitted peak, retained ownership
and unmeasured physical/native/JIT peaks remain distinct.

Bit-oracle tests cover boundary/subnormal/overflow/reversed cases and40,000 random
geometries; existing full-frame, BigInteger composition, refusal/cancellation and
survivor checks pass. Both OS pass **6,824 full/live checks**, focused307internal/
177public normal/AVX-off/scalar, the SAME actual Release package and all3examples.
The actual timed Linux DLL **c5ae4f5d8bac2b2ef608f4bef9498b5b9379eced9181adf181ea727ea24f1960** is unchanged in
private package **539cbe19a9a9ed060cef9d10eb2b600d60c1e4ab9d897455224572c6b1840ff6**. Published0.2.0 is immutable.
Affected performance reconciles before broad qualification; no timed-DLL rebuild.
Pure layout/font loading/prepared blending/codecs/resizing/copies/flips retain
identified source-valid evidence. Stop passing controls; final full165-comparison
immutable-package refresh and the full289-path objective remain open.

[Evidence](text-scan-band-coordinates-evidence.json) pins every actual, identity,
admission observation and qualification. Mirrored/member-verified raw archive
`text-scan-band-coordinates-qualified.tar.xz`: **4,595,004 B /228 members**, SHA256
`52e461cda54471ba1f935bb7905d5cb8a593f8105a55488bdc8f1ab927fec598`. Restore the dependency archives identified there, then the new
archive under the benchmark root; its preparation, final-build/performance,
reconciliation and qualification scripts reproduce the bounded stage. Private
screen/long confirmation, final controlled gain, uncertain/adverse observations,
all guards, sources, binaries, packages and original frames remain preserved.
