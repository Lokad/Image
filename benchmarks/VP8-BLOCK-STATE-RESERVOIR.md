# VP8 block-local arithmetic state

The noncancellable coefficient reader now keeps position, range, future-bit count
and code value local across a complete block. An unshifted UInt64 reservoir appends
32 bits at refill; normalization changes the range/count without shifting the code
on every decision. The same production token/category/sign kernels serve the direct
oracles. Superseded field-based implementations are removed.

This changes representation, not arithmetic precision. Token order, probabilities,
scan/dequantization, nonzero contexts and the exact scalar cursor remain unchanged.
Header/short-input and independent per-bit cancellable readers are unchanged.
The existing partition/window selection and 272-byte guard remain: at most 304
decisions require at most 268 refill bytes plus the initial byte, within 272.
The reservoir uses at most 47 bits; publication needs no additional input read.

The independent complete alpha-validation control saves **206.84 µs / 3.35% raw**
and 3.34% native-normalized. The unchanged-path original twelve actuals remain
valid; native timing changes −0.012%. Both filtered and all-actual confidence views
separate. The preceding independent screen is retained, not substituted for proof.

| Existing complete operation | Managed µs | Reference µs | Ratio / conservative upper | Managed B/op | Raw saving µs |
| --- | ---: | ---: | ---: | ---: | ---: |
| ValidateVp8Alpha | 5,967.45 | 5,442.74 | 1.096 / 1.106 | 554,478 | 206.84 |
| Native8 | 6,236.04 | 6,022.00 | 1.036 / 1.045 | 238,384 | 157.58 |
| StreamNative8 | 6,266.42 | 5,985.24 | 1.047 / 1.051 | 238,480 | 105.94 |
| Opaque8 | 5,125.26 | 4,789.87 | 1.070 / 1.079 | 760,564 | 154.84 |
| SimpleOpaque8 | 5,252.51 | 4,890.58 | 1.074 / 1.080 | 760,564 | 149.10 |
| OpaqueCancelable8 (unchanged, reused) | 5,468.99 | 4,783.96 | 1.143 / 1.150 | 760,564 | — |

All five affected complete controls have separated raw and normalized gains in
both confidence views. Native points move +0.86%, −0.29%, +0.12% and −0.03% for
span, stream, opaque and simple respectively. All six remain parity gaps.
The unchanged live reader reuses valid evidence; no favorable repeat is needed.
Two displayed numerical rows refresh; 123 reuse unchanged paths. This is not a
full actual-package release refresh or completion of the 283-path objective.

Validation retains its frozen 115,542-byte packet and exact 36-byte descriptor;
it parses full coding/container/source metadata without an escaping pixel image.
The alpha decode controls instead use the frozen 98,590-byte packet and exact
531,468-byte RGBA frame. These are different inputs. Full public operations,
normalization, working storage, owned output and disposal remain timed. Stream
delivery includes a fresh seekable source and acquisition/EOF. The explicitly
labeled FFmpeg software reference retains its declared codec/conversion contexts;
no fresh-context setup parity is claimed. Source/native/output identities, scalar
checks, original alpha/pixel gates, one-CPU execution and idle-v2 guards pass.

No heap capacity or retained storage is added. The window's stack layout grows by
at most 8 bytes within the unchanged 8 KiB allowance. Actual Linux ReadCoefficients
uses 2,144 bytes of code and 72 bytes of stack subtraction, with no ordinary helper
calls; those observations are not independent CPU proof. Existing alpha ownership
remains: coarse admitted peak 751,768 bytes on its original admission control,
retained output 531,468 bytes. The existing capped ungoverned pixel pool leaves one
such array idle after sequential decoding; governed calls bypass it. GC totals,
admitted capacity, physical peak and retention are distinct. Physical/native
allocation is not newly measured.

The transition includes two retained earlier implementations. Stage A's correct
UInt32 field-local design left an out-of-line category call and was revised before
timing. Stage B forced inlining but its independent 6,607.53/5,739.32 µs observation
was inconclusive/adverse; no gain was accepted. Its first screen unexpectedly ran
12 managed/3 native actuals through a cached alias. Completed logs were recovered
without replay; no confidence/parity claim uses that mixed-count screen. Final
UInt64 stage C is a material revision, with 12/12 screen and independent confirmation.
All intermediate binaries, source, JIT, points and protocol recovery are retained.
There are three Windows/two Linux production revisions in the experiment; only
the final immutable module per OS is reused throughout C measurement/qualification.

Windows/Linux each pass **5,750 full/live checks** (2,494 internal / 2,308 public /
948 differential), plus 55 internal / 131 public per normal/scalar mode. Separately
disabling AVX2 and AVX512 passes 55 internal / 42 public each. Coverage includes
4,096 production token/category/bit cursor oracles, 1,024 blocks, 24/25-block windows,
all eight maximal-category phases, strict partition/short-input boundaries, original
cancellation ordinals, 164 frozen frames, admission refusal, failure cleanup, async
and independent surviving owners. No benchmark/native/selector/fixture build occurs.

The SAME actual private Release package and all three usage examples pass both
OSes. Package `d2fcf61f3c1208c64dd4859f13a79f5c9695d2b33390a08910aa91876f2beee1`
is 419,293 bytes. Windows main
`d75951bbc1f12776b044d878bedd770acd8adb3b76ab11f191c96edbc4e29a28`;
Linux main `270f2904a1b1f9630f21d8ed31d8c07ee07ae4674a8757035537efb91ee814b3`.
Published packages remain immutable; publication is separate and postponed.

[Evidence](vp8-block-state-reservoir-evidence.json) records current costs, source
proof, confidence comparisons, retained storage and same-package qualification.
Raw `vp8-block-state-reservoir-qualified.tar.xz`: 3,646,660 bytes / 279 members,
SHA256 `f4ee950171f4e6ea02d101022aeb49140cac243ab1d8b8eb0d29adfdff5b5186`,
mirrored and member-verified with pinned hash-addressed dependencies. Its current
attribution dependency is `vp8-current-validation-state-attribution.tar.xz`,
SHA256 `89398293287135112cfdc17075d56cec7ca2f62660449faf7eb4ff689dbd4794`.
Earlier [alpha ownership](VP8-ALPHA-OUTPUT-HANDOFF.md),
[live reservoir](VP8-UNSHIFTED-LIVE-RESERVOIR.md) and
[encoder preparation](VP8-TRELLIS-CANDIDATE-PREPARATION.md) retain their own evidence.
