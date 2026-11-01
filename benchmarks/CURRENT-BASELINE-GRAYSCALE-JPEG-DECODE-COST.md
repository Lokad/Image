# Current baseline grayscale JPEG decode cost

Historical costs: affected JPEG boundaries are superseded by the
[current container-scan round](WARM-JPEG-CONTAINER-SCAN.md).
Original observations and raw evidence remain below.

The existing resident baseline grayscale control independently confirms
**829.713 µs**, **1.218× / upper 1.226** (lower 1.208), **672,196 B/op**.
This is a confirmed gap on the current qualified core, without a causal comparison
against older implementations. No production change, input or setting is added.

The original 513×259 one-component 8-bit JPEG remains **147,825 bytes**, SHA256
`a53c7643717578453750459a7ecc7edb84c4d95366bde811a4f82db242960859`.
Both subjects decode every pixel, expand grayscale, acquire independent complete
**531,468-byte RGBA8** output and release it. Native wrappers may be reused,
with state reset and every decode/conversion timed. Source/independent surviving
outputs, one codec/conversion CPU and both idle-v2 guards pass. Native allocation
is unmeasured; managed allocation includes harness costs. Startup/IPC/preparation/
validation are excluded. This differs from progressive-gray and caller-GRAY8.

Earlier independent costs precede direct baseline byteplanes (JDS1) and fused
DC/AC traversal (JBF1), both executed here. Their compiled twelve-iteration
selector is reusable; those older costs are not current proof. One current
twelve retains all actuals/native brackets on the immutable JFN1 build.
Zero builds, profiles, new rasters/settings or repeated full/package checks.

Initial setup stops with **zero actuals** at a stale derived managed-pixel hash.
Current Windows/Linux outputs, normal and intrinsics-disabled, match unchanged
FFmpeg normal/scalar pixels **byte for byte**: six outputs share SHA256
`b1069feb5a3ed55bce9a2d647686dbe6a1527865d4d22e86b9e14ebe241950ff`.
Only the catalog's managed hash is refreshed; packet, quantization, native pixels
and original maximum-4/RMSE-1.5 bounds are unchanged. Current errors are zero,
alpha is exact, and no successful timing is repeated.

The unchanged 5,331 checks/OS, same actual Release package/three examples and
separately qualified predictor fact remain: 5,332 applicable, no fresh full 5,332
claim. Package stays `228818d7e0026933b6cc800ee58dcece0603e945f7f46bcb02397a63184203f9`. Evidence covers this fixed
packet/boundary, without a general decoder or content claim. All 268 required
paths remain in the warm ≤1× objective.

[Evidence](current-baseline-grayscale-jpeg-decode-cost-evidence.json) pins actuals,
confidence, both catalogs, six-way pixel proof and qualification reuse.
Mirrored/member-verified raw `current-baseline-grayscale-jpeg-decode-cost.tar.xz` SHA256
`ee0f31776db0e5336af595c320f0de54588f009df283433bca7064b44149c7ff` (2,726,616 B / 80 members)
retains frozen input, cached runtime/selector/worker/probe, failed setup and
reproduction scripts, with JFN1/OAP1/original-selector dependencies retained.
