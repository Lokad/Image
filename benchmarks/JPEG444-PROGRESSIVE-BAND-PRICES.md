# JPEG444 progressive-band prices

C176 prices the final coefficient decisions on the same spectral bands used by
the progressive packet. It makes a verified incremental compression gain;
complete-packet parity remains open.

| Existing513×259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original4:4:4, RGBA8 | 135,481 | 135,342 / 133,041 | 0.220038 dB |
| Original4:4:4, RGBA16 | 135,487 | 135,265 / 133,043 | 0.225525 dB |

Savings are139/222 B, or778 B across the five affected ledger rows—about6.4%
of their previous byte excess. Both pass the unchanged0.25 dB matched source-quality
gate, exact alpha and decoder bounds (maximum4, RGB RMSE1.5). Source, sampling,
managed quality69/native qscale5, Original DQT, DC, source-trained Q8 prices,
lambda100, precision factors, MCU error allowance and0x204060 flattening stay fixed.
Equal quality numbers do not establish equivalence. These controls make no
photographic, diagram or general JPEG compression claim; optional CompactColor
and passing default controls remain separate.

Local, trellis, pair and triplet prices now reset runs/EOB at each existing
equal-point-transform AC band. The trellis has a root and terminal per band,
assembles one complete component candidate, then applies the original global MCU
error/rate criteria and bounded bisection. Bands receive no separate error allowance.
The source-price trainer is unchanged. Default single-band kernel calls retain
their prior decisions and work.

Two scalar64-bit masks come from the already computed amplitude grid. Their128
setup/visits fit2688 of the existing2880 precharged precision units. Price queries
retain64 visits; within-band DAG predecessors are a subset of the original2016
arcs. The905,445-unit MCU bound and1,999,574,840-unit control plan remain conservative
under the unchanged default2B limit. The same8KiB fixed and64KiB adaptive workspace,
shared root, terminal refusal, cancellation and output ownership remain. There is
no new heap owner, frame/input copy, source DCT or complete refinement pass.

Both OS pass **5,031 Release tests** (2,112 internal,1,983 public,936 live FFmpeg),
plus201 internal and62 public scalar checks each. Twelve new checks cover masks,
cross-band zero runs/EOB, read-only replacement prices, trellis boundaries and
carried prices, single-band ties/work, work limits, refusal and cancellation.
Public goldens cover default2B admission, fixed/adaptive/caller/baseline/progressive/
async paths and lifetimes. Actual packets match the independent C175 prototypes
byte for byte; all411,840 source-nearest and chosen coefficients/depth are verified.
Normal/scalar/native baseline-equivalent pixels agree exactly for each decoder.

One core build/OS stays frozen. A test-only expected tie is corrected by rebuilding
only Windows internal tests. The same actual private0.2.0 package and all three
usage examples pass on both platforms. Benchmark/native/driver/differential/parser
binaries are reused; unchanged paths retain evidence.

One quiet-VM three-actual complete forward444 screen records
**1,386,194.881 µs / reference3,771.208 µs**, ratio367.573/upper433.796,
**611,264 B/op**. Both versioned idle guards pass; native allocations are unmeasured.
Prior C171 records1,374,395.962 µs and613,632 B/op. This cost is accepted for the
compression gain, without a CPU-parity pass or independent timing confirmation.
Speed-only optimization remains deferred.

Five444 packet rows and their forward cost replace C171. Other invocations using
changed Original444 encoding retain historical timing until measured. Relevant
420/422/grayscale/CompactColor, native, decoder, PNG and WebP paths retain evidence.
The [packet ledger](JPEG-COMPLETE-PACKET-AUDIT.md) has13 default passes/5 open and
one separate optional pass. Maximum remaining excess is about1.73%; the finite256-case
warm objective and replacement readiness remain open.

[Numeric evidence](jpeg444-progressive-band-price-evidence.json) identifies frozen
builds, the actual package, original receipt and mirrored qualification raw. The
[benchmark summary](../docs/benchmark.md) remains limited to three tables.
