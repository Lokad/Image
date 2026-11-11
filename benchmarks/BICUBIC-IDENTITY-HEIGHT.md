# Exact identity-height bicubic execution

Height-preserving RGBA16 bicubic reduction saves **134.41 µs / 20.30% raw /
19.49% reference-normalized** against its source-valid predecessor; filtered,
all-actual and normalized intervals separate. Native time moves−1.01%.
The existing horizontal control passes/stops. The affected height-enlarging
companion remains a gap; no incremental gain is claimed for it.

| Existing complete public Resize, input+output+4KiB | Lokadµs | zimgµs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| TightHorizontal16 | 527.60 | 580.07 | 0.910 / 0.920 | 113 | pass |
| TightCross16 | 941.77 | 889.80 | 1.058 / 1.067 | 114 | gap |

Both use the frozen 513×259 RGBA16 input, width 256, heights 259/519. Managed
preparation, filtering/repair, output acquisition and disposal remain timed.
The explicitly labeled native zimg worker retains its graph and scratch; these
are existing prepared-reference boundaries, not fresh-graph equivalence.
Original exact managed hashes, full-word samples and native tolerances remain.
No new performance fixture/settings grid or production dependency is introduced.

For equal positive heights H≤2^26, `(y+.5)*H/H-.5` is exactly y: its product
is an exactly representable half-integer below 2^52. Original vertical weights
are +0,+0,1,+0,+0 and normalizer 1. One shared non-generic row/strip body uses
one unit tap and one cached row; general height retains five rows and its
original arithmetic. Original horizontal accumulation, certified paired/single
publication and exact repair are shared. Finite filtered values and the retained
zero-plus-unit multiply/add preserve zero canonicalization. Original repair
already skips zero weights. Larger heights retain original general geometry.
No reassociation, FMA, Q30/rational-policy change or separate direct-output kernel.

The result-backed workspace shrinks **91,168→58,400 B**, enabling 230 rather than
214 prefix rows. This is a smaller view of already-owned output, not a new
allocation saving. Explicit strip arrays shrink 3,440→2,352 B; the conservative
4 KiB admission remains. Output 530,432 B/1,062,912 B stays independently owned;
113/114 B/op are measured totals, not capacity. No extra owned/retained storage,
pool or owner. Scratch retires within the operation; default pool behavior and
governed bypass are unchanged. Physical/native/JIT peaks remain unmeasured.
Terminal refusal, direct cancellation, NoInlining stack retirement before exact
tiny-alpha replay and borrowed-root/source ownership remain intact.

## Bounded experiment and confidence

Variant A used compile-time value-type modes: primary 533.96/native 630.50 µs
passed, but companion 948.14/native 866.04 µs was 27.39 µs/2.97% slower than its
contemporary unchanged 920.75/native 870.21 µs predecessor. A was not accepted.
Variant B removes generic specialization while retaining one shared source body.
Three-actual screens are diagnostic; the table uses independent 12 actuals.

Final companion 941.77/native 889.80 µs is 21.02 µs/+2.28% above the earlier
predecessor, narrowly separated; native moves +2.25%, normalized points differ
only +.0304% with overlapping intervals. One adjacent unchanged predecessor is
921.54/native 826.98 µs with wide raw/all-actual/normalized intervals overlapping B.
This countercheck is **inconclusive**, not a speedup or regression-free claim:
the possible ~20 µs companion cost remains disclosed. No subject is repeated solely
for favorable bounds. Its independent current gap remains valid. All quietCPU0
idle-v2 receipts, actuals, filtered views and both native brackets are retained.

Only after reconciliation: **6,309 full/live checks per OS**, 400 internal/789 public
resize checks in normal/scalar/AVX2off/AVX512off modes, including the three named
storage suites. SAME actual Release-package consumer/all three examples pass
Windows/Linux, including both frozen word controls, refusal/cancellation/recovery
and independent survivors. Guard/geometry boundary tests and poison-prefix/strip
tests supplement existing rational/frozen/low-alpha checks. Other paths remain
source-valid; no extra text or codec qualification campaign.

Two production versions/OS; four embedded builds include two new-test corrections
(unclamped enlargement scale and direct-core success inputs), with failed logs
preserved. Zero public/benchmark/selector/native/fixture builds or production
compile failures. One broad/ISA/package milestone. Published releases unchanged;
private 0.2.1 package `4430c8fec3cc4eee40cf08c27b9d4c5784edd932e4eb9ceb9483c5b957f6cb20` contains timed DLL
`8a780eb3afde3254a26c51b8d027ae4a93702a85be010c7ef1d28ddb140fa7dd`; publication postponed.

[Evidence](bicubic-identity-height-evidence.json) pins current scope, storage,
source invalidation and qualification. Raw `bicubic-identity-height.tar.xz`: 4,576,020 B/
241 members, SHA256 `b3eb9212016559f03abf064170c3c35df26255d55895dcce32d1abf2f7e70790`, mirrored/member-verified.
Dependencies retain the original word-preparation controls and verified37ac
predecessor. Exact source/harness/native/fixture identities, failed/adverse and
inconclusive measurements and all reproduction scripts remain in those archives.
Stop the horizontal pass; investigate the named filtered-height gap independently.
Other kernels/default paths and all 289 required paths remain in scope.
