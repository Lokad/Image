# Vector copy bound and grouping screen

Neither candidate is retained. The accepted source and exact core/embedded/public
modules are restored, reusing the existing 6,640 checks per OS and same actual
Release package. No broad/package campaign or Windows candidate build follows.

| Existing complete RGBA8 vertical flip, interleaved managed comparison | Original → candidate µs | Candidate/original interval | GC B/op |
| --- | ---: | ---: | ---: |
| A: common vector bound | 8.481 → 8.480 | 0.995–1.005 | 112 |
| B: four-vector group spans | 9.102 → 9.496 | 1.030–1.057 | 112 |

A removes repeated destination guards, shrinking the actual optimized method
from 1,110 to 952 bytes, but gives no complete-operation gain. B uses one group
base with fixed vector offsets, shrinking it to 948 bytes while adding a group
range check; complete elapsed is **4.33% worse**. Static size/instruction changes
are not CPU savings. Both keep the existing vector/tail/portable paths, safe
span access, traversal, cancellation, admission and owners.

The initial separated A screen has a 1.76% raw gain while native elapsed moves
−4.54%; normalized evidence is adverse. One purpose-built adaptation of the
existing paired helper resolves that conflict: 12 alternating, matched
256-operation original/candidate pairs in one process, all retained. A is neutral;
B is adverse. Exact original source/output/alpha/hidden RGB and independent
surviving outputs pass, with idle-v2 guards and no new image/settings grid.
These are managed incremental comparisons, not native parity proof.

Each Linux candidate passes 25 internal/76 public checks in normal, scalar,
AVX512-off and AVX2-off modes; targeted JIT capture passes 16 vertical checks.
Two core/embedded builds, zero public/harness/native/fixture builds and one paired
helper build. New capacity, heap buffers and retained operation storage are zero.
One postcapture tuple-field reader error is recovered without repeating timing.

Other callers are not timed for discarded code. This does not prove A helps them
or prohibit a revised design supported by new attribution. The original-build
[current confidence](VERTICAL-FLIP-CURRENT-CONFIDENCE.md) is a separate result.
[Evidence](vector-copy-bound-screen-evidence.json) retains all raw screens,
interleaved pairs, JIT code, attempted sources/modules and exact restoration.
Raw vector-copy-bound-screen.tar.xz, SHA256 f4ecb3ab25e7139725e6a3d0e4954686ed676129cf1191d2f2993bcab0bae84c, is mirrored and member-verified.
