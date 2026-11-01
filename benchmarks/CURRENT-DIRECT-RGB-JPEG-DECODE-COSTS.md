# Current direct-RGB JPEG decoding costs

Historical costs: affected JPEG boundaries are superseded by the
[current retained-result round](WARM-JPEG-DIRECT-RGB-RESULT.md).
Original observations and raw evidence remain below.

The three existing 513×259 direct-RGB controls now have current independent
single-CPU warm evidence. All remain demonstrated gaps; these are current costs,
without paired speedup claims or broader JPEG parity claims.

| Complete resident-packet decode | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Encoded input B |
|---|---:|---:|---:|---:|---:|
| DirectRgb444 | 2,098.407 | 1,447.691 | 1.449 / 1.458 | 966,367 | 133,057 |
| DirectRgb422 | 1,435.807 | 1,095.788 | 1.310 / 1.324 | 829,164 | 71,184 |
| DirectRgb420 | 1,020.305 | 743.921 | 1.372 / 1.387 | 757,363 | 52,109 |

Original marker-only packets, entropy/quantization, sampling and settings remain
frozen. Every operation decodes all pixels, normalizes/packs and acquires/releases
an independently owned output. The native 4:2:2 phase correction and 4:2:0 RGB
component interpretation remain timed; both negative controls still distinguish
incorrect interpretation. Native allocation is unmeasured; managed B/op includes
harness costs. Startup/IPC/fixture generation and checks are excluded.

All current pixels equal independently extracted normal native component planes
with exact rational centered interpolation. Normal/scalar checks pass both OSes.
Three compiled benchmark hashes still described the former floating inverse
policy; only those managed expectations are refreshed to the already frozen
native/rational identities. No reference input/settings or pixel gate changes.
The 4:2:2 audit recovers immutable even-position samples from the full-resolution
GBRP view, exactly the existing Center422 oracle, then applies rational quarters.
The 4:2:0 audit applies the existing sixteenth-weight policy to native planes.

One harness build per OS; zero production/test/probe/native/selector builds.
The same immutable production DLL, cached selectors, worker and fixtures serve
three sequential twelve-actual jobs with both native brackets and idle-v2 guards.
Windows uses its existing ImageProbe for exact packet/pixel checks because the
warm native workers require Linux. Initial setup corrections and failed probes
are retained without successful-score repetitions. Unchanged 5,342 checks/OS,
same actual Release-package/three-example proof and all three green CI jobs carry
forward. No new image, settings grid, profile or repeated full/package checks.

Earlier costs predate fuel/ownership cleanup, integer inverse/color, baseline
sample planes and ordinary DC/AC fusion. Stop retiming these confirmed gaps
without a distinct change; the full 268-path objective remains open.

[Evidence](current-direct-rgb-jpeg-decode-costs-evidence.json) records all samples,
allocations, input/pixel hashes, boundaries and qualification reuse. Raw archive
`current-direct-rgb-jpeg-decode-costs.tar.xz` is mirrored/member-verified:
4,168,788 B / 119 members, SHA256
`f01da8c6740bdf02b616e0be28bfa87415e23a2be222aabad2a66bfb17fe2142`.
The retained J4N31 archive supplies unchanged producer/package provenance.
