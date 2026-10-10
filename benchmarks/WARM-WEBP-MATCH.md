# Warm VP8 quality preparation

The first native anchor is unmatched. At native quality 75, the closest of eight
managed settings is quality 100, with a logged source-error gap of **0.8984 dB**.
The fixed maximum is 0.25 dB. There is **no warm VP8 ratio** from this preparation;
the anchor remains unresolved and cannot support a uniform parity claim.

The [original numeric evidence](warm-webp-match-evidence.json) retains the full
retrieved preparation catalog, native control JSON, terminal process status,
source/worker identities and original four-decimal summary. The exact gap is
0.8983635456757593 dB: native source PSNR 16.701663 dB versus managed 15.803299 dB.
Evaluation order is 1, 25, 50, 75, 100, 88, 94, 97; every sampled error, decoder
comparison, output size and hash is retained. The Linux BenchmarkDotNet refusal
smoke is verified: it fails at the intended preparation gate and reports zero
accepted pairs. There are no warm timing samples.

This preparation uses one existing original 513x259 RGBA8 raster, with source
alpha and hidden RGB retained. Both effort settings are 4. Matching uses the
existing alpha-weighted RGB error before composite rounding, exact alpha and
unweighted decoder agreement including hidden RGB. No additional source raster,
quality-band relaxation, effort reduction or native-anchor sweep was used.

The original native worker passes fourteen protocol/argument/affinity rejection
controls, exact counters, repeated bitstream/source checks and independent live
packet ownership. Its complete still VP8/alpha output is 98,590 bytes. The
managed preparation gate separately accepts valid synthetic metadata and rejects
eleven invalid records on Windows and Linux, including unmatched or forged gaps/flags, extra settings,
duplicate evaluation entries and changed alpha/codec. These gate controls create
no rasters, encodes, native workers or timing samples.

Use the [worker instructions](../tools/WarmReference/README.md) to reproduce the
bounded preparation. `--validate-warm-webp-controls` checks its pre-native gate
without requiring a native environment. Retain this unmatched anchor when adding
future evidence; it is not an excluded passing case.
