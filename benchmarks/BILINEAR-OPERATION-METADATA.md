# Bilinear operation metadata preparation

The existing byte/word reduction kernels now capture immutable source/result
strides and the private destination view once per operation. Row slicing reuses
those values. Geometry, tap arithmetic/order, normalization, certificates, original
repair, cancellation, arrays and admission remain unchanged. No new kernel,
owner, pool, cache, API or numerical policy is added.

| Existing complete 513×259 → 256×129 resize | Baseline → current µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 | 393.45 → 398.01 | 414.98 | 0.959 / 0.966 | 8,473 |
| RGBA16 | 436.94 → 386.50 | 425.37 | 0.909 / 0.949 | 8,473 |

Word saves **50.44 µs / 11.55% raw / 12.31% normalized**; filtered/all-actual
and normalized intervals separate. All-actual gain is 49.51 µs / 11.31%.
Native time moves +0.87%. Byte costs **4.55 µs / 1.16% more raw** (all-actual
4.04 µs / 1.02%); raw intervals separate, while normalized intervals overlap.
Both current controls pass. This is a retained word improvement with a disclosed
byte tradeoff, not a uniform speedup. The contemporary byte baseline already
passes; its historical parity gap is not attributed solely to this code.
The initial byte screen is faster, but its gain is not reproduced in independent
confirmation. Keep the screen, every actual and all native brackets; no favorable
repeats or historical phase sums.

Both sides retain the original complete boundaries: managed preparation, owned
output, filtering/repair/disposal versus FFmpeg's retained scaler context and
complete normalization/output ownership. The actual immutable package DLL is timed
on the quiet VM, CPU0, one thread, optimized JIT, workstation nonconcurrent GC and
idle-v2 guards. Reuse the original short jobs: 12-actual screen, independent
12-actual baseline/candidate captures per depth. No image/settings grid is added.

All managed rational samples and approved native RGB/alpha gates pass. Input
immutability, independent surviving output, refusal/cancellation cleanup and shared
admission remain. Image shape is immutable, and Resize allocates/admits a private
result before dispatch, publishes only success and disposes failures. The cached
view cannot escape or outlive its owner; asynchronous storage is unaffected.

Total allocations remain 8,473 B/op. Geometry capacity/admission remains 8,512 B
plus the existing 4 KiB strip allowance, 12,608 B total; declared fixed arrays
occupy 3,560 B. No owned/retained image capacity is added, and operation-scratch
retention stays zero. The existing default image pool is unchanged; governed calls
keep explicit admitted ownership. Physical JIT/native peaks are unmeasured.

Whole original sibling sources reconstruct after undoing only metadata/view
substitutions and a stale comment correction. The actual 9,388-byte shared Resize
caller is identical after address/method-number normalization in both standalone
confirmations. The other 71 resize controls and unrelated codec/text/drawing costs
reuse source-valid evidence. Overall resize status is now 63 pass / two gaps /
eight uncertain across 73 existing controls. TightDown8 and Wide8 remain the named
gaps; stop passing reductions.

After affected costs reconcile, Windows/Linux each pass 6,511 full/live checks
(3,182 internal + 2,381 public + 948 live), 569 internal/791 public resize checks
per normal/scalar mode including all three named storage suites, then the same
actual Release package/all three usage examples. Its existing byte/word bilinear
consumer compares the accelerated path to bounded exact replay through a shared
budget and verifies cleanup. Disabled-ISA paths reuse evidence: both changed
kernels return before the new locals when AVX512F or AVX2 is disabled. One core/
embedded build per OS; public/harness/native/fixtures reused, one package milestone.

[Evidence](bilinear-operation-metadata-evidence.json) pins current costs, all actuals,
source/JIT/lifetime proofs and qualification. Raw `bilinear-operation-metadata-qualified.tar.xz`,
SHA256 `340452c6a0aad4a80d0462fdfd2cfe0d6f75e3a0c7340edd7085679812c41850` (2,956,104 B / 116 members), is mirrored
and member-verified. The original 1,995-location attribution is a lead, not a phase
cost or gain forecast; its separate archive and parent resize evidence remain.
Published packages are immutable; publication is postponed. All 289 required
paths remain in scope.
