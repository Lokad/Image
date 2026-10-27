# Bounded VP8 squared-metric screen

The later [distortion-only search measurement](WARM-VP8-DISTORTION-SQUARED.md)
exposed a larger gap and justified adopting this exact candidate on that distinct
path. The RDO-only decision below remains its historical screening result.

The same photograph measures 17.95 ms in one short: 1.848× reference time
(upper 1.969×), 809,056 B/op and an exact 17,688-byte packet. Its 1.34% elapsed
difference from the preceding independent mean is within uncertainty. The SIMD
candidate is rejected without another timing run or further metric variants.

Candidate checks pass on both systems. The accepted source and actual Windows/
Linux cores and PDBs are restored byte exactly; only linked tests are rebuilt.
Both systems then pass 53 focused checks normally and with intrinsics disabled.
One useful contract test remains: all 1..16 rectangle dimensions, independent
strides, offsets, exact-length tails and overlapping read-only inputs agree with
the mathematical sum, symmetry, self-distance and unchanged guards. Existing
frozen native metrics and macroblock/frame packets remain the production oracle.
Unchanged full/scalar/package qualification and all current costs are reused;
53 focused checks are supplemental, not a repeated full-suite claim.

The caller review also identifies a required warm-coverage omission: methods0/1/2
use distortion-only prediction search in `PrepareDistortion`/`Vp8DistortionDecision`.
Current method4 RDO and method6 trellis controls never run that algorithm. Add
only the existing diagram at Q75/effort2: exact 7,330-byte packet, SHA256
`325a9d5b55b300db2554681735642edcdc8dcb866a88760501d3fe95fac1423b`.
It is unmeasured, not an assumed gap or pass. No new image or effort/settings
sweep is needed. The finite checklist therefore has 255 named required cases.

[Evidence](warm-vp8-squared-metric-screen-evidence.json) retains candidate binaries,
samples, restoration proof and the mirrored, member-verified raw archive. Stop
squared-metric variants and qualify that one missing path using the restored
immutable production build before further optimization.
