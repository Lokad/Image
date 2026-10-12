# Warm Paeth RGBA16 baseline

One quiet screen exposes 2.604x FFmpeg, conservative upper
2.721x. Managed mean is
14432.02 microseconds. Both unchanged quiet
guards pass. This fixed pair remains above2x and has no independent confirmation.
Production is unchanged.

One original authoring uses the existing513x259 RGBA16 raster, noninterlaced PNG,
Paeth on every row, BCL Optimal level6 and one IDAT. The772,037-byte input is
identical on both sides. Independent reconstruction verifies every filtered byte
and all531,468 sixteen-bit samples, including alpha and hidden RGB. Normal/scalar
FFmpeg CLI outputs, source preservation and independent held outputs are exact.
Eight negative native protocol/input/profile/affinity controls pass. Windows
normal/scalar pixels match the retained actual package, with unchanged observed
public Stream allocations4,713,416 bytes/op. Fresh and actual timing libraries
retain all563 production methods and flags from that package. DecodePaeth16 is
explicit; the default remains one Decode8 case. No raster, geometry, filter,
quality or effort sweep is added.

Public resident-input decode includes validation, allocation and disposal.
Native wrappers/context are reused; complete decode, independent acquisition/
release and RGBA16 big-to-little-endian conversion with a second owned frame are
timed. CPU0, one codec/conversion thread, thread type zero, normal SIMD, tiering
off and nonconcurrent workstation GC are fixed. Startup, I/O, IPC, preparation,
hashing and validation checks are excluded. The screen retains three actual
managed iterations and three native samples per bracket. The conservative upper
uses the larger filtered/all-actual confidence bound with the corresponding
managed critical value for native bracket lower bounds.

One six-second CPU profile contains5,954 samples. Paeth
prediction and the generic unfilter loop account for
45.8% of sampled CPU. The next substantial
primitive groups eight independent channel bytes in safe128-bit lanes, preserving
Paeth tie order, byte overflow, scalar fallback, tails, cancellation, alias/truncation
behavior and existing work/memory admission. No upstream code or tables are copied.
Diagnostic sample shares do not certify a speedup.

The [numeric evidence](warm-pngpaeth-baseline-evidence.json) retains every actual
measurement, native bracket, guard, input/source/actual-child/package identity,
Windows controls and immutable timing/analysis/profile archives. Regenerate with
`python benchmarks/render-warm-pngpaeth-baseline.py`; `--check` verifies this report.
See the [reference recipe](../tools/WarmReference/README.md). Broader required,
unmatched and unconfirmed warm coverage remains open.
