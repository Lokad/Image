# Bounded JPEG confidence audit

Six previously uncertain controls receive one independent twelve each on the
same qualified DLL. **Two become confirmed gaps; four remain confidence-open.**
None passes. This updates current costs without a code change or causal gain claim.

| Existing complete 513×259 operation | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Status |
|---|---:|---:|---:|---:|---|
| NativeDecode420 | 1,062.27 | 1,017.29 | 1.044 / 1.078 | 757,362 | confirmed-gap |
| NativeDecode422 | 1,372.71 | 1,352.22 | 1.015 / 1.040 | 829,176 | confirmed-confidence-open |
| OwnedDecodeGray | 722.46 | 690.30 | 1.047 / 1.096 | 672,202 | confirmed-confidence-open |
| CmykCaller | 335.68 | 336.82 | 0.997 / 1.032 | 1,085,963 | confirmed-confidence-open |
| CallerGray | 634.72 | 652.91 | 0.972 / 1.008 | 273,430 | confirmed-confidence-open |
| ValidateJpegProgressive420 | 283.29 | 268.80 | 1.054 / 1.080 | 4,072 | confirmed-gap |

NativeDecode420 lower bound is 1.017; ValidateJpegProgressive420 lower is 1.035.
The other four intervals still cross 1: do not assume optimization is needed or
claim parity from their point means. Stop this bounded audit without another
measurement to chase significance. Earlier observations remain retained; the
latest six costs were selected before measurement, regardless of outcome.

Frozen inputs/settings, exact managed/native pixels or descriptors, caller padding,
source/output ownership, single-CPU execution, native brackets and idle-v2 gates
pass. Complete operations include output acquisition and release. Native allocation
is unmeasured. Already-qualified temporary metadata projections are restored
byte-exactly. Zero builds, new inputs/settings, profiles or production changes.

The current entropy-reader source matches the original retained JCG32 source
byte-for-byte, so its expensive-path attribution remains useful without another
profile. Prior mask-construction and rejected token/headroom/cached-bit evidence
does not justify reopening those small or closed trials.

Applicable 5,369 checks/OS, 948 live comparisons and the same actual Release package/
three examples remain valid; no full/package repeat. Unrelated current costs and
the accepted container-scan gain remain. All 268 required paths stay in scope;
full warm parity and replacement readiness remain incomplete.

[Evidence](current-jpeg-confidence-evidence.json) and
[qualified scan round](WARM-JPEG-CONTAINER-SCAN.md). Mirrored/member-verified
current-jpeg-confidence-resolution.tar.xz, 2,955,836 B/89 members,
SHA256 1c70d2b00d7cf60d904bcfb9b655872a738d73ff3dd11a0f136048b4089eac43.
Raw retains every sample, bracket, guard, receipt, actual immutable binaries,
selectors/workers, scripts, metadata restoration and pinned raw dependencies.
