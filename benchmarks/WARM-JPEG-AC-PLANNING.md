# Local JPEG AC event planning

Adaptive JPEG420 encoding with row restart markers now independently confirms
**1.799× FFmpeg / conservative upper 1.809×**, with **297,780 allocated B/op**.
Isolating one hot primitive reduces managed time **10.4%** from the current-build
screen. Exact packets, quality and allocation are unchanged. Uniform required
warm parity remains open.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Current-build three-actual screen | 3,220.29 | 1,608.95 | 2.001 / 2.679 | 297,780 | Above target |
| Candidate three-actual screen | 2,927.25 | 1,619.20 | 1.808 / 2.209 | 297,780 | Screen |
| Independent twelve-actual confirmation | 2,886.13 | 1,604.33 | 1.799 / 1.809 | 297,780 | Confirmed |
| Nonrestart adaptive regression, three actuals | 2,878.80 | 1,764.69 | 1.631 / 1.803 | 297,780 | Mean passes |
| Nonrestart independent twelve | 2,881.90 | 1,763.43 | 1.634 / 1.641 | 297,780 | Confirmed |
| RGBA16 source, three-actual screen | 3,039.42 | 1,841.93 | 1.650 / 1.793 | 297,780 | Screen |
| RGBA16 source, independent twelve | 3,098.92 | 1,842.05 | 1.682 / 1.697 | 297,780 | Confirmed |

The existing original 513×259 RGBA8 raster is encoded into a fresh MemoryStream,
using adaptive Huffman tables, quality 69, background 0x204060 and restart 33 MCUs.
FFmpeg uses optimal Huffman tables and qscale 5. Two logical native slices execute
serially on one CPU; both packets contain DRI33 and sixteen cyclic restart
markers. The source-quality gap remains 0.106110 dB within the frozen 0.25 dB gate.
Decoder agreement remains RGB max 4/RMSE 1.5 with exact opaque alpha.

One six-second profile records 5,923 samples: 62.1% in EncodeAdaptive, 12.6% in
WriteAdmitted and 7.9% in the forward transform. Targeted instruction attribution
identifies repeated histogram/scan loads and spilled loop state in AC event
planning. The accepted change isolates that loop from the much larger sampling
and transform method. Its histogram, scan order and event cursor stay local to
the smaller method. Coefficient traversal, zero-run events, signed amplitudes,
category checks, frequency counts and previously charged block work are
unchanged. No workspace, table or packet cache is added.

The complete restart packet remains 74,086 bytes, SHA-256
`68f646f5264a92cca439630e99330f458152e7d42efa10d5aebd4b50d28097d3`.
The nonrestart regression retains its 74,040-byte packet exactly. Source
preservation, independent simultaneous outputs, tight admissions, cancellation,
output quotas and caller streams pass. Twenty-one focused adaptive/entropy
checks pass normally and with intrinsics disabled on each OS.

Timing includes complete public encoding: flattening/conversion, transforms,
histogram/table construction, entropy coding, fresh output growth and disposal.
Startup, I/O, IPC, fixture creation, calibration and validation are excluded.
The original private event workspace may be reused; output is always newly
encoded and independently owned. No new raster, selector or settings grid is
introduced. Native allocation totals remain unmeasured; managed figures include
harness overhead. Quality-matched timing does not prove compression parity.

Sequential Release runs use CPU0, one codec/conversion thread, normal SIMD,
disabled tiering and workstation nonconcurrent GC. Both versioned quiet guards
pass on every observation. Confirmation retains all twelve actuals, selects ten
and takes the larger selected/all-actual upper against the faster native
bracket's lower bound. The initial mean above 2× receives no long run before
optimization. The passing regression mean precedes full qualification.

Both OSes pass 3,483 Release tests each: 1,199 internal, 1,386 public API and 898
live FFmpeg comparisons. The consumer and all three examples pass through the
same actual dependency-free Release NuGet package on Windows and Linux. One
changed core build is reused throughout timing; harness, native workers and
fixtures are unchanged. Full qualification refreshes only its required inputs.
The [numeric evidence](warm-jpeg-ac-planning-evidence.json) preserves samples,
allocations, guards, source/build identities and actual package qualification;
the [previous admission report](WARM-JPEG-ADMITTED.md) retains earlier results.

The nonrestart pair subsequently confirms independently on the same qualified
core/harness/native worker, retaining twelve actuals and selecting ten. Both
guards pass. This closes its earlier1.943× result without another optimization,
production rebuild, profile or suite/package proof. The raw confirmation also
retains the unchanged ordinary JPEG worker, matching the short regression's
worker/source/build hashes.

The existing RGBA16 4:2:0 pair also confirms on the same qualified build, retaining
twelve actuals and selecting eleven, with both guards passing. Complete full-word
alpha flattening and normalization to byte samples are timed on both subjects.
The original word raster and frozen quality69/native5 anchor are unchanged;
the complete 74,056-byte managed packet remains SHA-256
`ab163b4b8897668c5a2a4a4ac0869fabbe00704213e414943b1f5a33991b905d`.
This is one existing word-source case, with no new selector, fixture, grid,
production/harness/native rebuild, profile or repeated qualification. Stop all
three confirmed JPEG pairs.
