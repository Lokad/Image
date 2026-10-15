# Current-build PNG16 decode and horizontal bicubic enlargement

Two existing workloads independently confirm on the qualified build: PNG16
decode at **1.045× FFmpeg / upper 1.059×**, and RGBA16 horizontal bicubic
enlargement at **1.473× the FFmpeg scaler backend / upper 1.499×**. Exact managed
samples and all original native comparison gates pass. Production is unchanged;
these observations replace older near-2× results without a new optimization.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| PNG16 three-actual screen | 3,026.31 | 2,898.73 | 1.044 / 1.160 | 8,552 | Screen |
| PNG16 independent twelve | 3,034.16 | 2,903.68 | 1.045 / 1.059 | 8,552 | Confirmed |
| Bicubic16 preceding three-actual screen | 2,169.01 | 1,458.44 | 1.487 / 2.440 | 2,129,311 | Screen |
| Bicubic16 independent twelve | 2,144.05 | 1,455.48 | 1.473 / 1.499 | 2,129,311 | Confirmed |

PNG uses the original 513×259 RGBA16 raster and unchanged noninterlaced/None
packet, SHA-256 `3e4b64d778c395392451d5331d2a11b541d262e2f39826e1322d22d6f03bb4a8`.
Managed and normal/scalar native decoding preserve every full word, alpha and
hidden RGB sample. The complete public decode includes framing, checksums,
inflation, storage conversion, admission, output acquisition and disposal. Native
BE-to-LE conversion and owned output are timed. No output is cached.

Bicubic uses the same original word raster at 513×259→1027×259, ordinary working
limit. Managed pixels match the independent rational reference exactly. The
explicitly labeled zimg/FFmpeg zscale backend includes the required clamped halo,
source unpacking, premultiplied float filtering, independent output allocation,
unpremultiplication, quantization and zero-alpha RGB clearing. Normal/scalar
native RGB and alpha remain within maximum 1 and RMSE 0.25; neither gate is
widened. Source preservation and independent simultaneous owners pass.

Both versioned quiet guards pass on each scored observation. The original
bicubic preparation refusal has no timings: CPU0 I/O wait exceeded the threshold.
Its wrapper also omitted the actual pre-sync despite reporting the flag; the
successful retry explicitly syncs before and after measurement. The refusal is
retained, with no historical-load cooldown or rescore. Only the stale bicubic
native worker rebuilt after a previous shared source change; PNG's identity
matched. The earlier bicubic short uses the preceding core; editing inputs are
unchanged by the intervening JPEG-only production change. Its twelve-sample
confirmation uses the current qualified core.

Both confirmations retain all twelve actuals; PNG selects twelve and bicubic
eleven. The upper includes selected and all-actual intervals against the faster
native bracket's lower bound. Sequential Release uses one pinned CPU, one
codec/conversion/filter thread, normal SIMD, disabled tiering and workstation
nonconcurrent GC. Startup, I/O, IPC, fixture creation and validation are excluded.
Native allocation totals are unmeasured; managed figures include harness costs.

The [JPEG AC planning qualification](WARM-JPEG-AC-PLANNING.md) is reused: 3,483
Release tests per OS, including 898 live comparisons, and the consumer plus all
three examples through the same actual Release package on Windows and Linux.
No production/harness rebuild, new profile or repeated suite/package proof is
needed. Only necessary cached selection drivers change. The
[numeric evidence](warm-current-png16-bicubic16-evidence.json) retains actual
samples, guards, allocations, hashes and the refusal. Raw receipts retain machine
prefixes; published native library prefixes are normalized. Previous
[PNG](WARM-PNG-LITERAL-RUN.md) and [bicubic](WARM-BICUBIC-HORIZONTAL-UP16.md)
reports preserve older results. Stop both pairs; full uniform warm coverage
remains open.
