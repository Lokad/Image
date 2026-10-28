# Predictable-content PNG compression

Development 0.2.1 fixes default Optimal PNG growth on predictable content without
changing samples, output limits or the fourteen declared reference packets.
The original 3000×3000 RGBA16 recipe is R=16x, G=16y, B=8(x+y), A=65535.
The qualified preceding build produced 29,375,876 bytes; its horizontal flip
exceeded the original 20 MiB allowance. All six current controls satisfy a strict
2 MiB complete-packet bound:

| Geometry | Complete PNG bytes | Every 16-bit sample |
| --- | ---: | --- |
| original | 511,771 | Exact |
| horizontal-flip | 573,041 | Exact |
| vertical-flip | 572,666 | Exact |
| clockwise90 | 572,691 | Exact |
| half-turn | 512,122 | Exact |
| counterclockwise90 | 572,491 | Exact |

A bounded content probe selects PNG byte prediction only for a strong residual
reduction. Per-row Sub/Up/Average/Paeth selection uses two additional admitted
packed rows. Stored mode and the declared asymmetric reference controls retain
their packet policy. No upstream implementation, fixture-specific switch,
lossy conversion or new compression option is involved.

An original BCL-Zlib/unfilter decoder checks every sample for all six geometries
and smaller 8/16-bit transparent/hidden-RGB controls. FFmpeg independently checks
a large moving-alpha variation under its separate original 20 MiB allowance;
that extra variation is not a 2 MiB compression claim. Sync/async equivalence,
terminal shared refusal before output, cancellation/refund/recovery and borrowed
stream/root ownership pass.

Each OS passes 5,063 full checks (1,992 internal, 2,123 public, 948 live), plus
602 internal and 217 public PNG/Zlib checks with intrinsics disabled. The same
actual Release package passes on Windows/Linux, including independent lossless
decoding of the packaged horizontal-flip regression and all three usage examples.
Published packages remain unchanged.

The four affected displayed PNG encoding costs are refreshed on the immutable
qualified Linux producer with cached harness, selectors and native workers:
Smallest8/16 are three-actual screens, Fastest8/stored16 independent twelve-actual
observations. All remain CPU gaps. Exact packets, frozen settings, complete
normalization/output ownership, native brackets and idle-v2 guards pass.
These are current costs, not paired speedup claims. Allocations and confidence
are in the [machine-readable evidence](png-predictable-compression-evidence.json)
and [summary](../docs/benchmark.md).

The new predictive-content path still needs its own CPU qualification; its size
gate does not establish reference byte identity or CPU parity. Other affected
PNG encoder costs need reconciliation before pass claims. Decoder/inspection,
other-codec, editing and text evidence remains applicable where paths are unchanged.

Mirrored/member-verified raw `png-predictable-compression-qualified.tar.xz`
(111 members), SHA256
`77c31a2bb9a86bf22aa377d05f0362314c746d8704260b6c3464fda67110c241`,
retains original recipes, six packets, source patch, both producers, baseline
and actual packages, tests, guards, receipts and reproduction scripts. One guard
refused to launch after a documentation-only source-manifest mismatch; its
failed evidence is retained. The manifest was reconciled before any timing.
