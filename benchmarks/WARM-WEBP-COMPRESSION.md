# WebP compression controls

Two fixed 513×259 opaque RGBA8 inputs cover photographic texture and original
text/diagram content; [provenance and hashes](Fixtures/Compression/manifest.json)
are frozen. No shape, depth, quality or effort grid was added. Both native workers,
the production binary and existing quality sampler are reused.

| Complete encode | Managed / reference µs | Ratio / conservative upper | Managed allocated B/op | Managed / reference bytes | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| VP8L photograph | 6,347.64 / 83,747.05 | 0.076 / 0.077 | 1,051,162 | 385,854 / 160,032 | Independent parity |
| VP8L text/diagram | 1,905.57 / 9,558.54 | 0.199 / 0.204 | 67,720 | 31,072 / 7,234 | Independent parity |
| VP8 photograph | 7,531.53 / 9,729.98 | 0.774 / 0.783 | 541,256 | 21,826 / 17,688 | Independent parity; matched quality |
| VP8 text/diagram | 6,442.53 / 5,837.29 | 1.104 / 1.246 | 490,795 | 8,958 / 7,100 | Short gap; matched quality |

VP8L uses **libwebp exact=1, FFmpeg's encoder backend**, with every pixel exact
through managed and normal/scalar FFmpeg decoding. VP8 uses **FFmpeg libwebp**.
All encodes use effort4. Native VP8 quality75 is matched by managed78 for the
photograph and75 for the diagram: source PSNR33.325344/33.305959dB and
33.766523/33.548268dB respectively (native/managed). Gaps0.019385/0.218255dB pass
the existing0.25dB gate; alpha is exact and decoder differences stay within the
existing max4/RMSE1.5 gate (observed max1). The bounded sampler evaluates six/five
settings, not equal quality-number assumptions. [Quality evidence](warm-webp-compression-quality-evidence.json)
retains packets, sizes, metrics and selection order. A passing anchor is not retuned.

The boundary includes public managed admission, every source pixel, fresh
MemoryStream growth/disposal and complete independent encoded output; no ToArray
copy. Native normalization/repacking, pixel import, encoding and packet ownership
are included. Immutable input preparation, font rendering, I/O, IPC, validation
and startup are outside both timers. No output packet is cached. Source preservation,
syntax, deterministic packets and independent surviving owners pass. Native
allocations are unmeasured, not zero.

Dedicated EPYC9V45 VM, Ubuntu24.04.4, .NET10.0.8/SDK10.0.204, FFmpeg n8.1
9047fa1b084f; one pinned CPU, one codec/conversion thread, normal SIMD, tiering off,
nonconcurrent workstation GC. Four three-actual screens precede three independent
twelve-actual confirmations; the clear diagram VP8 gap receives no long repeat.
All48 actuals, native pre/post brackets and both idle-v2 guards per run remain in
[numeric evidence](warm-webp-compression-evidence.json). The upper is the larger
selected/all-actual interval against the faster native bracket's lower bound.

Production Linux SHA256 dedef95a0ef131df54855ebb6b090e7d194c8f4646f28bb570a06f81898add79;
new harness7dd64eaba92bb1472d65f6855cc9be231ffecd262bfeb1b6525e3fc7e0fecbe3.
Timed source53beca2166a5e6e0169cb632a654ac8fb9149545a3f17e2c765a9cc3dcca1cf0,
2401files, differs from the final documentation only. The prepared drivers load
the verified immutable producer/harness after isolated compilation. Existing
4,153 tests and24 affected hardware-off checks per OS and the same actual package
consumer/all three examples on Windows/Linux remain applicable: no production
code changed. Existing performance proofs remain applicable.

Raw `h9b-measurements.tar.gz` is retained on both hosts, SHA256
7ef549b49556fa451227bbcf542af7a479d953faecc69569a2208b4452636f0f.
It includes fixtures/source identities, quality packets, full logs, drivers,
receipts, guards and actual binary identities. These are four named measurements,
not broader content guarantees. Compression efficiency and the full parity
objective remain open; all three current passing time cases stop until affected.
