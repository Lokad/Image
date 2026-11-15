# Refill completion inlining

The small,loop-free CompleteFill routine now requests JIT inlining. Removing the
namespace import and attribute reconstructs the entire original reader body:
every read,check,counter update,poll,pending helper and ownership boundary is
verbatim. No new method,storage,cache,owner,API or dependency.

Independent complete async JPEG pairs save **3.89% /10.22 ns**:
**.262732 →.252515 µs**,interval **.949840–.972447**,still **568 B/op**.
This is a scoped DLL gain,not FFmpeg parity or a percentage added to prior stages.

| Original complete boundary / labeled FFmpeg adapter | Lokad / reference µs | Conservative upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| ProbeDimensionsPng | 0.117 / 0.122 | 0.997275 / pass | 568 |
| ProbeDimensionsWebp | 0.118 / 0.122 | 0.992660 / pass | 568 |
| ProbeDimensionsJpeg | 0.226 / 0.192 | 1.227279 / gap | 568 |
| ProbeDimensionsJpegAsync | 0.252 / 0.201 | 1.278172 / gap | 568 |
| PngHeaders | 0.294 / 0.573 | 0.524641 / pass | 568 |

All five current views use the same actual immutable timed/package DLL,twelve
actuals,both native brackets and quiet guards. Frozen inputs,acquisition,exact
independent descriptors and disposal remain. Async callbacks complete immediately;
suspended users are covered by ownership tests. WebP dimensions now passes/stops;
PNG dimensions/headers pass. JPEG probes remain gaps. Sync/JPEG-header paired
guards are inconclusive,with no gains claimed for those paths.

Current actual benchmark contexts verify the hint:the refill caller grows
**973 →1181 B**,its completion call sites fall **1 →0**,and both prologues
subtract **80 B**. This is not a total stack footprint or CPU-phase attribution;
instrumented times are discarded. The earlier rejected field-reader hint grew
large generic parsers;this targets the241B leaf after reader consolidation.

**Windows/Linux each pass6,721full/live checks**,10internal/148public focused
normal/scalar and the **SAME actual Release-package consumer/all three examples**.
One core/embedded build perOS,one pack,no public/harness/native/fixture build or
post-timing accepted-producer rebuild. Affected performance is reconciled before
the single broad/package milestone;unchanged production paths reuse valid evidence.
No new fixtures,quality/precision relaxation or favorable timing retry.

Admission stays **512B dimension /8448B bulk maximum**,with no added capacity,
cache or retained operation state. Total allocation,logical capacity and unmeasured
physical/native/JIT peaks remain distinct. Published packages remain immutable;
publication and the full289-path objective remain open.

[Evidence](refill-completion-inlining-evidence.json) retains producer identities,
samples,actual codegen,source reconstruction and package qualification. Actual
package `f7b691dd41b1937162f24c154cdd2a0dbe4acd8e1b65295e1232fbb16e865933`,465,592B,contains
timed DLL `8d186aaa377681a5879d3b29990357bff88ef83e0471775027e6a8b6496e3fd6`. Raw
`refill-completion-inlining-qualified.tar.xz`:4,609,016B/176members,SHA256
`ebd7636b270f0fe6aadab0a9a015bedb5dc7f25bf5144afc0df781eb8a720681`,mirrored/member-verified. [Reader consolidation](STREAM-READER-CONSOLIDATION.md)
retains the prior architecture/profile;[direct publication](PROBE-RESULT-PUBLICATION.md)
retains four unchanged complete Identify costs. [Matched resident PNG batches](PNG-RESIDENT-IDENTIFY-ADJACENT.md)
now own the fifth, resolving its uncertainty without a production change.
