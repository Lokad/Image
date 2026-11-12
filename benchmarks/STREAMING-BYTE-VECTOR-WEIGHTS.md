# Streaming bicubic paired weights

The byte streaming filter now forms paired weights with two broadcasts and one
half insertion. This removes repeated moves/unpacks from the inner tap loop.
Every lane bit, product, ordered addition, certificate and exact repair stays
unchanged. No preparation is omitted, and no buffer, cache, session or API is added.

| Complete RGBA8 Resize, 513x259 input, extra 4 KiB | Before / current us | Prepared zimg us | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| 16x8 | 511.03 / 463.98 | 483.72 | 0.959 / 0.970 | 113 |
| 8x17 | 545.89 / 457.40 | 516.75 | 0.885 / 0.890 | 113 |
| 8x4 | 501.90 / 380.48 | 663.44 | 0.573 / 0.577 | 113 |

All three independent confirmations pass and stop. The 16x8 and 8x17 gaps close;
the affected 8x4 control was already passing. Raw savings are 47.05 / 88.49 /
121.42 us, or 9.21% / 16.21% / 24.19%. Reference-normalized gains are 15.24% /
17.96% / 23.47%; native clocks move +7.12% / +2.14% / -0.94%. Filtered, all-actual
raw and normalized confidence intervals separate. All actuals and both reference
brackets remain, including the initial three-actual screen. There is no uniform
release-wide speed claim.

The one expression preserves the existing lower/upper tap addition order and
unsigned alpha products. The original file reconstructs byte-for-byte. The
actual timed byte filter shrinks from 460 to 410 code bytes; instruction counts
alone establish no gain. All three captures retain the same 2,579-byte shared
caller and 9,388-byte public root, including instructions and block offsets,
after only address/method-number and proven GS-cookie normalization. The cookie
is recognized from its stored prologue value and two matching fail-fast exit
checks; no arithmetic constant, branch or stack offset is normalized. Word and
other callers retain valid evidence.

Managed preparation, admission, filtering, exact pixels, independent output and
disposal stay timed. The explicitly labeled zimg backend of FFmpeg's zscale
retains its graph/scratch, while conversion, halo handling, filtering, quantization
and independently owned output stay timed. This is not a fresh-native-graph or
FFmpeg CLI comparison. Frozen inputs/settings, rational pixels, native bounds,
source/owner checks, single CPU and idle-v2 guards pass.

Explicit stack arrays remain 4,048 B within the existing 4,096 B charge. No new
owned capacity or retained scratch; total allocation stays 113 B/op. Physical,
native and JIT-stack peaks are unmeasured.

Windows and Linux each pass 6,497 full/live checks, plus 569 internal / 791 public
resize checks normally and with intrinsics disabled, including the three named
storage suites. Disabled AVX512/AVX2 fallback evidence is reused behind unchanged
guards. The same actual Release package passes on both OSes with all three usage
examples. Its existing streaming consumer now covers all three frozen controls
with shared refusal, cancellation, recovery and independent survivors. One core
and embedded build per OS, no public/harness/native/fixture rebuild, one package.

Across 73 standalone resize controls: 62 pass, 3 gaps, 8 uncertain. The other 70
costs remain source-valid. Remaining resize gaps are bilinear byte reduction
(16.63 us excess), bounded bicubic reduction (7.73 us) and wide byte reduction
(2.47 us). Codec/text uncertainties and the finite 289-path objective remain open;
publication is postponed.

[Evidence and reproduction](streaming-byte-vector-weights-evidence.json) retain
the source/module/package identities, scoped actual-package profile, every
timing/qualification result and raw dependencies. The mirrored archive is
streaming-byte-vector-weights-qualified.tar.xz (1,247,800 B, 107 members), SHA256
6c9d2c3cf6be0ae0a9dbfa419a807dd924e53c0aaf24eebbf1c1353ff71b279d.
