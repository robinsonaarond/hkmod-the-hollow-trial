# Version 0.14.0 — health animation, visible roof, badge and wave feedback

Changes:

- Advance native HUD sprite animations during both card selection and inventory
  pause. Health-only FSM waits also use real time. Preserve ordinary pause behavior.
- Stop forcing red/blue mask FSMs into Check Type/Break? during refreshes: doing
  so abandons the in-flight animation-completion callback. Preserve their native
  initialization/appear/break/refill paths and keep the premature full-heal-event
  suppression from 0.13.1.
- Trial-owned Blue Health Control edits make native row rebuilding display-only:
  read current remaining HP into native counters and omit the native blue-health
  reset/increment actions. Inventory rebuilds cannot grant health. Restore the
  exact original action arrays after the trial.
- Joni grants its initial conversion when earned, then refills only when the
  total cleared-wave count crosses 5, 10, 15, … . Fragile charms still repair at
  every clear. Repairing Heart restores capacity but does not refill Joni early.
  Repeated callbacks, card choices and five bell calls do not grant extra refills.
- Sample the gameplay camera top after arrival. Ceiling underside sits 0.35
  world units below it; native stone ledges form a visible continuous underside.
  Bell height and spawn bounds use the same calculated height. Retry recalibrates.
- Keep the shared plinth's existing Gruz completion badge visible on both faces,
  using the actual completion data shown by the Hall's entrance list. No new
  boss completion is written or fabricated for trial wins.
- Wave-number/title banner lasts 2.6 seconds. Rapid bell calls replace it with the
  newest wave immediately. Card/inventory/pause screens suspend the banner timer.

Verification:

- 295,256 core assertions passed, including banner replacement/pause/expiry.
- 4,835 loadout assertions passed, including acquisition, twelve subsequent clears,
  five bell calls, out-of-order stacked clears and duplicate milestone repairs.
- 524 encounter assertions passed, including display-only blue counters, native
  action restoration, UI timing restoration and camera-height ceiling recalibration.
- Release build succeeded against installed Hollow Knight 1.5.78.11833/API v77.
- Installation and live visual acceptance remain pending.

Live checks:

1. Take damage immediately before leveling. Health break/refill animations must
   finish during card selection and after closing inventory, without frozen frames.
2. Toggle Joni/Hiveblood/Heart while wounded; repeat inventory opening. Numeric
   HP and displayed masks must agree and no redraw may replenish blue health.
3. Confirm the visible stone ceiling meets the camera's top edge, the bell is
   visible/reachable, and Gruz Mothers collide with the visible boundary.
4. Compare the podium badge on both faces with Gruz's Hall entrance completion.
5. Ring five times: banners must promptly show the latest wave; no banner should
   expire unseen during level choices. Kill stacked waves in different orders.
6. Check Joni before/after total clears 4→5, 9→10 and 14→15, including a Heart
   repair on a non-milestone clear and repeated inventory toggles.

---

# Version 0.13.1 — health HUD synchronization

Native-code findings:

- HeroController.CharmUpdate calls MaxHealth during its calculations. The native
  hero proxy emits HERO HEALED FULL before the adapter restores wounded health.
- Native blue_health_display routes HERO HEALED FULL to Health Reset, removing
  existing blue masks. Restoring PlayerData.healthBlue afterward does not undo
  those queued removal animations.
- The red-mask Inactive state disables its renderer; Empty plays a clip without
  reenabling it. Rechecking only Check Max HP can leave an empty returning slot
  invisible and bypass Hiveblood sprite selection.
- CHARM INDICATOR CHECK restarts native mask initialization with a timed pause,
  which conflicts with the stopped card-selection/inventory gameplay clock.

Changes:

- Hook native HeroController.MaxHealth only during active loadout transactions.
  Preserve the PlayerData calculation while suppressing its premature HUD event.
  Ordinary healing outside the transaction retains native behavior; unload
  removes the hook.
- Publish CHARM INDICATOR CHECK after final health/blue-health values are restored
  for card changes and inventory charm toggles.
- Restore the mask renderer and Initialised flag, then execute Check Type so
  native type/max-HP/full/empty checks complete without replaying load animations.
- Rebuild the native blue row when Joni is toggled, using remaining HP rather
  than full capacity; restore real capacity immediately after the native rebuild.
- Retain native blue-health rebuilding for intended replenishment at wave clear.

Validation:

- Regression tests observe HP when charm-indicator events are published, covering
  repeated card applications and menu toggles.
- HUD boundary tests cover a hidden empty slot, cleared initialization flag and
  native type-selection entry point.
- 300,543 assertions passed (295,250 core; 4,770 loadout; 523 encounter).
- Release build succeeded against installed legacy game/API references.
- Native code and serialized FSM states inspected from installed game assets.
- Live reproduction/acceptance remains pending; screenshots remain useful if any
  missing/incorrect masks persist.

Live checks:

1. Take damage, level up repeatedly, then open/close inventory; verify filled and
   empty slots match remaining HP after every choice.
2. Break and repair Fragile Heart; newly restored slots must be visible.
3. Repeat with Joni and Hiveblood, including wounded Joni at a mid-wave level-up.
   Blue masks must persist without an unintended heal.
4. Toggle health charms while paused in inventory, then resume combat.

---

# Version 0.13.0 — arena, inventory and upgrade corrections

Implemented:

- Native particle renderers hidden during card selection; original visibility
  restored afterward. Exact decorative emitter still requires live identification.
- Inventory Open flag freezes gameplay using the existing clock/hatcher guard.
  Native inventory waits, tweens, fades and sprite animations use unscaled time.
  All timing edits are restored on run end, scene change and unload.
- Lower ceiling underside y=38; bell center x=35.7, y=36.2.
- Floor spikes removed, including the spike preload dependency.
- Spawn roots fit transformed body-collider corners above the floor and below
  the ceiling, including disabled intro colliders. Native roster collider audit
  verified box shapes; Mantis/Sentry root offsets explain embedded floor spawns.
- Arrivals alternate portals every 0.8 seconds after the 0.2-second cloud.
  Hero/enemy occupancy no longer defers, relocates or cancels an arrival.
- Down selects Skip; Up returns to cards; Confirm accepts either choice.
  Left/Right selects cards; Tab still skips.
- Skip XP +10% permanently; bell XP remains +40% temporarily per valid ring.
  Fractional kill XP rounds down, consistent with the existing integer XP rule.
- Thorns: five ranks; native charm at rank 1, +10 thorn damage per extra rank.
  Native Hit U/D/L/R children under Thorn Hit receive the bonus, independently
  of nail/spell bonuses. Rank 5 adds +40, enough for a 35-HP Primal Aspid.
- Last Light: +15 nail damage per rank at one mask; maximum +45.

Verification:

