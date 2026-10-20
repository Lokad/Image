# WebP alpha backward references and q0 entropy

The [limited-work prefix successor](WARM-VP8-PREFIX-WORK.md) supersedes the
work32M packet/cost claim below. The four unrestricted results remain valid;
earlier producer-specific work32M evidence is retained.

The existing 8/16-bit alpha controls shrink **268,028 → 170,196 B (36.5%)** with
exact decoded RGBA and unchanged source quality. Their complete-packet gate is
still open against the frozen **98,590 B** FFmpeg/libwebp reference. Alpha alone
is now **18,579 / 24,588 B**, including its header; that component pass is not a
whole-packet or whole-codec claim. Managed100/native75, effort4 and all input,
quality, exact-alpha and decoder gates remain unchanged.

| Existing unrestricted control | Before → current / reference packet B | Managed / reference µs | Managed allocated B/op | Short CPU ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 151,606 → 149,570 / 72,380 | 21,766.38 / 15,580.79 | 1,543,152 | 1.397 / 1.457 |
| EncodeOpaque16 | 151,606 → 149,570 / 72,380 | 22,277.85 / 15,618.46 | 1,543,152 | 1.426 / 1.531 |
| Encode8 | 268,028 → 170,196 / 98,590 | 28,120.00 / 53,786.79 | 1,793,800 | 0.523 / 0.559 |
| Encode16 | 268,028 → 170,196 / 98,590 | 28,612.36 / 53,790.31 | 1,793,800 | 0.532 / 0.591 |

The existing work32M packet remains **452,932 /98,590 B**, byte-for-byte and
work-for-work (31,961,577 units). Its prior cost/ownership proof is reused. All
five procedural packet gates remain open; the two primary opaque photograph/
diagram controls retain their separate passing matched-quality evidence.

This batch accepts two primitives. Full-frame q0 coefficient statistics reuse
private converted planes, preserving prediction and reconstructed pixels; this
alone saves2,036/2,048 B on opaque/alpha controls. Bounded alpha matching then
saves95,784 B with the same VP8 payload. Four recent positions per three-residual
hash, maximum4096 copies and adaptive green/distance codes exploit repeated alpha
sequences. There is no filtered plane, full RGBA intermediate or encoded-alpha
buffer. Complete padded byte counts preserve the smaller fixed or adaptive plan.

The history reserves262,176 B through the borrowed root;16KiB additional local
headroom bounds tables, construction scratch and existing2KiB fixed storage.
Selected storage stays charged through writing, and unselected successful plans
release promptly. Local eligibility is decided before admission; shared refusal,
work failure, cancellation and output failure terminate with cleanup. The original
codec work envelope is unchanged. The new alpha envelope96*N+5.2M is conservative:
four matches compare no further than the selected longest match, which is consumed,
giving at most12 comparisons/sample/pass; constant hash maintenance, writer calls,
history resets and bounded280/40-symbol table construction fit the remaining bound.
Every performed work charge remains present. The32M case keeps fixed coding.

A preceding gradient/adaptive-prefix screen produced250,012 B and98,395 B ALPH.
It is retained as evidence but not adopted: the stronger backward-reference path
replaces it, avoiding an extra algorithm solely for intermediate budgets. One
nullable-field compile correction happened before that final screen; final branch
cleanup reproduces all five staged packets/quality/work results exactly.

Both platforms qualify **4,628 tests** (1,733 internal,1,961 public,934 live FFmpeg),
161 internal+51 public+2 differential scalar checks, and the same actual private
0.2.0 package/all three examples. Fifteen added regression cases cover full-cache
admission/prefix freezing, exact backward copies and distance bounds, local policies,
refusal at every growth boundary, cancellation/output failure, retained charge and
subsequent success. The corrected mandatory-storage test permits fixed/adaptive
ALPH differences while requiring identical VP8/pixels and exact memory/output bounds.
Its original failures and the Linux proof-count correction are retained; passing
checks are reused without a production rebuild or another whole-suite repeat.

One accepted core/internal/public build perOS; three intermediate Linux cores are
retained. Existing differential/reference/harness/native/selector binaries are
reused, with no post-freeze producer rebuild. Four sequential three-actual short
cost screens retain eight idle-v2 guards and complete equivalent operation
boundaries, including normalization, output growth/ownership and disposal. CPU costs
are recorded without a parity veto or independent repeats; native allocations are
unmeasured. Exactly four existing rows change; the finite256-case scope is stable.

[Structured evidence](vp8-alpha-matches-evidence.json) retains every staged packet,
quality/hash/work result, native normal/scalar checks, costs/allocations,
qualification and immutable raw-archive identities.
