# PNG four-byte Sub reconstruction

Updated 2026-10-18. Existing RGBA8 Sub decode is **50.5% faster**, with an
independent **0.851× FFmpeg / conservative upper0.855×** confirmation. Managed
allocation remains **4,456B/op**. This named case reaches warm single-CPU parity;
the full207-case objective remains open.

| Existing case / job | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 Sub / three-actual screen | 585.81 | 681.74 | 0.859 / 0.926 | 4,456 |
| RGBA8 Sub / independent twelve-actual confirmation | 584.20 | 686.53 | 0.851 / 0.855 | 4,456 |
| RGBA16 Sub / unchanged control | 362.17 | 743.10 | 0.487 / 0.573 | 8,552 |
| RGBA8 Paeth / unchanged control | 3,148.28 | 2,852.56 | 1.104 / 1.143 | 4,456 |

The original513×259 raster and identical Sub/Optimal level6 PNG are unchanged
(49,686IDAT bytes). Both subjects decode the complete resident packet into fresh,
independent owned RGBA8 output and release it. Managed admission, framing,
inflation and reconstruction remain timed. Native codec/input/frame wrappers
may be reused; output acquisition/release remain timed. No decoded image is
cached. Startup, I/O, IPC and validation stay outside timing. Native allocations
are unmeasured. Exact RGBA, alpha/hiddenRGB, scalar/native CLI outputs, source
preservation and two simultaneously live owners pass.

Four independent modulo256 byte-prefix chains replace scalar stride4 Sub
reconstruction: two doubling shifts reconstruct four pixels perVector128, then
the preceding vector's final pixel supplies carry. No approximation, table,
heap storage, workspace or work charge is added. Scalar tails and original
65536-byte cancellation boundaries remain. Short previous rows retain original
partial failure; aliasing and empty canceled rows retain their behavior. The
old stride8 Sub returns before the new guard; other filter bodies are unchanged.

Only DecodeSub8 changes among the finite named workloads. Named YA16 uses None;
four Stream/composition packets match the frozen None PNG8/16 hashes; Adam7
palette/gray and RGBA16 use different byte strides. The two controls above stay
within1.2% of retained means; their selected evidence and all206 other comparisons,
including all58 preceding passing proofs, remain applicable. Current coverage is
**59parity passes,144gaps,4confidence-open**, selecting144independent confirmations
and63qualified short screens. No raster, selector, filter or settings grid added.

All jobs run sequentially on one quiet pinned CPU, normal SIMD, tiering off and
workstation nonconcurrent GC. Both five-second exclusive-vm-idle-v2 guards and
every actual/native bracket are retained. Confirmation selects11actuals while
retaining all12; the reported upper is the larger selected/all-actual bound.
Existing short and static twelve-actual selectors were reused without rebuild.
The current approved native worker adds sampling CPU telemetry outside the
wall-clock interval and unrelated encoder/admission support; timed decode
apply/release/context functions, static archives, compiler and flags are unchanged.
Fresh brackets, exact ready state and pixels are checked; no native build or
baseline-policy change was needed.

**3,692 Release tests perOS** pass (1,371internal+1,423public+898liveFFmpeg), plus
29scalar primitive and63scalar public PNG checks perOS. The same actual private
Release package passes its isolated consumer and allthree usage examples on
Windows/Linux. Screened production bytes and the frozen measured harness are
preserved. The published0.1.0 packages/tag are unchanged. Production remains safe
managed dependency-free .NET10. Stop this passing case; reuse its evidence unless
the relevant inputs change.

[Numeric evidence](warm-png-sub-prefix-evidence.json) retains allocations, all
actuals, guards, quality/output hashes, precise invalidation, native reconciliation
and cross-platform/package proofs, with checksums for retained raw archives.
