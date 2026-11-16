# Paired glyph-header validation trial

**Not retained.** Pairing two eager glyph-header comparisons, then outlining the
single-glyph path, demonstrates no useful complete owned-font loading gain.
All three original short captures retain every actual and native bracket:

| Original owned font load/dispose / FreeType 2.13.2 | Managed / reference µs | Native ratio interval | Managed GC B/op |
| --- | ---: | ---: | ---: |
| Qualified baseline | 17.556 / 13.554 | 0.892–1.807 | 176 |
| Paired scan | 18.216 / 13.464 | 1.196–1.531 | 176 |
| Outlined single-glyph path | 19.583 / 13.280 | 1.145–1.953 | 176 |

The native ratios compare each producer with its contemporary reference; these
are short diagnostics, not current accepted costs or a confirmed causal regression.
The unchanged baseline is reused for the revision's screen. No independent
confirmation, favorable repeat, complete-pipeline retiming or broad campaign follows.

The first design prepares two bounded headers from the operation-private font copy
and compares their signed coordinates in one SSE2 operation. A bad pair follows
original scalar error order before any second-glyph checkpoint; valid pairs retain
a cancellation checkpoint per glyph. Zero-contour/empty policy, scalar hardware,
eager validation, metadata, admission and independent ownership remain.

Actual generated code realizes the packed comparison and grows to 925 B; the
earlier source-valid packed scan is 510 B. A concrete revision moves rare SSE2
single-glyph work out of line, retaining the original inline scalar hardware path.
Its actual loop shrinks to 704 B, but the managed short point is higher. Size and
source instruction counts establish neither the cause nor a CPU gain. All
instrumented elapsed is discarded. No neighboring batch size or replay follows.

Each candidate passes 269 internal/165 public focused checks in normal/scalar
modes. An independent original-order oracle checks 8,192 authored short/long loca,
signed/zero-contour/empty/truncated/invalid/odd-tail/unaligned cases per mode, including
first-error type/message and unchanged poisoned inputs. Pre-canceled tokens and
all eight existing frozen full-word frames/owners pass. No new face or settings grid.

Exact original sources, 19 build-output files and the source inventory are restored
without rebuilding. Candidate-only code/tests are archived and removed; Windows
modules and the actual package never change. Reuse 6,745 full/live checks per OS
and the same Release-package consumer/all three examples. Two Linux core/embedded
builds; zero other builds, package or broad qualification repeats. The archive-only
missing local parser is copied before packaging evidence; no timing job is repeated.

Allocation stays 176 B/op; owned font capacity is 757,332 B, with no added array,
admission, pool/cache or retained state. Borrowed input remains separate. Physical,
native/JIT peaks are unmeasured. Frozen DejaVu Sans 2.37 Regular has 757,076 bytes /
6,253 glyphs. FreeType validation is lazy and managed validation eager; this scope
is not general parser equivalence or proof of overall text slowdown. Existing
complete one-shot and repeated public integrations remain passing on the restored
package; no historical stage sum or broader font-performance claim follows.

[Evidence](font-paired-glyph-trial-evidence.json) pins both implementations, scalar oracle, code checks, captures and restoration. Raw `font-paired-glyph-trial.tar.xz` (4,807,816 B/110 members), SHA256 `b6229b7a7471225f26ab43b653be022f456baa92748c604a8bf9e7e4b77b2380`, is mirrored/member-verified with original font-harness and qualified-package dependencies. Archived fgp1123/fgp1123b recipes reproduce the bounded study; all original observations remain. [Current font costs](FONT-PACKED-BOUNDS.md) and [complete public integrations](TEXT-GLYPH-ROW-BOUNDS.md) are unchanged. A revised design needs concrete attributable work; this result is not a family ban. Full 289-path parity stays open and publication postponed.
