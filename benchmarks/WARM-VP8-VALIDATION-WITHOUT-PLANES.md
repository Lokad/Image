# Complete VP8 validation without discarded planes

Complete VP8+ALPH validation independently uses **7.93% less filtered / 7.89% less
all elapsed time** and **215,614 fewer allocated B/op**. Raw 99.9% and conservative
normalized intervals separately separate. Five full-decoding companions have
current independent costs, without paired gain or regression claims. All six
remain gaps to warm single-CPU parity; the full 268-path objective stays open.

| Existing complete operation | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| VP8+ALPH complete coding validation | 6,685.351 | 1.227 / 1.232 | 554,478 |
| VP8+ALPH resident decode | 7,092.627 | 1.190 / 1.194 | 1,434,731 |
| VP8+ALPH fresh seekable stream | 6,950.267 | 1.159 / 1.162 | 1,434,827 |
| Opaque normal-filter decode | 5,750.182 | 1.200 / 1.207 | 760,564 |
| Opaque normal-filter, live token | 5,786.203 | 1.208 / 1.218 | 760,564 |
| Opaque simple-filter decode | 5,902.680 | 1.205 / 1.217 | 760,564 |

Validation and decoding share one parser. Validation consumes every header, mode
and coefficient with the original entropy contexts, quantization and Walsh step,
then omits discarded planes, prediction, block reconstruction and filtering.
The parsed mode domains make the skipped prediction error branches unreachable;
padded geometry supplies complete blocks. No actual format/unsupported/truncation
check is removed. All decoder pixels, encoder packets and public I/O stay fixed.

The existing 115,542-byte, 513×259 input and exact nine-field descriptor are frozen.
The reference retains its approved natural-frame/coding adapter: complete coding,
source metadata, ownership and cleanup, without RGBA conversion/output. Full
decoder companions retain their original packets, normalization, independently
owned RGBA and release; stream costs include acquisition/copies/EOF. Every original
pixel/alpha/source/owner/native and quiet gate passes. Native allocation is unmeasured.

The separate governed accounting of that same input drops **770,256→554,704
allocated B/op** and **751,800→536,248 B shared peak (28.67%)** on both OSes and
normal/scalar modes. Full-decoding allocation and peak stay 1,434,800/884,699 B.
A new 640 KiB root test fails the original, succeeds without planes, preserves
live-token behavior and proves terminal refusal, cancellation cleanup, later
success and caller root/input ownership. No new admission mechanism or fallback.

Both OSes pass **5,219 checks each** (2,111 internal, 2,160 public, 948 live),
63 internal/303 public focused checks in normal/scalar modes, the same actual
Release package, governed async/two-frame-alpha consumers and three examples.
All 164 reconstruction fixtures stay exact; new validation covers the whole
corpus and bounded malformed payloads, including errors beyond the complete header.
Public lossy checks now also cover asynchronous pending users and aggregate roots.
Before timing, a live-token assertion expands one new public test; only that test
is rebuilt/rerun normal/scalar, and subsequent full qualification uses its final source.

One immutable core per OS; zero harness/native/selector builds. One fresh short
pair, reused valid independent original twelve and one candidate twelve; one
twelve per five existing companions. Their normalized changes range from -3.83%
to +2.57%, within the predeclared 5% guard; no isolated causal changes are claimed.
No profile, variant, new performance input/settings or successful timing repeats.

[Evidence](warm-vp8-validation-without-planes-evidence.json) pins producers,
proofs and the mirrored/member-verified vp8-validation-without-planes-qualified.tar.xz.
Other affected VP8 boundaries need current costs before CPU-pass claims; ordinary
VP8L and other unchanged paths retain applicable qualification.
