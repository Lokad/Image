# Cached-wide cross-pair source reuse

CQH773 is unselected. Its revised independent confirmation measures
**394.83 → 393.77 µs**, a 1.06 µs / 0.27% raw point reduction with overlapping
filtered/all-actual confidence intervals. Native movement of +4.45% produces a
4.52% normalized point reduction; those intervals also overlap. Neither a useful
causal gain, regression nor equivalence is established. Allocations and retained
storage do not improve. Qualified production is restored exactly; this outcome
does not close materially different designs.

| Complete Wide8, 513×259 → 128×64 RGBA8 | Lokad µs | zimg µs | B/op |
| --- | ---: | ---: | ---: |
| Qualified original, short | 417.84 | 383.88 | 198,449 |
| A: two returned vector temporaries, short | 422.14 | 383.76 | 198,448 |
| B: direct borrowed-cache writes, short | 396.77 | 366.23 | 198,449 |
| Qualified original, independent confirmation | 394.83 | 348.61 | 198,449 |
| B, independent confirmation | 393.77 | 364.13 | 198,450 |

The current pair kernel already shares overlapping windows. This revision combines
two adjacent 16-tap/+4 pair calls: fourteen two-pixel vector loads cover the union
of 28 source pixels, instead of ten loads per call. Eight rolling vectors feed two
ordered accumulators. Existing private geometry markers, checked views and the
second pair's +8 base prove eligibility. Phase breaks, edges, odd tails, full and
unequal support retain actual existing fallbacks. Static source-read reductions
are not cache traffic, phase costs or predicted elapsed savings.

A's 668-byte leaf has no vector stack transfers, but returning two vectors adds
128 B of temporary stores and 128 B of reloads per quartet. B writes to the
borrowed cache directly and preserves the cancellation checkpoint before the
second pair's publication. Its 715-byte helper has no vector stack transfers;
caller code/frame adjustment fall from 10,720 B/1,800 B to 10,486 B/1,672 B.
The preceding qualified caller was 9,550 B/1,560 B. Generated-code smoke elapsed
is discarded. Smaller code and fewer loads do not establish a complete gain.

Every source input and exact managed result stays frozen. No FMA, reassociation,
certificate, repair, alpha, metadata, ownership or numerical policy changes.
Workspace admission remains 198,432 B and independent result capacity 32,768 B;
new owned capacity and retained scratch are zero. JIT frames are separate from
coarse admission and unmeasured physical/native peaks. Ceil-rounded GC differs
by at most two bytes without a capacity change.

A/B each pass Windows/Linux focused normal/scalar checks: respectively 360/361
internal and 747 public cases per mode. Actual kernel tests compare all scalar
lanes and existing pair results, check canaries, phase breaks, bounds, zero
allocation, borrowed-root ownership and cancellation between cache writes.
Prototype-only code/tests and both versions' binaries remain in the raw archive.
Two main/embedded builds per OS, zero production compile failures and zero
harness/native/selector/fixture builds serve the round.

Complete public preparation, allocation, filtering, independent output and disposal
remain timed. The explicitly labeled zimg reference retains graph/input/scratch;
conversion, halo, filtering, quantization, output and release are timed. One CPU,
sequential Release and both idle-v2 guards apply. All actuals and native brackets
are retained. A's short supplied no lead; B's removed-copy hypothesis justified
one independent pair, without more sampling for favorable results.

Sources, main/embedded binaries, public bindings and Linux source manifest are
restored byte for byte. [EQE772B](BICUBIC-SHARED-QUARTET-EXECUTION.md)'s 6,183
full/live checks per OS, affected scalar/ISA and same actual Release package/all
three examples remain valid. No broad suite/package rebuild or seven passing
companion repeats follow a discarded change. Displayed costs do not change and
this experiment is not a renderer dependency.

[Evidence](cached-wide-cross-pair-loads-experiment-evidence.json) pins both designs,
code, confidence and exact restoration. Mirrored/member-verified raw
`cached-wide-cross-pair-loads-experiment.tar.xz`: **8,325,520 B / 199 members**,
SHA256 `be17af5b98cfcaf96721178a4306333ac971b1ec55e907e1c4bd788e7c183520`.
Earlier qualified worker/reference providers are pinned dependencies. Independent
codec work resumes; all 283 required paths remain in scope and publication is
postponed.
