# Compact JPEG baseline output study

**Not accepted; qualified production, embedded tests and the actual package are
restored byte-exact.** Two compact destinations retain the original coefficient
representation and remove the earlier RGB420/dense regressions. They improve the
horizontal control, but create a material sparse-2D regression.

| Complete managed decode, controlled immutable-DLL pairs | Original → D µs | Original → E µs | Mean-pair ratio interval, D / E |
| --- | ---: | ---: | --- |
| Horizontal CMYK | 406.26 → 380.77 | 430.87 → 411.73 | .915–.960 / .952–.959 |
| Dense CMYK | 1290.32 → 1280.99 | 1276.15 → 1281.48 | .970–1.017 / .990–1.018 |
| One frozen mixed-density generalization | 834.34 → 842.54 | 829.70 → 826.06 | .997–1.023 / .987–1.005 |

Horizontal mean-pair gains are 6.24%/4.44%. Dense and mixed changes are inconclusive;
no broader gain is claimed. Paired increments are separate from standalone native
comparisons. Neither stage is accepted merely for a useful scoped gain.

| Sparse-2D, original reference boundary | Managed / FFmpeg µs | Conservative ratio interval | Outcome |
| --- | ---: | ---: | --- |
| Unchanged qualified production | 449.95 / 461.96 | .948–1.001 | Uncertain |
| D, compact state with emission-based selection | 484.94 / 440.30 | 1.021–1.149 | Gap |
| E, horizontal-support selection | 489.68 / 469.74 | 1.033–1.051 | Gap |

E is 8.83% slower raw and 7.03% slower reference-normalized than the contemporary
original. Filtered/all-actual raw and normalized intervals separate; native moves
+1.68%. Its short screen had looked better, so the independent confirmation is
decisive. No favorable unchanged retry follows. D's adversity is also retained.
Current accepted sparse-2D evidence is now uncertain, replacing its older pass;
this is a confidence update, not a production regression or an assumed gap.

D/E's independent RGB420 costs are 665.25/1023.78 and 660.35/1013.06 µs, upper
.682/.659; dense costs1323.21/3114.65 and1267.36/3113.00 µs, upper.439/.409.
The original fast coefficient/vector paths remain viable. E's horizontal native
comparison still has a gap465.68/418.98 µs, upper1.121; its equivalent fresh-context
view passes410.43/819.38 µs, upper.505. All costs and earlier successful D controls
remain in the evidence. Clock differences do not establish a causal speedup.

Both use one16-byte span descriptor and the common baseline grammar. Bounded
operation-stack state holds64sample words,64quantizers and support; D also counts
emissions (266B), while E removes that counter (264B). E retires fusion for a scan
component after actual vertical-frequency support appears. This preserves exact
DC saturation, modular AC and inverse arithmetic, with no fixture identifiers,
new heap owner, admission capacity, retained scratch, pool or public API.
Main allocation stays20,944B/op. Physical/native/JIT-stack peaks are unmeasured.

Each candidate passes474internal/342public JPEG-focused checks per normal, scalar,
AVX512-off and AVX2-off mode on Windows/Linux. Exact dequantized samples/support,
mixed component density, restart changes, default-generator packet identity,
cancellation/refusal cleanup and independent surviving outputs pass. Original
input/reference/settings, complete public decode/normalization/output/disposal,
normal/scalar native bounds and idle-v2 CPU0 guards remain unchanged.

One main/embedded build per OS per candidate serves all focused work. Existing
harnesses, references and fixtures are reused; one authored mixed tool/fixture
serves D and E. Broad and package qualification stop before acceptance. Exact
restoration reuses6600full/live checks per OS, scalar/ISA and the SAME actual
package/all3examples, without rebuilding or repeating qualification.

Any later redesign needs attributable removal of the sparse companion overhead;
this result rejects these implementations, not every entropy/output design.
Continue independent justified work. Exact encoder packets, managed pixels, all
289material paths and publication postponement remain. The requested PLAN change
stays limited to its ignored font/text section.

[Evidence](jpeg-compact-output-study-evidence.json) preserves every original result,
source/module identity and restoration. Mirrored/member-verified raw
`jpeg-compact-output-study.tar.xz`: 7,454,160B/346members,
SHA256`7b42ec38fe65e7380dffe4e15929a6c446632d440ef9ae79337cdc3ad35f22fa`. Its dependency retains the
[previous transition](JPEG-BASELINE-OUTPUT-TRANSITION.md), original tools and package.
