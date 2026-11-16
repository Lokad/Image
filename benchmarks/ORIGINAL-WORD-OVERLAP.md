# Original word-overlap copy

This is the initial immutable baseline. [Word-alias dispatch](WORD-ALIAS-DISPATCH.md)
owns current costs after its accepted production change;this evidence is retained.

The original required `CopyRegion/Rgba16Overlapping` boundary has a confirmed
warm single-CPU gap: **7.010 / FFmpeg 5.092 µs per forward/reverse pair**, ratio
**1.376832**, conservative interval **1.366637–1.379847**, **0 managed B/op**.
This is a newly measured existing requirement, not a production speedup or a new
workload grid. [Original scope reconciliation](ORIGINAL-PLANNED-COVERAGE-RECONCILIATION.md)
retains the previously unmeasured names.

Both subjects borrow the original 513×259 RGBA16 canvas and copy a 257×129
rectangle by (1,1), then reverse it. Original prose fixed extent/shift but not the
absolute origin; this control freezes (0,0)→(1,1). Two public managed moves and
all validation/clipping/cancellation stay timed. FFmpeg n8.1's public
`av_image_copy_plane` uses 258 disjoint one-row calls per pair, including band
scheduling in timing. Native arguments are prevalidated; managed public checks
remain included. Neither subject allocates an output, resets pixels, uses a
temporary frame or creates per-operation working state. Resident preparation,
verification, hashing, startup and IPC are excluded on both sides.

The short three-actual screen was **6.686 / 5.073 µs**, interval
**1.173224–1.465268**. The independent twelve-actual confirmation above retains
every sample, three native brackets, alternating paired order, 16,384 pairs per
actual and two clock reads per batch. CPU0, optimized JIT, natural nonconcurrent
workstation GC and the versioned idle-v2 guard apply. Confidence uses two-sided
99.9% Student-t, paired log ratios and managed upper mean versus the fastest of
the three native lower means. No filtering or favorable repeat.

Full-word snapshot oracles check both moves, untouched pixels, alpha and hidden
RGB. Managed cancellation/refusal/recovery and normal/scalar checks pass on
Windows and Linux using the same actual-package DLL. The borrowed canvas is
1,062,936 B; new admitted/retained library workspace is zero. Harness snapshots
are outside the measured operation. Native allocations and physical/JIT peaks
are unmeasured. Reuse the unchanged production's 6,745 checks on each OS and
same actual package/three examples; no broad qualification repeat is warranted.

Linux/timed/package DLL: `aa4a59ca8fe2ea623126d4b3a97793aec46625a59455da56d74a0f89e7a15d26`.
Actual private package: `924e81be4bb1ab477c705f54ff0e851d6552b26f3002983be360220652392b7c`.
[Evidence](original-word-overlap-evidence.json) pins workers, source paths, native
archive/compiler, all actuals and ownership. Raw
`tmp/original-word-overlap-confirmation.tar.xz`, SHA-256
`91ae1f043d02666e0bf9cd942f49eb7696e4fd815de9cd615cc4ee80a6310799`,
1,243,140 B / 65 members, is mirrored and member-verified. The prior qualified
package archive is a pinned dependency, not duplicated. Two new managed-tool
builds and one native-tool build; zero production, embedded, existing harness,
Windows or package builds. An initial untimed input-hash failure caught the two
frozen zero-alpha samples before sampling; both attempts remain in raw evidence.

Reproduce with `tools/WarmReference/build-word-overlap-linux.sh` against an
existing immutable Release DLL, then `measure-word-overlap.py --managed DLL
--native WORKER --core-sha256 SHA --input INPUT --prefix OUTPUT --mode short`
on the quiet VM; use a new prefix and `--mode confirm` for independent confirmation.
The retained first build recipe and binaries reproduce the reported identities.

Next hypothesis: source and destination slices of different rows are disjoint
at both depths. Test the existing grouped row kernel for word self-aliasing while
preserving reverse traversal, same-row memmove, separate-word copying, exact
samples and all cancellation/limit boundaries. A measured gap justifies that
bounded comparison; it does not establish the cause or authorize adoption.
