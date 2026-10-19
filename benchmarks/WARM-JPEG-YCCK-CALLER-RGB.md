# Caller YCCK420 RGB decoding

The first short screen demonstrates a **7.504x** gap against FFmpeg.
Both decode the same 6,704-byte original YCCK420/full-black packet to 398,601 bytes
of fresh caller-owned RGB24. Managed pixels exactly match the independent
analytical reference; native normalization matches the approved RGBA colors.

| Complete resident YCCK420 → caller RGB24 | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: |
| Existing original fixed projection | 4,532.59 / 604.01 | 7.504 / 7.804 | 1,337,431 |

This is the retained caller matrix/black path named in the finite checklist, with
the existing 513x259 packet and analytical RGBA colors projected to RGB outside
operations. All four coded components still decode. Native full-range BT.601
normalization goes directly from YUVA444P to RGB24. Both time fresh zeroed caller
acquisition, complete decode, necessary conversion/packing and release. Context,
resident padded packet and empty wrappers may be reused; decoded content cannot.
No RGBA intermediary, header patch, native packet change, ICC or EXIF transform.

Managed pixels, source, caller padding and independently surviving outputs pass.
The worker verifies two native live owners, deterministic pixels and one CPU/thread.
Normal/scalar native RGB24 is exactly the old native RGBA color projection:
max16/interior3/RMSE1.3489149162, within the unchanged approved fixed-packet bound
16 only at x/y mod16=0or15, 4 elsewhere, RGB RMSE≤1.5. No new baseline exception.

Three 200ms actuals, both idle-v2 guards and 50ms native pre/post brackets remain.
No long repeat above one. One six-second profile has 5,906 samples:93.2% in scalar
interpolation, caller color/black extraction, rounding and dispatch. Prioritize
one exact SIMD caller-row primitive. The profile makes no timing claim.

An initial harness size check rejected the new RGB24 opcode as generic RGBA,
before any WorkloadActual. Its failure is retained; the fix adds explicit RGB24
stride/size validation. One native build and initial/fixed narrow harness builds
per OS; zero producer/selector builds or repeated full qualification. Reuse the
qualified 4,208 tests/OS,95 hardware-off checks/OS and same actual private package
consumer/examples on both OS. All224previous numeric rows remain identical.
Scope247=225numeric+22unmeasured,70passes/154mean gaps/one confidence-open.

[Numeric evidence](warm-jpeg-ycck-caller-rgb-evidence.json). Raw on both hosts:

- `h10u-initial-gate.tar.gz` SHA`bd588864e06022c828937eb4a77e40429b121fcd370df3b7aa0479892049079e`.

- `h10u-profile.tar.gz` SHA`15b12cc58adbfa2981ee2596a70b522bf1c84aec9a546e5b1e68fdb2971f4472`.

- `h10u-measurements.tar.gz` SHA`50b879002cfb874713e0dc6ec8937f1d02b4620ed0fe44c7d2ea0f04cec12156`.
