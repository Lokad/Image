# Current PNG stream and composition costs

The final five named PNG costs are refreshed. Synchronous PNG16 passes and stops;
the other four remain gaps. Production is unchanged; no historical speedup claim.

| Complete operation, 513×259 input | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Seekable RGBA8 | 1,552.245 | 1.112 / 1.115 | 4,568 | Gap |
| Seekable RGBA8 + crop/fit | 1,694.517 | 1.147 / 1.154 | 701,264 | Gap |
| Seekable RGBA16 | 2,934.460 | 0.952 / 0.958 | 8,664 | Pass; stop |
| Seekable RGBA16 + crop/fit | 3,324.533 | 1.124 / 1.134 | 1,401,346 | Gap |
| Async nonseekable RGBA8 | 1,520.153 | 1.075 / 1.085 | 12,776 | Gap |

All 12 actuals/control, native brackets, filtered/unfiltered confidence envelopes
and idle-v2 guards are retained. Original packets/rasters/settings and all five
reference workers remain byte-identical. Both subjects include input acquisition,
copies, EOF, complete decoding, normalization, output ownership and disposal.
Crop/fit includes normal orientation, a centered 256×129 crop and nearest fit to
127×64, with all managed intermediates; the approved native graph can use views.
Native word decode includes BE-to-LE ownership; native word composition converts
selected planes and packs its final output. Native allocations are unmeasured.

One benchmark-harness rebuild fixes an untimed preparation defect: word inputs
now read the original packet instead of regenerating it with changed encoder
defaults. Production, selectors and native workers were not rebuilt. Completed
byte timings were reused without reruns; zero-actual setup failures remain raw.
The existing 21 async controls pass current-core/normal/scalar native and CLI
checks with only producer identities changed. The temporary shared proof is
restored byte-for-byte. Reuse 5,129 checks/OS and the same actual package/examples.

The planned PNG cost refresh is complete. Passing paths stop; demonstrated gaps
and open confidence bounds remain. The full 268-path parity objective stays open.

[Evidence](current-png-stream-composition-costs-evidence.json). Mirrored and verified
archive current-png-stream-composition-costs.tar.xz, 275 members,
SHA256 9076bf9b00e8576885fad8dda131d69223425d63c730c5281daafb52c995d29f; qualified-build dependency pinned by hash.
