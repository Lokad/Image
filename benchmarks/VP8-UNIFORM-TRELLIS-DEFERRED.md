# Deferred uniform-boundary VP8 trellis reuse

The existing opaque 513×259 diagram, Q75/effort6, has 1,573 uniform-boundary Luma4
blocks. Reusing their identical predictions and scores could avoid 14,157 of
87,384 trellis calls. One candidate enables the existing no-buffer score reuse
for non-cancellable trellis, preserving headers, flat penalties and strict ties.

The short measures 15,246.66 µs / 1.554×, 3.4% below the unchanged 15,778.14 µs
baseline. Independent confirmation measures **15,696.78 µs / 1.596×, upper1.655**:
only 0.52% below baseline, with overlapping confidence bounds. No material gain
is demonstrated. Defer and remove this trial; do not retune its guard.

All twelve confirmation actuals remain; ten are filtered. Exact 6,818-byte packets,
754,344 B/op, ownership and quiet gates pass. Eighteen focused macroblock/trellis
tests pass normal/scalar on each OS, including native rolling state and default/
live-token packet equality. No new fixture, repeated full suite or package proof.

The qualified color-trial source and producers are restored, with matching Linux
DLL/PDB/XML; its actual private 0.2.1 package is unchanged. [The evidence](vp8-uniform-trellis-deferred-evidence.json)
retains samples, identities, proofs and the mirrored member-verified raw archive.
The [current benchmark](../docs/benchmark.md) continues to show the qualified
baseline. The later [fixed-importance trial](VP8-FIXED-IMPORTANCE-DEFERRED.md) is also deferred.
