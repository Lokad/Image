# Exact all-width WebP cache histograms

Existing 513×259 RGBA8 controls retain frozen inputs and reference settings.
VP8 uses FFmpeg Q75/method4 and compressed alpha; VP8L uses the approved
**libwebp exact=1, FFmpeg's encoder backend**. Complete normalization, encoding,
fresh independent output acquisition and release are measured on one CPU.
Packets remain raw-exact: 98,590 B and 183,812 B respectively.

| Quiet VM run | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| VP8 short | 106,306.69 | 53,381.75 | 1.991/2.208 | 24,424,952 |
| VP8 independent | 106,690.16 | 53,676.58 | 1.988/1.998 | 24,424,952 |
| VP8L current short | 215,589.68 | 122,725.26 | 1.757/2.462 | 48,232,256 |

VP8 independent confirmation saves 5.6% elapsed against
[the preceding confirmation](WARM-VP8-VECTOR-TEXTURE.md). Both idle-v2 guards
pass. The VP8L short reconciles affected costs; it establishes no independent
speedup claim. CPU parity remains open.

Literal-channel histograms use exact cache-hit thresholds and suffix sums;
copy-length populations are computed once across widths. All 24,558 frozen
histograms, costs, winners and logical work remain exact. The all-width workspace
adds 47,608 admitted bytes at ten cache bits, with 44 stack bytes for offsets.
Full-operation allocation increases by 95,216 B for VP8 and 47,608 B for VP8L.
The timed workers do not separately measure shared-root peak. Real-root tests
verify capacity, aggregate refusal, cleanup and independent recovery.

Both platforms pass 5,127 checks each, including 947 live comparisons, 15 scalar
history/parsing checks and 56 scalar public WebP checks. The same actual private
0.2.0 Release package and all three examples pass both platforms. Existing proof
controls were adapted; no image/settings grid or benchmark/native/host build.

[Evidence](warm-webp-all-width-histograms-evidence.json) identifies producers,
costs and reuse. Other affected VP8/VP8L CPU observations stay historical.
PNG, JPEG, decoding, editing and text reuse unchanged evidence. Governed JPEG
now has the largest current confirmed encoder gap; full 257-case/1× and
replacement readiness remain open.
