# Current VP8 stream-decoder cost

The existing 513×259 VP8/ALPH control decodes the frozen 98,590-byte packet from
a fresh seekable Stream to independently owned RGBA8. Complete input buffering,
copies, EOF reading, decoding, chroma expansion, exact alpha and output release
are timed on one CPU for both subjects. Inputs/reference/settings are unchanged.

| Quiet short | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| StreamNative8 | 9,920.34 | 5,984.62 | 1.658 / 1.781 | 1,966,583 |

Both idle-v2 guards pass. Managed/native RGBA bytes, alpha and normal/scalar CLI
pixels remain exact: SHA256
`55cdb9d8ce5e4dbaf16967129b299f6f900aa1c2fa8dad554d26d91b02295dcd`.
Caller input remains unchanged and surviving output independent.

This is a current gap screen, without independent confirmation or a parity claim.
The [earlier 1.677× result](WARM-VP8-DECODER-RECONSTRUCTION.md) remains historical
for its pinned pre-cleanup producer; no cross-producer gain is attributed here.
[Current qualification](WARM-JPEG-PAIRED-ENTROPY.md), actual package and prepared
host/native workers are reused. No production, benchmark or reference build, extra
image/settings, repeated full suite/package proof or second timing run was needed.

[Evidence](warm-current-vp8-stream-decoder-evidence.json) retains samples, guards,
validation, immutable identities and mirrored raw archive. Keep this named gap;
the larger independently confirmed VP8 encoder gap remains the next optimization
priority. Full 253-path warm parity and replacement readiness remain open.
