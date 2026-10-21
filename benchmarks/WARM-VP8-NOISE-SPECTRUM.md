# DC/AC-aware WebP quantization noise

The same five procedural controls produce smaller complete packets, with the
unchanged reconstruction-quality, decoder and exact-alpha gates passing. Six of
seven declared lossy controls pass size parity. The constrained packet remains
**2,390 B above** its fixed reference; this is an incremental gain, not codec parity.

| Existing control | Previous â†’ current / reference packet B | Source-PSNR gap dB | Managed / reference Âµs | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 71,154 â†’ 70,298 / 72,380 | 0.005052 | 74,428.73 / 15,537.01 | 1,006,520 |
| EncodeOpaque16 | 71,154 â†’ 70,298 / 72,380 | 0.005052 | 75,226.22 / 15,616.85 | 1,006,520 |
| Encode8 | 96,708 â†’ 96,016 / 98,590 | 0.021958 | 86,100.75 / 53,409.99 | 1,537,304 |
| Encode16 | 96,708 â†’ 96,016 / 98,590 | 0.021958 | 83,863.51 / 53,727.26 | 1,537,304 |
| EncodeBudget8 | 101,478 â†’ 100,980 / 98,590 | 0.040560 | 15,545.61 / 53,549.92 | 2,004,972 |
| PhotographLossy8 | 17,366 â†’ 17,366 / 17,688 | 0.140944 | 35,062.49 / 9,726.09 | 590,152 |
| DiagramLossy8 | 6,722 â†’ 6,722 / 7,100 | 0.117409 | 13,923.43 / 5,847.14 | 570,856 |

Procedural managed 100/native 75, primary photograph 77/diagram 75 versus native 75,
effort 4, inputs, references and the 0.25 dB tolerance stay fixed. Numerical quality
equality is not a matching criterion. Primary packets/pixels/quality remain exact.
Lossy procedural RGB may change; alpha stays exact, with ALPH 18,579 B unrestricted
and 19,246 B constrained. Claims cover these inputs/settings only.

Unequal DC/AC steps induce covariance within each 4×4 inverse-transform block.
The shared analytical model propagates that covariance through centered chroma,
combining clamped taps and grouping their transform blocks. The source allowance,
base quality map, quantizers and cluster policy are unchanged. Local neighboring
steps and independent coefficient noise remain heuristic assumptions; quality
gates remain mandatory. No quality/lambda/deadzone grid or arbitrary offset is used.

Only scalar weights change: no new buffer, frame, cache or copy. Source sampling
charges 32 additional units per sample, with updated admission. A monotone search
uses at most 7 bounded evaluations while retaining the 128-unit search charge.
The constrained operation consumes 31,632,851 of 32M units. Shared refusal, work
exhaustion and cancellation stay terminal; ownership and cleanup are preserved.

Both OSes pass **4,723 tests**, 247 internal + 12 public + 4 differential scalar checks,
and the same actual private 0.2.0 package/all three examples. Fourteen new cases
verify independent inverse-basis impulses and quantizer threshold/floor behavior.
All five procedural packets/managed pixels agree across OSes and normal/scalar.
One frozen producer/internal-test build per OS is reused. A lower-work test had
assumed 128 capacity and zero stream output; it now checks either legal prefix,
preserved caller content/source, typed failure, cleanup and subsequent success.
Partial stream output was already permitted. Default 128 refusal/cancellation
assertions remain. Passing internal tests were reused after public-test repairs.

Seven short three-actual jobs record costs, without independent timing repeats
or CPU-parity acceptance. One failed premeasurement I/O-wait guard and setup
failures are retained separately; every accepted job has passing idle-v2 brackets.
Native allocations are unmeasured. Other codec/edit/font/inspection and frozen
decoder evidence remains unchanged. [Structured evidence](vp8-noise-spectrum-evidence.json)
retains exact quality/work, producer/package identities and immutable raw records.
