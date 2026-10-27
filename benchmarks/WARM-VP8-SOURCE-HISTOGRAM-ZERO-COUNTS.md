# Combine zero-bucket population updates

The existing effort2 diagram independently measures 5.095 ms: 2.020× reference
time (upper 2.028×), an exact 7,330-byte packet and unchanged 741,342 B/op.
Elapsed time is 5.18% below the preceding independent mean; separate runs
are not a paired baseline gain. All 12 actual and 11 filtered samples remain in
the evidence. Both idle-v2 guards pass for every row; the five other controls are
current shorts.

| Existing quiet one-CPU control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| DiagramLossyDistortion8 short | 5,147.66 | 2,519.53 | 2.043 / 2.163 | 741,342 | 7,330 |
| DiagramLossyDistortion8 confirm | 5,095.24 | 2,522.61 | 2.020 / 2.028 | 741,342 | 7,330 |
| PhotographLossy8 short | 17,641.56 | 9,714.43 | 1.816 / 1.885 | 809,056 | 17,688 |
| DiagramLossy8 short | 9,870.29 | 5,812.36 | 1.698 / 1.752 | 741,121 | 7,100 |
| DiagramLossyTrellis8 short | 17,208.25 | 9,823.48 | 1.752 / 1.828 | 754,328 | 6,818 |
| vp8 short | 76,733.96 | 53,568.38 | 1.432 / 1.501 | 23,258,168 | 98,590 |
| EncodeOpaque16 short | 26,839.08 | 15,604.39 | 1.720 / 1.796 | 1,201,536 | 72,380 |

Source analysis retains the same magnitude/bucket generation. Combine repeated
zero-bin writes, skip fully zero blocks, and visit only nonzero lanes in mixed
blocks when at least half the coefficients are in the zero bin. Denser blocks
keep their serial updates. Two comparison bits represent each short, so
population counts and signed integer wraparound remain exact.
Aliased buffers retain the original scalar loop and sequential feedback. No new
kernel family, heap storage, cache, owner, admission or cancellation boundary.

Both systems pass 4,946 Release checks (1,964 internal, 2,035 public, 947 live),
625 scalar WebP checks and 49 focused checks. One new fact covers all 65,536
zero-lane masks, signed bins, wraparound and tails; existing alias, signed-extreme,
native source/decision and complete-packet oracles remain exact. The same actual
private 0.2.0 package and all three usage examples pass both systems. Two bounded
candidates each used one immutable production/linked-test build per system. The
corrected build was reused through confirmation and qualification.

The [evidence](warm-vp8-source-histogram-zero-counts-evidence.json) retains current
pre-change attribution, identities, observations and a mirrored/member-verified
raw archive. The first candidate slowed the RGBA16 control to 27.94 ms; one
fresh cached-baseline replay reproduced 26.84 ms. The density guard restores
26.84 ms without changing that workload or retiming its baseline again. No full/
package proof was run for the rejected candidate. Sampling selected the caller
lead; instrumented times are discarded.
Complete normalization, fresh output ownership and release remain timed. Six
named VP8 anchors replace earlier costs; unrelated decoder/codec/edit/text paths
keep valid evidence. Shorts establish neither parity nor independent speedups.

Stop aggregation variants. The 2.020× effort2 gap, full 255-path warm parity and
replacement readiness remain open.
