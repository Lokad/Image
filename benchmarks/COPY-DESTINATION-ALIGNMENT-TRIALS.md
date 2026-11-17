# Destination-aligned copy trials

**Neither implementation is promoted.** Both preserve exact behavior but their
short complete-operation screens have adverse means and inconclusive confidence.
The qualified production DLL, package and all displayed costs remain unchanged.

| Original complete RGBA16 forward/reverse pair | Baseline → prototype µs | Paired ratio interval | Managed GC B/pair |
| --- | ---: | --- | ---: |
| Per-row geometry | 5.265340 →5.537695 | .992205–1.114819 | 0 →0 |
| Two row plans prepared once | 5.352444 →5.658187 | .773598–1.444010 | 0 →0 |

The native aligned-bulk strategy is already established by
[retained attribution](COPY-ATTRIBUTION-NEAREST-GUARD.md); it is not profiled again.
A short-lived managed handle reads only the backing alignment modulo64, then
frees before copying. First/bulk/end accesses use bounded unaligned-safe spans.
A later GC move can affect alignment benefit, never pixel correctness. The
existing byte/same-row/separate-word/wide/scalar kernels remain verbatim.
No pointer dereference, unsafe code, native binding, API, pool or persistent pin
is introduced, even in these unaccepted production-source prototypes.

The coherent revision removes repeated row geometry: two private 12-byte value
plans prepare once and each row selects its plan. An integer model verifies
2,080,832 length/prefix combinations; independent byte oracles qualify the actual
implementation. Temporary duplicate loop machinery is not retained in production.
Actual generated row-helper sizes are 530/644 B versus 438 B for the identified
qualified helper; code size is not a CPU attribution. Pixel accesses remain
unaligned-safe; one aligned fixed-frame local clearing instruction is separate.

Each prototype passes **seven internal/190 public checks per mode per OS**:
normal, AVX-512-disabled and scalar, plus the original worker's full-word snapshot,
hidden-RGB/untouched pixels, cancellation/refusal/recovery and cleanup checks.
All64 correct/stale hints and row/chunk boundaries retain exact canaries.
The pin-release test checks prompt admission release before GC, then verifies
that a disposed backing is not kept alive by a leaked handle.

All three reversed 16,384-operation pairs, equal warmups, original fixture,
two batch clocks and idle-v2 guards stay. CPU0 Release uses natural nonconcurrent
GC. Validation, hint preparation, public geometry and copying stay timed.
Canvas setup, IPC, final ownership disposal and checks stay outside the original
borrowed-mutation clocks.
These are incremental DLL comparisons, not standalone FFmpeg passing evidence.
No confidence-policy switch, confirmation or favorable replay follows adverse screens.

One scoped preparation diagnostic observes **23.77–23.79 ns/call** including a
delegate call; its getter control is **1.99–2.01 ns**. These are diagnostics, not
end-to-end sums or inferred speedups. They provide little reason for more
per-image cache/lifetime machinery in this round. Runtime handle-pool retention
and physical/native/JIT peaks are unmeasured. Managed allocation stays zero;
the second candidate adds 24 B of value-plan locals, not pixel or heap workspace.

Two isolated cores/embedded assemblies and one small diagnostic tool build;
zero VM/canonical/public/harness/native/fixture/package builds. One initial test
import error rebuilds only its embedded assembly. Reuse unchanged
[qualified production](TEXT-CONDITIONAL-POLYGON-SCRATCH.md): 6,803 full/live checks
each OS, the same actual package/all three examples and green CI. No candidate
broad/package campaign follows. Resolve separate-word uncertainty next, preserving
all finite289 paths and stopping unchanged passing cases.

[Evidence](copy-destination-alignment-trials-evidence.json) retains actuals,
source proof, codegen, corrections and qualification reuse. Mirrored/member-verified
`copy-destination-alignment-trials.tar.xz`: **5,909,732 B /145 members**, SHA256
`ffe3f45eccab2f7c3b2839e670cbf19c1faa9acc01a931b019eac18741018de6`. Archived `cqa1205-*` /`dca1206-*` /`dcp1207-*` /`pac1208-*`
recipes and declared dependencies reproduce the round. It supplies no displayed
results or renderer dependency; materially revised evidence may reopen the lead.
