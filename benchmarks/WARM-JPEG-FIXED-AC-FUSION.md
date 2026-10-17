# Fixed JPEG AC emission

Updated 2026-10-17. Fusing fixed AC traversal and writing reduces the two byte
forward-only encode times by about 19–20%; the other three controls improve
about 5–8% against their latest selected results. **All five remain parity gaps.**
These are qualified three-actual quiet screens, not independent ≤1x confirmations.

| Existing selector | Prior / current Lokad µs | FFmpeg µs | Ratio / upper | Lokad B/op | Packet B, Lokad / FFmpeg |
| --- | ---: | ---: | ---: | ---: | ---: |
| Encode420Stream8 | 3,382.57 / 2,733.19 | 1,765.36 | 1.548 / 1.705 | 263,843 | 81,255 / 52,093 |
| Encode420Stream16 | 3,057.46 / 2,808.87 | 1,839.95 | 1.527 / 1.641 | 263,820 | 81,275 / 52,117 |
| Encode444Cancelable8 | 5,635.05 / 5,356.88 | 3,779.19 | 1.417 / 1.531 | 526,075 | 170,978 / 133,041 |
| Encode444Stream8 | 6,613.11 / 5,276.99 | 3,773.28 | 1.399 / 1.521 | 526,107 | 170,978 / 133,041 |
| Encode420Cancelable16 | 3,031.41 / 2,840.65 | 1,837.08 | 1.546 / 1.661 | 263,788 | 81,275 / 52,117 |

All actuals and both versioned quiet guards are retained. Native allocation is
unmeasured. Managed B/op includes harness amortization; no production heap storage
or fields are added. Packet sizes and hashes are unchanged; compression-efficiency
parity is not claimed. The two previous short regressions and their independent
witnesses are historical after this production change.

The same two original 513×259 RGBA8/16 rasters, quality69/native qscale5,
4:2:0/4:4:4, background0x204060 and ≤0.25 dB source-quality band are retained.
Complete operations include flattening/normalization, color conversion, encode,
fresh MemoryStream growth, forward-only wrapping where selected, independent
output and disposal. Live tokens remain untriggered during timing. Exact frozen
packets/decoded samples, native quality, alpha and independent-owner checks pass.

One current six-second profile put 51.5% in fixed AC traversal and bit writing.
One production candidate keeps bit state local across that traversal and uses
the sealed bounded-stream entry when available. Each field retains its original
work/cancellation check; every byte callback, stuffing byte, output quota and
failure state is preserved. Validated categories prove the amplitude range;
general writer checks and the existing scalar fallback remain intact.

The primary three-case batch is followed by the other two existing selectors
on the identical core/harness/native/fixtures. The primary archived driver is
reused; one shared selector parses Case before Setup for the remaining jobs.
No quality sweep, new image/workload, native rebuild or repeated qualification.
Other 202 comparisons, including all 51 passing parity proofs, retain their
executed source paths; adaptive JPEG coding returns before the changed fallback.

All **3,596 Release tests per OS** pass (1,308 internal, 1,390 public, 898 live
FFmpeg), plus 456 scalar JPEG tests and 47 scalar encoding/admission controls.
Two new raw-stream theories cover all 1–16-bit code widths, pending positions,
stuffing, EOB tails and both token modes; existing oracle tests preserve exact
callback, quota, work and partial writer state. The same actual Release package
passes its consumer and all three examples on Windows/Linux; symbols and SPEC
match the qualified build. Full qualification leaves the screened core byte-identical
and restores the frozen measured harness after its metadata build.

[Numeric evidence](warm-jpeg-fixed-ac-fusion-evidence.json) includes statistics,
all packet sizes/quality records, source reconciliation and archive hashes.
The raw archive is `parity-p11-qualified-evidence.tar.gz`, SHA256
`c432a70916ad465299fd4ee2298916ee86a7620a01eeb50c5e284d8b1a682094`.
