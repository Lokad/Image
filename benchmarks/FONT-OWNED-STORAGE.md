# Owned font storage

Default font loads reuse exact-size byte storage within the existing two-array,
4 MiB image pool. Each load still copies every input byte, performs all eager
validation, returns an independent face and disposes that face inside timing.
This is storage reuse, with no parsed-font cache or production dependency.

Independent complete loading saves **258.73 µs / 89.04% raw** and **89.13%
reference-normalized**. Both filtered and all-actual confidence intervals separate;
native time moves +0.83%. All twelve actuals, including the filtered outlier, remain.

| Frozen DejaVu Sans Regular load / FreeType 2.13.2 | Lokad µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Contemporary original, twelve actuals | 290.59 | 13.31 | 21.836 / 22.061 | 757,597 |
| Independent current, twelve actuals | 31.86 | 13.42 | 2.375 / 2.396 | 176 |

The **gap remains open**. Claims cover this supplied 757,076-byte, 6,253-glyph
Regular face on one CPU. FreeType includes owned copy/open/retain/dispose.
Library/input setup, I/O, verification and IPC are excluded equally. Eager managed
header/cmap validation and FreeType's lazy validation differ; this does not prove
general parser equivalence. Governed loads and mixed workloads have no CPU claim.
The short screen precedes one contemporary original and one independent current
confirmation; there are no favorable repeats or expanded timing grids.

Current-source attribution retained 479 leaves:154 managed,210 libc,103 CoreCLR.
The matched libc hot instruction is `rep movsb` (204 leaves), under the input copy;
allocation chains are also present. Instrumented elapsed is discarded. These
samples motivate the design; they are not phase timings, exclusive GC time or
proof of which component caused the complete-operation gain. Earlier
[parser preparation](FONT-LOAD-PREPARATION.md) retains its source-specific history.

Admission and ownership precede rental/allocation. Caller-budgeted fonts bypass
the global pool, retain their original exact charge until disposal and never
populate that cache. Refusal is terminal; failed parsing/cancellation releases
owned admission without caching invalid input. No caller root or input is disposed.
The parser, lookup, validation order, every glyph checkpoint and original image
pool fields/`Rent`/`Return` bodies reconstruct unchanged. New exact lookup preserves
other idle sizes; successful font disposal may evict idle entries, never live ones.
Original image size misses retain their discard policy, so mixed reuse is not a
guaranteed hit. Prepared layouts own outlines and survive font storage reuse.

Active backing capacity remains757,076 B, plus the coarse256 B font descriptor
charge. The isolated serial harness reuses one backing array and retains757,076 B
idle after disposal; that storage was previously collectible. General idle storage
remains capped at two arrays totaling4MiB; simultaneous live faces are additional
independent owners. Two frozen-size live faces need1,514,152 B of backing storage.
These are logical capacity/source bounds, **not measured physical peaks or RSS**.
The original GC diagnostic has137 collections at each generation over688 loads;
current has zero over6,288 loads. Native allocations/physical peaks are unmeasured.

Nine added checks cover short/long locations, Regular/Bold independent faces,
full-overwrite source ownership, mixed font/image retention, byte/word layouts
after disposal/reuse, aggregate refusal, cancellation cleanup, malformed input,
recovery and concurrent owners. Each OS passes **5,859 full/live checks**
(2,594 internal /2,317 public /948 differential), plus238 internal /144 public
checks in each normal/scalar mode. Existing supplied-font/shaping/editing and
encoder exactness coverage remains required.

The SAME actual private 0.2.1 Release package/all three examples passes both OSes:
SHA256 `d9755c2e645183932bc6f1005be3a6026b3fd8895f1d6910f9d74dc1e7600372`,
426,621 B. Its consumer checks surviving layouts, aggregate admission and recovery.
One immutable core/embedded build per OS is reused; no public-test, benchmark,
worker, selector or fixture builds. Only the displayed font-load cost changes;
124 comparisons reuse unchanged executed paths. Other text controls prepare fonts
outside timing, and independent codec/resize workers do not load fonts. Published
packages, publication postponement and the full283-path objective remain unchanged.

[Evidence](font-owned-storage-evidence.json) retains source/ownership proofs,
measurements and qualification. Mirrored/member-verified raw
`font-owned-storage-qualified.tar.xz`:4,191,172 B /124 members,
SHA256 `c535b3d412eda3cc33137f0096626f6727c814b6a4e210d2d1a79aa62986fe91`.
Pinned dependencies retain the earlier qualified modules, parser preparation and
source-equivalent load disassembly; current copy attribution and native identities
are included.
