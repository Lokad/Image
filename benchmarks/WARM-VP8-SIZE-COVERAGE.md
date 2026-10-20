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
| WarmWebp/Encode8 | 286,532 / 98,590 | 100 / 75 | Open |
| WarmWebp/Encode16 | 286,532 / 98,590 | 100 / 75 | Open |
| WarmWebp/EncodeBudget8 | 471,436 / 98,590 | 100 / 75 | Open |

C57 reuses the approved bounded quality matcher only for the changed photograph:
1,25,50,75,100 seeds and one refinement77, chosen by source-quality distance before
size assessment. The old78 point (18,284 B,0.179085dB gap) remains recorded;77 is
closer (0.140944dB) and produces17,366 B. Native75/effort4, input, packet and the
0.25dB tolerance remain frozen. Diagram75 stays valid without another search.

The five q0 receipts retain their original source-quality, alpha and decoder
qualification; their production branch is unchanged. Opaque8/16 have identical
normalized packets. Alpha8/16 do likewise; the work32M boundary disables the
optional coefficient model, producing a distinct larger packet. Those three alpha
operations emit132,867 raw alpha samples, while compressed `ALPH` is already a
supported decode algorithm. A bounded exact-alpha encoding primitive is therefore
a concrete next compression opportunity. Additional coefficient/default-q0 gaps
remain explicit; removing raw alpha alone does not prove their final parity.

[Evidence](vp8-size-coverage-evidence.json) identifies the current and retained
receipts. No new performance workload is introduced and the256-case warm checklist
is unchanged. CPU parity remains deferred; work/admission/cancellation and buffer
ownership stay mandatory. After these named lossy paths, the existing lossless
photograph/diagram/opaque controls remain next; no settings or shape grid follows.
