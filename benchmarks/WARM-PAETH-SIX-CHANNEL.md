# Six-byte PNG Paeth reconstruction

Updated 2026-10-17. The existing RGB16 Paeth case now independently confirms
**0.959× FFmpeg / conservative upper0.969×**, at unchanged **6,504 B/op**.
Its complete decode time is about half the prior selected measurement.

| Existing selector | Prior / current Lokad µs | FFmpeg µs | Ratio / upper | Lokad B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| DecodeRgbPaeth16 | 8,510.46 / 4,290.59 | 4,472.22 | 0.959 / 0.969 | 6,504 | Independent parity |
| DecodePaeth8 | 3,230.00 / 3,149.31 | 2,847.74 | 1.106 / 1.128 | 4,456 | Qualified short control |
| DecodeRgbPaeth8 | 3,140.91 / 3,140.69 | 2,632.59 | 1.193 / 1.241 | 3,432 | Qualified short control |

The primary retains all12 actuals/11 filtered samples and both versioned quiet
guards. Its all-actual mean is4,297.72µs; the upper retains the worse selected/
all-actual bound. Controls retain all three actuals and both guards. Native
allocation is unmeasured. No heap storage, fields or admission charges are added.

The same original513×259 word raster, opaque RGB projection and frozen color2/
depth16/all-Paeth PNG are used. Every sample and normal/scalar native comparison
is exact. Both sides still decode the complete identical packet, inflate,
reconstruct and acquire/release independent output. Native full RGB48BE-to-owned
RGBA64LE conversion and its second allocation remain timed. No decoded image is
cached; no fixture, image or settings grid is added.

One six-second profile put57.6% in scalar reconstruction. The new kernel applies
the existing Paeth rule to six byte lanes in portable Vector128 short lanes,
retaining ties and modulo256. Loads stay within both rows; stores change only
six bytes. Scalar tails, overlap, short previous rows and other hardware retain
the original behavior. Cancellation stays within64KiB. Existing four/eight-byte
kernels and the scalar recurrence are byte-identical; the two controls check
dispatch. The stopped passing RGBA16 Paeth path keeps its original first branch.

All **3,619 Release cases per OS** are covered (1,331 internal,1,390 public,
898 live FFmpeg), plus41 scalar PNG predictor controls. Linux had one external
FFmpeg timeout in an unchanged nearest-resize test; its40-case isolated retry
passed on the same build, and the initial failure is retained. Thirteen new
cases cover all256³ predictor triples, tails, guard bytes, overlap, truncated
previous rows and cancellation. The same actual package passes its consumer and
all three examples on Windows/Linux; DLL, symbols and SPEC match qualification.

The current approved PNG worker replaces historical wrapper hashes; backend,
compiler flags, static libraries and typed complete-operation boundaries remain
matched. Completed runs were reused after reconciling those IDs and an unrelated
Gray1-only description sentence; initial metadata rejections are retained.
No native/harness rebuild or repeated statistical job was needed. The screened
core remains byte-identical through qualification; the measured harness is restored.

The other204 comparisons, including all51 earlier parity proofs, are reused.
Stop the newly passing case; the two controls and the full parity objective stay open.
[Numeric evidence](warm-paeth-six-channel-evidence.json) includes every actual,
allocation, qualification and reference reconciliation. Raw archive:
`parity-p15-qualified-evidence.tar.gz`, SHA256
`97afacf2e44d06947e1bf405f0bf93725b98dda6d667322e3eebb108c7fb7791`.
