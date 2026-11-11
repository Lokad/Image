# Canvas ownership and prepared text attribution

Predecessor diagnostic. The resulting [cleared canvas stage](CLEARED-CANVAS-STORAGE.md)
records current affected costs, retention and qualification; original evidence follows.

TCA811 follows the [complete public pipeline comparison](TEXT-COMPLETE-PUBLIC-PIPELINES.md)
on the same actual package, font, text,384×160RGBA16 canvas and exact full-word
frames. It changes no production code or library numerical policy.

| Complete diagnostic | Lokad µs | ImageSharp µs | Ratio / upper | GC B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: |
| Fresh canvas creation + explicit clear + disposal | 80.66 | 5.66 | 14.249 / 14.757 | 491,657 / 1,049 |
| Existing canvas clear + prepared draw | 95.42 | 72.27 | 1.320 / 1.546 | 57 / 32,433 |

The existing-canvas comparison retains actual preparation and one matching canvas
on both sides, including every clear and draw in timing. Each canvas has491,520 B
of pixel payload, separately from the managed39,904 B prepared owner and reference
paths/working storage. Canvas-only includes output ownership/disposal and no text
drawing. Reference is the explicitly labeled ImageSharp3.1.12 allocator with
Drawing2.1.7/Fonts2.1.3 prepared paths, not FFmpeg.

These separately measured boundaries are **not additive phase costs**. Subtracting
them from historical or complete pipeline timings would not establish a removable
CPU percentage. Different allocation/GC and working-state lifetimes remain visible.
Existing-canvas clear/draw still has a scoped gap, range1.039–1.546. The complete
one-shot and eight-draw pipeline passes remain separate and are not retimed.

Source explains the allocation lead: ImageBuffer.Create reaches Allocate's
`new byte[length]`; disposal drops that storage. Allocate also serves nearest
resizing and the CMYK decoder. The existing shared pool holds at most two exact-size
idle arrays within4MiB, with distinct font/image interleaving policies. A potential
cleared-rental design must preserve zero-before-publication, prompt failure and
cancellation cleanup, admission before allocation, independent owners and bounded
retention. Preserve cheap CLR-zero behavior on a cold miss. Governed storage must
still bypass idle pools, without refusal retries/wait/fallback. Do not enlarge
retention for this fixture or change other allocation policies incidentally.

The next bounded design should establish which public/internal allocation paths
change, exercise zeroed publication after dirty reuse, surviving siblings and
governed refusal/cancellation recovery, then measure the affected complete public
operations on an immutable build. This attribution supplies a lead, not an
optimization gain or merge authorization. It does not justify text-kernel tuning
before addressing canvas ownership costs, and does not block codec/resizing work.

One read-only run per diagnostic retains all12alternating adjacent pairs,
128operations/sample,12reference batches before/after, conservative paired/all-
actual confidence, CPU0/tieringoff/nonconcurrentGC and idle-v2 guards. Exact frozen
managed/reference full-word frames, existing fitting/layout/visual and owner
checks pass on Windows/Linux. One tool build; zero production/embedded/native
builds, broad suites or package repeats. Current actual package and6,264W/L/
same-package/all three example proof are reused. All289material paths remain.
This diagnostic supplies no displayed main-summary cost and adds no renderer
dependency. Full16-bit samples and the approved visual gates are unchanged.

[Evidence](text-canvas-attribution-evidence.json) pins methods, owners, source and
raw actuals. Mirrored/member-verified `text-canvas-attribution.tar.xz`:75,196B/
31members, SHA256 `691530cf0c95bd9505e5f06ed467b853a01245827e0c542b692b7e76bcb28e2f`. Its dependency
retains shared DLLs, original font/license/raster/adapters and actual-package proof.
Publication remains postponed.
