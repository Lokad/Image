# Exact bounded JPEG entropy output

Existing 513×259 controls retain frozen inputs, quantizers/settings and complete
normalization/encoding/fresh independent output acquisition/release, on one CPU.
Color uses FFmpeg; grayscale uses the approved standalone SIMD libjpeg-turbo
reference. Packets stay exact: only approved attribution COM differs for color,
52,115/52,117 B (RGBA16) and 52,091/52,093 B (RGBA8); grayscale is raw-exact 58,910 B.

| Quiet VM run | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Caller-root short | 3,825.79 | 1,838.44 | 2.081/2.365 | 804,635 |
| Caller-root independent | 3,811.21 | 1,837.69 | 2.074/2.084 | 804,635 |
| Nongoverned RGBA8 short | 3,651.90 | 1,765.34 | 2.069/2.507 | 373,416 |
| Nongoverned RGBA16 short | 3,744.72 | 1,836.31 | 2.039/2.384 | 373,416 |
| RGBA-gray short / SIMD libjpeg-turbo | 2,237.75 | 1,405.01 | 1.593/1.742 | 155,443 |
| Caller-gray short / SIMD libjpeg-turbo | 1,822.25 | 1,078.35 | 1.690/2.000 | 155,449 |

Independent caller-root confirmation uses 4.4% less elapsed than the
[preceding confirmation](WARM-JPEG-COMPILED-COLOR.md). All 12 actual/11 selected
iterations contribute to the conservative upper; both idle-v2 guards pass.
Other rows are current short costs, without independent speedup claims.

A lazy 512-byte entropy buffer replaces per-byte output calls. The first full
chunk is 256 bytes to avoid excessive caller-stream growth after a small header;
partial restart flushes preserve that threshold. Every bit, marker and positive
work charge remains. Reserve 640 B before allocation through the borrowed root,
hold through actual output users, release promptly on all exits and never flush
failure disposal. Caller output/context/root ownership and terminal refusal remain.

Main allocation increases 560 B/op. Observed whole-operation root peak remains
1,704,856 B, including 1,062,936 B surviving source; the extra 640 B owner is still
charged during use. An earlier candidate increased grayscale allocation by about
58.7 KB/op through caller MemoryStream growth. The bounded diagnostic identified
58,144 B extra growth; the first-chunk revision restores 65,536-byte capacity and
removes that regression before acceptance. Diagnostic times are discarded.

Both platforms pass 5,127 checks each, including 947 live comparisons, 16 scalar
writer/record checks and 148 scalar public JPEG checks. New real-root tests cover
aggregate ownership, refusal/recovery, cancellation, work failure and I/O lifetime;
existing amplitudes/records/native packets and the frozen-gray capacity control
remain exact. The same actual private 0.2.0 Release package and all three examples
pass both platforms. Benchmark/native/prepared hosts and fixtures were reused.

[Evidence](warm-jpeg-buffered-entropy-evidence.json) records identities and reuse.
Other affected JPEG encode timings stay historical; decoding, PNG, WebP, editing
and text reuse unchanged evidence. Full 257-case/1× and readiness remain open.
