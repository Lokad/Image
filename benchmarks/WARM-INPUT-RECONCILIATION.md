# Current input-operation reconciliation

All214 retained numeric comparisons now have current executed-path evidence.
The remaining22 prior-production rows are refreshed without changing production:
15JPEG pixel decodes, resident JPEG identification and six PNG/WebP stream paths.
The [preceding grayscale proof](WARM-JPEG-GRAY-CURRENT.md) remains valid and is
reused. JPEG metadata independently passes **0.488× / upper0.494×,88B/op**;
fresh-Stream RGBA16 PNG independently passes **0.988× / upper0.991×,8,648B/op**.
Other refreshed cases have clear short gaps,1.118–1.562×, without long repeats.
Current totals are **69 independent passes,144 mean gaps,1 confidence-open**;
29 named new boundaries remain unmeasured. Full uniform parity remains open.

| Complete operation | Managed / reference µs | Ratio / upper | Managed allocated B/op | Verification |
| --- | ---: | ---: | ---: | --- |
| JPEG baseline444, native packet | 2,300.388 / 1,719.444 | 1.338 / 1.418 | 966,317 | Qualified short gap |
| JPEG baseline420 | 1,202.774 / 1,008.956 | 1.192 / 1.315 | 757,345 | Qualified short gap |
| JPEG baseline422 | 1,510.490 / 1,351.288 | 1.118 / 1.263 | 829,148 | Qualified short gap |
| JPEG baseline444, owned packet | 2,522.162 / 1,902.720 | 1.326 / 1.431 | 948,023 | Qualified short gap |
| JPEG direct RGB444 | 2,193.437 / 1,452.835 | 1.510 / 1.656 | 966,317 | Qualified short gap |
| JPEG direct RGB422 | 1,472.207 / 1,094.022 | 1.346 / 1.509 | 829,148 | Qualified short gap |
| JPEG direct RGB420 | 1,073.365 / 743.411 | 1.444 / 1.518 | 757,346 | Qualified short gap |
| JPEG baseline444, live token | 2,392.144 / 1,720.109 | 1.391 / 1.482 | 966,317 | Qualified short gap |
| JPEG baseline420, restarts | 1,378.068 / 1,014.377 | 1.359 / 1.446 | 761,720 | Qualified short gap |
| JPEG420 restarts, fresh Stream | 1,377.942 / 991.962 | 1.389 / 1.473 | 761,816 | Qualified short gap |
| JPEG progressive gray | 2,647.799 / 1,712.344 | 1.546 / 1.634 | 683,138 | Qualified short gap |
| JPEG progressive420 | 2,438.109 / 1,947.562 | 1.252 / 1.446 | 775,336 | Qualified short gap |
| JPEG progressive422 | 3,071.457 / 2,238.428 | 1.372 / 1.508 | 847,162 | Qualified short gap |
| JPEG progressive444 | 5,037.722 / 3,361.009 | 1.499 / 1.715 | 984,298 | Qualified short gap |
| JPEG progressive444, live token | 4,960.361 / 3,350.735 | 1.480 / 1.614 | 984,298 | Qualified short gap |
| JPEG resident Identify | 0.592 / 1.214 | 0.488 / 0.494 | 88 | Independent parity |
| PNG RGBA8, fresh Stream | 1,918.967 / 1,393.950 | 1.377 / 1.583 | 4,552 | Qualified short gap |
| PNG8 Stream, orient/crop/nearest fit | 2,044.522 / 1,476.974 | 1.384 / 1.613 | 701,312 | Qualified short gap |
| PNG RGBA16, fresh Stream | 3,054.270 / 3,091.940 | 0.988 / 0.991 | 8,648 | Independent parity |
| PNG16 Stream, orient/crop/nearest fit | 3,355.487 / 2,946.757 | 1.139 / 1.213 | 1,401,314 | Qualified short gap |
| VP8+alpha, fresh Stream | 9,381.963 / 6,007.841 | 1.562 / 1.793 | 1,966,712 | Qualified short gap |
| VP8L, fresh Stream | 4,320.005 / 3,018.366 | 1.431 / 1.494 | 1,216,276 | Qualified short gap |

The new results replace complete operations invalidated by H4's shared JPEG
frame/scan/dispatch changes and H6d's final encoded-input copy removal. They are
current measurements of previously accepted production changes, not new tuning
or retuned baselines. All original packets, managed pixels/descriptors, native
workers, quality/alpha gates and complete ownership boundaries are unchanged.
JPEG and VP8 retain RGB max4/RMSE1.5, exact alpha and normal/scalar checks;
PNG/VP8L retain exact samples and hidden RGB. Native stream operations reacquire
owned encoded packets through fresh AVIO/EOF paths. Managed includes complete
stream reads/buffering, codec work, intermediate owners for compositions, and
input/output disposal. Native conversions, endian handling and ownership remain
inside timing. Startup, I/O, IPC, preparation and validation remain excluded.

Low PNG allocation counts reflect released private storage reuse and removal
of the final encoded-input copy. The independently owned RGBA16 output still
contains1,062,936 bytes; allocation B/op is not working-memory admission or RSS.
Two simultaneously live outputs, source stability, full reads, caller-stream
lifetimes and surviving output independence are checked before/after timing.

The approved JPEG Identify reference retains its full original structural/EXIF
scan, timed17-byte SOF2 projection and FFmpeg discard-all owned metadata
extraction, without pixel requests. Its existing21 exact input controls are
replayed against the current producer; descriptors, accept/reject behavior and
native worker identities match the prior proof. Direct span limits/cancellation,
changed metadata, independent descriptors and opaque-entropy checks also pass.

Twenty-two three-actual screens and two independent twelve-actual confirmations
retain all90actuals/outliers, native pre/post50ms brackets and both idle-v2 guards.
Only the two plausibly passing screens received confirmations. One pinned CPU,
normal SIMD, nonconcurrent workstation GC and the existing cadence are preserved.
The same immutable Linux producer8b87655188e2a03679cd835b336067d0903d50ffc6c41c50e25f3988e99a33e1
and da41b879... harness are reused. Missing selectors are restored from exact
retained members; compact transports record their original archive/member hashes.
No production/harness/native/selector build, source synchronization, whole-library
audit, new fixture/grid or repeated package qualification was needed.

The unchanged **4,167 tests/OS**,183affected intrinsics-disabled checks/OS and
same actual private Windows package consumer/all3examples on Windows/Linux
remain valid. Other numeric evidence is reused. Stop confirmed passing cases.
[Numeric evidence](warm-input-reconciliation-evidence.json) retains statistics,
allocations, gates, identities and boundaries. Raw on both hosts:
`h10b-measurements.tar.gz`
d3ede3076022f7e27749c1ea29c6cf9fecaad99c2094b804c953627bc99b5ef0.
