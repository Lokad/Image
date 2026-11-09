# Current resize and CMYK attribution

CNA707/JCA708 reuse the qualified [prepared-geometry package](CACHED-WIDE-PREPARED-GEOMETRY.md).
No production, harness, fixture or package build occurs. The first audit is an
arithmetic model; the second is one two-second quiet-VM capture. Neither supplies
new statistical timings, a CPU gain or phase-time measurements.

## Resize arithmetic and repair

The [earlier FP32 implementation](BICUBIC-FLOAT-REDUCTION-REJECTED.md) saved storage
but worsened its complete-operation screen. Its retained source uses interleaved
FP32/FMA rows, conservative publication certificates and original-order binary64
replay. It does not implement planar tap reductions or the current wide path.

A bounded model recreates its frozen input and complete output hashes exactly.
Its certificate declines **1,662 / 33,024** pixels: 1,591 meet an RGB half-boundary
condition, 59 an alpha half-boundary condition and 18 have alpha below 64; reasons
overlap. Thus simply lowering that alpha threshold addresses little of the
modeled replay. This is source/arithmetic evidence, not instrumented call counts
or a causal explanation of the old elapsed time.

On the existing 128×64 wide control, a draft conservative FP32 interval declines
176 / 8,192 pixels; the comparable ordered-double model with current wide margins
declines 41. The original smaller control has 830 / 195 under those same draft /
wide-double-margin models. These comparisons do not model the current narrow
publisher or establish a production certificate. Observed FP32 differences below
0.000075 samples do not prove a rounding guarantee on other inputs.

Keep the original binary64 product/accumulation boundary and rational tiny-alpha
repair. A planar/vector-reduction design would require a complete bound for its
actual instruction order and a credible repair strategy before another build.
The present audit does not justify replaying the earlier design. Revised designs
remain possible; no numerical policy is relaxed and no family is permanently closed.

## Current CMYK execution

The existing 22,708-byte sparse Adobe CMYK packet produces the exact independent
531,468-byte RGBA frame, including the original native and ownership checks.
The Windows package DLL supplies execution; Linux qualification remains separate.
CPU0 runs complete public decode/disposal, CPU1 samples, and both idle-v2 guards
pass. FFmpeg retains its labeled contexts. All instrumented elapsed is discarded.

Of 1,997 sample locations, 541 fall in inverse AVX2, 252 in component-MCU packing,
241 in the plain baseline reader, 206 in baseline scheduling, 152 in dequantization
and 78 in in-place CMYK conversion. Locations, inlining and sampling skid prevent
interpreting these counts as exact costs or removable percentages.

The 1,231-byte generic packer repeatedly prepares component/output row spans and
offsets. Its generated widening sequence also round-trips words through stack
temporaries. This supplies a concrete next hypothesis: share a validated
eight-pixel tile borrower across equal-grid RGB and raw CMYK, reducing repeated
row preparation and using bounded packed-byte views. Retain partial-width and
portable paths, all raw components including K, and final post-EOI/late-Adobe
interpretation. No early color conversion or public output publication is allowed.

Use complete sparse-CMYK operations for a focused screen and existing direct-RGB444
for generalization if promising. Inspect actual generated code and then confirm
useful complete-operation gains; code size or removed checks alone cannot qualify
the design. No new shape/settings grid, owner, pool or memory allowance is proposed.

[Evidence](architectural-current-attribution-evidence.json) pins the models,
commands, sources, receipt, JIT/map/trace, hypotheses and unchanged qualification.
The existing 6,096 full/live checks per OS, scalar/ISA paths and SAME actual package /
three examples remain applicable; this source-only work does not repeat them.
Total allocation, admitted capacity and retained storage remain unchanged.
Mirrored/member-verified raw `architectural-current-attribution.tar.xz`:
142,644 bytes / 35 members, SHA256
`196ce5b7563808bbabfc3b4df8116a971554f6a237e212a621c809d514c9822b`.
Publication is postponed and the full 283-path objective remains open.
