# Distortion-only VP8 preparation audit

DPA459/CEP460, 2026-11-07. No production change, production/harness/reference rebuild
or new timing claim. Two untimed diagnostic executables are built.
[Evidence](vp8-distortion-preparation-attribution-evidence.json) pins the
qualified VLS458 module, source, existing complete-operation harness and controls.
The effort2 diagram's current confirmed gap remains437.42µs/upper1.169; its complete
7,330-byte packet stays exact. This audit does not reopen passing controls.

One two-second current capture retains1,866 samples,1,783 resolved to managed
code. Largest leaves include coefficient writing228, distortion small-block search
164, source histogram153, coefficient statistics122, forward DCT107 and large
prediction104. These are sample counts, subject to inlining/skid and instrumentation,
not phase times, additive removable costs or predicted gains. All instrumented
elapsed values are discarded. The existing packet/pixel/quality/alpha/source/owner
checks and idle-v2 guards pass; cached harness/selector/reference/fixtures are reused.

The strongest preparation lead is the duplicated coefficient traversal. Method2
already computes its final decisions once, records statistics, finalizes probabilities,
then traverses retained coefficients again during partition coding. Both walks derive
last positions, bands, contexts and category branches. The writer also emits fixed
category/sign bits; arithmetic coding itself remains necessary. The `tokenBuffer`
argument is a counter-policy distinction, not an existing retained event buffer.

The earlier [shared-event investigation](VP8-SHARED-EVENT-STREAM-INCONCLUSIVE.md)
already tried two-byte arithmetic events, rolling coefficient scratch, admitted
trimmed/reused chunks and bulk/local replay for ordinary efforts3–6. It established
no useful primary opaque CPU gain, with1,126,872B/op extra allocation; photograph
CPU was inconclusive. Direct efforts0–2 retained their original path. Repeating
that design based only on this profile is unjustified.

An untimed current method2 footprint check verifies the exact packet, unchanged
coefficients and borrowed-root cleanup:561macroblocks,188skipped,68,283events before
skip selection/63,583emitted. Two-byte payload alone would be136,566B plus2,248B
offsets versus448,800B coefficients. These are hypothetical logical sizes, not
allocated capacity or peak measurements, and establish no CPU benefit.

A materially revised compact residual-instruction projection now passes all13
existing frame controls, efforts0–6 and existing quality settings, using the current
Windows embedded production assembly. A28-bit instruction stores adaptive base,
category/remainder, sign and EOB scheduling; a terminal class represents EOB-false.
Expansion matches actual adaptive counters including renormalization, top/left
contexts, the actual writer's complete partition and frozen packets. Coefficients
stay unchanged and the borrowed root releases correctly.

For the effort2 diagram,24,421emitted instructions need97,684logical bytes versus
127,166B of emitted two-byte events. The dense opaque control needs804,012B instead
of1,556,378B events, still above448,800B coefficients. Low-quality diagram instruction
storage is larger than its event storage. There is no blanket storage or CPU claim.

This proof stages diagnostic Lists outside production admission and expands
instructions before statistics/writing. It does not establish production ownership,
actual capacity, memory peak, CPU, or integration of all-skipped statistics and final
probability selection. A production design must record statistics during instruction
construction, avoiding a second statistics traversal. The next bounded task is an
operation-owned method2 prototype with the exact direct/skip policy. Its coherent
end state replaces full coefficient retention with instructions and block scratch.
Transitional overlap must be reserved and measured; dense storage/expansion costs
can still outweigh removed parsing. Samples do not establish acceptance.

The experiment must preserve direct versus token-buffer node9/node10 statistics,
category/sign order, strict decisions, the zero segment-header-cost policy, and
all-zero skip handling with exact top/left/Y2 contexts. Methods0/1 have nonfinal
statistics decisions; other methods use different statistics/retries. Their early
decisions cannot be reused under method2 assumptions. Start with the existing
effort2 control and focused dense/sparse/category/context checks against the actual
production writer. Retain all22 original packet controls. Additional affected
timing boundaries follow actual changed paths, not a new workload grid.

Local libwebp source inspection confirms distinct direct/statistics and retained-event
routes. Only algorithmic insight is used; no upstream source, tables or text become
tracked/build inputs. The JSON records checkout/file identities. Repeated border
interpolation and source packing remain a separate prediction lead; eager generation
of ten candidates can waste work when existing pruning stops early.

Run a bounded complete-operation screen after exact correctness/ownership checks;
independently confirm only a promising design. Admission must precede allocation,
including replacement peaks; preserve terminal refusal, cancellation, awaited users
and independent partition/output ownership. Qualify Windows/Linux/scalar and the
actual Release package at a coherent milestone. No repeated whole-library or package
qualification is needed for this source-only audit.

Raw`vp8-distortion-instruction-projection.tar.xz`,207,388B/19members,
SHA256`77f188abc8378c77ec9ef31999c84b8b3515584187d9ff89c7f8c67f33fef112`,
is mirrored and member-verified. It retains the projection and includes
`vp8-distortion-preparation-audit.tar.xz` (179,428B/11members,
SHA256`77b0a16e751258ce2a04853f537007dd7dee728126cf493088e94110ad749682`),
which includes the original profile archive
`vp8-distortion-preparation-attribution.tar.xz` (158,952B/27members,
SHA256`cabe2816eff2ddc094117af86e5b0ab89fa7f05908e39bc0ac447e4d5191928a`),
current source, commands/gates, trace/map/JIT, the diagnostic/source/results and an
identity-to-prior-archive map for unchanged inputs. Its inherited diagnostic producer
label was stale; current core/embedded hashes identify the actual qualified VLS458
inputs, with the original label retained separately. No timing is used.
No renderer dependency is added because this audit supplies no displayed result.
