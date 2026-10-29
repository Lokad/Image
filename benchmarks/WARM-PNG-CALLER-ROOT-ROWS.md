# Avoid full-frame PNG scratch under a caller root

The existing two-retained-output control independently improves **3.13% elapsed**
and **36.37% allocated bytes**, with admitted peak **2,306,560 → 1,766,641 bytes**.
Accept under the predeclared allocation-saving exception: both raw 99.9% CI views
and conservative native-normalized envelopes separate. It remains a CPU gap.

| Complete operation | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Two retained PNG8 decodes, prior PCF1 | 3,525.226 | 1.237 / 1.244 | 2,574,629 | Prior baseline |
| Two retained PNG8 decodes, current | 3,414.989 | 1.201 / 1.204 | 1,638,183 | Confirmed gap |
| Stored RGBA16 decode, current | 222.536 | 0.753 / 0.758 | 8,568 | Confirmed pass; stop |

One selection guard chooses the existing bounded row-output decoder for eligible
noninterlaced RGBA8/16 with a caller memory root, before any related reservation.
This avoids an uncached full inflated frame. Existing local-fit/height checks,
portable paths and independent result owners remain; no new buffers or caches,
shared-capacity snapshots, retries or lower-memory fallbacks are introduced.

Two complete 513×259 decodes retain the first output throughout the second,
then release both. Managed admission is timed; FFmpeg has no matching governor.
Both use the frozen original packet, exact RGBA/hidden RGB and normal one-CPU
SIMD settings. Native output ownership/release remains equivalent. The root and
caller-input charge are prepared outside timing. Allocations are cumulative
managed B/op, distinct from admitted peak; native allocations are unmeasured.
Exact pixels, retained charges, source/root ownership, survivor independence,
terminal refusal/cancellation cleanup and subsequent success pass.

One short original/candidate pair supports independent confirmation, which reuses
the previously recorded original 12 actuals. All actuals, native brackets, filtered
and unfiltered bounds and idle-v2 guards remain. Stored16 is refreshed because the
shared method changed; this is a current cost, without a second gain claim. The
four Scan-only validators are unchanged and retain their PCF1 evidence.

All 5,082 tests per OS pass, including 948 live comparisons; 225 focused public
checks pass normal/scalar per OS. The SAME actual Release package and three usage
examples pass on both OSes. One core build per OS, zero harness/native/selector
builds, no new cases/tests. Published packages remain immutable. The remaining
51 named PNG costs still need current evidence; the full 268-path objective stays.

[Evidence](warm-png-caller-root-rows-evidence.json) and
[prior retained-root confirmation](CURRENT-PNG-RETAINED-ROOT-COST.md).
Mirrored/member-verified raw archive: png-caller-root-row-selection-qualified.tar.xz, 294 members,
SHA256 6579fb69b2a35dac45d749031b1893595ff7d6469ef06ba20af7bf1bd5ff381a.
