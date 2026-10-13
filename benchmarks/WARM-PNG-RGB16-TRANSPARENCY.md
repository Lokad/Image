# Warm PNG RGB16 transparency

One safe RGB16 tRNS row primitive reduces the screened mean **40.8%**, from
**2.117× to 1.268× FFmpeg**. The independent quiet confirmation measures
**1.272×**, conservative upper **1.302×**, averaging **4,033.48 µs** with unchanged
**15,056 managed bytes allocated per operation**. All twelve actuals are retained;
both quietness guards pass. Stop tuning this fixed pair; uniform parity remains open.

| Stage | Lokad µs | Ratio / upper | Quiet guards |
| --- | ---: | ---: | --- |
| Generic mapping screen | 6,791.56 | 2.117 / 6.871 | Pass |
| Row primitive screen | 4,017.48 | 1.268 / 1.601 | Pass |
| First independent repeat, unscored | 4,014.38 | 1.262 / 1.292 | Post-run I/O wait failed |
| Independent quiet confirmation | 4,033.48 | 1.272 / 1.302 | Pass |

Both subjects decode identical authored 513×259 RGB16BE PNG bytes, filter None
and noninterlaced. All RGB words come unchanged from the existing original RGBA16
raster; only alpha is projected. The six-byte tRNS key **(12347,35806,59265)**
comes from pixel 1. Its two exact matches become transparent, retaining hidden
RGB; **169 upper-byte collisions remain opaque**. The other 132,865 pixels have
alpha 65535. No original image was added. The earlier RGB8 fixture stays byte-identical.

A six-second profile attributes 24.3% of samples to generic pixel mapping,
10.5% to stride access and 3.5% to pixel access. The new dense-row path expands
four big-endian RGB triples to native samples with safe AVX2 shuffles, compares
all three 16-bit words together and supplies exact alpha. Scalar tails and
fallback preserve every sample. Cancellation remains checked at least every
4096 pixels. Row admission/work charges, sparse-pass mapping and public contracts
are unchanged. Only pass routing changes among existing production methods;
the other 628 bodies and all 629 existing flags match.

The unchanged native worker decodes to owned RGBA64BE, then performs included
endian conversion and a second owned RGBA64LE output acquisition/release. It uses
one codec thread and no colorspace conversion. Normal/scalar FFmpeg and both
managed OSes agree exactly on full words, alpha, hidden RGB, geometry and held
independent outputs. Changed low-byte keys produce exact expected pixels, and
a malformed five-byte key with valid CRC fails explicitly.

Validation passes **2,784 tests per OS** (1,010 internal, 878 public, 896 live
differential), **48 focused row checks** with intrinsics enabled and disabled,
and all three examples through the **same actual Release package** on Windows
and Linux. Seventeen new tests cover vector boundaries, unaligned guards,
cancellation and every 16-bit value in each channel. Fresh builds and actual
children match all 630 methods and flags in that package.

The first twelve-actual repeat fails its post-run guard: CPU0 I/O wait is 2.4%,
above 1%. It remains unscored. All raw files and child identities were frozen
before one repeat using the same unchanged child. Pending preparation and result
writes are flushed outside timing before that repeat's quietness checks; both
five-second checks pass. Thresholds and sample counts are unchanged.

Sequential Release runs use one pinned CPU, normal SIMD, tiering off and
workstation nonconcurrent GC. Complete public decoding, independent output
acquisition/release and disposal are timed; released managed pixel storage may
be reused. Startup, I/O, IPC, hashing and checks are excluded. Native allocations
are unmeasured. The [numeric evidence](warm-pngrgb16trns-row-evidence.json) retains
the profile, baseline, failed guard, candidate and confirmation, with all four
child files, source/package identities and complete actuals frozen before later
preparation or mutation. Other material boundaries remain open.
