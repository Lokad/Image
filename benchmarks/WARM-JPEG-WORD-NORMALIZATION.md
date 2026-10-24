# Bounded JPEG word-row normalization

The existing513×259 RGBA16 JPEG420 control retains quality85/native qscale5,
adaptive Huffman, background0x204060 and frozen inputs/settings. Complete packets
remain52,115/52,117 B, allowing only the approved attribution COM replacement.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| short | 9,882.67 | 1,839.94 | 5.371/5.860 | 372,808 |
| confirm | 9,874.51 | 1,841.22 | 5.363/5.385 | 372,808 |

Independent confirmation saves13.6% against the
[previous11,434.83-µs screen](WARM-JPEG-SEPARABLE-QUANTIZATION.md).
Both versioned idle guards pass. Complete operations include full16-bit
flattening/normalization, encoding, fresh owned output and release. CPU parity
remains open; this result makes no claim for other content or settings.

Each bounded source row is acquired once into the existing MCU tile. Shared
alpha products retain exact single rounding without additional storage.
The8-KiB workspace, local/work limits, admission and ownership stay unchanged.
An exact vector candidate added only about3.6% and was removed to prioritize
larger duplicated-pass work.

Both platforms pass5,049 checks each, including947 live comparisons and focused
scalar checks. New word controls compare15,728,640 channels and test unaligned
mixed-alpha tails, guards, input ownership and explicit background requirements.
The same actual private0.2.0 package and all three usage examples pass on both.
The immutable benchmark, native worker and archived hosts are reused.

[Evidence](warm-jpeg-word-normalization-evidence.json) records identities,
samples, qualification, diagnostics and verified storage retirement. Source8,
callerRGB8, grayscale,444 and other codec/editing/text paths remain unchanged;
their relevant confirmations are reused.
