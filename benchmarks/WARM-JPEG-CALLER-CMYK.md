# Caller CMYK JPEG decoding

Current costs are superseded by [SIMD dequantization](WARM-JPEG-DEQUANTIZATION.md).
Original evidence and accepted behavior remain retained.

The first short screen demonstrates an **8.522x** gap against FFmpeg.
Both decode the same10,895-byte original513x259 untagged CMYK packet into
531,468bytes of fresh caller-owned canonical ink samples. All four channels,
including varying black, are exact against the independent block-mean reference.

| Complete resident JPEG → caller CMYK8 | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: |
| Original canonical CMYK projection | 2,817.13 / 330.56 | 8.522 / 8.986 | 1,635,373 |

The untagged T.81 DC fixture complements the existing original inverted component
means to canonical ink; all four full-resolution planes vary. Both implementations
receive the identical packet. FFmpeg's public YUVA444P frame holds raw C/M/Y/K
planes here, without color conversion, black multiplication or alpha replacement.
The original adapter packs those components directly into fresh zeroed caller
storage. Decode/reset, component normalization/packing and plane/output release
are timed; context, packet and empty wrappers may be reused. No RGBA intermediary,
header patch, altered native packet, ICC conversion or EXIF reorientation.

Normal/scalar public CLI bytes and the worker agree exactly. Source, padded rows,
independent surviving caller buffers, native live owners and one CPU/thread pass.
The precise timing scope is untagged canonical CMYK; Adobe inversion remains
correctness-covered. Three200ms actuals,50ms native pre/post brackets and both
idle-v2 guards are retained. Above-one mean receives no long confirmation.

One six-second profile has5,788samples:66.0% in scalar rounding, interpolation,
CMYK sample extraction and dispatch;19.7% in the general inverse transform.
Prioritize one substantial packing primitive. The profile supplies no timing claim.

Production is unchanged. Reuse4,202tests/OS,66hardware-off checks/OS and the same
actual private package consumer/all three examples on both OS. One narrow harness
build/OS and one native worker build; no producer/selector build or repeated full
qualification. All223previous numeric rows stay identical and all70passes stop.
Finite scope remains246:224numeric+22unmeasured;153mean gaps/one confidence-open.

[Numeric evidence](warm-jpeg-caller-cmyk-evidence.json). Raw on both hosts:

- `h10q-measurements.tar.gz` SHA`ee50830d32304c7dca1364fdeef61de8fd3535a6935d9eda1b0e8ada21132aa9`.

- `h10q-profile.tar.gz` SHA`41830f00c49ef1e19738e0cb4ac1141864b23d247c9502b8cb4ca42d7f3653a0`.
