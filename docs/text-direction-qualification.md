# Staged direction and joining

This records the internal direction/joining checkpoint before public integration.
The current [OpenType layout mode](text-shaping-qualification.md) uses these
primitives; fitting and replacement qualification remain open.

The independently implemented paragraph resolver follows
[Unicode 17 UAX #9 revision 51](https://www.unicode.org/reports/tr9/tr9-51.html):
paragraph direction, explicit embeddings/overrides, isolates, isolating run
sequences, weak/neutral resolution, paired brackets, implicit levels and per-line
L1/L2 reordering. Removed controls retain source positions. Line requests borrow
output storage and preserve the paragraph. Glyph shaping, cluster policy and
glyph mirroring belong to the consuming layout pipeline.

Joining masks follow the [normative joining rules](https://www.unicode.org/versions/Unicode17.0.0/core-spec/chapter-9/)
within logical runs: isolated/initial/medial/final forms, transparent marks,
join-causing characters, ZWJ/ZWNJ and level/run boundaries. This is not a claim
of complete Arabic shaping. Compact property constants independently encode
normative Unicode 17 facts. The reproducible generator verifies ignored
specification inputs by SHA-256; no upstream implementation or test corpus is
used, and external files are never production build inputs.

Owned paragraph storage and temporary resolution arrays reserve capacity before
allocation through the same operation context. Independent local/work limits
remain effective. Refusal is terminal; failures and cancellation refund owned
storage. Overlapping paragraphs share the caller's root until disposal.

Qualification passed **4,366 tests per OS** on Windows/Linux: 1,557 internal,
1,881 public API and 928 live FFmpeg comparisons. The **50 new checks per OS**
also passed with intrinsics disabled. They cover numeric/mark/bracket resolution,
isolates/overrides/depth overflow, independent line resets, joining forms, both
storage-admission failures, aggregate ownership, cancellation/work cleanup and
successful subsequent operations. The same actual private 0.2.0 Release package
passed isolated consumers and all three usage examples on both systems.

An additional correctness audit compared 1,167 original inputs and 3,462 line
orders with ICU 74 on the VM, excluding invisible formatting controls from visible
order. **3,456 agree**. Six differ on two authored edge cases: BD16 requires
discarding all bracket pairs on stack overflow, and L1 uses original isolate
types even under directional overrides. Focused tests enforce the pinned
normative rules; the unmodified native differences are retained. This bounded
audit is not a Unicode conformance claim or public text-compatibility proof.
No performance timing was measured.

Retained SHA-256 identities:

- Qualified source: `b00312ac8725c9f0d4757a3826c18ce6b957ce4aa5df5576db5e41114a4a3d53`.
- Windows producer: `e923b5f0b0c8ce6a54bf72af28890453e665a56364a7ebc4cae7e03397b4e9d6`.
- Linux producer: `70a750b66b78b116f085424b0708a7814b8587979efa727ddc2efd28a22a6b51`.
- Actual package: `b123bf7b8919ad5decc66c20a1ae0ff05afe79106c338a20e68481a55959b21c`.
- `p2d-qualified-windows.tar.gz`: `0f86668659782a79dc2f18181e3c90148181613793aefedb1fa8ca44f4d76880`.
- `p2d-qualified-linux.tar.gz`: `bdb01006379ba1eccce1d0ddb494015d23cff72ceea3126d731cf11355519df0`.

Published 0.1.0 and earlier evidence remain preserved. Current OpenType comparison
results are recorded in [text compatibility](text-compatibility.md).
