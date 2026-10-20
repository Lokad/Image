# Lossy WebP complete-packet coverage

Seven already declared encode controls have separate compression gates. The two
primary content controls pass at their current matched settings; **five fixed-q100
procedural controls remain open**. These numerical quality settings are explicit
benchmark inputs, not the library default85. No general lossy WebP parity follows.

| Existing control | Managed / reference packet B | Managed / native quality | Complete-packet gate |
| --- | ---: | ---: | --- |
| WarmCompression/PhotographLossy8 | 17,366 / 17,688 | 77 / 75 | Pass |
| WarmCompression/DiagramLossy8 | 6,722 / 7,100 | 75 / 75 | Pass |
| WarmWebp/EncodeOpaque8 | 83,838 / 72,380 | 100 / 75 | Open |
| WarmWebp/EncodeOpaque16 | 83,838 / 72,380 | 100 / 75 | Open |
| WarmWebp/Encode8 | 108,880 / 98,590 | 100 / 75 | Open |
| WarmWebp/Encode16 | 108,880 / 98,590 | 100 / 75 | Open |
| WarmWebp/EncodeBudget8 | 120,146 / 98,590 | 100 / 75 | Open |

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

The current [source-quantization milestone](WARM-VP8-SOURCE-QUANTIZATION.md)
reduces opaque149,570→83,838 B, alpha170,196→108,880 B and work32M181,998→120,146 B.
Existing source-quality gaps are0.002978/0.029329 dB within0.25 dB; alpha is exact.
Actual work31,752,723 fits32M. Primary packets/pixels remain exact. BothOS suites,
scalar checks and the same actual package/examples qualify; seven short jobs
replace all affected encode cost observations. All five packet gates remain open.

[Evidence](vp8-size-coverage-evidence.json) identifies the current and retained
receipts. No new performance workload is introduced and the256-case warm checklist
is unchanged. CPU parity remains deferred; work/admission/cancellation and buffer
ownership stay mandatory. After these named lossy paths, the existing lossless
photograph/diagram/opaque controls remain next; no settings or shape grid follows.
