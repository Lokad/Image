# Current PNG decode and coding-validation costs

All **47 existing resident decode controls and four coding validators** have
independent evidence: **49 pass and stop, two uncertain**.
[Ordinary/slab publication](PNG-NORMAL-CURSOR-PUBLICATION.md) refreshes43 affected
controls and closes four former gaps. [Prepared live-run invariants](PNG-LIVE-RUN-PREPARATION.md)
now own the two materialized live costs, saving93.08µs/2.89%raw/1.86%normalized
on RGBA16 with separated confidence; its upper1.001253 remains uncertain. Byte
also stays uncertain. Six other unchanged paths reuse their explicit providers.
The separate two-retained-image shared-root operation also now passes.
Frozen workers, harnesses, fixtures and settings are reused; no new workload grid.

| Existing case | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| DecodeCancelable8 | 1,529.69 / 1,535.97 | 0.996 / 1.011744 | 4,472 | uncertain |
| DecodeTight8 | 1,184.46 / 1,395.39 | 0.849 / 0.855 | 68,224 | pass |
| DecodeTight16 | 2,826.77 / 2,894.02 | 0.977 / 0.981 | 70,272 | pass |
| ValidatePng8 | 1,169.54 / 1,380.85 | 0.847 / 0.857 | 240 | pass |
| ValidatePng16 | 2,768.71 / 2,962.79 | 0.934 / 0.938 | 240 | pass |
| ValidateIndexedAdam7 | 1.76 / 3.94 | 0.447 / 0.463 | 792 | pass |
| ValidatePng16Async | 2,759.14 / 3,612.00 | 0.764 / 0.765 | 8,544 | pass |
| Decode8 | 1,177.08 / 1,395.32 | 0.844 / 0.846 | 4,472 | pass |
| Decode16 | 2,827.31 / 2,897.12 | 0.976 / 0.978 | 8,568 | pass |
| DecodeAdam7_16 | 2,929.79 / 3,922.87 | 0.747 / 0.749 | 23,240 | pass |
| DecodePaeth8 | 2,596.55 / 2,845.71 | 0.912 / 0.917 | 4,472 | pass |
| DecodePaeth16 | 4,549.40 / 5,508.52 | 0.826 / 0.832 | 8,568 | pass |
| DecodeRgb8 | 952.16 / 1,528.39 | 0.623 / 0.625 | 3,448 | pass |
| DecodeRgb16 | 2,120.01 / 2,229.39 | 0.951 / 0.957 | 6,520 | pass |
| DecodeGray8 | 297.48 / 361.84 | 0.822 / 0.828 | 1,400 | pass |
| DecodeGray16 | 725.93 / 958.81 | 0.757 / 0.760 | 2,424 | pass |
| DecodeGrayAlpha8 | 646.19 / 1,159.00 | 0.558 / 0.559 | 2,424 | pass |
| DecodeGrayAlpha16 | 1,455.73 / 1,800.48 | 0.809 / 0.812 | 4,472 | pass |
| DecodeUp8 | 995.15 / 1,517.87 | 0.656 / 0.658 | 4,472 | pass |
| DecodeUp16 | 1,368.23 / 1,909.14 | 0.717 / 0.720 | 8,568 | pass |
| DecodeSub8 | 466.63 / 682.41 | 0.684 / 0.688 | 4,472 | pass |
| DecodeSub16 | 271.06 / 742.62 | 0.365 / 0.378 | 8,568 | pass |
| DecodeAverage8 | 1,470.63 / 1,851.85 | 0.794 / 0.797 | 4,472 | pass |
| DecodeAverage16 | 3,438.64 / 4,478.17 | 0.768 / 0.771 | 8,568 | pass |
| DecodeCancelable16 | 3,126.33 / 3,153.78 | 0.991 / 1.001253 | 8,568 | uncertain |
| DecodeTightCancelable16 | 2,789.14 / 2,893.47 | 0.964 / 0.976 | 70,272 | pass |
| DecodeNoCompression8 | 92.78 / 135.99 | 0.682 / 0.686 | 4,472 | pass |
| DecodeNoCompression16 | 208.13 / 296.12 | 0.703 / 0.708 | 8,568 | pass |
| DecodeTightCancelable8 | 1,181.56 / 1,395.83 | 0.846 / 0.851 | 68,224 | pass |
| DecodeGray1 | 48.24 / 308.58 | 0.156 / 0.158 | 504 | pass |
| DecodeGray2 | 55.89 / 106.54 | 0.525 / 0.531 | 632 | pass |
| DecodeGray4 | 79.72 / 172.65 | 0.462 / 0.465 | 888 | pass |
| DecodePalette1 | 42.92 / 83.71 | 0.513 / 0.515 | 568 | pass |
| DecodePalette2 | 50.75 / 97.89 | 0.518 / 0.528 | 704 | pass |
| DecodePalette4 | 72.16 / 137.17 | 0.526 / 0.530 | 1,000 | pass |
| DecodePalette8 | 291.03 / 365.30 | 0.797 / 0.806 | 2,472 | pass |
| DecodeGray1Adam7 | 253.99 / 1,158.69 | 0.219 / 0.222 | 1,440 | pass |
| DecodeGrayAlphaPaeth8 | 1,718.20 / 2,007.33 | 0.856 / 0.867 | 2,424 | pass |
| DecodeGrayPaeth8 | 920.63 / 1,027.51 | 0.896 / 0.900 | 1,400 | pass |
| DecodeRgbPaeth16 | 3,636.13 / 4,465.35 | 0.814 / 0.819 | 6,520 | pass |
| DecodeRgbPaeth8 | 2,155.95 / 2,627.73 | 0.820 / 0.823 | 3,448 | pass |
| DecodePalette4Adam7 | 493.33 / 1,133.13 | 0.435 / 0.437 | 2,608 | pass |
| DecodePalette8Adam7 | 779.66 / 1,336.12 | 0.584 / 0.590 | 4,976 | pass |
| DecodePalette8PartialAlpha | 303.65 / 365.36 | 0.831 / 0.846 | 2,224 | pass |
| DecodePalette8Short | 66.73 / 110.58 | 0.603 / 0.609 | 1,512 | pass |
| DecodeGray1Transparency | 51.90 / 690.15 | 0.075 / 0.077 | 536 | pass |
| DecodeGray2Transparency | 55.62 / 498.38 | 0.112 / 0.112 | 664 | pass |
| DecodeGray16Transparency | 720.33 / 2,052.70 | 0.351 / 0.352 | 2,456 | pass |
| DecodeGray8Transparency | 296.27 / 757.27 | 0.391 / 0.394 | 1,432 | pass |
| DecodeRgb16Transparency | 2,158.25 / 3,181.97 | 0.678 / 0.680 | 6,552 | pass |
| DecodeRgb8Transparency | 985.47 / 1,281.73 | 0.769 / 0.773 | 3,480 | pass |

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

