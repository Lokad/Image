# RGBA8 bicubic boundary audit

The existing original 513x259 RGBA8 raster, downsampled to 256x129 with
Catmull-Rom B=0/C=0.5, matches Lokad.Image's independent rational reference
exactly. Three FFmpeg reference strategies remain unmatched under the existing
RGB maximum 1/RMSE 0.25, exact-alpha and zero-alpha RGB bounds. No timing runs,
performance claims, production changes or tolerance changes accompany this audit.

| Reference strategy | RGB maximum / RMSE | Alpha maximum / mismatched pixels | Result |
| --- | ---: | ---: | --- |
| Direct zscale, B=0/C=0.5 | 12 / 0.489359 | 9 / 696 | Boundary and alpha mismatch |
| Float premultiply, swscale, unpremultiply | 1 / 0.236237 | 1 / 16,310 | Alpha mismatch |
| Same scaling, final zscale quantization | 1 / 0.715324 | 1 / 339 | RGB and alpha mismatch |

Direct zscale reproduces every audit value and output byte on Windows and Linux.
695 of its 696 alpha mismatches lie on the outermost output border; one lies
in the interior. Primary-source inspection confirms that FFmpeg forwards the
bicubic parameters to zimg, whose boundary tap positions reflect back into the
image before clamping. Lokad.Image extends the edge pixel, as specified in
[SPEC.md](../SPEC.md). The remaining interior alpha difference prevents exact
matching even independently of the boundary issue. Source insights were used
without copying upstream implementation or tables. See the pinned
[FFmpeg wrapper](https://github.com/FFmpeg/FFmpeg/blob/9047fa1b084f76b1b4d065af2d743df1b40dfb56/libavfilter/vf_zscale.c)
and [zimg filter construction](https://github.com/sekrit-twc/zimg/blob/f819b14e8f39d1282400b0d9543e8ef73c1b2bbd/src/zimg/resize/filter.cpp).

The two swscale alternatives were checked on Windows only. The pinned Linux
reference build lacks the required premultiply filter; attempting the first
alternative failed explicitly. Its failure and current filter inventory are
retained. The unavailable alternatives do not count as Linux checks. Even on
Windows, neither qualifies. Stop this reference search at three strategies.

Reproduce the correctness-only audit with a suitable FFmpeg executable:

```powershell
dotnet build tools/WarmReference/Bicubic8Audit/Audit.csproj -c Release --tl:off
dotnet tools/WarmReference/Bicubic8Audit/bin/Release/net10.0/Audit.dll artifacts/warm-reference/bicubic8-audit-zscale zscale
# These explicit alternatives require premultiply/unpremultiply filters.
dotnet tools/WarmReference/Bicubic8Audit/bin/Release/net10.0/Audit.dll artifacts/warm-reference/bicubic8-audit-swscale swscale
dotnet tools/WarmReference/Bicubic8Audit/bin/Release/net10.0/Audit.dll artifacts/warm-reference/bicubic8-audit-swscale-zscale swscale-zscale
```

The tool checks deterministic normal/scalar native pixels, source integrity,
simultaneously live independent managed owners, every sample and zero-alpha RGB.
It writes original input and rational/managed/native bytes with hashes. The
[numeric evidence](bicubic8-boundary-evidence.json) retains the comparison values,
fixed bounds, source insights, explicit Linux failure and immutable raw archive
identity. All production bodies and flags remain equivalent to the accepted
Release package on both OSes; its 2,137-test results remain applicable. No new
performance image, geometry matrix, quality search or production package is made.
The required warm bicubic case remains open.

An additional correctness-only diagnostic uses the public zimg active-region API.
FFmpeg's zscale wrapper fixes that region to the complete input frame and exposes
no boundary-mode option. A four-pixel repeated-edge halo around the same original
raster, with active region 513x259 starting at (4,4), preserves sample centers and
supplies the required edge extension. Physical input is 521x267, 4.7% more pixels.
This is a labeled scaler-backend diagnostic, not a stock FFmpeg CLI result.

Scalar zimg matches the rational reference byte for byte. Normal SIMD differs by
one alpha sample at (6,4): 173 instead of 172, alpha RMSE 0.005503; RGB maximum 1 /
RMSE 0.003177. Zero-alpha RGB is zero. The normal/scalar equality gate failed and
both outputs were retained. The existing exact-alpha RGBA8 gate remains unchanged;
this result receives no timing or parity status. The
[active-region evidence](bicubic8-active-region-evidence.json) records both outputs,
the original adapter, native binary and immutable raw archive identities.
