# Direct caller RGB420 JPEG sample writing

The complete caller RGB decode now independently passes warm single-CPU parity:
**0.966x, conservative upper0.971x**, down71.3% in
elapsed time from3,906.80µs. The same52,093-byte513×259 JPEG, quality gates,
native worker and complete fresh-output boundary are retained.

| Complete public operation | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op | Output bytes, both | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Caller RGB24 | 1,121.95 / 1,161.53 | 0.966 / 0.971 | 624,508 | 398,601 | Independent12actuals |
| Caller GRAY8 dispatch control | 857.87 / 646.88 | 1.326 / 1.445 | 273,422 | 132,867 | Qualified3actual gap |

The single production primitive specializes normal-orientation YCbCr420 sample
writing to packed caller RGB. It shares centered quarter-weight row geometry,
converts sixteen pixels with the existing guarded SIMD rounding certificate,
and uses original binary64 eight-pixel arithmetic near half boundaries. Byte
shuffles store exactly twelve bytes per four pixels, with no alpha raster,
RGBA row/frame, extra heap workspace, input copy or padding overwrite. A24-byte
stack scratch supports double-precision fallback. Scalar clamped edges and tails,
per-row work/cancellation charges and all other layouts/orientations remain.
Unsupported hardware uses the previous sample writer. Exact pixel precision,
source ownership and independently surviving outputs are unchanged.

The public operation includes fresh zeroed destination allocation, complete
decode/normalization and ownership. Native software MJPEG includes its own fresh
packed output, centered chroma expansion/crop, BT.601 packing and decoder/chroma/
caller buffer release. Pixels exactly reproduce the qualified managed projection.
Native normal CLI delivery is exact; normal/scalar maximum4/RMSE1.5 remains,
with normal RGB maximum3/RMSE0.57937 and gray maximum1/RMSE0.11915. Padded rows,
suffixes, input stability, live tokens and independent surviving buffers pass.
Supplied EXIF reorientation stays opt-in. No quality or baseline adjustment.

Three short RGB actuals justify an independent12actual confirmation. The gray
rejection-dispatch control adds three actuals; its above-one mean needs no long
repeat. All18actuals/outliers, native pre/post50ms brackets and idle-v2 guards
are retained. One frozen production candidate per OS is reused; existing harness,
native worker, fixture and short selector are unchanged. The retained decoder
selector only supports3actuals, so its unchanged workload body is compiled once
with12actuals/six warmups. No workload/image/settings grid is added.

Accepted qualification passes4,176tests/OS (1,417internal/1,831public/928live
FFmpeg),73affected hardware-disabled checks/OS, and the SAME actual private
Windows package consumer/all three examples on both OS. Package SHA
`4b570c42b92a19d02f58401df90f0cd125978358991ff68e6c1375b5bc5ecf6e`.
The published0.1.0 artifacts and tag are unchanged.

Exactly two timing rows are replaced: the RGB writer and gray's new rejection
dispatch. Ordinary RGBA decode, parsing/IDCT, CMYK writing, encoding, streams,
editing, drawing and text retain their executed paths and evidence. Existing69
passing proofs stay valid; the new RGB pass stops until affected. Current scope:
218numeric rows,70passes/147mean gaps/one confidence-open;25new boundaries remain
unmeasured. The full parity objective remains open. Next largest gap: RGBA8
128×64 bicubic under the existing input+output+4KiB allowance,1.687x.

[Numeric evidence](warm-jpeg-caller-rgb420-evidence.json) retains exact identities,
source snapshots, timings, allocations, quality/ownership receipts and both-OS
qualification. Raw archives are retained on both hosts:

- `h10g-measurements.tar.gz` SHA`473c75355d4298f2bc46703bb4082b53af54b55db39bd2816715fcf4925b7c43`.

- `h10g-qualified-windows.tar.gz` SHA`56963f417e4864ae4afbf3472a7d65e578e43b4791afc3d18d1d7304f6fb8678`.

- `h10g-qualified-linux.tar.gz` SHA`ebdc6c4415091d6af0d2724a4a30607b1a10fad9b48883a14e09b42c66c5732b`.
