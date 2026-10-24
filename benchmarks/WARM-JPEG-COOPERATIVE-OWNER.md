# JPEG encoding with caller memory admission

`MemoryBudget/Jpeg420Rgba16` adds one required ownership boundary to the finite
checklist. The existing 513×259 RGBA16 control retains its input, background,
quality85/native qscale5 and sampling. Complete packets remain 52,115/52,117 B,
differing only by the approved attribution COM replacement.

| Quiet one-CPU screen | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Caller IMemoryBudget | 6,746.29 | 1,839.67 | 3.667 / 4.407 | 803,968 |

Both versioned idle guards pass. The conservative lower ratio is 2.952,
so the CPU gap is clear; there is no independent confirmation or speedup claim.
Operations include normalization, encoding, fresh owned output and release.
Source/root setup stays outside timing. Native allocation remains unmeasured.

The borrowed 32 MiB caller root uses the existing benchmark adapter through
`ImageLimits.MemoryBudget`. Its resident source charge is 1,062,936 B; peak live
charges including that source are 1,704,728 B. Governed coefficients use private
storage. The original ungoverned control allocates 372,808 B/op on its own boundary;
the separate coarse `ImageBudget` control does not exercise this ownership path.

The smoke and screen check exact packets/raster normalization, borrowed-root and
source lifetimes, temporary refunds, independent surviving packets, terminal
refusal/recovery and cancellation. The existing native, fixtures and generated
host are reused. Benchmark compilation passes on both platforms with an explicit
Compile target; production/probe hashes and timestamps stay unchanged. The VM
grows about 165 KB without copying runtime dependencies or rebuilding production.
Production retains its prior 5,064 checks per platform and actual package proof.

[Evidence](warm-jpeg-cooperative-owner-evidence.json) records the scoped costs,
identities and reuse. This is one distinct required boundary, preserving all
previous controls: 257 cases, 240 numeric and 17 unmeasured. JPEG remains the
largest encoder gap; a bounded stage diagnostic precedes its next optimization.
