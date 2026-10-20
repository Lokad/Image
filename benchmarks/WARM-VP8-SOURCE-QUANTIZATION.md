# Source-derived lossy WebP quantization

Source-only spatial quantization cuts the fixed opaque packets **149,570 → 83,838 B**
(43.9%), alpha packets **170,196 → 108,880 B** (36.0%) and work32M packet
**181,998 → 120,146 B** (34.0%). Five complete-packet gates remain open. These
procedural controls are distinct from the passing photograph/diagram controls;
there is no general compression-parity claim.

| Existing control | Previous → current / reference B | Source-PSNR gap dB | Managed / reference µs | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 149,570 → 83,838 / 72,380 | 0.002978 | 15,751.55 / 15,552.09 | 1,002,712 |
| EncodeOpaque16 | 149,570 → 83,838 / 72,380 | 0.002978 | 16,364.53 / 15,620.67 | 1,002,712 |
| Encode8 | 170,196 → 108,880 / 98,590 | 0.029329 | 22,841.16 / 53,441.37 | 1,532,200 |
| Encode16 | 170,196 → 108,880 / 98,590 | 0.029329 | 23,169.74 / 53,625.17 | 1,532,200 |
| EncodeBudget8 | 181,998 → 120,146 / 98,590 | 0.029329 | 15,743.21 / 53,763.89 | 2,033,809 |
| PhotographLossy8 | 17,366 → 17,366 / 17,688 | 0.140944 | 33,969.44 / 9,646.86 | 590,152 |
| DiagramLossy8 | 6,722 → 6,722 / 7,100 | 0.117409 | 14,068.64 / 5,815.18 | 570,856 |

All inputs, references, settings, exact-alpha and existing source-quality/decoder
gates remain fixed. Procedural controls use managed100/native75, effort4; primary
photo77/diagram75 retain identical packets, decoded pixels and quality records.
The unchanged base mapping and effective quantization policy are documented in
SPEC.md; equal numeric quality does not establish matching.

One sample per4x4 block estimates unavoidable RGB loss from source4:2:0 conversion.
The transform's expected quantization variance and conversion-matrix gain derive
a bounded additional precision allowance, independently of reference settings or
benchmark tolerance. Four source-only clusters select segment quantizers no finer
than the unchanged base index. This is an encoder heuristic, not an error bound
or global quality-monotonicity promise. Lossy reconstructed RGB can change.

The borrowed root reserves the map/owner before allocation. Bounded stack scratch,
actual analysis/header work, old/new arithmetic capacities, terminal refusal,
cancellation and independent outputs remain qualified; no new full-frame copy.
The constrained operation consumes31,752,723 units within32M. Alpha coding is
unchanged: unrestricted ALPH18,579/ref24,588 B, constrained28,333/ref24,588 B.

Both OSes pass4,666 tests and194internal+8public+2differential scalar checks.
Normal/scalar packets and managed pixels agree across OSes. The same actual
private0.2.0 package and all three examples pass bothOS. One production build/OS
is reused throughout; only public tests needed a second build to reconcile the
new charged map and intentionally changed lossy output. Frozen decoder packets
and exact peak/peak-minus-one safeguards remain intact. Initial failures remain
in the raw evidence.

Seven sequential, guarded three-actual jobs record costs for the seven existing
changed encode paths. No independent timing repeat or CPU-parity acceptance is
claimed; native allocations are unmeasured. Unchanged codec/edit/text/inspection
evidence retains its scope. [Structured evidence](vp8-source-quantization-evidence.json)
retains both immutable archives, every quality/ownership receipt and cost sample.