- 295,250 core assertions passed.
- 4,683 loadout adapter assertions passed.
- 516 encounter adapter assertions passed.
- Release build passed against installed 1.5.78.11833 / Modding API v77 references.
- NuGet vulnerability metadata lookup unavailable; compiler reported zero errors.
- Staged output: artifacts/Gauntlet. No installation or live gameplay acceptance.

Live acceptance checklist:

1. Level up with arrival clouds, death particles and Soul effects active. Cards
   must remain unobstructed; particles must return after accepting/skipping.
2. Open inventory in combat. Projectiles, enemies, pending arrivals and passive
   Soul consumption must stop. Navigate every pane, equip/unequip charms, and
   close/reopen. Menu animations must complete and combat must resume normally.
3. Wall climb to the visible bell. Ring five times; queued enemies must arrive
   even with the Knight and other enemies standing on both portals.
4. Observe Gruz Mother ceiling bounces and all large ground-enemy arrivals.
   No floor embedding, offscreen flight, spikes or stalled wave completions.
5. Select Skip using controller Down + Confirm, then return using Up. Verify
   +10% permanent XP per skip and +40% temporary XP per valid bell ring.
6. Upgrade Thorns five times; test one-hit Primal Aspid damage with the starter
   nail. Disable/re-enable the charm and confirm rank damage is retained.
7. Test Last Light at ranks 1–3: +15/+30/+45 only at one mask.
8. Check pause-menu nesting, death, F7, retry and unload for clock/UI restoration.

---

# Version 0.12.0 — level rewards, wall-climb bell and XP bonuses

- Wave definitions and base XP unchanged; Mantis Claw granted at start and removed
  from drafts. Other abilities remain earned normally.
- One upgrade choice per level gained, triggered on kill even mid-wave. Same-frame
  kills during the transition remain counted; multi-level rewards queue without
  unfreezing. Last-boss level choices are resolved before victory.
- Bell moved to right−1.3 and ceiling−1.8; wall jump provides access without a
  platform. No bonus or new wave is granted beyond the finale.
- Each accepted bell call adds +40 percentage points of temporary XP globally.
- Each skipped level choice adds +20 percentage points of run-long XP immediately.
- Both bonuses add together and apply at kill time, including old live enemies.
  Credit the clearing kill first, then reset temporary XP on each wave clear.
- Permanent bonus survives wave clears. Both bonuses reset on any run ending.
- HUD shows temporary/permanent percentages at y=140; the card header also shows
  both. Level-up announcement replaces the wave-clear reward announcement.
- Fragile repairs and Joni refills remain tied to actual wave clears. Unrelated
  mid-wave upgrades preserve damage, broken flags and remaining blue health.
- Verification: 296,481 core assertions, 4,458 loadout boundary assertions, 450
  encounter boundary assertions. Live gameplay acceptance remains pending.

Live checks:

1. Verify Mantis Claw on a fresh run and wall-climb access to the corner bell.
2. Ring during combat; confirm +40% per ring and unchanged old enemy presence.
3. Kill old/new enemies and check immediate boosted XP. Clear one stacked wave:
   its last kill is boosted, then temporary XP returns to zero.
4. Level up mid-wave; enemies, projectiles and pending arrivals must freeze and
   resume. Check every choice after a multi-level boss kill.
5. Skip several choices: permanent XP increases by +20% each without advancing
   the wave or changing existing XP. Later kills use the new bonus.
6. Check wounded Joni and broken fragile charms across mid-wave card choices,
   actual wave clears, final reward/victory, death, F7 and unload.

---

# Version 0.11.3 — character levels and HUD spacing

- Level 1 starts at zero XP; advancing from L costs 10×L XP.
- Cumulative threshold is 5×L×(L−1); no hand-authored level thresholds.
- Adjusted enemy XP gives one level on 43/47 waves and level 48 at completion.
- Moved XP right to x=205; placed Level at x=90. Both moved from y=89 to y=115
  on the 960×540 logical HUD to avoid the health display.
- Level is derived from XP, retained for the end screen and reset on retry.
- Verification passed: 277,190 core, 4,333 loadout, 450 encounter assertions.
- Required live check: inspect Level/XP below normal, blue and long Joni health
  bars; confirm counters during card choices and progression from enemy kills.
- Build staged under artifacts/Gauntlet. Live visual checks remain pending.

---

# Version 0.11.2 — safe spike returns and native charm toggles

- Moved central spikes to the sides: offsets ±5, ±7, then ±5/±9.
- Runtime placement reserves four units around the center and the current
  PlayerData.hazardRespawnLocation.x, regardless of the Knight's current location.
- Earned charm cards now populate native charm ownership. Native UI Charms equip
  gate works without sitting at a bench. Other inventory bench checks remain native.
- Earned charms cost zero notches during active trials. Disabled choices persist
  across waves and rewards; unearned charms cannot be enabled through Run rules.
- Native CharmUpdate synchronization preserves the remaining health budget, Soul
  and fragile break state. Extra charm-pane blue-health refill broadcasts are
  suppressed while the run owns the menu, restored on exit/unload.
- Automated verification: 236,821 core, 4,333 loadout, 450 encounter assertions.
- Required live checks: take spike damage repeatedly from waves 27–46, verify safe
  hazard returns; toggle Glowing Womb, Joni, Heart and Shaman Stone in the normal
  charm pane; confirm no healing/Soul loss, disabled effects stay off after cards,
  and normal menu restrictions/loadout return after F7, death and unload.
- Installation uses scripts/install-macos.sh; live verification pending.

---

# Version 0.11.1 — podium height and bell clearance

- User reports the initial build works but the idol still floats and the bell
  step interferes with enemies.
- Lowered idol local Y from −0.15 to −0.40 (another approximate 10 pixels).
- Removed the bell step object and its collider entirely.
- Bell remains at right−2.5, floor+5.4; user confirms jump access is sufficient.
- Rebuild staged under artifacts/Gauntlet; installation uses scripts/install-macos.sh.

---

# Version 0.11.0 — second chapter, bell, XP and card freeze

Status: staged build. Installation attempt was blocked by the filesystem sandbox before any game files were changed. Previous mod and save files backed up under `.local/backups/before-gauntlet-0.11.0-20261111-080157/`. Run `bash scripts/install-macos.sh --launch` from a user terminal to install and launch. Live tests remain pending.

Automated verification: 236,821 core assertions; 4,322 loadout boundary assertions;
407 encounter boundary assertions. Release build passes against local game/API
1.5.78.11833 references. NuGet vulnerability lookup emits NU1900 because the
NuGet service is inaccessible; compilation reports zero errors.

