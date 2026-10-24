# Current packet-exact encoder costs

E115/E116 reuses the E112 qualified production DLL and four existing513×259
controls. Each case passes a smoke check and one dedicated-VM Release short
screen, one CPU, with both versioned idle guards passing. Inputs/reference
settings remain frozen; encoding, normalization, fresh MemoryStream growth,
independent packet acquisition and release are included. Complete packets match;
JPEG permits only its approved attribution COM replacement. Reconstruction metrics
are sanity checks on matched packets, not an equal-quality-number assumption.

| Existing control | Bytes Lokad/reference | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| JPEG RGBA8 4:2:0 | 52,091/52,093 | 24,301.44 | 1,765.74 | 13.763/14.280 | 372,831 |
| VP8L RGBA8 / libwebp exact=1 | 183,812/183,812 | 605,260.92 | 120,947.64 | 5.004/5.897 | 48,064,592 |
| VP8 RGBA8 | 98,590/98,590 | 251,887.05 | 53,581.59 | 4.701/4.990 | 24,093,944 |
| PNG RGBA8 Optimal6/None | 210,973/210,973 | 25,089.49 | 17,183.73 | 1.460/1.489 | 1,325,678 |

These are current-cost screens, with no optimization, independent confirmation or
parity claim. JPEG is the largest demonstrated gap and the next primitive target.
Other cases remain open; no shape/depth/filter/quality grid is introduced.

Only three benchmark gates were reconciled; production/full-suite/scalar/actual-
package evidence is reused. Two archived BDN hosts are unchanged; the VP8L host
changes only its existing enum parameter before an isolated compile. The VP8L
native helper needed one rebuild for an optional CLI-method argument: its timed
operation source, backend archives, compiler/flags and default settings are unchanged.
Its previous binary/identity/source are mirrored and preserved; exact packets pass.
One VP8 pre-guard I/O rejection ran no measurement; after finishing own writes,
the normal guard and screen passed. No guard threshold or native input changed.

[Evidence](warm-current-encoder-costs-evidence.json) retains identities, allocations,
raw samples, guards, baseline reconciliation and precise qualification reuse.
The benchmark summary also repairs duplicate VP8L rows introduced by an earlier
label edit; all original distinct decoder evidence remains unchanged.
