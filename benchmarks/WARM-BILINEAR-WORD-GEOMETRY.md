# Reuse RGBA16 bilinear geometry

The existing word reduction independently confirms **13.7% less elapsed time**
and **17,309 fewer allocated B/op**. Native-normalized improvement is 14.0%;
filtered, all-actual and normalized independent intervals separately separate.
It remains a gap to warm single-CPU parity.

| Existing complete operation | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Down16, original | 544.837 | 1.308 / 1.318 | 290,124 |
| Down16, accepted | 470.261 | 1.125 / 1.133 | 272,815 |
| Up16, current companion | 2,624.619 | 1.287 / 1.297 | 4,498,923 |

One primitive prepares vertical geometry once and enters before an unused
horizontal plan. This control reduces vertical kernel evaluations from 22,317
to 519, replaces 25,792 admitted horizontal-plan bytes with 8,512 vertical-plan
bytes and retains bounded 4 KiB stack workspace. The six-column cache, pixel
arithmetic, rounding certificates and exact replay remain unchanged. Capacity
is reserved before allocation and released on every exit; shared refusal is
terminal. Tight local limits retain portable filtering. No new cache or owner.

The frozen 513×259→256×129/1027×519 controls keep complete public Resize,
admission/preparation, independent output and disposal. Native FFmpeg filtering
keeps packing, widened triangle, encoded-space alpha normalization, RGBA64 and
output ownership/release. Rational managed pixels remain exact; original native
RGB/alpha max1/RMSE≤0.25, zero-alpha policy, hashes/settings, normal/scalar CLI,
source and independent owners pass. Up16 is a current cost without a paired gain
claim. No new performance fixture, setting or shape grid.

A fresh three-actual pair clears 5%; its stable original reuses BRC1's valid
twelve and one candidate twelve confirms the gain. Every actual, filtered/all
interval, native bracket and passing idle-v2 guard is retained. One six-second
baseline profile and one existing Up16 companion confirmation; no variants or
successful timing repeats. One immutable core per OS, zero harness/native/
selector builds. Native allocation remains unmeasured.

Both OSes pass **5,220 checks each** (2,111 internal, 2,161 public, 948 live),
184 public resize/memory checks in normal/scalar modes, the same actual Release
package, governed asynchronous and new bilinear consumers, plus three examples.
New coverage proves temporary-storage release, retained output charge, terminal
aggregate refusal, cancellation cleanup, later success and caller ownership.
The accelerated public/package control fits output plus 24 KiB of workspace;
portable filtering retains its established allowance. Two test-tooling compile
failures are retained; correction changes no measured production bytes.

[Evidence](warm-bilinear-word-geometry-evidence.json) pins producers, baseline
reuse, qualifications and mirrored/member-verified
`bilinear-word-geometry-qualified.tar.xz`, SHA256
1649a10fddcede6d15d80e443c91b86fe43968b46fcfee2d005c55c8f43d2c60. The full 268-path parity objective
remains open; other required word geometries/boundaries need applicable proof.
