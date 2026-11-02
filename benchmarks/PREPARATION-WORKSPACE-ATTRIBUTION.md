# Preparation and workspace attribution

The existing resize comparison times managed preparation against a prepared
native graph. These boundaries are retained and explicitly labeled. A quiet-VM
diagnostic using the unchanged production DLL compares additional boundaries:

| Lanczos3 RGBA8, 513×259 → 256×129 | µs/op | Managed allocated B/op | Retained preparation |
| --- | ---: | ---: | --- |
| Managed public one-shot | 810 | 138,416 | None |
| Managed diagnostic with prepared Q30 axes | 699 | 114,928 | 24,932 admitted B |
| Native zimg prepared execution | 544 | Unmeasured | Graph, input planes, scratch |
| Native zimg one-shot preparation/execution/disposal | 1,763 | Unmeasured | None between calls |

Both native modes include input/halo packing, independent planar/packed output,
filtering, unpremultiplication, quantization and disposal. Managed pixels remain
exact against the frozen oracle; native max error ≤1 and RMSE ≤0.25 remain.
Prepared managed execution still allocates its working rows. Geometry alone is
about 107 µs / 23,536 B. Coarse warmed phase snapshots put row preparation around
17 µs and computation around 690 µs. These direct screens are attribution, not
independent performance qualification or precise subtraction of method costs.

FFmpeg's PNG worker retains AVCodecContext; the pinned encoder initializes its
zlib context once and resets it per frame. Managed Deflate allocates history,
dictionaries and tokens each call: 659,576 B, about half the complete PNG
allocation. Its observed construction time is only 12–26 µs against complete
7–25 ms encodes. Reuse has an allocation motivation; CPU benefit is not inferred.
Equivalent native one-shot PNG and a managed reusable session remain future work.

| Complete WebP control | Total allocated B/op | Histogram-phase B | Token + trace-storage B | Coarse admitted peak B |
| --- | ---: | ---: | ---: | ---: |
| Lossless photograph | 20,416,808 | 9,987,984 | 6,450,656 | 12,217,052 |
| Lossless text/diagram | 27,417,688 | 18,743,184 | 6,174,352 | 20,929,228 |
| Transparent lossy, including lossless alpha | 21,348,720 | 2,157,856 | 12,340,320 | 6,579,383 |

Photograph/diagram complete packets remain exactly 160,032 / 7,234 B; the alpha
control is exactly 98,590 B. The diagram allocates populations for 2,145 tiles,
but only 265 contain token starts: 18,120,960 B of dense population storage.
The photograph uses 2,142 of 2,145 tiles. This supports compact storage with
unchanged logical tile order, retaining dense addressing where appropriate.
Rejected parsing candidates also die immediately before new trace buffers of
the same capacity are allocated; transferring that storage needs no global pool.
External libwebp reuses hash/reference storage and recycles token blocks; this
is algorithmic insight only, with no upstream code entering production.

Allocated bytes, admitted peak and retained axes are different quantities.
Peak figures come from separate shared-budget runs with conservative library
charges, excluding borrowed source/caller-owned stream capacity; they are not
physical heap peaks. Coarse timers live only in an isolated copy of owned source,
outside pixel/symbol loops. Initial cold ledger overhead is retained and corrected
by warming only the affected phase observation, without repeating complete costs.

[Evidence](preparation-workspace-attribution-evidence.json) retains actuals,
source/producer identities, exact outputs and all quiet guards. Raw
preparation-workspace-attribution.tar.xz: 1,214,132 B / 511 members,
SHA256 07a1e6dc52f6ac621363250ec202dba01cce3d7149c9080824bc5b0a466cf23e,
mirrored/member-verified. It retains original source, diagnostic variants and
producers, replay scripts, initial/corrected observations and native adapter.
No production/package rebuild or numerical-policy change occurred in this round.
