# Exact constant JPEG blocks

Nonconstant sparse-CMYK/baseline420 costs are superseded by
[packed-word matrices](WARM-JPEG-PACKED-WORD-INVERSE.md). The DC-only caller
confirmation and accepted constant-block primitive remain applicable.

The existing DC-only caller-CMYK decode uses **40.1% less elapsed time** with
exact pixels: **1.807× / upper1.817** FFmpeg. No owned storage is added.

| Complete 513×259 decode | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Prior DC-only caller CMYK, independent | 1,002.66 | 330.66 | 3.032 / 3.051 | 1,634,905 | 10,895 |
| Constant-block caller CMYK, independent | 600.31 | 332.30 | 1.807 / 1.817 | 1,634,913 | 10,895 |
| Current sparse-CMYK, owned RGBA8 | 1,074.77 | 441.68 | 2.433 / 2.446 | 1,085,513 | 22,708 |
| Current baseline420, owned RGBA8 | 1,279.56 | 1,010.28 | 1.267 / 1.285 | 757,319 | 52,093 |

The two companions refresh existing affected controls. Sparse-CMYK elapsed is
0.091% higher than its prior independent result; baseline420 is **2.430% higher**
(normalized point +2.235%). This bounded cost is recorded alongside the primary
40.1% gain; no fresh paired causal gain claim is made for companions. Native
allocations are unmeasured. Caller-CMYK's measured allocation rises by 8 B/op;
no new buffer, cache, capacity or admission is introduced. Its coefficient pool
and independent caller-storage ownership remain unchanged.

After unchanged dequantization, 63 zero AC words make both matrix passes
constant. The reduced expression preserves signed-word row multiplication,
wrapped column bias, the original Q14 product, arithmetic shift and clipping.
Full-width DC multiplication and saturation remain in their original order.
Nonconstant blocks retain the exact matrix. This differs from the closed
[constant-vector trial](WARM-JPEG-DC-TRIAL.md), which hoisted one vector inside
an earlier sparse kernel rather than removing complete constant-block work.

An independent proof evaluates all **65,536** post-dequant signed DC states,
**4,194,304** pixels. Two actual production tests check that frozen sequence,
unaligned spans, guards, source/quantizer borrowing, nonzero AC coefficients
whose products wrap to zero and zero-budget execution. Existing native spectra,
full-range scalar blocks and cancellation-before-mutation tests remain.

A fresh three-actual pair clears the strict 5% raw/normalized gate. One candidate
twelve reuses the valid original twelve: **40.128% filtered/unfiltered elapsed**
and **40.424% normalized-point** reduction. Raw 99.9% CIs and conservative
normalized envelopes separately separate. Each companion has one twelve-actual
confirmation. All original inputs/settings, exact normal/scalar/native pixels,
complete operations, owners and idle-v2 guards pass; no measurement is repeated.
No sampling, quality, ICC, orientation or memory policy changes.

All **5,168 checks/OS** pass (2,085 internal, 2,135 public, 948 live), plus
29 internal/98 public focused checks normal/scalar per OS. The same actual
Release package, governed streaming consumer and three examples pass on both
platforms. One core build/OS; zero harness/native/selector builds. A missing
test-only import required one internal-test rebuild before timing; the frozen
Windows production DLL was reused and the original failure retained.

Older costs of all JPEG rendering paths using this common inverse need current
evidence before CPU-pass claims; the three controls above are refreshed here.
Metadata-only inspection and pixel-coding validation bypass it. Encoder,
other-codec, resident-editing and text evidence stays applicable. Sparse-CMYK
is now the largest confirmed gap. Stop this constant-block primitive; the full
**268-path ≤1×** objective remains open.

[Evidence](warm-jpeg-constant-block-evidence.json),
[prior SIMD preparation](WARM-JPEG-DEQUANTIZATION.md).
Mirrored/member-verified jpeg-constant-block-qualified.tar.xz, 163 members,
SHA256 655eebf21aee9e65f279019b29c8bee7870c618def1aa2650b2d262a2b78a00e; dependencies are pinned by hash.
