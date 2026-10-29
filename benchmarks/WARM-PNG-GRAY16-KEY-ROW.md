# Direct PNG gray16 color-key expansion

Complete gray16 color-key decoding improves **76.2%**, with unchanged
**2,456B/op**, and independently passes warm one-CPU parity. Stop this case.
Both filtered/unfiltered raw99.9%CIs and conservative normalized envelopes
separate from the valid prior independent confirmation.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Prior gray16 tRNS | 2,939.882 | 1.441 / 1.443 | 2,456 |
| Current gray16 tRNS | 699.699 | 0.341 / 0.342 | 2,456 |

The missing dense-row path used generic per-pixel mapping. A direct row primitive
retains both gray bytes, expands to native-endian RGB and compares the complete
two-byte transparency key. A bounded AVX2 broadcast/shuffle handles four pixels;
alpha is zero only for exact matches, retaining hidden RGB. Portable scalar
tails and direct cancellation checks remain. No workspace or owner is added.
Dense keyed-gray16 rows select it; sparse passes and other formats retain their
algorithms. Unchanged Gray8/RGB8/RGB16 key controls remain passing and stop.

One fresh original/candidate3actual screen clears5%. Candidate12actual
confirmation reuses PXT1original12:76.200%filtered/76.203%all elapsed reduction,
76.332%normalized point gain. Frozen original packets/native settings, full
decode/normalization/owned output/disposal and exactsamples/alpha/hiddenRGB/
source/owner gates remain. All raw/native brackets and idle-v2 guards are retained.
Native allocations are unmeasured; no newimage/settings or timing sweep.

All5,129checks/OS pass (2,046internal,2,135public,948live), plus36kernel and231public
checks normal/scalar perOS.22new cases cover every65,536sample under8explicitkeys,
fullword and low-byte distinctions, hiddenRGB, tails/unaligned guards and
cancellation. The SAMEactualReleasepackage andthreeexamples pass bothOS.
Onecorebuild/OS,zero harness/native/selector builds; immutable binaries reused
throughout. Published releases remain immutable.17existingPNG costs still need
refresh; this scoped pass does not complete the full268pathobjective.

[Evidence](warm-png-gray16-key-row-evidence.json) and
[prior color-key costs](CURRENT-PNG-TRANSPARENCY-COSTS.md).
Mirrored/member-verified archive png-gray16-key-row-simd-qualified.tar.xz, 136members,
SHA256 0bce9a8981b3655d152ec1fc02c5883e1f374290f6ff66ae32ff5443d29a0af3; reproduction dependencies pinned by hash.