DLL `23d65df3…` supplies43 source-valid costs; `8e44e29f…` supplies the refreshed
materialized-live pair and `05f30275…` the six unchanged stored/counting/BCL cases.
The [scope audit and evidence](png-normal-cursor-publication-evidence.json) identify
each earlier provider and routing reason. Current qualification passes6,351
full/live checks per OS,712internal/246public PNG/admission cases in normal/scalar
modes, and SAME actual Release-package/all three examples on Windows/Linux.
Other source-valid ISA evidence is reused. Passing cases stop. No broader speedup follows
from refreshing costs; grayscale16, for example, is effectively unchanged.

B/op measures total managed allocation, separately from peak live capacity and
retained storage. All51 allocation counts are unchanged; framing admission remains
4KiB with no new owned capacity or retained scratch. Physical/native peaks are
unmeasured. The two-image shared-root operation retains its own peak/output-pair
figures in the new report. Older stream/composition costs remain historical;
prefix/Identify and encoding do not enter the changed cursor. This inventory does
not establish all PNG or289-path parity.

[Live-run preparation](PNG-LIVE-RUN-PREPARATION.md) owns the two current live costs
and latest package qualification; the other49 costs remain source-valid. Earlier
[framing evidence](png-current-framing-costs-evidence.json),
[live evidence](png-live-prepared-cursor-evidence.json) and their raw archives retain
superseded measurements, adverse results and reproducibility. The new mirrored,
member-verified archive and its dependencies are identified in
[current live evidence](png-live-run-preparation-evidence.json) and
[ordinary cursor evidence](png-normal-cursor-publication-evidence.json). Publication remains
postponed; uncertainty is resolved before assuming further optimization.
