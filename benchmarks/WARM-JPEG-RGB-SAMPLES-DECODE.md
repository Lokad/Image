# JPEG decoding into caller RGB samples

`JpegSamplesDecode/Rgb420Resident` is the largest demonstrated warm parity gap.
The qualified three-actual screen reuses the original52,093-byte native4:2:0
JPEG at513×259. Production, input, quality gates and settings are unchanged.

| Complete public operation | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op | Output bytes, both |
| --- | ---: | ---: | ---: | ---: |
| DecodeJpeg to fresh caller RGB24 | 3,906.80 / 1,159.57 | 3.369 / 3.516 | 624,454 | 398,601 |

Both sides acquire fresh zeroed packed caller output, decode every component,
normalize color and include independent output lifetime. Native software MJPEG
expands the ceil-sized chroma planes by2×2 and crops to the original grid, then
packs full-range BT.601 directly into RGB24. Its decoder/chroma buffers and caller
output are acquired/released inside every operation. The caller allocation adds
FFmpeg's64-byte tail padding after the packed payload. There is no RGBA frame,
implicit ICC conversion, EXIF reorientation or omitted packing pass. Empty native
frame wrappers, codec/conversion contexts and the resident input packet are reused;
decoded content is never cached. Startup, I/O, IPC and checks stay outside timing.

Managed RGB exactly reproduces the qualified RGBA decoder's RGB projection.
Normal native pixels exactly match public FFmpeg CLI RGB24 delivery. Normal/scalar
native comparisons pass the existing maximum4/RMSE1.5 gate: normal maximum3,
RMSE0.57937. Caller padding/suffix and encoded input stay unchanged; two surviving
outputs remain independent after another output is mutated/released. Three actuals,
native pre/post50ms brackets and both idle-v2 guards pass, with one CPU and
nonconcurrent workstation GC. The above-one mean does not justify a long repeat.

One narrow harness and one separate native adapter are compiled. The existing
selector, original fixture and pinned FFmpeg libraries are reused. The grayscale
helper/worker and every production path remain unchanged; their evidence stays
valid. An initial native script line-ending correction required no harness rebuild
or timed rerun. The current production retains4,169tests/OS,109affected hardware-off
checks/OS and the same actual private Windows package consumer/all three examples
on both OS from [its accepted qualification](WARM-JPEG-CALLER-MCU.md).

This closes one missing measurement without adding workloads. Current218numeric
rows contain69independent passes,148mean gaps and one confidence-open case;
25named boundaries remain unmeasured. Next optimize the demonstrated RGB sample
writing gap before smaller gains. The full warm parity objective remains open.
[Numeric evidence](warm-jpeg-rgb-samples-decode-evidence.json) retains all samples,
allocations, guards, receipts and exact identities. Raw archive on both hosts:
`h10f-measurements.tar.gz`
`8602d62714eb3ef0e3de9b367928bc088fcdc9073e904439c543b8de93f8ba31`.
