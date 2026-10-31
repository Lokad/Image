# Current full-resolution CMYK JPEG costs

The existing DC-only control has a current independent confirmation:
**383.588 µs**, **1.011× / upper 1.023**, **1,085,758 B/op**.
Its lower bound is **0.994×**: confidence remains open, with no passing or
confirmed-gap claim. No repeat is added. Other three applicable costs are reused.

| Existing owned RGBA8 control | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| DC-only | 383.588 | 1.011 / 1.023 | 1,085,758 | Confidence open |
| Horizontal AC | 535.551 | 1.366 / 1.396 | 1,085,755 | Gap |
| Sparse 2D | 638.035 | 1.442 / 1.481 | 1,085,758 | Gap |
| Dense | 2,786.896 | 0.894 / 0.916 | 1,085,661 | Pass; stop |

These are the original 513×259 authored four-component packets, normalized to
fresh independently owned RGBA8. Complete warm one-CPU decode, conversion,
materialization and release are timed against the frozen FFmpeg decoder.
They exercise DC, horizontal, sparse-two-dimensional and dense coefficient paths;
this does not claim representative photographic CMYK or caller-buffer costs.
Native allocation is unmeasured.

The 10,910-byte DC packet and settings are unchanged. Its old derived RGB oracle
used rounded device conversion. Independent DC component samples and the current
Q16 reciprocal produce the exact original native frame, SHA256
2ad208860a59692ff79b51a0eb85ab00d2b91a4213d4bdf8f5c61ae320b9505b.
Only temporary derived RGB/metadata change; canonical CMYK is already exact.
Managed/native normal/scalar pixels, source/output ownership and both quiet-v2
guards pass with zero error. Original derived files are restored byte-exactly.

Zero builds, profiles, new benchmark inputs or repeated qualification. Current
5,295 applicable checks per OS and the same actual Release-package/three-example
proof are reused. Horizontal/sparse costs retain JRO1 confirmations; dense retains
its valid pass through the explicit unchanged-path proof. No causal timing claim
is made against old unmatched producers.

The independent canonical frames show 226 identical adjacent sample rows in
DC/horizontal controls and 65 in sparse 2D. Reusing completed color rows is a
future feasibility lead, not a measured gain. DC block colors vary horizontally;
no constant-image assumption is made. Dense passes and stops; DC confidence stays
open, while the confirmed horizontal/sparse gaps can guide further work.

[Evidence](current-owned-cmyk-jpeg-costs-evidence.json) pins actuals, references,
pixel oracles, exact restoration and reuse. Raw current-dc-cmyk-jpeg-confirmation.tar.xz
SHA256 578415ef098e1e5a43bc8f253798acf861580c6a5546ae9d08ed22c2d69b1940
(1,056,648 bytes, 40 members) is mirrored/member-verified, with the
[qualified JRO1 archive](WARM-JPEG-COMPLETED-ROW-REUSE.md) as dependency.
The full 268-path ≤1× objective remains open.
