# Packed preparation and filtered rows

BPC688 accepts operation-owned source-row preparation and contiguous filtered
rows for RGBA8 bilinear enlargement. The existing complete public Resize control
saves **201.65 µs / 9.05% raw** and passes its declared warm reference boundary.
Filtered and all-actual confidence intervals separate in both raw and conservative
native-normalized views. This is a scoped gain, not all-path parity.

| Complete 513×259 → 1027×519 | Managed µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified original, independent confirmation | 2,229.37 | 1,986.32 | 1.122 / 1.149 | 102,891 |
| Packed preparation and rows, independent confirmation | 2,027.72 | 2,165.52 | 0.936 / 0.993 | 102,891 |

Each confirmation has twelve actuals and both native brackets. The original has
ten filtered results; the candidate retains all twelve. Native clocks differ,
and candidate variability is retained. There is no favorable repeat. The earlier
source-row-only short (2,195.05 µs) and completed packed short (2,115.86 µs) remain
development evidence, not independent passing proof.

Every source row now computes exact Int32 RGB×alpha and alpha once while scratch
is available. One shared weighting loop consumes these products or raw input.
Original at-most-three double weights per column move left without overwriting
future unread columns. Their bits and scalar replay order remain unchanged.
Two filtered rows occupy contiguous storage, replacing four scattered stores and
the corresponding reconstruction on every vector read.

All storage uses existing admitted capacity: weights take 3×width doubles,
filtered rows 4×width doubles, leaving width doubles for source products. The
frozen control has 8,216 spare bytes for its 8,208-byte source row. Smaller
workspace may borrow unpublished output-tail capacity only until its first row;
later preparation uses raw input. Tiny and nonfactorable-width execution retain
their portable or existing arithmetic paths. No owner, allocation, pool, session,
numerical policy or cross-operation retention is added.

Horizontal workspace admission remains 102,892 bytes; independently owned output
is 2,132,052 bytes. Warm default output backing uses the existing two-slot/4 MiB
pool; caller-governed output bypasses it. Temporary workspace is released before
return. Total allocations above, admitted capacity, idle pool backing and retained
output are distinct. Physical/native peaks and admitted highwater are not newly
measured, and no allocation saving is claimed.

Managed timing includes public Resize, geometry preparation, weight packing,
source preparation, filtering, exact-result repair and independent output/disposal.
The reference includes conversion, filtering, writable output acquisition,
zero-alpha normalization and release. Its public FFmpeg graph, geometry and
wrappers remain retained. This does not establish equivalent one-shot graph setup.
Managed rational pixels remain exact; native float retains the existing scoped
RGB/alpha bounds. No codec packets or metadata policy change.

Focused normal/scalar/AVX-512-disabled checks pass 240 internal and 729 public
tests per mode on each OS. Exact products, guards, tail retirement, tiny results,
refusal/cancellation cleanup and independent survivors are covered. Full/live
checks pass **6,036 per OS**. The same actual private Release 0.2.1 package passes
both consumers and all three usage examples; its DLL is the measured artifact.
Unchanged production paths reuse qualification. An initial editor assertion
failed before writing; subsequent unchanged checks are preserved separately and
are not counted as packed-candidate proof.

[Evidence](bilinear-current-boundary-evidence.json) pins producers, all actuals,
storage and qualification. Mirrored/member-verified raw
`bilinear-packed-row-storage-qualified.tar.xz` has 7,826,156 bytes / 182 members,
SHA256 `609a6c38bdca563b234cab251d7767023308b41212ee2e2eedbfab30569f19fc`.
It supersedes [the original gap confirmation](BILINEAR-CURRENT-CONFIDENCE.md) for
current costs; [attribution](BILINEAR-CURRENT-EXECUTION-ATTRIBUTION.md) retains the
design lead. Stop this passing control. Other codec/resize work and the full
283-path objective remain open; publication is postponed.
