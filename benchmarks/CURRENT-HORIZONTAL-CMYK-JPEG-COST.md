# Current horizontal-only CMYK JPEG decode cost

The existing single-horizontal-AC control independently confirms a gap:
**591.095 µs, 1.457× / conservative upper 1.521**, **1,085,757 B/op**.
This resolves the old fixed-inverse/entropy uncertainty; no causal gain or
regression is inferred from its historical 1.714× screen.

The original 513×259 Adobe0 four-component packet is 17,346 bytes, with unit
quantizers and one horizontal AC=4 per block. It exercises a distinct required
inverse path. Both subjects include complete resident entropy/inverse decoding,
all four planes, device-color normalization, fresh zeroed RGBA ownership,
packing and release. ICC/reorientation remain off; native allocation is unmeasured.

An independent integer Q14/Q16 oracle from the original authored block means and
spectrum matches every managed normal/scalar byte on both OSes and every pinned
FFmpeg normal/scalar byte exactly. The old derived reference differs by at most
one RGB sample/RMSE 0.709290. Derived RGBA/canonical-CMYK samples and metadata are
temporarily corrected; their original files are retained and restored byte-exact
after scoring. Packet, raster, projection, means, quantizers, native executable
and settings remain unchanged. No pixel/native tolerance is relaxed.

One successful independent twelve retains every actual, native bracket and
passing idle-v2 guard. Frozen production, harness, generic selector and worker
are reused with **zero builds**, profiles or repeated timings. Source preservation
and independent output ownership pass before and after scoring. Applicable
5,289 checks per OS and the same actual Release-package/three-example proof
carry forward without repetition.

[Numeric evidence](current-horizontal-cmyk-jpeg-cost-evidence.json) pins samples,
producers, independent proofs and restoration. Raw
current-horizontal-cmyk-jpeg-confirmation.tar.xz, SHA256
3fd858a5fd8d0a94a015c585945011cdbbdba47cd6c1bc26a2843247b0eb89af,
is mirrored with all 57 members verified; prior qualified archives are pinned
dependencies. The next bounded review considers eliminating separate component
storage with an explicit owned output, preserving validation before publication
and admission/cleanup. The dense/general passing case is not retuned. All 268
material paths remain required; this adds one already-required displayed case.
