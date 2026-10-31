# Wider bicubic tap-view transfer: rejected

One Wide8 short screen misses the gain gate: candidate **539.453 µs** versus fresh
original **523.424 µs** (3.06% higher), with unchanged **231,239 B/op**.
This short difference establishes no causal regression. There is no candidate
independent confirmation, variant or new profile. Current accepted Wide8 remains
529.180 µs, 1.518× / upper 1.534 from its retained independent confirmation.

The trial transfers the narrower cache's borrowed vector view to the wider
paired horizontal loop. Offset=4*i reads identical32bytes; tap order, padding,
arithmetic, width, guards, repair, odd tail, cancellation and admission stay fixed.
Exact managed rational/frozen pixels, unchanged native hashes and max1/RMSE≤.25,
source/output ownership and both quiet-v2 guards pass. A win in the narrower
kernel does not establish a wider-kernel win.

One core build per OS and 217 existing public controls per normal/scalar mode
pass on both OSes, also Linux without AVX512. Zero test/harness/native/selector
builds, new benchmark inputs or profiles. Qualified source, core DLL/PDB/XML and
public core copies are restored byte-exactly; test assemblies/package remain
unchanged. Current 5,291 checks per OS and same actual Release-package/three-
example proof apply without repeated full/package qualification.

[Evidence](bicubic-wide-tap-view-rejected-evidence.json) retains both three-actual
screens, confidence, guards, source identities and restoration. Mirrored,
member-verified raw bicubic-wide-tap-view-rejected.tar.xz SHA256
1fa4c8f0ef7d272322f0ddf8cde62bb12cfd77d6a73894abc95491769397b7e7
(978,216 bytes, 84 members) preserves the candidate, exact original, immutable
cores, reused selectors and fixture controls. Dependencies retain the qualified
[current package](WARM-JPEG-COMPLETED-ROW-REUSE.md) and
[original Wide8 cost](WARM-EXACT-ALPHA-INT128.md). This transfer is closed without
width/guard/inlining variants; all 268 required paths remain open as applicable.
