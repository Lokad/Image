# Bicubic cache-pair view experiment

Neither implementation is selected. CPV667 prepares a paired view over the
existing pixel-vector cache; PCR668 makes pairs the root storage and borrows a
pixel view. Both preserve cache capacity, arithmetic order, certificates, repair,
cancellation and ownership. Three-column unaligned reads remain unchanged.

| Complete managed operation | Qualified original | Derived pair view | Paired root |
| --- | ---: | ---: | ---: |
| Frozen RGBA8 →128×64, µs | 476.04 | 460.76 | 459.78¹ |
| Frozen RGBA16 →128×64, µs | 453.66 | 449.48 | 440.83¹ |
| Existing large RGBA8 →1024×512, ms | 30.785 | 31.471 | 31.598¹ |

¹ Three-actual screen only. Other points use retained original confirmations and
independent twelve-actual candidate confirmations. Frozen inputs remain513×259;
the one existing large raster is4097×2051. Preparation, working storage,
independent output and disposal are timed. Existing native brackets and all
exact/native/source/owner/idle-v2 gates remain in the raw evidence. The large
comparison is managed-only and does not establish native parity.

The derived view improves the small-byte control by15.28 µs, but the large
operation regresses **0.686 ms / 2.23%**, with separated filtered and all-actual
intervals. The revised root's large interval is inconclusive; its point is not a
confirmed loss. Neither removes the targeted cache-base reload. Both byte ranges
have5159-byte code and a504-byte scalar frame, versus5158/472 previously. Register
and stack assignments differ, so the two implementations are not instruction-
identical. These findings do not establish a useful overall redesign or a cause
for every timing difference. No further unchanged confirmations are run.

Frozen allocations remain113 B/op; the large confirmation reports153 B/op. Its
657 B/op short-screen counter is retained and unexplained, rather than treated
as an established allocation regression. Cache payload/admission and scratch
retention remain unchanged; physical peaks are unmeasured. Each variant passes
227 internal/703 public focused normal/scalar checks on Windows/Linux, including
partial strips, half ties, transparency, tiny-alpha retirement, refusal,
cancellation and independent owners. No new fixtures or harness/native builds.

Sources, production/embedded modules and public bindings are restored byte for
byte to [qualified shared repair](BICUBIC-SHARED-BYTE-REPAIR.md). Its6,006 full/live
checks per OS, ISA checks and SAME actual package/all-three-example qualification
remain valid. No redundant full suite or package proof follows restoration.
Current displayed costs and package remain unchanged. Reconsider materially
revised designs when justified; this result does not close a kernel family.

[Evidence](bicubic-cache-pair-views-experiment-evidence.json) pins both variants,
all actuals/JIT/guards, original artifacts and restoration. Mirrored,
member-verified raw `bicubic-cache-pair-views-unselected.tar.xz` has15,297,852 bytes /
400 members, SHA256
`19a38b91c883ce945b2002342293186816af890b6b9059a0c00e7f10f274d83c`.
It adds no renderer dependency. Continue the larger demonstrated JPEG gap;
compression correctness remains ahead of CPU gains.
