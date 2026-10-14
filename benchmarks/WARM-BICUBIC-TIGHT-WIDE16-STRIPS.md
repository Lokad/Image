# Warm RGBA16 wider-support bicubic with a tight working limit

Independent quiet confirmation measures **1.453× FFmpeg's scaler backend**,
conservative upper **1.470×**, at **602.40 µs / 65,658 B/op**. All twelve actuals
are retained, eleven selected; both guards pass. The corrected direct baseline
was 10,028.95 µs / 23.963×: the accepted change reduces its mean **94.0%**.

The existing original 513×259 RGBA16 raster becomes 128×64 with a working limit
of **1,132,568 bytes: input + output + 4 KiB**, excluding the full row cache.
One separate word specialization reuses three-column strips and at most twenty
source rows/taps per axis within 4-KiB admission (4,056 bytes of fixed arrays).
Full UInt16 products are unsigned 32-bit integers converted exactly to binary64.
Checked wide stores, fixed offsets and a common-tap prefix retain independent
unrounded filtering. Paired and single outputs use guarded unsigned packing.
No heap workspace, persistent state or output cache is added. Baseline allocation
is 65,664 B/op; raw figures include harness overhead.

The `2^-40*20^2` error budget scales with 65535 and squared color products.
Uncertain ordinary-alpha pixels replay their original tap order locally when
normalization is at least 0.5 and computed normalized alpha at least two.
Exact tiny-alpha work declines the complete strip operation, unwinding frame
and admission before original replay needs scratch. Every frozen managed sample
matches the independent rational reference exactly. Work, bounded cancellation,
source and owner contracts remain. Existing byte and short-support word kernels
stay unchanged; unsupported supports/windows/hardware retain prior paths.

The baseline remains **zimg, FFmpeg's zscale backend**, with timed eight-pixel
clamped padding (9.489% extra input area), full-word source conversion,
premultiplied float filtering, independent output allocation, unrounded-alpha
unpremultiplication, word quantization, rounded-zero-alpha RGB clearing and
release. Normal/scalar native maximum-one RGB/alpha and RMSE-0.25 bounds pass.
The separate vertical-enlargement allowance does not apply. This is the labeled
backend, not FFmpeg CLI; native memory is neither constrained nor measured.

The initial selection accidentally used default memory; its results are retained
**unscored**. The corrected setup asserts the declared tight budget before timing,
and receipts verify it. The selected native identity is checked before preparation;
only that stale worker rebuilds. Final screen/repeat and byte wider-support
regression reuse identical production/harness assemblies and verified generated
children. The regression passes its mean gate at **1.684×** before qualification;
it is a screen, not a new independent certification. No additional profile or
original performance raster is needed.

Windows and Linux each pass **2,983 Release tests**, including **896 live FFmpeg
comparisons**, plus **332 focused checks** in normal/scalar modes. Eight new word
cases cover frozen pixels, unsigned products, admission/work, strip/support tails,
negative/tiny alpha, source edits, cancellation and independent retained owners.
The consumer and all three examples pass through the **same actual dependency-free
Release package** on both OSes. Full qualification runs once per platform for the
accepted production change.

The [numeric evidence](warm-bicubictightwide16-strips-evidence.json) retains the
unscored diagnostic, corrected baseline, final confirmation/control and qualification.
Stop this pair. Larger/unmatched supports, other tight-memory directions and
remaining material required/current-source uniform parity stay open.
