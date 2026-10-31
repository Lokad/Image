# Cancelable PNG cursor reassessment: rejected

One current short saves **3.38% filtered/all-actual / 3.49% normalized**, below
the 4.1% continuation scale. Restore without confirmation or variants.

| Existing complete RGBA8 decode, live token | Managed µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Current original, twelve actuals | 1,607.109 | 1.150 / 1.182 | 4,472 |
| Rejected candidate, three actuals | 1,552.733 | 1.110 / 1.593 | 4,472 |

The [earlier cursor trial](PNG-CANCELABLE-MATERIALIZED-CURSOR-REJECTED.md) saved
4.34%/4.20% in its short, missing the former 5% cutoff. Its original framing
source is still byte-identical. The revised triage justified one reassessment
of that exact candidate. Shared GFNI checksum changes invalidate the older
complete-operation cost, so an independent current baseline is measured first.
It remains a gap; the new short does not reproduce a qualifying gain.

Both subjects retain the original 513×259 packet, pixels, native settings,
normalization/owned-output/disposal boundaries, live token, admission, cancellation
and ownership gates. All actuals, native brackets and both idle-v2 guards pass.
The candidate reuses the existing local cursor in bounded cancellation slabs and
removes duplicate token handling. No storage, owner, admission or API is added.

Candidate 130 internal/278 public checks pass normal/scalar on each OS.
Original source/core/PDB/XML/linked tests are restored byte-exactly; 122 internal/
278 public restored checks pass in the same modes. Eight trial-only tests are
removed. One core/internal build per OS; no public/harness/native/selector build,
profile, successful-score repeat, full-suite or package repeat. Unchanged
**5,342 checks per OS** and the SAME actual Release package/three examples remain.

The independent original result replaces the stale CPU observation for this
already named case. No new workload/grid, causal historical speed claim or parity
claim is added. Close this cursor family under the current acceptance threshold;
the **268-path warm conservative-upper ≤1× objective remains open**.

[Numeric evidence](png-cancelable-cursor-reassessment-evidence.json) retains
actuals, confidence, producers, gates, precise scope and restoration.
Mirrored/member-verified raw png-cancelable-cursor-reassessment.tar.xz, SHA256
af1d211a5fbb643876749f13e5ddc69eccebaeb1f3d5c2b3266ffe0d166b5161 (3,936,120 B / 112 members);
WPC31/PMC2/APC31 dependencies remain retained.
