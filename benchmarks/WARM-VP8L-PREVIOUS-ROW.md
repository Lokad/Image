# VP8L previous-row references

One bounded primitive copies matching normalized pixels from the previous row,
including overlapping sequences that span rows. The fixed text/diagram packet
shrinks **31,072→15,120 bytes (51.3%)** and allocations **67,720→34,928 B/op
(48.4%)**. The photograph shrinks **385,854→383,154 B (0.7%)**. All five affected
complete encodes independently pass the warm single-CPU time target.

| Complete encode | Lokad / reference µs | Ratio / conservative upper | Allocated B/op | Encoded B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: |
| Original RGBA8 | 9,878.57 / 122,155.83 | 0.081 / 0.082 | 2,100,170 | 533,100 / 183,812 |
| Original RGBA16 → 8-bit | 11,972.58 / 121,563.39 | 0.098 / 0.099 | 2,100,119 | 533,100 / 183,812 |
| Original opaque RGBA8 | 9,198.47 / 70,847.86 | 0.130 / 0.132 | 1,051,213 | 400,262 / 121,896 |
| Photograph RGBA8 | 9,279.32 / 82,847.01 | 0.112 / 0.113 | 1,051,162 | 383,154 / 160,032 |
| Text/diagram RGBA8 | 2,713.60 / 9,769.91 | 0.278 / 0.284 | 34,928 | 15,120 / 7,234 |

Additional sequence comparisons increase these mean times about40–55% versus
the retained prior-round means. This is a compression-size/allocation improvement
with an explicit time cost; all remain substantially below the reference. Lossless
size still exceeds the backend2.394×/2.090× for photograph/diagram. Compression
efficiency and the full library parity objective remain open. Stop these passing
time cases until a relevant production change.

The independent implementation follows [RFC9649 sections3.6.2.2–3.6.2.2.1](https://www.rfc-editor.org/rfc/rfc9649.html#section-3.6.2.2):
distance code1 selects the pixel one row above. Each sequence is bounded by
`64<<effort`, at most4096; effort0 remains literal-only. Existing constant-color
runs win ties. Both passes compare resident normalized samples, preserving hidden
RGB, alpha and16→8bit integer normalization. No row/frame copy, histogram, hash
table, third source scan or additional working-memory allowance is added. Source
plus8KiB scratch, charged work/cancellation and complete output preflight remain.

The reference remains **libwebp exact=1, FFmpeg's encoder backend**, effort4,
native quality75 as a lossless compression control. Inputs/settings are unchanged:
the two original rasters/opacity projection and the two [fixed content controls](Fixtures/Compression/manifest.json).
No new profile, native worker, timing harness or driver build. Original verified
FF58 and7dd harnesses and prepared short/confirmation drivers are reused with one
immutable candidate DLL. Every operation includes public admission, required
normalization/repacking, fresh independent complete output ownership and disposal;
no ToArray in timing. I/O/IPC/preparation/startup/validation are outside both sides.
Native allocations are unmeasured.

Five3actual screens then five independent12actual confirmations retain all75
managed actuals, native pre/post brackets and both idle-v2 guards per run. The
upper is max(selected/all-actual interval) against the faster native bracket's
lower bound. Normal/scalar FFmpeg and managed decoding preserve every RGBA sample;
source preservation, deterministic packets, independent surviving owners and
syntax pass. VP8 packets reproduce their frozen anchors exactly without retuning;
their quality and timing evidence remains applicable. Other codecs/decoders,
Identify, edits, drawing and text are unchanged. Shared/async lossless operations
remain named pending cases, not implied passes.

All **4,161 tests/OS** (1,412 internal,1,823 public,926 liveFFmpeg), **30 affected
hardware-off checks/OS**, and the **same actual Windows package** consumer/all
three examples pass on Windows/Linux. New controls cover nonconstant repeated
rows, overlapping references, changed suffixes, following constant runs,
transparent hidden colors, normalized16bit pixels, exact working/output limits
and preflight ownership. Existing resource, shared-budget and cancellation tests
remain. No upstream sources/tables/assets enter production; no unsafe/native or
NuGet dependency is added. No tracked original raster is regenerated.

[Numeric evidence](warm-vp8l-previous-row-evidence.json) retains complete logs,
statistics, allocations, packet sizes and qualification. Timed owned source
5a6f8e5e85487088c2c51164bb1e717f7ac38ed40464b90561edb28fd37c7af0,
2404files, predates only final reporting. Linux producer
84cdfc050926bfdfc790825f5c9810187b4e87f49173d7fd1c92befb0e306da3;
Windows35227d2cd6c52eaf86f5e3f05ba6a7a8631aa8515cc2343c253d2f5eb1017e6c.
Private Windows package75873df4952e01749dc0fefe4b52bfc72f0fd27f4b3140ab54c92c1b9fcbdc48.
The published0.1.0 package/tag is unchanged.

Raw archives retained on both hosts: `h9c-measurements.tar.gz`
97fad709dd6a33f9c6a7d3b7aea3f499ec92195ce469874d01e0da9a0adf053b;
`h9c-qualified-windows.tar.gz`
418e23bf366558a644cb726bbd006ca2bb6c36664012c10ebe2a6106ddc26dac;
`h9c-qualified-linux.tar.gz`
ce2e5384083ca61b665c75b6bdec865d36396a57d4c6467c8cf38b42024ffe65.
Prepared-driver archives retain their original hashes in each observation.
The complete finite checklist and unchanged passing proofs are preserved.
