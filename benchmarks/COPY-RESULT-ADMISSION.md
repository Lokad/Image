# Copy-result admission

Clone, crop, flips and turns now admit their escaping image directly through the
existing memory reservation. Their temporary execution object owned no storage.
All18owned edit controls allocate **56 B/op instead of112 B/op**. Source/result
capacity, the two-slot/4MiB idle store and governed-output bypass are unchanged;
retained operation workspace is zero. Physical/native/JIT peaks are unmeasured.

One independent complete RGBA16 clone comparison confirms **1.68% /0.291µs**:
17.340→17.049µs, paired interval **0.97501–0.99150**,12alternating pairs×1024calls.
Byte flip is inconclusive at0.99631–1.01513; word flip at0.98793–1.00134. Neither
establishes equivalence or a flip gain. Separate word-flip points look adverse,
but paired evidence does not establish that apparent4%causal regression.
Original word normal-copy also has a reference gap. All outcomes remain; no
unchanged favorable repeat or historical phase sum.

| Existing complete513×259 control | Lokad µs | FFmpeg µs | Upper / status | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Clone RGBA16 | 17.33 | 16.62 | 1.054 / gap | 56.0 |
| Clone RGBA8 | 5.61 | 7.84 | 0.734 / pass | 56.0 |
| Normal orientation copy RGBA16 | 17.22 | 16.75 | 1.038 / gap | 56.0 |
| Normal orientation copy RGBA8 | 5.20 | 8.39 | 0.629 / pass | 56.0 |
| PNG decode/orient/crop/fit RGBA8 | 1,235.79 | 1,475.12 | 0.844 / pass | 169,454.8 |
| PNG decode/orient/crop/fit RGBA16 | 2,888.51 | 2,938.65 | 0.990 / pass | 338,159.1 |
| Half turn RGBA8 | 7.90 | 11.79 | 0.677 / pass | 56.0 |
| Half turn RGBA16 | 19.60 | 21.78 | 0.918 / pass | 56.0 |
| Horizontal flip RGBA8 | 8.90 | 9.38 | 0.959 / pass | 56.0 |
| Horizontal flip RGBA16 | 21.43 | 26.99 | 0.803 / pass | 56.0 |
| Mirrored transpose RGBA8 | 30.07 | 37.55 | 0.805 / pass | 56.0 |
| Mirrored transpose RGBA16 | 41.70 | 44.61 | 0.946 / pass | 56.0 |
| Clockwise turn RGBA8 | 28.91 | 37.92 | 0.770 / pass | 56.0 |
| Clockwise turn RGBA16 | 38.90 | 44.03 | 0.892 / pass | 56.0 |
| Transpose RGBA8 | 26.85 | 37.83 | 0.716 / pass | 56.0 |
| Transpose RGBA16 | 34.98 | 43.85 | 0.859 / pass | 56.0 |
| Counterclockwise turn RGBA8 | 28.57 | 37.43 | 0.772 / pass | 56.0 |
| Counterclockwise turn RGBA16 | 38.49 | 44.19 | 0.875 / pass | 56.0 |
| Vertical flip RGBA8 | 8.44 | 8.40 | 1.012 / uncertain | 56.0 |
| Vertical flip RGBA16 | 18.45 | 17.94 | 1.047 / gap | 56.0 |

**16passes, three word gaps, one byte-flip uncertainty** remain. Stop the passes;
the three demonstrated gaps are subsequent work, not hidden by the scoped gain.
Every current reference uses the same immutable packaged DLL, original input/
output identities, exact full samples/alpha/hiddenRGB and independent owners.
Complete public admission, output acquisition/materialization/disposal remain
timed. FFmpeg retains graph/source/frame wrappers, then owns/materializes/releases
output pixels; padded rows need no redundant packing. Compositions retain complete
decode/orient/crop/fit and all managed intermediates. Setup/JIT/IPC/checks are
excluded equally. CPU0, sequential natural nonconcurrent GC, all12adjacent
pairs/native brackets and idle-v2 guards pass;64calls per composition and256per
edit. The paired driver measures managed builds, not standalone FFmpeg parity.

Policy/pre-cancel/disposed-source/local-input/output-limit ordering is preserved.
The existing escaping reservation precedes allocation, remains on the borrowed
caller root until result disposal, and is released after failure without retry.
All pixel kernels are byte-identical after token binding; every live checkpoint
remains. Common codec/resize allocators, pool and ImageBuffer.Create/disposal are
verbatim. This adds no budget, quota, cache, pool or public API. The two removed
composition objects account for112Bof source-derived temporary metadata;
current complete allocation counters are not a paired physical-peak comparison.

Windows/Linux each pass **6,683full/live checks** (3254internal/2481public/948live)
and11internal/221public focused normal/scalar checks.24new public cases exercise
aggregate refusal, local overlap, retained siblings, borrowed scopes, cancellation
cleanup/recovery and failure order. Unchanged arithmetic/ISA proof is reused.
The **same actual Release package and all three examples pass both OSes**.
One core/internal build perOS, one successful public build perOS; the corrected
Linux public-only compile failure precedes tests/timing. One reused-helper
generalization resolves the concrete word-cost concern; no native/harness/fixture
build or post-timing production rebuild. All20current costs reconcile before
broad/package qualification. Published packages remain immutable.

[Evidence](copy-result-admission-evidence.json) pins producers, every capture,
source proof, qualification and raw. Mirrored/member-verified
copy-result-admission-qualified.tar.xz: **3,262,716B/383members**, SHA256
46d4eed930e9dba98874ffb08bede13a2389da0327e080417f26fda8885b9024.
Earlier [copy](SHARED-RASTER-COPY.md), [reversal](SHARED-PIXEL-REVERSAL.md),
[tile](SHARED-AXIS-SWAP.md) and [flip-confidence](VERTICAL-FLIP-CURRENT-CONFIDENCE.md)
reports retain scoped history/raw. Full289-path parity is open; publication
is postponed.
