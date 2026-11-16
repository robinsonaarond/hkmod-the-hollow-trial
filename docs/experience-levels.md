# Character level balance — 0.13.0

Level 1 starts at 0 XP. Each level advance costs `10 * currentLevel` XP.
Cumulative XP to reach level L is `5 * L * (L - 1)`. These formulas apply to
every level; there is no per-level or per-wave threshold table in the rules.

The unboosted campaign awards 11,565 XP. Exactly 43/47 waves grant one level; the remaining
four grant zero or two. At every wave clear, level stays within one of wave
number + 1. Final level is 48. Existing formations and reward logic are unchanged.

| Cleared wave | Level | Total XP | Cost from current level to next |
| ---: | ---: | ---: | ---: |
| 1 | 2 | 15 | 20 |
| 5 | 5 | 140 | 50 |
| 10 | 12 | 695 | 120 |
| 15 | 17 | 1425 | 170 |
| 20 | 22 | 2430 | 220 |
| 25 | 27 | 3545 | 270 |
| 30 | 31 | 4870 | 310 |
| 35 | 36 | 6500 | 360 |
| 40 | 41 | 8390 | 410 |
| 45 | 46 | 10565 | 460 |
| 47 | 48 | 11565 | 480 |

The cost column is the entire level's cost, before subtracting XP already earned
within the current level. HUD XP remains the cumulative total.

Implementation: CharacterLevels derives level from integer XP and verifies
boundary comparisons against integer thresholds. Tests exercise thresholds for
levels 2–20,000, maximum integer XP, the full campaign, retry reset, stale kills
and bell calls. Levels add no automatic stat bonuses. Each gained level now awards one card
choice immediately; level 2 retains the Focus/dash/fireball starter offer. A
kill crossing several thresholds queues every earned choice, including on the
final boss. Victory follows resolution of those final choices.

Bell calls add 40 percentage points of temporary XP. Skips add 10 percentage
points of run-long XP. These bonuses add together and apply to base enemy XP
when the kill callback is accepted, including already-spawned enemies. A wave's
last kill receives its bonus before temporary XP resets; permanent XP remains
until the run ends. Fractional kill awards round down. Baseline checkpoints above apply without either bonus.

HUD: Level x=90, XP x=205, both y=115 in the 960×540 logical canvas. Old XP was
x=90, y=89. Bonus labels sit at y=140. Live visual acceptance remains pending.
