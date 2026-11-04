# Complete one-shot color PNG comparisons

Four existing 513×259 Optimal6 controls now include equivalent one-shot setup.
No production change or new workload. One shared adapter preserves the original
conversion/packing algorithms while moving their working state inside timing.

| Complete encode | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Exact packet B, both | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| RGB8 | 10,026.74 | 9,878.14 | 1.015 / 1.023 | 819,876 | 168,729 | gap |
| Grayscale8 | 2,255.69 | 1,964.05 | 1.148 / 1.185 | 551,356 | 72,586 | gap |
| Gray-alpha8 | 4,578.16 | 4,428.73 | 1.034 / 1.050 | 552,324 | 120,366 | gap |
| Grayscale16 / red-plane graph | 5,215.54 | 5,407.47 | 0.965 / 0.987 | 822,732 | 265,433 | pass |

One twelve-actual job per control, four complete operations per actual; all
actuals/brackets and filtered/all-actual confidence are retained. Gray16 passes
this current scoped boundary and stops; three gaps remain. These replace affected
historical costs. **No incremental CPU or allocation gain follows from changing
production identity, clocks and reference preparation boundaries.**

Both subjects borrow the frozen resident RGBA source and own independent complete
packets. Fresh managed MemoryStream/public EncodePng/growth/disposal stays timed.
Native fresh codec/options/zlib, packet wrapper, working frame and disposal are
included. RGB8/gray8 retain identical single-thread SWS conversion/options;
gray-alpha8 retains direct red/alpha packing without SWS; gray16 retains its
single-thread red-plane extraction/endian filter graph, now constructed/disposed
per operation. Necessary sample conversion remains timed. This gray16 result
does not compare every possible FFmpeg conversion implementation.

All original inputs/settings, complete packet identities, samples/alpha/hidden
RGB, normal/scalar CLI roundtrips, source, independent outputs, caller-stream and
quiet CPU0 checks pass. B/op is total managed allocation, separate from live/
retained capacity. Physical/admitted peaks and native allocations are unmeasured.

One isolated harness build, one smoke driver and two native builds serve all four
cases; no production/test/package/selector or measurement builds. The initial
worker reports live graph presence before one-shot construction, stopping the
gray16 smoke before timing. The promoted status fields report the configured,
runtime-validated extraction/threading policy, with unchanged encode work. All
four final smokes pass. Duplicate initial/final run IDs are disambiguated by
worker hash during receipt extraction, without repeating completed execution.
No statistical job is repeated.

Unchanged production reuses **5,634 full/live checks per OS**, affected scalar/
AVX512-off and the same actual Release-package/all-three-example verification in
[the qualified checkpoint](VP8-FULL-BLOCK-QUANTIZATION.md). Publication is postponed.
Reproduce using the retained `pgc297-*.py` scripts, one shared tracked build script,
unchanged confirmation selector and frozen receipts; use the dedicated quiet
single-CPU VM. [Evidence](png-color-one-shot-evidence.json) pins every identity,
gate, boundary and qualification dependency.

Mirrored/member-verified `png-color-one-shot-current.tar.xz`: 3,547,852 B / 275 members,
SHA256 `3ab81c3d4a0ced0acc25af7f5d7415c8b836b96d748cbb90579ed3379a3e879e`. Both worker versions,
harness/source, original/final smokes, failed logs and all actuals/brackets/guards
are retained, with unchanged production/package/framework dependencies.
