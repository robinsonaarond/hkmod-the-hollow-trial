## Current work — 0.12.0

Kept all 47 wave definitions/base XP unchanged. Rewards now follow each level;
start with Mantis Claw; bell moved into the ceiling's upper-right corner. Added
additive kill-time XP bonuses (+40% temporary per bell call, reset on individual
wave clear; +20% run-long per skipped choice). Wave repairs remain separate from
level rewards. Tests and live checklist are in docs/playtest.md.

## Current work — 0.11.3

Added XP-derived character levels using the functional 10×current-level cost.
Rebalanced enemy XP for approximately one level per wave. Added Level left of
XP and lowered both labels. Campaign balance verified in docs/experience-levels.md.

## Current work — 0.11.2

Added permanent central/native-respawn spike clearance and native charm-pane
management without a bench. Earned ownership and equipped effects are tracked
separately; choices survive wave/card refreshes. No free healing from menu toggles.
Build and regression verification recorded in docs/playtest.md.

## Current work — 0.11.0

Implemented: lower idol, triple Gruz wave 26, 20 advanced waves and double-Vessel
finale, wave-stacking native bell, deferred rewards, run XP, card-selection
world freeze with explicit Glowing Womb hatch suspension. Build and automated
checks recorded in docs/playtest.md. Live gameplay acceptance remains pending;
new DLLs staged under artifacts/Gauntlet. No game installation changed.

# Hollow Knight: Gauntlet — project plan

## Intended end state

A replayable single-player mini-game inside Hollow Knight: enter a Godhome combat arena,
clear escalating waves, choose one of three upgrade cards between encounters,
and build a different Knight each run. A run ends on death, abandonment, or
victory. Run upgrades disappear afterward. Initial target: Steam version
1.5.78.11833, native Fedora Linux and macOS, using the community Modding API.
Working title: Gauntlet. No final name or art direction chosen.

The first complete release should have one arena (optionally a second rest/shop
room), twenty-five curated regular waves and a boss at wave 26, a fixed starting loadout,
meaningful capped upgrades, readable cards, keyboard/controller navigation, restart,
and a dedicated mod save flow. Existing game assets come first. Keep build
choices separate from the combat code so XP or a Geo shop can replace the
between-wave reward trigger later. Do not spend normal-save Geo in the prototype.

## Scope decisions

- First reward rule: free pick of one of three cards after each nonfinal wave.
- Upgrades stack with explicit caps. Choices and waves use a reproducible seed.
- No permanent metaprogression, multiplayer, procedural maps, new bosses,
  custom shaders, or dependencies on other gameplay mods in the first release.
- Fedora is the primary play target; test macOS from the first playable build.
- Use a disposable test save until dedicated save isolation is implemented.
- Latest Unity 6 game support is a separate compatibility investigation, not an
  assumption. Never mix a legacy API with a newer game install.

## Architecture

- `Gauntlet.Core`: engine-independent C# rules, wave definitions, seeded drafts,
  capped upgrades, enemy ownership and run lifecycle. No game assemblies.
- `Gauntlet.Mod`: Modding API entry point, enemy preloads, spawning, card UI,
  scoped combat hooks, cleanup on death/scene exit/unload.
- `Gauntlet.Tests`: executable scenario tests without external test packages.
- Keep game binaries, SDK downloads and local references out of source control.
  Distribution includes our mod and its core library, not game assemblies.

## Milestones and acceptance

1. **Foundation**: buildable core, five-wave run, six cards, deterministic drafts;
   tests cover victory/death/restart, duplicate or stale enemy events, invalid
   selections, caps, and reset. Write platform/build instructions.
2. **Experimental integration**: manually start in Godhome workshop, spawn
   preloaded Crawlids near the player, clear five waves, choose cards, apply
   temporary effects, abort on scene exit/death. This is a technical sandbox:
   inherited loadout, one enemy type, no boss, no custom arena or save mode yet.
   Compile against matching legacy game/API references, then playtest both OSes.
3. **Playable vertical slice**: isolated arena and entry/exit flow, fixed loadout,
   native controllers disabled or avoided, curated varied enemies, reliable
   death accounting, controller UI, dedicated save, boss finale. A full run must
   work twice consecutively without state leaking into the next run.
4. **Content and polish**: tune 6–10 cards and synergies, accessibility/readability,
   audio/art, balance wave difficulty, document supported mod combinations.
5. **Release**: test exact game/API/dependency versions on Fedora and macOS;
   package with install instructions and regression checklist. Add XP/Geo only
   after the core loop is fun and stable.

