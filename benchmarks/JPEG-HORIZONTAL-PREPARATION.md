# JPEG horizontal inverse preparation

The complete horizontal-CMYK decode saves **4.03% / 16.92 µs**
in 12 alternating immutable-DLL pairs: 419.79→402.86 µs,
ratio interval 0.954–0.966. All 28 existing RGBA/fresh/caller-buffer
reference views independently pass and stop. No new image or settings grid.

| Complete original 513×259 decode / FFmpeg reference | Lokad / native µs | Upper | GC B/op |
| --- | ---: | ---: | ---: |
| Horizontal CMYK, retained native contexts | 410.41 / 418.11 | 0.988 | 20,944 |
| Horizontal CMYK, fresh contexts both sides | 372.15 / 753.22 | 0.498 | 20,944 |
| Sparse-2D CMYK | 414.93 / 433.23 | 0.970 | 20,944 |
| Dense CMYK | 1,280.01 / 3,113.05 | 0.413 | 20,944 |
| DC-only caller CMYK | 322.49 / 331.83 | 0.994 | 1,085,981 |
| Progressive grayscale | 1,588.74 / 1,711.21 | 0.933 | 151,432 |

Exact post-dequantization support now selects 1/2/8 rows in the existing shared
inverse. A known-zero second row needs no materialization/read; other two-row
blocks retain their actual zero-result test, including wrapped constant rows.
The completed eight-byte output broadcasts to all eight rows. Original matrix,
word bias, products, wrap, shifts, saturation and clipping stay exact. Disassembly
confirms the work removal: 1,589→1,614 code bytes, same 280-byte frame. Instrumented
elapsed and unverified historical profile offsets supply no phase-cost evidence.

Every native confirmation retains 12 actuals and 12 samples before/after, both
filtered/all-actual views and idle-v2 guards. Public admission, full decode,
normalization, independent output acquisition and disposal stay timed. Ordinary
native views retain decoder/frame/packing contexts; the fresh view includes their
creation/disposal. Frozen packet, pixels, normal/scalar bounds, alpha, source,
caller padding and independent survivor checks stay unchanged.

Historical 434.53/native 392.72 µs and current 410.41/418.11 µs clocks are not a
causal saving comparison. Use the controlled 4.03% increment. RGB444's contemporary
standalone 1,131.76→1,157.64 µs point is adverse 2.29%, but its conservative
normalized interval 0.9994–1.0457 includes 1. One targeted controlled pair resolves
the suspected contradiction: ratio 0.930, interval 0.925–0.934, favorable. Both
contexts and adverse samples remain; no universal 7% RGB444 saving or isolated
instruction attribution is claimed. No successful standalone capture is repeated.
Exact modules are restored/reinstated without rebuilding during this resolution.

Main allocation remains 20,944 B/op. No owned/admitted capacity or retained scratch
is added. The unchanged default idle store has two slots / 4 MiB and is bypassed for
caller-governed storage. Total allocations, live capacity and retention stay
distinct; physical/native/JIT peaks are unmeasured.

Windows/Linux each pass **6,602 full/live checks**: 3,243 internal / 2,411 public / 948 live.
JPEG-focused 466 internal / 342 public checks pass per normal/scalar/AVX512-off/AVX2-off
mode on each OS. Existing independent full-width/mixed-spectrum matrix coverage,
memory refusal, cancellation, async lifetimes and surviving owners remain.
The **same actual Release package**, containing the timed Linux DLL, passes both
OS consumers and all three examples. One immutable core/embedded build per OS,
one adapted RGB444 diagnostic-driver build, no public/native/fixture build,
one Release/--no-build pack and no production rebuild after timing.

Only the shared inverse's pixel-producing callers invalidate costs: 24 RGBA/fresh
and 4 caller-buffer views refresh. Two coding-only JPEG validators do not call it;
encoder/PNG/WebP/edit/resize/font/text timing and ISA proof remain source-valid.
Full correctness and actual-package gates preserve their contracts.

The preceding raw-row normalization trial remains inconclusive: 424.74→427.75 µs,
ratio interval 0.996–1.019. Its prototype-only entry/tests are removed; no broad or
package campaign follows it. This is not a permanent family ban.

Current image views: **174 pass / 4 gaps / 13 uncertain**, 191 comparisons. All 289 material
paths remain required and publication stays postponed. This is a narrow tested
scope, with no whole-library or arbitrary-size performance claim.

[Evidence](jpeg-horizontal-preparation-evidence.json) owns all 28 costs, identities,
complete boundaries, comparison decisions and qualification. Raw
`jpeg-horizontal-preparation-qualified.tar.xz`: 1,770,036 B / 51 members,
SHA256 `b5bcafb5913deaa70d29d0ecc7b8b07ec4ace16c357411bc714d752ad078c9ee`, mirrored and member-verified. Its first-snapshot dependency
retains all original/candidate captures, the inconclusive row trial, exact restored
checkpoint and frozen source. The supplement adds companion reconciliation,
reinstatement, both OS qualification and actual package. Original tools, inputs,
profile and previous qualified package remain in the linked raw dependencies.
