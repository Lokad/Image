# VP8L production coverage

The finite checklist below is covered by the production managed encoder at
libwebp1.6.0 quality75, `exact=1`, with the declared matching effort. It comprises
19 selected packet identities across25 existing source/settings rows and two
additional forced candidate packets. It adds no warm benchmark grid.
[Complete packet sizes and provenance](VP8L-COMPLETE-PACKET.md) and the
[numeric ledger](vp8l-byte-parity-evidence.json) bound every claim.

1. **Palette/index coding.** Singleton RGBA `(255,0,1,0)`, two-color4103×2,
   four-color7×3, sixteen-color17×3 and eighteen-color23×13 controls qualify
   one/two/four/eight indices per byte, partial final bytes, numeric/greedy order,
   hidden alpha, retained palette ownership and the implicit final zero.
2. **Direct coding and axis boundaries.** Xorshift129×129 selects no transform,
   cache or extra group. Existing16383×1/1×16383 companions both match their
   distinct74-byte native packets, qualifying narrow-width distances and both
   axes. The separate16384-axis tests qualify exact functional round trips beyond
   the native encoder maximum, without a native packet claim.
3. **Spatial-only selection.** Hidden-RGB19×7 selects prediction/cross-color
   without subtract-green or palette, including uniform512×512 maps. Asymmetric
   RGBA513×259, its opaque projection and photograph/diagram qualify the other
   non-palette families, cached parsing and multiple spatial groups.
4. **Effort and candidate routing.** Existing two-color methods0..6, asymmetric
   methods0/1/4/5, photograph method5 and eighteen-color methods0/4/5 qualify
   low-effort forced prediction/numeric ordering, fixed/adaptive transform sizes,
   cached/uncached and ordinary/Box choices. Independent forced palette/spatial
   packets test both candidates even where plain coding wins. No claim is made
   about method6/quality100 brute force or unmeasured inputs/settings.
5. **Public operation boundaries.** Asymmetric RGBA8/16 normalizes once to the
   same packet through sync/async and ImageBudget APIs. Source/root/stream are
   borrowed; outputs remain independent. Forward-only, output-size, writer failure,
   cancellation and work-limit tests remain applicable. No budget-dependent
   alternate encoder or compatibility mode is retained.
6. **Owned implementation and qualification.** Analysis, palette, prediction,
   cross-color, matches, parsing, clustering, Huffman and framing use the same
   admission context. Capacity is reserved before allocation, overlapping owners
   stay charged, losing candidates release and refusal is terminal. Focused
   tests cover retained results, refusal/recovery, cancellation and late work
   failure. Full Windows/Linux offline/live differential suites, affected scalar
   checks and the SAME actual private Release package/three examples qualify
   the public integration. Final axes are a test-only coverage addition against
   the unchanged production DLL; their receipts are recorded separately.

This completes the named VP8L integration checklist, not the full codec objective.
Exact lossy WebP encoding and decoder samples remain open. New production encoder
CPU/allocation evidence is unmeasured; earlier producer timings are retained with
that limitation. Resume speed work only after required exact codec coverage.
Add a case only when evidence identifies a distinct required omitted path.
