# Frame-private VP8 dequantizer preparation

VP8 decoding prepares six exact scalar dequantizers per frame segment once,
instead of repeating clamping and table lookups in each macroblock's 24-subblock
loop. Up to four segments use 96 B of operation-owned stack storage within the
existing 8 KiB allowance. No new heap capacity, field, owner, API, pool or cache.

Independent contemporary comparisons establish these scoped gains:

- Live opaque: **4,926.83 → 4,881.47 µs**, saving **45.36 µs / 0.92% raw**.
  Filtered/all-actual raw intervals and filtered normalized intervals separate.
  The **0.86% normalized point gain is not robust across all actuals**.
- Resident RGBA8 with alpha: **6,072.55 → 5,961.70 µs**, saving
  **110.85 µs / 1.83% raw / 1.82% normalized**. Both confidence views separate.

Native points move −0.061%/−0.002%. Paired short screens had a larger live point
gain; they are retained separately, without pooling or favorable repeats.

| Existing complete control | Lokad µs | FFmpeg µs | Upper / status | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Native8 | 5,961.70 | 5,968.47 | 1.003 / uncertain | 238,864 |
| Vp8Stream | 5,847.99 | 5,996.63 | 0.980 / pass | 238,960 |
| Opaque8 | 4,821.81 | 4,796.94 | 1.009 / gap | 229,344 |
| SimpleOpaque8 | 4,923.60 | 4,889.73 | 1.011 / gap | 229,344 |
| OpaqueCancelable8 | 4,881.47 | 4,792.22 | 1.028 / gap | 229,344 |
| ValidateVp8Alpha | 4,409.05 | 5,448.59 | 0.811 / pass | 14,064 |

These six frozen controls cover the declared 513×259 VP8 decode/validation
boundaries. Stream alpha and validation pass/stop; three opaque controls remain
gaps. Resident alpha is uncertain, not a demonstrated optimization gap. Its
historical 5,844.19 µs point first suggested an adverse effect; the contemporary
6,072.55 µs original resolves that contradiction in favor of the scoped gain.
Do not substitute the historical pass for current confidence. The other four
costs establish standalone current results, without separate causal gain claims.

Complete public header/preparation, input delivery, normalization, filtering,
alpha/color conversion, independent output and disposal remain timed. Native
decoder/conversion contexts retain their explicitly declared preparation
boundary. The existing live-token cancellation asymmetry is unchanged. One CPU,
sequential quiet-VM operations and idle-v2 guards; no fixture/settings grid added.
Native allocation counts and physical peaks are unmeasured.

## Correctness and ownership

All decoded RGBA frames are exactly 531,468 B and match their frozen native
hashes; descriptor validation remains exact at 36 B. The old source reconstructs
outside quantizer preparation and scalar selection after ignoring whitespace.
All coefficient readers, token/context order, signed products/short casts,
prediction, inverse transforms, filtering and ownership bodies remain exact.
Original cancellation checks remain, with at most four preparation checkpoints.

Seven actual C# edge vectors cover base clamps, independent signed deltas, Y2
scaling, chroma-DC saturation, source preservation and destination sentinels.
The prior bounded algebra model covers 65,280 segment combinations and 19,840
independent delta vectors. This model is not CPU or managed-implementation proof.

GC allocations are unchanged on every control. The existing model owns 1,536 B;
the new 96 B table retires with ReadPlanes. Logical fixed payload plus the coarse
minor allowance totals 5,108 B within the existing 8,192 B reservation, made
before allocation. This is not a measured physical-stack peak. Frame/metadata
admission, terminal refusal, cancellation cleanup and awaited asynchronous input
lifetimes are unchanged. Independently retained frames remain charged at
531,468 B until disposal; no new retained scratch. Existing default output-pool
retention is separate from these operation-owned values.

## Qualification and reproduction

Windows/Linux each pass **5,955 full/live checks**, 1,004 internal/239 public
normal and scalar focused checks, and 80 internal/239 public with AVX2 or AVX512
disabled. The SAME actual private Release 0.2.1 package and all three examples
pass on both OSes. One main/embedded build per OS is reused throughout; no
new harness, selector, worker, profile or repeated confirmation.

Package `307d8866270fb021c6307a372f71b69675a058c37f09711b202d000a02235e44` (434,777 B);
Windows main `56acf730ef4cd87e644237e5128004c925e3a4736d73348daac8f4526a923477`;
Linux main `4f59928419711977442d09005b14755a52ee5698cdf5f62bbe4ea6cce2714371`. Published releases are unchanged.

Raw `vp8-frame-quantizer-preparation-qualified.tar.xz`, SHA-256 `845e24ce204393658cdd89e9bc910bbb42d2d0cab292f419f8478413bb96c176`,
13,831,460 B/294 members, mirrored and member-verified, retains
original/candidate sources and modules, the model, all actuals/native brackets,
frozen worker identities, proof restoration, checks and actual packages.
Reproduce with archived vqp630 prepare/edit/build/focused/measure/validation/
observe/compare/qualify/isa/package scripts and their pinned inputs.
[Structured evidence](vp8-frame-quantizer-preparation-evidence.json).

[Model-row stride](VP8-MODEL-ROW-STRIDE.md) remains useful implementation history;
its six CPU costs are superseded. Encoders, VP8L, other codecs, resizing and text
retain source-valid evidence. Publication and a full actual-package release
refresh remain postponed; the complete 283-path parity objective is open.
