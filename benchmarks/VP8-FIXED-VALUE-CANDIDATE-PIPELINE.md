# VP8 candidate values through reconstruction and scoring

The ordered Luma4 search snapshots source pixels once per block and holds
coefficient, prediction and reconstructed values through the actual inverse,
SSD and texture cores. Only winning pixels are published. Rate-rejected modes
avoid prediction copies; known-zero residuals reuse their prediction. Real
strided/span callers share the same arithmetic. Packed coefficients within2047,
full signed16 first-pass wrap, rounding/clamp, mode/tie/uniform/trellis ordering,
pruning, cancellation and exact complete packets remain unchanged.

Independent12alternating pairs×16complete photograph encodes save
**257.06µs / 2.26%**,
11,381.22→11,124.16µs;
ratio0.977417, interval[0.974451,0.980387].
809,072B/op is unchanged. The separate three-pair photograph screen was uncertain;
diagram/trellis screen leads remain scoped screens, not independent gain claims.
Identical resident-source setup/JIT/checks are excluded; complete public encode,
admission, preparation/normalization, fresh output growth, independent ownership
and disposal stay timed. All pairs remain. This establishes the managed-build
increment, separately from standalone native-reference costs below.

The first implementation is rejected: its independent photo ratio1.015075,
interval[1.011585,1.018571] reverses its favorable
short. The final revision moves full-range inverse arithmetic behind a non-inlined
value return; hot Luma4 is8437→8412B, shared inverse entry1365→646B, versus8923/
951B in the first revision. No unchanged-candidate favorable repeat or family ban.

| Current frozen complete513×259 encode | Managed µs | FFmpeg/libwebp µs | Ratio / upper | B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque RGBA8, effort4 | 15,467.57 | 15,491.08 | 0.998 / 1.001 | 1,201,552 | 72,380 |
| Opaque RGBA16, normalization | 15,881.94 | 15,508.86 | 1.024 / 1.029 | 1,201,552 | 72,380 |
| Opaque photograph, effort4 | 11,309.77 | 9,693.26 | 1.167 / 1.180 | 809,225 | 17,688 |
| Opaque text/diagram, effort4 | 6,536.34 | 5,821.82 | 1.123 / 1.129 | 741,137 | 7,100 |
| Diagram, effort2/distortion | 2,987.26 | 2,503.72 | 1.193 / 1.197 | 741,386 | 7,330 |
| Diagram, effort6/trellis | 11,352.17 | 9,784.91 | 1.160 / 1.171 | 754,497 | 6,818 |
| RGBA8 + lossless alpha, effort4 | 54,570.25 | 53,541.53 | 1.019 / 1.038 | 7,034,392 | 98,590 |
| RGBA16 + alpha, normalization | 55,084.71 | 53,449.62 | 1.031 / 1.039 | 7,033,984 | 98,590 |

Current status: 7 gaps, 1 uncertain,
0 passes. Every row retains12actuals, filtered/all confidence,
both native brackets and passing idle/process gates. Frozen inputs/settings,
exact packets/alpha/pixels and source/output owners pass. Native codec/scratch/
wrappers are retained as declared; necessary normalization/import, pixel work
and independent output ownership stay timed. These are repeated-use boundaries,
not FFmpeg CLI startup or general one-shot setup equivalence. Historical/current
clock differences are not causal incremental gains. The opaque16 discrepancy
prompted one direct12pair×16reconciliation:225.19µs/1.44% improvement,
ratio.985642, interval[.982009,.989282],1201552B/op unchanged and exact72380B
packet. Its current standalone gap remains; no favorable standalone repeat.
Other117displayed comparisons reuse
unchanged relevant paths; lossless passing cases are not retuned.

Fixed values add no heap capacity, owner, pool, session or public API. Existing
16KiB macroblock admission remains; operation scratch retention is zero. Report
total standalone GC/batch allocations separately from unchanged paired809072B;
complete physical/admitted peaks and native allocation are not newly measured.
No CPU gain is inferred from allocation. Original inputs/caller buffers remain borrowed.

Final immutable builds pass5,718full/live checks each Windows/Linux
(2479internal/2291public/948differential),108internal/137public affected checks in
normal/scalar/AVX2-disabled/AVX512-disabled modes. Existing independent scalar and
native frozen-digest datasets now exercise the real fixed-value inverse/SSD/
texture entries, including full-range wrap/clamp and borrowed tails/aliasing.
Complete packet, shared refusal/cancel cleanup, async actual-user lifetime and
surviving-output tests pass. The SAME actual private0.2.1 Releasepackage and all3
usage examples pass both OSes. A stale resource-admission document on the VM was
caught before the Linux consumer; synchronize it and verify the same package,
without rebuilding or repeating performance. Published packages are unchanged.

The current [attribution](vp8-fixed-value-candidate-pipeline-evidence.json) retains
the2second profile and exact complete-encode applicability counts as leads,
not phase timings or predicted savings. The [preceding quantizer stage](VP8-QUANTIZER-OPERAND-PREPARATION.md)
is history for these eight costs. Full282-path parity remains open; publication
and a complete125-row actual-package release refresh remain separate.

[Evidence](vp8-fixed-value-candidate-pipeline-evidence.json) pins all sources,
modules, pairs/actuals/guards, both revisions and SAMEpackage proofs. Raw
`vp8-fixed-value-candidate-pipeline-qualified.tar.xz`: 60,684,580B/1441members,
SHA256`200b93fa32bddea6d3a49c0bf0f5fb233fd2f40343138e833ea44fdd955fe8bb`, mirrored/member-verified with qualified-base/attribution dependencies.
