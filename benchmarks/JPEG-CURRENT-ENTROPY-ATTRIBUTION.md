# Current RGB444 JPEG entropy attribution

JBA530 reuses the qualified immutable module and existing `DirectRgb444` control.
One bounded two-second capture has 1,995 sample locations: 958 in the fused
baseline reader, 394 in inverse AVX2, 60 in plain AC, 69 in baseline scheduling
and 57 in MCU packing. Sampling/skid identifies leads, not phase times or
predicted savings. Instrumented elapsed is discarded. No production change,
build, new fixture or current-cost update.

The frozen 133,057-byte marker-only RGB packet still produces the exact
531,468-byte independently owned RGBA output. Complete public decoding,
preparation, entropy, transform, packing, acquisition and disposal remain inside
the operation. Native codec/packet/frame contexts retain their declared
preparation; this is not fresh-context performance. Both native brackets,
pixel/alpha/source/owner checks and idle-v2 guards pass. CPU0 runs the operation,
CPU1 the sampler. The cached 128-operation selector and original harness are reused.

The current 816-byte fused routine still loads and checks its primary Huffman
table inside the AC loop. Other hot locations cover signed amplitudes and
zigzag stores. First inspect borrowing fixed-length views of the **existing**
tables once at reader entry, preserving complete-field and long-code guards.
No extra table, cache, capacity, session or output work is proposed. Less repeated
setup would need complete-operation evidence; code size or allocation alone
cannot establish a gain.

A separate architectural lead is a shared MCU entropy window: current baseline
calls initialize/fill and publish the high-aligned cache per block. Establish
geometry, stuffing, tails and canonical-cursor applicability before prototyping
that lifetime. Keep exact normal/partial coefficients, predictors, following
reads, malformed-input behavior, every live checkpoint and ownership cleanup.

The [accepted long lookup](JPEG-BUFFERED-LONG-LOOKUP.md) and
[live publication](JPEG-LIVE-NORMAL-PUBLICATION.md) remain. Their earlier profile
shares are historical. Prepared-amplitude nibble and row-pair implementations are
not blindly replayed or permanently banned. `DirectRgb444` decodes RGB-encoded
input to an owned image; it is distinct from the passing `CallerRgb` sample-output
control. Passing and uncertain controls are not tuning targets.

[Evidence](jpeg-current-entropy-attribution-evidence.json) pins current sources,
producer, exact receipt, JIT/map/trace, guards and next hypothesis. Raw
`jpeg-current-entropy-attribution.tar.xz`, SHA256
`f915afc6cb8774e4bb9ae35ab34158cab20a37a924a9844085decd8bbaeaf2d0`,
6,202,504 bytes / 115 members, is mirrored/member-verified with pinned current
module and fixture/tool providers. Reproduction scripts, original actuals and
every observation remain. All 125 displayed costs and package qualification
reuse unchanged paths; no broad/package repetition or renderer dependency is
added for this attribution. Publication and the full 283-path objective remain open.
