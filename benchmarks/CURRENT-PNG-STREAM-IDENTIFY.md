# Current complete-stream PNG Identify

The existing seekable stream screens **1.425×/upper1.535**,192B/op. The existing
nonseekable async stream independently measures **1.238×/upper1.271**,8400B/op.
Both are gaps on this frozen input; no production optimization is made.

| Existing control | Lokad µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| PngSeekable, short | 22.674 | 15.915 | 1.425 / 1.535 | 192 |
| PngAsync, short | 23.499 | 20.534 | 1.144 / 25.072 | 8,400 |
| PngAsync, confirm | 26.586 | 21.477 | 1.238 / 1.271 | 8,400 |

Both use the same218775B original513×259RGBA8 PNG, default limits, one warmCPU.
All nine metadata facts agree, including source color/alpha/components. Actual
input acquisition, copies/growth, EOF, fresh source and independently owned
metadata plus release remain inside each operation. Native references are the
original FFmpeg AVIO/public-parser/CRC/full metadata adapters. Seekable includes
length acquisition; nonseekable uses immediate callbacks and private growth,
without seek/length/input view. Neither side decodes pixels. Work counters are
removed while meaningful input/container/dimension/metadata bounds remain.

Each boundary's existing21same-packet controls pass with managed intrinsics
on/off. Async also compares native normal/scalar, actual ReadAsync-only delivery,
EOF, source bytes, zero sync/seek/length queries and caller-open behavior. Runtime
gates retain admission, cancellation, source integrity and independent outputs.
One shared harness/two native builds reuse the existing short/confirmation
selectors. The temporary six/nine-field tooling option is removed: all five
existing Identify reference boundaries now require nine fields.

The ordinary seekable gap needs no extra confirmation. Async's three-sample
before bracket varied19.527/20.434/21.640µs, giving a very wide short upper25.07.
One independent12actual observation resolves that uncertainty; all12are retained
and filtered. Every native bracket and idle-v2 guard passes. Earlier short data
remains; differing same-producer means are not a speedup/regression claim. Native
allocation and admission peaks are unmeasured here.

Production source, immutable DLL and actual private0.2.1 package are unchanged;
prior5052checks perOS and same-package consumers remain applicable. No production
build, full-suite or package proof is repeated for tooling.
[Evidence](current-png-stream-identify-evidence.json) retains samples, controls,
identities and the mirrored/member-verified489-member raw master.

Earlier six-field stream comparisons are superseded. Resident PNG's screen and
independently passing JPEG/WebP controls remain applicable. This does not qualify
other formats/delivery/network/concurrency or the pending default-decoder cases.
Full267-path warm single-CPU ≤1× remains open; larger material encoding gaps keep
priority over small stream variants.
