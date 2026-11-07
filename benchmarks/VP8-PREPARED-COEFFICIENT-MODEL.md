# Prepared VP8 coefficient model

One decoder-owned 96-row model replaces the 1,056-byte probability array with
the same payload and lifetime. Header updates borrow a byte view; ordinary,
cancellable and tail readers borrow typed rows. Per-block casting disappears.
The ordinary loop also traverses coefficient classes directly, removing its
numeric token helper. There is no second model, cache, session or public API change.

Complete opaque decoding saves **155.50 µs / 3.08% raw / 3.16% reference-normalized**
against the qualified original. Both filtered and all-actual raw/normalized
intervals separate; native moves +0.083%. The alpha control establishes neither
a gain nor a regression: its intervals overlap in every view. This is a scoped
opaque gain, without a general decoder or equivalence claim.

| Complete operation, independent twelve actuals | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| VP8 + alpha, resident | 6,237.83 | 5,978.44 | 1.043 / 1.048 | 238,384 | Gap |
| VP8 + alpha, fresh seekable stream | 6,132.96 | 5,985.53 | 1.025 / 1.028 | 238,480 | Gap |
| Opaque VP8 | 4,899.37 | 4,792.09 | 1.022 / 1.026 | 228,864 | Gap |
| Simple-filter opaque VP8 | 5,013.25 | 4,887.05 | 1.026 / 1.031 | 228,864 | Gap |
| Live-cancellation opaque VP8 | 4,979.43 | 4,789.64 | 1.040 / 1.044 | 228,864 | Gap |
| VP8 + alpha coding validation | 4,529.58 | 5,446.76 | 0.832 / 0.836 | 13,584 | Pass; stop |

All six affected costs are refreshed: one pass, five gaps, none uncertain.
`StreamNative8` is the existing `Vp8Stream` control, not an added workload.
Only opaque/resident-alpha have paired original/candidate confirmations; the
other current costs establish no causal gain. Stream/resident point differences
are not an attribution experiment. GC allocations are unchanged on paired controls.

Inputs remain the frozen 513×259 packets. Parsing/model preparation, entropy,
reconstruction/filter/color, output acquisition and disposal remain timed on one
CPU. Fresh stream delivery is included where named. Native codec/conversion/
frame wrappers retain their declared prepared boundary; this is not fresh-context
performance. Exact pixels, alpha and independently owned output remain mandatory;
validation returns the exact 36-byte descriptor without decoding an output image.

The 8,192-byte coarse header allowance still covers one 1,056-byte model. No new
owned capacity, scratch payload or retained state is introduced. Returned frames
retain 531,468 bytes until disposal. Actual GC is measured above; aggregate
admission/physical/native peaks were not remeasured or inferred from GC. Existing
root/refusal/cancellation/survivor checks and scoped source projections establish
unchanged ownership. No production dependency or numerical contract changes.

All arithmetic/refill/guard/publication bodies remain exact. The live class
reconstructs its original source after undoing only the typed parameter/cast
change; the decoder reconstructs its original except the single model owner,
header byte view and scalar row view. Numeric-helper coverage now exercises
4,096 complete production blocks, witnessing all twelve symbols, signed wrapping,
contexts, tails, canonical cursors and following reads against the independent
bitwise oracle. Every live decision checkpoint remains.

Windows/Linux each pass **5,875 full/live checks**, plus **40 internal / 239 public**
per normal/scalar mode. The **same actual private Release package and all three
examples** pass both OSes. One immutable build per coherent A/B stage per OS;
broad/package qualification runs once at acceptance. No public/harness/native/
selector/fixture rebuild. An initial missing-import test compile failed before
any measurement and is retained with its correction. No completed job is repeated
for favorable confidence. Published packages remain unchanged; publication is postponed.

Stage A's correct direct-class loop retained per-block casting and gave
inconclusive short screens. Stage B prepares the typed owner once. Current JIT
code is 2,123 bytes versus the original 2,144, with the same 72-byte local frame;
code size alone is not CPU evidence. The preceding attribution's 1,993 samples
selected ordinary coefficient work; its shares are historical after this change.

[Evidence](vp8-prepared-coefficient-model-evidence.json) pins sources, modules,
complete actuals, output checks, same-package verification and storage boundaries.
Raw `vp8-prepared-coefficient-model-qualified.tar.xz`, SHA256
`1d8d81f5fe9bf6c86bb32cd7b1635df078d8868c5c534fcab19e31c63cc19ee1`,
10,813,884 bytes / 356 members, is mirrored and member-verified. Its pinned raw
providers retain the original controls and attribution. Reproduction scripts,
commands, guards, intermediate sources, failed preflights and qualified outputs
are retained there.

This supersedes six VP8 costs from [output ownership](VP8-OVERWRITE-OUTPUT.md),
[live classes](VP8-DIRECT-COEFFICIENT-CLASSES.md) and
[final publication](VP8L-FINAL-PUBLICATION.md). Six unchanged lossless costs from
the last report remain valid, including its scoped gain and palette regression.
Two displayed rows refresh; 123 reuse unchanged paths. This is not a full
actual-package release refresh or completion of all 283 required material paths.
