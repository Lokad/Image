# PNG composition storage attribution

Three untimed traces of the original public PNG16 composition reproduce every
frozen output sample on the current actual-package DLL. The two warm rounds agree:

| Complete public stage | Managed allocation B/op |
| --- | ---: |
| Decode(Stream) | 8,600 |
| AutoOrient(Normal) | 56 |
| Centered crop | 264,272 |
| Nearest fit | 65,160 |
| Original timed MemoryStream creation | 72 |
| Complete operation | 338,160 |

The total exactly reconciles the [current precision measurement](PNG-WORD-COMPOSITION-PRECISION.md).
Crop plus nearest account for 329,432 B/op. Four independently owned pixel arrays
actually overlap: 1,062,936 + 1,062,936 + 264,192 + 65,024 = **2,455,088 B**.
They cannot alias while their callers retain them. The default two-slot pool
reuses the large frames; after crop disposal, `ReturnLargest` replaces its idle
array with the larger decoded frame. Idle payload ends at **2,125,872 B**, within
the existing 4 MiB ceiling. No new retained storage or physical-peak measurement.

Nearest resize currently calls cleared, nonreusable `ImageBuffer.Allocate`, even
though it privately overwrites every output sample and disposes failures before
publication. Investigate that ownership boundary and the existing fixed idle-slot
topology together before further kernel tuning. Prefer operation-owned reuse;
these measured simultaneous owners rule out sharing their buffers in this exact
operation. Any existing-pool revision must retain its byte ceiling, exact lengths,
independent live owners, caller-root bypass, refusal and cancellation behavior.
This is a design lead, not automatic adoption or permission for a general cache.

Stage allocation reads exclude trace scaffolding and reflection snapshots. Hashes
and observed identities prove this scope; there are no elapsed-time claims, CPU
forecasts or isolated GC-pause causes. One small Windows diagnostic build, zero
production/embedded/public/benchmark/native/fixture/package builds. Qualified
source, modules and package remain unchanged; reuse both OSes' 6,750 full/live
checks and SAME actual-package consumer/all three examples. Passing codec and
resize kernels remain stopped until an accepted change actually affects them.

[Evidence](png-composition-storage-attribution-evidence.json) retains all 24
ownership snapshots, stage allocations, source identities and reconciliation.
Mirrored/member-verified `png-composition-storage-attribution.tar.xz`: SHA256
`832a0e038a2eb412b7401b49d1b3b352fc1fd3276a51cb346b7e3a136659eaf2`, 620800 B /22 members. Archived `pal1149-*`
recipes and pinned input/runtime dependencies reproduce the three original
operations. The current performance comparison remains uncertain, with all prior
actuals retained; full 289-path completion and publication remain open.
