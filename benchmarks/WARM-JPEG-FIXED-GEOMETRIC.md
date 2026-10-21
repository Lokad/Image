# JPEG fallback: geometric AC coding

The [subsampled AC decision round](WARM-JPEG-AC-DECISION.md) supersedes the Original color packet/pixel and cost claims for affected layouts below. Default caller-RGB420 is now55,797/ref52,093 B; grayscale and optional CompactColor remain separate. The results below retain their historical producer scope.


The [fixed AC fusion round](WARM-JPEG-FIXED-AC-FUSION.md) supersedes the fixed
encoder timings below. Frozen packets/quality anchors and unaffected adaptive
controls remain applicable; earlier results retain their historical status.

The complete forward-only RGBA8 encode confirms **1.916× FFmpeg / conservative
upper 1.936×**, with **263,812 allocated B/op**. The original screen was 2.098×
at 527,367 B/op. The packet shrinks **45%**, allocation falls **50%**, and every
quantized coefficient, amplitude and decoded pixel remains exact. This fixed
pair clears the target; uniform current-source coverage remains open.

| Stage | Lokad.Image µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Original fixed-width AC screen | 3,703.74 | 2.098 / 2.370 | 527,367 |
| Geometric table screen | 3,536.06 | 2.004 / 2.196 | 263,812 |
| Packed lookup screen | 3,532.18 | 2.002 / 2.067 | 263,812 |
| Prevalidated writer confirmation | 3,659.37 | 2.059 / 2.139 | 263,812 |
| Final block serializer screen | 3,477.76 | 1.971 / 3.121 | 263,812 |
| Final independent confirmation | 3,382.57 | 1.916 / 1.936 | 263,812 |

Intermediate steps are not accepted separately. The first independent repeat
misses two and remains retained. A production change then moves fixed-block
serialization out of the large sampling/transform loop. The final confirmation
retains all twelve actuals, with ten selected; its bound uses the more
conservative filtered/all-actual interval against the fastest native bracket's
lower bound. Both quiet guards pass for every retained run. No unchanged timing
is repeated to obtain a pass.

The fallback replaces eight-bit AC codes with an original image-independent
geometric model: weight `2^max(0,20-run-size)`, EOB `2^18`, ZRL `2^4`. The existing
independent canonical builder covers all 162 supported AC symbols, limits codes
to sixteen bits and reserves all ones. DC codes remain four bits. Shared
immutable codewords combine code/length for a prevalidated writer; dynamic
amplitudes retain their original checks. For the new packet, checked and
prevalidated emitters have identical work, byte callbacks, quotas, cancellation
and failure state. Sampling/transform failure ordering stays unchanged. No image
statistics, fitted corpus, upstream implementation table or per-image table
cache is introduced. Normal adaptive MemoryStream coding is unchanged.

An independent entropy recoder preserves every coefficient and amplitude while
changing the packet from **147,635 to 81,255 bytes**. The integrated packet equals
that reference byte for byte. The existing authored 513×259 RGBA8 raster,
quality 69/native qscale 5, background RGB 0x204060 and **0.106110 dB** source-quality
gap remain fixed. Managed/native decoded hashes are unchanged. This is a
source-quality timing comparison; compression efficiency parity is not established.
There is one quality setting, no sweep and no additional performance raster.

The complete operation includes a fresh forward-only Stream wrapper over fresh
owned MemoryStream storage, wrapper creation/disposal, storage growth, full
conversion/encoding and release. Native flattening/conversion, encode and
independent packet acquisition/release are timed with prepared wrappers reused.
Sequential Release runs use one pinned CPU and codec/conversion thread, normal
SIMD, tiering off and workstation nonconcurrent GC. Startup, IPC, I/O, preparation
and checks are excluded. Native allocation is unmeasured; managed figures include
harness overhead. The default RGBA16 adaptive-encode regression screen passes
at **1.956×**, with unchanged packet and **297,772 B/op**; it does not certify
forward-only RGBA16 output.

Windows and Linux each pass **3,281 Release tests**, including **896 live FFmpeg
comparisons**, plus **456 focused checks per mode per OS**. Eleven new writer
checks compare every pending-bit position, stuffing, restart and exact work,
output, callback and cancellation failures against the checked emitter. Three
new public checks cover complete packet/pixels/source, exact output/workspace
limits, all AC symbols/reserved ones, and the unchanged legacy short-table decode
admission. Historical quality-report bytes remain immutable; all 160 current
fixed/adaptive quality profiles preserve their frozen quantization and pixels.
New nine/ten-bit AC codes require the decoder's existing additional 2 KiB prefix
lookup, explicitly accounted for without changing decoder admission policy.

The consumer and all three examples pass through the **same actual dependency-free
Release package** on both systems. The qualified Linux DLL equals the timed DLL.
The same final production/harness serves screen and confirmation; generated
drivers and exact-packet anchors are reused. A stale confirmation-child copy is
refused before timing, then corrected by copying the verified assemblies without
rebuilding them. Full qualification runs once per platform after passing means.
The [numeric evidence](warm-jpeg-fixed-geometric-evidence.json) retains all raw
identities, intermediate results, coefficient proof and package qualification.
Stop this pair; other material fallback/layout/public boundaries remain open.