- Original 25 formations preserved; wave 26 has three native-health Gruz Mothers.
- Twenty new waves (27–46), followed by two Broken Vessels (47).
- Native enemy, spike and bell paths checked against local 1.5.78.11833 assets.
- Failed Champion's FalseyControl references FK Floor, Battle Scene, Crack Dust,
  FK Barrel Summon Dream and Rubble Fall. The requested Vessel fallback avoids
  recreating those original-room dependencies.
- Bell at right−2.5, floor+5.4; permanent small step top at floor+2.4.
- Bell queues additional waves with owned tokens; older enemies/arrivals persist.
- Rewards bank until the batch clears. Finale victory waits for all active waves
  and both owned corpse animations. Unused banked choices are discarded at victory.
- XP resets at run start, awards each registered enemy once and rejects stale
  callbacks. Summoned/unregistered objects do not award XP yet.
- Gameplay time scale is captured and set to zero during card selection. The
  native `Hatchling Spawn` FSM is explicitly suspended, including when upgrades
  refresh charm state. Card presentation remains on unscaled time.
- Cleanup restores the prior time scale, preserves a native pause menu, restores
  only hatch controllers that were enabled, and removes bell/step/hazards.
- Idol local Y lowered from +0.10 to −0.15 world units (approximate 10px correction).

Required live acceptance:

1. Toggle the Hall idol and verify the base rests on the plinth.
2. Reach the high bell from its step with only basic jump. Ring repeatedly;
   ensure each ring adds exactly one wave without deleting living enemies.
3. Clear stacked waves out of order. Verify every earned card choice appears,
   with no control release or gameplay resume between queued choices.
4. Select Glowing Womb, retain Soul, and leave cards open for 15 seconds. Verify
   unchanged Soul/minion count. Confirm/skip, pause/resume, F7, die/retry, and
   unload: gameplay and hatch controller must recover correctly.
5. Verify three Gruz Mothers on wave 26. Their deaths must not return to the hall.
6. Check Mantis aggression on a save with defeated Mantis Lords, both sentry
   wake-up paths, native HP and safe arrivals across waves 27–46.
7. Check spike dust/expansion, no placement under the Knight, jump clearance,
   normal damage/recovery, and removal before the finale or on exit.
8. Verify both Broken Vessels wake, land, attack, stun and die independently.
   Check bound/height adaptation, no old-room teleports or missing-object logs,
   no native achievement/completion write or automatic dream return.
9. Verify XP beneath health, once per registered kill (including minion kills),
   during choices and after defeat; retry resets XP to zero.

---

# Playtest record and acceptance

## Version 0.10.4 arrival clouds, idol shading and compact HUD acceptance

Yellow arrival rings and countdown captions are replaced with the native orange
Death Puff Med effect, referenced by the already preloaded Gruz Mother death
component. Local asset inspection verified its orange gradient, 0.1-second
emission and 0.6-second particle lifetime. An owned copy runs at 60% scale;
its pooling scripts are disabled before activation. The safety delay remains
0.2 seconds; the cloud continues fading after an enemy emerges. Unsafe arrivals
cancel their cloud, and completed clouds self-destruct shortly afterwards.
No additional combat scripts, damage, sound or preload scenes were added.

The Gruz Mother reference in GG_Workshop uses Sprites-Diffuse Unity, sorting
layer/order zero, sculpture z=2.29 and plinth z≈1.959. The old idol was unlit at
z=1.5, in front of the plinth. The owned idol now copies the actual sculpture’s
relative depth, shared material, layer, sorting and mask interaction. Native
sculpture p90 luminance is ≈44/255 versus ≈180/255 in the generated stone art; a
0.25 RGB tint matches that restrained highlight level, preserving alpha. Its
bitmap/pivot and native swap animation stay intact; it receives the same Hall
lighting instead of glowing at its unlit texture brightness.

The full-width boxed debug overlay is replaced by a compact upper-right display
using native Perpetua, muted ivory/gray and a thin divider. Wave/title and
foes/nail/boons/return hint occupy two rows. Warnings/countdown/broken-charms
notices appear below only when relevant. The left 60% of the screen is reserved
for native masks and Soul; redundant numeric HP and arrival spam are removed.
The display is hidden during card rewards and victory.

Build passes against the installed legacy API/game. Existing automated checks
pass: 236,403 core assertions, 4,322 loadout assertions and 303 encounter/HUD
assertions. Native inspection also confirms the puff has collision disabled,
nonlooping emission and no combat component; source assets remain untouched.
Those automated checks do not verify the rendered effect, lighting or screen layout.

Installed 0.10.4 with matching staged hashes. Previous mod files and save copies
are in `.local/backups/before-gauntlet-0.10.4-20261110-220802/`. Fresh startup
confirms all seven preload scenes and Gauntlet 0.10.4 initialized, including
successful resolution of the native orange particle prefab. Rendering and
slide occlusion remain pending live visual verification.

Live checks pending:

- Ground/flying/boss arrivals: orange poof, no yellow ring/name/countdown, enemy
  emerges after the usual 200 ms. Cloud fades naturally rather than popping off.
- Move into a warning point: arrival cancels/moves, with no lingering effect or
  collision/damage from the cloud. Pause/F7/retry/scene exit should clean up.
- Toggle lever/F6 both ways: idol rises behind the stone plinth, with its base
  concealed during the slide, and its lighting matches the Gruz Mother statue.
- Check default and large/Joni health bars, 1280×720 and 1920×1080: no text over
  masks or Soul; wave and stats remain readable in the upper right.
- Confirm reward titles/card artwork are unobstructed and countdown/error/
  broken-charms notices remain readable.

## Version 0.10.3 fragile charm breakage and Gruz Mother title acceptance

Fragile boons previously only repaired, with no on-hit break trigger. The trial
now observes health/blue health before and after native TakeDamage. Actual lost
health breaks equipped Fragile Strength and Heart; i-frames, Baldur Shell and
Patient Shell hits that cost no health preserve them. Native brokenCharm flags
remain temporary and saved normal loadouts retain their original state.

Strength's native nail-damage FSM was inspected: it requires equippedCharm_25
and NOT brokenCharm_25, and responds to UPDATE NAIL DAMAGE. Breakage broadcasts
that event. Heart loses its two-mask capacity, clamping current health only if
necessary. Joni's converted capacity shrinks too; existing blue masks recheck
remaining health without a refill. Breakage avoids CharmUpdate and UPDATE BLUE
HEALTH, both of which can heal. The HUD marks fragile charms broken for the
remainder of the wave. Wave clear repairs before card selection; Heart restores
its two bonus masks, and the existing Joni wave refill applies. Repair is blocked
during combat and on a dead Knight. Repeated damage/repair cannot stack healing.

The boss adapter previously stripped Wake's SetFsmBool/SetFsmString actions,
including AreaTitle's Visited flag and Area Event = BIGFLY. Both native title
settings are now preserved, while old-room battle dependencies remain removed.
This lets the native title use Gruz Mother's localized name instead of defaults.

