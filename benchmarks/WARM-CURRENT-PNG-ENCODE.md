# Current RGBA PNG encode confirmations

Historical <=2x checkpoint. Its PNG encode timings are superseded by the current
[chunk checksum round](WARM-PNG-CHUNK-CRC.md), which refreshes all16 affected
selectors and distinguishes independent parity from qualified short gaps.

The two named Optimal/level6 RGBA8/16 encode cases independently confirm on
current production paths. Their older full-operation confirmations preceded
the row-packing changes; later short regression screens were not current
independent confirmations. These measurements resolve that uncertainty. Stop
both fixed pairs; no production optimization is needed.

| Fixed case | Lokad.Image / FFmpeg µs | Ratio / upper | Managed B/op | PNG bytes Lokad / FFmpeg | IDAT bytes Lokad / FFmpeg | Actual / selected |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Encode16 | 14,974.15 / 20,089.84 | 0.745 / 0.763 | 2,103,624 | 960,565 / 963,202 | 960,340 / 960,316 | 12 / 12 |
| Encode8 | 5,425.94 / 17,261.40 | 0.314 / 0.316 | 527,627 | 218,775 / 210,973 | 218,682 / 210,283 | 12 / 11 |

Both encoders consume the same original 513x259 RGBA byte/word raster, use color6,
filter None and numeric compression level6, and produce a complete independently
owned PNG. Exact full samples/precision, alpha, hidden RGB, source preservation,
normal/scalar native CLI validation and simultaneously held independent output
owners pass. Frozen managed/native packet hashes and lengths are unchanged.
The existing size gate requires managed IDAT <=105% of native IDAT; no quality,
compression, filter or pixel allowance is relaxed. Other already confirmed
color-model/compression cases are unchanged; no settings grid is introduced.

Managed timing includes fresh MemoryStream creation/growth, the complete public
EncodePng operation, row packing, compression, checksums/container output and
disposal, with no redundant ToArray. Native codec/source/packet wrappers are
warm; real packet acquisition, complete FFmpeg PNG encode and independent packet
release are timed. For words, the complete native LE-to-BE conversion runs every
operation into the existing reused prepared frame; managed row conversion and
storage remain part of EncodePng. No output cache. Startup, IPC, file/network I/O,
generation, hashing and validation are excluded.

Passing three-actual screens precede independent twelve-actual confirmations on
the same immutable production/harness/native build. Every actual is retained;
conservative uppers cover selected/all-actual intervals against the faster native
bracket's lower confidence bound. Both versioned quiet guards pass every scored
run. Sequential Release uses CPU0, one codec/conversion thread, normal SIMD,
tiering off and workstation nonconcurrent GC. Managed allocations include harness
costs; native allocation totals are unmeasured.

The existing verified harness, native worker, qualified production library and
two original rasters are reused. Only one shared two-case driver per sampling
length is prepared; every cached child, including WarmDry, receives verified
assemblies. Production/harness/native builds are zero. Existing qualification
remains valid: 3,483 Release tests per OS, including 898 live comparisons, and the
consumer/all three examples through the same actual Release package on both OS.
No repeated suite/package proof, profile, whole-library method audit or extra
image/shape/depth/filter/quality grid is introduced.

[Numeric evidence](warm-current-png-encode-evidence.json) retains all samples,
allocations, packet/IDAT sizes, exact identities, guards and qualification reuse.
This closes the two named B08 fixed-input cases; full required warm coverage
remains open. Next: the two existing alpha-bearing VP8 encode cases.
