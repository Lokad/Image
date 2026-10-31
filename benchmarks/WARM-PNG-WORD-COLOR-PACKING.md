# Full-word PNG color packing

Gray16 default encoding improves by **21.14% filtered / 21.09% all-sample /
21.01% native-normalized**, with separated raw/all/normalized confidence.
Its just-recorded CPE1 independent original is reused; one candidate short and
one independent twelve establish the gain. Allocation remains **1,506,972 B/op**
and the complete **265,433-byte packet** stays exact.

| Existing 513×259 default6/None control | Lokad µs | Ratio / upper | Managed B/op | Packet B, exact both |
| --- | ---: | ---: | ---: | ---: |
| EncodeGray16 | 6,812.24 | 1.254 / 1.285 | 1,506,972 | 265,433 |
| EncodeRgb16 | 20,239.97 | 1.320 / 1.338 | 3,571,252 | 739,472 |
| EncodeGrayAlpha16 | 13,361.83 | 1.314 / 1.333 | 2,264,564 | 506,887 |

RGB16 and gray-alpha16 each receive one independent current-cost confirmation,
without separately paired speedup claims. All three remain confirmed warm
single-CPU gaps. Reference settings/inputs, full sample precision, alpha/hidden
RGB, source and independent output ownership and both idle-v2 guards pass.
Native allocation is unmeasured; managed B/op includes harness costs.

The scalar encoder acquired the same row through Source.Sample four times per
pixel. It now borrows one span after each original 4,096-pixel checkpoint and
packs selected words directly. Guarded AVX-512/VBMI processes eight RGBA16 pixels,
combining channel selection and big-endian byte order. A failing group returns
to the scalar loop, preserving the first invalid pixel and RGB-before-opacity
error order. The portable scalar path also uses the bounded source span.
No new buffer, capacity, owner or admission charge. Working/root limits,
cancellation, actual pending-I/O lifetime and caller ownership remain.

One diagnostic profile has 275 workload/managed samples: Encode accounts for
21.45% exclusive samples and ProcessLazy for 60%. Those are method-attribution
fractions, not component elapsed time; all instrumented times are discarded.
Source review supplies the concrete row-acquisition lead. No additional profile,
input/settings grid, selector/native/harness build or successful retime.

Both OSes pass **5,342 checks**: 2,170 internal, 2,224 public and 948 live.
All 211 focused PNG checks pass normal/scalar on both OSes and AVX-512-off on
Linux. Ten new public cases cover full-word big-endian values, tails/unaligned
row starts/4,096-pixel checkpoints, error ordering and cancellation. Existing
four-compression/all-color admission and fourteen complete native-packet gates
remain. One immutable core/public build per OS, then one internal build at the
accepted milestone; zero differential, benchmark, native or selector builds.

The SAME actual Windows Release package and three examples pass both OSes.
The consumer explicitly checks all three word colors, sync/async equality,
terminal shared refusal before output, cleanup and subsequent success, retained
independent pixels and borrowed budget/input/output ownership. Package
`318791526e4dd7b351856a237fecb54cff8959cd28b9a8c9729e38c4d3587589` is 389,444 B; published releases are unchanged.

Only 16-bit gray/RGB/gray-alpha packing callers lose earlier CPU proof. The three
named default controls are current; unmeasured affected settings get no pass
claim. RGBA16, all byte packing, Deflate/CRC/prediction and unrelated codec,
editing, text and inspection paths retain applicable unchanged evidence. Close
this packing family without vector-width/inlining/mask variants and continue
the full **268-path warm single-CPU conservative upper ≤1×** objective.

[Numeric evidence](warm-png-word-color-packing-evidence.json) retains actuals,
confidence, exact gates, source scope and qualification. Mirrored/member-verified
`png-word-color-packing-qualified.tar.xz`, SHA256
`a5336e1d1290d65c1e5a26db3cb07f97a832ae8eca2fbb06f1be808eaf0995f1` (6,438,100 B / 158 members),
retains original/candidate cores/source, cached driver/worker/sampler, trace,
fixtures, guards/logs, actual package and reproduction scripts. CPE1/FMP1 raw
dependencies remain retained. No full parity or release-readiness claim.
