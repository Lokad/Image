# Bounded wide horizontal reuse

[Current adjacent confidence](BICUBIC-CURRENT-ADJACENT-CONFIDENCE.md) supersedes
the standalone parity uncertainty below: the same bounded byte-wide operation
passes on the current immutable package DLL. The earlier scoped DLL gain and
all adverse/uncertain data remain distinct; this is not a new production gain.

The existing complete 513×259 RGBA8 → 128×64 bicubic control saves **3.29%**
in one counterbalanced immutable-DLL comparison: **385.00 → 372.34 µs**, ratio
confidence **.9615–.9728**. All twelve alternating pairs of sixteen complete
public operations are retained. This establishes a scoped incremental gain.

Standalone current cost is **360.12 / zimg 348.02 µs**, upper **1.041**:
parity remains **uncertain**. Independent raw saving is 4.87%, but the native
clock moves −8.23%; normalized intervals overlap. The contemporary baseline
is also uncertain at 378.55 / native 379.22 µs, upper 1.004. The paired comparison
resolves the incremental effect without replacing standalone costs or proving
native parity. No favorable repeat is taken. Earlier adverse and inconclusive
captures remain in their linked reports and raw archives.

Four eligible sixteen-tap windows reuse their 28-pixel union instead of 64 logical
reads. The operation-owned row cache receives the same ordered multiply/add
results; there is no FMA, precision or pixel-policy change. Pair/indexed/clamped,
odd and 17-tap paths remain. The byte row-loan caller grows 6,539→7,211 native bytes;
the 723-byte leaf has no vector stack transfers. Logical read counts do not
establish cache-miss or elapsed-time savings. Five other closed specializations
have identical instructions after verified process-cookie/relocation normalization,
so their source-valid costs are reused.

The complete managed Resize still includes preparation, admission, filtering,
exact repair, independent output and disposal. Its working limit is input+output
+4,096 B. The labeled FFmpeg zimg backend retains graph/input/scratch preparation;
deinterleaving, 8-pixel halo, filtering, unpremultiplication, quantization, independent
output and release remain timed. This is an asymmetric diagnostic; separately
qualified equivalent one-shot/prepared controls remain unchanged and pass.
Exact managed pixels and the approved native bounds remain intact.

Allocation is unchanged: **113 B/op** in the original harness, **112 B/op** in
the paired bridge. Source 531,468 B, output 32,768 B and 4,096 B admitted workspace are
unchanged. Cache/weights borrow 10,880 B of unmaterialized output; explicit row-loan
stack payload remains 3,296 B. No new owned/admitted capacity or retained operation
scratch is introduced. The unchanged two-slot/4 MiB idle store is bypassed for
caller-governed storage. Physical/native/JIT peaks remain unmeasured.

**6,592 full/live checks per OS** pass: 3,241 internal / 2,403 public / 948 live.
Focused 525 internal / 730 public checks pass normal/scalar, plus 5/22 with AVX512
disabled. All three named resize-storage suites are included. Five additional
checks cover ordered scalar bits, poisoned boundaries, zero allocation, cancellation
and independent exact-rational tiny-alpha frames at the original and 307×400
geometries. Shared refusal, cleanup and surviving owners pass. Encoder packets,
decoder pixels and meaningful editing coverage remain correct.

The **same actual Release package**, containing the timed Linux DLL, passes both
OS consumers and all three examples. There is one core build per OS, no public
test/native/fixture/original-harness rebuild, and one Release/--no-build pack.
Linux embedded tests rebuild once more to correct the new low-alpha test oracle's
missing scratch allowance before timing; failed setup logs are retained. Initial
interleaved diagnostic JIT and unnormalized process-cookie comparisons are retained;
serial captures are reused for the final proof. No production rebuild follows timing.
Publication is postponed; published packages remain immutable.

The 191 current owner views have 166 passes / 8 gaps / 17 uncertainties; the 73 resize
views have 67 passes / 6 uncertainties. All 289 material paths remain required. This stage makes
no wider shape/content, whole-library or native-parity claim.

[Evidence](bounded-wide-horizontal-quartet-evidence.json) records immutable identities,
boundaries, confidence, ownership, qualification and reproduction. Raw
`bounded-wide-horizontal-quartet-qualified.tar.xz`, SHA256
`72d4b2d414718a061e3fa6e5a611a886a206a6721204bb8283acf924eb8a174d` (2,852,768 B / 155 members), is mirrored
and member-verified. Dependencies retain the preceding qualified package and the
[earlier authored cached quartet](CACHED-WIDE-CROSS-PAIR-LOADS-EXPERIMENT.md).
Its old 40→28 logical-read experiment was inconclusive; this 64→28 caller is distinct.
