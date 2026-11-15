# Direct JPEG selector trial

**Not retained.** Returning the JPEG parser directly after an immediately
completed signature read removes the nested internal selector state machine,
but the existing three-pair screen shows no useful gain: **0.234933
to 0.236088 µs**, ratio interval **0.879903–1.147685**,
**568 B/op** on both sides. This is inconclusive, not evidence of a regression
or standalone FFmpeg parity. All pairs and original gates remain.

The complete public operation includes original input delivery, bounded prefix
acquisition, exact descriptor construction, reader disposal and independently
owned output. Both public callers remain async owners awaiting the returned
parser; internal immediate exceptions retain public fault/cancellation semantics.
The first read is consumed exactly once. Pending signatures use one selector
state machine; all format grammars and reader bodies remain verbatim. No cache,
pool, API, workspace or fixture-specific path. Pending state carries a ValueTask
and could allocate a larger box; that cost was not measured or adopted.

Linux passes **10 internal/148 public** normal/scalar checks, including mixed
completed/pending reads for all formats, hostile/truncated framing, exact metadata,
immediate failures/tokens, cancellation during uncooperative reads, refusal,
held admission until actual completion, cleanup and later successful reuse.
One preflight guard refused CPU0 activity above its 5% bound **before timing**;
its receipt remains. Fresh before/after guards pass for the sole screen.

Stop without confirmation, other controls, native/codegen capture or broad/package
qualification. Exact qualified source/core/embedded/public modules and source
inventory are restored without rebuilding. Windows modules and actual package
never change. Reuse **6,721 full/live checks per OS** and SAME actual-package/
three-example proof. One Linux core/embedded build; zero other builds.

[Current costs and qualification](REFILL-CALL-INLINING.md) remain unchanged.
[Evidence](jpeg-selector-direct-trial-evidence.json) pins source, tools and restoration.
Raw `jpeg-selector-direct-trial.tar.xz` (1,783,336 B/74 members), SHA256
`62781c5700b694ae8fbb9055aca31813afb7bba50465d583e8b41266953c7e69`, is mirrored and member-verified, with its qualified baseline
dependency retained. This trial adds no renderer dependency. Materially revised
designs remain possible; no unchanged favorable replay or permanent family ban.
Full 289-path completion remains open; publication stays postponed.
