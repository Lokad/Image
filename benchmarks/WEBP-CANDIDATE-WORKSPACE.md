# WebP candidate workspace

The accepted HWR76 stage compacts sparse histogram populations and transfers a
rejected token candidate's storage to tracing. Logical tile order, candidate
selection, tie-breaking and complete packets stay unchanged. Dense populations
retain direct addressing. Storage stays owned by one operation; selected
candidates remain independent. No cross-operation pool or fallback is added.

| Complete encode | Allocated B/op, before → after | Admitted peak B, before → after | Exact packet B |
| --- | ---: | ---: | ---: |
| Lossless photograph, 513×259 | 20,416,808 → 19,327,976 | 12,217,052 → 12,217,052 | 160,032 |
| Lossless text/diagram, 513×259 | 27,417,688 → 10,481,064 | 20,929,228 → 6,174,402 | 7,234 |
| Transparent lossy with lossless alpha, 513×259 | 21,348,720 → 19,224,016 | 6,579,383 → 6,579,383 | 98,590 |
| Original bars generalization, 127×65 | 966,592 → 933,264 | 312,716 → 312,716 | 660 |

The diagram has 265 populated tiles out of 2,145, previously requiring an
18,120,960-byte dense population array. The photograph has 2,142 populated tiles;
the added occupancy pass keeps its dense layout. Empty tiles do not require
replay unless live populations merged. Trace reuse resets live entries and cache
metadata while keeping its charge; transferred storage is disposed on failure.

Measurements use complete public encodes, resident original inputs and fresh
MemoryStream growth/disposal. Peak admission is a separate run, excludes borrowed
inputs/caller-owned stream capacity, and is a conservative ownership charge, not
physical heap usage. All operation-owned charges return to zero. No retained
cross-operation storage is introduced. Allocation reductions alone imply no CPU gain.

Three direct quiet-VM batches observed approximately 12% less elapsed time for
the diagram, neutral photograph time and 2.6% less alpha-control time. These are
diagnostic screens, **not independently qualified CPU improvements**. Historical
VP8L and transparent-VP8 encoding costs remain labeled as pre-workspace evidence;
other executed production paths are unchanged. Setup was not removed from timing.

Both Windows and Linux pass 5,383 full/live checks and 67 affected checks in
normal and scalar modes. The same actual Release package passes both consumers
and all three usage examples. A separate original 383×193 packaged-consumer
image exercises sparse populations, hidden RGB, sync/async packet equality,
terminal refusal before output, borrowed inputs/root and surviving result lifetimes.
Published releases remain unchanged; publication is postponed.

[Evidence](webp-candidate-workspace-evidence.json) records packets, identities,
allocations, diagnostic observations and qualification. Raw
webp-candidate-workspace.tar.xz: 1,587,996 B / 75 members,
SHA256 5a1492e1abc26a1f32e6abad99df161aebfa4962349041e1647f383d99adf806,
mirrored/member-verified. It retains original/selected sources and producers,
replay scripts, guards, complete logs, and old/new package artifacts.
