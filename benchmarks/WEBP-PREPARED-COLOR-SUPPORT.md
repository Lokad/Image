# Prepared WebP color support

Elapsed/allocation figures below precede the subsequent
[cache-prefix aggregation](WEBP-CACHE-PREFIX-AGGREGATION.md), which has current
qualified costs. Historical proof and unchanged-method attribution leads remain.

Clustering now prepares operation-owned support for full256-symbol color banks,
ORs it during merges and rebuilds it during replay. Sparse pair scoring skips
known-zero bins, accounting exactly for gaps and equal-count runs across words.
Fuller color banks use direct traversal; green/cache and distance stay direct.
One shared run-cost model also removes the separate single-population entropy
and run scans. Integer costs, order/ties, full final plans and packets are exact.

| Complete warm encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Admitted peak owned B | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Original RGBA8 / libwebp exact=1 | 133,142.31 | 121,487.41 | 1.096 / 1.111 | 25,289,136 | not measured | 183,812 |
| Photograph / libwebp exact=1 | 94,867.74 | 83,054.28 | 1.142 / 1.151 | 13,736,480 | 10,166,464 | 160,032 |
| Text/diagram / libwebp exact=1 | 9,075.67 | 10,233.72 | 0.887 / 0.905 | 6,686,461 | 4,802,292 | 7,234 |
| Transparent VP8 / FFmpeg | 67,281.66 | 53,514.67 | 1.257 / 1.265 | 11,526,656 | 4,085,883 | 98,590 |

Four independent confirmations retain twelve actuals, twelve native samples
before/after, source/worker identities and both quiet guards. Frozen complete
packets, reconstruction/quality, alpha/hiddenRGB, normal/scalar native pixels,
source and independent-output gates pass unchanged. Libwebp exact=1 is explicitly
FFmpeg’s encoder backend. Diagram now passes the1×target and stops. Ordinary
RGBA8, photo and alpha retain confirmed CPU gaps. Claims cover these controls.

OrdinaryRGBA8 saves5.72ms /4.12% raw (4.43% reference-normalized); photo saves
2.09ms /2.15% (2.19% normalized). Both raw and normalized confidence separate.
Diagram saves0.71ms /7.22% raw (14.28% normalized), with separated confidence;
its native clock rises8.24%, disclosed rather than attributed to managed work.
Other native clocks move+0.32% /+0.03% /+0.20% (ordinary/photo/alpha). Alpha’s
2.08% raw point gain overlaps, with no gain claim or favorable resampling.

The metadata tradeoff adds205,968B/op for ordinaryRGBA8. Matched direct counters
add137,328 /17,008 /9,888B for photo/diagram/alpha; the authored small control
adds48B. Photo/diagram admission peaks rise137,312 /16,992B; alpha/small peaks
stay unchanged. Direct and BenchmarkDotNet figures have differing small harness
costs; retain both. No scratch survives disposal. Total allocation is not peak
live capacity; ordinary admitted/physical peak and native allocation are unmeasured.
Coarse admitted peaks exclude borrowed input/caller streams.

At most four UInt64words per full color bank live in the existing working owner,
with actual capacity/header admission before allocation. Compact constant banks
need none; the small-tile admission path stays unchanged. Dense/sparse dispatch
uses actual combined support. Selected published plans remain full and independently
owned. No pool/cache/new governor/fixture-specific path, available-budget allowance,
refusal fallback or changed numerical policy. Cancellation and cleanup cover
actual users. Shared final-cost arithmetic avoids a second maintained cost model.

Windows/Linux each pass5,450 full/live checks and295 affected normal/scalar checks.
Ten new checks cover word/gap/run/density/high-count/unaligned/cancellation/no-
allocation cases, independent plans and terminal refusal/cancellation recovery.
1,120existing frozen color pairs also exercise prepared scoring, alongside the
10,080cost checksum and frozen native maps/every population/packets. The same
actual0.2.1Release package and three usage examples pass both OSes. One core and
internal build perOS; zero public/harness/native/selector builds. Immutable DLLs
are reused through scoring and broad --no-build qualification.

One initial ordinary-screen preparation hit an existing tool-directory name and
stopped before timing. Its corrected separate directory uses the same build;
failed setup/script retained, no scored-job repeat. One ordinary short and one
independent confirmation per control; valid unchanged before evidence is reused.

Other affected VP8L / VP8 lossless-alpha encoding costs remain historical.
Opaque VP8, all decoders, other codecs, resizing and text/drawing reuse unchanged
proof. Complete public operations still include preparation, normalization,
candidate selection and independent owned output growth/disposal. Native wrappers/
scratch retain the established prepared-state labels; full per-call import and
encoding remain timed. Startup/I/O/IPC/checks are excluded. This is not a newly
equivalent one-shot native-context comparison. Publication remains postponed.

[Evidence](webp-prepared-color-support-evidence.json) retains all actuals, direct
diagnostics, source/runtime/package identities, qualification and adverse setup.
Raw webp-prepared-color-support.tar.xz: 18,293,128B /532members, SHA256
4974a1aecef6f89d389ab193d91252ef7f24201bf27df10f3993c9c5d88f9406, mirrored/member-verified. Before dependencies:
webp-constant-bank-workspace.tar.xz (SHA256
cb7feb8c8ded7a5693021ca0d20e7e3299e1df3f8db6235853cf761de0a255d6) and
current-rgba-vp8l-workspace-attribution.tar.xz (SHA256
3a249ddcf33e8e81fc727f824080b63d43bda1095b3ed2629088b40eb2fde580).
The [support audit](VP8L-PAIRED-BANK-SUPPORT-AUDIT.md) records the original lead;
its counts alone were not performance proof.
