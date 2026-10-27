# Pixel-validation storage

`ValidatePixels` checks complete coding without retaining RGBA. PNG shares row,
filter, palette and checksum checks; JPEG shares entropy/table/progression parsing;
VP8 retains required planes and VP8L retains ARGB history/transforms. Final sample
conversion is omitted. Memory remains codec-dependent, rather than fixed-space.

The five existing `OriginalExif.Create(profile,513,259)` controls below compare
one borrowed encoded span to metadata, versus decode-and-dispose of an owned raster.
Input/options/root preparation is excluded; library cleanup and the test budget
adapter's scopes/reservations are included. Each path is warmed once. These are
functional allocation/admission observations, **not CPU timing or FFmpeg parity**.
Admitted peaks agree on Windows/Linux normal/scalar runs. PNG16's one Linux
normal allocation record is8,112B higher; the range below preserves that
variation without inferring its cause.

| Control | Encoded input B | Decode allocated B | Validate allocated B | Decode admitted peak B | Validate admitted peak B |
| --- | ---: | ---: | ---: | ---: | ---: |
| PNG8 RGBA | 210,972 | 1,278,440 | 276,456 | 1,547,917 | 279,914 |
| PNG16 RGBA | 963,200 | 3,095,504 | 1,026,488–1,034,600 | 3,364,989 | 1,029,946 |
| JPEG baseline420 | 65,163 | 1,619,240 | 872,096 | 1,014,706 | 871,090 |
| WebP VP8L + alpha | 183,310 | 1,217,192 | 685,544 | 1,063,480 | 623,292 |
| WebP VP8 + compressed ALPH | 115,542 | 1,966,392 | 770,256 | 1,411,931 | 751,800 |

Borrowed input is excluded from shared peaks but counts toward local working limits.
PNG8/16 validation also succeeds under bounds that refuse the unallocated decoder
raster. Every operation refunds owned charges; returned scalar metadata survives.

Both OSes pass 5,041 Release tests, including 947 live FFmpeg comparisons, plus
137 internal and 88 public scalar checks. The same actual private 0.2.1 package
and three usage examples pass on both. Coverage includes legal PNG/Adam7 controls,
baseline/progressive/CMYK/YCCK/411/410 JPEG, VP8L transforms/history, raw/compressed
alpha, malformed coding, metadata policy, cancellation and shared refusal.

[Current streaming evidence](warm-png-streaming-validation-evidence.json) records
all four sets of observations, packet/producer/package identities and the
mirrored raw master. Caller-budget allocation is higher than the
[initial observations](pixel-validation-storage-evidence.json), while admitted
PNG peaks decrease. Warm pooled timing allocation is a different boundary.
Reproduce checks with `./test.ps1 -Differential` and filter public tests by
`FullyQualifiedName~PixelValidation`; no elapsed-time measurements are implied.
