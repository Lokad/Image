# Rejected sparse JPEG axis-cross trial

The complete CMYK sparse-2D decode **regressed 4.5%**. The candidate
is rejected; the qualified production implementation is restored.

| Existing complete resident decode | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Current qualified producer | 805.26 | 440.28 | 1.829 / 1.902 | 1,085,680 |
| Rejected axis-cross candidate | 841.28 | 444.63 | 1.892 / 1.984 | 1,085,682 |

The candidate specializes at most eight active frequencies on the horizontal/
vertical axes, preserving every original multiply, ordered addition and rounding.
It evaluates independent columns and transposes bytes in the existing block
destination with safe AVX512VBMI. No new heap storage or changed admission,
ownership, work or cancellation contract. Exact analytical pixels, normal/scalar
native quality (maximum1/RMSE0.7091, alpha255), unchanged input and two-live-output
independence pass. These correctness results do not imply a speedup.

One quiet three-actual short with three native samples before/after and both
idle-v2 guards reuses the original packet, current harness, selector, native worker
and complete-operation boundaries. Both outputs include normalization, fresh
ownership and disposal. JPEG22,708B; RGBA531,468B. No long run above parity,
new profile, image/settings grid or full/package qualification for rejected code.

Production source is unchanged. The qualified immutable producers/private package
and existing full cross-platform evidence remain valid. Five sparse-IDCT tests
freshly compiled against restored code pass normally and with intrinsics off on
both OS, including new signed-axis mixtures, quantizer extremes, guards, input
preservation and prewrite work/cancellation. The numeric summary retains the
[current producer](WARM-JPEG-CMYK-CURRENT.md); finite coverage is unchanged.

[Evidence](warm-jpeg-axis-cross-trial-evidence.json), raw
`cm3-rejected-linux.tar.gz`: `6bc9a4b09bc4e1bdac13962e1ab1ba3b89769e4032a5955d93048987f1007944`.
Candidate source `928d30d6a3e009a86fd6cadf568bf740422cf2e39011250e16fa18560ec32189`;
Linux producer `329d2d326120bbebc4508bff953dd6e0111d5387f5c6437b53b42899f10107ea`. Do not revisit this or the
earlier rejected prefix-sharing trial without a materially different strategy.
