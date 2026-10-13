# Palette8 expansion in SIMD registers

Full 256-entry palette8 rows with absent/full tRNS and at least 256 pixels may
expand 64 indices at once through safe AVX-512 VBMI/BW intrinsics. RGB component
positions derive from PNG RGB triples (3*i+c); byte lookups preserve all channels,
including hidden RGB at zero alpha. Palette channels stay in registers, without
an expanded lookup buffer. Partial palettes/tRNS, short rows and other CPUs keep
the original scalar path; tails are scalar. Cancellation still checks every 4,096
pixels. Work/memory admission and failure ordering are unchanged.

One unchanged noninterlaced/None palette8+full tRNS PNG uses the existing original
513x259 RGBA8 projection. Complete resident Decode/owned-output disposal is timed.
Native PAL8 expansion, one-thread conversion and independent output acquisition/
release stay inside the boundary; contexts are reused. Pixels, alpha/hidden RGB,
native normal/scalar pixels, source and retained owners are exact. Startup/I/O/IPC/
preparation/validation stay outside timing. No original performance image is added.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Prior buffered scalar expansion | 737.54 | 363.65 | 2.028 / 2.446 | 10,976 |
| Three-actual SIMD screen | 556.90 | 365.94 | 1.522 / 1.820 | 10,976 |
| Independent twelve-actual confirmation | 553.32 | 362.52 | 1.526 / 1.543 | 10,976 |

The screen reduces managed mean 24.5%. The independent confirmation is
**1.526x / upper 1.543x**, with 10,976 B/op: this fixed pair is **confirmed**.
Both guards pass; all twelve actuals and eleven filtered results remain retained.
Upper takes the larger filtered/all-actual bound. Allocation includes harness
overhead; native totals are unmeasured. Stop tuning this pair. First-use, held-output
and tight-budget boundaries remain distinct.

The updated profile preceding this change attributed 24% to scalar palette
expansion. Actual baseline JIT retained per-pixel palette slicing, channel assembly
and output bounds checks. Candidate JIT executes the byte permutes, inlines all
three inner helpers and keeps palette vectors in registers. These diagnostics are
not timing comparisons or a performance claim for the scalar fallback.

Each OS passes 2,669 Release tests (902 internal, 871 public, 896 live FFmpeg),
48 focused public PNG controls and 22 palette row controls both normal/scalar,
including original authored profiles, all indices/channels, unrelated channel
values, unaligned guards, vector/row tails, partial alpha, invalid-index prefix and
cancellation. The same actual dependency-free Release package and all three usage
examples pass on both OSes. All 595 final fresh/timed method bodies and flags match
that package. Earlier failed guards and rejected trials remain unchanged.

The [numeric evidence](warm-pngpalette-simd-evidence.json) retains every actual,
bracket, input/raster/source hash, four actual child identities, JIT/QA/package
proofs and frozen raw archives. RGB16/JPEG420 and the complete required material
scope remain open; this confirmation is not uniform warm parity.
