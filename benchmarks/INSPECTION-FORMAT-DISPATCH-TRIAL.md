# Inspection format-dispatch trial

**Not retained.** Returning completed JPEG parsing directly after format detection
removes the outer async dispatcher on immediate input. PNG/WebP keep functional
signature helpers; genuinely pending prefixes await before dispatch. Public owners,
the three grammars, acquisition, reads/checks/polls and metadata remain unchanged.
No new parser, array, cache, pool, owner, admission model or API.

The independent complete async-JPEG paired helper improves **215.943→209.008 ns**,
**3.21% / 6.93 ns**, interval **0.953284–0.982638**. All-actual mean intervals
also separate; allocation remains **568 B/op**. Every pair is retained.

That gain does **not** generalize to the original standalone harness. Its candidate
async JPEG capture is **271.952 ns**, versus **225.363 ns** for one qualified-
baseline control on that same boundary. All-actual mean intervals separate.
The direction materially contradicts the favorable paired result. No exact causal
regression percentage or codegen explanation is established, and the candidate is
restored without a favorable retry. These are complete public calls with fresh
input adapters and independent descriptors, not parser-only timings.

| Rejected producer / labeled FFmpeg AVIO-header adapter | Managed / reference µs | Conservative upper / status |
| --- | ---: | --- |
| ProbeDimensionsPng | 0.104830 / 0.121826 | 0.889896 / pass |
| ProbeDimensionsWebp | 0.104390 / 0.123260 | 0.874499 / pass |
| ProbeDimensionsJpeg | 0.179983 / 0.195661 | 0.960177 / pass |
| ProbeDimensionsJpegAsync | 0.271952 / 0.201006 | 1.388059 / gap |
| PngHeaders | 0.291329 / 0.572275 | 0.525933 / pass |

All five captures retain 12 actuals, original native brackets, frozen inputs and
descriptor/prefix/ownership checks. Filtered and all-actual statistics stay separate;
the upper takes the worse view. **None of these rejected-producer costs or passes
replace current evidence.** The current-cost renderer has no dependency on this trial.

The unchanged one-gated-read allocation helper saves **152 B/operation**:
probe **1,960→1,808 B**, full headers **9,984→9,832 B**. Two alternating 128-call
pairs follow 128 warmups per subject/mode. Totals include common gated-input/bridge
overhead, not standalone production GC costs or elapsed/native performance.
Immediate allocation stays 568 B, admission 512 B for dimensions / 8,448 B bulk,
with no added arrays or retained state. Physical/native/JIT peaks are unmeasured.

Linux passes **10 internal / 154 public** checks, normal and scalar, including
mixed pending prefixes, failure/cancellation, refusal, cleanup and recovery.
The prior 21-control header prerequisite is reused; current representative
descriptor, bounded-delivery and ownership checks pass. This is not a fresh
21-control run. Two header idle preflights detect I/O wait and stop before
timing; live inspection finds no overlapping workers. Their
records remain, and the later before/after guards pass. No completed job is repeated.

Qualified source, tests, core/embedded/public modules and source inventory are
exactly restored without rebuilding. Windows modules and actual package never
change. Reuse **6,728 full/live checks per OS** and the same actual-package/three-
example proofs. One Linux core/embedded build; zero other builds. No further
timing, codegen, broad or package campaign follows this unhelpful implementation.
Future dispatch work needs actual standalone-path attribution or a materially
revised design; no permanent strategy ban. Passing independent cases stay closed.

[Evidence](inspection-format-dispatch-trial-evidence.json) retains every result,
source/owner proof and restoration. Raw `inspection-format-dispatch-trial.tar.xz` (1,862,080 B /
163 members), SHA256 `c9f0d4bd7eb719d49a825544d828b79c0e9488121a84175a586039a84d982613`, is mirrored and member-verified
with qualified-baseline, helper/reference and prior implementation dependencies.
The full 289-path objective remains open; publication is postponed.
