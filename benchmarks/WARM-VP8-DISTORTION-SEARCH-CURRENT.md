# Current distortion-only VP8 search

This baseline timing is historical after [squared groups](WARM-VP8-DISTORTION-SQUARED.md).
Its exact packet/reference boundary remains unchanged.

The existing 513×259 opaque RGBA8 diagram at Q75/effort2 measures 9.04 ms:
3.615× reference time (upper 3.869×), 741,257 B/op and an exact 7,330-byte
packet. This is a guarded short, not independent parity evidence. All three
actual/filtered samples remain; both idle-v2 guards pass on one CPU.

This control runs distortion-only prediction search, which the method4 RDO and
method6 trellis controls never execute. It is one existing frozen functional
control promoted to the finite warm checklist, without a new image or settings
sweep. Complete packets, normal/scalar reconstructed pixels, alpha, borrowed
source and independent output ownership pass. Both complete operations include
normalization/repacking, fresh packet growth/acquisition and release; startup,
transport, validation and ToArray are untimed.

Only the missing effort2 worker/harness/dispatch were built. The native worker
derives from our owned opaque adapter with three checked effort substitutions;
existing4/6 workers and all production binaries stay unchanged. The binary-reference
harness compiles on Windows; live preparation and scoring run on the quiet VM.
Current full/scalar/package qualification is reused, without repetition.
[Evidence](warm-vp8-distortion-search-current-evidence.json) retains immutable
identities, samples and the mirrored, member-verified raw archive.

This is now the largest demonstrated gap. The previously verified squared-metric
candidate can be rescreened once on this distinct algorithm; its marginal RDO
photograph result does not predict its importance here. No variant or effort grid
is needed. Production remains on the qualified flat-count build pending acceptance.