## Current status (2026-11-09)

The user confirmed the first prototype's enemy waves and card choices work on
macOS. Version 0.2.0 responds to playtest feedback:

- Opt-in takeover of the Attuned Gruz Mother arena, armed from Hall of Gods.
  Native boss/battle roots are disabled only for the armed visit; the normal
  challenge remains available. Use the native statue arrival and dream return.
- Five curated mixed waves: Husk Hornheads, Aspid Hunters, Hoppers and a Great
  Hopper elite finale. Preload paths were inspected in local legacy game assets.
- Fixed edge arrival seals, 1.8-second warnings, staggered activation, occupancy
  checks and a live distance recheck before materialization. No Knight-relative
  spawns. Pending enemies still count toward wave completion.
- Original generated card frame, six scalable line-art emblems, live text,
  selection highlights and rank markers, embedded in the mod DLL.

Version 0.2.0 builds against matching legacy references, passes 80,964 rules
assertions, and is installed on the Mac with a backup. Startup logs confirm all
enemy preloads and initialization. New arena combat still needs a live test.

Version 0.2.2 moves entry points four units inside the walls and shortens
arrival warnings to the requested 200 milliseconds; live clearance adapts to the
new placement.

Version 0.2.1 removes mouse-dependent startup: F6 arms in the hall, followed by
a three-second countdown after dream arrival. It also corrects the arena-width
check to accept the native 17-unit room and fits portal clearance to that width.

Version 0.2.3 adds a 500 ms wave-complete cue followed by a 500 ms card
rise/fade. Selection and skipping unlock after the full second; pause freezes
the transition, and end conditions reset it.

Version 0.3.0 starts a basic Knight (five masks, Old Nail, empty Soul, no charms),
adds full-vessel mask cards, native nail-tier cards and eight native charm boons.
It snapshots and restores loadout fields and overlays original values onto a
separate save copy during runs. Native death restoration waits for scene exit.
Wave-clear timing is now one second plus 500 ms for card animation. Movement
abilities and spell tiers stay available. Native effect/HUD and restoration
acceptance still need playtesting; real-adapter boundary tests supplement the
engine-independent rules tests.

Version 0.4.0 refreshes cached health HUD maximum checks, and integrates a native
nail-struck lever and alternate gauntlet idol at Gruz Mother's podium. Selection
is session-local and virtual: real statue unlock/completion writes are preserved.
Returning with the idol selected automatically readies another attempt. A new
transparent generated idol is embedded in the DLL and uses native swap animation.

Version 0.5.0 expands the default run to twenty curated waves in four sections,
with elite gates at 5/10/15 and a two-elite finale at 20. Nineteen card choices
allow substantially more build progression. Main-stat follow-up offers gain
weight with investment, and milestones (5/10/15 plus pre-finale 19) guarantee an
eligible main-stat option. Seed simulations check that focused builds can
practically cap one or two stats before the finale. Current native enemy assets,
spawn/transition timing, and loadout/save protections are retained.


Version 0.6.0 moves combat to Elder Hu’s Godhome arena while preserving the Gruz
podium entry and checking the pending trial scene identity. It prepends five
Crawlid/Vengefly tutorial waves and retains the twenty previous formations.
Native prefab HP replaces all escalating overrides. A pale flash and reset text
mark a baseline of five masks, a 5-damage nail, movement and jump only. Abilities,
Focus, spell tiers, nail arts and reserve vessels become cards, with complete
snapshot/save-copy restoration. The pool now has 52 cards and 30 charm boons,
including Fragile Strength/Heart and wave-refilled Joni. Pale Ore reaches rank
eight (37 damage); Shaman Stone reaches rank five (+40 beyond its native effect).
Drafts preserve a stat route while presenting abilities and charms. Native arena
and HUD acceptance remains a playtest requirement.

Version 0.7.0 adds 50% rounded-up reinforcements to waves 1–25 and targets the
Knight on arrival by extending native awareness rather than attack ranges.
Crawlids follow the Knight and walkers/flyers engage earlier. Gruz Mother is the
actual boss of wave 26, with a final card after wave 25. Native fight behavior
remains, while old-room title/battle actions and the untracked baby-spawning
corpse are removed from the owned clone. Victory grants a five-second styled
screen, damage protection, and automatic dream return. Core timer/lifecycle and
real-adapter encounter tests cover these behaviors; native combat still needs
playtesting.

Version 0.7.1 softens wave 10 to three regular Hoppers and two Aspid Hunters,
postponing the first Great Hoppers to wave 15.

