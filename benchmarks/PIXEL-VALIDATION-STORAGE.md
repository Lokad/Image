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
Normal/scalar records agree on Windows and the Linux VM.

| Control | Encoded input B | Decode allocated B | Validate allocated B | Decode admitted peak B | Validate admitted peak B |
| --- | ---: | ---: | ---: | ---: | ---: |
| PNG8 RGBA | 210,972 | 1,278,440 | 223,568 | 1,547,917 | 484,722 |
| PNG16 RGBA | 963,200 | 3,095,504 | 977,696 | 3,364,989 | 1,238,858 |
| JPEG baseline420 | 65,163 | 1,619,240 | 872,096 | 1,014,706 | 871,090 |
| WebP VP8L + alpha | 183,310 | 1,217,192 | 685,544 | 1,063,480 | 623,292 |
| WebP VP8 + compressed ALPH | 115,542 | 1,966,392 | 770,256 | 1,411,931 | 751,800 |

Borrowed input is excluded from shared peaks but counts toward local working limits.
PNG8/16 validation also succeeds under bounds that refuse the unallocated decoder
raster. Every operation refunds owned charges; returned scalar metadata survives.

Both OSes pass 5,037 Release tests, including 947 live FFmpeg comparisons, plus
66 targeted scalar checks. The same actual private 0.2.1 package and three usage
examples pass on both. Coverage includes existing legal PNG/Adam7 controls,
baseline/progressive/CMYK/YCCK/411/410 JPEG, VP8L transforms/history, raw/compressed
alpha, malformed coding, metadata policy, cancellation and shared refusal.

[Evidence](pixel-validation-storage-evidence.json) records packet hashes, exact
producers, package identity and the mirrored, member-verified raw archive.
Reproduce checks with `./test.ps1 -Differential` and filter public tests by
`FullyQualifiedName~PixelValidation`; no elapsed-time measurements are implied.
