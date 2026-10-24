# Bounded JPEG MCU conversion and integer transform

One existing513×259 RGBA8 4:2:0 control, unchanged quality85/native qscale5,
adaptive Huffman and background0x204060. Complete packets remain52,091/52,093 B;
only the approved attribution COM differs. Source, normalized samples, quality,
alpha and independent output ownership checks pass before/after each run.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| short | 13,483.02 | 1,763.18 | 7.647/7.893 | 372,817 |
| confirm | 13,495.10 | 1,783.59 | 7.566/7.611 | 372,817 |

Independent confirmation is 44.5% faster than the24,301.44-µs E116 screen,
with unchanged heap allocation. Full CPU parity remains open. This is one named
control; no general JPEG speed claim or equal-quality-number assumption follows.

Area plans are reused per MCU/row. A bounded RGB tile eliminates repeated pixel
normalization; exact SSE2/AVX2 forward transforms retain the scalar fallback.
Quantization reciprocals are prepared once. All scratch fits the existing8-KiB
owner, with unchanged work admission and no new heap/frame cache.

Both platforms pass5,036 checks each, including947 live comparisons; affected
scalar tests and SSE2-only coefficient checks pass. The same actual private0.2.0
package and all three usage examples pass on both platforms. Other production
paths and their valid evidence are unchanged. Only this combined producer has an
independent timing confirmation; intermediate screens are diagnostic.

See [evidence](warm-jpeg-mcu-evidence.json) for raw samples, build identities,
qualification, archived diagnostics and precise reuse/invalidation scope.
