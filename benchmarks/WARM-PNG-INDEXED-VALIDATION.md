# Indexed PNG validation

Complete validation of the original 417-byte indexed Adam7 control improves
**66.81% filtered / 66.89% all elapsed**, **67.14% native-normalized**, with
separated raw and normalized confidence: **5.352 → 1.776 µs**, **0.448× / upper
0.460**. Warm allocation falls **1,168 → 792 B/op**. This case passes and stops.

| Existing complete validation | Lokad µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| PNG8, span | 1,417.189 | 1.023 / 1.056 | 240 | confidence-open |
| PNG16, span | 2,805.515 | 0.943 / 0.972 | 240 | pass |
| Indexed Adam7 19×17, span | 1.776 | 0.448 / 0.460 | 792 | pass |
| PNG16, async nonseekable | 2,836.823 | 0.783 / 0.790 | 8,544 | pass |

Indexed validation now materializes certified filtered bytes once, then performs
the existing pass reconstruction and palette-index checks. It reuses the existing
inflation and row machinery, avoiding a second BCL inflation. Capacity is capped
at 256 KiB − 4 KiB and selected only within the old local workspace allowance.
Larger/tight cases retain the original path. Incomplete trees or uncertain Adler
release speculative storage before the original BCL path preserves acceptance
and error order. Admission refusal is terminal. There is no new owner, decoder,
quota or cache. Decode buffering, encoders and pixel arithmetic remain unchanged.

The primary remains 19×17 indexed4 Adam7 with tRNS, five row filters, 209 filtered
bytes and one stored Deflate block. Input/settings, exact descriptors and complete
operation boundaries are frozen. Twelve original-path source hashes match the
valid independent baseline; fresh original means stay within 1.64%. One candidate
screen and independent confirmation clear the default 5% gate. The cached short
selector emits twelve managed actuals with three native samples per bracket;
native confidence uses the proper three-sample critical value. Its count-only
postcheck was recovered without rerunning timing. Final evidence is twelve/twelve.

The three companions are current costs without paired gain claims; native-normalized
point changes range −0.99%..+2.06%, within the 5% guard. Both PNG16 paths pass and
stop. PNG8 remains confidence-open (lower 0.992), neither a pass nor a proven gap.
No new fixture, workload grid, profile or harness/native/selector build was needed.

Both OSes pass **5,310 Release checks each** (2,164 internal, 2,198 public, 948 live),
plus 776 internal/332 public focused checks normal/scalar and Linux AVX512-off.
Five new public checks cover malformed/truncated streams, exact pixels, failure
order, incomplete-tree acceptance, bounds, aggregate admission, refusal and
cancellation cleanup and successful reuse. One immutable core/internal/public
build per OS is reused throughout. The same actual Windows package/DLL passes
both OSes, governed/retained/async consumers and all three usage examples.
Packaged indexed validation peaks at **4,581 shared-admitted bytes**, releases
everything on success/failure and preserves input/root ownership under an 8 KiB
budget. This is a governed-storage measurement, distinct from warm allocation;
native allocations are not measured.

Development package 0.2.1 is 388,039 bytes, SHA256
da88014f4257b801bcf59a17e77f3ebc95860d11dd677146aae65b3e4c6ec69f. Published releases stay immutable.
[Numeric evidence](warm-png-indexed-validation-evidence.json) names all four
affected inventory paths and precise unchanged-path reuse. Mirrored/member-verified
raw png-indexed-validation-qualified.tar.xz SHA256
a28ab627a2f7f212f31bc39693d671e1168f4239171ee0e126bcfc13d6f5f7f9 (8,614,612 bytes, 220 members) retains
source, binaries, package, tests, actuals, guards and proof restoration, with
GFNI and group-dispatch archive dependencies. The full 268-path objective remains
open; passing these controls does not establish full parity.
