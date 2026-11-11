# Shared bilinear preparation

[Ordered horizontal windows](BILINEAR-ORDERED-HORIZONTAL-WINDOWS.md) now owns
both reduction costs; [vector certification](BILINEAR-VECTOR-CERTIFICATION.md)
and its adjacent view supersede Up16. This stage retains the unchanged Up8 cost,
original preparation design and historical gains.

BGI801B moves byte reduction ahead of an unused horizontal plan and shares
vertical geometry preparation with word reduction. Caller-local arrays preserve
execution facts for the JIT; the pixel loops, ordered weights, certificates,
original replay and tiny-alpha unwind remain unchanged. No public API, cache,
pool, retained workspace or numerical-policy change.

Independent complete Down8 saves **68.39 µs: 14.00% raw / 13.72% normalized**
against the valid BIA800 baseline. Filtered/all-actual raw and conservative
normalized confidence intervals separate. Allocation falls **25,785→8,473 B/op**,
17,312 B less; allocation alone is not evidence of the CPU gain.

| Existing complete public Resize | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Down8 | 420.16 | 385.48 | 1.090 / 1.101 | 8,473 | gap |
| Down16 | 465.51 | 449.98 | 1.035 / 1.051 | 8,473 | gap |
| Up8 | 2,091.11 | 2,296.62 | 0.911 / 0.916 | 102,891 | pass |
| Up16 | 2,724.96 | 2,146.42 | 1.270 / 1.316 | 4,498,923 | gap |

The four frozen controls are 513×259→256×129 or 1027×519, byte/word. Down8/16
and Up16 remain gaps; Up8 passes/stops. Up16 is an existing required case, not an
added displayed row. All twelve actuals, both native brackets, source/output hashes,
independent owners, exact rational managed frames, native bounds/normal-scalar CLI
and quiet CPU0 guards pass. No new image/settings grid or native worker build.

**Word causal effect is inconclusive.** BIA800 443.43/native420.90 µs, final
465.51/449.98 µs, contemporary predecessor 448.80/409.48 µs: raw and normalized
directions disagree with material reference movement. The first B screen was
444.40/422.52 µs. All observations remain; no word speedup, equivalence or confirmed
regression is claimed. Standalone final evidence remains a gap.

Unselected A used a shared value descriptor: byte415.47 µs, word488.27/native412.19
µs in the first observation. Scoped actual-module JIT inspection showed additional
row-metadata bounds/null checks despite smaller code; these are code facts, not
phase-cost proof. B retains local arrays and shares only the fill. No independent
A confirmation or broad/package round; its sources/modules/adverse actuals survive.

Managed timing includes admission, preparation, complete filtering/repair,
independent output acquisition and disposal. FFmpeg retains the public triangle
graph, source wrappers and geometry contexts; conversion, filtering, normalization
and output acquisition/release remain timed. These are not fresh native graphs.

Byte admitted scratch falls 29,888→12,608 B; word remains 12,608 B (8,512 B
geometry plus 4 KiB stack allowance). Explicit stack arrays total 3,560 B.
These source-based concurrent charges are distinct from measured total GC;
physical/native/JIT peaks are unmeasured. No operation scratch survives. Result
payloads are 132,096/264,192 B down and 2,132,052/4,264,104 B up, independently
owned until disposal. Existing two-slot/4 MiB default pixel pooling and governed
bypass are unchanged. Up16 exceeds that pool's cap; this is an attribution lead,
not authorization to fit the cap to a benchmark or infer CPU savings.

New public tests cover both depths, aggregate borrowed-root admission, retained
results, cancellation/refusal cleanup and subsequent recovery; independent exact
reference checks cover existing odd/edge/tiny-alpha cached-strip cases. Admission
precedes allocation and terminal refusal never retries. Existing tight/portable
fallbacks remain. Source restoration checks preserve all non-bilinear paths.

Windows/Linux each pass **6,251 full/live checks**: 2,939 internal, 2,364 public,
948 live. Resize 363 internal/779 public pass normal/scalar/AVX512off/AVX2off,
including PackedColumnStorageTests, EnlargingStripBoundaryTests and
EnlargementVerticalStorageTests. Unchanged codec/text ISA evidence is reused.
Two main/embedded builds per OS cover A+B; one successful public test build is
reused for B. One failed Windows public compile used internal access and was
fixed through public rows, without rebuilding unchanged main/internal inputs.
Prepared selectors are reused even for the first screens; no worker/harness/
selector/fixture rebuild. A metadata preflight was corrected before timing.
Affected performance reconciliation precedes broad and one actual-package round.
The SAME Release package and all three usage examples pass both OSes.

Private 0.2.1 package `debdc5ca4cae978c404f846ebd92422e22f7553f0696080fe3c059b6107837c3` (448,450 B), DLL
`5f5696ad33b13cc7b419aad6179c805007e59ceee9c5fd4bfc96ec5b95af4994`; Linux qualified DLL
`8bc02909db780453c2bf6e8900137109ba84a1a6028ca179c6b9fd1153f8e032`. Published releases remain immutable.
Publication postponed; this incremental stage is not a whole-release refresh.

[Evidence](bilinear-shared-preparation-evidence.json) pins source proof, current
costs, all confirmations, capacity model and qualification. Mirrored/member-verified
raw `bilinear-shared-preparation.tar.xz`: 6,243,272 B / 292 members,
SHA256 `36f5882c8d2458e20bd967fc1c33ea396cd0f27084d1991dfb1b888db794c172`. It preserves both designs and JIT inspection,
controllers/modules, baseline and package proof; pinned dependencies retain
fixtures/reference tools. [Earlier current-cost attribution](BILINEAR-DOWNSIZE-CURRENT-COSTS.md)
and [original packed enlargement](BILINEAR-PACKED-ROW-STORAGE.md) remain history.
All 289 required paths remain. [Current Up16 attribution](BILINEAR-UP16-CURRENT-ATTRIBUTION.md)
selects direct certified-mask publication as a candidate, not a proven gain.
