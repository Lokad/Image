# Identity-axis word bicubic geometry

Current mixed/vertical costs are superseded by [result-backed bands](BICUBIC-RESULT-BACKED-BANDS.md). This report retains stage history and raw evidence.

Unchanged-width RGBA16 reduction now selects its exact center tap at entry.
The existing band loop, column cache, vertical accumulation, checked publication,
repair and ownership remain shared. Repeated horizontal kernel preparation and
four zero-weight taps disappear. No public API, pool, session or owned array is
added; two private geometry variants specialize the same source loop.

| Complete existing control | Previous µs | Current µs | zimg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| TightVertical16, 513×259 → 513×129, 4 KiB scratch | 730.32 | 458.51 | 589.46 | 0.778 / 0.779 | 113 |

Independent twelve-actual confirmation follows the 485.35 µs short screen.
The gain is **271.81 µs / 37.22% raw / 36.98% reference-normalized**; filtered and
all-actual intervals separate in both comparisons. Native moves −0.38%.
The current conservative upper passes parity: **stop this case**. The original
source-valid confirmation is reused; no favorable timing repeats or new inputs.
Fourteen other word controls retain valid evidence, leaving **7 passes / 8 gaps**
across the existing fifteen-case inventory. This is not a general resizing claim.

For equal width N≤2²⁶, the original center calculation is exactly x in binary64.
Bicubic weights at integer offsets −2…2 are exactly 0,0,1,0,0, with normalizer 1.
Larger caller-permitted widths retain the original geometry. Original repair
still receives x−2; vertical tap order, quotients, certificates, rational pixels,
full sixteen-bit samples and scoped native pixel bounds are unchanged.
One current two-second capture motivated the change: 1,795 of 1,995 samples were
in the shared band driver and 159 in Kernel. Samples are code locations, not
exact phase costs or predicted gains; instrumented elapsed is discarded.

Complete public resize includes admission, geometry, filtering, independently
owned output and disposal. The frozen RGBA16 source and settings remain.
The reference is explicitly **zimg, FFmpeg zscale's backend**, with graph/scratch
prepared before timing, packed-source conversion and output ownership timed.
This is not fresh-graph setup parity or an FFmpeg CLI comparison. Managed exact
arithmetic remains distinct from native float intermediates.

GC stays **113 B/op**. Explicit/admitted scratch stays **3,992 / 4,096 B**,
or **7,596 / 8,192 B** for the existing larger band. Scratch retention is zero;
the result owns **529,416 B** until disposal. Existing idle pixel-pool retention
is separate and unchanged; physical/native peaks are unmeasured. Admission
refusal remains terminal, and cancellation and exception cleanup retain their
existing boundaries.

Windows/Linux each pass **5,905 full/live checks**, plus **73 internal / 688 public**
affected checks per normal/scalar mode. Direct identity controls cover odd and
single-column edges, row tails, both band sizes, tiny-alpha rejection, refusal,
cancellation and recovery against the original fallback. The SAME actual Release
package and all three examples pass both OSes. Its 4/8 KiB vertical controls use
the frozen input/output hashes and verify aggregate admission, retained owners,
refusal, cancellation cleanup, recovery and independent survivors. Eight-KiB
controls establish correctness/ownership, not additional CPU scope.
Linux exercises the SIMD bands; Windows and disabled-intrinsic qualification
exercise portable behavior within the same memory envelopes.

[Evidence](bicubic-identity-word-geometry-evidence.json) pins producers, complete
actuals, confidence, identities, memory boundaries and qualification. Main and
embedded tests build once per OS; prepared workers/references/fixtures are reused.
Raw `bicubic-identity-word-geometry-qualified.tar.xz`: 6,085,516 B / 116 members,
SHA256 `27c8c62ff1455b2e30094283ce6f13f99bdb603d841da00d9dbee3e33b087e0a`,
mirrored and member-verified. Retained rva576/rvi577 scripts and cited providers
reproduce the stage. [Shared bands](BICUBIC-BOUNDED-BAND-EXECUTION.md) retain
their unchanged mixed-axis costs and historical gains. This is a scoped refresh,
not a full actual-package release refresh; published packages remain immutable
and publication is postponed. The full 283-path objective remains open.
