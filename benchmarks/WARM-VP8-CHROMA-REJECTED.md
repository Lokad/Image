# VP8 chroma quality diagnostic

One alpha-weighted linear-sRGB chroma candidate improves the fixed quality-100
encode from 15.8033 to 16.4108 dB, but stays 0.2908 dB below the retained native
quality-75 anchor. The required band is 0.25 dB. The candidate is rejected;
production remains unchanged and no parity timing or quality sweep follows.
The original production gap remains [0.8984 dB](WARM-WEBP-MATCH.md).

The preceding conversion-only round trip has 15.7991 dB source PSNR, almost the
same error as the original quality-100 encode. This guides chroma preparation;
it is not an optimal error bound. The single candidate averages mixed-opacity
2x2 blocks in linear sRGB with alpha weights, converts back to encoded sRGB,
then applies the existing limited-range BT.601 matrix. Fully opaque and fully
transparent blocks keep the original average. Luma, alpha, padded edges and
16-to-8 normalization retain their original rules. Transfer constants follow
the public [sRGB transfer specification](https://www.w3.org/TR/css-color-4/#color-conversion-code);
no upstream source or implementation table is copied.

Conversion-only candidate error falls about 13% to 16.4080 dB. One complete
quality-100/effort-4 encode of the same original 513x259 RGBA8 raster reaches
16.4108 dB through pinned FFmpeg n8.1 and the registered chroma-expansion/color
oracle. Alpha is exact; owned/native decoder RGB maximum is 1, RMSE 0.1289.
Normal/scalar owned encodes are byte-identical on both OSes, and normal/scalar
native decoded samples agree. Every existing production method body and flag
in the ignored prototype assembly is equivalent to the accepted actual package;
only the private diagnostic entry and candidate encoder are additional.

The [numeric evidence](vp8-chroma-rejected-evidence.json) retains the candidate
definition, complete errors, source/output/prototype/reference identities and
immutable raw archive digest. An initial direct-RGBA native conversion failed
normal/scalar agreement; those invalid controls remain archived and unscored.
Only the existing registered oracle supplies quality figures. These are untimed
diagnostics, not performance measurements. See the
[maintained benchmark state](../docs/benchmark.md). Next, cover the required
PNG RGBA8 encode pair with the existing raster and fixed level/filter.
