# Warm mixed-axis bicubic row reuse at both depths

RGBA16 independent quiet confirmation measures **1.924× FFmpeg's scaler
backend**, conservative upper **1.975×**, at **1,636.58 µs** and **1,813,110 B/op**.
Both guards pass; all twelve actuals are retained, with eleven selected results.
The 17,383.93-µs / 20.419× direct-evaluation baseline falls **90.6%**.

On the same final library/harness, RGBA8 independently confirms **1.848× / upper
1.867×**, at **1,570.03 µs** and **1,283,063 B/op**. All twelve actuals are retained,
with ten selected results; both guards pass. Stop tuning these fixed pairs.

The two existing original 513×259 RGBA8/16 rasters become 1027×129: width grows
while height shrinks. One primitive extends operation-local row reuse to words.
Source products remain exact unsigned integers, horizontal intermediates remain
binary64, and word output keeps unsigned packing and the existing precision
guards and original-order/tiny-alpha fallback.

Row reuse first screens 2.109×. One six-second profile records 48.7% in
horizontal/cache work, 43.6% in word-row output and 3.6% in precision-bound helper
calls. Computing the identical exponent bounds locally removes calls and spills,
screening 1.992×. Adjacent four-tap outputs then share five source vectors,
preserving each accumulation's tap order; edges and five-tap midpoint support
retain the general loop. No approximate reciprocal or pixel tolerance is added.

An initial word-only shared-tap implementation confirms the word pair but screens
RGBA8 at 2.013×. Acceptance is held. Applying the same exact shared-tap path at
both depths restores byte parity. Both final confirmations use the same immutable
production and harness DLLs. The earlier confirmation and regression refusal
remain superseded evidence. A final quiet three-actual RGBA16 enlargement control
also stays below two; it is a regression control, not a new confirmation.

Optional workspace stays **716×output width + 32×source width + 128 +
512×(output width & 1) bytes** for this direction, 752,388 bytes here. Word
allocations rise 650,327 B/op, about 55.9%, against direct evaluation. Byte
allocations are unchanged from the previous row-cache round. Tight budgets,
unsupported hardware, cancellation, work charges, source and held-owner contracts
remain intact. No state or output is cached between calls.

The explicitly labeled native baseline is **zimg, FFmpeg's zscale backend**.
The existing original adapter now selects BYTE/WORD at compilation. Its four-pixel
clamped halo, full source conversion, premultiplied float filtering, independent
planar/packed allocation and release are timed. Packing unpremultiplies using
unrounded alpha, quantizes straight RGBA and clears RGB under rounded-zero alpha.
Word normal RGB/alpha maximum errors are 1, RMSE 0.048112 / 0.038168; scalar
RMSE is 0.052273 / 0.042651. Byte output hashes and bounds remain unchanged.
The explicit native maximum-1 / RMSE-0.25 gates preserve exact managed rational
pixels. Source, geometry and independent owners pass at both depths.

Windows and Linux each pass **2,823 Release tests**, including **896 live FFmpeg
comparisons**, and the consumer plus all three examples through the **same actual
dependency-free Release package**. Five new word cases cover mixed geometry,
exact admission, held owners, edits and tiny/zero alpha. All 157 focused resize
checks pass in normal/scalar modes on both platforms. Package qualification uses
the final production inputs; earlier partial qualification is superseded.

The [numeric evidence](warm-bicubicmixed16-rows-evidence.json) retains baseline,
intermediate screens, superseded confirmation/refusal, final both-depth
confirmations, profile, regression control and qualification. Only three distinct
workloads and the two existing original performance rasters are used. Other
mixed directions, wide support, public boundaries and uniform current-source
parity remain open.
