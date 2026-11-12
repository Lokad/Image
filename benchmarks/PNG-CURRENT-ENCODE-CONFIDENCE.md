# Current PNG encode confidence

Two existing complete controls reuse the prepared adjacent driver, frozen fixtures
and actual package DLL. RGB16 passes/stops; grayscale8 remains uncertain and is
not a demonstrated optimization target. No production speedup is claimed.

| Fresh complete encode | Lokad µs | FFmpeg µs | Ratio / conservative range | Managed B/op | Complete packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGB16, 4 operations per subject | 15,418.18 | 16,086.09 | 0.958 / 0.936–0.977 | 2,427,377 | 739,472 |
| Grayscale8, 64 operations per subject | 2,054.47 | 2,009.79 | 1.022 / 0.951–1.068 | 547,123 | 72,586 |

Each capture retains 12 alternating pairs and 12 native batches before/after. The
conservative bound includes paired log ratios and all-actual/diagnostic managed
confidence against the fastest native bracket. Gray's longer batches still leave
wide confidence; no repeat seeks a favorable result. Native gray before/paired/
after means are 2.014/2.039/2.010ms. Natural-GC conditioning differs from the original
post-GC global brackets, retained separately.

Gray's older adjacent capture predates PIF873 IDAT forwarding (PngCodec 1cc625→
dbce48), ABP875 block Adler and WCF876B wide encoder CRC. Those production paths
invalidate its CPU confirmation. RGB16's recent adjacent confirmation remains
source-valid and is reused without remeasurement. Current source/module identities
and all original gates pass; the completed resize change does not alter PNG paths.

Both sides time fresh codec/conversion/zlib/working-frame/output preparation and
disposal, including every complete managed EncodePng/MemoryStream operation and
native frame/packet allocation. Native codec/conversion state is not retained.
Frozen Ready flags and actual boundaries override generic driver prose about
retained wrappers. Inputs/settings, full-depth samples, normal/scalar CLI, exact
complete packets, source immutability and independent outputs remain unchanged.
Single CPU0, nonconcurrent workstation GC and idle-v2 guards pass.

Gray's exact 72,586 B packet remains SHA256
`c5b4a248c1ca0f76115d06e7795698f50ac7e623c366fcee4a659cb08da2872a`.
Its direct allocation mean rounds up to 547,123 B/op; differing older BDN/4-operation
counters are instrumentation observations, not savings. Both controls retain the
same 384 KiB logical deflater workspace and zero operation scratch. Physical/native/
JIT/pool peaks are unmeasured; output capacity and ownership are unchanged.

Reuse 6512 Windows/Linux full/live checks and SAME-package/all three examples from the
qualified current producer. No production, embedded/public tests, driver, harness,
native, fixture, package rebuild or broad qualification repeat. Current 17 fresh
PNG controls remain 16 pass/one uncertain. Retained-native stored-word is still a
separate demonstrated gap; fresh and natural-GC stored views pass only their scopes.
Next investigate that gap through a materially revised stored-only design, without
retuning uncertain gray or passing RGB16.

[Evidence](png-current-encode-confidence-evidence.json) pins both current scopes.
New gray raw `png-gray-current-confidence.tar.xz`, SHA256
`68fd5d1f0302a11bbb7cb681c32e01869e9aef204931895f59e37b608f0ba0b0` (383,348 B/20 members), is mirrored/member-verified;
dependencies retain the reused [RGB16 capture](PNG-RGB16-CURRENT-CONFIDENCE.md),
[older gray study](PNG-GRAY-ADJACENT-CONFIDENCE.md), reference workers and actual package.
All 289 required paths remain; publication stays postponed.
