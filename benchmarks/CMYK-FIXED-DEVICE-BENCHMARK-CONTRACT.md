# CMYK benchmark pixel contract

**Resolved:** [current fixed-policy cost](CURRENT-CMYK-FIXED-INVERSE-COST.md)
confirms2.850× / upper2.858 with exact pixels. The original attempt stopped in setup,
before any scored actual, because the fixture expects the superseded nearest-
integer device-CMYK product. That attempt produced no performance result; its failure is retained.

On the unchanged qualified DLL, managed normal and scalar output both exactly
match the original frozen native pixels. An independent oracle derived from the
original canonical CMYK samples and the [declared Q16 device policy](../docs/jpeg-inverse-rounding.md)
produces those same bytes: `((255-ink)*(255-black)*floor(65536/255)) >> 16`.
The old expected RGBA differs at 200,427 samples, all by one. The original
22,708-byte packet, component/canonical data and native output remain unchanged.
This is a benchmark expectation mismatch, without evidence of a decoder defect
on this control or a broader decoder-equality claim.

Fixture generation now also accounts for one canonical sample at the pinned
integer inverse rounding boundary, derived independently from original block
means and the T.81/Q14 policy. Neither oracle uses production decoded pixels.
Original image inputs/native bytes stay unchanged; only expected output data
and hashes change, with originals archived and proof overlays restored first.

The correction preserves exact managed
pixels, frozen inputs/settings, native bounds, source/ownership and quiet gates.
Derive expected pixels from independent canonical data, never from production
RGBA output. Preserve the original fixture/evidence and restore any temporary
proof overlay byte-exact. One independent twelve-actual confirmation is retained. The rejected
axis-cross family remains closed; no CPU optimization is inferred from stale cost.

Zero production/harness/native builds or repeated package qualification; one small
selector and the existing ImageProbe served the diagnosis. Current 5,163-check
Windows/Linux and same-package qualification remains applicable. The pixel probe
itself covers Linux normal/scalar execution. Full 268-path parity stays open.

[Evidence](cmyk-fixed-device-benchmark-contract-evidence.json).
Mirrored/member-verified cmyk-fixed-device-contract-diagnosis.tar.xz, 66 members,
SHA256 83d3a9ac5e8cdd0c9f956defacfc0482522d8c8e133cd829eb882e056acfdcf0; retained failed setup, diagnostic outputs and dependencies
are pinned by hash.
