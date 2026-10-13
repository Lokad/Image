# Warm PNG gray16 transparency

The existing decoder independently confirms **1.722× FFmpeg**, conservative
upper **1.755×**, at 3,501.16 µs with **10,960 managed bytes allocated per
operation**. All 12 actual results are retained; both quietness guards pass.
Production is unchanged. Stop tuning this fixed pair; uniform required warm
parity remains open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Three-actual coverage screen | 3,499.49 | 1.721 | 1.883 | Pass |
| Independent twelve-actual confirmation | 3,501.16 | 1.722 | 1.755 | Pass |

Both subjects decode the same original 513×259 gray16 PNG, filter None and
noninterlaced. The existing RGBA16 raster's full red word is replicated into
RGB; the two-byte tRNS key 0x7f7f makes three pixels transparent, retaining their
hidden RGB 32,639. The other 132,864 pixels have alpha 65,535. No sample is
normalized to eight bits. The previous gray8 transparency PNG and reference
remain byte-identical. Only the existing two original performance rasters are
used; no profile sweep is added.

Managed decoding, normal/scalar FFmpeg decoding, chunk lengths/key, full sample
words, alpha, hidden RGB, unchanged input and independent held owners pass
before and after measurement. Native timing includes decode, YA16BE-to-owned
RGBA64LE conversion and frame release. Managed timing includes public Decode,
independent output acquisition and disposal. Released pixel storage may be
reused; resident input, contexts and wrappers are reused. Startup, I/O, IPC,
fixture generation and checks are excluded. Native allocations are unmeasured.
Sequential Release runs use one pinned CPU, one codec/conversion thread,
normal SIMD, tiering off and workstation nonconcurrent GC.

Fresh Windows/Linux builds and both actual generated benchmark children match
all **600 production method bodies and flags** against the already validated
actual Release package. Its previous **2,729 Release tests per OS** and all
three examples through the same package on each OS are reused for unchanged
production. This round adds benchmark coverage only.

The [numeric evidence](warm-pnggray16trns-evidence.json) preserves the screen,
independent confirmation, allocations, exact identities and prior validation
reference. All four actual child files, source, raw evidence and actual package
were frozen before subsequent preparation or mutation. The
[benchmark summary](../docs/benchmark.md) reports this fixed pair at its measured
source; RGB transparency and other required material coverage remain open.
