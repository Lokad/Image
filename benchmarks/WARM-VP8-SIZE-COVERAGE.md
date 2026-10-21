# Lossy WebP complete-packet coverage

Six of seven already declared encode controls pass complete-packet size and
quality: the two primary content controls and four unrestricted procedural controls.
**Only the32M-work control remains open**, by1,954 B. Fixed managed100/native75
settings below are benchmark inputs, not the library default85 or evidence of
matching by numerical equality. No general lossy WebP parity follows.

| Existing control | Managed / reference packet B | Managed / native quality | Complete-packet gate |
| --- | ---: | ---: | --- |
| WarmCompression/PhotographLossy8 | 17,366 / 17,688 | 77 / 75 | Pass |
| WarmCompression/DiagramLossy8 | 6,722 / 7,100 | 75 / 75 | Pass |
| WarmWebp/EncodeOpaque8 | 70,298 / 72,380 | 100 / 75 | Pass |
| WarmWebp/EncodeOpaque16 | 70,298 / 72,380 | 100 / 75 | Pass |
| WarmWebp/Encode8 | 96,016 / 98,590 | 100 / 75 | Pass |
| WarmWebp/Encode16 | 96,016 / 98,590 | 100 / 75 | Pass |
| WarmWebp/EncodeBudget8 | 100,544 / 98,590 | 100 / 75 | Open |

The current [zero/one round](WARM-VP8-ZERO-ONE.md) reduces the constrained packet
100,980→100,544 B with a0.041936dB source-quality gap within0.25dB, exact alpha
and31,696,404 units inside32M. BothOS4,733tests/scalar and the same actual package/
examples qualify. One short cost replaces that path; six other C83 costs remain.

The [DC/AC-noise round](WARM-VP8-NOISE-SPECTRUM.md) saves856 B per opaque
control,692 B per unrestricted alpha control and498 B under32M. Source-quality
gaps are0.005052/0.021958/0.040560dB within0.25dB; exact alpha and decoder bounds
pass. Primary packets/pixels/quality stay exact.31,632,851 actual units fit32M.
BothOS4,723tests/scalar and the same actual package/examples qualify. Seven short
cost observations supersede affected analysis paths; CPU parity stays deferred.
Earlier milestones below retain their historical scope and raw evidence.

C57 reuses the approved bounded quality matcher only for the changed photograph:
1,25,50,75,100 seeds and one refinement77, chosen by source-quality distance before
size assessment. The old78 point (18,284 B,0.179085dB gap) remains recorded;77 is
closer (0.140944dB) and produces17,366 B. Native75/effort4, input, packet and the
0.25dB tolerance remain frozen. Diagram75 stays valid without another search.

The [backward-reference milestone](WARM-VP8-ALPHA-MATCHES.md) reduces the two
unrestricted alpha packets268,028→170,196 B with exact old RGBA/source quality.
ALPH alone is18,579/24,588 B; complete-packet parity remains open. Full-frame q0
entropy also reduces opaque8/16 to149,570 B. Four current cost screens,4,628tests/OS
and the same actual package/examples qualify. References, inputs, numerical settings
and all source-quality/alpha/decoder tolerances are unchanged; no quality search.

The [limited-work prefix milestone](WARM-VP8-PREFIX-WORK.md) cuts the32M-work
packet452,932→268,028 B with exact old decoded RGBA/source quality and31,818,464
charged units inside32M. Prefix statistics now have separate eligibility; actual
variable work remains capped and exhaustion/refusal stays terminal.4,635tests/OS,
the same actual package/examples and one short cost screen qualify that row.
No setting, source-quality gate or reference changes; other four procedural
packet/work/quality records remain exact. Five whole-packet gates stay open.

The unrestricted gap is coefficient coding; its ALPH already beats the reference
component. The [single-pass alpha milestone](WARM-WEBP-ALPHA-SINGLE-PASS.md) further cuts
the work32M packet268,028→208,216 B and ALPH114,363→54,552 B against24,588 B.
Exact prior RGBA/source quality and31,997,511 units fit32M, with shared staging/
failure/ownership tests and the same actual package qualified on both OSes.
The [token-replay milestone](WARM-WEBP-ALPHA-TOKENS.md) further reduces
that packet208,216→181,998 B and ALPH54,552→28,333 B. Exact prior RGBA/source
quality,31,980,594 units inside32M, same-root overlap/cleanup and the same actual
package qualify on both OSes. The shared coefficient gap remains the larger
opportunity; constrained ALPH is still above24,588 B. Passing primary
content or a component does not establish whole-codec parity.

