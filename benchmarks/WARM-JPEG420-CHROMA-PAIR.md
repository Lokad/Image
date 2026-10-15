# JPEG RGBA8: shared 4:2:0 chroma sampling

The complete adaptive JPEG encode now confirms **1.943× FFmpeg / conservative
upper 1.988×**, with **297,772 allocated B/op**, unchanged. The previous
independent upper was 2.014×. This fixed pair clears the target; uniform
current-source coverage remains open.

| Measurement | Lokad.Image µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Short screen | 3,402.69 | 1.929 / 2.022 | 297,772 |
| Independent confirmation | 3,434.93 | 1.943 / 1.988 | 297,772 |

The confirmation retains all twelve actuals, with eleven selected by
BenchmarkDotNet. The upper uses the more conservative filtered/all-actual
interval against the fastest native bracket's lower bound. Both quiet guards
pass. The short screen alone does not establish the conservative bound.

One primitive shares RGBA8 unpacking and exact alpha flattening between Cb and
Cr on AVX-512/AVX2. A charged, optional 64-byte stack block holds Cr through the
Cb transform. Insufficient admission retains the original path. Binary64
arithmetic and rounding order, component order, work charges, coefficients,
amplitudes and the entire 74,040-byte adaptive packet remain unchanged. Padding,
RGBA16, other sampling and unsupported processors retain their existing paths.

The existing authored 513×259 raster, quality 69/native qscale 5, and background
RGB 0x204060 stay fixed. The source-quality gap is exactly 0.106110 dB within
the unchanged 0.25 dB gate. This is a source-quality timing comparison;
compression efficiency parity is not established. Source, alpha, geometry,
decoder agreement and independent owned outputs pass. No additional performance
raster or quality sweep is introduced.

Adjacent short regression screens pass at **1.954×** for RGBA16 4:2:0
(297,772 B/op) and **1.896×** for RGBA8 4:2:2 (294,110 B/op). These three-actual
controls do not replace their earlier independent certificates.

Sequential Release runs use one pinned CPU, one native codec/conversion thread,
normal SIMD, tiering off and workstation nonconcurrent GC. The complete public
encode includes fresh MemoryStream creation, growth, conversion, sampling,
transform, adaptive histogram/table construction, entropy coding and output
release; native conversion, encoding and independent packet acquisition/release
are included. Startup, fixture preparation, IPC and validation are excluded.
Native allocation is unmeasured; managed allocation includes harness overhead.

The same verified production/harness builds, prepared children and revalidated
exact-packet quality anchors serve all scored runs. An initial copy of an older
production DLL was detected and retained unscored; internal tests compile linked
production source and do not produce that DLL. The explicit production build and
affected public tests were corrected before these measurements.

Windows and Linux each pass **3,267 Release tests**, including **896 live FFmpeg
comparisons**. Focused JPEG checks pass **424 per mode per OS**, with normal
intrinsics and intrinsics disabled. Six new internal checks cover independent
scalar arithmetic, half-tap rounding, guards, source preservation, geometry and
transparency; four public checks freeze the packet and optional admission/restart
boundaries. The consumer and all three examples pass through the **same actual
dependency-free Release package** on both systems. The fully qualified Linux DLL
matches the timed DLL.

Qualification and retained raw identities are in the
[numeric evidence](warm-jpeg420pair-evidence.json). Stop tuning this confirmed
pair; other material workloads and final current-source coverage remain open.
