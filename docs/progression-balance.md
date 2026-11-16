# Family buckets and varied builds — version 0.13.0

`./scripts/test.sh` uses production Run/draft rules. The old system guaranteed a
main-stat option and assigned invested main stats/Shaman Stone weight
`1 + 3 × current rank`. This encouraged Pale Ore streaks. Those rules are removed.

## Current draft rules

- Each upgrade family has one bucket. Thorns has five ranks. Rank slots belong inside the
  bucket: Pale Ore has eight ranks, Shaman Stone five, Hiveblood one.
- Choose a family uniformly, then offer its next available rank. No rank count
  or investment bonus affects its roll. Draw three distinct families.
- Families from the preceding offer are excluded while enough fresh eligible
  families remain. If the pool is almost exhausted, reuse only as needed to fill
  the available slots. Capped/prerequisite-locked families stay ineligible.
- The first Focus/dash/projectile-spell choice remains fixed. All later drafts
  use the same family rule; no guaranteed damage/stat slot or milestone bonus.
- Seeds remain repeatable. The simulation samples the first 25 level choices. End/restart
  clears both acquired ranks and recent-offer history.

## Measurements

Each policy uses seeds 0–999, assumes all combat won, buys available basic
abilities and then prioritizes its named stats. Results describe offers and
choices, not survival or promises for other strategies.

| Priority | Cap at least one target stat | Cap both | Median distinct families | Median nail rank | Median Arcane Spark rank |
| --- | --- | --- | --- | --- | --- |
| Nail, then masks | 81 / 1,000 | 0 / 1,000 | 20 | 2 | 0 |
| Spell damage, then Soul gain | 323 / 1,000 | 18 / 1,000 | 21 | 1 | 1 |

All 1,000 runs in each policy selected at least 12 distinct families. Nail still
caps at eight ranks and Arcane Spark/Soul Well at three, but maxing a stat is now
rare. This deliberately replaces the earlier goal of making one/two caps common.

A separate 10,000-seed comparison uses the same eligibility after selecting
Focus first. The level-3 offer includes Pale Ore 728 times and Hiveblood 732 times.
That is consistent with equal per-family probability despite eight versus one
rank. Tests also check exact no-repeat/uniqueness/cap rules across the full runs.

## Encounter and level changes

The 47-wave roster is unchanged in 0.13.0. Mantis Claw is part of the baseline
loadout and no longer occupies a draft bucket. Choices are awarded from levels,
including mid-wave. These simulations choose cards rather than skipping, so they
exclude permanent skip bonuses and bell XP boosts. Skipping choices now increases
XP and therefore future choice frequency; the measurements above do not model
that policy or survival.
