# RGBA16 bilinear precision audit

Lokad.Image matches the exact rational reference on the existing original
513x259-to-256x129 RGBA16 raster. FFmpeg's float path differs by one raw alpha
sample at 62 of 33,024 output pixels. This case remains unmatched under
exact-alpha performance acceptance. This historical audit reports no timing.
The current comparison additionally allows an explicit native-only alpha bound
of maximum 1 / RMSE 0.25 at depth 16. Managed rational pixels remain exact;
the default native mode and RGBA8 still require exact alpha. Current timing and
confirmation status belong in the [benchmark summary](../docs/benchmark.md).

| Subject | RGB maximum / RMSE | Alpha maximum / RMSE |
| --- | ---: | ---: |
| Lokad.Image versus rational reference | 0 / 0 | 0 / 0 |
| FFmpeg versus rational reference | 1 / 0.04733703 | 1 / 0.04332923 |

All differences use full 16-bit sample units, with a maximum value of 65,535.
Input/output formats are RGBA64LE, with GBRAP16LE around the widened triangle
filter. Straight-alpha metadata preserves encoded-space premultiplication and
unpremultiplication. This is the existing authored asymmetric raster at full
precision, with explicit hidden RGB at zero-alpha input corners. No performance
raster, size, pattern or quality sweep is added. Normal and disabled FFmpeg CPU
flags give the same output. Windows and Linux reproduce every audited byte.

Alpha mismatches lie 0.00001497 to 0.00514816 sample units from a
half-integer rounding boundary. At output (20,5), the independent integer
reference evaluates 73237924864 / 2138240 = 34251.498832684825 and rounds to 34251.
The native float-stage observation is 34251.5007930994; the integer output is
34252. A separate GBRAPF32LE output records the native alpha stage. The numeric
evidence retains the rational numerators/denominators and eight examples.

The shared CLI oracle and original native adapter now preserve RGBA16 input,
packing and output precision. Independent native validation uses 128-bit
integer alpha/color products, covering the adapter's admitted geometry without
overflow. Validation remains outside timed filter calls. Exact-alpha acceptance
rejects this raster before the native ready/sample protocol. A small all-zero
alpha control verifies hidden RGB clearing, source integrity and two independent
live RGBA16 outputs. Fourteen existing native RGBA8 negative controls and the
single RGBA8 BenchmarkDotNet smoke case still pass.

The added live test audits full-precision native rounding while retaining exact
managed rational pixels. Its one-sample native alpha diagnostic bound does not
qualify a matched performance pair under this audit's strict-alpha rule.
All 2129 Release tests passed on each OS at the audited source.
All 543 production method bodies and implementation flags match the retained
Release package; production output and algorithms are unchanged.

Reproduce the correctness audit with the pinned development FFmpeg tools:

```powershell
dotnet build tools/WarmReference/Bilinear16Audit/Audit.csproj -c Release --tl:off
dotnet tools/WarmReference/Bilinear16Audit/bin/Release/net10.0/Audit.dll artifacts/warm-reference/bilinear16-audit
```

On the dedicated Linux environment, build the native adapter and check refusal
and ownership with `python3 tools/WarmReference/verify-bilinear16-linux.py --cpu 0`.
The [numeric evidence](bilinear16-precision-evidence.json) retains the exact audit
and controls. The immutable archive SHA-256 is
`927169a84d22a5ccf8147fdbe54a4a20e364b556eab993bf3a0b8b5d2c20bcfc`.
Regenerate with `python benchmarks/render-bilinear16-precision.py`; use `--check`
to verify this report. Other material warm coverage and independent quiet
confirmation remain open.
