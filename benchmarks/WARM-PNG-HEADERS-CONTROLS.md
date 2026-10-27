# Fixed-workspace PNG header controls

Current 0.2.1 tooling compares all nine typed source fields and reuses the 21
original RGBA8 structural/EXIF/CRC controls. Normal/scalar managed/native outputs,
bounded acquisition/opaque skipping/EOF, admission/cancellation and independent
lifetimes pass. The complete seekable operation screens at 1.365× / upper1.603,
8,528 allocated B/op. It remains a CPU gap.

The reference is explicitly an original bounded PNG header/EXIF adapter using
public FFmpeg AVIO and owned descriptor APIs. It excludes the complete-packet
parser, CRC and pixels, as the managed header contract does. The control uses
default Strict metadata on one frozen RGBA8 packet; no broader metadata-policy
or source-layout performance claim follows.

[Current results and reproducibility](WARM-READ-RECONCILIATION.md) supersede the
six-field preparation. [Historical evidence](warm-png-headers-controls-evidence.json)
and its cited raw archives remain retained; they are not current timing proof.
