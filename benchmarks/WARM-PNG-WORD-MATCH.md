# Bounded word comparisons for exact PNG matching

The predictable 3000² RGBA16 horizontal-flip control now independently passes
the warm one-CPU target: **0.941× / conservative upper0.979**. Stop it unchanged.
Default Optimal and the pinned FFmpeg n8.1 level6/pred=mixed reference retain the
same complete 573,041-byte packet and exact samples. The <2 MiB gate remains.

| Quiet CPU0 observation | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Fresh baseline, one op | 744,612.47 | 1.874 / 2.131 | 3,402,944 |
| Candidate, one op | 372,714.64 | 0.924 / 1.081 | 3,402,944 |
| Unchanged candidate, one op | 384,434.72 | 0.966 / 1.109 | 3,402,944 |
| Candidate, four ops/sample | 383,214.38 | 0.941 / 0.979 | 3,402,572 |
| SmallestSize8 screen | 25,562.15 | 1.330 / 1.966 | 1,325,670 |
| SmallestSize16 screen | 29,650.63 | 1.324 / 1.733 | 3,813,679 |

The matched independent pair finds **49.95% less elapsed**, with separated
filtered/all-actual intervals and unchanged3,402,944B/op. Its first single-op
confirmation and one unchanged-build repeat have upperbounds above1; neither
establishes parity. Execution-rate shifts in both subjects motivate one bounded
confirmation with four complete sequential operations per managed iteration and
native sample. Twelve actuals and both twelve-sample brackets pass the unchanged
guards/confidence target. Per-operation ownership/normalization is unchanged.
The smaller batched allocation figure amortizes measurement overhead; it is not
a library allocation gain.

An initial batched selector retained sixteen generated calls while declaring an
unroll factor of one. It was stopped and excluded entirely. Both generated
unrolled bodies were corrected and checked against the job before the valid
confirmation; operation counts and per-operation allocation agree. Production
was unchanged throughout this measurement correction. Raw failed/source evidence
is retained; no invalid result contributes to the table or parity claim.

After the already-verified eight-byte prefix, the encoder compares UInt64LE words.
XOR and trailing-zero-count identify the first different byte. Spans split at
either64KiB ring edge and the <=258-byte lookahead, with scalar short/tail work.
Overlapping input history is immutable during search. Chain order, lazy decisions,
token/entropy rules, level selection, pre/post cancellation and owned storage are
unchanged. No new buffers, cache, admission, unsafe code or dependencies are added.

Before edits, actual qualified DLLs on both OSes freeze four original generic
Deflate packets across three delivery sizes: deterministic noise/long overlapping
runs, copies and first mismatches at logical/physical history edges. Independent
BCL decoding, exact bytes, input and caller-stream ownership pass. The new direct
kernel checks first mismatch and invisible lookahead tails at2,008 points across
unaligned, either-ring-wrap and overlapping views. The fourteen declared PNG
packets, six large opaque geometries, partial/hidden alpha and resources also pass.

Each OS passes5,070 full checks (1,999 internal,2,123 public,948 live), plus609
internal and217 public PNG/Zlib checks with intrinsics disabled. The same actual
Release package and three usage examples pass on both OSes, including independent
packaged PNG decoding. Published packages remain immutable.

Two affected existing SmallestSize controls have current screens around1.33×;
no paired gain/regression or passing claim follows. Other affected level6/9
default/smallest/async/composed encoder costs need current evidence before pass
claims. Level1/stored paths, decoding/inspection, other codecs and editing/text
retain applicable proofs. The complete268-path objective remains open.

Use the [predictive control recipe](PREDICTIVE-PNG-CURRENT.md) and current narrow
WarmShort/WarmConfirm jobs; predictive jobs now batch four complete operations.
[Machine-readable evidence](warm-png-word-match-evidence.json) retains all valid
observations and explicitly excludes the selector failure. Mirrored/member-verified
raw `png-word-match-qualified.tar.xz`, SHA256 `cfe0f5950547d145a5acce0ba40548505d112e0a83e27723c7711049353c1fd3`,
retains producers, packets, source, original oracles, pre-adoption/full/scalar/
package logs, actual packages, guards and valid/excluded selector evidence.
