# Current PNG decode and coding-validation costs

[Block Adler sums](PNG-BLOCK-ADLER.md) now owns all **51 existing controls**:
**49 pass and two uncertain**. Live byte and tight-memory cancelable word remain
uncertain; live word now passes. Original inputs, full-depth pixels, metadata,
hidden RGB, independent outputs, cancellation and frozen native gates remain.
These are complete operations with the recorded retained native contexts.
No workload grid or additional source image is added.

| Existing case | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| DecodeCancelable8 | 1,499.29 / 1,505.44 | 0.996 / 1.010 | 4,472 | uncertain |
| DecodeTight8 | 1,283.58 / 1,506.58 | 0.852 / 0.861 | 68,224 | pass |
| DecodeTight16 | 3,052.97 / 3,142.15 | 0.972 / 0.989 | 70,272 | pass |
| ValidatePng8 | 1,276.83 / 1,437.38 | 0.888 / 0.935 | 240 | pass |
| ValidatePng16 | 2,972.99 / 3,204.75 | 0.928 / 0.932 | 240 | pass |
| ValidateIndexedAdam7 | 1.80 / 3.97 | 0.454 / 0.472 | 792 | pass |
| ValidatePng16Async | 2,748.50 / 3,615.58 | 0.760 / 0.763 | 8,544 | pass |
| Decode8 | 1,222.99 / 1,396.63 | 0.876 / 0.893 | 4,472 | pass |
| Decode16 | 2,993.24 / 3,116.95 | 0.960 / 0.993 | 8,568 | pass |
| DecodeAdam7_16 | 3,117.24 / 4,251.72 | 0.733 / 0.738 | 23,240 | pass |
| DecodePaeth8 | 2,578.85 / 2,846.40 | 0.906 / 0.936 | 4,472 | pass |
| DecodePaeth16 | 4,632.07 / 5,522.34 | 0.839 / 0.879 | 8,568 | pass |
| DecodeRgb8 | 1,022.33 / 1,650.23 | 0.620 / 0.622 | 3,448 | pass |
| DecodeRgb16 | 2,103.96 / 2,228.21 | 0.944 / 0.957 | 6,520 | pass |
| DecodeGray8 | 291.53 / 363.89 | 0.801 / 0.811 | 1,400 | pass |
| DecodeGray16 | 770.37 / 1,035.06 | 0.744 / 0.749 | 2,424 | pass |
| DecodeGrayAlpha8 | 655.17 / 1,163.63 | 0.563 / 0.591 | 2,424 | pass |
| DecodeGrayAlpha16 | 1,449.64 / 1,795.33 | 0.807 / 0.826 | 4,472 | pass |
| DecodeUp8 | 1,063.46 / 1,612.81 | 0.659 / 0.682 | 4,472 | pass |
| DecodeUp16 | 1,451.81 / 2,041.51 | 0.711 / 0.720 | 8,568 | pass |
| DecodeSub8 | 481.13 / 728.53 | 0.660 / 0.666 | 4,472 | pass |
| DecodeSub16 | 271.71 / 756.82 | 0.359 / 0.386 | 8,568 | pass |
| DecodeAverage8 | 1,461.76 / 1,849.03 | 0.791 / 0.807 | 4,472 | pass |
| DecodeAverage16 | 3,763.89 / 4,838.21 | 0.778 / 0.782 | 8,568 | pass |
| DecodeCancelable16 | 2,859.45 / 2,902.44 | 0.985 / 0.988 | 8,568 | pass |
| DecodeTightCancelable16 | 3,054.27 / 3,092.17 | 0.988 / 1.008 | 70,272 | uncertain |
| DecodeNoCompression8 | 84.72 / 136.59 | 0.620 / 0.647 | 4,472 | pass |
| DecodeNoCompression16 | 184.83 / 297.00 | 0.622 / 0.629 | 8,568 | pass |
| DecodeTightCancelable8 | 1,249.30 / 1,397.89 | 0.894 / 0.927 | 68,224 | pass |
| DecodeGray1 | 56.20 / 328.46 | 0.171 / 0.174 | 504 | pass |
| DecodeGray2 | 59.91 / 115.57 | 0.518 / 0.522 | 632 | pass |
| DecodeGray4 | 84.68 / 183.72 | 0.461 / 0.467 | 888 | pass |
| DecodePalette1 | 43.03 / 86.75 | 0.496 / 0.501 | 568 | pass |
| DecodePalette2 | 50.34 / 98.09 | 0.513 / 0.524 | 704 | pass |
| DecodePalette4 | 74.81 / 145.42 | 0.514 / 0.545 | 1,000 | pass |
| DecodePalette8 | 296.51 / 366.64 | 0.809 / 0.827 | 2,472 | pass |
| DecodeGray1Adam7 | 278.10 / 1,249.11 | 0.223 / 0.228 | 1,440 | pass |
| DecodeGrayAlphaPaeth8 | 1,928.43 / 2,170.76 | 0.888 / 0.894 | 2,424 | pass |
| DecodeGrayPaeth8 | 901.26 / 1,032.38 | 0.873 / 0.879 | 1,400 | pass |
| DecodeRgbPaeth16 | 3,539.15 / 4,467.94 | 0.792 / 0.796 | 6,520 | pass |
| DecodeRgbPaeth8 | 2,294.99 / 2,840.60 | 0.808 / 0.812 | 3,448 | pass |
| DecodePalette4Adam7 | 494.33 / 1,223.49 | 0.404 / 0.416 | 2,608 | pass |
| DecodePalette8Adam7 | 837.60 / 1,439.85 | 0.582 / 0.585 | 4,976 | pass |
| DecodePalette8PartialAlpha | 325.25 / 396.31 | 0.821 / 0.828 | 2,224 | pass |
| DecodePalette8Short | 68.97 / 120.06 | 0.574 / 0.584 | 1,512 | pass |
| DecodeGray1Transparency | 50.80 / 687.84 | 0.074 / 0.076 | 536 | pass |
| DecodeGray2Transparency | 60.32 / 541.04 | 0.111 / 0.112 | 664 | pass |
| DecodeGray16Transparency | 727.26 / 2,042.53 | 0.356 / 0.364 | 2,456 | pass |
| DecodeGray8Transparency | 319.47 / 820.42 | 0.389 / 0.393 | 1,432 | pass |
| DecodeRgb16Transparency | 2,281.19 / 3,445.71 | 0.662 / 0.666 | 6,552 | pass |
| DecodeRgb8Transparency | 943.84 / 1,284.04 | 0.735 / 0.741 | 3,480 | pass |

Every control retains12actuals and both12-sample native brackets, with
conservative filtered/all-actual confidence and quiet idle-v2 guards. Validators
return equivalent36-byte typed metadata after complete coding validation;
decodes include full RGBA normalization, ownership and disposal. The separate
two-retained-image shared-root operation also refreshes and passes.

Allocation/admission and zero retained operation scratch remain unchanged.
Physical/native/JIT peaks are unmeasured. After affected costs reconcile,
Windows/Linux each pass6,510full/live checks,703internal/246public PNG/admission
cases in normal/scalar/AVX512-off/AVX2-off modes, and the SAMEactual package/all
three usage examples. Reuse qualified unchanged other-family paths.

[The current owner](PNG-BLOCK-ADLER.md) retains source/module identities, all
comparisons, adverse evidence, raw archives and reproducibility. Earlier
[ordinary framing](PNG-NORMAL-CURSOR-PUBLICATION.md) and
[live preparation](PNG-LIVE-RUN-PREPARATION.md) remain implementation history;
their affected CPU costs are superseded. Historical stream/composition costs
already marked H remain historical. All289paths and publication postponement
remain; this does not establish complete PNG or release performance parity.