Build passes against the installed legacy references. Automated checks pass:
236,403 core assertions, 4,322 loadout assertions and 303 encounter/HUD assertions.
New cases cover red/blue damage, blocked hits, one-time breakage, capacity loss,
repair before reward selection, no repair in combat/on death, save/exit state,
and preservation of the native BIGFLY title initialization.

Installed 0.10.3 with matching staged hashes. Previous mod files and save copies
are in `.local/backups/before-gauntlet-0.10.3-20261110-200808/`. Fresh startup
logs confirm seven scenes preloaded and Gauntlet 0.10.3 initialized. The live
charm behavior and final boss title still need gameplay verification.

Live checks pending:

- With Strength, compare nail hits before/after taking damage: its 50% bonus
  stops until wave clear. HUD shows breakage; next wave starts repaired.
- With Heart, taking damage removes the two extra mask slots. No full heal on
  breakage; clearing the wave restores those two slots/masks.
- Try Heart + Joni: losing a blue mask breaks both fragiles, reduces bonus
  capacity and preserves existing wounds. Wave clear restores the Joni pool.
- Blocked hits should preserve fragiles; F7/death/retry/quit should preserve
  the ordinary save's original fragile/broken/unbreakable state.
- On wave 26, the native title should say Gruz Mother with no placeholder
  sub/super text. Wake, attacks and the native death/victory sequence should work.

## Version 0.10.2 inventory artwork acceptance

The user confirmed the charm artwork and vessel HUD fixes, and requested inventory
spell and nail artwork. Cards now reuse the native inventory sprites for Focus,
Vengeful Spirit/Shade Soul, Desolate Dive/Descending Dark, Howling Wraiths/Abyss
Shriek and all three nail arts. Spell cards preview their next rank and show the
upgraded spell name for rank two. Pale Ore previews Sharpened, Channelled, Coiled
and Pure Nail in order; ranks 5–8 keep Pure Nail. Nail icons have a taller display
area to make their narrow silhouettes readable. Existing charm rendering is shared.

Local asset inspection verifies the six spell sprites against the inventory
spell FSM references, and all five native nail tiers against InvNailSprite's
actual fields. Inventory sprites are resolved from already loaded game assets;
no game artwork is copied into the distributable and the inventory stays closed.
Missing art retains the existing emblem fallback and is logged at trial start.

Build passes against the installed legacy game/API. All 14 sprite names resolve
uniquely in native resources. Existing automated checks pass: 236,403 core,
4,299 loadout and 296 encounter/HUD assertions. No gameplay rules changed.
Installed 0.10.2 with matching staged hashes; prior mod and save copies are in
`.local/backups/before-gauntlet-0.10.2-20261110-185215/`. Fresh startup logs
confirm all seven preload scenes and Gauntlet 0.10.2 initialized. The inventory
artwork itself still needs a live playtest.

Live checks pending:

- The fixed opening spell card shows Vengeful Spirit's inventory art.
- Check Dive and Wraiths; their second ranks show Descending Dark/Abyss Shriek,
  and Vengeful Spirit's second rank shows Shade Soul, with matching titles.
- Check Focus and the three nail arts for actual inventory art.
- Pick consecutive Pale Ore ranks: Sharpened → Channelled → Coiled → Pure Nail;
  later ranks keep Pure Nail while damage/rank progression remains unchanged.
- Confirm charm icons still render correctly. The trial-start log should report
  14 native inventory icons prepared.

## Version 0.10.1 charm artwork and Soul vessel HUD acceptance

The user reported black charm-card squares (including Fury) and extra Soul
vessels that remained visible after their capacity was removed. Inspection of
Unity's native Sprite getter confirms it returns Rect.zero for tightly packed
sprites. Fury and Shaman Stone both use tightly packed sprites in the UI atlas.
Cards now cache transparent images rendered by a native SpriteRenderer, preserving
its mesh/atlas mapping instead of sampling textureRect. Blank/failed renders use
the existing emblem fallback and log a warning; cached images are released on unload.

The native vessel_orb Init state rereads MPReserveMax and selects Not Obtained
or the appropriate native fill state. Loadout HUD refresh now reruns that state,
without disabling the vessel object or restarting the main Soul meter. The same
refresh runs on baseline reset, card changes and restoration.

Build passes against the installed legacy API/game references. Automated checks
pass: 236,403 core assertions, 4,299 loadout assertions and 296 encounter/HUD
adapter assertions, including reserve-capacity changes 99 → 0 → 33 → 66 → 99,
repeat reset and restoration. These boundary checks do not test GPU rendering.

Installed 0.10.1 with matching staged hashes. Previous mod files and save copies
are under `.local/backups/before-gauntlet-0.10.1-20261110-182430/`. Fresh startup
logs confirm seven preload scenes and Gauntlet 0.10.1 initialized. Gameplay
appearance remains unverified by this automated pass.

Live checks pending:

- Inspect Fury, Shaman Stone and other charm cards: actual transparent artwork,
  correct proportions, no black squares or pieces of neighboring atlas sprites.
- Start with three reserve vessels on the normal save. Enter the trial: all
  three should disappear. Vessel Fragments should reveal exactly one per rank.
- Fill/drain each earned vessel; verify native fill animations still work.
- F7, victory and death/return should restore the original vessel display/capacity.
- The mod log reports how many native charm icons were prepared at trial start.

## Version 0.10.0 Soul Tyrant arena and Kingsoul acceptance

The user requested Soul Tyrant’s palette/music and Kingsoul. Kingsoul was absent
from the previous 52-card pool; this version adds charm 36 as one notch-free rank
and uses completed Kingsoul state 3, with state 4 (Void Heart) snapshotted and
restored. The pool is now 53 families / 31 charms.

Build passes with 236,403 core assertions, 4,299 loadout adapter assertions and
271 encounter adapter assertions. The native Kingsoul FSM was inspected in the
local resources: it reads equippedCharm_36/royalCharmState and responds to
CHARM INDICATOR CHECK. Refresh now broadcasts that event as well. Native Soul
generation itself still needs observing in game.

- Use the same trial idol at Gruz Mother’s podium, Attuned. Arrive in Soul
  Tyrant’s blue Sanctum room. Both native boss phases remain inactive; the floor
  does not break or change phase. The separate environment/music FSM stays active
  and receives GG MUSIC when the trial begins. Verify the intended native track.
- Bounds should be x=3..37, floor y=28. A 331-position collision audit found a solid
  roof at y≈44.1 with no gaps. Runtime checks/barrier remain; verify Gruz Mother
  stays enclosed. Platforms/arrivals use the shifted bounds automatically.
- Kingsoul appears with its native icon and no notch requirement. Pick it and
  wait for passive Soul gain, including with a save that originally had Void
  Heart. Check its interactions with Focus/spells and existing Soul upgrades.
