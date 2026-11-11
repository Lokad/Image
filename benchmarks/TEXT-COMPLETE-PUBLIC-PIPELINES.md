# Complete public text pipelines

Predecessor evidence: public canvas creation has since changed. Current affected
costs and qualification are in [cleared canvas storage](CLEARED-CANVAS-STORAGE.md).

The existing ConstrainedFit control measures two complete public workloads on
the unchanged actual 0.2.1 package DLL. Both pass the warm single-CPU target.
The reference is **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12**,
a complete public-API integration, not FFmpeg or a bare shaping engine.
No production speedup is claimed from this measurement-only round.

| Warm public operation, fixed RGBA16 control | Lokad µs | ImageSharp pipeline µs | Ratio / upper | GC B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: |
| One-shot font→fit→layout→draw→dispose | 1,470.18 | 6,791.52 | 0.216 / 0.261 | 774,915 / 9,256,953 |
| Prepare + eight fresh-canvas draws + disposal | 2,720.53 | 7,089.27 | 0.384 / 0.452 | 4,217,697 / 9,494,633 |
| Preparation creation only (diagnostic) | 1,174.29 | 6,715.67 | 0.175 / 0.192 | 283,282 / 9,227,176 |
| One fresh-canvas draw after preparation (diagnostic) | 209.76 | 79.97 | 2.623 / 3.122 | 491,713 / 33,433 |

The measured eight-draw batch amortizes to **340.07/886.16 µs per draw**, including
preparation and all disposal. This is total batch time divided by eight. It is
not a sum of historic stages, nor a claim for arbitrary numbers of draws. Existing
loading and drawing stages remain scoped diagnostics; they do not establish an
overall text-processing slowdown. The fresh-canvas steady draw remains a scoped
gap, conservative1.760–3.122; neither complete-workload pass closes that boundary.

Scope is one frozen DejaVu Sans Regular2.37 face, **384×160 RGBA16**, `AV To AV To`,
OpenType/kerning, wrap96/height30, maximum24/minimum8/step.25. Both select16.75
after30fitting trials. Drawing uses origin8/8 and RGB20/40/60. Font bytes/options
are borrowed inputs. Every draw creates, explicitly clears, returns ownership of
and disposes a fresh complete canvas. All font/layout/raster/owner setup and
disposal belong inside the appropriate complete boundary.

One-shot uses ordinary drawing. Repeated use prepares owned geometry once and
releases font/layout before drawing. The reference winner uses four public calls
(RenderText, TryMeasureCharacterAdvances, CountLines, MeasureAdvance); fitting
includes all three public calls per trial and complete winner reconstruction.
Its metrics come from its own public font API, without a Lokad font parse in the
reference timer. Configuration/options are supplied reusable inputs on both sides.
Preparation creation is measured separately, with prepared-owner disposal outside
those diagnostic clocks; complete batches include all cleanup. Steady diagnostics
retain each side's actual preparation and include fresh-canvas ownership costs.

Parser validation/eagerness differs. Managed loading validates ranges/mapping/glyph
headers, not all outlines/checksums. FontCollection/FontFamily provide no Dispose:
the adapter drops its owning references, and reclamation remains GC-driven. The
managed owner releases admission promptly. Managed preparation owns raster working
storage; the reference retains positioned/flattened paths and allocates raster
working state per draw. These distinctions are disclosed, not parser equivalence.

GC allocations above are totals, not peaks or retention. The managed prepared
owner charges **39,904 B** until disposal, with no font/layout owner retained.
Untimed warmed-process full-GC heap deltas are approximately **33,920/22,504 B**;
raw before/live/after values remain in evidence. These exclude unchanged pools in
the baseline, are approximate, and are not reference admission counters. Existing
managed idle font capacity757,076 B is separate. Managed governed peak807,708 B
includes preparation overlap; physical/native/JIT peaks remain unmeasured.

All full16-bit samples reproduce their original subject-specific frames. Managed
glyphs/clusters/lines/positions/bounds/fitting remain exact. Reference selection,
clusters and lines remain exact, positions/bounds within1e-4pixel of the frozen
reference. Approved visual alpha gates remain normalized0..255: mean<=8,RMSE<=16,
coverage<=2%,edges<=1pixel. Current mean4.010/RMSE6.920,coverage1.542%,edges0.
Independent outputs survive sibling/preparation/font/layout disposal; borrowed
input/root and all admitted owners clean up correctly. This preserves the wider
supplied-face/Arabic correctness evidence without adding a font/script/grid.

Three-pair screens precede one independent twelve-pair confirmation on the same
immutable tool/core/reference identities. Every valid pair and before/after bracket
is retained, with alternating order,32complete operations/sample,CPU0, tiering off,
nonconcurrent workstation GC and idle-v2 guards. Conservative confidence includes
all actuals and paired ratios. An initial aborted screen rejected an uninformative
interval before persisting pairs; it supplies no usable numerical claim. That
failure/module/source is retained. Corrected reporting persists raw actuals first
and records null-upper uncertainty; no successful favorable repeat follows.

The actual package,19font/layout/text and additional ownership source pins are
unchanged. Reuse6,264Windows/Linux full/live checks, affectedISA and SAME-package/
all three examples. Only focused new harness/ownership/full-word checks run on
both OSes: zero production/embedded/native builds, broad suites or package repeats.
All289required material paths remain; these bounded complete passes do not establish
whole-library or general-font parity. Continue independent material codec/resize
work. Before any further text-kernel tuning, attribute fresh-canvas allocation/
initialization versus raster costs: current GC491,713/33,433B per steady draw is
an allocation observation, not CPU attribution or a proven optimization gain.

[Evidence](text-complete-public-pipelines-evidence.json) pins scope, complete boundaries,
allocations/retention, gates, identities and current qualification reuse. Mirrored/
member-verified raw `text-complete-public-pipelines.tar.xz`:6,567,012B/110members,
SHA256 `174875e70b16d363625b6457e4fe354b44f49701eaca26544af1a48b7f9a0dec`. Its dependency pins retain original supplied-font
compatibility and the actual package. Package `a891e89b2246bfcf5637614a3e9ff0d35467aef048cc054ca75d74ddc875608f`,
DLL `cdd859a8c07473dc25858d28ce4ad887c31d9ab7aa91f9c1b36e92f6d2184f92`. Publication remains postponed.
