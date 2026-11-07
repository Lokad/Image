# Progressive JPEG output ownership

JPO468, 2026-11-07. Accepted; publication postponed.
[Evidence](jpeg-progressive-output-evidence.json) owns the current five progressive
decoder controls and the unchanged coding-validation comparison. Full parity across
283 required paths remains open.

The full-resolution, three-component progressive renderer now uses the existing
complete-overwrite allocator. Its first sample plane still retires its coefficients
before result allocation. Second/third strips are initialized before consumption
in disjoint output-tail bytes; tail collisions use initialized stack blocks. RGB
and YCbCr kernels write every visible channel, including opaque alpha. Numeric
validation of padded blocks, orientation, cancellation and cleanup are unchanged.

Default calls reuse the existing pixel pool after actual result disposal. Governed
calls reserve the retained owner before fresh uninitialized storage and bypass the
pool. No new pool, owner, session, cache, input copy or numerical tolerance is added.
Other JPEG renderers and caller-owned sample decoding retain their distinct paths.

| Complete 513×259 progressive444 decode | Original → current µs | FFmpeg µs | Current ratio / upper | Managed GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Ordinary | 3,628.57 → 3,466.85 | 3,355.46 | 1.033 / 1.037 | 169,584 |
| Live unsignaled token | 3,738.19 → 3,555.20 | 3,363.08 | 1.057 / 1.066 | 169,584 |

Independent contemporary-original/candidate confirmations save **161.72µs/4.46%raw**
and **183.00µs/4.90%raw**, respectively. Native-normalized reductions are4.47%/5.15%;
native points move+0.015%/+0.267%. Both filtered/all-actual raw and normalized
confidence views separate for both comparisons. Initial three-actual screens and
every actual remain; no further sampling follows. Both cases remain parity gaps.

Dedicated quiet VM, Release, CPU0;12managed actuals and12samples per native bracket
with idle-v2 guards. Frozen132961B input produces exact531468B RGBA. Complete public
decode includes preparation, normalization, admission, independent output and disposal.
Caller token preparation is outside timing. FFmpeg retains its documented decoder/
conversion contexts and private scratch. These are development-module costs, not
fresh-context setup parity or a full125-row actual-package refresh. Two displayed
costs refresh;123 numerical rows reuse unchanged paths. Gray/420/422 and coding
validation do not enter the changed renderer: six controls remain1pass/3gaps/2uncertain.

Allocation falls701233→169584B/op on both controls. Both OSs observe the following
on the same frozen input with normal/live tokens; borrowed encoded input is outside
the shared charge:

| Storage boundary | Original → current |
| --- | ---: |
| Governed admitted peak | 1,864,806 → 1,864,806 B |
| Governed result retained until disposal | 531,468 → 531,468 B |
| Governed charge after disposal | 0 → 0 B |
| Sequential default idle pixel capacity | 0 → 531,468 B |
| Sequential default idle coefficient capacity | 1,697,280 → 1,697,280 B |

Pixel and coefficient ceilings remain4MiB each; the pixel pool retains at most two
exact-size arrays. Simultaneous images own distinct storage. Governed calls neither
consume nor populate either cache. Total GC, admitted peak and idle retention are
separate; physical/native peaks are unmeasured. Default timings establish no governed
CPU gain. Earlier [plane scheduling](JPEG-PROGRESSIVE-PLANE-SCHEDULING.md) and
[coefficient-reference lifetimes](JPEG-COEFFICIENT-REFERENCE-LIFETIME.md) retain history.

Windows/Linux each pass **5808full/live checks** (2543internal/2317public/948differential),
plus42internal/375public per normal/scalar mode. Existing numerical kernels reuse
their unchanged disabled-ISA evidence. New tests poison pooled output across the
existing tiny/RGB/strip-tail controls, compare every pixel with caller-RGB samples,
retain simultaneous survivors and verify terminal refusal, cancellation, pool
bypass and async lifetime cleanup. The existing authored JPEG helper is linked into
internal tests; no new performance image or settings grid.

One immutable core/embedded build per OS serves the bounded round. Public-test,
harness, native, selector and fixture builds are zero; one diagnostic build adds
coefficient-cache observation. The SAME actual private0.2.1Release package and all
three examples pass both OSs, including exact progressive sync/async owned results.
PackageSHA256`064ca2e022066acfc19cc5dc32120b9f85a6c37334fa97040d37a949a341a54d`,
423197B; Windowscore6f108575…/Linuxcored47ded65…. Published packages are unchanged.

Raw`jpeg-progressive-output-qualified.tar.xz`,2116300B/138members,
SHA256`653eebc202f4fdcf10c007a9596fe6052df8e6ff8894efe9d9ea996d0011e2fc`,
is mirrored/member-verified. Sources/modules, originals, screens/confirmations,
guards, memory/full/package logs and prior-archive input providers are retained.
An input-metadata lookup was corrected before storage samples; timing jobs and
production builds were unchanged. Encoder packets, managed pixels and resource/
ownership contracts remain intact. Further work should target material remaining
costs, preserving the tested warm/default and native-context boundaries.
