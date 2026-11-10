# Contiguous packed bicubic views

CCP737 prepares count-sized sample and weight views once per decoded row, under
a once-per-strip proof of equal counts and contiguous source positions. Clipped
and unequal pairs retain the indexed loop. Both depths use the shared scheduler,
unchanged ordered arithmetic, certificates, exact publishers and original-order
repair. No fixed-support class, numerical relaxation or new storage is added.

| Complete 513×259 → 64×32 public resize, 4 KiB workspace | Managed µs | zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 | 458.14 | 370.31 | 1.237 / 1.245 | 113 |
| RGBA16 | 480.01 | 418.59 | 1.147 / 1.159 | 113 |

The quiet VM runs sequentially on one CPU, twelve actuals per control. Managed
preparation, working storage, independent output and disposal stay timed; native
zimg retains graph/scratch, with its declared halo/conversion/repack/ownership
boundary. This is not fresh native graph construction. Exact rational/frozen
managed pixels, native RGB/alpha max 1 / RMSE ≤0.25, source, owners and idle-v2 pass.

Byte improves from 485.70 to 458.14 µs: **27.56 µs / 5.67% raw / 6.41% normalized**, with
separated raw/normalized intervals in filtered and all-actual views; native +0.786%.
Word's 479.19→480.01 µs raw intervals overlap, while native +8.07% changes the ratio;
there is no causal word gain, equivalence or nonregression claim. Both remain
confirmed gaps. No passing family was retuned and no favorable repeat was made.

The first inline stage's 453.98 / native 398.28 µs short point is unconfirmed, with
wide intervals and native clock movement. Actual code retains four per-tap guards.
An equal-length loop-bound revision also retains them and is not statistically
retimed. A framework-only ignored diagnostic on the verified 10.0.8 runtime
compiles the existing standalone helper without those guards, motivating the
final non-inlined boundary. Actual byte-prefix/suffix bodies are 181/178 bytes,
with no per-tap guard or call; complete measurements include the boundary's call
cost. Three immutable main/embedded stages per OS, no production compile failure.
All earlier sources/modules, short actuals and codegen captures remain in raw
evidence; instrumented elapsed is discarded. A rejected wrong-runtime diagnostic
host stops before codegen. This distinguishes attribution from measured gains.

The same 4,096-byte admission precedes 3,880/3,992/4,044-byte phase payloads. New views
borrow the decoded row; prefix loan users retire before result-tail publication
or precision replay. Independent 8,192/16,384-byte outputs stay charged until
disposal, operation scratch retention 0. Existing pool retention is separate;
governed calls bypass it. Total GC 113 B/op and declared capacity are unchanged.
Physical/native peaks are unmeasured; minor aliases/spills are not a peak claim.

All **6,148 full/live checks per OS**, 322 internal/729 public resize checks per
normal/scalar/AVX2-off/AVX512-off, SAME actual Release package/three examples pass
Windows/Linux. The expanded focused filter includes the 11 PackedColumnStorageTests
cases; the new proof covers repeated edges, unequal counts and discontinuities.
Existing layouts/frozen frames, aggregate admission, retained survivors, refusal,
cancellation and recovery pass. Unchanged JPEG 453/375 per-mode and other evidence
is reused. No new image/settings, benchmark harness/native/selector builds or
repeated package proof. Published packages immutable; publication postponed.

[Shared scheduling](PACKED-BYTE-WORD-CONSOLIDATION.md) retains earlier gains;
[attribution](PACKED-BYTE-EXECUTION-ATTRIBUTION.md) retains the model and profile.
[Evidence](packed-bicubic-contiguous-views-evidence.json) pins this stage. Raw
`packed-bicubic-contiguous-views.tar.xz`: SHA256 `6b98c99939eb762654d017e64b702a8026317a5654efde461a3896f70e2cb8ff`,
16,399,004 bytes / 327 members, mirrored and member-verified. The SAME
actual private package SHA256 is `8ef1f2e8efc182dd683b237a9074308d2428e1eb4960b109ce64c18b0a5fca9e`.
This is scoped development evidence, not a full 127-row release refresh or 283-path
completion. The renderer uses a distinct current qualification variable, fixing
its older PNG-variable collision in the footer; underlying test records are intact.
