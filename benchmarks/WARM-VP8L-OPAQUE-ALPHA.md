# VP8L opaque alpha coding

Historical accepted round; [current previous-row encode measurements](WARM-VP8L-PREVIOUS-ROW.md) replace these three timings. The alpha-coding size reduction remains applicable.

Opaque lossless output shrinks **24.9%**, from 533,100 to **400,262 bytes** on the
existing 513×259 opacity projection. Managed allocations fall from 2,100,170 to
**1,051,213 B/op** (49.9%). Exact RGBA is preserved. All three affected complete
encode operations independently reconfirm warm single-CPU parity.

| Existing input | Lokad µs | Backend µs | Ratio / upper | Allocated B/op | Encoded B, Lokad / backend |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque RGBA8 | 6,251.86 | 70,476.34 | 0.089 / 0.090 | 1,051,213 | 400,262 / 121,896 |
| RGBA8 with alpha | 7,045.64 | 122,788.28 | 0.057 / 0.058 | 2,100,170 | 533,100 / 183,812 |
| RGBA16 with alpha → 8-bit | 7,737.99 | 120,636.47 | 0.064 / 0.065 | 2,100,119 | 533,100 / 183,812 |

The approved reference is **libwebp exact=1, FFmpeg's encoder backend**, with
effort4 and native quality75 as a lossless compression control. Reuse the existing
two original rasters and one documented opacity projection. These inputs do not
establish photographic/text compression efficiency: the opaque packet remains
3.28× the backend's size, and that broader work remains open.

The encoder uses a zero-bit alpha255 leaf only when its header saves bytes.
Counting omits whole-byte alpha fields, then adds their exact cost or the
29-byte normal-singleton header difference. The original two passes, source-plus-
8 KiB workspace, alpha-field work/cancellation checks and output preflight remain.
No histogram, extra input scan or frame copy is added. Alpha-bearing packets stay
byte-identical. [RFC 9649](https://www.rfc-editor.org/rfc/rfc9649.html#section-3.7.2.1)
defines the single-leaf representation.

Times include public EncodeWebp, admission, normalization, fresh owned output
growth and disposal. Native calls include required normalization/repacking,
pixel import, encoding and independent packet acquisition/release. Only wrappers
and released scratch are reusable; pixels/packets are not cached. Native
allocations are unmeasured. Startup, IPC, disk I/O and validation are excluded.

Three-actual screens precede independent twelve-actual confirmations on one pinned
CPU of the quiet VM, with native pre/post brackets and both versioned idle guards.
All actuals and selected/all-actual conservative bounds are retained. Reuse the
prepared drivers, immutable producer, native worker and frozen inputs; no harness/
native rebuild for timing, profiling or settings grid. Normal/scalar FFmpeg and managed decode,
source preservation and independent owners pass before/after timing.

All **4,153 Release tests/OS**, **24 affected hardware-off checks/OS**, and the same
actual Windows package's consumer/all three usage examples pass on Windows/Linux.
Compressed ALPH admission fixtures now declare their own full-alphabet trees,
keeping their exact workspace boundary independent of production compression.
No tracked image corpus is regenerated. Other executed paths reuse valid evidence;
the three changed encode paths replace their older timings. Stop these confirmed
cases. [Numeric evidence](warm-vp8l-opaque-alpha-evidence.json) records current
allocations, sizes, confidence and qualification. Full required parity stays open.
