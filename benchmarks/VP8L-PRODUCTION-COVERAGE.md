# VP8L remaining production coverage

One dependency-free source model reproduces ten packet identities on Windows/Linux:
the four frozen non-palette cores, four existing functional inputs and two named
single-axis companions. All use method 4, quality 75 and `exact=1`.
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
   both palettes alongside the other packet families. Three named controls remain
   to exercise distinct untested branches: **Palette4ColorPartialPacking** (four
   indices per byte), **Palette16ColorPartialPacking** (two per byte), and
   **Palette18ColorUnpackedZero** (unpacked indices, greedy color ordering and
   handling a zero palette entry). These are bounded algorithm controls, not a
   palette/shape grid; their inputs and native packets still need to be frozen.
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
   That reference boundary needs resolution before release qualification.
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
4. **Existing effort range.** Public `Effort` accepts 0..6. Reuse the current
   single two-color admission input at its existing efforts/depths for functional
   and option qualification. Do not multiply this into a new image/settings matrix.
   Implementation-specific old packet/header assertions are replaceable; exact
   normalized pixels and the declared option/resource contracts remain required.
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