Version 0.7.2 restores one Great Hopper as the final arrival in wave 10, alongside
three Hoppers and two Aspid Hunters. Wave 17 converts all Aspids to native Primal
Aspids; waves 18–25 alternate regular/Primal variants in existing slots. The verified
Primal prefab uses flyer spawn height, its native three-shot attack and HP, and
arena-wide engagement settings that keep its old-room sleep checks from stalling.

Version 0.8.0 moves the trial to Grimm’s Godhome stage, preserving curtains,
scenery and audience while suppressing only encounter actors/hazards and the
native scene FSM. Dense local collision inspection verifies a solid roof with
zero gaps at 331 positions. Runtime checks before the run and boss add a scoped
invisible terrain ceiling only if coverage fails, recheck it, and clean it up on
exit/unload. This addresses Gruz Mother escaping Elder Hu’s open-topped room.

Version 0.9.0 preserves the owned native boss corpse sequence and waits for its
completion before the victory overlay, which now waits for Confirm instead of
returning on a timer. Equal-probability upgrade-family buckets replace stat
routing and investment weighting; recent offers sit out a draft when possible.
This intentionally makes damage caps rarer and rewards broader builds. Native
Godhome floating-platform layouts rotate between waves with basic-jump heights,
entrance/player clearance and cleanup. Tutorial/elite/boss waves remain open.
Spikes are feasible but await hazard recovery and placement design.

Version 0.10.0 uses Soul Tyrant’s Godhome arena with its native music controller
and palette. Both boss phases are suppressed while the environment/music remain.
Dense collision inspection verifies a continuous ceiling at y≈44.1 over the
34-unit floor. Kingsoul joins the pool as a one-rank, notch-free charm; its native
completed variant is temporary and the original Kingsoul/Void Heart state is
included in snapshot/save protection and restoration.

Version 0.10.1 renders native charm sprites into cached transparent card icons,
including tightly packed atlas artwork whose textureRect is empty. Native vessel
ownership checks now refresh with the mask HUD after loadout changes, so the
reserve orbs match current capacity and return with the original loadout.

Version 0.10.2 reuses native inventory spell, Focus, nail-art and nail-tier
sprites through the existing transparent icon cache. Spell cards preview the
next form, with matching upgraded spell titles; Pale Ore previews Sharpened,
Channelled, Coiled and Pure Nail, retaining Pure Nail for ranks 5–8.

Version 0.10.3 makes Fragile Strength and Heart break after real damage,
including blue-mask loss. Native broken flags suppress Strength damage and
Heart capacity; no native full-heal refresh runs during a break. Blocked hits
preserve charms. Wave clear repairs them before the reward; original save
state remains protected. Gruz Mother retains native title initialization.

Version 0.10.4 reuses the native orange Death Puff Med particle effect for
arrivals, preserving the 200 ms safety delay and letting the cloud fade after
emergence. The idol copies Gruz Mother’s sprite depth and lit material so it
rises behind the plinth and receives Hall lighting. A compact Perpetua status
display replaces the full-width debug box at the upper right, with native HUD
space reserved on the left and status hidden during rewards.

This implements parts of milestones 3 and 4. Dedicated save isolation, a bespoke boss and Fedora verification remain
open. The baseline loadout is implemented in 0.3.0. See
README.md, docs/playtest.md and docs/asset-prompts.md for usage and evidence.

## Risks to resolve through playtesting

- Enemy prefabs can depend on scene-specific FSMs, terrain, pooling or death
  behavior. Missing/destroyed enemies must abort a wave rather than grant rewards.
- Verify the opt-in Soul Tyrant arena takeover, native arrival/return, and lack of
  statue completion side effects before balancing encounters.
- Damage hooks may interact with charms and other mods; verify actual damage,
  not only displayed values. Avoid broad claims of mod compatibility.
- Verify temporary hooks stop on death, victory, scene exit and mod unload.
- Controlled loadouts are implemented; verify native save protection and
  restoration and consider a dedicated save mode before public use.
- macOS architecture, graphics and Linux case-sensitive paths need real tests.

## Research references

- https://github.com/hk-modding/api/releases (legacy v77 / game 1.5.78.11833)
- https://github.com/hk-modding/api/pull/164 (Unity 6 port under development)
- https://hk-modding.github.io/api/api/Modding.ModHooks.html
- https://github.com/Korzer420/TrialOfCrusaders (existing roguelike precedent)
- https://github.com/TheMulhima/Lumafly (cross-platform installation)
- https://github.com/nickc01/WeaverCore (possible later custom room tooling)
