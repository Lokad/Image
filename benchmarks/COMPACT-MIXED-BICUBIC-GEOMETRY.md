# Compact mixed-axis bicubic geometry

Exact preparation now uses **36 rather than84B per column** and shares its extent
with byte/word result-bank placement. The existing default RGBA8 mixed control
saves **35.35µs /4.01%raw /4.07%reference-normalized**. Bounded byte saves
**13.27µs /1.54%raw /4.10%normalized**. Raw/filtered/all-actual and normalized
intervals separate for both; native points move+0.063%/+2.673%. Both byte controls
pass/stop. Word results remain uncertain; no uniform speedup or full parity claim.

| Existing513×259→1027×129 complete Resize | Lokadµs | zimgµs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Mixed8 | 846.46 | 867.46 | 0.976 / 0.989 | 114 | pass |
| TightMixed8 | 850.67 | 893.27 | 0.952 / 0.978 | 114 | pass |
| Mixed16 | 921.81 | 918.99 | 1.003 / 1.012 | 114 | uncertain |
| TightMixed16 | 923.05 | 914.24 | 1.010 / 1.016 | 114 | uncertain |

Default uses the normal local limit; Tight means input+output+4KiB. Complete public
Resize includes geometry, workspace, filtering/repair, independent output and
disposal. Reference is the explicitly labeled FFmpeg zscale **zimg backend**,
with graph/input/scratch retained. Its packed conversion/halo/filter/quantization,
owned output and release remain timed. This is the existing repeated-reference
boundary, not equivalent fresh-graph or FFmpeg CLI performance. Original samples,
dimensions, settings and native bounds remain frozen; no workload grid is added.

The old seven doubles/seven ints duplicate centers, normalizers and clamped source
indices. Four original weights plus a packed first/count now suffice. A fifth tap
is compacted only when its original weight is exactly+0. Count/range/endpoint guards
decline unproved preparation before publication. Read reconstructs the original
center expression, every ordered normalization addition including the fifth zero,
and original clamped indices. No tap, arithmetic order, certificate or repair policy
changes. Actual-kernel tests compare all double bits, counts/indices and guards;
odd/edge/zero-alpha frames retain rational/qualified-original oracles.

For the frozen width, geometry shrinks86,268→36,972B. Both temporary views borrow
already-admitted unpublished output; the old body and all cancellation/ownership
code are verbatim after reverting only the bank-extent expression. Safe large-bank
prefix grows101→113byte rows and115→121word rows. The logical model removes two
byte suffix geometry passes and one bounded-word pass; it is not a phase-time or
CPU-gain forecast. Existing generalization/poison/retirement cases cover smaller
odd images and both depths. Portable fallback and exact tiny-alpha replay remain.

Managed allocation stays**114B/op**, separately from active storage and retention.
Result payload stays529,932/1,059,864B, scratch4/8KiB, two-word-result governed peak
2,127,920B. No heap capacity, owner, pool/session or retained scratch is added.
Default pixel-pool retention and governed bypass are unchanged. Physical/native/
JIT-stack peaks are unmeasured. Shared refusal remains terminal, with no retry,
wait or lower-memory fallback; cancellation/refusal cleanup and survivors pass.

## Word evidence and unselected revision

Current bounded word923.05/native914.24µs is uncertain. Older893.16/native851.29µs
cannot supply a raw gain when native moves7.4%. One contemporary unchanged
predecessor gives939.43/native919.87µs: raw16.37µs intervals separate, normalized
1.14%point intervals overlap. Default predecessor842.82/native847.54µs upper1.006
is also uncertain, versus current921.81/native918.99µs upper1.012. Raw time rises
**78.99µs/9.37%** while reference moves**8.43%**; normalized intervals overlap.
The old default pass is historical, not a current substitute. These observations
resolve the clock contradiction without proving a causal word gain or normalized
regression. All adverse observations remain, and both current word costs stay open.

CMG814 moved center reconstruction to exact-repair calls. Its short bounded-word
screen is961.82/native914.71µs, +4.20%raw/+4.15%normalized points against the selected
implementation; wide three-actual intervals establish no independent regression.
No broad/package checks or favorable repetition follow it. Original compact source,
main/embedded modules and bindings are restored byte-for-byte. The existing MIA795
profile is reused as unchanged-source insight; no redundant new capture is taken.

## Qualification and reproduction

Only after all four affected costs reconcile: **6,283 full/live checks per OS**,
374internal/789public resize checks in normal/scalar/AVX2off/AVX512off modes,
including all three named storage suites; **SAME actual Release-package consumer
and all three examples on Windows/Linux**. Two core/embedded versions per OS,
zero public/benchmark/selector/native/fixture builds and zero production compile
failures. One final full/ISA/package milestone; unchanged codec/text proof is reused.

Private0.2.1 package`7dc196cc9f0cd9217f7be402f54b88ab9a4408abbec0dc9f5f974bd60e5e11f4` contains timed DLL
`a8e3b116d783bef5186e7472fe21ffe3cea2957af81ad2aee4431b0bee1e09df`. Published releases remain immutable; publication is postponed.
All289material paths remain required. Current byte passes stop; word uncertainties
do not block independent codec/resize work or justify retuning an assumed gap.

[Evidence](compact-mixed-bicubic-geometry-evidence.json) pins exact sources,
scope/invalidations, all four costs, word counterchecks, restoration and owners.
Mirrored/member-verified raw`compact-mixed-bicubic-geometry.tar.xz`:3,969,688B/270members,
SHA256`e69eeb17d3d8fe9a3a8e315d5c44048206a85eafd9fc6e458837064871f4f7e3`. It retains all attempted sources/modules and every actual,
guard and failed lead; dependencies retain frozen runtime/reference/fixture inputs
and original qualification. Older[geometry/bands](BICUBIC-RESULT-BACKED-BANDS.md)
and[quartet execution](BICUBIC-SHARED-QUARTET-EXECUTION.md) remain linked history.
