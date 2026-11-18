# Bicubic storage on the current package

Four original bicubic cases use the changed shared idle storage. The tight
RGBA8 and default horizontal RGBA16 enlargements pass on the actual 0.2.1
package. Both RGBA8 reductions remain uncertain.

| Complete operation | Lokad.Image µs | Prepared zimg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Default RGBA8 reduction | 416.73 | 417.10 | 0.945039–1.039399 | 199,984 | Uncertain |
| Tight RGBA8 reduction | 414.38 | 419.26 | 0.974644–1.003866 | 113 | Uncertain |
| Tight RGBA8 enlargement | 3,031.48 | 3,301.11 | 0.906749–0.925615 | 116 | Pass |
| Default horizontal RGBA16 enlargement | 1,496.19 | 1,572.94 | 0.932678–0.961315 | 114 | Pass |

All cases retain twelve actuals, both native brackets and idle-v2 guards.
The original workload delegates share one runtime with 127 verified non-core
files and the immutable package DLL. The complete public Resize includes
preparation, admission, filtering, exact repair and independent output disposal.
The labeled FFmpeg zscale backend retains geometry and scratch. Packing, filtering,
normalization and output ownership stay timed. Exact managed output hashes,
original native bounds, source preservation and independent output checks pass.

Default RGBA8 reduction's first seven managed actuals range from 414.55 to
415.38 µs. Its final three range from 424.94 to 435.16 µs. Native bracket
means move from 417.10 to 423.40 µs. The adjacent confirmation below investigates this shift. Its cause remains
unverified. The tight
reduction's upper is 0.999062 for the filtered view and 1.003866 with every
actual; it stays open under the conservative rule. Neither reduction establishes
a production gap. Both enlargements pass and stop.

This is measurement-only work on the previously qualified package. GC allocation
stays separate from retained storage, admitted capacity and physical peak.
The mirrored `bicubic-storage-current-package-confirmations.tar.xz` has 28 verified
members and 109,044 B, SHA-256
`23a5cbf0938c4478ab5043d267d781b4253f9aa545642fe73fb351920a143245`.
It retains the original and current commands, every sample, quietness guards
and validity checks. [Evidence](bicubic-current-package-evidence.json)

## Adjacent native confirmation

Default RGBA8 reduction remains uncertain at **414.12 / 416.74 µs**, interval
**0.895484–1.024193**, **199,985 B/op**. Each original managed actual has
240 complete native operations immediately before and after, outside its clock.
The twelve actuals use 496 operations each under the original 200 ms pilot rule.
Both original native brackets also remain. The before-actual paired upper
is 0.995325. The after-actual paired upper is 1.010318. The faster original
bracket and all-actual rule keep the full result open. The reference groups
still differ, so the result supports neither a parity pass nor a demonstrated
gap. Further identical captures are deferred pending a concrete timing
diagnosis or affected production change.

The initial launcher supplied an extra argument. The driver rejected it before
setup or sampling, and the corrected command reused the same binary, runtime,
input and checks. The mirrored archive retains both attempts, all valid samples,
guard and validation records. `bicubic-default-byte-interleaved-confirmation.tar.xz` has 17 verified
members and 23,980 B, SHA-256
`e7c5a136f1940309ecbb731b6f24e8c6ff00cadc6cab678969f5243f1133e160`.
