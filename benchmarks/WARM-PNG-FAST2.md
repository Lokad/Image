# Streaming RGBA16 Fastest encoding

Complete live-cancelable RGBA16 Fastest encoding independently confirms **0.731x
FFmpeg / conservative upper 0.747x**, at **2,103,647 managed B/op**. Stock output
independently confirms **0.732x / upper 0.735x**, at **2,103,738 B/op**. Every word,
alpha and hidden RGB sample is exact. The prior stock recoder allocated5,478,765
B/op; its timing remains historical evidence, not a same-build speedup baseline.

| Complete encode | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Live cancelable, short | 12,901.71 | 17,671.72 | 0.730 / 0.922 | 2,103,738 | Quiet screen |
| Live cancelable, independent twelve | 12,932.71 | 17,700.33 | 0.731 / 0.747 | 2,103,647 | Confirmed |
| Stock output, short | 13,681.68 | 17,715.38 | 0.772 / 0.973 | 2,103,624 | Quiet screen |
| Stock output, independent twelve | 12,927.95 | 17,660.77 | 0.732 / 0.735 | 2,103,738 | Confirmed |
| Ordinary RGBA16 Optimal | 14,680.33 | 20,089.60 | 0.731 / 0.887 | 2,103,624 | Regression screen; prior confirmed |

The comparison explicitly uses **managed BCL level2 / FFmpeg level1**, filter
None. The [pinned runtime's selector](https://github.com/dotnet/runtime/blob/v10.0.8/src/native/external/zlib-ng/deflate.c)
uses a fixed-tree quick compressor at level1 and a fast adaptive compressor at
level2. A bounded correctness probe shows that level1 `Filtered` retains the
same1,023,460-byte payload and fails the unchanged5% IDAT-size gate. The directed
level2 alternative produces **962,820 IDAT bytes**, against native **965,318**:
**0.259%** difference. Windows/.NET10.0.13 and Linux/.NET10.0.8 produce identical
payloads; exact samples and pinned normal/scalar native decoding pass. No
compression-quality, precision or size tolerance is relaxed. Managed/native zlib
class flags1/0 are validated separately, matching the declared backend choices.

Production now streams RGBA Fastest directly through the public BCL compressor.
Whole-payload staging, the candidate array, the independent Huffman recoder and
output-capacity preallocation are removed. Caller streams, live cancellation and
admitted tight budgets use the same packet policy. Source/row/64KiB IDAT/1MiB BCL
admission, logical row work, grouped cancellation and caller lifetime remain.
Tests preserve quota/failure prefixes relative to the selected packet policy,
caller exception identity, nonexpandable output and cancellation from callbacks.
Forward-only output is pixel/packet-correct; the subsequent
[caller-output round](WARM-PNG-FAST-FORWARD16.md) confirms0.771x / upper0.805x.

Both subjects time complete original pixels to independent PNG output: endian
packing, compression, CRCs, output acquisition and release. Native wrappers and
endian workspace are reused; conversion repeats every operation. There is no
output cache. The live token is never signaled; its setup is excluded, and the
native backend has no cancellation switch. Startup, I/O, IPC, fixture generation
and validation are excluded. Only the original two rasters and three workload
types are used. Native allocations are **unmeasured**.

All quiet guards pass. Both independent confirmations retain twelve actuals;
BenchmarkDotNet selects eleven for cancelable and all twelve for stock output.
The upper includes selected and all-actual intervals against the faster native
bracket's lower bound. The Optimal regression mean passes before full **3,373
Release tests per OS**, including **898 live comparisons**, plus167 focused
normal/scalar checks and two live checks per OS. Obsolete recoder tests are
removed; two public streaming contracts are added. One actual package passes its
consumer and all three examples on both systems. Measured/qualified inputs match.

[Numeric evidence](warm-pngfast2-evidence.json) retains every sample, allocation,
guard, source/assembly/job identity, prototype, package proof and raw archive hash.
Preparation refusals and the premature private launch remain retained and
unscored. Only the harness is rebuilt for its header-validation fix; the final
immutable production/harness is reused throughout scored runs and qualification.
The [original refusal](WARM-PNG-FAST16-REFUSAL.md) and
[retired recoder](WARM-PNG-FAST16-RECODE.md) remain historical evidence. Stop these
two confirmed pairs; remaining material boundaries and unmatched policies stay open.

Receipt-text erratum: the legacy generic boundary sentence “Both level1” was
incorrect for this policy. Explicit numeric fields and preparation validation
already enforce managed2/native1, as stated throughout this report. Raw receipts
and their hashes remain unchanged; subsequent harness text is corrected.
