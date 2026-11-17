# Pinned copy-alignment trial

**Unpromoted:** complete original RGBA16 forward/reverse copies measure
**6.085311 → 6.888712 µs**, **13.20% /0.803401 µs slower**, ratio interval
**1.101823–1.163052**. GC allocation remains **0 B/op**. Stop without
confirmation, generalization, standalone-reference refresh or broad/package repeat.
Qualified copying, ownership and current costs remain unchanged.

The [retained native trace](COPY-ATTRIBUTION-NEAREST-GUARD.md) proves the actual
reference reaches aligned AVX-512 stores. An untimed diagnostic finds 113/129
forward destination rows unaligned on both OSes. It verifies 1,408 bounded prefix
cases, address stability through forced compacting GC and prompt pin release.
Those facts establish an alignment strategy, not a speed forecast.

The isolated implementation pins the borrowed canvas once per complete public
call, computes its actual alignment, copies at most 63 prefix bytes and reuses
the existing grouped kernel. Up to three remaining vectors precede the ordinary
short tail. All access uses checked spans; the address is used only for alignment
arithmetic. There is no unsafe code, pointer dereference, external native binding,
dependency, new pixel buffer, cache or pool. Scalar, small and same-row copying
retain their existing selection. The fixed general guard is disjoint rows,
at least 512 B/row and 64 KiB total; no fixture identity is consulted.

Pin creation, address calculation, prefix/tail handling and Free are timed.
Generated code realizes them: caller **1,751 B**, aligned helper **671 B**, with
normal/exception cleanup. The complete result is adverse; this does not isolate
pin overhead from copying, dispatch or changed code generation. No neighboring
pinning/group/tail implementation is selected from this screen.

Both OSes pass **21 internal/190 public editing/admission checks per normal/scalar
mode**, including 1,024 prefix/group/tail cases, canaries, unchanged inputs, pin
ownership and subsequent budget reuse. Linux's original worker passes full-word
snapshots, alpha/hidden RGB, cancellation/refusal/recovery and source integrity
on both DLLs/modes. Three reversed alternating 16,384-operation pairs, equal
warmups, CPU0, natural nonconcurrent GC and idle-v2 guards remain. Every actual is
retained. This paired DLL diagnostic is not FFmpeg performance.

Caller image/budget ownership is borrowed; only the short runtime pin is owned
and freed in finally after actual users. No pixel capacity or cross-operation
retention is added. Fixed minor handle metadata is distinct from GC allocation;
its runtime/native capacity and physical/JIT peaks are unmeasured. Zero GC bytes
does not mean zero total runtime allocation.

One small diagnostic, one isolated Windows core and one embedded-test build;
zero VM, canonical/public-test, actor/harness/native/fixture/package builds.
Record serialization and an archival absolute-path normalization error resume
only preparation/verification; no completed build, test or timing repeats.
The first archive is retained. Reuse **6,790 full/live checks**, the same actual
package and all three examples each OS.

[Evidence](pinned-copy-alignment-trial-evidence.json) pins sources, all actuals,
generated code and ownership/qualification reuse. Mirrored/member-verified raw
`pinned-copy-alignment-trial.tar.xz`: **4,502,064 B /86 members**, SHA256
`a384e8034d694b2265d3b4887c0ddca451fb6db1927d536e9a8f33d9912a20f0`.
Archived `pba1181-*` recipes reproduce the bounded round. It supplies no current
displayed costs or renderer dependency. Publication remains postponed.
