# FP32 Lanczos3 byte-reduction trial

The proposed wider horizontal primitive is **50.7% slower** on the
existing complete RGBA8 reduction. It is rejected; the accepted source, core,
internal tests and symbols are restored exactly on both OSes, without rebuilding.

| Existing complete operation | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Rejected Reduce8 | 1,143.69 | 490.80 | 2.330 / 2.657 | 146,693 |
| Accepted Reduce8, retained | 758.75 | 490.07 | 1.548 / 1.646 | 138,466 |

One admitted 8,240-byte source row holds exact byte premultiplication as FP32.
Four tap subsets retain original Q30 plans. Bounds include coefficient conversion,
every FMA, recombination and tail, then double vertical accumulation. The strict
certificate and original Int128 fallback preserve exact pixels. Local selection
includes overlapping row/cache/output capacity and work before shared admission;
refusal stays terminal. No new cache, root, image or settings grid.

A narrow frozen-plan check on both OSes certifies 32,860/33,024 original pixels;
164 require fallback and no certified pixel differs from the independent frame.
Sixty focused Lanczos controls pass in normal, scalar and AVX512-off modes per OS,
including signed-coefficient envelopes, exact pixels, guarded rows, ownership,
refusal and cancellation cleanup and subsequent success. Two initially overstated
test assertions were corrected before freezing; failed output remains archived.

Two completed one-second diagnostics retain 946 accepted-producer samples and
964 candidate samples, with both official guards each. Horizontal filtering
accounts for 53.6% and 58.3% respectively; candidate exact fallback accounts for
5.2%. These are sampled locations, not comparative timings or new confirmations.
They do not establish fallback as the principal cause of the regression.

The scored short retains all three actuals, native 3+3 brackets and selected/all-
actual confidence views. Both guards and every original native/managed quality
gate pass. Reference remains **zimg, FFmpeg's zscale backend**, with the
[complete equivalent operation](WARM-LANCZOS3-BASELINE.md). No longer repeat above
parity, full suite or package proof for rejected code. The accepted 4,484 tests/OS
and same actual private 0.2.0 package qualification remain valid; published 0.1.0
is immutable. No accepted numeric row or coverage count changes.

[Evidence](warm-lanczos3-fp32-byte-trial-evidence.json) includes identities,
allocations, bounds, sanitized profiles and raw hashes. Both candidate archives
and the diagnostic archive are mirrored and verified. Next: resolve the two named
historical default PNG resident measurements on the unchanged accepted producer.
Uniform parity and replacement readiness remain open.
