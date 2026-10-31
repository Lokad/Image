# Cross-block JPEG lookahead: rejected

One sparse-CMYK short is **640.481 µs**, versus the reused unchanged original
**626.368 µs**: means are 2.25% higher raw and 3.68% higher native-normalized.
Confidence overlaps; no causal regression or gain is claimed. Allocation stays
unchanged within noise: 1,085,762 → 1,085,758 B/op. The retention gate fails;
no independent candidate confirmation or variant. Current accepted sparse-CMYK
remains 638.035 µs, 1.442× / upper 1.481, 1,085,758 B/op.

The candidate retains known ordinary lookahead between complete baseline blocks
in the existing UInt32 cache. Partial/miss/generic transitions return unused
bytes. One Boolean distinguishes baseline lookahead from already consumed bytes
on other failures; no new buffer, table, owner or admission model. A cold syntax
model predicts 38.4% fewer four-byte read/check attempts on the original packet;
that structural observation does not establish an elapsed-time gain.

An initial build passes 354/355 internal focused controls on each OS: an existing
progressive EOB failure exposes an error-position mismatch (3 versus 4). The
baseline-only state correction preserves that failure contract before timing.
Corrected 355 internal/354 public focused controls pass normal/scalar on both
OSes and Linux without AVX512. Exact managed/native/scalar frames, source/output
ownership and both quiet-v2 guards pass. Original worker, selectors, fixtures,
benchmark assembly and baseline measurements are reused; no new input/profile.

Both original production files and core/public-core copies are restored byte-
exactly. Two useful independent-traversal controls remain: 24-block chains across
all eight bit phases/every truncation, altered padding/restart/interior markers,
generic 1–16-bit reads, long/invalid codes, predictor overflow, positions and
canaries. Restored internal suites pass **2,164 checks per OS**, plus the new
scalar controls and Linux AVX512-off. Unchanged 2,193 public + 948 live and same
actual package/three examples are reused: **5,305 applicable checks per OS**.
Two core/three internal builds per OS include the necessary correctness fix and
restored test assembly; zero public/differential/harness/native/selector builds
or repeated package proofs. Package bytes remain unchanged.

[Evidence](jpeg-cross-block-lookahead-rejected-evidence.json) retains actuals,
brackets, decisions, failed precheck and restoration. Mirrored/member-verified
raw jpeg-cross-block-lookahead-rejected.tar.xz SHA256
c42338bc755d73f2dfe3146c6ac0e8566989721396e4f520769b47e98f5a2a45
(2,502,360 bytes, 108 members); restored boundary qualification archive
jpeg-cross-block-boundary-qualification.tar.xz SHA256
73bbdfc0245c8ca6c56227c016b7eab9c35411cbafb23579206950c6ba7bd641
(939,436 bytes, 22 members). The reused original short is retained in the cited
[CMYK allocation trial](CMYK-OVERWRITE-ALLOCATION-REJECTED.md) archive.
This lookahead family is closed. All 76 costs and 268 required paths remain.
