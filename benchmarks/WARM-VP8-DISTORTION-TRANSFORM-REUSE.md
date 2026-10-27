# Reuse four-block transforms in distortion-only reconstruction

The existing effort2 diagram independently measures 5.37 ms: 2.145× reference
time (upper 2.206×), an exact 7,330-byte packet and unchanged 741,342 B/op.
Elapsed time is 7.78% below the preceding independent mean; separate runs
are not a paired baseline gain. All 12 actual and 11 filtered samples remain in
the evidence, with conservative bounds and both idle-v2 guards passing.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| short | 5,459.32 | 2,516.33 | 2.170 / 2.295 | 741,342 | 7,330 |
| confirm | 5,373.40 | 2,504.79 | 2.145 / 2.206 | 741,342 | 7,330 |

Selected-mode reconstruction now uses the existing exact `DctFourBlocks` rather
than repeating scalar residual gathering and transform setup. Four luma batches
preserve block/DC order; two chroma batches preserve the interleaved U/V layout
through the helper's existing disjoint coefficient-pair offsets. Walsh, quantizers,
mode selection and reconstructed pixels are unchanged. The helper retains its
portable fallback. This adds no kernel, owner, cache or allocated capacity.

Both systems pass 4,945 Release checks (1,963 internal, 2,035 public, 947 live),
624 scalar WebP checks and 48 focused checks. Existing native method0/1/2
statistics/final-decision hashes and complete packets remain exact. The same
actual private 0.2.0 package and all three usage examples pass both systems.
One immutable production build per system and the existing harness/reference/
selectors were reused. Complete normalization, packet ownership and release remain
timed. Shared admission, cancellation and output independence remain required.

The [evidence](warm-vp8-distortion-transform-reuse-evidence.json) retains identities,
samples, qualification and a mirrored, member-verified raw archive. The earlier
method2 trace supplied the caller lead; no new profile was needed. Only the
distortion-only methods0/1/2 reconstruction changes. The five unchanged RDO,
trellis and opaque16 anchors stay valid; no redundant timing confirmation.

Stop selected-transform variants. This remains the largest confirmed gap;
full 255-path warm parity and replacement readiness are open.