- A fresh run has no inherited royal-charm effect. Leaving, dying, saving/reloading
  and unloading must preserve the original royalCharmState/equipment. Saved data
  must not replace Void Heart with the trial’s Kingsoul or vice versa.
- Ordinary Soul Tyrant, Grimm and Pantheon fights remain unaffected by trial
  selection. Existing boss death, victory confirmation and stage layouts remain.

Installed 0.10.0 with matching staged hashes. Previous mod files and save copies
are under `.local/backups/before-gauntlet-0.10.0-20261110-125903/`. Fresh startup
logs confirm seven scenes preloaded and Gauntlet 0.10.0 initialized. Native
Soul Tyrant entry/music and Kingsoul regeneration remain for live playtesting.

## Historical version 0.9.0 death, buckets and layouts acceptance

User confirmed 0.8.0’s stage worked. Their next feedback requested normal boss
victory effects, input-driven return, fewer repeated damage upgrades and level
variety. Build passes; automated tests pass 237,953 core assertions, 3,954 loadout
adapter assertions and 273 encounter adapter assertions. These use simulated
native boundaries and do not prove the full Unity death or platform sequence.

- Wave 26: observe the native corpse Steam/Ready/Blow death animation and sound,
  blood/burst/shake. The victory overlay must wait until the corpse is destroyed
  or deactivated, instead of clearing the boss/aftermath immediately.
- The corpse copy keeps the native Godhome branch. No native statue completion
  or automatic dream return is invoked. Normal Gruz fights keep their shared
  corpse prefab, since only the owned copy is adapted.
- Final screen: no countdown, remains after waiting, Confirm/Enter returns once.
  Controls stay held and damage is blocked. A prior attack or held button must
  not dismiss the reveal. Pause cannot advance the reveal or confirm return.
- F7 during death/screen, scene exit and unload clean up the owned corpse and
  stop its camera rumble. Retry must not inherit victory input or timers.
- Rewards: first Focus/dash/projectile offer remains. Later offers show distinct
  family buckets with no investment weights/stat guarantee. No family from the
  previous offer should recur when three fresh eligible alternatives remain.
  Prerequisites/caps, deterministic seeds and baseline/reset/save behavior remain.
- Platforms: waves 1–5 open, then Center Dais / Split Steps / Staggered Steps /
  Twin Balconies / Open Stage. Repeat through wave 25, then clear for boss.
  Confirm native art and solid collision, actual basic-jump accessibility, clear
  arrival seals, and no clipping the Knight. A blocked placement is skipped.
- Standing on an old platform when choosing a card must result in a safe drop
  to the normal floor if it is removed. Old collision disappears before the new
  layout. F7, defeat, scene exit, retry and unload clean up all placed objects.
- Spike layouts remain follow-up work; no new spike/pit hazards in this rotation.

Installed 0.9.0 after the user saved and quit, with prior mod files and save
copies backed up under `.local/backups/before-gauntlet-0.9.0-20261110-124226/`.
Installed artifact hashes match. Fresh startup logs confirm seven scenes
preloaded, both platform templates resolved, the native boss/corpse layout
validated, and Gauntlet 0.9.0 initialized. The complete death/confirmation and
platform combat flows still need a live playtest.

## Historical version 0.8.0 Grimm stage and ceiling acceptance

The user reported Gruz Mother escaping upward in Elder Hu’s arena. Switch the
trial to Grimm’s stage, preserving scenery and audience. The dense local asset
audit found a solid terrain polygon ceiling at all 331 sampled positions; see
[geometry evidence](arena-geometry.md). Runtime coverage checks and a conditional
invisible barrier provide a second check before the run and boss.

Automated checks pass: 199,403 core assertions, 3,935 loadout adapter assertions,
and 261 encounter adapter assertions including native-stage ownership and
ceiling repair/cleanup. Production build passes against legacy game/API v77.

- Use the existing Gruz lever and trial idol on Attuned: arrive on Grimm’s stage.
  Tent/curtains and audience remain, with no native Grimm/NPC, bats or spikes.
  The foreground audience should follow the Knight.
- Log should show Grimm stage claimed, bounds near x=69..103/floor=5 and verified
  solid ceiling underside. If the native roof is intact, no extra barrier is made.
- Play the same 26-wave progression; wave 10 and Primal changes remain intact.
- On wave 26, watch Gruz Mother’s upward attacks hit the solid roof and return
  to combat. She must not leave the arena indefinitely.
- Check five-second victory and native return, defeat/F7/retry and restoring the
  original profile. No fallback collider or audience target may leak on exit.
- The normal Grimm statue and Pantheons remain native even while the trial idol
  is selected at Gruz Mother’s podium.

Installed 0.8.0 with matching artifact hashes after backing up prior mod files
and save copies under `.local/backups/before-gauntlet-0.8.0-20261110-111053/`.
Fresh startup logs confirm all preloads completed and version 0.8.0 initialized.
Native Grimm stage entry and the boss/ceiling fight remain for live playtesting.

## Historical version 0.7.2 balance adjustment

Wave 10: Hopper → Aspid Hunter → Hopper → Hopper → Aspid Hunter → Great Hopper.
Exactly one Great Hopper arrives last, after three regular Hoppers and two Aspids.
Wave 17: five Primal Aspids, two Hoppers and one Husk Hornhead. No regular Aspids.
Waves 18–25 alternate Primal and regular Aspids in their existing Aspid slots,
starting with Primal. A wave with one slot has one Primal. Other counts are retained.

The native Primal prefab was verified at `Deepnest_East_06 / Super Spitter` in
the local game assets. Its spitter FSM retains the native three-projectile Fire
state. It spawns at flyer height, targets the Knight, and bypasses its old-room
sleep distance. Native HP and attack timing/reach remain unchanged. Check actual
three-shot firing, airborne arrival, wake-up at distant seals, cleanup of bullets
at rewards, and original challenges remaining unaffected during playtesting.

Build passes, with 199,403 core assertions, 3,935 loadout adapter assertions and
178 encounter adapter assertions. Installed 0.7.2 with matching artifact hashes;
previous mod/save copies are in `.local/backups/before-gauntlet-0.7.2-20261110-104231/`.
Fresh logs confirm six scenes preloaded (including the new Primal prefab), and
Gauntlet 0.7.2 initialized successfully. New Primal behavior still needs a live
playtest.

## Historical version 0.7.1 balance adjustment

Wave 10 now uses Hopper → Aspid Hunter → Hopper → Hopper → Aspid Hunter:
three regular Hoppers, two Aspid Hunters and no Great Hoppers. The first Great
Hoppers now arrive in wave 15. Other waves and enemy health are unchanged.

## Version 0.7.0 acceptance

