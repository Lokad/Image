# Current ordinary JPEG decode confirmations

Historical timing note: [bounded coefficient reuse](WARM-JPEG-COEFFICIENT-POOL.md)
replaces all16 selected JPEGdecode comparisons with current qualified short gaps.
Other family proofs retain their applicable scope; the <=1x objective stays open.

8 of the eight named ordinary baseline decode cases independently confirm
within the 2x warm single-CPU target on the qualified production DLL. This
resolves older evidence invalidated by the AC loop/lookahead changes `bc73f41`
and `a65b7f6`. No production optimization is needed. Stop the confirmed pairs;
full required warm coverage remains open.

| Existing case | Encoded input bytes | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| OwnedDecodeGray | 147,825 | 937.68 | 681.99 | 1.375 / 1.391 | 1,221,556 | 12 / 12 |
| NativeDecode420Restart | 58,012 | 1,640.44 | 1,013.53 | 1.619 / 1.626 | 1,623,906 | 12 / 12 |
| NativeDecode8 | 133,041 | 2,648.65 | 1,718.17 | 1.542 / 1.549 | 2,664,373 | 12 / 11 |
| Decode8 | 324,002 | 2,622.85 | 1,896.19 | 1.383 / 1.390 | 2,596,169 | 12 / 12 |
| DirectRgb420 | 52,109 | 1,131.19 | 742.22 | 1.524 / 1.532 | 1,619,540 | 12 / 11 |
| NativeDecode420 | 52,093 | 1,455.95 | 1,009.28 | 1.443 / 1.449 | 1,619,530 | 12 / 12 |
| DirectRgb422 | 71,184 | 1,495.61 | 1,092.79 | 1.369 / 1.375 | 1,978,658 | 12 / 12 |
| NativeDecode422 | 71,168 | 1,730.66 | 1,347.06 | 1.285 / 1.291 | 1,978,655 | 12 / 12 |

Every operation decodes the same resident original 513x259 encoded JPEG into an
independently owned RGBA8 buffer. Managed public Decode, admission, all parsing/
entropy/transforms, output materialization and disposal are timed. FFmpeg's
software MJPEG decoder includes per-operation state reset, required chroma
expansion/crop and full-range conversion, owned output acquisition/release.
Grayscale includes owned GRAY8-to-RGBA expansion. Contexts/packet/frame wrappers
are warm; every call decodes/converts all pixels. Native padded rows are exposed
without an extra packing copy. Required direct-RGB interpretation/phase
compatibility, when selected, is included inside native timing and explicitly
recorded in its receipt. No output cache. Startup, I/O, IPC, fixture generation,
hashing and validation are excluded.

All frozen input and per-decoder output hashes remain unchanged. Managed pixels
stay exact; native RGB maximum 4 / RMSE 1.5, exact opaque alpha, deterministic pixels,
normal/scalar CLI agreement, source preservation and simultaneously held
independent outputs pass before/after timing. Restart inputs retain DRI33 and
sixteen sequenced markers. Existing packets and original byte raster are reused;
no new raster, layout, sampling/quality grid or encoder calibration is introduced.

Each useful passing three-actual screen precedes an independent twelve-actual
confirmation on the same immutable production, benchmark and native worker.
Every actual remains retained; the conservative upper includes selected/all-
actual intervals against the faster native bracket's lower confidence bound.
Both versioned quiet guards pass every scored run. Sequential Release uses CPU0,
one codec/conversion thread, normal SIMD, disabled tiering and workstation
nonconcurrent GC. Managed allocations include harness costs; native allocation
totals are unmeasured.

Production, harness and native rebuild counts are zero. Only cached selection
drivers change; every child including WarmDry has the verified assemblies.
Production/package inputs remain unchanged from the existing
[qualification](WARM-JPEG-AC-PLANNING.md): 3,483 Release tests per OS including
live FFmpeg comparisons and the same actual package's consumer/all three
examples on Windows/Linux. No profile, full-suite/package repeat or whole-
library method audit is introduced. Older evidence stays historical at its
recorded source. The [numeric evidence](warm-current-jpeg-decode-evidence.json)
retains all samples, guards, exact identities, allocations and qualification
reuse. Each row certifies only its named fixed input/public boundary.

All eight affected ordinary cases are stopped. Continue the finite checklist.
