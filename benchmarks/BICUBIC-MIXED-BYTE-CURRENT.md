# Current tight mixed-byte bicubic cost

One current confirmation replaces the historical TightMixed8 point. The original
513×259 RGBA8 input becomes 1027×129 with input+output+4 KiB working allowance.
The actual immutable package DLL is already qualified on Windows/Linux.

| Complete operation | Lokad µs | zimg µs | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| TightMixed8 | 1,319.86 | 860.30 | 1.534 / 1.545 | 114 |

This is a demonstrated gap: conservative lower 1.507, approximately
459.57 µs above reference. The older 1,167.23 µs point
is retained as history, without a causal gain/regression claim. Twelve actuals,
both native brackets and idle-v2 guards pass. No favorable repeats, production
changes, builds, new inputs, profiles, tests or package runs.

Complete public admission, preparation, filtering, exact-result repair,
independent output and disposal remain timed. The labeled reference is zimg,
FFmpeg zscale's backend, with graph/input/scratch prepared outside timing;
packing, four-pixel clamped halo, filtering, output acquisition/repacking/release
remain timed. This is not FFmpeg CLI or equivalent one-shot setup parity.

Managed rational pixels remain exact, including zero-alpha RGB. Existing native
RGB/alpha maximum1/RMSE0.25 bounds remain unchanged. Source preservation, normal/
scalar checks and independent owners pass. Output retains529,932 B until disposal;
the local ceiling is1,065,496 B. Physical/native peaks are not measured.

Actual package SHA-256`307d8866270fb021c6307a372f71b69675a058c37f09711b202d000a02235e44`;
DLL`56acf730ef4cd87e644237e5128004c925e3a4736d73348daac8f4526a923477`. Reuse its **5,955 full/live checks perOS**, affected
scalar/ISA and same-package/three-example proof from
[frame quantizers](VP8-FRAME-QUANTIZER-PREPARATION.md). Qualification inputs are
unchanged. This scoped current result does not refresh all release comparisons.

Raw`bicubic-mixed-byte-current-package.tar.xz`, SHA-256`814aa535ff8796f93393901afd204cf816c8cb420876c5b925ff2682171fa651`,
6,968,600 B/121 members, mirrored/member-verified. It retains the
actual package, pinned runtimes/workers, all actuals/native brackets, guards,
target source snapshots and mxc632 prepare/measure/observe scripts.
[Structured evidence](bicubic-mixed-byte-current-evidence.json).

The next source audit targets this larger current gap. Eight-row bands, paired
512-bit column decoding, result-backed geometry and deferred exact tiny-alpha
repair already exist. Attribute repeated scheduling, certification/fallback and
repair work before selecting a coherent redesign; allocation counts alone are
not CPU attribution. Preserve exact pixels, certified rounding, cancellation,
shared admission and the retirement of scratch before tiny-alpha replay.
Publication remains postponed; the complete283-path objective stays open.
