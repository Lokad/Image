# PNG chunk checksum folding

Updated 2026-10-17. Stored RGBA8 encoding is **13.1% faster** than its retained
357.17 µs predecessor; stored RGBA16 is **14.5% faster** than the current accepted
build control. Both remain warm single-CPU parity gaps. All 16 existing affected
PNG encodes were refreshed: ten independently reconfirm upper≤1; six retain
qualified three-actual screens. No workload, raster or settings grid was added.

| Existing encode | Managed µs | Reference µs | Ratio / upper | Managed B/op | PNG bytes managed/native | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| RGBA8, level 6 / None | 5,406.30 | 17,266.72 | 0.313 / 0.315 | 527,627 | 218,775/210,973 | Parity confirmed |
| RGBA8, NoCompression / None | 310.52 | 191.23 | 1.624 / 1.854 | 2,102,474 | 531,971/533,444 | Gap (short; qualified) |
| RGBA16, NoCompression / None | 674.12 | 405.05 | 1.664 / 1.753 | 4,202,998 | 1,063,615/1,066,552 | Gap (short; qualified) |
| RGBA8, Fastest managed2/native1 / None | 3,053.16 | 4,355.98 | 0.701 / 0.704 | 527,694 | 231,465/236,828 | Parity confirmed |
| RGBA16, Fastest managed2/native1 / None | 13,289.83 | 17,822.40 | 0.746 / 0.776 | 2,103,738 | 963,045/968,216 | Parity confirmed |
| RGBA16, Fastest managed2/native1 / live cancelable token | 12,846.40 | 17,513.32 | 0.734 / 0.743 | 2,103,738 | 963,045/968,216 | Parity confirmed |
| RGBA16, Fastest managed2/native1 / forward-only output | 12,790.89 | 17,465.07 | 0.732 / 0.740 | 2,103,770 | 963,045/968,216 | Parity confirmed |
| RGBA16, level 6 / None | 14,604.45 | 20,095.18 | 0.727 / 0.731 | 2,103,624 | 960,565/963,202 | Parity confirmed |
| RGBA8, SmallestSize9 / native9 | 14,486.25 | 17,324.40 | 0.836 / 0.843 | 527,564 | 209,407/210,973 | Parity confirmed |
| RGBA16, SmallestSize managed9/native9 / None | 19,610.01 | 20,110.26 | 0.975 / 0.980 | 2,103,522 | 960,650/963,202 | Parity confirmed |
| RGB8, Optimal managed9/native6 / None | 10,442.38 | 9,493.05 | 1.100 / 1.165 | 527,032 | 163,358/168,729 | Gap (short; qualified) |
| RGB16, level 6 / None | 13,163.23 | 15,313.53 | 0.860 / 0.863 | 2,102,732 | 737,426/739,472 | Parity confirmed |
| gray8, Optimal managed9/native6 / None | 3,547.97 | 1,940.70 | 1.828 / 2.025 | 263,664 | 72,373/72,586 | Gap (short; qualified) |
| gray16, level 6 / None | 5,762.37 | 4,913.26 | 1.173 / 1.246 | 1,051,421 | 264,939/265,433 | Gap (short; qualified) |
| gray-alpha8, Optimal managed9/native6 / None | 5,907.01 | 4,295.19 | 1.375 / 1.966 | 264,179 | 120,036/120,366 | Gap (short; qualified) |
| gray-alpha16, level 6 / None | 9,426.43 | 10,150.04 | 0.929 / 0.933 | 1,052,317 | 505,483/506,887 | Parity confirmed |

The original 513×259 RGBA8/16 rasters and established RGB/gray/gray-alpha projections
are unchanged. Complete public EncodePng includes fresh MemoryStream creation,
growth, encoding and disposal, without redundant ToArray; live-token and
forward-only cases retain their complete wrappers. Native encoding includes the
frozen conversion/packing/filter boundaries and independent owned packet
acquisition/release. Frozen levels, None filter, exact samples/alpha/hidden RGB,
packet hashes, IDAT sizes and the 5% IDAT gate all pass. Native allocations remain
unmeasured; managed B/op includes harness amortization. No compression-policy change.

One six-second current profile placed 34.6% of 2,147 userspace samples in chunk
CRC. These fractions are attribution, not wall-time measurements. A serial shuffle
prototype was flat at 358.13 µs and rejected. The accepted primitive reuses the
existing independent polynomial kernel: payloads ≥64 bytes use four independent
folding residues, a chunk-type seed, ordered combination and one final 32-bit
append. Smaller payloads and unsupported hardware retain the original path.
No new fields, tables, heap workspace, dependencies or unsafe code are added.

Removing the two-line dispatch restores the entire previous Crc32 file. Removing
the new seeded entry point and normalizing its class summary restores the entire
previous PngIdentificationCrc32 file. Existing arithmetic, metadata polling, write
sequence, work, cancellation, admission and ownership remain unchanged. Only the
16 PNG encodes are affected; **191 other comparisons, including 42 passing proofs,
are reused**. All ten refreshed passing cases are stopped.

The historical stored RGBA16 mean of 504.25 µs was not reproduced. One short
control using the current accepted DLL, same harness/selector/reference/fixture
and no rebuild measured 788.36 µs; the candidate measured 674.12 µs. Both quiet
guards and exact checks pass. Both records remain; no word parity or regression
claim is inferred from the older result. Short bounds, including gray8 upper
2.025, remain visible; gaps receive no routine twelve-actual confirmation.

All three actuals for each screen, all twelve for each independent confirmation,
both native brackets and both exclusive-vm-idle-v2 guards are retained. Completed
jobs initially refused for sample-count or descriptive-wording mismatches were
strictly reconciled without statistical repeats or changes to typed settings,
packet hashes or quality gates. Historical/current native identities and their
bounded source/build-script reconciliation are recorded. The same immutable
production, harness, workers and fixtures served the affected campaign.

All **3,639 Release cases per OS** pass (1,337 internal, 1,404 public, 898 live
FFmpeg), plus 83 scalar CRC and 49 scalar public PNG controls per OS. Six new
independent bitwise-recurrence theory cases exercise every byte value at each
position across vector, dispatch, multi-lane and tail boundaries. The same actual
Release package passes its consumer and all three examples on Windows/Linux;
symbols match the qualified DLL. Linux production bytes match the screened
build through qualification. Unaffected scalar proofs are reused; released
0.1.0 packages and tag remain intact.

[Numeric evidence and archive hashes](warm-png-chunk-crc-evidence.json) retain
all statistics, packets/sizes, receipts, rejected prototype, current-build word
control, profile, source identities and package qualification. Main raw archive:
`parity-p19-qualified-evidence.tar.gz`, SHA256
`058bbb82c2cfe6665b3604a11b13ff910aa9e9f66a03b4baee9a47c83349fdbc`.
