# Vector normalization and certified bilinear publication

[Subsequent same-package adjacent measurement](WARM-ADJACENT-CONFIDENCE.md)
owns the current Up16 cost. This stage retains its independently confirmed gain.

BVC804 keeps RGBA16 enlargement's alpha normalization and certification in vector
lanes. One Normalize helper and one combined Classify helper replace repeated
scalar extraction/vector construction, margin-lane insertion and four tiny-alpha
branches. The same alpha permutation serves margin construction and classification.
Direct full-width repair-mask extraction completes that boundary. No new owner,
cache, pool, generic execution abstraction or public API.

Independent complete-operation confirmation saves **769.82 µs: 27.67% raw /
18.12% reference-normalized**. Filtered/all-actual raw and conservative normalized
confidence intervals separate. Native time also moves −11.66%, so the raw saving
contains clock movement. These views remain distinct.

| Existing 513×259→1027×519 RGBA16 Resize | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Contemporary predecessor confirmation | 2,782.52 | 2,214.83 | 1.256 / 1.266 | 4,498,923 | Gap |
| Vector normalization/classification | 2,012.69 | 1,956.52 | 1.029 / 1.038 | 4,498,935 | Uncertain |

**FFmpeg parity remains uncertain**, conservative range .919–1.038. No pass or
confirmed remaining-gap claim. The first screens were 2556.23/native2037.08 µs
and 2056.93/native2127.92 µs; both complete independent confirmations follow on
unchanged builds, in reversed order. All twelve actuals and native brackets remain.
No unchanged favorable repeat follows. The earlier one-expression mask trial
was inconclusive and restored; this redesign changes a larger dataflow boundary.

Ordered weighting, Int64 accumulation, division, reciprocal/margin arithmetic,
rounding, packing and original/tiny-alpha replay remain exact. Lane permutation
and blending select the original divisor/margin bits. Broadcast ordered tiny-alpha
comparison publishes the same four repair bits, including transparent NaN handling.
This changes no numerical policy. Whole-source reverse proof isolates only the
normalization/classification prelude and helpers; row scheduling/cache, portable
dispatch, cancellation, admission and ownership remain verbatim.

Two focused actual-helper tests cover 4,096 normalization lanes (non-NaN bits exact)
and 25,920 scalar certificate lane decisions at tiny-alpha, half/adjacent and ordered
NaN boundaries. Existing exact rational/qualified full frames, odd/partial/edge/
low-alpha cases, source immutability, refusal/cancellation/recovery and independently
surviving owners pass. Normal hardware runs execute the new SIMD helpers; disabled
modes retain whole-operation portable coverage. Actual-module JIT inspection
retains exact output and shows both helpers inlined: main 2,822→2,487 B. This code
observation is separate from timing proof; instrumented elapsed values are discarded.

Managed timing includes preparation/admission, source processing, exact complete
filtering/repair, independent output acquisition and disposal. FFmpeg retains its
public triangle graph/input wrappers/geometry contexts; conversion, filtering,
normalization and output acquisition/release stay timed. This is not equivalent
fresh-graph work. Frozen input/settings/output hashes and native RGB/alpha bounds,
normal/scalar native CLI, independent owners and quiet CPU0 idle-v2 guards pass.

Total GC is **4,498,935 B/op**, including the 4,264,104 B result. The 12 B aggregate
counter difference is reported, without an allocation-saving or new-capacity claim.
Existing admitted geometry 102,892 B and row cache 131,520 B stay unchanged; these
source-based capacities are not measured physical peaks. Output ownership lasts
until disposal; operation scratch retention is zero. Existing default pixel pooling
and governed bypass remain. No fixture-sized retention increase. Physical/native/
JIT-stack peaks are unmeasured.

Windows/Linux each pass **6,253 full/live checks**: 2,941 internal, 2,364 public,
948 live. Resize 365 internal/779 public pass normal/scalar/AVX512off/AVX2off,
including all three named storage suites. Unchanged codec/text ISA evidence and
the other three bilinear costs are reused. One successful main/embedded build per
OS serves the stage; no public/harness/native/selector/fixture rebuild. An initial
main compile failed on an intrinsic overload and was fixed before tests/timing.
Affected performance reconciliation precedes broad and one actual-package round.
The SAME Release package and all three usage examples pass both OSes.

Private 0.2.1 package `f22ab4015ca38494228059978b8fa7e0899f7786ae5fec485cb7c52c9976f98f` (448,505 B), DLL
`6ef75b8185e03309b29dacbe9cf50e757e3b49336e996c60078b756970e4a272`; Linux qualified DLL
`29ca7c976cdba35357c7f1cb779936decb36cfbb5189169a5ec1263cb894cd81`. Published releases are unchanged;
publication postponed. This incremental stage is not a whole-release refresh.

[Evidence](bilinear-vector-certification-evidence.json) pins actuals, exact source
proof, capacities, current confirmation and qualification. Mirrored/member-verified
raw `bilinear-vector-certification.tar.xz`: 8,419,784 B / 131 members,
SHA256 `8cddc33d352357b64ee0c493182746fc78cd6e276aa951387a6dff8d5681b6a5`. Its pinned dependencies retain original fixtures,
selectors/reference tools, [accepted reduction preparation](BILINEAR-SHARED-PREPARATION.md)
and [attribution/inconclusive mask history](BILINEAR-UP16-CURRENT-ATTRIBUTION.md).
All 289 required paths remain; continue consequential uncertainty resolution or
independent demonstrated codec/resize gaps, without retuning a presumed Up16 gap.
