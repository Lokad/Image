# Block Adler sums for PNG

Encoder costs are superseded by [wide chunk CRC](PNG-WIDE-CHUNK-CRC.md).
The unchanged read/validation and shared-root paths retain this evidence.

The shared checksum now accumulates SIMD byte sums, local weights and prefixes
through each existing 4 KiB block, then reduces lanes once. The recurrence is exact:
for 32-byte group sums s and weighted sums t, a += sum(s),
b += 32*m*a_initial + sum(t) + 32*sum(previous group sums).
At most 128 groups keep weighted int lanes <= 3,982,080, SAD sum/prefix lanes
<= 261,120/16,581,120 and the complete b <= 2,408,052,720. Scalar tails, modulo,
entry/block cancellation and final state publication remain verbatim. There is
no new owner, array, pool, cache, API, dependency or numerical tolerance.

An actual-package [profile](png-block-adler-evidence.json) located checksums;
918 locations included 459 mapped managed, 249 CRC and 122 Adler. Inlining/skid/runtime/
GC/batching prevent phase-cost or removable-percentage claims. The current CRC
already uses GFNI/parallel carryless folds; it is unchanged. No profile repeats.

| Existing complete operation | Lokad / FFmpeg µs | Ratio / conservative upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Stored RGBA8, native retained / post-GC4 | 79.34 / 189.21 | 0.419 / 0.463 | 609,708 | pass |
| Stored RGBA16, native retained / post-GC4 | 475.21 / 404.55 | 1.175 / 1.194 | 1,147,452 | gap |
| Fresh stored RGBA8 | 79.17 / 541.04 | 0.146 / 0.161 | 609,708 | pass |
| Fresh stored RGBA16 | 470.06 / 1,384.28 | 0.340 / 0.348 | 1,147,452 | pass |
| DecodeCancelable16 | 2,859.45 / 2,902.44 | 0.985 / 0.988 | 8,568 | pass |
| DecodeCancelable8 | 1,499.29 / 1,505.44 | 0.996 / 1.010 | 4,472 | uncertain |
| DecodeTightCancelable16 | 3,054.27 / 3,092.17 | 0.988 / 1.008 | 70,272 | uncertain |

Current-baseline word 494.91→475.21 µs saves 19.70 µs / 3.98% filtered and
18.45 µs / 3.73% all-actual, both raw confidence intervals separated. Native moves
−0.18%; normalized point gain 3.81% has overlapping confidence. Byte 86.06→79.34 µs
has 6.72 µs / 7.81% filtered gain, while all-actual/normalized intervals overlap.
Live-word filtered 23.88 µs improvement is similarly scoped; its standalone native
upper now passes. These are not uniform gains or proof of normalized primary
improvement. Preserve the screen and every baseline/candidate actual, including
outliers and slower companion captures. No unchanged timing is repeated for a pass.

All 51 reads now have 49 passes/two uncertain; live8 and tight cancelable16 remain open.
The 17 fresh-state encodes now have 14 passes/one gap/two uncertain: Gray8 gap and
Fastest8/ordinary RGBA8 uncertainties. Both gray-alpha controls pass. These changed
paths supersede older CPU costs; no general PNG/289-path parity claim follows.
Three historically slower reads get one contemporary baseline each: Average16,
Gray1 and GrayAlphaPaeth8. None confirms separated normalized adversity. Paeth8
raw rises 182.50 µs alongside native +8.62%; normalized confidence overlaps.
Its adverse filtered/all-actual evidence remains. Predictable16 also has a slower
raw point than the preceding stage, without separated normalized adversity.
The complete current costs and all clock/confidence effects are in the evidence.

All 20 encoder views retain exact complete reference packets, including 533,444/
1,066,552 B stored and 573,041 B predictable packets. The two retained-native stored
views and predictable control keep their original lifetime boundaries. Equivalent
fresh-state stored operations include native codec/options/zlib/packet creation,
normalization, independent output and disposal; word also allocates/converts/
releases its frame. Managed complete preparation/output ownership stays timed.
Natural-word 64-operation pairs cost 346.69/442.42 µs,
upper 0.810, 1,147,393.875 direct B/op and pass only that
scope. Do not sum/subtract historical means to infer phases. CPU0, one thread,
warm optimized JIT, workstation nonconcurrent GC, idle-v2 guards, original inputs,
settings and native identities remain. No workload/image/quality grid is added.

Total allocation, governed capacity and retention remain distinct. Matching
counter boundaries retain previous allocations; word counters differ by 408 B in
some captures and are not an allocation-saving claim. No substantial owned or
retained storage is added; transient vector state is not a measured JIT frame.
Existing 4 KiB codec allowance, 72 KiB stored / 384 KiB nonzero deflater admission and
zero operation-scratch retention remain. Output capacity, borrowed input and
independent surviving images/streams are unchanged. Physical/native peaks are
unmeasured. [IDAT ownership](PNG-IDAT-FORWARDING.md) and its storage gains remain.

One additional scalar-oracle test covers 8,256 vector-count/unaligned-tail cases,
max/zero seeds, original/all-255 bytes and source guards. Existing split/cancel/
modulo/format/ownership tests are reused. Windows/Linux each pass 6,510 full/live
checks (3,181 internal + 2,381 public + 948 live),703 internal / 246 public PNG/admission
cases per normal/scalar/AVX512-off/AVX2-off mode, then same actual Release-package
consumer/all three usage examples. Other family/resize ISA and three named storage
suites reuse source-valid proof. One core/embedded build per OS, no public/harness/
native/fixture builds, one accepted full/package milestone. A baseline receipt's
stale metadata path is fixed after its original 12 actuals finish; raw data is
recovered, never rerun. Published releases are unchanged; publication postponed.

[Current read costs](PNG-CURRENT-FRAMING-COSTS.md) and [evidence](png-block-adler-evidence.json)
pin packets/sources/modules, comparison boundaries, all actuals and qualification.
Raw `png-block-adler-qualified.tar.xz`, SHA256`f6134fcb8e0782ca33767d5899f7b4f9ddd8660796ae270d25ea7db81ea4dea4` (3,277,484B / 665 members),
is mirrored/member-verified. Parent archives preserve earlier ownership,
framing, live decoding and original attribution. All 289 required paths remain.