The user confirmed version 0.6.0 worked. This build passes 199,391 core checks,
3,935 real loadout adapter checks and 147 new real encounter adapter checks.
Native boundaries are simulated in adapter tests; live combat remains required.

- Enter through the existing trial idol and check normal Elder Hu/Gruz challenges
  remain unaffected. Existing baseline, cards, HP and save protection remain.
- Wave 1 has three Crawlids. All regular waves use `ceil(old count × 1.5)` enemies,
  with 200 ms warnings, distance/occupancy checks and 0.8 s stagger preserved.
- Stay near the room center: arrivals should engage without approaching their
  old alert zones. Vengeflies chase, Aspids approach/fire, walkers turn toward
  the Knight, Hoppers stay engaged and Crawlids follow a change of side.
- Check native attack reach, speed and HP remain unchanged. Extra awareness
  must not enlarge body/hit colliders or attack-range triggers.
- Wave 25 awards the last card. Wave 26 is a single native Gruz Mother, with the
  same warning and extra elite clearance. Her wake sequence and charge/slam
  attacks must work against Elder Hu’s terrain, with no original battle/title
  controller errors. The Knight’s full earned build must remain active.
- Killing her must trigger victory once: no baby Gruzzers, lingering attacks,
  card offer, normal statue completion or immediate native boss-scene exit.
- **TRIAL CONQUERED** stays for five seconds and then returns automatically.
  The original loadout is restored; controls are held and damage blocked during
  the celebration. Pause must preserve the remaining time; F7 can leave early.
- Retry from the hall. Neither the countdown nor input ownership may leak into
  the next run. Death, scene exit and unload must cancel pending victory returns.

Installed 0.7.0 after confirming the game was closed. Previous mod files and
save copies are under `.local/backups/before-gauntlet-0.7.0-20261110-102619/`.
Installed files match the staged build. Fresh startup logs confirmed six scenes
preloaded, the Gruz Mother prefab passed layout validation, and Gauntlet
0.7.0 initialized. Boss combat and automatic victory return still need a live
playtest.

## Historical version 0.6.0 acceptance

User feedback on 0.5.0: reached wave 17 and confirmed improved progression.
Version 0.6.0 builds against the installed legacy game/API. Automated verification
passes 194,980 rules assertions and 3,935 adapter assertions. The latter exercise
the production RunLoadout against simulated native healing/blue HUD boundaries;
they do not replace live Unity checks.

- Use the existing lever beside Gruz Mother, select The Hollow Trial on Attuned,
  and arrive in **Elder Hu’s arena**. Log should show four suppressed native roots
  and arena bounds near x=32..66, floor y=2. Hu, his rings and boss title stay absent.
- Select normal Elder Hu while the trial idol is selected elsewhere: it must run
  the ordinary boss. Also check normal Gruz and Pantheons are unchanged.
- After arrival countdown, verify a pale flash and **ALL UPGRADES RESET** for
  1.5 seconds. Before the first arrivals, show exactly five full masks and no
  inherited blue masks, reserve vessels or charm effects.
- Confirm movement, jump and basic nail work; dash, wall jump, double jump,
  Focus, all spells and nail arts are unavailable until their cards are chosen.
- Waves 1–5 use only Crawlids/Vengeflies. Wave 6 is the old Broken Vanguard;
  elites are at 10/15/20/25. Each enemy logs its preserved native HP.
- After wave 1, offer Focus/dash/projectile spell. Choose each on separate runs;
  check the chosen action works and unchosen actions remain locked. Test spell
  second ranks, Shade Cloak, nail arts and other movement cards as offered.
- Basic nail damage is 5. Pale Ore can exceed Pure Nail through rank eight (37).
  Check actual hits, including Strength’s native multiplier. Shaman ranks 2–5
  add +10 spell damage each above the native charm effect.
- Test Joni: native blue conversion, Focus unavailable even if previously earned,
  blue masks refill when advancing a wave, including **Tab** to skip. Check mask
  and Fragile Heart cards increase the converted pool. Check blue HUD shows the
  correct remaining health during combat and restores normally on exit.
- Check new native charm effects, especially companions after mid-run acquisition,
  Fragile Strength/Heart, Baldur Shell refill and spell-charm combinations.
- Return with F7, win, die and retry: original abilities, spell levels, Soul
  vessels, masks, charm/fragile state and nail return. Quit/reload after a run
  autosave to verify the normal save retained its original loadout.
- Keep the existing one-second clear cue plus 500 ms card rise, 200 ms arrival
  warnings, pause handling, keyboard/controller choices and repeated-run checks.

Installed 0.6.0 on macOS after confirming the game was closed. Previous mod
files and save copies are backed up under
`.local/backups/before-gauntlet-0.6.0-20261110-091626/`; installed DLL hashes match
the staged build. The first launch stalled while the Mac was locked; the user closed it.
On the subsequent requested relaunch, fresh logs confirmed five scenes repacked,
preloads completed, API v77 loaded, and Gauntlet 0.6.0 initialized successfully.
Native combat, HUD, effects and save reload remain for the user to verify.

## Confirmed before 0.2.0

- Game 1.5.78.11833 and API v77 installed on macOS after backing up originals.
- Native log confirms mod startup. User confirms Crawlid waves and card choices work.
- First silent F6 attempt was missing the mod loader, not a save-mode restriction.

## Historical version 0.5.0 acceptance

Use a backed-up save with Hall of Gods access; Godseeker mode is not required.
Start without other gameplay mods. Repeat on Fedora after macOS checks pass.

### Arena ownership and return

- Press F6 to arm Gruz arena in the Hall of Gods, enter Gruz Mother at **Attuned**, and
  complete the native dream arrival. A visible three-second countdown starts
  the trial without clicking or pressing F6 in the arena. Pause mid-countdown:
  it must resume with the same time remaining. F6 during a pending countdown
  must not restart it. Death, exit and unload must cancel it.
- Gruz Mother and her original battle FSM stay inactive. Arena terrain, camera,
  dream arrival, death and return remain native.
- Run a normal **unarmed** Gruz challenge: the original boss should still work.
- Arm then enter another challenge: the armed flag clears; that boss is unchanged.
- Ascended/Radiant variants and Pantheon fights are never taken over.
- F7 during countdown, combat, cards, victory or an aborted run restores input,
  clears upgrades, cancels pending arrivals and returns via the statue's gate.
- Trial wins do not mark the Gruz statue complete or trigger native boss rewards.
- Unloading in the arena cancels the trial and re-enables the original battle roots.

### Arrivals and enemy behavior

- Each wave starts with 1.5 seconds of breathing room. Arrival seals then warn
  for 200 milliseconds before activation; successive warnings start at least 0.8 seconds
  after the previous spawn. Markers have no colliders or contact damage.
