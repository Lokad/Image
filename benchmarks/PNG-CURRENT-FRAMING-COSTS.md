# Current PNG decode and coding-validation costs

All **47 existing resident decode controls and four coding validators** now have
independent evidence: **45 pass and stop, 6 gaps, 0 uncertain**.
The final 27 missing confirmations reuse the prepared immutable build, worker,
harness and fixtures. No production change, rebuild, extra image/settings grid,
repeated passing measurement or repeated package qualification.

| Existing case | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| DecodeCancelable8 | 1,539.10 / 1,394.18 | 1.104 / 1.113 | 4,472 | gap |
| DecodeTight8 | 1,297.38 / 1,395.09 | 0.930 / 0.935 | 68,224 | pass |
| DecodeTight16 | 3,019.22 / 2,895.06 | 1.043 / 1.052 | 70,272 | gap |
| ValidatePng8 | 1,287.37 / 1,377.05 | 0.935 / 0.938 | 240 | pass |
| ValidatePng16 | 2,906.21 / 2,961.99 | 0.981 / 0.983 | 240 | pass |
| ValidateIndexedAdam7 | 1.76 / 3.94 | 0.447 / 0.463 | 792 | pass |
| ValidatePng16Async | 2,956.81 / 3,613.24 | 0.818 / 0.826 | 8,544 | pass |
| Decode8 | 1,307.81 / 1,393.64 | 0.938 / 0.944 | 4,472 | pass |
| Decode16 | 2,952.03 / 2,898.14 | 1.019 / 1.031 | 8,568 | gap |
| DecodeAdam7_16 | 3,062.16 / 3,927.59 | 0.780 / 0.785 | 23,240 | pass |
| DecodePaeth8 | 2,803.38 / 2,843.39 | 0.986 / 0.989 | 4,472 | pass |
| DecodePaeth16 | 4,826.89 / 5,512.13 | 0.876 / 0.885 | 8,568 | pass |
| DecodeRgb8 | 1,129.16 / 1,526.29 | 0.740 / 0.752 | 3,448 | pass |
| DecodeRgb16 | 2,336.31 / 2,221.40 | 1.052 / 1.062 | 6,520 | gap |
| DecodeGray8 | 344.42 / 367.29 | 0.938 / 0.949 | 1,400 | pass |
| DecodeGray16 | 725.86 / 958.75 | 0.757 / 0.761 | 2,424 | pass |
| DecodeGrayAlpha8 | 764.32 / 1,158.31 | 0.660 / 0.661 | 2,424 | pass |
| DecodeGrayAlpha16 | 1,563.99 / 1,798.70 | 0.870 / 0.877 | 4,472 | pass |
| DecodeUp8 | 1,123.47 / 1,508.68 | 0.745 / 0.750 | 4,472 | pass |
| DecodeUp16 | 1,533.72 / 1,910.94 | 0.803 / 0.807 | 8,568 | pass |
| DecodeSub8 | 517.24 / 685.78 | 0.754 / 0.759 | 4,472 | pass |
| DecodeSub16 | 301.91 / 741.90 | 0.407 / 0.412 | 8,568 | pass |
| DecodeAverage8 | 1,619.39 / 1,849.21 | 0.876 / 0.879 | 4,472 | pass |
| DecodeAverage16 | 4,102.68 / 4,476.03 | 0.917 / 0.920 | 8,568 | pass |
| DecodeCancelable16 | 2,950.95 / 2,895.32 | 1.019 / 1.025 | 8,568 | gap |
| DecodeTightCancelable16 | 3,005.70 / 2,900.91 | 1.036 / 1.041 | 70,272 | gap |
| DecodeNoCompression8 | 92.78 / 135.99 | 0.682 / 0.686 | 4,472 | pass |
| DecodeNoCompression16 | 208.13 / 296.12 | 0.703 / 0.708 | 8,568 | pass |
| DecodeTightCancelable8 | 1,315.05 / 1,392.42 | 0.944 / 0.951 | 68,224 | pass |
| DecodeGray1 | 54.78 / 308.41 | 0.178 / 0.181 | 504 | pass |
| DecodeGray2 | 64.14 / 107.30 | 0.598 / 0.605 | 632 | pass |
| DecodeGray4 | 95.91 / 171.17 | 0.560 / 0.570 | 888 | pass |
| DecodePalette1 | 50.69 / 83.50 | 0.607 / 0.610 | 568 | pass |
| DecodePalette2 | 60.06 / 98.17 | 0.612 / 0.615 | 704 | pass |
| DecodePalette4 | 91.47 / 141.39 | 0.647 / 0.664 | 1,000 | pass |
| DecodePalette8 | 338.37 / 362.85 | 0.933 / 0.939 | 2,472 | pass |
| DecodeGray1Adam7 | 253.99 / 1,158.69 | 0.219 / 0.222 | 1,440 | pass |
| DecodeGrayAlphaPaeth8 | 1,807.90 / 2,002.94 | 0.903 / 0.909 | 2,424 | pass |
| DecodeGrayPaeth8 | 956.41 / 1,029.75 | 0.929 / 0.932 | 1,400 | pass |
| DecodeRgbPaeth16 | 3,913.69 / 4,463.85 | 0.877 / 0.880 | 6,520 | pass |
| DecodeRgbPaeth8 | 2,356.48 / 2,627.15 | 0.897 / 0.906 | 3,448 | pass |
| DecodePalette4Adam7 | 493.33 / 1,133.13 | 0.435 / 0.437 | 2,608 | pass |
| DecodePalette8Adam7 | 779.66 / 1,336.12 | 0.584 / 0.590 | 4,976 | pass |
| DecodePalette8PartialAlpha | 351.76 / 365.63 | 0.962 / 0.969 | 2,224 | pass |
| DecodePalette8Short | 76.82 / 109.89 | 0.699 / 0.705 | 1,512 | pass |
| DecodeGray1Transparency | 59.80 / 691.10 | 0.087 / 0.088 | 536 | pass |
| DecodeGray2Transparency | 64.46 / 499.54 | 0.129 / 0.130 | 664 | pass |
| DecodeGray16Transparency | 732.70 / 2,054.27 | 0.357 / 0.357 | 2,456 | pass |
| DecodeGray8Transparency | 352.90 / 760.86 | 0.464 / 0.473 | 1,432 | pass |
| DecodeRgb16Transparency | 2,259.63 / 3,180.54 | 0.710 / 0.713 | 6,552 | pass |
| DecodeRgb8Transparency | 1,117.12 / 1,283.24 | 0.871 / 0.873 | 3,480 | pass |

