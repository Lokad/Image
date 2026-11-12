# Expanded bicubic coefficient trial

**Not retained.** Preparing expanded coefficient vectors once per tile removes
repeated routing, but both complete candidate screens are slower than the
qualified immutable baseline.

| Existing 513×259 → 256×129 RGBA8 Resize / 4 KiB | Lokad / zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Qualified baseline, reused screen | 430.01 / 418.45 | 1.028 / 1.106 | 113 |
| A: runtime storage choice, grouped taps | 575.38 / 416.61 | 1.381 / 1.507 | 113 |
| B: typed storage choice, fixed taps | 549.75 / 420.59 | 1.307 / 1.404 | 113 |

Each screen retains three actuals and both native brackets. A/B raw costs are
33.81%/27.85% higher; reference-normalized costs are 34.40%/27.19% higher.
Filtered, all-actual and conservative normalized confidence separate from the
baseline. Native moves −0.44%/+0.51%. The valid recent baseline screen is reused
without another capture. These adverse screens justify stopping; no independent
confirmation, word recapture, generalization timing or broad/package repeat.

The candidates borrow 8 KiB more from already admitted, unpublished result rows,
retain compact coefficients for exact repair, and retire every borrower before
tail metadata/output reuse. The primary tail grows from 12 to 20 rows.
Geometry-only selection keeps short/unsupported layouts compact; shared refusal
remains terminal. Owned capacity, 4 KiB scratch admission and retained state do
not increase. Additional coefficient traffic, preparation, tail work and generated
code stay timed. Physical/native/JIT peaks are unmeasured.

A keeps runtime selection live in the large executor and repeats coefficient
bounds checks. B selects the compiled strip mode before the hot loop and uses
fixed ordered tap accesses. A/B row bodies are 7,511/7,311 B versus 6,616 B;
their frames are 872/856 B versus 632 B. Tail bodies remain larger too.
This code evidence explains the revision, without attributing phase percentages
or claiming that fewer shuffles establish speed. The shared source executor
temporarily has compact/expanded compiled modes; all new modes are removed.

Both versions pass 535 internal/791 public affected checks per normal/scalar mode
on Windows/Linux. Seven new cases and the extended original-bit oracle cover
disjoint loans, canaries, partial/odd/full pairs, unsupported compact paths,
cancellation, admission and surviving owners. Original Add(Multiply) order,
exact repair, source/working storage and pixel gates remain intact.
Two core/embedded builds per OS; no public/harness/native/fixture build.

All three production/test sources, core/embedded/public modules and the actual
package are restored exactly. Candidate-only cases disappear with the discarded
implementation. The unchanged [6,449-check Windows/Linux and SAME-package
qualification](PNG-STORED-WORKSPACE.md) is reused. Current benchmark costs are
unchanged; no permanent ban follows for a materially revised design.

[Evidence](bicubic-expanded-coefficient-trial-evidence.json) links both versions,
source proofs, full measurements, generated code and verified restoration.
Mirrored/member-verified raw `bicubic-expanded-coefficient-trial.tar.xz`, SHA256
`1700c547977dbdbfd9c7496f2360871b8e0b02200bcfaee698a26718cb48403b`
(1,640,068 B / 102 members), retains reproducible inputs and baseline dependencies.
All 289 paths remain; publication is postponed. Continue an existing unresolved
material read boundary before assuming optimization is needed.
