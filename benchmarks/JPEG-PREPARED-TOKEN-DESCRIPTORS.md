# Prepared JPEG token descriptors

Complete direct RGB444 decode saves **35.00 µs / 2.18% raw /
2.87% reference-normalized**,1,606.03→1,571.03 µs. Filtered/all-actual
raw/normalized confidence intervals separate. The standalone reference comparison
remains uncertain:1,571.03/native1,563.64 µs, upper1.013,42,936 B/op.

All 23 affected baseline costs refresh on the actual final package DLL; six unchanged
progressive controls reuse source-valid evidence. Current 29 reads: **22 passes / 4 gaps /
3 uncertain**. Passing cases stop. Gaps are direct RGB420/RGB422, sparse CMYK and
caller CMYK; RGB444, horizontal CMYK and progressive live444 need confidence
resolution before further tuning. Companion points prove no causal gain,
regression or equivalence. Complete encoder packets and metadata are unchanged.

## Preparation and execution

Existing scan-owned 4,096×UInt32 tables keep pair priority and populate unused
entries with decoded single-coefficient or EOB descriptors. Short singles reuse
the first signed-byte store; their flag distinguishes continuation from paired
EOB. Wide singles/EOB use reserved next-offset 63 so the existing pair-boundary
guard routes them away from the common pair path. Both plain/cancelable readers
retain exact coefficients, cursor/following reads, partial failures and checkpoints.

The first layout adds a tag branch to every pair. Its RGB/sparse screens improve
only 19.5/6.9 µs with broad intervals; no gain is proven. The revised layout removes
that dispatch from pair hits. Its first independent RGB confirmation has two slow
actuals 1,640.43/1,626.87 µs and broad all-actual confidence despite stable native
brackets. Exactly one targeted unchanged-build check resolves the measurement;
all earlier actuals remain. No further unchanged favorable sampling follows.

The [independent canonical model](JPEG-BASELINE-TOKEN-PREFIX-ATTRIBUTION.md) checks
two existing packets, including final padding/EOI. Modeled eligible replacements
are 16,943 RGB short singles/2,788 EOB and 8,579 sparse EOB. These ignore actual
window/refill/stuffing handoffs and are not execution counts or phase costs.
No fixture-specific logic, new input/settings grid, table eligibility or cache API.

Capacity remains 16,408 B per prepared table; there is **no added owned capacity**.
Total GC remains 42,936 B/op for RGB and 20,944 for sparse CMYK. Independent RGBA
output retains 531,468 B until disposal; operation scratch retention 0. Minor
allocation-point differences elsewhere are amortization, not claimed savings.
Physical/native/JIT-stack peaks remain unmeasured. Shared-root refusal remains
terminal, with reservation before allocation and scan release after actual users.

Five new checks cover all prefixes/signs/pair priority and actual plain/cancelable
run 0/15 category 10 fields, last coefficients, all bit phases, stuffing, truncation,
following reads, partial failures and canaries. Existing admission/cancellation/
release/recovery checks remain. A test-only expansion reused both production DLLs.
Other reader methods, eligibility/release, decoder scheduling and inverse sources
remain unchanged; six progressive paths reuse valid evidence.

## Current complete controls

