# VP8 whole-frame entropy compression milestone

The fixed opaque diagram now meets the **complete-packet size and unchanged
quality gates**: **7,046 B versus7,100 B** for FFmpeg/libwebp. The photograph
improves to19,808 B versus17,688 B; its size gate remains open. This qualifies
only the two frozen513×259 controls and their named settings. CPU parity remains
deferred while compression gates are open.

| Fixed control | Previous → current / reference B | Source-PSNR gap dB | Complete managed / reference µs | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Photograph8 | 21,202 → 19,808 / 17,688 | 0.021969 | 11,605.40 / 9,664.37 | 625,323 |
| Diagram8 | 7,642 → 7,046 / 7,100 | 0.214030 | 6,638.80 / 5,829.99 | 571,203 |

Managed78/75, native75 and effort4 remain frozen. Equal quality numbers do not
establish matching. Relative to the previous intra16/Y2 milestone, this saves
1,394/596 B (6.6%/7.8%). Relative to the earlier21,432/8,288 B packets, savings
are7.6%/15.0%. Reconstructed pixels and alpha are **identical to C53**; the source-
PSNR absolute-gap ceiling remains0.25dB. The diagram result does not establish
arbitrary diagram, default-quality or general WebP compression parity.

An independent owned bitstream replay first proves exact original syntax, then
changes only the coefficient probability model, including update signaling cost.
Normal/scalar native YUV and managed RGBA remain exact. The integrated producer
emits exactly those verified packets. Prediction costs freeze at the original
64-macroblock prefix, while the final model uses whole-frame statistics. Converted
private planes hold coefficient data; there is no second raster pass or full-frame
copy. Additional cache capacity is admitted before allocation through the same
borrowed root. Local limits choose the existing prefix path before allocation;
shared refusal terminates without retry, waiting or a lower-memory fallback.

**4,601 tests/OS** pass, including932 live FFmpeg comparisons, plus136 internal
and2 public scalar checks perOS. A regression verifies frozen prefix prediction
costs through final entropy selection; existing cache-range, cancellation, refusal,
cleanup and independent-output checks remain passing. The same actual private
0.2.0 package and all three examples pass on both platforms. One core and one
internal-test build perOS; existing public/differential/reference/harness/native/
selector binaries are reused, with no production rebuild after freeze.

Two sequential three-actual short **cost screens** retain idle-v2 guards and
native pre/post brackets on the dedicated EPYC9V45 VM, one pinned CPU,
.NET10.0.8, normal SIMD, tiering off, nonconcurrent workstation GC. Admission,
source processing, fresh output growth and independent packet ownership are timed
on complete equivalent operations; preparation, validation, IPC and startup are
excluded. Allocations include owned managed operation storage; native allocations
remain unmeasured. CPU/allocation increases are recorded, without a parity veto
or independent timing confirmation. No quality/settings/input grid is added.

Exactly the two matched-quality content encoding rows supersede their C53 costs.
The other five existing VP8 encode rows use Quality100/q0 and retain their prefix
branch; VP8L, PNG, JPEG, editing, inspection, font and frozen decode proofs remain
valid within their original producer scope. Optional JPEGCompactColor and retained
PNG packet acceptance remain separately labeled.

[Numeric evidence](vp8-full-model-compression-evidence.json) retains actuals,
quality gates, allocation figures, identities and guards. Raw archives on both
hosts:

- `c54-qualified-windows.tar.gz`: `79da880630497c149915a2392735d8eb466e19a7feb109e39a27064373d7febb`

- `c54-qualified-linux.tar.gz`: `a6c0bb385b55503e5f7127d52c5755f5b6527b4a65304d59075f3b3f18b84de6`

The replay audit is retained as `c54-full-model-audit.tar.gz`, SHA256 `41e9c0f7f109f47695febc8722f2c70d68424fbfe3db19b6b635119e562d84fc`.

Actual private package SHA256: `de50cb8fb7f4555bdddefe8e19f3f56835a9d339f0542b5cf5669a596ad21619`.
