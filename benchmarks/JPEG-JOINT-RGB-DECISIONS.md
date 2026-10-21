# JPEG coupled RGB category decisions

C132 considers smaller AC amplitude categories across one co-sited 4:4:4 MCU.
It prices changes with the existing geometric AC model and models RGB error through
the library's inverse BT.601 matrix. Added modeled error is capped at 1/32 of the
initial MCU error. Original DQT/DC and stable ties remain. The model precedes
clipping/rounding; measured reconstruction quality remains decisive.

| Existing 513×259 control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 155,732 | 150,479 / 133,041 | 0.225345 dB |
| Original 4:4:4, RGBA16 | 155,795 | 150,395 / 133,043 | 0.232559 dB |

Savings are 5,253/5,400 B. Reconstruction changes remain within the unchanged
0.25 dB gate; alpha is exact and native/public decoder differences stay at most
3 with RMSE below 0.229. Actual production packets exactly reproduce the independent
C131 prototypes on both platforms and normal/scalar paths. Frozen inputs, reference
settings and normalization remain unchanged. These procedural controls do not
establish photographic/diagram compression claims; size parity remains open at
about 13.1% excess. The useful C130 category-only candidate is retained, superseded
by the better coupled strategy; both experiments precede this qualification round.

Three-block scratch uses 2,304 bytes within the existing admitted 8 KiB workspace,
without a frame copy. Sampling stays unchanged; emission follows joint planning.
Initial error/prices charge 192 units each, every trial charges 64 before mutation,
and work is bounded by 121,344 per MCU (40,448 per block). Local admission includes
that bound. Shared refusal remains terminal. Ten focused joint cases cover signs,
nonzero shrinking, coupled error reduction, DC/tables, work refusal and trial
restoration, cancellation and the maximum work bound. Existing nested/root,
stream-failure, quota and independent-output checks pass with the new decisions.

Windows and Linux each pass 4,917 Release tests (2,008 internal, 1,973 public,
936 live FFmpeg), plus 97 internal and 53 public scalar checks. The same actual
private 0.2.0 package and all examples pass on both platforms. One immutable
production build/platform is reused; differential, benchmark, native and driver
assemblies are reused. Quality is reused by exact packet identity; no new reference
preparation or quality search is performed. The [19-row ledger](JPEG-COMPLETE-PACKET-AUDIT.md)
still has five default passes and thirteen open cases. Optional CompactColor stays
separate; passing 4:2:2, grayscale, restart 4:2:0 and WebP controls remain stopped.

One quiet-VM three-actual forward-only 4:4:4 screen records **55,068.595 µs /
reference 3,824.019 µs**, ratio 14.401/upper 45.954, **563,802 B/op**. Both idle-v2
guards pass. Native allocations are unmeasured. This is a short cost observation;
no CPU-parity or independent confirmation is claimed. Normalization, decisions,
admitted entropy work and fresh wrapper/output ownership/release are timed on one
CPU. The reused harness's historical `fixed-geometric-v1` text is only a selector;
the actual producer uses coupled decisions and adaptive Huffman coding.

[Numeric evidence](jpeg-joint-rgb-decision-evidence.json) retains producers,
quality reuse, allocations and raw statistics. Mirrored raw archives are
`c132-qualified-linux.tar.gz` (2,749,542 B; SHA256
`f883e3449d83d2eea2149eef36243ce1f6a16acb3097aa12c3b684bf27e52719`) and
`c132-qualified-windows.tar.gz` (2,737,451 B; SHA256
`7558e1627f0c46ac1bee70571847ed62bec012300ba6f54fd538ba00e6455242`).
