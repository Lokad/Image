# VP8 header-mode arithmetic window

**Historical after fixed-position arithmetic.** [Current six affected costs](VP8-FIXED-CODE.md)
supersede the timings below and close the live-token gap. Retain this stage's
source, scoped gains, adverse points and raw evidence as history.


Accepted: reuse the existing exact wide buffered arithmetic reader for macroblock
header modes. The mode tree has one source and selects the wide or scalar path
once per macroblock. Short declared partitions keep the scalar fallback. Every
decision still checks cancellation; no additional heap capacity or retained state.

Contemporary complete ordinary decoding improves **4,737.09 → 4,663.48 µs**,
**73.60 µs / 1.55% raw**. Reference-normalized effect is .98208, conservative
upper .98958, separated. These are actual standalone public-harness captures,
not subtraction from historical clocks.

The independent paired live-token confirmation improves **4,846.11 → 4,757.46 µs**,
**88.65 µs / 1.83%**, ratio interval .978514–.984912. Its earlier three-pair screen
was inconclusive (.937241–1.023385); both are retained. Contemporary standalone
live baseline/candidate intervals overlap. The larger ALC effect remains scoped;
no standalone live gain, cause or parity closure is inferred. Both contexts are
recorded, and no candidate capture is repeated.

| Complete existing control / FFmpeg | Lokad / reference µs | Conservative ratio interval | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Opaque8 | 4,663.48 / 4,791.29 | 0.968796–0.977427 | 230,880 | pass |
| OpaqueCancelable8 | 4,818.81 / 4,788.03 | 1.002505–1.009319 | 230,880 | gap |
| SimpleOpaque8 | 4,788.89 / 4,877.71 | 0.977477–0.985605 | 230,880 | pass |
| Native8 | 5,695.75 / 5,943.94 | 0.953885–0.960999 | 240,400 | pass |
| StreamNative8 | 5,664.40 / 5,985.36 | 0.943467–0.948885 | 240,496 | pass |
| ValidateVp8Alpha | 4,236.13 / 5,452.22 | 0.775561–0.778541 | 15,600 | pass |

Five affected passing boundaries stop; the live-token gap remains open. All twelve
WebP reading views have eleven passes and one gap. Six lossless-only paths reuse
unchanged evidence. The 289-path objective and publication postponement remain.

Each decode includes public parsing, admission, reconstruction/filtering, chroma/
RGBA normalization, independent output acquisition and disposal. Stream includes
fresh non-exposed MemoryStream acquisition/copies/EOF. Coding-only validation
checks every coded pixel but produces source metadata, without RGBA normalization.
FFmpeg retains the declared codec/conversion wrappers and input/scratch boundaries;
every packet is decoded. Frozen source/settings/native identities, exact complete
pixels/alpha or descriptors, input independence, CLI and ownership gates pass.

CPU0, sequential Release, optimized JIT, natural nonconcurrent workstation GC,
idle-v2 and all twelve actuals/native brackets remain. Existing tools, workers and
fixtures are reused. Coding validation also retains the filtered and all-actual
confidence bounds; the conservative combination passes. Its single-control proof
overlay is explicit and the original proof is restored byte-for-byte afterward.

A macroblock needs at most 121 decisions; the existing 272-byte/304-decision
lookahead proof covers it and respects the declared first-partition end. Tests
check 512 varied scalar/wide sequences, every completed cursor and 19 following
reads, partition boundaries, frozen large-packet normal/live decoding, borrowed
admission, cancellation cleanup and surviving outputs. Existing every-ordinal
cancellation tests remain. Constructor/header options, coefficient arithmetic,
reconstruction and filtering are unchanged except the internal factory rename.
An abandoned private decoder publishes only successful completed state; callers
cannot observe or resume its failed state.

All allocations are unchanged. An independent RGBA result owns 531,468 B; logical
capacity, active output ownership and default idle storage remain distinct from GC
allocation. The two-slot/4MiB idle policy and caller-root bypass are unchanged.
No new array, cache, pool, session or reservation; physical/native/JIT peaks are
unmeasured. No CPU benefit is inferred from storage figures.

Each OS passes **6,690 full/live checks** (3,261 internal / 2,481 public / 948 live),
plus **1,040 internal / 402 public normal and scalar checks**. The SAME actual
461,527 B Release package and all three usage examples pass on both OSes. Package
coverage reuses the frozen 513×259 packet and checks exact hashes, retained owners,
canceled-token identity and successful subsequent decode. One core/embedded build
per OS, zero tool/harness/native/fixture builds, zero producer rebuilds after timing,
one pack. Published releases are unchanged. Unaffected qualification is reused.

[Evidence](vp8-mode-window-evidence.json) pins source/modules, every sample,
baseline, gate, actual package and retained dependencies. Mirrored/member-verified
raw: vp8-mode-window-qualified.tar.xz, 3149316 B / 183 members, SHA256
e1ba2ec841b31acfcdbabfee9968a5028f33e3f0c5571ecbd49656347beea199.
[Earlier coefficient-window trials](VP8-LIVE-WINDOW-TRIALS.md) remain unaccepted
history; none of their coefficient state-bridge changes survives.
