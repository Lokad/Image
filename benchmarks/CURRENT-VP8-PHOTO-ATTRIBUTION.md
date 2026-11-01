# Current VP8 photograph attribution

One six-second trace on the current qualified DLL captures **5,910 samples**
for the existing 513×259 photograph/effort4 control. Its exact 17,688-byte packet,
reconstruction, source/output ownership and idle-v2 guards pass. No production,
harness, native or host build; no repeated correctness/package qualification.
Instrumented elapsed times are discarded. Samples describe attribution, not
phase timings or predicted gains; the existing independent CPU cost is unchanged.

- Luma4 decisions: 1,016 samples (17.2%).
- Ordinary quantization helpers: 751 (12.7%).
- Reconstruction: 502 (8.5%), including AddVector 434 (7.3%).
- Coefficient emission: 421 (7.1%).

The earlier QIV1b trace predates the accepted quantizer guard cleanup. This trace
resolves that attribution uncertainty without retuning a confirmed case.
[Paired prepared quantization](VP8-PREPARED-QUANTIZATION-PAIR-REJECTED.md) has now
been screened and rejected: 2.39% elapsed / 2.92% native-normalized, unchanged
allocation and exact packet. Source and binaries are restored without rebuilding;
no confirmation or variants. This trace remains current attribution, not a gain
claim. Closed reconstruction, trellis and width-only variants remain closed.

[Evidence](current-vp8-photo-attribution-evidence.json). Mirrored/member-verified
current-vp8-photo-attribution.tar.xz, 47 members, SHA256 d04385dc49b612e4a1b03fd830b7a89fbc7fc96f993faaff0bb0f49558a41121.
Raw retains the event trace, same-process map, targeted JIT text, guards/validation,
actual host/library/worker binaries and pinned reused dependencies.
