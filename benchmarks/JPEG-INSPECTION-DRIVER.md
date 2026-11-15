# JPEG inspection advancement and suspension

One nongeneric JPEG grammar now advances immediate I/O without preparing its
parser async state machine. One out-of-line loop awaits genuine pending requests
and resumes the same grammar. Public wrappers retain reader ownership throughout;
no duplicate parser, fallback, cache, pool, API or new array.

Independent complete-operation DLL comparisons, all twelve pairs retained:

- Async dimensions: **0.255839→0.225184 µs**, saves
  **30.66 ns /11.98%**,
  interval **0.859263–0.901898**, **568 B/op** unchanged.
- Complete JPEG headers: **6.080528→5.431971 µs**, saves
  **0.649 µs /10.67%**,
  interval **0.886672–0.900054**, **8,776 B/op** unchanged.

Both all-actual mean intervals also separate. These scoped effects are neither
FFmpeg parity nor sums of earlier stages. The original favorable short screens,
all adverse/uncertain observations and quiet guards remain.

| Complete boundary / labeled FFmpeg AVIO-header adapter | Lokad / reference µs | Conservative upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| ProbeDimensionsJpeg | 0.199 / 0.193 | 1.084926 / uncertain | 568 |
| ProbeDimensionsJpegAsync | 0.229 / 0.203 | 1.191698 / gap | 568 |

Both original references use the SAME actual immutable timed/package DLL,
twelve actuals, original brackets and exact prefix/descriptor/independent-owner
gates. Sync is uncertain; async remains a gap. Unchanged PNG/WebP dimension and
PNG-header costs reuse their [identified producer](REFILL-CALL-INLINING.md),
with byte-identical relevant paths. No full release refresh or retuning passes.

The first implementation has only an inconclusive0.8%short point. Actual code
inlines pending-builder setup into its entry, using536B stack adjustment.
Keeping the pending helper out of line removes that builder from immediate entry
and reduces its adjustment to280B, still above168B original. Selected entry/parser
native code grows125+7451→400+8264B; cold pending code/physical peaks are unmeasured.
The change adds 110 lines (174 inserted, 64 removed) for explicit resumption. This is an evidenced tradeoff,
not a code-size saving. All instrumented elapsed is discarded.

One bounded allocation diagnostic forces a real pending marker or EOF read.
It adds **64 B/operation**: probe1888→1952B, headers9920→9984B. Absolute figures
include the same fresh gated input/typed bridge/batch overhead and are not
standalone production GC costs. Two alternating pairs of128operations retain
exact descriptors, independent outputs, unchanged borrowed input and quiet guards;
no elapsed/native claim. Immediate allocations stay568/8776B. Admission stays
512B dimensions/8448B bulk, with zero added arrays/cache/retained operation storage.
Approximate object accounting and physical/native/JIT peaks remain distinct.

Windows/Linux each pass **6,725 full/live checks**, **10 internal/152 public**
normal/scalar and the **SAME actual Release-package consumer/all three examples**.
Four new EOF cancellation/failure/ownership cases also pass the original baseline
on both OSes: admission survives an uncooperative read, late I/O failure retains
precedence, caller stream/root survive, cleanup releases capacity and reuse succeeds.
One accepted core/embedded build per OS, one public build per OS, one diagnostic
helper build and one pack; no harness/native/fixture builds or timed Linux rebuild.
One earlier unaccepted Linux core/embedded build remains recorded. Performance
reconciliation precedes the single broad/package milestone.

[Evidence](jpeg-inspection-driver-evidence.json) pins the phase mapping, both
implementations, original callbacks/reads/checks/polls/error order, actual package
and qualification. Exact encoder packets, managed pixels and public metadata
contracts remain. Raw `jpeg-inspection-driver-qualified.tar.xz` (6,489,460 B/253 members),
SHA256 `344d8ed3399c39cdc9fd87197698415ff048f974570b06bb6ed994698005a7de`, is mirrored/member-verified with baseline/attribution
dependencies. Private package `e9f499d029c63253e24b7a9b60b2b076fb4ba64377d7ee0b5f76845ececf7626` (466,286 B)
contains timed DLL `aee849fea4a3347bbf1987c092f592ecfa6bf2796adcefe913f2209ffe9f0216`. The full289-path goal
remains open; published releases are immutable and publication stays postponed.