- Locations are fixed near arena edges, chosen farthest from the current Knight.
  The native room is 17 units wide (walls at x=86 and x=103); both kinds use
  4.5 units of clearance there, with entry positions four units inside each wall.
  Larger rooms retain 7/10-unit normal/elite clearance. Test arrivals with the
  Knight standing at the exact center; they must not stall permanently.
- Move onto a warning before it completes: no enemy appears there; a new location
  gets the full warning. Camping/occupied portals delay arrivals, not overlap them.
- Foes not yet spawned count toward the wave, so early kills cannot skip arrivals.
- Verify Hornhead charges, Hopper jumps, Aspid projectiles, and the Great Hopper
  landing behavior. Prefab FSM startup/home positions may need runtime adjustment.
- Verify floor/wall raycasts locate valid room boundaries. If not, the trial
  should abort with a log entry instead of spawning at an unsafe fallback point.
- Destroying/disabling an enemy without its death event aborts without a reward.
- Native projectiles are purged between waves; card choices reject incoming damage.
- Verify all twenty curated waves match README. Wave five must continue into
  its reward and wave six; victory occurs only after wave twenty. Confirm
  nineteen drafts, no final extra card, then restart twice.

### Cards and effects

- Three portrait cards display ivory filigree frames, distinct emblems, readable
  live text and rank markers. Selected card lifts and brightens with a gold edge.
- Check 1280×720, 1920×1080 and Mac window sizes; mouse, Left/Right and controller
  menu-confirm should select exactly one card. Tab/Forgo skips.
- Confirm, skip and pause cannot leave the Knight's input locked.
- Honed Edge: +2 nail damage per stack, player attacks only.
- Soul Well: +3 Soul per successful Soul-granting hit per stack, within game caps.
- Arcane Spark: +5 per player spell hit per stack; verify projectile/dive/scream
  sources use the player-attack layer and reach HitInstanceHook.
- Quiet Respite: heal stack count after accepting a card, capped by normal max HP.
- Patient Shell: block every 6th/5th/4th damaging hit at 1/2/3 stacks. Zero damage
  does not advance the counter; verify hazard and overcharm interactions.
- Last Light: +4 nail damage per stack at exactly one mask, removed after healing.
- Death, return, scene exit, restart and unload remove all temporary bonuses.

## Validation limits

Rules tests cover roster composition, HP, complete runs, deterministic drafts,
card caps, lifecycle cleanup, portal bounds, clearance, occupied portals and
rechecking a warning entered by the player. C# builds use matching legacy refs.
Runtime results for the new room/enemies must be recorded separately; passing
rules tests does not establish prefab AI or scene-transition correctness.

## Recorded validation, 2026-11-09 (0.2.0)

- **80,964 assertions pass**, including mixed-roster runs and spawn clearance checks.
- Legacy-reference release build passes. NuGet reported an unavailable package
  vulnerability feed (NU1900); there were no C# compile errors.
- Installed into the closed game with the previous mod backed up under
  `.local/backups/before-gauntlet-0.2.0-20261109-233147/`. Loader and saves unchanged.
- Startup test initially caught a wrong Great Hopper scene reference; corrected
  it to the standalone `Deepnest_East_14b / Giant Hopper` prefab.
- After relaunch, ModLog.txt confirms all four enemy preloads and
  `GauntletMod : 0.2.0-experimental`, followed by `Experimental gauntlet ready`.
- The native UI inspection tool timed out. New arena takeover, live enemy AI,
  spawn timings, return flow and in-game card appearance still need user playtest.

## Version 0.2.1 regression

The first arena playtest reached the new room, but F6 logged "Could not locate
the Gruz arena walls." Inspection of the scene's transformed terrain colliders
shows x=86..103 (17 units); the old 18-unit minimum falsely rejected it. The
minimum is now 14, and arrival clearance adapts to the actual width. Regression
tests use the real 17-unit geometry, including the exact center position.

Startup is now automatic after three ready/unpaused seconds, with a one-shot
countdown tested for loading, pause, completion, retry and cancellation. The
mouse-dependent Begin Trial control was removed. Actual countdown and combat
in the installed game still need playtest confirmation.

0.2.1 validation: **80,995 assertions passed**; matching-reference release build
passed. Installed mod hash matches the build artifact. After relaunch, ModLog.txt
confirms all preloads and `GauntletMod : 0.2.1-experimental`, with the new
countdown instructions. Previous mod is backed up at
`.local/backups/before-gauntlet-0.2.1-20261109-233922/`. Saves and loader unchanged.

## Version 0.2.2 spawn adjustment (2026-11-10)

The user's 0.2.1 playtest log records Hornhead/Hopper arrivals in wave one and
Aspid/Hornhead/Aspid arrivals in wave two, with arena bounds 86..103, floor 14,
and no spawn exception. This confirms scheduled instantiation, not each AI's
correct behavior. Inward placement addresses the user's next requested test.

Entry points now stay at least four units inside either wall (x=90..99 in the
native room), rather than two. Centered-player clearance adjusts to 4.5 units.
The arrival warning and marker caption share a single 0.2-second constant.
The three-second startup countdown and 0.8-second interarrival gap are unchanged.
Retest each enemy's visibility, collision and AI at the new entry points.

0.2.2 validation: **81,005 assertions pass**, including four-unit wall margins,
real-room clearance, and the exact 200 ms warning setting. The release build
passes with matching legacy references. Live behavior at the revised entry
points remains for the next playtest.

## Version 0.2.3 reward transition (2026-11-10)

- After the last enemy of a nonfinal wave dies, show an ivory/gold **WAVE N
  COMPLETE** cue with expanding rules and a quick fade for 500 ms.
- The following 500 ms slides the cards upward 130 UI pixels with cubic easing
  and fades them in. The background dim also deepens during the reveal.
- Keyboard, controller, mouse selection and skipping stay locked through both
  stages (one second total), preventing the final combat input selecting a card.
- Use unscaled animation time so combat hit-stop does not stretch the reveal;
  an explicit pause stops the transition. Damage remains blocked while choosing.
- Test pause in each half, held combat input at wave end, skip before/after
  unlock, subsequent rewards, F7 cancellation, death and scene exit.
- Rules tests cover cue/reveal boundaries, one-second completion, input unlock,
  pause, cancellation and fresh timing for subsequent waves. All **81,016**
  assertions and the matching-reference release build pass. Actual animation
  appearance still needs a live playtest.

## Version 0.3.0 timing and loadout acceptance (2026-11-10)

- Wave-complete cue now lasts one second, then card rise/fade lasts 500 ms.
  Input unlock is 1.5 seconds after wave completion. Pause freezes the reveal.
- Before starting, record masks/partial shards, nail tier/damage, equipped and
  owned charms, health and Soul on a backed-up save. On run start, verify five
  masks, five nail damage, no charm effects/inventory, zero partial shards and
  empty Soul. Movement/spell tiers and Soul capacity should be unchanged.
