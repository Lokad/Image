# Current literal/run VP8L decode evidence

Historical timing checkpoint: [borrowed VP8L lookup](WARM-VP8L-BORROWED-GREEN.md)
records the current qualified short measurement. Its selected decode timing is superseded; original packets and boundaries remain.

The existing `WarmWebpDecode/Lossless8` case independently confirms after the
shared canonical Huffman lookup changed. Stop this pair. Newer native-input,
indexed, cancelable and Stream confirmations remain valid and are reused.

| Fixed input | Lokad.Image / FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | ---: | ---: | ---: | ---: |
| Literal/run VP8L, RGBA8 | 3,479.66 / 2,150.57 | 1.618 / 1.625 | 1,074,301 | 12 / 12 |

The original 513x259 raster and frozen 533,100-byte packet are unchanged. This
packet uses literals/runs without transforms, color cache or spatial entropy.
Both subjects perform complete resident-input decoding and acquire/release an
independent RGBA8 output. Native state reset and ARGB-to-RGBA conversion are timed;
warm codec/frame/packet wrappers do not cache decoded pixels. Exact RGBA, alpha,
hidden RGB, source preservation, simultaneous independent owners and normal/
scalar FFmpeg CLI checks pass before and after timing. No fixture regeneration,
extra image, settings sweep or new workload is introduced.

A three-actual screen precedes the independent twelve-actual confirmation on
the same immutable production, harness and reference binaries. All twelve
actuals are retained. The conservative upper includes selected/all-actual
intervals against the faster native bracket's lower confidence bound. Both
versioned quiet guards pass each run. Sequential Release uses CPU0, one native
codec/conversion thread, normal SIMD, tiering disabled and workstation
nonconcurrent GC. Managed allocations include harness costs; native allocations
are unmeasured. Startup, IPC, file/network I/O, hashing and validation are excluded.

The already-approved FFmpeg worker also serves newer indexed/cancelable/Stream
proofs. Its added fresh-Stream branch is unselected here; resident padded input,
decode/reset/conversion/output ownership and pinned FFmpeg archives are unchanged.
The short run's private postcheck initially expected an older worker hash and
preparation JSON without two new optional null fields. Those expectations were
corrected after checking the relevant worker change, original input hashes,
binary identities and current reference witness. Original refusal/configuration,
timings, guards and exact-pixel receipt are retained; no timing was repeated.

Zero production, harness or native builds are needed; two selection drivers
reuse every prepared child. Existing qualification remains valid: 3,483 Release
tests per OS including 898 live comparisons, plus the consumer and all three
examples through the same actual Release package on Windows/Linux. No repeated
qualification, profile or whole-library audit.

[Numeric evidence](warm-current-vp8l-literal-evidence.json) retains samples,
allocations, input/output hashes, binary identities, reference reuse, guards,
postcheck recovery and qualification reuse. This closes named B10 scope. Full
required warm coverage remains open; next are the twenty existing single-owner
exact edits after their shared output-pool change.
