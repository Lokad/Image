# Current owned-edit confidence

Only the four open 513×259 owned-edit controls receive a longer integration.
No production change or historical-clock speedup is claimed.

| Complete operation / FFmpeg | Lokad / reference µs | Conservative ratio interval | Status | Managed allocated B/op |
| --- | ---: | ---: | --- | ---: |
| NormalOrientation16 | 17.201 / 16.561 | 1.033051–1.042814 | gap | 56 |
| VerticalFlip16 | 17.417 / 17.424 | 0.991211–1.005451 | uncertain | 56 |
| Clone16 | 17.224 / 17.081 | 0.988814–1.026794 | uncertain | 56 |
| VerticalFlip8 | 8.614 / 8.507 | 1.000615–1.022566 | gap | 56 |

One prespecified capture per control increases 256 calls (~2–5 ms) to 4096
(~35–71 ms), retaining twelve alternating pairs, every native bracket sample,
the conservative confidence rule and natural GC. Original/adverse samples remain.
Normal RGBA16 copying and RGBA8 vertical flipping are demonstrated gaps; word
clone and vertical flip remain uncertain. No identical favorable retries.

Complete public operation, admission, independent result acquisition,
materialization and disposal remain timed. FFmpeg retains graph/source/frame
wrappers; null/vflip views are materialized by av_frame_get_buffer/av_frame_copy
inside each operation. Native padded rows are exposed directly without another
packing copy. Exact channel bytes, alpha/hidden RGB, unchanged source, independent
live outputs, normal/scalar native checks and idle-v2 all pass. CPU0, Release,
optimized JIT and nonconcurrent workstation GC; original inputs/settings retained.

The normal-copy and clone harnesses use the **same native worker, setup, source
bytes, geometry and owned-copy path**. Managed normal orientation routes to Clone.
Native costs still differ (16.561/17.081 µs); no boundary discrepancy or cause is
established. Do not attribute this to alignment, allocation or clocks without
evidence, or infer a production change from separate historical measurements.

The existing driver's argument guard/error text alone changes 256→4096; portable
PDB checks identify its actual source, and reversing the patch reconstructs it
byte-exactly. One tool-only build; **zero production, embedded-test, harness,
native or fixture builds, broad checks or package-verification repeats**.
Reuse [fixed-position decoding](VP8-FIXED-CODE.md): **6,692 full/live checks per OS**,
normal/scalar/ISA evidence and the SAME actual package/all three examples on both
platforms. Timed DLL `6d2c0833c8a9a022832925e5fa793af60f2c0ba7843e02cdffb9bdaa35e0a01b`;
actual package `b4853b840376c5291973dc23f1b885d3b4e702c9020f4ca8265e0709d1637d33`.

All eighteen basic owned edits still allocate **56 B/op**. Active result payload
is 531,468 B (RGBA8) or 1,062,936 B (RGBA16); caller input is borrowed. Default
idle pixel storage and caller-root bypass are unchanged. These capacities are
separate from allocations and unmeasured physical/native/JIT peaks; no storage
saving follows from a confidence update.

The twenty owned-edit/composition views remain **16 pass / two gaps / two uncertain**;
191 image views remain **185 pass / two gaps / four uncertain**. Stop confirmed
passes. Next: test row-view preparation against the confirmed byte vertical-flip
gap, preserving word behavior and all live cancellation/admission boundaries.
All 289 required paths remain in scope; publication stays postponed.

[Evidence](owned-edit-current-confidence-evidence.json) pins identities, samples,
boundary audit and qualification reuse. Raw `owned-edit-long-integration.tar.xz`, 77,832 B /
60 members, SHA256 `ed303cbd46ea756b5a43a9e2935146736bd080b25aed38322f205c00b74215e4`, is mirrored/member-verified.
Dependencies retain prior observations, actual package and unchanged tools.
