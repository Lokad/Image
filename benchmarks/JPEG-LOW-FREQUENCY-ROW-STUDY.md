# Low-frequency JPEG row study

JLP774 A–D is **not retained**. The final candidate improves complete horizontal
CMYK decoding, but contemporary original-build counterchecks expose larger
adverse complete YCCK and grayscale costs. The stage does not justify acceptance.
Qualified production and the private package are restored byte for byte; current
benchmark costs remain unchanged. This does not permanently close low-frequency
preparation or establish that its arithmetic caused every observed difference.

| Complete frozen control, independent confirmations | Original µs | D µs | Native original / D µs | Original / D B/op |
| --- | ---: | ---: | ---: | ---: |
| Horizontal CMYK | 446.22 | 408.83 | 399.37 / 426.95 | 20,944 / 20,944 |
| Direct RGB444 | 1,447.28 | 1,447.61 | 1,449.68 / 1,452.29 | 42,936 / 42,936 |
| YCCK420 | 423.83 | 556.78 | 706.87 / 708.64 | 895,563 / 895,559 |
| YCCK420, caller RGB | 436.83 | 604.80 | 604.59 / 604.79 | 762,721 / 762,713 |
| Caller grayscale | 609.74 | 655.46 | 647.65 / 647.49 | 273,444 / 273,439 |

Horizontal saves37.38 µs /8.38% raw /14.30% normalized, with separated
filtered/all-actual intervals; native movement is+6.91%. RGB intervals overlap,
without a gain, regression or equivalence claim. The three adverse controls have
separated raw and normalized intervals in both views. D's standalone horizontal
upper .964 passes, while its RGB upper1.017 remains uncertain. A primary pass
cannot qualify the complete stage. All actuals, native brackets and counterchecks
remain retained; there is no unchanged favorable repeat.

## Preparation and execution

The hypothesis removes algebraically zero row products and intermediate storage
for dequantized top-left2×2 support. Nonconstant rows use their first pair product;
constant rows retain signed-word wrapping, distinct from AC row saturation.
First/second transformed vectors feed the shared column publisher. A repeated
horizontal row is broadcast to a complete64-byte output. Existing Q14/shift/bias,
Int32 modular sums, clipping, reflection, checkpoints and exact pixels remain.
No encoder, packet, metadata, precision or ownership policy changes.

A's general six-word searches and separate publisher screen425.69/native392.36 µs
versus original430.71/native402.98 µs, without a useful normalized lead. B's direct
entry and inline publication confirm a horizontal gain but have adverse RGB points.
C replaces a per-block short-span search with bounded reads; its horizontal gain
also confirms, while RGB remains unresolved. One completed C RGB run fails the
**after** I/O-wait guard (.01249 > .01), is retained unscored and receives one
immediate guarded replacement. No cooldown, threshold relaxation or hidden retry.

D restores the entire shared inverse entry and puts fixed-size eligibility under
the existing two-low-row gate. Its RGB pair resolves the preceding discrepancy,
but the broader25-control refresh exposes the table's adverse results. A YCCK
code capture compiles only the unchanged global-DC inverse entry, with no
`InverseAvx2` or new low-frequency body. The cause of that complete-cost
contradiction remains unresolved; this is not proof that the new arithmetic slows
DC blocks. The stage is restored instead of dismissing standalone evidence.

D's actual RGB capture has a551-byte shared entry,1,761-byte general AVX2 body
and357-byte specialized helper. Initial frame adjustments are40/280/32 B;
the shared entry separately reserves two128-byte stack blocks. Observed helper
readonly data is64 B, including weights, bias and padding. These differ from
owned capacity and unmeasured physical/native/JIT-stack peaks. No per-image heap,
new static format arrays, retained operation scratch, pool, session or owner is
added. Ceil-rounded GC differences do not establish a capacity improvement.

## Correctness and retained coverage

All four implementations pass462 internal/375 public JPEG checks per normal/scalar
mode on Windows/Linux. C/D additionally complete6,186 full/live Windows checks;
D passes affected Windows ISA and actual-package/all-three-example checks before
the final performance counterchecks. Those interim proofs are not acceptance or
a Linux/package-equivalence claim. Four main/embedded versions per OS, no production
compile failures and zero harness/native/selector/fixture builds serve the study.
No benchmark image/settings grid is added.

Three independent behavioral tests **remain against the restored production
inverse**. They cover8,192 natural/scan-order blocks and524,288 scalar-oracle
samples: full-width coefficients/quantizers, word/DC boundaries, saturation,
low-support eligibility edges, canaries, unchanged inputs, zero allocation,
borrowed admission and cancellation. No test-only production entry survives.
One embedded-only rebuild per OS runs462 JPEG checks in normal/scalar/disabled-
AVX2/disabled-AVX512 modes. Main modules are unchanged; existing public/resize,
6,183 full/live and same actual Release-package proof are reused. The retained
new tests do not imply a fresh6,186-check Linux run.

Complete parsing, preparation, entropy, inverse, color normalization, independent
output and disposal remain timed. FFmpeg retains its documented contexts. Frozen
inputs/settings, original exact managed/native gates, one CPU, sequential Release
and idle-v2 apply. Instrumented elapsed is discarded. Compression exactness,
limits, cancellation, shared admission and surviving-output ownership stay intact.

[Evidence](jpeg-low-frequency-row-study-evidence.json) retains all stages, scoped
source reconstruction, comparisons, restoration and useful tests. Mirrored/member-
verified raw `jpeg-low-frequency-row-study.tar.xz`: **16,854,184 B /783 members**,
SHA256 `39d31b86fbeaa1432792df21400d410648b4892b7b9c01b1a59da70f1f5899a6`.
Originals/prototypes, modules, scripts, rejected actuals and input projections are
preserved; qualified dependencies retain the workers. Restored private0.2.1 package
SHA256 `933a4fb19a966bdccbac712c000ac1b58404c03c88301f4feb996afb4757d338` remains
unpublished. This study is not a renderer dependency or current timing replacement.
Future broad/package checks follow material performance reconciliation. Independent
Lanczos work resumes; all283 required paths remain in scope.
