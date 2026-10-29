# Reuse framed PNG output for Adam7

Complete Adam7RGBA16 decoding independently improves **43.8%**, with exact pixels
and **23,616 → 23,240 allocated B/op**. It passes the warm one-CPU parity target
and stops. Both raw99.9%CI views and conservative native-normalized envelopes
separate from the valid prior independent confirmation.

| Complete 513×259 operation | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Previous Adam7RGBA16 | 5,293.592 | 1.346 / 1.362 | 23,616 |
| Current Adam7RGBA16 | 2,976.317 | 0.758 / 0.761 | 23,240 |

A six-second current-baseline diagnostic attributes47.1%of samples to BCL
inflation and1.7%to sparse copying. Reuse the existing certified framing output
rather than tuning small copy costs. One eligibility condition now permits
interlacedRGBA without a shared memory root to use the existing materialized
reader. It already supports all seven concatenated passes in wire order.
No new decoder, cache, owner, quota or unsafe code is introduced.

The tradeoff is **1,063,423 bytes of additional admitted filtered capacity** for
this control, reusable within the existing4MiBidle pool. It is distinct from
warm managed allocation. Reserve before allocation and return on every exit.
Existing local-fit guards choose the previous reader under tight ceilings;
shared-root interlace retains its previous storage requirements. There is no
retry or fallback after admission refusal. Cancellation, tree/checksum replay,
filter/error ordering, exact samples/alpha/hiddenRGB and independent ownership
remain. Noninterlaced and other relevant paths keep applicable evidence.

One fresh3actual original/candidate screen gives43.3%less elapsed. Independent
candidate12actual confirmation reuses the original PWE1twelve actuals, giving
43.775%filtered/43.775%all; no duplicate original confirmation, variants, new
images or settings. Both subjects retain complete decode, normalization, owned
output/disposal and the frozen original packet/native one-CPU normalSIMD settings.
All raw samples/brackets, filtered/unfiltered bounds and idle-v2 guards remain.
Native allocations are unmeasured; no timing prediction is taken from the profile.

All5,094tests/OS pass (2,011internal,2,135public,948live), plus11kernel and231public
normal/scalar checks. Six additional authored cases cover eight/sixteen-bit
interlace, exact tight limits, shared-root lifetimes, first/last filter/checksum
replay ordering and cleanup/recovery. The SAMEactualReleasepackage andthree usage
examples pass on both OSes. One corebuild/OS,zero harness/native/selector builds.
Published releases remain immutable.37existingcosts still need current evidence;
this scoped pass does not complete the full268path objective.

[Evidence](warm-png-adam7-framed-output-evidence.json) and
[prior word costs/diagnostic](CURRENT-PNG-WORD-DECODE-COSTS.md).
Mirrored/member-verified archive png-adam7-framed-output-qualified.tar.xz, 139members,
SHA256 daca93b49045bdcf8d35aca9bd2096d0a59852ff799ca0fb85b7138bf01eff99. Reproduction dependencies are pinned by hash.
