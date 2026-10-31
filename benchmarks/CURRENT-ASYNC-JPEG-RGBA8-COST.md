# Current asynchronous RGBA8 JPEG encoding

The existing 513×259 RGBA8 nonseekable-output control independently confirms a
**1.199× / conservative upper 1.210** CPU gap. Complete packets are **133,039 /
133,041 bytes**, differing only by approved COM attribution. Current public/native
reconstruction matches exactly. This replaces stale cost evidence without a
causal comparison to its older producer.

| Complete warm single-CPU operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 EncodeJpegAsync, 444 | 4,626.462 | 3,857.583 | 1.199 / 1.210 | 820,475 | 133,039 / 133,041 |

Both subjects include full alpha flattening onto 0x204060, conversion/444 encoding,
admission/staging, fresh growing nonseekable output, actual immediate delivery
copies and release. Managed awaits WriteAsync users; native includes FFmpeg AVIO
buffering and internal flush. Neither times real I/O, forced scheduling or a final
snapshot. Q85/native qscale5 and the original input/reference packet stay frozen;
numerical quality values are not treated as equivalent. Native allocation is unmeasured.

The stored native quality record expected a prior public-decoder difference
(maximum3/RMSE0.227). Independent checks establish unchanged native pixels/source
quality and current exact public decoding. Only those decoder-result fields are
refreshed in a temporary header. No pixels, settings or gate change. Both global
headers are restored byte-for-byte after validation and timing.

The current public async probe and setup/cleanup verify exact packets, ownership,
surviving outputs, terminal refusal, cancellation, sink failure and recovery.
All twelve actuals, both confidence views, native brackets and passing idle-v2
guards are retained. The first cached selector rejects the wrong Case before
any warmup/actual. One small selector build supplies Encode8 and twelve actuals,
with explicit unroll16 matching sixteen calls. No production/harness/native build
or profile; unchanged full/live and actual-package/three-example proof is reused.

The archived immutable harness retains its original untimed reporting field;
the current reporting-only cleanup does not change timed methods or validation.
Current quality comes from the actual probe and exact packets. No successful
timing is repeated, and no required case or workload grid is added.

[Evidence](current-async-jpeg-rgba8-cost-evidence.json) pins all samples, scoped
metadata repair, original failures, source reuse and qualification. Mirrored,
member-verified raw: `current-async-jpeg-rgba8-cost.tar.xz`, SHA256
`ad9d5a1081922df1163a9b8449a289bb4fe2503a5a54e0042981be1c8104e376`; runtime/qualification dependencies are pinned.
The full 268-path upper≤1× objective remains open.
