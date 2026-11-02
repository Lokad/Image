# WebP matching phase attribution

One warmed diagnostic on the existing 513×259 transparent lossy control separates
matching setup, chain construction and search. Production source and the qualified
DLL remain unchanged. This is an instrumented source projection, not a benchmark
or a CPU gain claim.

| Full-frame matching call | Setup µs | Chaining µs | Search µs | Search fraction |
| --- | ---: | ---: | ---: | ---: |
| first | 658.14 | 339.90 | 4,152.01 | 80.6% |
| second | 24.06 | 345.03 | 5,322.43 | 93.5% |

Three additional 153-pixel calls take 5.7–7.5µs each. All five matching calls use
quality32 with low-effort disabled. Fractions describe these observations only;
they are neither qualified component costs nor a bound on available savings.

Four timestamps surround existing phases; no pixel loop is instrumented. One
complete warmup and one observed encode both preserve the exact 98,590-byte packet
(SHA256 8fcc1c935850d7813ebfce329a8b00c41fe9b02683d2be40b91dedfd74192a10),
source pixels and shared-root cleanup. Both quiet guards pass. One private tool
build; zero production, harness, selector or native builds. Reuse the current
[qualified package](WEBP-CACHE-PREFIX-AGGREGATION.md) and the restoration checks in
the [cache-tree trial](WEBP-CACHE-TREE-INCONCLUSIVE.md).

Next, inspect exact repeated-run opportunities in the actual transformed search
inputs. A useful redesign could reuse hash-head storage after chaining rather
than allocate another full-frame array. It must preserve every match-index word,
candidate order, tie and complete packet. This lead does not authorize replaying
the inconclusive chunked-prefix trial or claiming gains from allocation alone.

[Evidence](webp-matching-phase-audit-evidence.json) includes source/runtime
identities, all phase observations, exact output and ownership checks. Raw
webp-matching-phase-audit.tar.xz: 612,404B /31members, SHA256
82296a211d83215ce9dc836ed7392128d6c3aaa785f8dbb8f9c64c19b7651615,
mirrored/member-verified. Qualified dependency: webp-cache-prefix-aggregation.tar.xz,
SHA256 34e5bbd505797c124e14330bdfaa3e1cb0992c3c6f04a7fc9861d7b10e28cd06.