| Frozen public decode/validation control | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| DirectRgb444 | 1,571.03 | 1,563.64 | 1.005 / 1.013 | 42,936 | uncertain |
| Decode8 | 1,754.55 | 2,037.01 | 0.861 / 0.870 | 4,320 | pass |
| NativeDecode8 | 1,570.70 | 1,725.02 | 0.911 / 0.946 | 42,936 | pass |
| NativeDecode420 | 906.60 | 1,085.68 | 0.835 / 0.843 | 258,446 | pass |
| NativeDecode422 | 1,215.15 | 1,456.09 | 0.835 / 0.844 | 330,253 | pass |
| OwnedDecodeGray | 608.38 | 680.71 | 0.894 / 0.910 | 140,455 | pass |
| ProgressiveGray (reused) | 1,630.45 | 1,711.66 | 0.953 / 0.964 | 151,432 | pass |
| Progressive420 (reused) | 1,559.16 | 1,948.18 | 0.800 / 0.809 | 243,664 | pass |
| Progressive444 (reused) | 3,172.92 | 3,355.21 | 0.946 / 0.949 | 169,664 | pass |
| Progressive422 (reused) | 2,201.41 | 2,439.31 | 0.902 / 0.913 | 315,496 | pass |
| NativeDecode420Restart | 964.77 | 1,015.01 | 0.951 / 0.963 | 262,824 | pass |
| StreamDecode420Restart | 962.59 | 991.30 | 0.971 / 0.977 | 262,920 | pass |
| NativeDecodeCancelable8 | 1,682.76 | 1,848.40 | 0.910 / 0.932 | 42,936 | pass |
| ProgressiveCancelable444 (reused) | 3,338.76 | 3,355.58 | 0.995 / 1.012 | 169,664 | uncertain |
| DirectRgb422 | 1,106.97 | 1,098.15 | 1.008 / 1.012 | 330,260 | gap |
| DirectRgb420 | 779.06 | 743.31 | 1.048 / 1.073 | 258,452 | gap |
| CmykAdobe | 324.37 | 379.40 | 0.855 / 0.861 | 4,536 | pass |
| CmykAdobeAc | 428.38 | 397.03 | 1.079 / 1.098 | 20,944 | uncertain |
| CmykAdobeGeneral | 1,387.46 | 3,110.83 | 0.446 / 0.449 | 20,944 | pass |
| CmykAdobeSparse2D | 484.96 | 437.68 | 1.108 / 1.121 | 20,944 | gap |
| Ycck420 | 458.05 | 756.80 | 0.605 / 0.612 | 895,562 | pass |
| CmykCaller | 365.86 | 353.37 | 1.035 / 1.047 | 1,085,975 | gap |
| Ycck420CallerRgb | 489.76 | 646.09 | 0.758 / 0.764 | 762,717 | pass |
| Decode411 | 388.42 | 477.04 | 0.814 / 0.884 | 219,552 | pass |
| Decode410 | 346.77 | 508.74 | 0.682 / 0.703 | 189,091 | pass |
| CallerGray | 660.58 | 698.76 | 0.945 / 0.958 | 273,439 | pass |
| CallerRgb | 964.47 | 1,158.03 | 0.833 / 0.847 | 657,359 | pass |
| ValidateJpegBaseline420 | 483.06 | 674.27 | 0.716 / 0.720 | 10,016 | pass |
| ValidateJpegProgressive420 (reused) | 288.75 | 297.28 | 0.971 / 0.984 | 4,088 | pass |

Complete boundaries retain parsing/preparation, entropy, inverse/color,
normalization, required input delivery, fresh independent output and disposal;
caller samples and validation keep their declared output boundaries. Native
workers retain documented codec contexts. All frozen input/output/native pixel,
source/ownership and scoped YCCK bounds pass, one CPU/quiet idle-v2.
Existing qualified fixture projections are reused unchanged; only read-proof
producer identity is rebound after current full checks. Every original file is
byte-restored in finally. No new expected pixels, quality gate or reference setting.

Windows/Linux each pass **6,172 full/live checks**, 458 internal/375 public JPEG per
normal/scalar/disabled-AVX2/AVX512 mode, and the **same actual Release package/all
three examples**. Unchanged resize/codec/editing evidence is reused. Two coherent
main/three embedded versions per OS; final broad/package qualification once per OS;
zero harness/native/selector/fixture builds. This is not a whole-release refresh.

[Evidence](jpeg-prepared-token-descriptors-evidence.json) owns all29 costs, reuse,
source boundaries, stages and qualification. Mirrored/member-verified raw
`jpeg-prepared-token-descriptors.tar.xz`: 10,491,112 B / 503 members, SHA256
`bcb777111e6a2586d1e2a1d9bb202efc5424e05730e6bbb7eb78e94c8281b2d5`. Private 0.2.1 package: 443,536 B, SHA256
`f688d9232f00a0661fec1fd454198b0708468259de849ca809671b85308157dd`. Published packages remain immutable; publication is postponed.
Earlier [prepared Huffman](JPEG-LIVE-PREPARED-HUFFMAN.md) and
[quarter interpolation](JPEG-QUARTER-INTERPOLATION.md) retain history, without
supplying superseded costs to the renderer. All 283 required paths remain.
