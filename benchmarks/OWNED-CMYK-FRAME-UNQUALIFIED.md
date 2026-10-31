# Owned CMYK frame experiment

The bounded candidate is **unaccepted**: it removes **549,486 allocated B/op
(50.61%)**, but fails independent CPU confirmation against the reference.
The qualified source, core/test assemblies and package are restored exactly.
The substantial memory result and candidate remain archived.

The existing horizontal-only CMYK control measures 591.095 → 573.795 µs:
**2.93% less filtered / 2.74% less all-actual raw elapsed**, with separate raw
confidence. Reference-normalized point cost changes **1.457× → 1.463×**;
conservative confidence overlaps. Allocation is 1,085,757 → 536,271 B/op.
These establish a candidate memory reduction, not a certified CPU improvement.
Current production keeps its original 1.457× / upper 1.521 and 1,085,757 B/op.

The short screen shows 4.43% raw / 1.44% normalized point gain, missing the
planned 2% normalized gate. Its false decision is retained. One explicit bounded
triage exception permits independent confirmation because of the raw gain and
large allocation saving; final ≥2% raw/all/normalized and separate-confidence
requirements remain unchanged. The candidate fails those final requirements.
No second confirmation, profile or variant follows. The original twelve is
reused after a stable fresh three (+1.51% raw, native −0.10%). Every actual,
native bracket and passing idle-v2 guard remains retained.

The candidate replaces four padded component planes with the privately owned
output frame, using 512 admitted stack bytes for MCU packing. Raw samples stay
private until EOI/component validation and final Adobe interpretation; in-place
conversion retains exact Q16 CMYK/shared YCCK precision. The result owner exists
before allocation and survives until disposal; failure/cancellation disposes it.
Caller samples, progressive/subsampled inputs and validation retain their model.
No packet, raster, projection, quantizer, reference setting or tolerance changes.

Actual frozen/native pixels and ownership pass. Twenty candidate-only public
cases cover component order, split/partial scans, clipped equal-factor MCUs,
restarts, nonzero AC, late Adobe, retained outputs, terminal refusal, cancellation,
malformed tails and reuse. They join 347 internal/366 public focused checks per
normal/scalar mode on both OSes, also Linux without AVX512. A cold placement
model supplies 222 checks on two existing frames and small original-sample views;
it is not a decoder admission or timing proof. One core/internal build per OS;
Windows needs a second public-test build to correct test assumptions, without
rebuilding production. Cached harness/selectors/workers are reused.

Candidate-specific code/tests are removed with the restored implementation.
Existing full 5,289 checks per OS and the same actual Release-package consumer
evidence apply without repetition; candidate full/package qualification was not
run. [Numeric evidence](owned-cmyk-frame-unqualified-evidence.json) pins both
decisions, all samples, actual builds, focused checks and restoration records.
Raw owned-cmyk-frame-unqualified.tar.xz, SHA256
2fb5a805f610a901979bc1d79f9f1f198f3bfc3fad1b04c19e0fec493f92e3bb,
is mirrored with all 122 members verified. Original/candidate source, test
assemblies, proofs, runtime/selectors, receipts and guards are retained; prior
qualified archives are pinned dependencies. All 268 material paths remain open
where passing evidence is absent. The explicitly retained approximately 4.1%
color-histogram improvement remains in production.
