# PNG prefix preparation — unselected

PGS548–PGM551, 2026-11-08. Production is restored exactly to
[JFW547](JPEG-LIVE-ENTROPY-WINDOW.md). No prefix implementation is accepted.
[Machine evidence](png-prefix-preparation-experiment-evidence.json) retains the
designs, sources, modules, actuals, adverse results and qualification.

The hypothesis was to reuse a current eight-byte prefix across candidate-chain
comparisons, replacing repeated short comparisons without changing candidate
order, ties, match depth, cancellation, tokens or complete packets. An independent
RFC 1951 view of the frozen byte packet found 99,882 of 106,186 selected matches
had length 3–7. The word packet instead contains 861,228 literals and 35,318
matches. These counts describe selected tokens, not rejected searches or CPU time.

The stages were:

- **A, eager prefix:** byte Fastest independently improves 4,540.44 → 4,160.74 µs
  (379.70 µs, 8.36% raw / 8.53% reference-normalized). Its word screen regresses.
- **B, prefix after the existing cheap gates:** byte improves to 4,258.60 µs,
  but word independently regresses 18,513.54 → 19,088.42 µs. Both filtered and
  all-actual raw/normalized intervals separate for that adverse result.
- **C, shared driver with two compiled matching kernels:** reuse the existing
  block-frequency census to select later sweeps. The scalar kernel preserves the
  original 399 normalized instructions, 1,548-byte code and 104-byte frame.
  Byte improves to 4,182.47 µs (357.96 µs, 7.88% raw / 8.07% normalized); ordinary
  word shows no separated gain or loss. Identical scalar code is not proof of
  equivalent complete-operation performance.

C's actual standalone live-word workload regresses by approximately 340 µs in
both launches: original 18,830.06 / 18,795.73 µs versus C 19,170.55 / 19,135.10 µs.
The fixed four-launch envelope was recorded before extending the initial pair;
there were no further unchanged repeats. A separate paired host, including
reversed module-load order, favors C, but preconstructs options and uses separate
assembly contexts. Its different setup/host boundary cannot override the
repeatable standalone result. The context dependence remains unexplained.

Each stage passes focused exact-packet and ownership tests on Windows/Linux,
normal/scalar. C also passes 5,879 full/live checks per OS and the same actual
private package/all three examples on both OSes. Its fifteen encoding costs and
private package are retained as **unselected** evidence. No production gain or
current-cost claim follows from those results. Codec admission remains 384 KiB,
retained scratch zero, measured byte/word allocations 820,916 / 2,437,660 B/op;
physical/native peaks were not measured.

After restoring the original sources, main and embedded test modules, and public
bindings without rebuilding, the unchanged package passed the new Fastest8/16
sync/async frozen-packet, shared-admission, independent-output and BCL pixel checks
on both OSes. Those useful consumer checks are retained. The Linux source manifest
then changes only for this consumer source. Existing broad/scalar proof is reused.

[Current original costs](CURRENT-PNG-FASTEST-COSTS.md) resolve ordinary word as
passing and live word as a gap. The next task attributes that original live
workload. This unsuccessful implementation does not close a whole algorithm family.

The ignored raw archive is `png-prefix-preparation-experiment.tar.xz`; its SHA-256,
size, member count, verified mirror and dependencies are in the machine evidence.
It includes original and A/B/C source/module snapshots, frozen inputs/packets,
independent token parsing, JIT captures, commands, guards, all samples, paired
contexts, qualification, both private packages and exact-restoration proofs.
