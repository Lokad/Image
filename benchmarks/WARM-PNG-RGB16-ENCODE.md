# Warm PNG RGB16 encode

RGB16 PNG encoding independently confirms **0.865× FFmpeg**, with conservative
upper **0.871×** and **2,102,765 managed bytes allocated per operation**. The
independent repeat averages 13,121.10 µs; all 12 actual results are retained and
both quietness guards pass. Production is unchanged. Stop tuning this fixed pair;
other material cases and uniform warm parity remain open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Three-actual screen | 13,112.42 | 0.866 | 0.906 | Pass |
| Independent twelve-actual confirmation | 13,121.10 | 0.865 | 0.871 | Pass |

One existing 513×259 RGBA16 raster is projected to opaque RGB: every RGB word is
unchanged and alpha is 65535. This is the frozen RGB16 decode projection, with
no additional original image. Both outputs use RGB16, filter None, noninterlaced
PNG and compression level 6. IDAT payloads are 737,237 versus 737,246 bytes,
within the existing 5% size gate. Inflated scanlines and every decoded word are
exact, including normal/scalar FFmpeg CLI output. Source and independent held
output owners are checked before and after measurement.

Managed timing covers fresh MemoryStream creation/growth, public EncodePng and
disposal, without a redundant ToArray. Native timing includes one-thread
RGBA64LE-to-RGB48BE conversion inside every operation, encoding and independent
owned packet acquisition/release. The source, prepared frame, codec context and
packet wrappers are reused. Startup, I/O, IPC, hashes and validation are excluded.
Native allocation figures are unmeasured. Runs use one pinned CPU, normal SIMD,
tiering off and workstation nonconcurrent GC; managed iterations target 200 ms
and native brackets 50 ms.

The original preparation exposed a harness classification omission: the new RGB
encode operation was incorrectly checked as a raw pixel buffer. Adding it to the
encoded-packet branch preserves the existing counter, size, stride and CPU
checks. The failed preparation has no scored samples and is retained. After the
fix, the first screen stopped at CPU0 I/O wait 1.20%, above the unchanged 1% guard;
it also has no scored samples. A fresh guard passed using the same prepared child.
Its mean was parsed before preparing the independent confirmation. Neither
original failure is reclassified as passing.

The native adapter passes RGBA8/RGBA16 encode, RGB8/RGB16 encode and RGB8/RGB16
decode controls with exact samples and independent owners. Forced fresh builds
and both actual generated children match all 595 production method bodies and
flags of the same verified actual Release package. Production/package are
unchanged; prior 2,682 Release/differential tests per OS and all three same-package
examples on Windows and Linux are reused.

The [numeric evidence](warm-pngrgb16encode-evidence.json) retains every actual
managed/native sample, allocation evidence, original failures, exact controls,
source/child/native identities and immutable archive identities. All four actual
child files, source, receipts and actual package are frozen before further
preparation or mutation. The [benchmark summary](../docs/benchmark.md) reports this
fixed pair at its measured source; it does not certify every workload at current
HEAD. RGB8 encode's compression comparison and JPEG420's upper-bound gap remain
open.
