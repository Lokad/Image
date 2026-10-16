# Current bicubic evidence reconciliation

Five of the existing 61 named bicubic cases have valid current-path evidence.
The current horizontal RGBA16 enlargement and single-pixel RGBA8 reduction remain
stopped. Three more confirmations are reused without new timings:

| Existing case, input+output+4 KiB limit | Ratio / upper | Verification |
| --- | ---: | --- |
| TightHorizontalUp8, 1027x259 | 1.583 / 1.591 | Reused independent twelve |
| TightUp8, 1027x519 | 1.783 / 1.795 | Reused independent twelve |
| TightUp16, 1027x519 | 1.773 / 1.783 | Reused independent twelve |

The original two 513x259 rasters, exact managed pixels, approved native bounds,
complete packing/filtering/output ownership, all twelve actuals and both quiet
guards are preserved. Allocation figures and unchanged timing details remain in
the [original enlargement report](WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md) and the
[benchmark summary](../docs/benchmark.md). No new image or workload grid.

Actual archived source inventories establish that 35 resize, helper, buffer,
limit, geometry and build inputs match current production. The only changed
shared-context source adds `CommitAdmittedWork`, which resizing never calls;
all executed context methods and fields are unchanged. Ordinary Resize allocates
zero-initialized output and does not use the changed overwrite-storage pool.
The captured byte hashes establish the measured code even where the recorded
parent revision precedes its uncommitted optimization.

The three pinned native worker binaries, identities and scaler archive remain
byte-identical on the VM. The shared adapter source later added an unselected
compile-time single-pixel mode, an equal vertical-halo constant alias for this
horizontal case, and untimed ready metadata. Removing those scoped differences
yields identical remaining adapter source; complete timed work is unchanged.
The reference remains labeled zimg, FFmpeg's zscale backend. No baseline or
pixel/quality adjustment is introduced.

[Reuse evidence](warm-current-bicubic-evidence.json) pins original observations,
archives, receipts, relevant source hashes, native identities and current
qualification. The existing 3,483 Release tests per OS, 898 live comparisons and
same actual package consumer/all three examples remain applicable. Zero
production/harness/native builds, statistical jobs or repeated qualification.

B12 remains open. Next are ordinary Mixed16/Mixed8: the later vertical-first
helpers precede the old row cache and change their selected algorithm. Resolve
that uncertainty before assuming a performance gap. Reconcile the other named
cases individually, reusing unchanged paths. Identification boundaries and the
full required warm objective remain open.
