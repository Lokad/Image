# Mixed word bicubic scheduling

Mixed-axis RGBA16 filtering shares horizontal taps across four output rows and
reuses the existing quartet certificate. Both pair quotients are prepared before
certification. Every pixel keeps its original multiply/add order, guards, rounding
and exact repair. Pair/single tails remain; no new certificate, owner or cache.

Independent complete operations save **15.20 µs /1.34% raw** with default workspace
and **22.87 µs /2.00% raw** with the bounded workspace. Both raw confidence views
separate. Bounded reference-normalized confidence separates too (2.15% point);
default normalized intervals overlap (1.04% point), so that gain is uncertain.
Native clocks move −0.30%/+0.15%. These are modest incremental gains, not parity.

| Existing 513×259 RGBA16 source / zimg | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Mixed16 →1027×129, default | 1,121.96 | 849.86 | 1.320 /1.331 | 114 | gap |
| TightMixed16 →1027×129, input+output+4 KiB | 1,118.45 | 846.38 | 1.321 /1.327 | 114 | gap |
| TightVertical16 →513×129, input+output+4 KiB | 718.57 | 588.98 | 1.220 /1.223 | 113 | gap |

Contemporary originals are1,137.15/1,141.32 µs for default/bounded mixed operations.
The old718.36 µs vertical result is historical, not a paired gain/regression
baseline. Only these three named controls execute the changed schedule; the other
twelve [word costs](BICUBIC-WORD-PUBLICATION-PIPELINE.md) remain source-valid.
The merged fifteen controls retain one pass, thirteen gaps and one uncertain case.
One displayed row refreshes;124 displayed comparisons reuse unchanged paths.

Two short screens precede four independent original/current confirmations and one
affected vertical confirmation. Every confirmation keeps twelve actuals, both
reference brackets, full frozen/rational managed pixels, native-only bounds,
unchanged input, independently owned output and idle-v2 guards. Filtered and all
actuals remain, including the default candidate's outlier. No favorable repeats,
additional timing grid or broader shape/content performance claim.

Managed timing includes complete public Resize, preparation/admission, filtering,
owned output and disposal. The explicitly labeled reference is zimg3.0.6,
FFmpeg zscale's backend, retaining graph/input/scratch outside timing. Conversion,
halo/filtering, quantization, independent output and release remain timed. This is
the established repeated-reference boundary, not one-shot setup parity. Native
float intermediates do not relax exact rational/qualified managed pixels.

The existing3,992-byte fixed arrays and4KiB admission are unchanged. Packed staging
uses32 rather than16 bytes within1,536 bytes of retired decode storage. No heap
buffer, retained state or output capacity is added. Mixed results still own
1,059,864 B; source+output+workspace bound stays2,126,896 B. GC stays114 B/op on
both mixed controls and113 on tight vertical. Logical bounds and direct-kernel
admission peak4KiB are separate from unmeasured physical stack, register spills,
process/native peaks and required live output storage.

Exact source undo removes only the new four-row loop. Geometry, cache/decode,
vertical filtering, two/single-row tails, repair, dispatch and every cancellation
checkpoint reconstruct unchanged. The caller holds an operation-private result
until success; failure disposes it. Refused pairs repair in original row order.
Shared admission refusal remains terminal, with no retry or lower-memory recovery.

Three added direct-kernel checks cover four/two/one-row tails, band boundaries,
tiny-alpha unwind, refusal, cancellation cleanup and recovery. The existing1,008
pair controls in both quartet positions retain their frozen decisions/canaries.
Each OS passes **5,862 full/live checks** (2,597 internal /2,317 public /948 live),
plus98 internal /703 public checks in each normal/scalar mode. Other ISA evidence
reuses unchanged paths; there is no vector policy or numerical change.

The SAME actual private0.2.1 Release package/all three examples passes both OSes:
SHA256 `8a46b970ad452a023572c2ac7791bce28ac061c67082e4954053b982af0e0feb`,
427,006 B. Its consumer checks frozen mixed pixels, aggregate admission,
cancellation/recovery and independent surviving outputs. One immutable core and
embedded build per OS; no public-test, benchmark, native, selector or fixture
build. Published releases remain immutable; publication and the283-path goal stay open.

The preceding current profile captures1,997 leaves,1,845 in the mixed driver,
21 in exact repair and14 in geometry preparation. Its elapsed is discarded;
self-samples include inlined work and are not phase timings or stall proof. An
initial symlinked launcher resolves an old DLL and fails the identity gate before
capture. Isolated hardlinks correct that binding; only one valid capture runs.

[Evidence](bicubic-mixed-word-scheduling-evidence.json) retains every measurement,
source/ownership proof, profile and qualification. Mirrored/member-verified raw
`bicubic-mixed-word-scheduling-qualified.tar.xz`:10,700,996 B /240 members,
SHA256 `a4ccddcbb560b33bd9bc41a8324e750c77b29a94f45062071eeb7f7fea790b07`.
Earlier geometry/publication experiments remain separately linked history.