Each control retains twelve actuals, twelve samples in each native bracket,
filtered/all-actual confidence and idle-v2 guards on one pinned CPU. Inputs,
sampling/filter/transparency settings and native output are frozen. Decode includes
parsing, preparation, workspace, full-depth RGBA normalization, independent output
and disposal. Live cancellation and exact tight-memory ceilings remain. Validation
checks complete coding and returns metadata without RGBA normalization.

Decode samples, alpha/hidden RGB, normal/scalar native CLI output, source
immutability and independent output ownership pass their original gates. Coding
validators retain exact metadata and prefix/coding scope checks. The resident
decode worker `cfb2403d…`, source `a75eaefa…`, compiler/static archives and settings
remain unchanged in the prepared isolated root; the four coding validators keep
their separate frozen natural-frame worker and metadata adapter. The existing one-bit grayscale transparency
control includes its declared native filter/one filter thread; other controls retain
their declared conversions and threading. Native contexts/wrappers remain prepared.

Measurement DLL `05f30275…` is unchanged from the qualified PNG framing milestone.
Production changes through `172de02d` affect other codecs, fonts and bicubic resizing;
PNG decode and shared ownership paths are unchanged. Reuse its Windows/Linux
5,751-check/scalar/same-package proof. The current library also passes5,850 full/live
checks and the same actual package on both OSes; this update adds no production or
package changes. These are source-valid reused proofs, not new runs on all DLLs.

B/op measures total managed allocation, separately from peak live capacity and
retained storage. Complete physical/native peaks are unmeasured. The separate
[shared-root operation](PNG-PREPARED-PREFIX.md) retains its own paired gain and
capacity evidence; do not transfer its figures to these resident controls. Stream,
composition, prefix/Identify and shared-root boundaries keep their own finite
coverage. This closes missing resident confirmations, not all PNG or283-path parity.

Historical costs and adverse results remain in [evidence](png-current-framing-costs-evidence.json)
and its linked raw archives; they are not paired causal effects of this measurement
update. Passing cases stop; uncertain results are not demonstrated gaps. The earlier
framing/source reports remain in Git history and the recorded measurement rounds.

The final batch raw `png-remaining-current-costs.tar.xz`, SHA256
`6f763998fe9c148045fa65278e25bb74edc9cfacb0674f4c66973acca8b837e3`, 5512732B/338members,
is mirrored/member-verified with frozen input and tool providers. Earlier rounds
retain their original raw identities. Publication remains postponed.
