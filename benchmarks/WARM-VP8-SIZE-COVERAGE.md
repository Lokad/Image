# Lossy WebP complete-packet coverage

Seven already declared encode controls have separate compression gates. The two
primary content controls pass at their current matched settings; **five fixed-q100
procedural controls remain open**. These numerical quality settings are explicit
benchmark inputs, not the library default85. No general lossy WebP parity follows.

| Existing control | Managed / reference packet B | Managed / native quality | Complete-packet gate |
| --- | ---: | ---: | --- |
| WarmCompression/PhotographLossy8 | 17,366 / 17,688 | 77 / 75 | Pass |
| WarmCompression/DiagramLossy8 | 6,722 / 7,100 | 75 / 75 | Pass |
| WarmWebp/EncodeOpaque8 | 151,606 / 72,380 | 100 / 75 | Open |
| WarmWebp/EncodeOpaque16 | 151,606 / 72,380 | 100 / 75 | Open |
| WarmWebp/Encode8 | 268,028 / 98,590 | 100 / 75 | Open |
| WarmWebp/Encode16 | 268,028 / 98,590 | 100 / 75 | Open |
| WarmWebp/EncodeBudget8 | 452,932 / 98,590 | 100 / 75 | Open |

C57 reuses the approved bounded quality matcher only for the changed photograph:
1,25,50,75,100 seeds and one refinement77, chosen by source-quality distance before
size assessment. The old78 point (18,284 B,0.179085dB gap) remains recorded;77 is
closer (0.140944dB) and produces17,366 B. Native75/effort4, input, packet and the
0.25dB tolerance remain frozen. Diagram75 stays valid without another search.

The [compressed-alpha milestone](WARM-VP8-COMPRESSED-ALPHA.md) saves18,504 B on
each alpha8/16/work32M packet. ALPH payload falls132,868→114,363 B, including its
header. VP8 payloads, reconstructed RGBA and source quality are unchanged; no
quality search or reference change occurs. Those three paths are freshly
qualified with4,613tests/OS and the same actual package/examples. Their three
current costs are short screens; CPU parity and independent repeats are deferred.
Opaque8/16 retain their valid original receipts and identical normalized packets.

All five procedural gates remain open. The work32M case completes in31,961,577
units, still declining the optional coefficient model and producing a distinct
larger packet. Its coefficient-model admission and the114,363 B alpha stream are
named remaining opportunities. Raw-alpha removal alone does not establish size
parity, and the two primary opaque passes do not establish whole-codec parity.

[Evidence](vp8-size-coverage-evidence.json) identifies the current and retained
receipts. No new performance workload is introduced and the256-case warm checklist
is unchanged. CPU parity remains deferred; work/admission/cancellation and buffer
ownership stay mandatory. After these named lossy paths, the existing lossless
photograph/diagram/opaque controls remain next; no settings or shape grid follows.
