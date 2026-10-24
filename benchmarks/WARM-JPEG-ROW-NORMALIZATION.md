# JPEG byte-row normalization

One existing513×259 RGBA8 JPEG420 control, unchanged quality85/native qscale5,
adaptive Huffman, background0x204060 and reference inputs/settings. Complete
packets remain52,091/52,093 B, allowing only the approved attribution COM.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| short | 11,345.66 | 1,768.34 | 6.416/6.715 | 372,808 |
| confirm | 11,388.76 | 1,767.95 | 6.442/6.523 | 372,808 |

Independent confirmation reduces elapsed time by 15.6% against the
[previous13,495.10-µs result](WARM-JPEG-MCU.md). Full CPU parity remains open.
All12 actual samples and11 filtered samples contribute conservative bounds.
No new image/settings controls or equal-quality-number assumption are used.

Each MCU row is acquired once; safe SIMD flattens and packs four pixels with
exact divide255 rounding. Scalar tails/fallback and unchanged16-bit/caller paths
retain their contracts. No new heap storage or admission allowance is needed.

Both platforms pass5,043 checks each, including947 live comparisons, affected
scalar tests, exhaustive byte/alpha/background arithmetic and unaligned/tail
controls. The same actual private0.2.0 package and all three examples pass on
both platforms. Unchanged primitives retain their prior valid evidence.

[Evidence](warm-jpeg-row-normalization-evidence.json) records original samples,
allocation/build identities, qualification, precise scope and raw preservation.
