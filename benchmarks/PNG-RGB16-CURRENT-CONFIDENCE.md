# Current RGB16 PNG confidence

The existing 513×259 RGB16 level 6/filter None complete encode now **passes/stops**
on the actual development-package DLL. This resolves measurement uncertainty;
no production speedup or allocation saving is claimed.

| Complete fresh encode | Lokad µs | FFmpeg µs | Ratio / conservative range | Managed B/op | Complete bytes, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGB16 PNG | 15,418.18 | 16,086.09 | 0.958 / 0.936–0.977 | 2,427,377 | 739,472 |

One existing driver measures 12 alternating adjacent pairs, four complete operations
per subject and 12 native batches before/after. Every actual remains; conservative
confidence combines paired log ratios and all-actual/diagnostic managed confidence
against the fastest native bracket. Native means are 16.098/16.124/16.086ms.
This natural-GC sampling scope differs from the original post-GC global brackets,
which remain historical and uncertain. No unchanged favorable repeat.

Both sides include fresh codec preparation and output/disposal: managed public
EncodePng with fresh MemoryStream growth; native codec/options/zlib/conversion
context, word-endian frame allocation/conversion, packet acquisition and cleanup.
No native codec or conversion frame survives between operations. Frozen Ready
flags and the actual boundary govern this statement; the generic driver's prose
about documented retained wrappers does not override them. Source fixture/JIT/
validation/IPC remain excluded equally; CPU0/one thread and idle-v2 guards pass.

Complete packets remain exactly 739,472 B, SHA256
`4b8a2c3c99342ab51f2919c35f5dfb50506cb6c5812e84ede61c53f0db7388bf`.
Original inputs/settings, full 16-bit samples, alpha/hidden RGB, source immutability,
normal/scalar CLI and independent surviving-output checks pass. Direct allocation
ranges 2,427,232–2,427,480 B/op; mean 2,427,376.67B is rounded up in the table.
The earlier BDN 2,427,356 B counter is different instrumentation, not a saving.
Logical deflater workspace remains 384 KiB and operation scratch retention zero;
physical/native/JIT/pool peaks are unmeasured. No storage or ownership changes.

Reuse the unchanged actual package's 6,512 Windows/Linux full/live checks and SAME
package/all three consumer examples. Production, embedded/public tests, driver,
harness, native, fixtures and package are not rebuilt; no qualification repeat.
The current 17 fresh PNG controls have 16 passes/one Gray8 uncertainty. Retained-native
stored-word performance remains a separate gap; its passing equivalent fresh and
natural-GC scopes are unchanged. Do not infer a Gray8 gap from uncertainty.

[Evidence](png-rgb16-current-confidence-evidence.json) pins actual package, producer,
source/worker identities, original boundary and all checks. Raw `png-rgb16-current-confidence.tar.xz`,
SHA256 `aedc11766ae8c5960200fb2254d3043c1b32414f7d2cd555596feaeab1a34b5e` (380,740 B/18 members), is mirrored/member-verified.
The [wide-CRC report](PNG-WIDE-CHUNK-CRC.md) retains superseded RGB16 global-bracket
costs and source-valid other controls. All 289 required paths remain; publication
stays postponed.
