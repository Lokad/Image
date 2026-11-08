# Current RGB444 JPEG attribution

JCP600 refreshes attribution after [paired AC decoding](JPEG-PAIRED-AC-WINDOW.md).
One two-second capture of the qualified immutable build records 1,996 sample
locations: 843 in the fused baseline reader, 413 in inverse AVX2, 85 in baseline
scheduling, 67 in plain AC, 51 in MCU storage, 34 in pair-table preparation and
27 in dequantization. Sampling/skid identifies leads, not phase times or predicted
gains. All instrumented elapsed time is discarded.

The existing 133,057B RGB444 packet still produces exact 531,468B independently
owned RGBA output. Complete public parsing, preparation, entropy, transforms,
packing, acquisition and disposal remain in the operation. The labeled native
decoder retains its declared codec/input/frame contexts. This is not fresh-context
equivalence. Source/pixel/alpha/ownership gates and both idle-v2 guards pass;
CPU0 runs the operation and CPU1 the sampler. The existing worker is reused, with
zero builds, new fixtures, settings or statistical measurements. Current qualified
1,530.34/native 1,450.52µs costs and package qualification remain unchanged.

Next inspect current fused-reader lookup/refill/descriptor and block-boundary work
using this JIT/map/trace. Preparation has a smaller location count; do not infer a
session benefit. Dequantization is already SIMD and completed reader exits already
publish canonical state. Preserve exact samples, following reads, malformed-input
behavior, cancellation and ownership. Establish actual caller lifetimes before
removing exception-time state work elsewhere.

Consult [paired inverse rows](JPEG-PAIRED-INVERSE-ROWS-EXPERIMENT.md),
[borrowed table views](JPEG-BORROWED-TABLE-VIEWS-EXPERIMENT.md) and
[shared entropy windows](JPEG-SHARED-ENTROPY-WINDOW-EXPERIMENT.md) before proposing
related work. Their adverse implementations remain useful evidence, not permanent
family bans. `DirectRgb444` is owned RGB-encoded-image decoding, distinct from the
passing caller-sample RGB boundary. No passing or uncertain case is a tuning target.

[Current evidence](jpeg-paired-current-attribution-evidence.json) pins sources,
producer, exact receipt, JIT/map/trace and guards. Mirrored/member-verified raw
`jpeg-paired-current-attribution-jcp600.tar.xz`, SHA256
`7ce1508f3951011ae7f9503f9d2f395799fa20a9b91356fd3f269cad74bd438d`,
143,056B/22 members, retains reproduction and qualified module/worker dependencies.
The earlier [JBA530 capture](jpeg-current-entropy-attribution-evidence.json) remains
historical evidence; its whole-operation shares predate paired AC decoding.
The full 283-path objective remains open; publication is postponed.
