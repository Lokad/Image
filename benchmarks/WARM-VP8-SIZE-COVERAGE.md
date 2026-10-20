# Lossy WebP complete-packet coverage

Seven already declared encode controls have separate compression gates. The two
primary content controls pass at their current matched settings; **five fixed-q100
procedural controls remain open**. These numerical quality settings are explicit
benchmark inputs, not the library default85. No general lossy WebP parity follows.

| Existing control | Managed / reference packet B | Managed / native quality | Complete-packet gate |
| --- | ---: | ---: | --- |
| WarmCompression/PhotographLossy8 | 17,366 / 17,688 | 77 / 75 | Pass |
| WarmCompression/DiagramLossy8 | 6,722 / 7,100 | 75 / 75 | Pass |
| WarmWebp/EncodeOpaque8 | 149,570 / 72,380 | 100 / 75 | Open |
| WarmWebp/EncodeOpaque16 | 149,570 / 72,380 | 100 / 75 | Open |
| WarmWebp/Encode8 | 170,196 / 98,590 | 100 / 75 | Open |
| WarmWebp/Encode16 | 170,196 / 98,590 | 100 / 75 | Open |
| WarmWebp/EncodeBudget8 | 452,932 / 98,590 | 100 / 75 | Open |

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

All five procedural gates remain open. The work32M case remains byte/work-identical
at452,932 B and31,961,577 units, keeping its existing fixed model and fixed ALPH
policy. Its C58 cost proof is reused. The remaining unrestricted gap is coefficient
coding; alpha on that control already beats its reference component. Primary
opaque passes and component passes do not establish whole-codec parity.

[Evidence](vp8-size-coverage-evidence.json) identifies the current and retained
receipts. No new performance workload is introduced and the256-case warm checklist
is unchanged. CPU parity remains deferred; work/admission/cancellation and buffer
ownership stay mandatory. After these named lossy paths, the existing lossless
photograph/diagram/opaque controls remain next; no settings or shape grid follows.
