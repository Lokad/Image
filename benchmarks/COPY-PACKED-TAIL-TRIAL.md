# Fixed-size packed-copy tail trial

**Inconclusive and unaccepted.** An isolated prototype replaces the variable BCL
copy for exactly four/eight remainder bytes with bounded managed dword/qword
reads and writes. The current word-overlap operation has 258 eight-byte row tails.
The four-load snapshot/bulk kernel, every other BCL remainder, portable fallback,
callers, clipping, cancellation, admission and ownership stay unchanged. No new
array, cache, pinning, unsafe code, retained capacity or API is introduced.

| Original complete RGBA16 overlap pair | Qualified / prototype µs | Paired ratio interval |
| --- | ---: | ---: |
| Three-pair screen | 7.113 / 5.974 | 0.736187–0.958371 |
| Independent twelve-pair confirmation | 5.753 / 5.702 | 0.971204–1.011680 |

The initial 16.01% /1.138 µs point does not hold independently. Confirmation's
0.88% /0.051 µs point spans parity. Neither a useful gain nor a causal regression
is established. Every actual/adverse capture is retained; no favorable repeat,
neighboring variant, standalone-reference refresh or production promotion follows.
The existing immutable package and source-valid copy/flip costs remain current.

Both persistent workers use the original frozen 513×259 word canvas, 257×129
rectangle and forward (0,0)→(1,1) then reverse public moves. All validation,
clipping, cancellation and mutation stay inside each complete operation; input,
full-frame verification, IPC and setup stay outside. Each side warms 16,384 calls,
then alternates 16,384-operation batches in reversed order, one CPU0 operation at
a time, natural nonconcurrent GC. Two clocks per batch, no filtering or forced GC;
Student-t log ratios and all quiet guards remain. DLL pairs do not establish
standalone FFmpeg parity.

Windows/Linux each pass 190 public editing/ownership checks in normal/scalar modes.
Both actual workers pass exact full-word snapshots, hidden RGB/untouched pixels,
unchanged input, cancellation/refusal cleanup and recovery in both modes. JIT emits
the intended fixed-size tail moves; complete caller size 1364→1585 B.
Instruction/code-size observations establish no elapsed-time attribution, and
instrumented clocks are discarded. Allocation remains 0 B/op; added owned/retained
capacity is zero. The caller canvas stays 1,062,936 B; physical/native/JIT peaks
are unmeasured.

One isolated Windows core build; zero VM/canonical/embedded/public/harness/native/
fixture/package builds. A Windows path-separator error was corrected before any
VM check or worker started; only VM preparation resumed, without repeating Windows
checks. Canonical sources, modules and actual package remain byte-verified unchanged.
Reuse 6,750 full/live checks per OS and the SAME actual-package consumer/all three
examples. No broad campaign or affected passing-path retiming follows this result.

[Evidence](copy-packed-tail-trial-evidence.json) pins both sources, exact workers,
all observations, generated code, cleanup and qualification reuse. Mirrored/member-
verified `copy-packed-tail-trial.tar.xz`: SHA256 `d6dedd1e068841f26095baba7d16f3c8c0d5b0a314d8aee115e70635d81d76e9`,
2091428 B /69 members, with reproducible `ctr1147-*` recipes and
pinned qualified/tool dependencies. This trial stays outside renderer dependencies.
Full 289-path parity remains open; publication stays postponed. Independent named
uncertainties proceed while a new copy design requires distinct attribution.
