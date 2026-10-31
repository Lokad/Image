# Ordinary progressive JPEG refinement window

The existing ordinary 4:4:4 decode is **9.41% faster independently**, with exact
pixels and unchanged **984,212 B/op**. All-actual elapsed improves 9.41%; the
native-normalized point improves **9.32%**. Raw and normalized confidence separately
separate: candidate upper **1.349** is below original lower **1.427**. This remains
a CPU gap against the ≤1× objective.

| Existing complete warm one-CPU operation | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Progressive444 | 4,487.297 | 1.335 / 1.349 | 984,212 | Gap |
| ProgressiveGray | 2,270.109 | 1.326 / 1.332 | 683,126 | Gap |
| Progressive420 | 2,208.307 | 1.060 / 1.160 | 775,318 | Confidence open |
| Progressive422 | 2,724.566 | 1.206 / 1.223 | 847,116 | Gap |
| ProgressiveCancelable444 | 4,821.634 | 1.438 / 1.448 | 984,212 | Gap |
| ValidateJpegProgressive420 | 291.290 | 1.087 / 1.094 | 4,072 | Gap |

One local 64-bit window retains ordinary lookahead across refinement codes and
fields. Grouped corrections, bit reversal, PDEP and signed updates are unchanged.
Unused whole bytes return to the logical reader before fallible reads and exits.
Failed EOB fields keep already-read whole bytes; coefficient overflow keeps the
already-consumed correction field. Live cancellation retains its original reader;
portable fallbacks remain. No new buffer, owner, cache or admission allowance.

The original 513×259 packets, settings and reference boundaries stay frozen.
Complete public/native reconstruction, conversion, fresh owned RGBA8 and release
are timed; validation excludes RGBA normalization on both sides. Every managed
frame equals its frozen native frame; source, independent owners, opaque alpha,
CLI/scalar checks and quiet-v2 guards pass. 4:2:0 decode confidence remains open
(lower 0.969 / upper 1.160); it is retained without retiming or a parity claim.
Companion costs are current observations, not causal gain/regression comparisons.

One fresh original short is stable (−1.36% raw/native −0.27%), permitting reuse
of the existing original twelve. The 8.66% short gain advances to one independent
confirmation. Five existing companions each receive one current twelve. One core
and internal-test build per OS; zero harness/native/public-test builds or new
profile/input. One small short selector is built; the missing validation-confirm
host is rebuilt from unchanged cached source before any successful actuals.

**5,289 checks per OS** pass (2,156 internal + 2,185 public + 948 live), plus
41 internal and 346 public controls normal/scalar and Linux without AVX512.
Four new controls cover consecutive windows, phases/truncations/error tails,
EOB partial bytes, grouped overflow and zero arithmetic allocation. The **same
actual Windows Release package/DLL** passes both OSes and three examples,
including packaged normal/live/async progressive pixels and retained-root,
borrowed-input and independent-survivor ownership. Published releases remain
unchanged. Temporary managed-hash/proof metadata is restored byte-exactly.

Only ordinary progressive decoding/validation uses the changed body; baseline
JPEG, encoders, metadata-only reads, other codecs and editing/text reuse applicable
unchanged-path evidence. The live-token algorithm is unchanged and gets a current
sentinel. These six named paths exhaust this round's affected performance scope;
no shape/settings grid is added. This primitive is closed after acceptance.

[Numeric evidence](warm-jpeg-refinement-window-evidence.json) retains every actual,
bracket, confidence view, source/producer/package identity and exact-restoration
record. Raw jpeg-refinement-window-qualified.tar.xz, SHA256
34228786823fe3ea18cb4ff3495eb7f543773bfe7f31e300747efa499ef07fd8,
is mirrored with all **243 members** verified, including actual builds/package,
source/tests, selectors, workers, input/proofs, frames, scores and guards. Earlier
qualified archives remain dependencies. All **268 material paths** remain required;
this is not a full immutable-package release benchmark refresh.
