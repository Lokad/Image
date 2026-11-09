# Shared wider bicubic columns

CWW661 shares preparation, ownership and row scheduling across wider byte/word
reductions. Constant width/depth modes retain specialized kernels. Eligible word
images gain the existing four-column/17-slot strategy; three-column/20-slot
fallback remains. Relative byte indexes replace the word subtraction pass.
Production source falls477→373 lines; the duplicate word-wide driver is deleted.
No forwarding wrapper, public API, cache, pool, session or owner is added.

On the existing **513×259→128×64 bicubic, input+output+4 KiB** controls, word
saves **89.87 µs /16.75% raw /16.46% normalized** and byte saves **23.58 µs /
4.97% raw /5.17% normalized**. Matched Windows-produced originals and selected
artifacts have separated filtered/all-actual raw/normalized confidence intervals.
Native means move −0.35%/+0.21%; no old cross-artifact point supplies causal gains.

| Complete control | Original µs | Selected µs | zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 | 536.45 | 446.58 | 413.05 | 1.081 /1.090 | 113 |
| RGBA8 | 474.69 | 451.12 | 347.95 | 1.296 /1.302 | 113 |

Both cases remain parity gaps:33.53/103.16 µs. Each original and selected
confirmation has twelve actuals; selected byte retains10 filtered and all12
all-actual results. One three-actual screen per case is retained (445.31/461.94 µs).
There are no timing repeats, new fixtures or new profiles. The separate
[small-byte adverse comparison](BICUBIC-SHARED-BYTE-WORD-COLUMNS.md) is unchanged;
these results do not supersede its uncertainty.

At the frozen word geometry, strips fall43→32, source-row visits11137→8288,
decoded pixels263662→229474, and single-pixel publications2688→0. Original
single-pixel publication already uses SIMD. Tap contributions remain531468;
these work counts are not phase times or predicted speedups. Byte grouping is
unchanged; constant specialization may affect code generation, without assigning
its measured gain to an unmeasured substage.

Original depth-specific certificates, unsigned products, tap order and word
ordinary/tiny-alpha repair remain. The conservative20-tap word error bound stays
in the17-slot mode. Array payloads4068/3776 B share one4096 B admission and never
overlap. Eligible word payload rises only12 B within that charge; geometry borrows
9728 B of already owned output, down from11264 B for word. Retained outputs own
65536/32768 B until disposal. New owned heap capacity and retained scratch are
zero. Allocation113 B/op is distinct from governed capacity, physical/native
peaks and existing idle pixel-pool capacity; the latter remain unmeasured.

Timing includes complete public Resize, preparation, normalization, working
storage, independent output ownership and disposal. Explicitly labeled zimg,
FFmpeg's scaler backend, retains graph/scratch before timing; halo/deinterleaving,
processing, quantization and independent output remain timed. This is not fresh
graph setup parity. Exact managed/frozen/rational pixels and approved native
max1/RMSE≤0.25 remain intact.

Windows66185497/Linuxfff91357 each pass **6004 full/live checks**, plus225 internal/
703 public per normal/scalar/disabled-AVX512/disabled-AVX2 mode. Ten added cases
and existing checks cover generalization, exact4.125, partial strips, geometry retirement,
poison, terminal refusal/cancellation, tiny-alpha unwind/replay and surviving
aggregate ownership. Three-column fallback retains functional checks; broader
shape/mode performance is not claimed. The SAME actual private Release0.2.1
packagec2c3d209 (435986 B) passes both OS consumers and all three examples. One
main/embedded build per OS is reused throughout; no public/harness/native builds.

[Structured evidence](bicubic-shared-wide-columns-evidence.json) pins current
costs, every actual/guard, sources, actual package and qualification. Mirrored,
member-verified raw bicubic-shared-wide-columns-qualified.tar.xz has12826244 B /
295 members, SHA25694eefda9adf1e4876aa1f0f395183907d7e011192e4cc03427d225bc1d47f40e.
The [earlier shared geometry](BICUBIC-WIDE-PREPARATION.md) is retained history;
unchanged paths reuse source-valid evidence. Full283-path parity remains open,
and publication is postponed.
