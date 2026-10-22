# VP8L remaining production coverage

The four frozen non-palette packet cores are raw-exact on Windows/Linux.
Production adoption also has to preserve the existing public encoding scope.
This bounded checklist comes from current tests and options; it adds no warm
performance grid.

1. **Palette/index coding.** `WebpTests.SixteenBitInputRoundsOnceToEightBitSamples`
   supplies a 1×1 input normalizing to RGBA `(255,0,1,0)`. Its 34-byte palette
   packet now matches the pinned backend on both platforms in a separate source
   primitive. The opaque index carrier preserves the real alpha in the palette.
   Multi-color ordering/deltas and packing remain open: reuse the existing 19×7
   hidden-RGB control and the two-color 4103×2 admission control.
2. **Direct coding and variable dimensions.** The existing 129×129 xorshift input
   selects direct coding, confirmed by a fresh snapshot from the unchanged pinned
   worker. Its 50,010-byte packet replaces the obsolete predecessor-packet target
   when the new encoder lands. General sizing and borders must also cover the
   existing 16384×1/1×16384 controls. The source distance mapper now gives direct
   coordinates priority where forms overlap on narrow widths; ten fixed format
   examples pass on Windows/Linux. Current four-core widths are unaffected.
3. **Existing effort range.** Public `Effort` accepts 0..6. Reuse the current
   single two-color admission input at its existing efforts/depths for functional
   and option qualification. Do not multiply this into a new image/settings matrix.
   Implementation-specific old packet/header assertions are replaceable; exact
   normalized pixels and the declared option/resource contracts remain required.
4. **Complete public operations.** Qualify existing RGBA16 normalization and fresh
   nonseekable async delivery, including independent surviving outputs, failures
   and cancellation. Their core packet identity reuses the first frozen packet;
   complete operations still require integration evidence.
5. **Admitted integration.** Reserve substantial storage before allocation through
   the borrowed shared root, account for replacement peaks and retained outputs,
   preserve work/local limits, and release operation storage promptly. Refusal is
   terminal. Reuse current admission, cancellation, recovery and ownership suites;
   qualify the actual changed package on both platforms at the production milestone.

The 1×1 palette primitive and narrow-distance correction are development tooling.
They do not establish general palette, shape, production or performance readiness.
The default four-core builds remain valid and are not retuned. See
[numeric evidence](vp8l-byte-parity-evidence.json) and the
[complete-packet report](VP8L-COMPLETE-PACKET.md).
