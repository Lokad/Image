# Paired JPEG inverse-row study

JIA543/JRP544/JRI545 is unselected. Two independent rows share a 512-bit matrix,
but neither implementation produces a promising complete-operation screen.
Qualified WAF542 sources, main/embedded modules and public bindings are restored
byte for byte on Windows/Linux. The package and all 125 displayed costs are unchanged.

| Three-actual RGB444 stage | Managed / reference µs | Actual production code generation |
| --- | ---: | --- |
| A: paired matrix helper | 1,712.89 / 1,451.00 | 1,976-byte caller, 248-byte frame; 576-byte helper called in row loop |
| B: inlined paired matrix | 1,734.03 / 1,450.19 | 2,585-byte caller, 808-byte frame; paired call removed |

GC remains 10,088 B/op. These short screens establish no causal gain, regression
or equivalence. No independent confirmation, favorable repeat, workload expansion,
broad suite or package run follows. The inlining revision tests an observed call
boundary; fewer calls and eligible row counts alone do not prove elapsed savings.
These results do not permanently close SIMD or inverse-transform work.

An independent canonical T.81 entropy/dequantization model reads the existing
133,057-byte RGB444 control, without regenerating inputs. It verifies 6,630 blocks,
1,105 MCUs, the final cursor, padding and EOI. There are 23,519 aligned adjacent
AC-row pairs, covering 96.12% of 48,937 AC-row transforms. These are eligibility
counts for one control, not runtime attribution or broader content evidence.
Source-valid JBA530 attribution is reused rather than reprofiling unchanged code.

Each 256-bit half preserves the original signed-word products, Int32 wrapping,
1,024 bias, shift and saturation. Original dequantization, DC/two-low-row shortcuts,
single-row AVX2/scalar paths, column execution, checkpoints and ownership remain.
Existing row weights and the 256-byte input/row workspace are reused; no heap,
static array, cross-block buffer, pool, session or owner is added. JIT frames are
reported separately and do not establish physical/admitted/native peaks.

Both stages pass **401 internal / 375 public checks per normal/scalar mode on
each OS**, including the existing 640 basis, 2,048 dense and 32 extreme independent
inverse-transform oracles. Two existing public sync/async checks expose actual
production JIT code per stage. Complete decode still includes parsing, preparation,
entropy, dequantization, IDCT, color conversion, independently owned RGBA output
and disposal. Frozen inputs/settings, exact pixels, native/source/ownership gates
and idle-v2 single-CPU guards pass.

[Evidence](jpeg-paired-inverse-rows-experiment-evidence.json) pins both stages,
source proofs, checks, actuals and exact restoration. Raw
`jpeg-paired-inverse-rows-experiment.tar.xz`, SHA256
`f9be869a3f2f849008f9ae877070c543aaae5e870e0d9ed7f110b6df7248e7b2`,
13,692,696 bytes / 234 members, is mirrored and member-verified. Sources/modules,
scripts, guards and pinned WAF542/JBA530 providers are retained. This unselected
study adds no renderer dependency.

Reuse the unchanged 5,877-check and same-actual-package qualification. Next audit
the live JPEG DC/AC preparation boundary before another production edit. Hot AC
and refinement loops already capture cancellation locally. Publication remains
postponed; the full 283-path objective remains open.