- Mask Shards should add one full vessel (a group of four shards) and heal one,
  with a maximum of nine masks. Pale Ore should advance native nail damage to
  9/13/17/21 and never receive the old flat Honed Edge bonus on top.
- Eight curated charm cards are one-time boons: Quick Slash (32), Longnail (18),
  Shaman Stone (19), Spell Twister (33), Soul Catcher (20), Grubsong (3), Quick
  Focus (7), Steady Body (14). They set the native equipped flags without granting
  persistent ownership or spending notches. Verify each effect and native icon.
- Acquiring non-health cards must not fully heal. Native CharmUpdate performs a
  full heal; the adapter explicitly restores wounds after refreshing effects.
  Mask cards heal the newly gained mask, and Quiet Respite heals separately.
- After victory, F7, scene exit and mod unload, verify the original masks, partial
  shards, nail progression, charm ownership and equipped list return. On death,
  verify native dream return completes, without resurrection or charm breakage,
  and the original loadout is restored outside the arena.
- Save/quit during a run, relaunch and verify original progression survived.
  The before-save hook overlays the original loadout onto a separate shallow
  PlayerData clone, including completion percentage (native save recalculates it
  from temporary progression before the hook). The live singleton is never swapped.
- Test a different save after quitting: no prior loadout may be applied to it.

Automated coverage includes the real RunLoadout adapter compiled against a
minimal native boundary: save cloning, list aliasing, wound preservation around
native-style full heals, original health/blue health and charm restoration,
death without a heal, unrelated progress preservation, another save, and unload.
These boundary tests do not establish native HUD/FSM correctness; the live checks
above remain required.

0.3.0 recorded validation: **114,174 rules assertions and 313 real-adapter
boundary assertions pass**. Release build passes against the legacy game/API.
After installation and relaunch, the native log confirms preloading and
`GauntletMod : 0.3.0-experimental`; installed DLL hash matches the build artifact.
Current save files and the previous mod were copied to
`.local/backups/before-gauntlet-0.3.0-20261110-073609/` before installation.
The installer did not edit save contents. Native HUD, boon effects, real autosave
serialization and post-run restoration still require the acceptance playtest.

## Version 0.4.0 health HUD and podium (2026-11-10)

Native asset inspection found `health_display` on the HUD's eleven health slots.
Its `Check Max HP` state checks `maxHealth` against `Health Number` and routes
excess slots to `Inactive`. The previous refresh only updated full/empty masks,
leaving surplus slots visible. The new callback reruns that native maximum check
after applying/restoring loadouts, without destroying mask objects or changing HP.

- Start a run from a nine-mask save: exactly five full mask slots should show.
- Take damage: only those five slots should change full/empty state.
- Choose Mask Shards: reveal one new slot; preserve wounds except the one-mask heal.
- Finish, F7 or exit: original maximum, filled/empty masks and blue health return.
- Open/close inventory, pause and reload a save: no stale hidden or extra slots.

The Hall of Gods scene already includes an inactive `alt_lever` and animated
`Base/StatueAlt` under `GG_Statue_Gruz`. We activate the actual BossStatueLever,
bind it as the statue's native toggle, and place a new transparent idol sprite
under its alternate display. Native strike feedback, statue Down/Up animations,
audio and inspection UI are retained. A cloned alternate BossScene routes to the
same arena; original Gruz scene assets are not edited.

- Strike the lever with the normal nail: native swap raises the gauntlet idol.
- Inspect: title reads The Hollow Trial, with a gauntlet description. Select Attuned.
- Enter: countdown and gauntlet run still work. Return: idol remains selected,
  ready for another visit, without needing F6 to re-arm.
- Strike again: normal Gruz Mother returns. Normal Attuned/Ascended challenges
  must still behave normally. Hornet's own lever and other statues are unchanged.
- Spam lever/F6 during a swap: no overlapping swap or permanently locked lever.
- Save/quit with gauntlet selected: the selector, seen/unlock and tier writes
  are virtual and must not change Gruz Mother's genuine completion fields.
- Unload the mod mid-swap: stop the swap, restore original display/position,
  inspection control, prior lever visibility and original scene/details.

The generated idol is embedded in the DLL, so there is no separate image install.
See docs/asset-prompts.md for the generation prompt. Native appearance and live
lever/HUD behavior remain acceptance playtests, not established by a compile.

0.4.0 recorded validation: **114,174 rules assertions and 337 adapter-boundary
assertions pass**, including health-HUD refresh callbacks after baseline/card
changes and restoration. The matching-reference release build passes. Installed
DLL hash matches the artifact, and native startup logs confirm preloading and
`GauntletMod : 0.4.0-experimental`. Mod and save copies were backed up under
`.local/backups/before-gauntlet-0.4.0-20261110-075441/`. Lever setup runs on Hall
of Gods entry, so live lever swaps, native mask appearance and save-state
preservation still need the acceptance playtest above.

## Version 0.5.0 extended trial (2026-11-10)

- HUD shows wave 1/20 through 20/20. All wave numbers are sequential.
- Sections 1–5, 6–10, 11–15 and 16–20 have distinct formations and growing HP.
  Elite gates occur at 5/10/15; the final encounter includes two Great Hoppers.
- Clear wave five: the run must keep its loadout, offer a card and proceed to
  wave six. Repeat at 10 and 15. Original loadout restoration occurs at the
  actual end (20), death/exit or unload, not an earlier milestone.
- Nineteen drafts precede the finale. Main-stat options appear at checkpoints
  5/10/15/19 while uncapped main stats exist; duplicate and capped cards do not.
- Follow-up weighting respects the selected build and stays deterministic for a
  seed. Check supported charm/support cards still appear throughout the run.
- Try a focused nail/mask build and a spell/Soul build. Observe actual cap pacing,
  damage, healing resources, late-wave pressure and total run length. The
  simulations in progression-balance.md assume every wave is cleared and do not
  establish combat difficulty or duration.
- Full-run save/quit, death and victory must restore the original profile, HUD
  and charm effects. A second run must reset to the basic Knight and wave one.

0.5.0 recorded validation: **193,919 rules/simulation assertions and 438
adapter-boundary assertions pass**. The production draft code was simulated
for 1,000 nail/mask-focused and 1,000 spell/Soul-focused seeds; results are in
progression-balance.md. Tests verify all twenty waves, nineteen drafts, milestone
main-stat options, capped-card exclusion, seeded repeatability and cleanup.
The release build passes against matching legacy references. Native startup
logs confirm `GauntletMod : 0.5.0-experimental`; installed DLL hash matches the
artifact. Original mod files and current save copies are under
`.local/backups/before-gauntlet-0.5.0-20261110-080744/`. The new twenty-wave combat
balance and late-run save/restore behavior still need live playtesting.
