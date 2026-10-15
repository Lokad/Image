# Rejected VP8 token-local reader trial

A reader copy scoped to an entire coefficient token produces no useful gain.
The three-actual opaque VP8 screen is **9,542.96 us**, versus **9,512.66 us** for
the current baseline: **0.32% slower**, with unchanged **760,548 B/op**.
The trial is rejected and production restored. Opaque VP8's earlier independent
conservative upper **2.004x** remains open.

| Three-actual screen | Lokad.Image us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Qualified baseline | 9,512.66 | 1.984 / 2.076 | 760,548 |
| Token-local candidate | 9,542.96 | 1.994 / 2.135 | 760,548 |

Only `Vp8BooleanReader.CoefficientToken` changes. It copies the reader once,
performs the original token decisions on that local copy, and publishes the
exact partial state in `finally` on success or failure. This scopes the copy
across a complete token, unlike the earlier rejected per-Read normalization
field caching. Bit recurrence, validation, work, cancellation and probability
access order remain unchanged. Twenty-nine focused arithmetic controls pass
on Windows and Linux for both candidate and restoration, comparing result,
exception type/message, every cursor field and remaining work.

Both measured subjects retain exact frozen opaque pixels, alpha, source and
independent owners. Input, native worker, harness and generated driver are
unchanged; only the production DLL differs. CPU0, one codec/conversion thread,
normal SIMD and the full decode/output/release boundary remain fixed. Both
versioned pre/post quiet guards pass for both screens. All three actuals are
retained. Native allocations are unmeasured. No new original image, profile,
quality adjustment, longer candidate repeat or full qualification is introduced.

Before restoration, both complete raw archives retain source, input, native
worker and all four actual child files. Restoration returns the original source
inventory and byte-identical qualified Linux DLL to production and benchmark
locations; focused tests pass again on both systems. Existing cross-platform
Release/live/package qualification remains applicable. No repeated whole-library
method audit or package proof is needed.

The [numeric evidence](vp8-token-local-rejected-evidence.json) retains every
sample, guard, identity and restoration proof. Do not repeat this representation
unchanged. Other material public/depth boundaries and uniform current-source
coverage remain open.