The [source-quantization milestone](WARM-VP8-SOURCE-QUANTIZATION.md)
reduces opaque149,570→83,838 B, alpha170,196→108,880 B and work32M181,998→120,146 B.
Existing source-quality gaps are0.002978/0.029329 dB within0.25 dB; alpha is exact.
Actual work31,752,723 fits32M. Primary packets/pixels remain exact. BothOS suites,
scalar checks and the same actual package/examples qualify; seven short jobs
replace all affected encode cost observations. All five packet gates remain open.

The [interpolation-noise milestone](WARM-VP8-INTERPOLATION-NOISE.md) further
reduces opaque83,838→74,962 B, alpha108,880→99,816 B and work32M120,146→111,094 B.
Unchanged quality gates pass (0.016224/0.038933 dB gaps), exact alpha is preserved
and31,916,945 units fit32M. BothOS4,674tests, scalar and same actual package/examples
qualify; seven short costs supersede affected encodes. All five packet gates stay
open, including12,504 B on the constrained path; no whole-codec parity claim.

The [alpha-anchor milestone](WARM-WEBP-ALPHA-ANCHORS.md) reduces the
constrained packet111,094→104,500 B, exact prior VP8/RGBA/source quality and
31,970,409 units within32M. ALPH21,740/ref24,588 B passes its component gate;
the complete packet remains5,910 B above98,590 B. BothOS4,682tests/scalar and
the same actual package/examples qualify. One short cost replaces only this row;
four unrestricted records and opaque primary paths reuse unchanged C70 evidence.

The [segment-search milestone](WARM-VP8-SEGMENT-SEARCH.md) reduces opaque
74,962→73,740 B and unrestricted alpha99,816→98,760 B. Remaining gaps1,360/170 B
stay explicit; source-quality gaps0.008682/0.030410 dB pass with exact unchanged
ALPH. BothOS4,688tests/scalar and same actual package/examples qualify; four short
costs supersede affected encodes despite higher CPU. Work32M104,500 B/31,970,409
units and primary paths retain their exact prior scoped proofs. Five gates stay open.

The [four-residual-key milestone](WARM-WEBP-ALPHA-WORD-KEY.md) reduces
the constrained packet104,500→102,006 B, with exact prior VP8/RGBA/source quality
and31,947,691 units within32M. ALPH19,246/ref24,588 B is a component pass; the
complete packet remains3,416 B above98,590 B. BothOS4,692tests/scalar and the same
actual package/examples qualify. One short cost supersedes this row; four
unrestricted full records and primary paths retain C73/C70 scoped evidence.

The [selected-coefficient milestone](WARM-VP8-COEFFICIENT-REFINEMENT.md)
passes all four unrestricted packet gates:opaque71,154/ref72,380 B and
alpha96,708/ref98,590 B. Source-quality gaps0.003192/0.022172 dB pass with exact
unchanged ALPH. BothOS4,699tests/scalar and the same actual package/examples
qualify. Four short cost screens record higher CPU, accepted for compression.
The constrained full record is exactly C74,102,006 B/31,947,691units; primary
cases stay passing/stopped. Six pass, one remains open; no broader parity claim.

The current [pruned-prefix milestone](WARM-WEBP-PRUNED-PREFIX.md) reduces the
constrained packet102,006→101,478 B with exact prior source/decodedRGBA/alpha and
31,343,084 units inside32M. Exact prediction pruning enables one128-block entropy
sample; local/shared admission and every performed work charge remain enforced.
BothOS4,709tests/scalar and the same actual package/examples qualify. One affected
short cost replaces that row; six other cost/quality scopes are retained. The
complete98,590 B ceiling remains open by2,888 B; six controls pass, one is open.

[Evidence](vp8-size-coverage-evidence.json) identifies the current and retained
receipts. No new performance workload is introduced and the256-case warm checklist
is unchanged. CPU parity remains deferred; work/admission/cancellation and buffer
ownership stay mandatory. After these named lossy paths, the existing lossless
photograph/diagram/opaque controls remain next; no settings or shape grid follows.
