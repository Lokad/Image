# Initial JPEG spectral AC window

[Narrow scan preparation](JPEG-NARROW-SCAN-PREPARATION.md) supersedes 27 decoder
costs and owns the current 29-read scope. Two unchanged coding-validation costs
and this stage's exact entropy/gain evidence remain valid.

JSA694 keeps the existing bit cache and coefficient index local across short
Huffman symbol/amplitude pairs in an initial progressive AC scan. Ordinary and
live-token calls share this path. Every original logical cancellation check,
signed value, band/shift and partial cursor is preserved. Four-byte lookahead
admits only ordinary bytes and cannot raise a future marker error. Long codes,
EOB/ZRL, invalid runs, escaped bytes and short tails remain unconsumed for the
original scalar traversal. Refinement changes only a shared publisher's name.
No new table, array, reservation, owner, pool, session or production dependency.

Independent contemporary grayscale confirmation saves **132.88 µs / 7.40% raw /
7.26% native-normalized**: 1,795.99→1,663.11 µs. Filtered/all-actual raw and
conservative normalized confidence separate. The original ordinary-only first-AC
trial was an unresolved lead on an older allocation pipeline; this both-token,
logical-poll, normal-publication implementation is a revised experiment.

| Complete progressive operation, 513×259 | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Grayscale decode | 1,663.11 | 1,711.72 | .972 / .997 | 151,432 | Pass |
| Ordinary 4:4:4 decode | 3,260.12 | 3,361.69 | .970 / .994 | 169,664 | Pass |
| Live-token 4:4:4 decode | 3,470.52 | 3,366.73 | 1.031 / 1.059 | Gap |
| 4:2:0 decode | 1,795.48 | 2,162.47 | .830 / .836 | 243,664 | Pass |
| 4:2:2 decode | 2,304.03 | 2,480.88 | .929 / .943 | 315,496 | Pass |
| 4:2:0 coding validation | 288.75 | 297.28 | .971 / .984 | 4,088 | Pass |

Each row has twelve actuals, native brackets and idle-v2 guards on the quiet VM,
sequential Release/one CPU. Parsing, preparation, required storage, computation,
independent output and disposal remain timed. Native retains its labeled codec,
frame/input/conversion context; it has no matching live cancellation token. Frozen
inputs, exact managed pixels/descriptors and existing native gates are unchanged.
These are complete public operations, without a fresh-native-context parity claim.
Five passing controls stop;23 unchanged read costs reuse valid evidence. Current
29-read scope is **19 pass / 9 gap / 1 uncertain**, not general JPEG parity.

Live444's short raw point is adverse (3,568.49→3,735.26 µs); confirmation favors
the candidate (3,705.58→3,470.52 µs). Native clocks differ, and conservative
normalized incremental intervals overlap. No robust live speedup, regression or
equivalence claim follows. All adverse samples remain; no favorable repeat.
Other historical-to-current point changes are not causal gain claims.

Allocation counts are unchanged. Prior [completed-output](JPEG-COMPLETED-OUTPUT.md)
coarse admission and retained-result traces remain source-valid: gray peak699,834 B,
result531,468 B until disposal, then zero charge. Existing idle pixels/coefficients
and governed pool bypass are unchanged. Physical/native peaks are unmeasured.

Eight new cases cover independent scalar traversal, bands/phases/shifts, errors,
stuffing/truncation/restarts, exact partial state, precancellation/concurrent
logical prefixes and zero helper allocations. Windows/Linux each pass **6,050
full/live checks**,435 internal/375 public per normal/scalar and AVX2-disabled
mode, and the SAME actual Release package plus all three examples. One immutable
main/embedded build per OS; no harness, selector, native or fixture rebuild.
Completed ordinary444 measurements were recovered after a source-identity schema
correction. Validation's stale producer preflight failed before actuals; rebinding
only that identity retained its independent descriptor/input/ownership checks.

[Evidence](jpeg-spectral-ac-window-evidence.json) pins sources, modules, qualification,
unchanged providers and all observations. Mirrored/member-verified raw
`jpeg-spectral-ac-window-qualified.tar.xz`: SHA256
`ac71ab4df454faa4c90027dac2f4e9a721388b5c76b57e5ca1cf2eff863eaedb`,
5,872,548 B /210 members. The full283-path objective remains open; publication is
postponed.
