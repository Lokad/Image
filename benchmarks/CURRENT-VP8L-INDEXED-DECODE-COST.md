# Current indexed lossless WebP decode

The existing sixteen-color control independently confirms **1.070× /
upper 1.081**, **581.342 µs** and
**820,192 allocated B/op** on the qualified WOD1 core.
It remains a gap. This resolves historical costs; it makes no before/after gain
claim and changes no production code or required case.

The same 11,810-byte, 513×259 type3 palette packet and original sixteen-color
projection are retained. Complete public decode, packed-width/palette expansion,
RGBA normalization, independent output ownership and release stay timed. Exact
RGBA, alpha, hidden RGB, normal/scalar native pixels, source preservation and
independent owners pass, as do both versioned quiet guards. All twelve actuals,
filtered/unfiltered statistics and native brackets are retained.

One tiny twelve-actual selector is built from the cached typed driver. Production,
harness, native worker and fixtures are reused unchanged. Before measurement,
the source guard catches a changelog/manifest mismatch and is reconciled to the
already verified WOD1 final manifest. No actual source or binary changes and no
successful timing repeats. Applicable 5,199 checks per OS, focused normal/scalar,
the same actual package, governed async consumer and three examples are reused.

[Evidence](current-vp8l-indexed-decode-cost-evidence.json) pins producers,
boundaries and the mirrored/member-verified current-vp8l-indexed-decode-cost.tar.xz,
SHA256 e21a54a21c2fd648bc07103ab32afb0dfb3a651922591eebf472ae48103bbd51.
This smaller gap gets no follow-up variant; larger gaps and the finite 268-path
objective remain priorities.
