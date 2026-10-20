# Current VP8 simple-filter decode

The existing SimpleOpaque8 case remains a current-code gap at
**1.556x/upper2.065x**. This resolves its pre-0.2
admission/disposal uncertainty before optimization.

| Existing complete operation | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Opaque VP8, simple luma filter | 7620.16 | 4898.55 | 1.556 / 2.065 | 760,572 |

All three actuals, native pre/post brackets and both official idle-v2 guards pass.
The original76,128B packet, managed/native frame hashes, exact alpha, normal/scalar
CLI pixels, source and independent owners pass. Native RGBmaximum4/RMSE1.5 is
unchanged. Full decode, conversion and owned-output acquisition/release remain
timed on one CPU; native allocation is unmeasured. Supplied-root performance is
a separate untimed boundary.

The accepted C18 core, original harness/selector, fixture and native worker are
reused without builds, new settings, long jobs or repeated qualification. The
4,475-test/OS and same-actual-package consumer/three-example proof remains valid.
Only this existing row refreshes;32other current rows and204historical rows keep
their identified scopes. Full parity and release readiness remain open.

One current one-second profile retains975samples with both official guards:
coefficient-token reading53.2%, coefficient decoding22.9%. These are profile
shares, not comparative timings. Earlier bulk normalization, wider per-Read
windows, local-state and scoped-work trials remain rejected.

Next: a single block-scoped coefficient reader. Independent recurrence checks
match622,592decisions across2,048initial states. A block needs at most304boolean
decisions;272available partition bytes cover its worst consumption and lookahead.
Only noncancelable calls with that work/input allowance are eligible. Exact state
projection restores the original byte/bit cursor; tails, low work and cancelable
calls keep the original path. This is feasibility, with no production change or
speed claim. [Evidence](warm-vp8-admission-current-evidence.json) retains all
statistics, allocations, identities, profile, proof and mirrored raw hashes.
