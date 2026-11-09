# WebP clustering storage handoff

WCS673 reuses the dead matching index for clustering slots, statistics and pair
storage after population row preparation finishes. One stack-confined execution
state holds typed views; selected tokens, maps and codes remain independent.
Physical capacity selects reuse before admission, otherwise allocated storage is
used. Refusal never retries. Candidate order, ties and packet bytes are unchanged.

Complete contemporary screens save **470,360 B** per photograph and diagram
encode, **52,720 B** on transparent alpha and **3,672 B** on existing authored
bars. Admitted peaks stay **3,470,059 / 3,251,328 / 3,656,309 / 312,716 B**;
retained scratch stays zero. Earlier overlapping token candidates still dominate
the peak. Borrowed inputs and caller stream capacity are excluded. These are
coarse ownership charges; physical/native peaks are unmeasured. Direct GC
counters and the aggregate benchmark counters below are distinct observations.

| Complete operation / existing reference | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| VP8L RGBA8 / libwebp exact=1 | 106,060.70 | 119,911.37 | 0.884 / 0.895 | 6,871,448 | Pass |
| VP8L opaque / libwebp exact=1 | 69,786.62 | 69,713.36 | 1.001 / 1.011 | 5,377,824 | Uncertain |
| VP8L photograph / libwebp exact=1 | 80,313.11 | 82,136.49 | 0.978 / 0.984 | 5,912,164 | Pass |
| VP8L text/diagram / libwebp exact=1 | 7,474.87 | 9,578.90 | 0.780 / 0.792 | 3,758,681 | Pass |
| VP8L RGBA16 / libwebp exact=1 | 106,281.45 | 120,895.25 | 0.879 / 0.887 | 6,871,448 | Pass |
| VP8L nonseekable async / libwebp exact=1 | 105,426.73 | 120,807.40 | 0.873 / 0.882 | 6,871,832 | Pass |
| VP8 + lossless alpha RGBA8 / FFmpeg | 53,059.60 | 53,172.41 | 0.998 / 1.007 | 6,911,120 | Uncertain |
| VP8 + lossless alpha RGBA16 / FFmpeg | 53,054.13 | 53,857.43 | 0.985 / 0.990 | 6,910,304 | Pass |

Each has twelve actuals, native brackets, quiet guards and frozen packet/source/
ownership checks on the intended actual-package DLL. Normalization, preparation,
candidate selection, fresh output ownership and disposal stay timed. References
retain their declared context/input/scratch boundaries; no new one-shot setup
equivalence is claimed. Six passes stop; the other two are uncertain, without
demonstrated CPU gaps. The stage is accepted for allocation savings, **without
a causal CPU-gain claim**. No workload/settings matrix expands.

All four complete screen packets remain exact, including the original authored
bar generalization. New actual clustering tests cover nonzero scratch, empty
tiles, guards, undersized fallback, aliasing beyond the row prefix, refusal,
cancellation, recovery and independent surviving plans. Windows/Linux each pass
**6,015 full/live checks**, plus 417 internal / 239 public checks per normal/scalar
mode. The SAME actual Release package and all three examples pass both OSes.
SIMD helpers and dispatch remain unchanged; their prior ISA evidence is reused.

One immutable main/embedded artifact per OS serves the stage. A compile-time
name collision was corrected before producing either artifact. Existing workers,
fixtures, native references and resource tooling are reused. An async receipt
still named the prior DLL: setup failed before any timed actual; a fresh managed
probe restored current resource/sink/cancellation/ownership proof while retaining
the unchanged byte-exact native proof. No completed timing was repeated.

[Evidence](webp-clustering-storage-evidence.json) records source projection,
costs, ownership and actual-package proof. Mirrored/member-verified raw
`webp-clustering-storage-qualified.tar.xz` has 13,189,628 B / 267 members, SHA256
`1d3b2c5fddf29df61300395b468daa1de13551384c5763070128eb79f452b6eb`.
[Lifetime attribution](ARCHITECTURAL-PREPARATION-AND-STORAGE.md#current-webp-lifetime-follow-up)
and [earlier handoff](WEBP-MATCHING-POPULATION-STORAGE.md) retain history.
This is an eight-control development refresh; other unchanged paths reuse their
evidence. Full 283-path parity remains open; publication is postponed.
