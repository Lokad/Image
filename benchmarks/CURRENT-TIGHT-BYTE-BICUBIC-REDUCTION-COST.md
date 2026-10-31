# Current tight-memory byte bicubic reduction

The existing513×259 RGBA8→256×129 strip path confirms **1.552× / upper1.558**,
**646.826 µs** against416.724 µs for FFmpeg zscale's zimg backend, with
**132,309 allocated B/op**. It remains a gap against the ≤1× objective.

The unchanged local bound is **667,660 bytes**: source531,468 + owned output132,096
+ **4,096 scratch**. Complete public Resize/admission/geometry/filtering/fresh
output/disposal and native halo/packing/premultiplication/filtering/independent
planar/packed outputs/unrounded-alpha quantization/release are timed. Native
allocation is unmeasured; preparation/startup/IPC/I/O/validation are excluded.

All twelve actuals, filtered/all confidence, native brackets and quiet-v2 guards
are retained. Exact rational/frozen pixels, source/output/native hashes, native
max1/RMSE≤0.25 and normal/scalar bounds, source immutability and independent owners
pass. **Zero builds/profiles**: reuse the accepted immutable production/harness,
completed selector and byte-identical native worker. Full5,245/live checks per OS
and the same actual Release package/three-example proof are reused from the
[accepted guard change](WARM-BICUBIC-BYTE-FIXED-CERTIFICATE.md).

An older twelve used six-column/int-offset strips and fuel charging; the current
eight-column/byte-offset layout and direct cancellation invalidate that cost.
This is not a causal speedup claim against the older producer. A postcheck found
only the description changing from “optional cache excluded” to “full output-row
cache excluded”; every other boundary character and operation gate matches.
Original successful samples are recovered without retiming.

This existing distinct boundary brings the summary to67 comparisons, with all268
required paths unchanged. It joins tight enlargement among the largest substantive
gaps. No new input, sampling, settings, filter, depth or grid is introduced.

[Numeric evidence](current-tight-byte-bicubic-reduction-cost-evidence.json) retains
producers, complete samples, bounds and qualification reuse. Mirrored/member-
verified raw current-tight-byte-bicubic-reduction-cost.tar.xz, SHA256
50f12c50c4b0264ff3c699359581a34440db7c99d37868043f475eafc5ec7f81,
retains sources, actual runtime/native worker, selector, guards and recovery.
Use existing WarmBicubicResize/TightDown8 with the
[reference tooling](../tools/WarmReference/README.md).
