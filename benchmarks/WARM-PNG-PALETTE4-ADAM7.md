# Packed palette4 Adam7 decoding

The unchanged production decoder independently confirms **1.304× FFmpeg /
upper 1.387×**, at **2,592 managed B/op**, for one original packed palette4
Adam7 packet with full transparency.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Short screen | 1,418.73 | 1,135.39 | 1.250 / 1.493 | 2,592 |
| Independent confirmation | 1,480.35 | 1,135.35 | 1.304 / 1.387 | 2,592 |

This reuses the original 513×259 byte raster and its existing sixteen-entry
palette projection: j=red>>4, i=17*j, RGBA=(i,255-i,(73*i+19)%256,i).
The deterministic PNG contains sixteen PLTE/tRNS entries, seven canonical
Adam7 passes, filter None and BCL Optimal level6 compression. Four-bit indices
are MSB-first; unused pass-row low bits are zero. Header/chunks, inflated pass
samples/padding and frozen packet bytes are checked outside timing. No new
original raster, depth/filter/settings grid or tolerance adjustment is added.

Managed, normal native and scalar FFmpeg CLI RGBA match every sample exactly,
including alpha and nonzero hidden RGB at index0. Validation checks simultaneous
independent owners and unchanged source. Complete public decode includes PNG
framing/CRC, inflation, unfiltering, packed-index extraction, palette expansion,
Adam7 placement, owned output acquisition and release. Native palette expansion
and conversion are timed. Codec/frame/packet/conversion wrappers and admitted
scratch are reusable, while every operation decodes/materializes all pixels.
Startup, IPC, I/O, fixture creation and validation are excluded.

Both sequential Release runs pass the versioned pre/post quiet guards on CPU0,
one codec/conversion thread, disabled tiering and workstation nonconcurrent GC.
All twelve confirmation actuals are selected and retained. The conservative
upper includes selected/all-actual intervals against the faster native bracket's
lower bound. Process CPU/wall telemetry is retained. Allocation figures include
harness overhead; native total allocations are unmeasured.

Only one harness and one native-worker build are required. The native change
admits Adam7 palette4 in the existing guard; its timed algorithm is unchanged.
The same verified immutable core/harness/native binaries serve both runs.
Existing 3,483-tests-per-OS/live/same-actual-package qualification is reused:
production code is unchanged, so no production rebuild, profile or repeated
suite/package proof is needed.

[Numeric evidence](warm-png-palette4-adam7-evidence.json) retains every actual,
allocation, guard, source/build/packet identity and raw archive hash. Stop this
fixed pair; remaining material/current-source coverage stays open.
