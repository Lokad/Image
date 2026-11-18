# Reusing the disjoint row copier for owned crops

The original 513×259 to 256×129 crop still prepares row spans and a chunk
loop for each row. The candidate routes crops that fit one 1,024-pixel
cancellation chunk per row through the existing disjoint-row copier. It
borrows the packed input and independently owned result spans once.
The copier keeps bounded accesses and a cancellation checkpoint per row.
Admission, orientation, result lifetime and exception cleanup use the
existing crop code. Small, wide and portable cases keep their existing loop.

| Complete crop | Baseline µs | Candidate µs | Candidate ratio interval | Managed GC B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8, three managed pairs | 2.585436 | 2.157509 | 0.761870–0.914038 | 56 each |
| RGBA16, three managed pairs | 4.240583 | 3.875339 | 0.882991–0.945828 | 56 each |
| RGBA8, twelve new managed pairs | 2.368079 | 1.975896 | 0.831835–0.836947 | 56 each |
| RGBA16, twelve new managed pairs | 4.055008 | 4.198008 | 1.032881–1.037655 | 56 each |
| RGBA8-only candidate versus standalone FFmpeg | 2.496757 | 2.164972 | 0.851594–0.879181 | 56 |

The managed pairs use the actual package `1699cfb1` as baseline and
alternate complete operations in blocks of 4,096. They confirm a 16.6%
RGBA8 gain. RGBA16 reverses on confirmation and is 3.5% slower. Both
observations are retained. The next private build restricts the fast path
to RGBA8 and passes the separate standalone comparison. Its original driver
retains twelve actuals and all three native groups using the same 4,096-call
schedule. Every native operation includes crop and independent frame
materialization/release. Its FFmpeg graph and wrappers stay prepared.

All VM runs pass idle-v2, exact pixels including hidden RGB and alpha,
source preservation, independent output and the frozen normal/scalar CLI
comparison. The RGBA8-only candidate passes 126 focused Windows tests
in each of normal, AVX-512-disabled and scalar modes. Current working
capacity and retained storage are unaffected by the dispatch, and physical
peak was unmeasured. Two private core builds and one actor build cover
the trial. The native worker, benchmark harness and package are reused.

The RGBA8-only candidate is private pending final Release-module cost and
package qualification. The existing PNG8 stream decode/orient/crop/fit
control also calls this route, so its complete boundary needs confirmation
on the transfer DLL. The RGBA16 copy loop stays outside the fast path and
its crop performance remains uncertain.
[Current package owned-edit results](OWNED-EDITS-CURRENT-PACKAGE.md)

[Evidence and replay manifests](crop-row-reuse-evidence.json) pin the five
mirrored archives and all 85 members. They retain both sources and compiled
candidates, actuals, guards, checks and commands.
