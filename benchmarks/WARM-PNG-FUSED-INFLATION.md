# PNG framing and inflation in one walk

The RGBA16 Stream/orient/crop/nearest-fit composition confirms **1.216× FFmpeg /
upper 1.231×**, at **3,572.35 µs** and **4,460,393 B/op**. Combining inflation
with framing cuts its retained baseline mean **39.2%**, from **1.993× / upper
2.022×**. Stop this confirmed pair; overall material/current-source coverage
remains open.

The complete operation, original 513×259 word raster, frozen PNG, integer
coordinate reference and native graph are unchanged from the
[baseline](WARM-PNG-COMPOSITION16.md). Every selected word, alpha, geometry,
source/caller lifetime and independent output passes exact checks, including
normal/scalar FFmpeg CLI agreement. No tolerance, quality, shape or input changes
are introduced. Managed timing includes fresh Stream acquisition and all
intermediate owners. Native timing includes fresh AVIO/packet acquisition,
decoding, crop, word conversion, complete point graph and release. One pinned
CPU/codec/filter/conversion thread, normal SIMD, tiering off and workstation
nonconcurrent GC are used. Startup, preparation, I/O, IPC and validation are
excluded; native allocations are unmeasured.

The existing independent RFC 1951 walk can now write literal and history bytes
into a private admitted buffer, eliminating a second Deflate walk. It uses safe
spans and the original prefix tables, bit charges and row processing. Eligible
noninterlaced, noncancelable inputs preflight the exact filtered buffer, at most
4 MiB, plus every original workspace/result/row allowance. A separate exact-size
cache has a combined 4 MiB idle ceiling. Only complete trees and a matching Adler
check enable direct reads. Uncertain trees/checksums use the original BCL reader
without repeating framing work; constrained, cancelable, large and Adam7 paths
retain their behavior. Pixel precision, ownership, filter/palette/checksum failure
order and work limits remain unchanged.

The short screen and independent repeat use the same immutable core/harness.
Confirmation retains all twelve actuals, including one filtered from the mean,
in its conservative upper. Every scored quiet guard passes. One earlier
confirmation pre-guard fails CPU/I/O idle thresholds and launches no benchmark;
its failed receipt is preserved. A live process check and fresh guard permit
confirmation on the same prepared build, without rebuilding or waiting on
historical load.

Two shared-path regression means pass before full qualification: RGBA8 PNG None
**1.455×**, **4,456 B/op**, and RGBA16 Paeth **1.052×**, **8,552 B/op**. These are
three-actual screens; their short confidence uppers are **2.779×** and **1.327×**,
respectively. Their earlier independent confirmations remain linked in the
[byte report](WARM-PNG8-DECODE-QUIET.md) and [Paeth report](WARM-PAETH-VECTOR.md);
no new independent confirmation is claimed for those screens.

Both OSes pass the full **3,332 Release tests**, including **896 live FFmpeg
comparisons**, plus one additional normal/scalar complete-long-code control.
The resulting 3,333 distinct normal checks and 146 focused normal/scalar checks
per OS cover values beyond the root lookup, overlapping/max-distance history,
all literal truncations, output bounds, exact bit work, admission, pixels and
failure order. Production bytes stay frozen for the additional test; full suites
are not repeated. One actual Release package passes its consumer and all three
usage examples on Windows and Linux.

[Numeric evidence](warm-png-fused-evidence.json) retains all measurements,
allocations, native brackets, guards, actual generated children and package/source
identities. Immutable raw archives include the failed pre-guard and qualification.
Only SPEC, resource documentation and the additional test change after timing;
production and benchmark inputs remain unchanged. No unrelated method audit,
native rebuild or repeated unchanged package proof is performed.
