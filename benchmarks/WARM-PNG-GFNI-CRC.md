# Exact GFNI reversal for PNG CRC

Complete seekable PNG Identify improves **14.16% filtered / 14.27% all elapsed**,
22.759→19.537µs. It remains **1.226× / upper1.232**, with unchanged192B/op.
Fresh original/candidate three-actual screens precede one independent twelve
each: raw99.9% intervals and conservative normalized envelopes both separate,
clearing the predeclared5% gate. No variant or successful timing repeat.

One private input step uses GFNI to reverse bits independently in each byte,
then retains the original byte-order shuffle and CRC folding. Matrix byte r
is basis bit r, derived from the
[Intel affine definition](https://cdrdv2-public.intel.com/835748/252046-sdm-change-document.pdf).
Hardware checks cover65,536impulse and4,096mixed-lane outputs against literal
reversal. Original nibble/PCLMUL and scalar fallbacks remain. Polynomial powers,
reduction,seeds,tails,thresholds,cancellation and every packet stay unchanged.
No buffer/table/cache,extra owned storage,admission model or owner is added.

The same218,775B513×259PNG and nine source facts are retained for Identify,
including complete CRC/container/EXIF validation,input/EOF,fresh ownership and
release. Neither side decodes pixels; header-only inspection is a separate scope.
Eleven existing affected comparisons are refreshed,without new image/settings
grids or paired companion gain claims. Packet sizes/hashes and all original
pixel,quality,native-boundary,source and ownership gates remain frozen.

| Existing complete operation | Lokad µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Seekable Identify | 19.537 | 1.226 / 1.232 | 192 | Gap |
| Async nonseekable Identify | 20.312 | 1.051 / 1.060 | 8,400 | Gap |
| Resident Identify | 13.079 | 1.002 / 1.005 | 96 | Gap |
| SmallestSize RGBA8 | 22,600.320 | 1.306 / 1.331 | 1,325,732 | Gap |
| SmallestSize RGBA16 | 26,874.288 | 1.337 / 1.347 | 3,813,732 | Gap |
| Fastest RGBA8 | 6,083.650 | 1.398 / 1.420 | 1,344,428 | Gap |
| Stored RGBA16 | 203.451 | 0.500 / 0.539 | 1,741,468 | Pass |
| Predictable RGBA16,3000square | 352,397.336 | 0.865 / 0.870 | 3,402,572 | Pass |
| Validate PNG8 | 1,382.593 | 1.003 / 1.010 | 240 | Confidence open |
| Validate PNG16 | 2,799.737 | 0.946 / 0.956 | 240 | Pass |
| Validate indexed Adam7,19x17 | 5.352 | 1.363 / 1.386 | 1,168 | Gap |
| Validate PNG16 async | 2,850.602 | 0.791 / 0.802 | 8,544 | Pass |

Stored/predictable PNG16 and ordinary/async PNG16 validation pass and stop.
PNG8 validation remains confidence-open(lower0.995,upper1.010);keep its one
observation without retiming. Resident Identify is a small confirmed gap:
lower1.00034,upper1.00466. No CPU-pass or broader content claim follows.

Both OS pass **5,193checks each**(2,097internal,2,148public,948live),plus
642internal/386public focused checks in normal,GFNI-only-disabled and scalar
modes. Existing exhaustive literal-polynomial,all-byte/position,unaligned
length/tail/chunk/cancellation coverage is reused;no mirrored tests. Current
21-control stream,resident and actual-ReadAsync scopes pass. The same actual
Release package,governed async consumer and three examples pass on both OSes.

One core build per OS is frozen throughout. All harnesses,native workers and
selectors are reused. The cached generic12-actual encoder selector has four
invocations per actual and correct one-call unrolled bodies;reported costs and
allocations are per complete operation. Predictable native samples also batch
four. Untimed proof refreshes preserve fixtures/settings and restore originals.

All affected displayed rows now use this producer. Other affected PNG CPUcosts
need applicable current evidence;historical passing costs are not current proof.
CRC-opaque probes/header inspection,JPEG,WebP,editing/text retain relevant
unchanged-path evidence. The full **268-path≤1×** objective stays open.

[Evidence](warm-png-gfni-crc-evidence.json). Mirrored/member-verified
png-gfni-crc-qualified.tar.xz,343members,SHA256
9c3c42947542991f480bbe06c900e50e871ad75756bb30d48ddc354f7d7a5330;build/reference dependencies are pinned.
