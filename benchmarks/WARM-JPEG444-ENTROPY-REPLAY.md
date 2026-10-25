# Exact JPEG444 entropy replay

The existing 513×259 controls keep frozen inputs, 0x204060 flattening, native
qscale5 and managed reference quality85. Complete normalization/encoding/fresh
output acquisition and release use one CPU. RGBA8 includes a forward-only
wrapper; RGBA16 uses its existing resident-input/owned-output boundary.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| RGBA8 short | 5,076.15 | 3,773.16 | 1.345/1.508 | 820,131 |
| RGBA8 independent | 5,049.03 | 3,776.93 | 1.337/1.343 | 820,131 |
| RGBA16 short | 5,170.10 | 3,836.09 | 1.348/1.484 | 820,099 |

Independent confirmation saves **48.4% elapsed** against
[the preceding short](WARM-CURRENT-JPEG444-STREAM.md). All12actual/11selected
iterations and both idle-v2 guards pass; upper includes selected/unfiltered
confidence. Allocation differs by51B/op; no meaningful reduction is claimed.
Packets remain133,039/133,041B for8-bit and133,041/133,043B for16-bit, with only
the approved attribution COM difference. Source error/reconstructed pixels match.

Existing entropy records replace the second444 conversion/transform pass.
Every coefficient, histogram, MCU/work boundary and emitted field stays exact.
The cache adds storage: a shared-root audit peaks at3,085,708B forRGBA8,
3,617,176B forRGBA16 and2,952,841B forcallerRGB, including each source owner.
Each is2,328,960B above its prior audit, chiefly old/new growth overlap.
Governed storage bypasses pools. These are untimed memory audits; their
single-operation allocation figures are not the benchmark B/op above.

Declared local workspace must cover worst growth and futuretable peaks.
Tight local limits select recomputation before admission; shared refusal never
selects another path. Source storage stays charged until owner disposal;
refusal/cancellation release temporary charges and allow independent recovery.

Both platforms pass5,133 checks each, including947live comparisons, plus13
focused normal/scalar and148scalar publicJPEG checks. The same actual private
0.2.0 Release package/allthree examples passbothOS. Its consumer verifies4MiB
defaultcache and2MiB terminal-refusal/declared-tight recovery paths. The initial
consumer's smaller-budget failure and package are retained; production was
unchanged during that docs/consumer correction.

[Evidence](warm-jpeg444-entropy-replay-evidence.json) records identities and reuse.
Other affected444 CPU observations remain historical. Fixed/restart JPEG,
420/422/gray, decoding, PNG, WebP, editing and text retain valid evidence.
No image/settings controls were added. VP8 now leads current confirmed encoder
anchors; full257-case/1× and replacement readiness remain open.
