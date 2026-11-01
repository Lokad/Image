# Exact word transpose with AVX2

The existing RGBA16 transpose independently improves **32.57% elapsed**
(32.56% all-actual / **31.77% native-normalized**), with exact pixels and unchanged
**112 B/op**. Raw confidence separates in both views; normalized candidate upper
**0.748** is below original lower **1.081**. This fixed path passes the warm
single-CPU ≤1× objective and stops. The byte companion below is historical; its current passing result is in
[the byte transpose round](WARM-BYTE-TRANSPOSE-MATRIX.md).

| Existing complete 513×259 → 259×513 transpose | Lokad µs | Ratio / upper | Managed B/op | Status |
|---|---:|---:|---:|---|
| Original Whole16, independent12 | 48.831 | 1.085 / 1.090 | 112 | Gap |
| AVX2 Whole16, independent12 | 32.929 | 0.740 / 0.748 | 112 | Pass / stop |
| Whole8 companion, independent12 | 38.855 | 1.022 / 1.024 | 112 | Gap |

One safe AVX2 4×4 matrix transpose rearranges complete UInt64 pixels within the
original 32×32 tiles. Signedness, channels, alpha and hidden RGB are never
interpreted. Scalar tails/fallback, tile cancellation checkpoints and existing
admission/owners stay unchanged. There is no new buffer, cache or allocation.
Frozen input/output/native hashes and exact normal/scalar FFmpeg CLI pixels pass;
source and independently surviving outputs are unchanged. Every pixel is processed
inside complete public admission/materialization/output acquisition and release.
The native bracket is 1.17% faster between runs; normalized gain is stated separately.

One current original twelve resolves old B11 evidence after b62fe39 changed fuel
charging and overwrite ownership. One promising short earns one independent
candidate twelve. The existing byte control receives one current twelve for the
common entry; this is a cost refresh, not a paired byte speedup claim. No repeated
score, new profile/input or settings grid. Native allocation is unmeasured.

**5,352 checks per OS** pass: 2,170 internal + 2,234 public + 948 live. Focused
**201 public checks pass normal/scalar on each OS**, including lane/tile/tail
boundaries, independent whole-pixel placement, full word payloads/hidden RGB,
aggregate shared refusal, prompt cleanup, source independence and recovery.
The **same actual Windows Release package** passes both OSes and all three
examples, including a packaged 35×37 word transpose/shared-lifetime check.
One core/public-test build per OS; zero internal/differential/harness/native/selector builds.
The immutable measured DLLs serve the full qualification; published releases stay.

Quarter turns/mirrored transpose dispatch separately; normal-orientation and
stream composition controls use Normal orientation. Other warm codec/edit/text
paths retain applicable unchanged evidence. EXIF/AutoOrient correctness remains
qualified; file/subprocess timings are outside this comparison. No whole-library
method audit or claim about broader geometry/hardware. All **268 required material
paths** remain; this is not a full immutable-package release benchmark refresh.

[Numeric evidence](word-transpose-matrix-evidence.json) retains all actuals,
brackets/confidence, source/producer identities, scope, qualification and package.
Raw `word-transpose-matrix-qualified.tar.xz`, SHA256
405a7e381b3bcea24425d6e2f5ad846f0bdd0fa569a9123f018e478fed6f836a (3,227,356 B / 156 members),
is mirrored and member-verified, including original/current DLLs, actual package,
source/tests/consumer, cached selectors/worker and original recipes. Dependencies
retain original exact-edit controls and unchanged earlier qualification.
