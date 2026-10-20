# Reconstruction search for source-derived WebP segments

Admitting the existing intra16/Y2 and reconstruction search for spatial quantizers
reduces opaque packets **74,962 → 73,740 B** and unrestricted alpha packets
**99,816 → 98,760 B**. Remaining gaps are1,360/170 B against72,380/98,590 B.
The32M-work packet remains exactly104,500 B,5,910 B above reference. Five complete-
packet gates remain open; primary photograph/diagram controls remain passing.

| Existing unrestricted control | Previous → current / reference B | Source-PSNR gap dB | Managed / reference µs | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 74,962 → 73,740 / 72,380 | 0.008682 | 59,760.91 / 15,526.15 | 1,006,048 |
| EncodeOpaque16 | 74,962 → 73,740 / 72,380 | 0.008682 | 60,143.94 / 15,587.76 | 1,006,048 |
| Encode8 | 99,816 → 98,760 / 98,590 | 0.030410 | 67,738.66 / 53,475.79 | 1,537,008 |
| Encode16 | 99,816 → 98,760 / 98,590 | 0.030410 | 68,204.81 / 53,813.37 | 1,537,008 |

Benchmark inputs/settings and native references stay fixed: managed100/native75,
effort4, unchanged0.25dB source-quality/decoder gates and exact alpha. Numeric
quality equality does not establish matching. ALPH is byte-identical18,579 B and
is not retuned. Primary77/75 and the work-limited packet/work/quality retain their
separate C70/C72 evidence. Source quality and reconstructed lossy RGB may change.

The previous factory excluded mixed search whenever the base index was zero,
even when the existing spatial plan selected positive segment indices. It now
offers the existing admitted search and per-segment quantizer/rate scale. If local
work/memory cannot admit it,4x4 full-frame/prefix selection remains available;
offering modes alone no longer disables the limited-work prefix. All selection
precedes shared allocation; refusal/exhaustion/cancellation never retries it.
Existing cache/stack/private-plane ownership and every work charge remain intact.

Both OSes qualify4,688 tests,212internal+12public+6diff scalar checks and the same
actual private0.2.0 package/all examples. Normal/scalar packets and managed pixels
agree acrossOS; depth pairs agree after once-only normalization. New quality100
ownership and native decode controls complement existing positive-base cases.
Entropy-only exact-pixel comparisons now useeffort3 to isolate that primitive;
independent tests cover the lossy search at78/100 and both depths. Two initial
differential test-scope failures were corrected without a production rebuild;
passing internal/public suites were reused, only the changed differential suite
was rebuilt/rerun. Initial failures and final proof remain in raw evidence.

Four guarded three-actual cost screens record the higher CPU cost above. These
verified incremental byte gains are accepted despite that cost; CPU parity and
independent timing repeats remain deferred. Native allocations are unmeasured.
Three unaffected encode costs are retained; no new benchmark workload, calibration,
default option, native reference or quality-tolerance change is introduced.
[Structured evidence](vp8-segment-search-evidence.json) retains source/producer/
package identities, every comparison, failed/final qualification and raw samples.
