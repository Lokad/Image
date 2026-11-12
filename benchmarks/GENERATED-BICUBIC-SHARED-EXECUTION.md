# Shared generated bicubic execution

Extreme 8/16-bit reductions now share geometry, coefficient verification,
16-source-row preparation and 16-output-row scheduling. Sample expansion is
statically specialized; unsigned full-range word products stay unsigned.
Per-depth pixel publishers and exact repair are preserved verbatim. The
duplicated 208-line word implementation is removed without a forwarding wrapper.

| Complete 513×259 input, additional 4 KiB workspace | Before / current µs | Prepared zimg µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA16 → 2×9 | 1,324.96 / 462.67 | 1,269.67 | 0.364 / 0.380 | 113 |
| RGBA16 → 2×2 | 1,074.91 / 442.47 | 2,134.32 | 0.207 / 0.221 | 113 |
| RGBA8 → 2×9 | 460.45 / 437.49 | 1,049.53 | 0.417 / 0.419 | 113 |
| RGBA8 → 2×2 | 464.56 / 437.92 | 1,622.84 | 0.270 / 0.273 | 113 |
| RGBA8 → 1×1 | 446.66 / 447.09 | 4,872.34 | 0.092 / 0.093 | 113 |

All five existing controls pass independent confirmation and stop. Word 2×9
saves **862.29 µs / 65.08% raw / 65.62% normalized**;
filtered, all-actual and normalized intervals separate. Its native clock moves
+1.58%. These are separate source-valid captures, not
paired-DLL measurements. Word 2×2 also improves; byte 2×9/2×2 show smaller gains.
Byte 1×1 is incrementally inconclusive (−0.10% raw / −0.32% normalized), with
no separated adverse interval. No uniform gain or allocation-saving claim.
Each original three-actual screen and twelve independent actuals remain,
including excluded statistical outliers and both native brackets.

The actual predecessor package profile has 1,766 generated-word FilterRow and
209 coefficient locations among 1,996 samples. Its source/count model finds
662→518 horizontal row filters and 85,656→4,644 coefficient calls for word 2×9.
Counts, code sizes and sampled locations are diagnostics, not component times
or speed predictions. The archived byte schedule supplies the shared design.
Profiling raw `generated-word-current-attribution.tar.xz`, SHA256
`3ce4279e82ee8d46b9485f1dc3b52caeead18df0247425c0b0560e034deedb39`,
97,204 B / 26 members, is mirrored and verified; instrumented elapsed is discarded.

Managed measurements include complete public Resize preparation, filtering,
admission, independent output and disposal. The explicitly labeled zimg backend
retains graph/scratch; timed work includes packed input conversion, clamped halo,
filtering, unpremultiplication/quantization, owned output and disposal. This is
not a fresh-native-graph or FFmpeg CLI comparison. Frozen inputs/settings,
rational/frozen managed samples, approved native-only bounds, immutable inputs,
independent surviving owners and idle-v2 guards pass. No new workload grid,
numerical policy, quality relaxation or fixture-specific path is introduced.

Every row keeps its original lower-then-upper tap additions, virtual vertical
order and duplicate clamped edges. Verification, magnitudes, certification and
repair stay exact. Explicit stack arrays remain 1,824 B for byte samples and
grow 512→1,824 B for word samples, within the same 4,096 B preadmission. No new
owned heap capacity, retained scratch, pool, session or API. Physical/JIT/native
peaks remain unmeasured. The non-inlined typed frame retires before release;
refusal is terminal, and cancellation/exception cleanup preserve ownership.

Only the root word callee changes. All 1,620 resolved root IL instructions,
locals, stack and flags match after that remap. The actual timed module's
9,388 B root JIT has the same 2,204 instruction/label lines and block offsets,
normalizing only call-owner names and addresses. This scoped check supports
reusing unrelated costs; it is not a whole-library audit or timing proof.

Windows/Linux each pass **6,487 full/live checks** (3,158 internal / 2,381 public /
948 live), plus 559 internal / 791 public per normal/scalar resize mode and
the three named storage suites. Seven additional word cases cover block tails,
zero/tiny/full alpha, unsigned products, rational pixels, poison output,
refusal/cancellation cleanup and recovery. Existing portable/disabled-ISA proof
is reused after unchanged rejection guards. The SAME actual Windows Release
package consumer and all three usage examples pass both OSes. One core/embedded
build each OS, no public/frozen-harness/native/fixture rebuild and one pack.

Across 73 standalone resize controls: **59 pass / 7 gaps / 7 uncertain**.
Other 68 costs remain source-valid: 65 base, two byte-Cross and one DeepWord.
Scoped adjacent views remain separate. Stop the five generated passes; the
largest demonstrated remaining resize excess is byte Cross, 81.81 µs. Attribute
its current preparation/execution before another coherent design. All 289
required paths remain in scope; publication is postponed.

[Evidence](generated-bicubic-shared-execution-evidence.json) retains source
proofs, current/old clocks, all actuals, profile/model, qualification and
reproduction scripts. Raw `generated-bicubic-shared-execution-qualified.tar.xz`, SHA256
`5ccf5623e26a1d0d271f4490c4710d1a45afa3b9b1a0e1daf0ed1aa4739e4b72`
(979,796 B / 190 members), is mirrored/member-verified with
retained dependencies. The 455,162 B development 0.2.1 package has SHA256
`fa535658517b0d06af8d37840c8a071bb892f58f44709988c699aa826ad93c01`. Published releases remain immutable.
