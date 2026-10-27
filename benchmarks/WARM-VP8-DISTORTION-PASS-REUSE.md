# Reuse the completed effort2 decision pass

These effort2 costs are historical after [selected-transform reuse](WARM-VP8-DISTORTION-TRANSFORM-REUSE.md).
The retained source/package/raw identities describe this recorded producer.

The existing effort2 diagram independently measures 5.83 ms: 2.314× reference
time (upper 2.325×), an exact 7,330-byte packet and observed 741,342 B/op.
Elapsed time is 25.76% below the preceding independent mean; these separate runs
are not a paired baseline gain. All 12 actual/filtered samples remain and both
idle-v2 guards pass. It was the largest confirmed gap for this producer.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| short | 5,929.82 | 2,515.68 | 2.357 / 2.689 | 741,342 | 7,330 |
| confirm | 5,826.82 | 2,518.06 | 2.314 / 2.325 | 741,342 | 7,330 |

Method 2 previously repeated a complete decision pass. It ignores initial mode
hints, searches UV modes afresh, and resets all spatial state between passes.
The statistics update changes coefficient probabilities/skipflags, while the
distortion-only search uses fixed mode prices. Quantizers, source and zero diffusion
are unchanged. Induction across reconstructed neighbors therefore gives identical
second-pass decisions. Reuse the already stored first complete result; retain
statistics/profile and final-filter publication. Methods 0/1 keep two passes;
RDO/trellis paths are untouched. No new cache, owner, admission or allocation.
Observed memory figures are reported literally, separately from capacity.

Both systems pass 4,945 Release checks (1,963 internal, 2,035 public, 947 live) and 624 scalar
WebP checks. A focused assumption test varies all eight luma/chroma hints on four
original blocks and compares full scores/levels/pixels/modes/errors. Existing
native statistics/final decision hashes and complete methods 0..6 packets remain
exact. The same actual private 0.2.0 package and all three usage examples pass both
systems. One production build per system was reused throughout; harness/native/
selectors and unaffected timing/qualification evidence carry forward.

The bounded pre-change effort2 trace and caller review are retained in
[evidence](warm-vp8-distortion-pass-reuse-evidence.json), with identities, samples
and a mirrored, member-verified raw archive. Instrumented times are discarded.
Complete normalization, fresh packet growth/acquisition and release remain timed;
packet/pixel/source/ownership gates and cancellation are unchanged. Stop pass-reuse
variants. Full 255-case warm parity and replacement readiness remain open.
