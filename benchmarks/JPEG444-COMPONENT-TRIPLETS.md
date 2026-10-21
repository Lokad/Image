# Joint JPEG444 AC choices

C171 adds one joint three-component AC sweep after the existing precision-grid
decisions. It reduces the remaining packet gap incrementally while preserving
the frozen quality gate. Complete-packet parity remains open.

| Existing 513×259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original4:4:4, RGBA8 | 135,630 | 135,481 / 133,041 | 0.222041 dB |
| Original4:4:4, RGBA16 | 135,576 | 135,487 / 133,043 | 0.224030 dB |

The savings are149/89 B, or685 B across the five affected ledger rows—about5.3%
of their remaining byte excess. Both pass the unchanged0.25 dB source-quality
gate, exact alpha and decoder bounds (maximum4, RGB RMSE1.5). Source, sampling,
managed quality69/native qscale5, Original DQT, DC and0x204060 flattening stay
fixed. Equal quality numbers do not establish equivalence. These controls make
no photographic, diagram or general JPEG compression claim; optional CompactColor
and passing controls remain separate.

At each frequency the sweep considers current/minus-one/plus-one values in all
three components. Six changed full-block prices are cached. A strict improvement
in the same RGB-error/rate objective, nonincreasing total AC price and the existing
fine-nearest/coarse-feasible error cap select a legal triple, with stable ties.
The pair stage returns its already computed error and three current prices.
Avoiding their repeated scans admits the compression tail under the unchanged
2B work limit: **1,999,574,840** planned units instead of2,000,810,360.

The tail adds at most39,123 work/MCU, for905,445 total decision units/MCU. Checks
precede pricing, evaluation and commit. Its48-byte stack bank is sequential with
the preceding DAG scratch inside the same8KiB workspace. There is no new heap
owner, frame/input copy, source DCT or complete refinement pass. Shared admission,
terminal refusal, cancellation, async users and independent outputs remain covered.

Both OS pass **5,019 Release tests** (2,100 internal,1,983 public,936 live FFmpeg),
plus189 internal and62 public scalar checks each. Thirteen new checks cover a
true three-way improvement at a pair minimum, cached prices, exact work bounds,
legal amplitudes and cancellation/refusal before commit. Public goldens cover
default2B admission, fixed/adaptive/caller/baseline/progressive/async boundaries
and lifetimes. Actual packets match the independent C170 prototypes byte for
byte; all411,840 chosen coefficients/depth are independently reconstructed,
and each decoder matches its equivalent baseline packet. Managed scalar and
normal pixels agree exactly.

One core build/OS stays frozen. Test-only cancellation setup and an outdated
admission-bound assertion are corrected by rebuilding only affected tests.
The same actual private0.2.0 package and all three usage examples pass on both
platforms. Benchmark/native/driver/differential/parser binaries are reused.

One quiet-VM three-actual complete forward444 screen records
**1,374,395.962 µs / reference3,773.855 µs**, ratio364.189/upper380.647,
**613,632 B/op**. Both versioned idle guards pass; native allocations are
unmeasured. Prior C165 was1,312,293.197 µs with the same allocation count. The
additional CPU cost is accepted for the byte savings, without a CPU-parity pass
or independent confirmation. Speed-only optimization remains deferred.

Five444 packet rows and their forward cost replace C165. Other invocations
containing changed Original444 encoding retain historical timing until measured;
relevant420/422/grayscale/CompactColor, decoder, PNG and WebP paths retain evidence.
The [ledger](JPEG-COMPLETE-PACKET-AUDIT.md) has13 default passes/5 open and one
separate optional pass. Maximum remaining excess is about1.84%; the finite256-case
warm objective and replacement readiness remain open.

[Numeric evidence](jpeg444-component-triplet-evidence.json) identifies the frozen
builds, actual package, original receipt and mirrored qualification raw. The
[benchmark summary](../docs/benchmark.md) remains limited to three tables.
