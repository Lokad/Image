# VP8L remaining production coverage

One dependency-free source model reproduces nineteen packet identities on Windows/Linux:
thirteen default-method packets, the method-0 two-color variant and three
non-palette effort controls on the existing asymmetric input, plus one adaptive
photograph packet and one effort-0 numeric-palette packet. Main quality
is 75 with `exact=1`; palette auxiliary encoding uses the backend's internal quality 20.
Production adoption also has to preserve the existing public encoding scope.
This bounded checklist comes from current tests and options; it adds no warm
performance grid.

1. **Palette/index coding.** `WebpTests.SixteenBitInputRoundsOnceToEightBitSamples`
   supplies a 1×1 input normalizing to RGBA `(255,0,1,0)`. Its 34-byte palette
   packet matches the pinned backend through the shared palette composition path.
   The opaque index carrier preserves the real alpha in the palette.
   The existing two-color 4103×2 admission input also has an exact source-only
   packet on both platforms: 46 bytes, including palette deltas, eight indices
   per byte, a partial final byte and complete framing. The current model qualifies
   both palettes alongside the other packet families. Three further named controls
   now pass: **Palette4ColorPartialPacking** (four indices per byte, 66 bytes),
   **Palette16ColorPartialPacking** (two per byte, 130 bytes), and
   **Palette18ColorUnpackedZero** (unpacked indices and greedy ordering, 236 bytes).
   The last uses 17 encoded entries for 18 source colors: the final zero is implicit.
   All palette orders, packed samples and complete packets are exact on both
   platforms. This covers the named branches, not arbitrary palette inputs/settings.
2. **Direct coding and variable dimensions.** The existing 129×129 xorshift input
   now has a byte-exact 50,010-byte source-model packet on both platforms, reusing
   the pinned snapshot. The dimension-aware parser and framer select no transform,
   no cache and one group. This replaces the obsolete predecessor-packet target
   when the new encoder lands. The public native encoder refuses a side of 16384;
   its limit is 16383. Two explicitly named **Axis16383Horizontal/Vertical**
   companions, using the first 16383 samples of the existing authored axis input,
   isolate single-axis/narrow-distance coding and both match complete 74-byte
   reference packets in the current model. They do not substitute for the existing
   16384×1/1×16384 functional tests or establish native packet parity there.
   The same frozen model also emits 74-byte packets at both original 16384-axis
   sizes; pinned FFmpeg decodes every sample exactly. These are functional format
   extensions beyond the native encoder's maximum, without an encoder-byte or
   performance claim. Preserve their functional tests during production adoption.
   The source distance mapper gives direct
   coordinates priority where forms overlap on narrow widths; ten fixed format
   examples pass on Windows/Linux. Current four-core widths are unaffected.
3. **Spatial-only selection.** The existing 19×7 hidden-RGB control has 133 colors
   but the pinned backend selects prediction and cross-color, without
   subtract-green or palette coding. Its 82-byte reference uses uniform 512×512
   transform maps. The source-only model now reproduces all 82 bytes, both maps
   and every residual sample on both platforms. Its histogram-based palette
   eligibility, spatial-only prediction and uniform map sizing are qualified on
   this input; other maps and general shape coverage remain open.
4. **Existing effort range.** The two-color input now matches native methods 0..6
   in the source model on both platforms: 48 bytes at method 0, 46 at methods 1..6.
   The fast matcher omits left/above probes and emits 1,026 literals here, yielding
   two histogram groups; full parsing uses three tokens and one group. Native
   default-method and 16-bit normalization argument paths also retain those bytes.
   The existing asymmetric RGBA8 input also passes three exact complete packets:
   **Effort0ForcedPredictorRgba8** (316,566 bytes, forced mode 11, four-bin fast
   clustering and equivalent histogram-map coarsening), **Effort1CoarseTilesRgba8**
   (176,216 bytes, 64×64 transform tiles), and **Effort5CacheComparisonRgba8**
   (155,162 bytes, cached versus uncached writer selection). The full method-5
   adaptive selector now also reproduces every 4×4/8×8 predictor mode and entropy
   cost on the asymmetric input and the existing photograph. It selects 8×8 on
   the first, 4×4 on the photograph; their complete packets are 155,162 and
   160,600 bytes. **Effort5AdaptivePhotograph** therefore closes the missing
   fine-winner algorithm on an existing input, without a new fixture/grid.
   Hierarchical traversal, per-level histories, entropy selection and packet
   composition pass together in the same model on both platforms.
   The existing eighteen-color input now qualifies both method-5 transform
   candidates: 236-byte plain palette and 256-byte palette-plus-spatial. Both
   complete candidate packets match; source-derived payload comparison selects
   the native plain result. Packed index carriers use adaptive prediction without
   subtract-green or cross-color, and cache search is capped by palette size.
   This verifies both candidates and plain selection; it adds no spatial-winning
   input or selected packet identity.
   The remaining two effort primitives now have named proofs.
   **Effort0NumericPalette18Color** reproduces all 268 bytes with numeric order
   and an omitted final zero. **SmallPaletteBoxTwoColor** reuses the existing
   4103×2 input: every 1,026-entry box index matches (1,020 nonzero copies), as
   do ordinary/box candidate packets at 46 bytes for plain indices and 50 bytes
   with prediction. Equal writer sizes retain the ordinary candidate. The small
   four-color carrier was not used for this proof because it has no long copies.
   Both new packets pass exact pinned-FFmpeg decoding, including hidden RGB.
   All nineteen selected packet identities and the forced candidate/index proofs
   pass in one frozen model on both platforms. These are bounded algorithm proofs;
   generic routing, admitted production and complete public operations remain open.
   Quality100/method6 brute-force search is outside this pinned quality75 scope.
   Reuse current two-color depths for complete public-operation qualification. Unpublished implementation
   assertions may change; exact normalized pixels and declared option/resource
   contracts remain required. No compatibility alias is needed for experiments.
5. **Complete public operations.** Qualify existing RGBA16 normalization and fresh
   nonseekable async delivery, including independent surviving outputs, failures
   and cancellation. Their core packet identity reuses the first frozen packet;
   complete operations still require integration evidence.
6. **Admitted integration.** Reserve substantial storage before allocation through
   the borrowed shared root, account for replacement peaks and retained outputs,
   preserve work/local limits, and release operation storage promptly. Refusal is
   terminal. Reuse current admission, cancellation, recovery and ownership suites;
   qualify the actual changed package on both platforms at the production milestone.

The direct/palette models and narrow-distance correction are development tooling.
They do not establish general palette, shape, production or performance readiness.
The current source project is `tools/WarmReference/Vp8lParityModel`; it needs no
production/reference assemblies to build. Fixture PNG decoding is separate.
Unchanged production qualification is reused; no performance/allocation claim
follows from these packet proofs. See
[numeric evidence](vp8l-byte-parity-evidence.json) and the
[complete-packet report](VP8L-COMPLETE-PACKET.md).
