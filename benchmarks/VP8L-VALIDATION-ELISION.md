# Main VP8L validation without discarded pixels

VLS458, 2026-11-07. Accepted architectural stage; publication postponed.
[Current evidence](vp8l-validation-elision-evidence.json) owns the nine existing
affected controls. The full 283-path performance objective remains open.

## Design and contract

`ValidatePixels` previously allocated the main ARGB raster, updated color-cache
values and applied inverse transforms before discarding those pixels. Validation
now uses the same non-generic token/geometry scheduler with a validation mode.
Auxiliary predictor, palette and entropy images still decode numerically. Every
Huffman/channel/prefix read, transform/predictor check, group selection, cache-index
bound and back-reference distance/length check remains. Main colors cannot change
syntax acceptance: they feed only stores, cache hashes, copy values and total
inverse arithmetic. Rendering keeps those operations and exact pixels.

Wide-green tables retain the original eligibility, reader schedule and scratch-use
cutoff. Validation reserves an operation-owned lookup array only when required;
it releases that storage after its users. Logical raster and delayed palette
expansion bounds remain at their original phases. This adds no pool, retained
cache, production dependency, independent allowance or lower-memory retry.
Cancellation remains direct and cooperative. Input/descriptor/output ownership,
terminal shared-budget refusal and asynchronous cleanup remain required.

The source proof mechanically undoes the changes to the entire qualified original
source. That supports the semantic audit; elapsed-time evidence below is separate.
New public tests compare rendering/validation verdicts on authored transform/cache/
group/copy controls and bounded damaged payloads, plus root refusal, cancellation,
cleanup and recovery. Actual-package consumers freeze encoder packets and exercise
bounded validation, full-decode refusal and asynchronous ownership.

## Complete warm costs

Dedicated quiet VM, Release, one CPU. Every independent confirmation has twelve
managed actuals and twelve samples in each native bracket, original output/source
checks and passing idle/process guards. Native codec contexts remain prepared and
retained. Validation compares complete coding validation/independent metadata with
FFmpeg natural-frame decoding plus the source-metadata adapter; it does not require
an RGBA output on either side. Full decoding includes independent normalized output
and disposal. Frozen controls/settings are unchanged.

| Existing control | Managed µs | Reference µs | Ratio / upper | Managed GC B/op | State |
| --- | ---: | ---: | ---: | ---: | --- |
| ValidateVp8lAlpha | 2,330.73 | 3,047.56 | .765 / .769 | 297,070 | Pass; stop |
| ValidateVp8Alpha | 4,656.82 | 5,448.61 | .855 / .858 | 13,584 | Pass; stop |
| WebpResident | 3,248.80 | 3,008.34 | 1.080 / 1.085 | 152,728 | Gap |
| Lossless8 | 1,524.63 | 2,151.92 | .708 / .712 | 10,840 | Pass; stop |
| IndexedNative16Colors8 | 553.66 | 544.24 | 1.017 / 1.027 | 820,194 | Gap |
| NativeLosslessCancelable8 | 3,277.51 | 3,010.67 | 1.089 / 1.096 | 152,728 | Gap |
| WebpStream | 3,297.50 | 3,030.29 | 1.088 / 1.094 | 152,824 | Gap |
| Native8, VP8+ALPH | 6,221.60 | 5,969.45 | 1.042 / 1.047 | 238,384 | Gap |
| Vp8Stream | 6,231.39 | 5,994.69 | 1.039 / 1.046 | 238,480 | Gap |

Contemporary original VP8L validation is 3,361.42µs/native3,050.34µs and685,421B/op.
Independent candidate saves **1,030.69µs/30.66% raw**,30.60% normalized. Filtered and
all-actual raw/native-normalized confidence intervals separate; native movement
is−0.091%. The short screen was separate and is not parity proof.

An apparent full-render slowdown against an older clock prompted one contemporary
original confirmation:3,265.79µs/native3,008.74µs versus candidate3,248.80/native
3,008.34µs. The raw point improves0.52%, but normalized and all-actual intervals
overlap. There is no demonstrated incremental gain/loss or equivalence claim.
Preserve these inconclusive views; do not repeat unchanged samples for a win.

The family is **three passes, six gaps, zero uncertain**. Five displayed costs
refresh;120 numerical rows reuse unchanged paths. Opaque VP8 decoding, encoders,
other codecs, editing and font paths are unaffected. The prior
[VP8 materialization stage](VP8-VALIDATION-MATERIALIZATION.md) and
[lossless group dispatch](WARM-VP8L-GROUP-DISPATCH.md) retain historical evidence.

## Memory and qualification

| Complete validation | First/live admitted peak before → after | Retained after disposal | Total GC before → after |
| --- | ---: | ---: | ---: |
| VP8L | 623,292 → 235,152 B | 0 B | 685,421 → 297,070 B/op |
| VP8+ALPH | 536,248 → 11,121 B | 0 B | 545,478 → 13,584 B/op |

Both OSs measure the same admission peaks on the frozen513×259 source packets,
using identical compiled tests rebound to original/candidate DLLs. VP8+ALPH's GC
before value is the preceding qualified stage, not a new paired CPU comparison.
Admitted capacity, total GC, retained storage and physical peak are distinct.
Native allocation/physical peak are unmeasured. No cross-operation state is added.

Windows/Linux each pass **5,780 full/live checks**:2,515 internal,2,317 public,
948 differential. Focused433internal/295public pass in normal and scalar modes
on each OS. No ISA selection changes; unchanged disabled-ISA evidence is reused.
One immutable main/embedded/public build per OS serves this bounded round;
no benchmark/native/selector/fixture rebuild. Windows core655a27fa… and embedded
88598f67…; Linux core3f6ea9f4… and embedded7abdcc10….

The SAME private Release0.2.1 package and all three usage examples pass on both OSs,
including the new exact-packet/bounded-validation/async consumer. Package
SHA256`c421e79f3c1046cdc0e1a66bf4fd0912fca344c1a72ee90632e05248c13c9898`,
421,689B; symbols`d60803c056417a0363e69a1a3b957eb5249d6102fec11edc709582f31a865756`.
The package was regenerated only after correcting its validation-workspace docs;
production modules and completed regression/timing evidence were reused. Published
packages remain immutable. This is not a125-row actual-package release refresh.

Raw`vp8l-validation-elision-qualified.tar.xz`,6,174,576B/272members,
SHA256`0343d021464500db0a492680f2a591d711f5083e34da5b2e68efae86dbad7fab`,
is mirrored and every member verified. It retains original/candidate modules,
sources, exact commands/worker identities, source manifest, screens/confirmations,
adverse/inconclusive views, memory diagnostics and functional/package logs.
The JSON pins prior archives for byte-identical reused harness/reference inputs.
