# Shared preparation for Lanczos word rows

[Current byte confidence](LANCZOS-BYTE-PRODUCT-PREPARATION.md) supersedes the
prepared-byte cost below: 489.94/zimg 491.00 µs upper 1.015, uncertain on the
unchanged actual package. Other seven costs remain source-valid here; the current
eight-control scope has six passes and two uncertainties. Row-product candidates
are unselected, with exact production restoration and adverse evidence retained.

LWP776C extends the existing row scheduler to share Q30 coefficient loading,
conversion and lane preparation across two immediately required missing RGBA16
rows. Their products and original four FMA chains remain independent. No new
owner, pool, session, coefficient matrix or numerical policy is introduced.

The B paired study establishes prepared **550.41→509.31 µs**, saving41.10 µs /
7.47% raw /7.84% reference-normalized; one-shot **684.52→643.37 µs**, saving41.15 µs /
6.01% raw /6.32% normalized. Both filtered/all-actual confidence views separate.
Those scoped mechanism results do not replace final-build costs below.

| Existing complete operation / explicitly labeled zimg | Lokad µs | Native µs | Ratio / upper | GC B/op | Retained plan B | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| OneShot8 | 667.33 | 1,629.83 | 0.409 / 0.436 | 138,647 | 0 | pass |
| Repeated8 (superseded) | 504.32 | 491.61 | 1.026 / 1.039 | 138 | 139,800 | historical gap |
| OneShot16 | 698.00 | 1,963.30 | 0.356 / 0.360 | 155,077 | 0 | pass |
| Repeated16 | 550.80 | 552.29 | 0.997 / 1.012 | 139 | 156,248 | uncertain |
| OneShotEnlarge8 | 3,769.63 | 5,231.96 | 0.720 / 0.726 | 319,224 | 0 | pass |
| RepeatedEnlarge8 | 3,496.35 | 4,060.63 | 0.861 / 0.865 | 148 | 325,156 | pass |
| OneShotEnlarge16 | 4,084.94 | 5,070.48 | 0.806 / 0.811 | 4,592,818 | 0 | pass |
| RepeatedEnlarge16 | 3,797.13 | 3,893.13 | 0.975 / 0.979 | 4,265,332 | 333,396 | pass |

All eight costs use the immutable DLL inside the actual final package. Both
prepared sides retain geometry and workspace; both one-shot sides include their
creation. Source reads, row-ID reset, exact computation/certification/repair,
normalization, independent output ownership and disposal remain timed. Twelve
actuals and native samples per bracket, filtered/all-actual statistics, CPU0 and
idle-v2 guards are retained. Managed pixels remain exact; native float gates are
unchanged. These controls do not establish all resizing or283-path parity.

Contemporary original prepared word547.68/native509.71 µs versus final550.80/
552.29 µs gives a0.57% raw increase and7.18% normalized reduction. Both views
are disclosed; this is not a repeated raw-gain confirmation. Original prepared
byte528.43/native516.91 µs also has a gap (upper1.027); final byte normalized
intervals overlap. Do not attribute its current gap to this redesign. No final
candidate is repeated to obtain more favorable bounds. Six passes stop; prepared
word uncertainty is resolved before assuming another optimization is needed.

The primary existing UInt32 product array grows8208→16416 B, with no added array
header or workspace field. Coarse plan retention148040→156248 B and the scoped
owned peak rise8208 B. Prepared GC139 B/op is unchanged; one-shot155077 versus
146866 B/op includes the array growth and amortized instrumentation. Independent
word result264192 B and source1062936 B are separate. One-shot scratch retention
after disposal is zero. Physical/native/JIT-stack peaks remain unmeasured.

Capacity is chosen from local limits and array bounds before shared admission.
A refused reservation terminates without retry or a smaller fallback. One-shot
selection leaves room for its pending result; plans already account that result.
This is selection headroom, not an extra reservation or allowance. The same root,
cleanup, cancellation, private cache publication and surviving-result lifetimes
remain. Source reconstruction verifies all other arithmetic/axes/certificates/
repair, default byte/word and enlargement implementations remain verbatim.

The initial A implementation spills a64-byte accumulator inside the tap loop and
screens10.5% slower. B separates source preparation from a NoInlining numeric
producer and consumes checked256-bit product halves promptly; its numeric body
is1038 B/frame56 versus A1680 B/frame216, with no loop accumulator spill. Code
size alone is not a performance claim. B's word-enlargement counter has a1.74%
normalized adverse result while still passing; that evidence is retained. C only
fixes pending-output headroom, refreshing all eight actual costs without changing
the numeric producer. The earlier [byte rows](LANCZOS-PAIRED-BYTE-ROWS.md) and
[attribution](LANCZOS-WORD-REDUCTION-ATTRIBUTION.md) remain history and provenance.

Eleven added tests cover independent scalar-FMA bits and BigInteger envelopes,
six geometries, full unsigned products, guards, source immutability, no producer
allocation, aggregate/root ownership, terminal refusal, cancellation/recovery and
tight local output headroom. Final **6197 full/live checks per OS**,362internal /
747public resize checks in normal/scalar/AVX2-off/AVX512-off modes, and the SAME
actual package/all three usage examples pass Windows/Linux. The package consumer
also exercises the tight pending-output boundary through public APIs.

Three coherent main/embedded versions per OS are retained. Interim B broad/ISA
6196checks precede the late C safety fix; no Bpackage proof was run. The initial C
test compile and package-consumer compile each needed one API correction. The
already packed package was reused after the consumer-only correction. No harness,
native worker, selector, fixture or new image/settings matrix was built; no
unchanged favorable sampling repeats occur.

[Evidence](lanczos-paired-word-rows-evidence.json) records current costs, all
comparisons, source/code proofs, ownership, adverse stages and qualification.
Private0.2.1 package SHA256 `d1b2574906275ce426d4a72cef7e71b3a0cd397e496f395e532c474f1fd8d829` contains timed DLL
`d42a3d7a63ca9395474d67d4edd64fe3ab1f361475a686bc9be841ed1a0b808d`. Publication remains postponed and published
packages stay immutable. Raw `tmp/lanczos-paired-word-rows.tar.xz`, SHA256
`b4d9cc9ca790793ec9875850ebf88bae8148eba4c209d92c484562a538432d50`, 49,186,292 B/531members, is mirrored and
member-verified; its pinned dependencies retain unchanged tooling and history.
This report supplies seven current costs; the linked byte-confidence owner supplies
the remaining prepared-byte cost. Earlier measurements stay retained history.
