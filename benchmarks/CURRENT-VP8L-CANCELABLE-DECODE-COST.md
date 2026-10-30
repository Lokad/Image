# Current live-token lossless WebP decode

The existing live-token control independently confirms **1.282× /
upper 1.291**, **3,877.435 µs** and
**684,377 allocated B/op**. It remains a gap;
this resolves an old changed-path measurement without a historical gain claim.

The original 183,812-byte, 513×259 exact=1 packet, complete resident decode,
normalization, independent owned result and release stay fixed. The caller token
is live and never signaled during timing; its lifetime remains outside timing.
The native reference has no cancellation option and retains its approved complete
operation. Exact RGBA/alpha/hidden RGB, normal/scalar native pixels, source/owners,
live-token/already-canceled rejection and both quiet guards pass. All twelve
actuals, filtered/unfiltered bounds and native brackets are retained.

The immutable WOD1 core, harness, worker, fixture and VIC1 generic twelve-actual
driver are reused: **zero builds**. Applicable 5,199 checks per OS, normal/scalar,
the same actual package, governed async consumer and three examples remain valid.
No source sync, profiling, variants or successful timing repeats.

[Evidence](current-vp8l-cancelable-decode-cost-evidence.json) pins producers and
the mirrored/member-verified current-vp8l-cancelable-decode-cost.tar.xz,
SHA256 9ce249357ade174f449e91498874c418a124dfb4d2554215ea0c28a652d4660a. No cancellation-specific optimization is inferred;
larger gaps and the full 268-path objective remain open.
