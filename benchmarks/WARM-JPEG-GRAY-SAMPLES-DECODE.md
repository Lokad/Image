# JPEG decoding into caller grayscale samples

Historical before-writer measurement. Current timing/verification is in
[the accepted RGB420 writing round](WARM-JPEG-CALLER-RGB420.md).


The named `JpegSamplesDecode/GrayscaleResident` boundary is a demonstrated warm
parity gap. A qualified three-actual screen uses the existing147,825-byte gray
JPEG at513×259. There is no production change or new image/settings grid.

| Complete public operation | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op | Output bytes, both |
| --- | ---: | ---: | ---: | ---: |
| DecodeJpeg to fresh caller GRAY8 | 856.64 / 646.91 | 1.324 / 1.449 | 273,422 | 132,867 |

Both operations include destination acquisition, complete decode/normalization
and output lifetime. Managed creates a zeroed packed caller byte array and uses
public `DecodeJpeg`; native software MJPEG creates a zeroed independently owned
packed GRAY8 buffer, decodes, copies every row from its decoder planes and releases
both owners. No RGBA intermediate, implicit ICC conversion or EXIF reorientation.
Native codec, padded resident input packet and empty frame wrapper are reused;
decoded pixels are never cached. Startup, I/O, IPC, hashing and validation are
outside timing. The existing RGBA decode case remains a different operation.

Managed samples exactly reproduce the previously qualified grayscale projection.
Native delivery exactly matches normal-dispatch public FFmpeg CLI output. The
unchanged JPEG maximum4/RMSE1.5 agreement gate passes: normal native maximum1,
RMSE0.11915; scalar CLI is also checked. Managed packed/padded destinations,
untouched padding/suffix, input stability and two surviving independent outputs
pass. Native checks outputs after decoder-plane release and after mutating/freeing
another output. All three actuals, native pre/post50ms brackets and both idle-v2
guards are retained. One CPU and nonconcurrent workstation GC remain.

One narrow tooling harness and one separately named native adapter are built.
The existing compiled selector, production, original packet and FFmpeg libraries
are reused. The old native worker is unchanged. No independent long repeat is
justified by this above-one mean. The unchanged production retains4,169tests/OS,
109affected hardware-off checks/OS and the same actual private Windows package
consumer/all three examples on both OS from
[its accepted qualification](WARM-JPEG-CALLER-MCU.md). No full suite or package
qualification is repeated for tooling changes.

This fills one missing checklist measurement:217current numeric rows now contain
69independent passes,147mean gaps and one confidence-open case;26named boundaries
remain unmeasured. No older evidence is invalidated. The full parity goal is open.
[Numeric evidence](warm-jpeg-gray-samples-decode-evidence.json) retains samples,
allocations, guards, exact identities and all correctness receipts.
Raw archive retained on both hosts: `h10e-measurements.tar.gz`
`7b488bd62a557ea8c6428e5e1be92753cbd3912c10584f75008b79d9d641fc11`.
