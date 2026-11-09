# Narrow progressive JPEG preparation

JNP705 narrows scan-order AC coefficients to words **before** permutation and
dequantization. Two word permutations replace the natural Int32 scatter/read;
the original single-block inverse kernel remains shared. Two progressive
materializers use one explicit internal input-order flag. Full-width DC products,
word wrapping, bias, saturation, shortcuts and exact managed pixels are unchanged.
The 128 B inverse-zigzag data is independently generated from T.81 geometry.

Independent reversed confirmation saves **107.15 µs / 3.11% raw / 3.24%
reference-normalized** on complete live-token 4:4:4 decoding: 3,445.90→3,338.76 µs.
Filtered and all-actual raw/normalized confidence separate; all-actual saving is
99.87 µs / 2.90%. Original/candidate three-actual screens remain retained.

| Complete progressive decode, 513×259 | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Grayscale | 1,630.45 | 1,711.66 | .953 / .964 | 151,432 | Pass |
| 4:2:0 | 1,559.16 | 1,948.18 | .800 / .809 | 243,664 | Pass |
| 4:2:2 | 2,201.41 | 2,439.31 | .902 / .913 | 315,496 | Pass |
| Ordinary 4:4:4 | 3,172.92 | 3,355.21 | .946 / .949 | 169,664 | Pass |
| Live-token 4:4:4 | 3,338.76 | 3,355.58 | .995 / 1.012 | 169,664 | Uncertain |

The shared entry's flag/ABI can affect baseline costs, so all 27 affected existing
decoder comparisons refresh on the same immutable module; two pixel-free coding
validation paths reuse unchanged evidence. Current 29-read scope is **18 pass /
7 gap / 4 uncertain**. Raw RGB422, resident/stream restart420 and live444 remain
uncertain. No workload or quality grid is added, and no full JPEG parity is claimed.
Companion point differences are not causal gains.

Sparse CMYK's adverse point receives one contemporary original comparison:
522.84→531.60 µs, with overlapping raw/normalized bounds. Restart's comparison is
1,072.96→1,021.84 µs, but native clocks differ and normalized bounds overlap.
Neither establishes regression, nonregression or equivalence. The former passing
restart control's wider current uncertainty remains visible. Completed candidate
jobs are reused; there is no favorable candidate repeat.

Unlike the [paired-component study](JPEG-PAIRED-COMPONENT-PREPARATION.md), this
revision retains the existing inverse kernel and narrows before reordering,
without paired execution or per-frame quantizer plans. Actual production JIT
contains two word permutations and two word products; its one-decode smoke keeps
the exact frozen output and discards instrumented elapsed. Instruction counts
alone establish no speedup.

Independent 640-basis and 2,048-dense frozen hashes pass for both input orders;
full-width coefficient/quantizer cases, canaries, zero heap allocation and
precancellation also pass. Modular-word arithmetic proves AC equivalence for the
entire Int32/UInt16 domain; DC separately uses its original Int64 product/clamp.
Windows/Linux each pass **6,077 full/live checks**, plus 448 internal/375 public
checks per normal/scalar/AVX2-disabled/AVX512-disabled mode. The SAME actual private
Release package and all three examples pass both OSes.

Primary allocation remains 169,664 B/op. No production array, owner, reservation,
pool or retained operation scratch is added. Existing admission/retained-result
evidence remains source-valid: grayscale peak 699,834 B, result 531,468 B until
disposal and default idle coefficients 549,120 B; governed calls bypass pools.
The helper's captured 64 B JIT frame and 128 B module data are separate from those
coarse charges. Full physical/native/stack peaks are unmeasured.

Complete public parsing/preparation, entropy, normalization, independent output
and disposal remain timed, with original inputs/settings/gates on the quiet VM,
sequential Release/one CPU. FFmpeg retains its labeled codec/input/frame/conversion
contexts and has no matching live cancellation token. This is not fresh-context
equivalence. The intended Windows package DLL supplies timings; Linux-native code
is separately qualified. Encoders, strict metadata exceptions, shared admission,
cancellation, ownership and other codec/editing contracts remain unchanged.

One main/embedded build per OS serves the round. A caller-output summary alias
initially failed before actuals; its frozen enum/sample-mode mapping was restored,
preserving all 25 completed jobs. No harness, selector, native or fixture rebuild.
This scoped development checkpoint does not refresh every displayed release row.

[Evidence](jpeg-narrow-scan-preparation-evidence.json) pins current costs, proofs,
every job, adverse results, qualification, runtime providers and actual package.
Mirrored/member-verified raw `jpeg-narrow-scan-preparation-qualified.tar.xz`:
6,909,304 B / 399 members, SHA256
`22d40ca3c459957ce5cdb0da95c65009506fdd7c95f45e57f4abdffc58d3cf9d`.
Private package: 439,043 B, SHA256
`096496456574d01b6f75a647d6ae425bf4cd4a00c27c1d76bd30060f2b1b1c40`.
Published packages remain immutable; publication is postponed. The full 283-path
objective remains open, with independent resizing work next.

[Paired tile packing](JPEG-PAIRED-TILE-PACKING.md) supplies later affected JPEG costs
and current qualification; this report retains valid unchanged progressive paths.
