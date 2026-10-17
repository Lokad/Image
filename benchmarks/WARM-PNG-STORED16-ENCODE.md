# RGBA16 PNG encoding without compression

Historical <=2x checkpoint. Its PNG encode timings are superseded by the current
[chunk checksum round](WARM-PNG-CHUNK-CRC.md), which refreshes all16 affected
selectors and distinguishes independent parity from qualified short gaps.

Complete warm public RGBA16 `NoCompression` encoding confirms **1.243× FFmpeg /
upper 1.289×**, at **504.25 µs** and **4,203,541 B/op**. Replacing per-channel
sample access with bounded, safe span endianness conversion cuts the baseline
mean **79.6%**, from **6.118×**. Stop tuning this fixed pair.

The same original 513×259 word raster is used throughout. Managed encoding times
fresh MemoryStream creation, growth, encoding and disposal. The original native
adapter times full-frame little-to-big-endian conversion, encoding and independent
owned packet acquisition/release; codec/source/prepared-frame wrappers are reused.
Both subjects use filter None and stored Deflate, with exactly **1,063,366 IDAT
bytes**. Full words, alpha, hidden RGB, source and independent outputs pass exact
checks, as do normal/scalar FFmpeg CLI results. Managed PNG bytes are identical
before and after the change. Compression policy and the 5% size gate are unchanged.

A six-second profile attributes 81.2% of samples to the encoder and repeated
pixel/stride access. The replacement converts at most 4096 pixels per group with
the BCL's vectorized span API. It retains cancellation checkpoints, row work,
admission, scratch size and stream ownership. Focused tests cover full word order,
channel/hidden-RGB preservation, scalar execution, tails, compression policies,
cancellation and exact admission boundaries. The compressed RGBA16 regression
screen passes at **0.725× / upper 0.756×**, **2,103,776 B/op**, before qualification.
This is a three-iteration regression screen; its prior confirmation remains linked
in the [earlier report](WARM-PNG16-ENCODE-CRC.md).

Sequential Release measurements pin one CPU, one codec thread, normal SIMD,
tiering off and workstation nonconcurrent GC. The short screen has three actuals;
the independent confirmation uses the same frozen core/harness and retains all
twelve actuals, including two observations filtered from the reported mean, in
the conservative bound.
All versioned quiet guards pass. Preparation, startup, I/O, IPC and validation
are excluded. An unscored regression preparation with incorrect command flags
is retained; the separately guarded measured job is verified without repeating it.

Both Windows and Linux pass **3,318 Release tests**, including **896 live FFmpeg
comparisons**, and 49 focused normal/scalar checks per OS. One actual Release
package passes its consumer and all three usage examples on both OSes. Only SPEC
and resource-admission documentation changed after measurement; production and
harness bytes remain frozen. No unrelated method audit was repeated.

[Numeric evidence](warm-pngstore16-encode-evidence.json) retains all actuals,
allocations, native brackets, guards, packet/build identities, profile and package
qualification. Ignored immutable archives preserve receipts, source inventories,
generated jobs and four actual child identities. Native allocations are unmeasured.
[Fastest RGBA16](WARM-PNG-FAST16-REFUSAL.md) remains unscored after its size-gate
refusal; the remaining material public/current-source coverage stays open.
