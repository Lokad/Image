# Bounded word bicubic attribution

The existing 513×259→64×32 RGBA16 complete Resize remains a demonstrated gap:
422.81/zimg 397.48 µs, range 1.003–1.098, 113 B/op and 4 KiB admission, from
[current packed costs](PACKED-BICUBIC-FOUR-PIXEL-PREPARATION.md). This read-only
capture changes no production code or performance result. Complete managed
preparation, source processing, exact repair, independent output and disposal
remain distinct from the reference's retained zimg graph/scratch.

One two-second CPU0 operation/CPU1 sample uses the current actual package DLL,
reusing harnesses, original inputs/settings and frozen/rational/native/ownership
gates. Both idle-v2 guards pass. **Instrumented elapsed is discarded.** No new
builds, fixtures, settings, confirmations or broad/package repeats.

Of 1,995 locations, 1,979 map to managed methods. Distinct actual code instances
give prefix/suffix executors 895/348 samples (6,986/6,230 B); contiguous horizontal
helpers 411/164 (181/186 B); indexed helpers 23/25. Kernel has 101 locations.
Inlining, sample skid and generic display-name merging prevent treating these
counts as phase times or removable percentages. Actual address/size and JIT
signatures distinguish the two modes.

The eligible pair producer returns a 64 B SIMD value through a temporary stack
buffer. The caller then reloads lower/full vectors, extracts the upper half and
writes the existing cache. Observed prefix offsets 0x62a/0x632 and suffix
0x5fa/0x602 identify these reloads; they are **once per pair**, not the old
per-tap indexed-accumulator spill. A source-only model finds 20 strips, 5,180
decoded-row visits and 8,288 horizontal pair calls, 7,770 contiguous. Counts do not
forecast elapsed savings.

**Next bounded hypothesis:** change the existing private contiguous producer to
write directly into its borrowed two-vector cache view, preserving every ordered
multiply/add. Keep indexed/unequal/odd fallback, geometry, certificates/repair,
output-backed loan lifetimes, cancellation and admission unchanged. Add no owner,
storage, wrapper, pool or numerical policy. Extra destination arguments/guards
or code-layout changes may offset the removed copying; inspect actual code and
focused correctness before a complete short, then independent affected byte/word
confirmation if justified. Broad qualification belongs to an accepted stage.

Packed production source is byte-identical to the cost owner's version; later
Math additions leave old bodies unchanged. Current DLL 8a0fff84/Linux 745929aa and
actual package a2f3ea5a reuse 6,351 full/live checks per OS, 442/789 resize checks in
normal/scalar/disabled-AVX2/AVX512 modes and SAME actual package/all three examples.
Allocations, 4 KiB admitted scratch, 16,384 B retained result and zero retained
operation scratch are unchanged; native/physical/JIT peaks are unmeasured.

[Evidence](packed-word-current-attribution-evidence.json) records identities,
instances, source-only model and hypothesis. Raw `packed-word-current-attribution.tar.xz`:
114,172 B/20 members, SHA256
`36ed9196b8b0182d236ddab95eadf3b96c10df525a8b3bd7fb85b02b65b04edc`; mirrored and member-verified with original/current dependencies.
No renderer dependency: current costs remain owned by the unchanged packed report.
