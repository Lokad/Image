# Shared Deflate hash preparation: inconclusive

A one-second profile of the existing one-shot Optimal6 control places 689/770
samples in ProcessLazy, including its inlined helpers. This identifies parser
work; it does not measure hash costs or establish available savings. Profiled
timings are discarded.

Two bounded implementations reuse the exact three-byte hash across consecutive
insertions, recomputing it after parser jumps. Masking to fifteen bits drops the
oldest byte exactly. The first stores state in encoder fields; the revision keeps
it in parser locals, removing the extra 16 B/encoder. Dictionary order, matches,
ties and packets remain unchanged.

The revision's short Optimal point gain of 4.38% does not reproduce convincingly:

| Independent complete encode | Original → candidate µs | Candidate ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| RGBA8 Optimal6 | 24,462.89 → 24,209.66 | 1.291 / 1.314 | 1,325,732 |
| RGBA8 Fastest1 | 6,060.82 → 6,531.13 | 1.385 / 1.415 | 1,344,428 |

Optimal's raw 1.04% gain has overlapping raw/all/normalized confidence. Fastest's
raw 7.76% loss accompanies a 7.06% native clock shift; normalized intervals overlap.
Neither establishes a useful causal gain or a confirmed normalized regression.
**Restore both implementations; no favorable resampling.** The two existing
production controls retain confirmed CPU gaps and exact 210,973 / 236,828 B packets.
Current summary costs remain valid and unchanged.

Both comparisons include complete fresh one-shot preparation, encoding, independent
output ownership and release on each side. Existing PNG84 workers/harness/3- and
12-actual selectors are reused, four operations per actual. Ten scored jobs total;
the unchanged short baselines are reused across the two source versions.

Each candidate passes 575 internal checks normal/scalar on Windows/Linux and 69
public packet/admission checks on Linux. Frozen ring/overlap packets, distant
logical origins, input chunks, cancellation and terminal refusal remain exact.
After exact source/core/public-copy restoration, both platforms pass 575 affected
checks per mode. The actual Release package is byte-identical; reuse full/package
proof without another broad cycle. No storage or scratch survives the revision.

[Evidence](png-rolling-hash-inconclusive-evidence.json) retains every short and
independent result, both source designs and producer/output pins. Raw
png-rolling-hash-inconclusive.tar.xz: 8,076,648 B / 391 members, SHA256
2296e677e4a2961d0a8920eb3326ec2a067766f56cfa0cc4155f032dfd1d7e72,
mirrored/member-verified, including the original profile, all actuals/brackets,
guards and restored assemblies. Producer/qualification dependencies are explicit.
No permanent family prohibition follows. Publication remains postponed.
