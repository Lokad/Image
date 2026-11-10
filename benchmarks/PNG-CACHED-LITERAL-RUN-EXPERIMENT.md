# Cached literal-run experiment

PLR778A/B is unselected. Neither complete live RGBA16 decode screen supplies a
useful normalized gain; the three-actual intervals are wide. Production source,
main binaries, public bindings and the verified package are restored exactly.

| Complete existing live RGBA16 decode | Lokad µs | FFmpeg µs | Allocated B/op |
| --- | ---: | ---: | ---: |
| Qualified original | 2,917.74 | 2,896.50 | 8,568 |
| A: two cached short literals | 3,174.03 | 3,121.02 | 8,568 |
| B: prepared lookup/input/output views | 2,921.42 | 2,900.01 | 8,568 |

A groups two prefixes only with at least18 cached bits and two output bytes,
retaining both cancellation checks per symbol, exact partial stores, completed
cursor and fallback ordering. B prepares a512-entry lookup/source view once and
a two-byte output view per group. Only the existing live literal-run method
changes; ordinary, rolling-history, counting, encoding and ownership remain.
No array, owner, pool, session, admission or retained capacity is added.

A's raw increase8.78% accompanies native movement7.75%; its normalized point
is0.96% adverse. B's normalized point is effectively unchanged (+0.005%). These
screens establish neither equivalence nor a useful gain. No unchanged candidate
confirmation, byte-companion timing or broad/package repeat follows. This does
not close materially revised literal-processing designs.

Each candidate passes Windows/Linux normal/scalar checks:712 internal and246
public PNG/admission cases per mode. Three retained tests exercise the actual
production run against independent RFC1951 fixed-code ranges over1024 prefixes,
three phases, all short input cuts and output ceilings, plus guards, completed
state, pre-cancellation, zero allocation and borrowed-root ownership. They pass
against restored production after one embedded-only rebuild per OS; no obsolete
producer is retained for tests.

Two main/embedded candidate versions per OS, zero compile failures and zero
harness/native/selector/fixture builds serve the round. A completed original
capture encountered a receipt-only manifest-path error; its log, guards and
validation were recovered without rerunning measurement. All actuals and adverse
results remain. Sequential CPU0, idle-v2 guards, frozen input/settings, exact
full-depth pixels/alpha/hidden RGB and independent output gates pass.

Unchanged [LWP776C](LANCZOS-PAIRED-WORD-ROWS.md) qualification remains valid:
6197 full/live checks per OS, resize ISA and SAME actual package/all three usage
examples. The three added tests have separate focused evidence; this is not a
new6200-check full run. Current51 PNG costs remain49pass/1gap/1uncertain.
Displayed timings and renderer dependencies do not change.

[Evidence](png-cached-literal-run-experiment-evidence.json) records both designs,
immutable versions, source scope, restoration and retained tests. Raw
`png-cached-literal-run-experiment.tar.xz`, SHA256
`fffe693e7d8f130b6f6518f9631e8adbd26c194418eb68608736296c05fa007f`,
10,565,992 B/224 members, is mirrored and member-verified. The next task attributes
cached-wide bicubic preparation versus execution before another kernel change;
native retained graph boundaries stay explicit. Publication remains postponed.
