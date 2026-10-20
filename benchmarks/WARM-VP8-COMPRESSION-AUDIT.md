# Lossy WebP compression audit

This bounded round accepts **no production change**. Neither tested prediction-rate
strategy reaches the required photograph/diagram ceilings of **17,688/7,100 bytes**.
The qualified C37 source, build and package are restored and reused.

| Untimed candidate | Photograph bytes | Diagram bytes | Source PSNR gap, photograph / diagram |
| --- | ---: | ---: | ---: |
| Qualified production, effort4 | 21,432 | 8,288 | 0.019385 / 0.218255 dB |
| Add coefficient-skip syntax | 21,434 | 8,274 | Not rechecked after failed size screen |
| Reconstruct/rate-score best two modes, effort4 | 20,556 | 8,182 | 0.054068 / 0.244163 dB |
| Reconstruct/rate-score all six modes, effort4 | 19,912 | 8,054 | 0.096337 / 0.182589 dB |
| Existing full mode search, effort6 diagnostic | 21,126 | 8,002 | 0.011402 / 0.153289 dB |

All reconstruction experiments pass the unchanged0.25dB source-quality gate,
managed/native decoder bounds, exact alpha, borrowed-source and independent-output
checks. None passes the size gates. Reference input, libwebp quality75/effort4 and
packets remain frozen. Managed quality stays78 for the photograph and75 for the
diagram. The effort6 row isolates missing direction-mode coverage on the accepted
build; it is not a changed baseline or an additional benchmark case. No quality,
effort or image grid, timing, allocation, confirmation or profiling claim follows.

An independent syntax reader/writer reproduces both original header and token
partitions byte for byte before adding skip syntax. Only8/561 photograph and
273/561 diagram macroblocks have all-zero coefficients. Their removed token cost
is largely offset by skip signaling: the final savings are−2/14bytes. That path
is immaterial. Current packets use a bounded64-macroblock coefficient model;
the earlier full-frame alternative's rejected performance evidence is reused.

The stronger rate decision evaluates actual reconstruction error and context-
dependent mode/coefficient bit cost, using an independently derived quantization
noise model. All-six scoring saves7.1% of photograph bytes and2.8% of diagram
bytes. It remains a4×4-only strategy and misses both ceilings. Two immutable Linux
candidate builds are reused for their screens; no Windows, harness, selector or
native rebuild, full suite or package qualification follows the failed gates.
A diagnostic helper reference error was corrected without rebuilding production.

The packet audit locates77.6%/81.3% of the excess bytes in the coefficient partition.
Both native packets have one token partition, so this area contains no partition-
size table. Container/padding is20bytes and the uncompressed keyframe header is
10bytes on both sides. Container trimming cannot close these gaps.

| Frozen native structure | Photograph | Diagram |
| --- | ---: | ---: |
| Intra16 macroblocks / total | 43 / 561 | 353 / 561 |
| Intra4 macroblocks / total | 518 / 561 | 208 / 561 |
| Frame filter level | 13 | 11 |
| Segment quantizer indices | 36,32,25,19 | 36,33,29,22 |

Lokad.Image currently emits only B_PRED4×4 luma, one global quantizer and no loop
filter. The next distinct primitive is16×16 luma prediction with grouped Y2 DC
coding, which the decoder already supports. Its prevalence makes it a concrete
diagram opportunity. This does not establish that intra16 alone closes the
photograph gap; segmentation, filtering and coefficient decisions remain separate
structural differences. Frozen numerical gates and complete-operation performance,
resource, cancellation and ownership requirements remain cumulative.

[Numeric evidence](vp8-compression-audit-evidence.json) retains the exact packet
identities, structures and candidate quality results. Raw source, arithmetic replay,
candidate binaries and outputs are mirrored as `c38-vp8-compression-audit.tar.gz`,
SHA256 `0cd946c50bf213d13988dd258e3604c9902e71cd48b7b78ee56754d4cea7fc17`.
No benchmark selection, qualified production result or published release changes.
