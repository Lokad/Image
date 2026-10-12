# Rejected RGB16 PNG framing experiments

The same fixed RGB16/None input and complete decode boundary were screened with
two independently implemented changes to the literal framing walk. Neither
demonstrated a useful parity gain; both production changes were reverted.

| Quiet three-actual screen | Lokad.Image ms | Ratio / conservative upper | Decision |
| --- | ---: | ---: | --- |
| Seven-byte refill | 4.467 | 2.004 / 2.362 | Reverted |
| Refill plus four-literal groups | 4.783 | 1.999 / 2.452 | Reverted |

Native brackets also moved between screens, so absolute means alone do not
establish a speedup or regression. Each scored run passes both unchanged quiet
guards; all actuals and the larger filtered/all-actual confidence bound remain.
No twelve-actual confirmation was justified. RGB16 remains open at the earlier
[row-expansion checkpoint](WARM-PNG-RGB16-ROW.md).

Both prototypes pass 2,429 Release tests per OS, scalar framing controls and their
actual-package examples. Eight longer authored literal runs are retained as
tests of stored-block alignment, every truncation, lookahead return and exact
work admission. The production reader was restored byte for byte and rebuilt;
all methods and flags match the accepted package before subsequent work.

Three failed I/O-wait prechecks launched no statistical child and remain failed.
A separate five-second diagnostic recorded zero aggregate/CPU0 I/O wait before
one fresh attempt. Later preparation flushes source/build/archive writes before
the unchanged quiet check; this is outside timing and adds no cooldown. The
[numeric evidence](warm-pngliteral-rejected-evidence.json) retains the raw archives,
prototypes, packages, checks and decisions. The larger measured JPEG 4:2:0 gap
takes priority.
