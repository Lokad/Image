# Shared byte cross-resize geometry

Bounded byte reverse-mixed and identity-height resizing now reuse the existing
vertical geometry view in unpublished output-row tails. Prefix strips read the
prepared weights; tail strips rebuild after retiring that storage. The entire
previous Cross byte file reconstructs after removing this routing. Original
decoding, filtering, caches, certificates, repair and cancellation unwind remain.
The shared geometry helper and all other production sources are unchanged.

| Complete public Resize, extra4KiB | Before / current µs | Reference µs | Current ratio / upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8,513×259→256×519 | 1,212.50 / 1,063.49 | 841.66 | 1.264 / 1.270 | 114 |
| RGBA8,513×259→256×259 | 805.37 / 690.55 | 576.22 | 1.198 / 1.212 | 113 |

Independent confirmations save **149.01µs/12.29%raw/12.83%normalized** and
**114.82µs/14.26%raw/14.79%normalized**, respectively. Filtered/all-actual raw
and normalized intervals separate. Native clocks move+0.614%/+0.627%; raw and
normalized views stay distinct. Both controls remain gaps, not parity passes.
Original three-actual screens and all twelve confirmation actuals are retained.

Every managed operation includes preparation, admission, full filtering/repair,
independent output and disposal. The explicitly labeled zimg scaler backend keeps
graph and scratch prepared; this is not a fresh-native-graph comparison. Frozen
inputs/settings, exact managed hashes, original native-only bounds, source
immutability, output ownership and idle-v2 guards pass. No new fixture or grid.

The view borrows56B per output row from already charged, independently owned
storage. At width256, thirty prefix strips reuse geometry and two tails rebuild
it; logical construction drops32H→3H, not a CPU forecast. No additional owned
or retained capacity, explicit stack array or cache. Existing arrays occupy3440B
within4096B admission; the physical JIT frame is unmeasured. Preparation follows
reservation, views never escape, and the old NoInlining tiny-alpha unwind remains.

Seven focused tests cover preparation thresholds/odd tails, poisoned outputs,
zero/low alpha against the independent rational oracle, terminal refusal,
cancellation cleanup and recovery. Windows/Linux each pass **6,463 full/live
checks**,535internal/791public normal/scalar, including all three storage suites.
Disabled-AVX512/AVX2 evidence is reused: the unchanged guard rejects before new
code, and fallback sources are unchanged. The same actual package/all three
examples pass bothOSes. One core/embedded build eachOS, no public/native/harness/
fixture rebuild, onepack. Other71 resize costs and unaffected codec/text proof
remain source-valid; this is not a release-wide refresh.

[Evidence](byte-cross-shared-geometry-evidence.json) records bounds, source
reconstruction, both comparisons and qualification. Raw
`byte-cross-shared-geometry-qualified.tar.xz`, SHA256
`1d9f5c6a0fe7d42d293ab3c8655994e14c4853539c24675e6d0a736090fd18ca`
(904,908B/104members), is mirrored/member-verified and depends on the qualified
[filter-weight round](SHARED-RESIZE-FILTER-WEIGHTS.md). The454,764B development
package0.2.1 has SHA256`b4fdfa6cc6f418c24e7b755ea7ff742a2a7e856a96799e7e9e3c93ffc18ca82f`.
Published releases are unchanged; publication is postponed.

Next establish current-path attribution for bounded byte2×9, the largest remaining
resize gap:1,338.67/zimg1,049.55µs. Current cross/identity gaps221.83/114.33µs
remain. Keep all289required paths and uncertainties open; stop passing cases.
