# Direct dimension-field assembly

Dimension-only probes reuse their existing 256-byte scratch for partial fields,
removing a separate 32-byte array and each buffer-to-header copy. Complete calls
allocate **688 rather than 744 B/op**, with the same **512 B admitted capacity**
and **zero retained scratch**. Total allocation, admitted capacity and unmeasured
physical/native/JIT peaks remain distinct.

The same immutable Release DLL confirms all five affected comparisons on the
dedicated quiet VM, one CPU, all 12 actuals and both native brackets retained:

| Complete operation | Lokad µs | Native adapter µs | Ratio / upper | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Png | 0.144 | 0.122 | 1.176 / 1.204 | 688 | gap |
| Webp | 0.143 | 0.121 | 1.177 / 1.205 | 688 | gap |
| Jpeg | 0.323 | 0.192 | 1.685 / 1.805 | 688 | gap |
| JpegAsync | 0.385 | 0.202 | 1.905 / 1.991 | 688 | gap |
| PngHeaders | 0.311 | 0.579 | 0.537 / 0.550 | 592 | pass |

These use the frozen original inputs and **FFmpeg AVIO plus the labeled bounded
header adapter**, not the public codec parser. Fresh source, scratch, independent
descriptor ownership and disposal remain timed. Dimension probes acquire only
the required prefix; complete PNG headers scan framing/EXIF and EOF while skipping
opaque payloads. Neither boundary validates compressed pixels or PNG CRC.
Async uses immediately completed resident callbacks; held-read tests establish
actual suspended-user ownership separately. No cached result or whole-input copy.

PNG/WebP conservative native-normalized intervals separate from the preceding
independent confirmation. JPEG CPU effects remain inconclusive; allocation falls
on all four. All four probes remain small absolute gaps. Complete PNG headers
remain passing; these results do not establish whole-library parity.

Field assembly retains the explicit 32-byte field limit, efficient 256-byte
forward draining, acquisition/cursor/error precedence and cancellation. A partial
field is filled at its assembled offset only in dimension-only mode; other facts
and complete inspection keep independent header assembly. No extra read state
machine, session, pool, retention or memory-admission model.

Windows/Linux each pass **6,642 full/live checks**: 3,248 internal, 2,446 public
and 948 live differential. Focused normal/scalar checks pass 1 internal and
175 public cases per mode/platform. Mixed immediate/pending one-byte reads cover
dimension and full metadata requests. Cancellation/failure after a first fragment
keeps scratch admitted through the actual user, then releases it and permits a
subsequent operation. Borrowed roots/inputs remain open and no refusal retries.
The **same actual Release package** and three examples pass both platforms,
including new packaged fragmented-probe and held-read checks. Published releases
and the preceding private package remain unchanged.

One immutable production build per platform covers this round. Existing cached
harnesses, fixtures and native workers are reused; affected performance is
reconciled before one broad/package milestone. Unchanged codec/edit/resize/text
qualification is reused. Publication is postponed.

[Evidence](direct-probe-fields-evidence.json) records producer/package identities,
all-actual statistics, costs, source reuse and the mirrored member-verified raw
archive. [Earlier header/probe evidence](SEEKABLE-HEADER-STORAGE.md) is retained as
history. The short-screen console label said twelve actuals in error; its raw
logs/receipts contain three, while every independent confirmation contains twelve.
No measurement was repeated for that label.
