# Paired word bicubic weights

Bounded word32×16 resizing now constructs the same paired weight vector with
two broadcasts and upper-half insertion. It replaces repeated eight-scalar
construction; every lane bit stays identical. No new floating arithmetic,
coefficient/product/add order, source decoding, cache, geometry, normalization,
certificate, repair or ownership changes. The entire old DeepWord file
reconstructs after one constructor-expression substitution.

| Complete513×259RGBA16→32×16, extra4KiB | Before / current µs | Prepared zimg µs | Current ratio / range | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Public Resize | 553.89 / 441.45 | 414.55 | 1.065 / .999–1.072 | 113 |

Independent confirmation saves **112.44µs/20.30%raw/14.21%normalized**.
Filtered/all-actual raw and normalized confidence intervals separate. Native
clock moves−7.09%; raw and normalized effects stay distinct. These are separate
source-valid captures, not paired-DLL evidence. Original three-actual screen
and all twelve independent actuals remain. Current parity is **uncertain**;
do not presume a residual gap, repeat unchanged captures or tune for a pass.
The113B/op versus earlier114B/op is not an allocation-design claim.

The [actual-package profile](DEEP-WORD-CURRENT-ATTRIBUTION.md) identifies horizontal
unsigned decoding/filtering and weight construction. Original JIT3050B becomes
2989B; paired weights use two broadcasts and insertion. Code size/instruction
shape are diagnostics, not component times or predicted gains. The488B publisher
and source arithmetic remain unchanged; instrumented profile elapsed is discarded.

Every managed call includes complete public preparation/filtering/exact repair,
admission, independent output and disposal. Explicitly labeled zimg retains
graph/scratch and times complete input conversion/halo/filtering/quantization/
independent output. Not a fresh-native-graph or FFmpeg CLI comparison. Original
input/settings, exact rational/frozen samples, approved native-only bounds,
immutability, independent surviving owners and idle-v2 gates pass. Default
word32×16 uses the earlier unchanged cached workspace; only this existing bounded
control changes. No new image/settings matrix or numerical policy.

Explicit arrays remain4032B under4096B preadmission, no new owned/retained
capacity, buffer, cache, pool, session or API. Physical/JIT/native peaks remain
unmeasured. Refusal/cancellation and exact-repair cleanup preserve their original
boundaries. Existing full-word/rational/half/edge/storage/admission coverage is
reused; no test mirrors a constructor substitution. EachOS builds core/embedded
once, with no public/frozen-harness/native/fixture build and onepack.

Windows/Linux each pass **6,480 full/live checks** (3151internal/2381public/
948live),552internal/791public per normal/scalar resize/admission mode, including
all three named storage suites. Unchanged disabled-AVX512/AVX2 fallback proof is
reused after the same guard. SAME actual Release-package consumer/all three
usage examples pass bothOSes. Other72resize costs are source-valid:67base,
two current byte-Cross and three generated-byte controls. Across73standalone
controls58pass/8gaps/7uncertain; scoped adjacent views remain separate.

[Evidence](deep-word-vector-weights-evidence.json) retains source reversal,
all actuals/native clocks/JIT and qualification. Raw `deep-word-vector-weights-qualified.tar.xz`, SHA256
`6df91d8bd8387ac86bcc8892090779a5607c0ef479f3338d44ee6a47040837db`
(891,220B/77members), is mirrored/member-verified with
retained dependencies. The 455,565B development package0.2.1 has SHA256
`3bfbee9767acc1e2d50527f33ec4810ff6ff6311060ba6c22a29e6c806cfe4c2`. Published releases remain immutable; publication is postponed.

Next demonstrated excesses are byte Cross81.81µs and word2×9about75µs. The latter
still repeats coefficient and source-row preparation removed by the accepted
byte row-group design. Profile its actual current path before choosing shared
execution/decoder specialization; stop unaffected passing controls. All289paths
remain required; this is not a release-wide performance refresh.
