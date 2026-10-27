# Lanczos3 warm baseline

Four required paths now have quiet three-iteration screens on the existing
513×259 RGBA8/16 inputs. Production is unchanged from the
[functional qualification](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/docs/lanczos3-qualification.md). All four remain
material gaps against the **≤1× independently confirmed** target; no long
confirmation was run above one.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Reduce8 →256×129 | 7,397.17 | 493.31 | 14.995 / 15.681 | 146,616 |
| Reduce16 →256×129 | 7,412.73 | 506.33 | 14.640 / 15.438 | 146,616 |
| Enlarge8 →1027×519 | 74,429.21 | 4,082.73 | 18.230 / 19.052 | 351,966 |
| Enlarge16 →1027×519 | 74,799.35 | 3,915.33 | 19.104 / 19.532 | 4,616,094 |

The explicitly labeled reference is **zimg, FFmpeg's zscale backend**, radius
three, encoded sample space. Timed work includes packed input normalization,
an eight-pixel clamped halo, filtering, fresh planar/packed output allocation,
unrounded-alpha division, quantization and release. Graph/input/scratch contexts
are reused; converted inputs and outputs are not cached. Managed measurements
include complete public Resize, admission, materialization and disposal. Both
subjects use one pinned CPU and normal SIMD; startup, IPC and validation are
excluded. Default managed pixel reuse explains the allocation difference between
the two enlargement depths: the 16-bit result exceeds the 4 MiB idle-pool cap.

All four full managed outputs match the independent decimal/Taylor-sine and
BigInteger oracle on both operating systems. Native normal/scalar controls
pass RGB/alpha maximum error 1 and RMSE ≤0.25, including zero-alpha normalization,
source integrity and simultaneous independent outputs. Twelve rejection controls
per case pass. No exact managed pixels or native quality gates were relaxed.

The actual generated child reuses the verified immutable producer and benchmark
assembly. Its entry, runtime configuration and loaded assemblies match before
and after every screen. Eight versioned idle/process guards pass; both native
brackets contain three samples. The upper is the maximum of selected-result and
all-actual confidence views. Production/package qualification is reused:
4,426 tests and 25 scalar Lanczos3 checks per OS, the same actual private 0.2.0
package consumer and all three usage examples on both systems.

A bounded diagnostic profile highlights wide integer division and final
normalization as the next primitive. Its samples are diagnostic; profile timings
are not scored. The next experiment should certify an estimated quotient with
exact integer remainders and retain the exact division fallback, then screen
these same four cases. Other filter kernels and all 227 earlier numeric rows
remain unchanged, with evidence scoped to their identified builds.

[Numeric evidence](warm-lanczos3-baseline-evidence.json) retains samples,
allocations, quality controls and identities. Full raw archive
`l3-baseline-qualified-linux.tar.gz` has SHA-256
`5988834a4415b3897e356f11df0514eaa29ede34829216cb0a411270904a7634`.
The exact runtime, generated child, worker, inputs, outputs, guards and profile
remain archived. Published 0.1.0 is unchanged; replacement readiness remains open.
