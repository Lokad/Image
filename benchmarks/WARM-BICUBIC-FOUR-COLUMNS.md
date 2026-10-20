# Four-column bicubic strips

Four-column strips save **16.2%** on the existing tight-memory RGBA8 reduction,
preserving exact pixels and the same 4KiB cooperative reservation.

| Existing TightWide8 operation | Lokad µs | Native µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Previous accepted producer | 594.66 | 349.08 | 1.704 / 1.949 | 32,913 |
| Four-column producer | 498.19 | 347.76 | 1.433 / 1.644 | 32,913 |

At scale<=4.125,17row/coefficient slots and byte relative offsets let four columns
fit in4,068B. The fixed513×259→128×64 operation needs32strips instead of43;
both output pairs use the existing SIMD certificate. Actual tap/offset bounds are
checked before writes. Larger supports retain three columns, with unchanged
per-pixel coefficient/addition order, conservative work, cancellation and exact
original-order replay. No new heap storage, cache, budget root or refusal recovery.

The original rational full frame, input/native hashes,568332B local limit,
source and independent outputs pass. The labeled zimg reference is FFmpeg's
zscale backend, with the original packed-input/halo/float-filter/quantization/
owned-output boundary. Native normal/scalar bounds remain maximum1/RMSE0.25.
Native allocations are unmeasured. This normal-operation timing does not qualify
performance with a supplied shared root.

One short retains three actuals, native pre/post brackets and both official
idle-v2 guards. The same frozen producer serves all qualification; original
harness/selector/native worker are reused. No new fixture, settings grid,
profile or long repeat above parity. The earlier profile describes the previous
producer only.

Qualification: **4,475 Release tests per OS** (1,651 internal,1,892 public,932 live
FFmpeg),14focused strip/root controls normally, scalar and AVX512-off per OS.
These cover tails, scale bound, negative lobes/tiny alpha, retained siblings,
replacement peaks, terminal refusal, cancellation cleanup and subsequent success.
The same actual private0.2.0 package passes both consumers and all three examples,
including the original bicubic frame and independent charged survivors.

Only the existing TightWide8 row changes. Default Wide8 retains its earlier cached
path; word/mixed/enlargement cases reject this helper before its changed body.
Other supports use their unchanged earlier helpers or retain the three-column
mode. Unchanged JPEG/Lanczos proofs are reused. Published0.1.0 remains immutable;
the finite254-case parity and release objective stays open.

Next: reconcile current-code performance of the existing VP8 simple-luma-filter
case before optimizing it. [Evidence](warm-bicubic-four-columns-evidence.json)
retains statistics, allocation, source/package identities and mirrored raw hashes.
