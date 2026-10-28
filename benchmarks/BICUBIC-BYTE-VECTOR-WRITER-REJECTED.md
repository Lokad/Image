# Bicubic vector-indexed byte writer: rejected

The existing Down8 screen measures 632.938 µs, 1.508× (upper 2.001),
332,187 B/op. The applicable unchanged-production confirmation is 632.921 µs.
There is no meaningful whole-operation gain; no variants or confirmations follow.
This is not a gain or regression claim against unmatched runs.

One six-second current-production profile (5,936 samples) attributes 66.12% to
CachedBicubicResize.TryApply, 29.09% to WriteByteRow and 1.33% to exact replay.
These are diagnostic shares, not component elapsed costs. The rejected trial
reuses the existing vector-indexed byte writer where even widths align cached
rows to complete pixel pairs. Odd-width behavior, arithmetic, guards, exact
replay, storage and admission stay fixed. Exact managed pixels, native RGB/alpha
maximum 1 and RMSE ≤0.25, source, ownership and quiet guards pass.

Candidate and restored builds pass 89 focused public cases normal/scalar on each
platform. The original source, actual DLLs and package are restored exactly;
unchanged full/package qualification is reused. Candidate production is built
once per OS; existing selectors, harness and native worker are reused. Profiling
requires only one small host build.

[Evidence](bicubic-byte-vector-writer-rejected-evidence.json) records samples,
confidence, attribution, pins and restoration. Both raw archives are mirrored
and member-verified:

- bicubic-byte-vector-writer-rejected.tar.xz: 122 members, SHA256
  24ca2efc38b4fdb715bc179783920fd24a54dd433516bd9f92acde06e034f5cf.
- current-bicubic-down8-attribution.tar.xz: 41 members, SHA256
  642b3abe5ca692a259ccbed8ab00b027b0b7fc620c1a77b175c5a2c803113aaa.

The next investigation maps the retained horizontal-filtering samples to current
JIT instructions before selecting another primitive. No new profile or workload
grid is needed. The full 268-path objective remains open.
