# Current byte-color PNG encoding costs

Three existing default color controls independently confirm warm single-CPU gaps
on the unchanged qualified core. These are current costs, without a paired
gain/regression comparison against older encoders.

| Existing 513×259 control | Lokad µs | Ratio / upper | Managed B/op | Packet B, exact both |
| --- | ---: | ---: | ---: | ---: |
| EncodeGray8 | 2,570.87 | 1.325 / 1.354 | 1,001,812 | 72,586 |
| EncodeRgb8 | 12,457.28 | 1.309 / 1.324 | 1,325,068 | 168,729 |
| EncodeGrayAlpha8 | 5,626.99 | 1.305 / 1.350 | 1,021,036 | 120,366 |

The original RGBA8 input projections remain: opaque equal-RGB grayscale, opaque
RGB and equal-RGB with full alpha/hidden-RGB preservation. Complete operations
include color validation/packing, compression, output growth, independent fresh
packet ownership and disposal. Preparation, hashing/checking, startup and IPC are
excluded. Native allocation is unmeasured; managed B/op includes harness costs.
All complete packets match the frozen FFmpeg level6/None reference byte for byte:

- Gray8: `c5b4a248c1ca0f76115d06e7795698f50ac7e623c366fcee4a659cb08da2872a`.
- RGB8: `e1b0f39154bb07b69c6fab59512dbd55f1fb855c0a9805a44892d510efb73228`.
- Gray-alpha8: `eefb49b94adbd654b38b4e33edc8aac89fdfd9273a8124a1fbf7884ee8789fb5`.

Every sample, alpha/hidden RGB, source and independently surviving output,
normal/scalar native decoding and both idle-v2 guards pass. Managed exact level6
and native level6 retain their declared packet contract; no setting is retuned.

Earlier independent byte-packing costs used the replaced BCL level9 policy with
a 5% IDAT tolerance. The exact managed encoder and later shared GFNI checksum
path invalidate those older CPU passes. Later displayed refreshes covered other
named PNG controls. Native inputs/settings/complete reference packets are
unchanged; no equivalent-code regression or broad content claim is inferred.

One twelve-actual job per control, four complete calls per actual; one immutable
SKP1 runtime, cached selector and unchanged native worker. **Zero builds,
profiles, new inputs/settings, retiming or repeated full/package checks.** The
rejected FMP1 candidate was restored byte-exactly on both OSes before these jobs.
Existing **5,332 checks per OS**, 948 live comparisons, the SAME actual Release
package/three examples and 201 focused restored PNG checks normal/scalar remain
applicable. Package stays `dfa0e669b84872990e01a0e65f21cdd87753b802859be904ac592945cfd49425`.

These three uncertainties are resolved; stop retiming them. Reconcile remaining
explicitly named stale boundaries, then optimize demonstrated material gaps.
The complete **268-path warm single-CPU upper ≤1×** objective, exact packets and
pixels, admission, cancellation and ownership remain.

[Numeric evidence](current-byte-color-png-costs-evidence.json) retains all actuals,
filtered/unfiltered confidence, input and packet pins and qualification reuse.
Mirrored/member-verified `current-byte-color-png-costs.tar.xz`, SHA256
`07a40c4925011bca5775a6e9f1272e4700154557062685112a70c5d668c25d1e` (3,331,448 B / 62 members),
retains the core, cached driver/worker, original projected inputs/reference
packets, guards, validation/logs and reproduction scripts. CF16/SKP1/restoration
raw dependencies remain retained.
