# Warm lossless WebP encoding from RGBA16

RGBA16 lossless WebP encoding independently confirms **0.0654× libwebp
exact=1 (FFmpeg encoder backend)**, conservative upper **0.0664×**, averaging
7.928 ms with **2,100,119 managed bytes allocated per operation**. All 12 actual
results are retained and both quietness guards pass. Production and package
are unchanged. Stop tuning this fixed pair; uniform warm parity remains open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Three-actual screen | 7,954.02 | 0.0657 | 0.0785 | Pass |
| Independent twelve-actual confirmation | 7,928.11 | 0.0654 | 0.0664 | Pass |

Both subjects encode the existing original 513×259 RGBA16 raster. All four
channels normalize with integer `(sample+128)/257` inside every operation.
The original native adapter repacks to BGRA8, freshly imports pixels and uses
the approved public libwebp `exact=1` backend, lossless mode, effort 4 and
compression quality 75. Its normalized samples match the integer reference
before Ready and after measurement; a corrupted hidden-RGB reference fails.
The reference is never used as timed source pixels. The prior 8-bit native
bitstream remains unchanged; no original image or profile sweep is added.

Every normalized RGBA sample, alpha and hidden RGB matches through managed and
normal/scalar FFmpeg decoding. Geometry, syntax, source integrity, deterministic
bytes and independent held output owners pass before and after timing.
Lokad.Image emits **533,100 bytes**, versus the backend's **183,812 bytes**.
This is the approved exact-pixel timing comparison; compression efficiency
parity is not established.

Managed timing includes normalization, public EncodeWebp, admission, fresh
MemoryStream growth, encoding and disposal. Native normalization, repacking,
fresh pixel import/allocation, encoding and independent packet acquisition/
release repeat every operation; wrappers and BGRA scratch storage are reused.
Startup, I/O, IPC, preparation, hashes and checks are excluded. Native allocations
are unmeasured. Sequential Release runs use one pinned CPU, normal SIMD, tiering
off and workstation nonconcurrent GC. The native thread level is zero; the
three-actual mean was parsed before the single independent confirmation.

Only the benchmark harness and original native adapter change. Forced fresh
builds and both actual generated children match all 599 production method bodies
and flags of the same verified actual Release package. Prior 2,715 Release/
differential tests per OS, 60 focused normal/scalar row checks per OS and all
three same-package examples on Windows/Linux are reused for unchanged production.

The [numeric evidence](warm-vp8lencode16-evidence.json) retains every actual
sample, allocations, controls and exact source/child/native identities. All four
actual child files, source, receipts and package were frozen before subsequent
preparation or mutation. The [benchmark summary](../docs/benchmark.md) reports
this fixed pair at its measured source; JPEG420 gaps and required material
coverage remain open.
