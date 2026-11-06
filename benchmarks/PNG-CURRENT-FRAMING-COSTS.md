# Current PNG framing costs

Seven existing comparisons now have independent evidence on the immutable
qualified LPP437 build. Five pass and stop; live RGBA8 and tight RGBA16 retain
demonstrated gaps. No production, fixture, setting or workload boundary changes.

| Complete operation, existing input | Managed µs | Reference µs | Ratio / upper | Managed B/op | Current status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8 decode, live token | 1,539.10 | 1,394.18 | 1.104 / 1.113 | 4,472 | Gap |
| RGBA8 decode, tight working limit | 1,297.38 | 1,395.09 | 0.930 / 0.935 | 68,224 | Pass; stop |
| RGBA16 decode, tight working limit | 3,019.22 | 2,895.06 | 1.043 / 1.052 | 70,272 | Gap |
| Validate PNG8, span | 1,287.37 | 1,377.05 | 0.935 / 0.938 | 240 | Pass; stop |
| Validate PNG16, span | 2,906.21 | 2,961.99 | 0.981 / 0.983 | 240 | Pass; stop |
| Validate indexed Adam7, 19×17 | 1.76 | 3.94 | 0.447 / 0.463 | 792 | Pass; stop |
| Validate PNG16, async nonseekable | 2,956.81 | 3,613.24 | 0.818 / 0.826 | 8,544 | Pass; stop |

Every job retains twelve actuals, both native brackets, filtered/all-actual
confidence views and idle-v2 guards, sequentially on one CPU. Validation returns
an exact independently owned 36-byte metadata descriptor after full coding checks,
without RGBA normalization. Async includes complete nonseekable acquisition/EOF.
Decode includes exact full samples, alpha/hidden RGB, normalization, working storage,
independent output and disposal. Live polling and original tight limits remain.
Native codec/conversion contexts retain their declared preparation. FFmpeg has no
equivalent cooperative-admission API. Startup and checks remain outside timing.

The three decoder comparisons use the original worker
`cfb2403dbdbc4dd4fd1195c64ac64a2fea9c4bc2a0c93a2bd5a38ae004960e2f`, recovered
byte-exactly with its source/build identity into the prepared isolated root. The
current ordinary worker differs after encoder-tool changes; it is not substituted.
Frozen inputs remain 218,775/960,565 bytes. Four validation controls retain the
original natural-frame worker/metadata adapter and their distinct frozen inputs.
Original proof metadata is borrowed only for the current module identity and
restored byte-exactly after each job; every descriptor/native/source gate runs.

The first live decode completed successfully, then an overly strict comparison
against an older reference schema failed: four normal-SIMD readiness fields were
absent there. The current retained reference carries those exact fields. Strict
full equality succeeds against it; completed logs are recovered without replay.
Inputs, worker, output, settings and quality gates are unchanged.

Historical points remain context, not paired causal effects. Tight RGBA16 managed
time falls 3,120→3,019 µs, while native falls 3,129→2,895 µs: ratio movement alone
does not establish a managed regression. Current lower 1.033 confirms a gap.
Word coding validation's 2,806→2,906 µs point is adverse, but upper 0.983 still
passes. No unchanged favorable repeats or retuning of passing controls.

GC totals are per complete operation; peak live capacity and retained pools are
not newly measured. The shared-root peak/pair in the
[prepared-prefix report](PNG-PREPARED-PREFIX.md) belongs to its separate two-result
operation. Do not transfer those figures to these controls or infer CPU effects
from allocations alone. Existing output ownership/admission qualification remains.

The immutable Linux DLL is
`05f302755073e4711cdecd7158ce5aa37165bcbc2bf0fe6f9c67eaedf825befb`.
Reuse Windows/Linux **5,751 full/live checks**, affected normal/scalar and the SAME
actual Release package/all three examples from LPP437. No rebuild, new profile,
matrix expansion, repeated full suite or package proof is required. Published
releases remain unchanged; publication is postponed.

[Evidence](png-current-framing-costs-evidence.json) keeps current scope, points,
guards, reference recovery and qualified-source reuse. Raw
`png-current-framing-costs.tar.xz`: 1,970,652 bytes / 110 members, SHA256
`def1bbc0127a093e3623a3bd50c053d6ea0ac88c3a834deebff3af0fc19afefe`,
mirrored and member-verified with pinned dependencies. Seven displayed numerical
rows refresh; 118 reuse unchanged paths. Historical cursor/indexed/tight studies
remain linked history and no longer supply renderer assertions. Full 283-path
parity remains open; other already-named PNG framing callers still need current
evidence. No full actual-package release refresh or broad PNG pass is claimed.
