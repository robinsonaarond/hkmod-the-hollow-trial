# The Hollow Trial (working title)

> In the hall of heroes, you can improve, adapt, overcome, and emerge still unchanged, still... hollow.

A Hollow Knight gauntlet: fight 47 waves in Soul Tyrant’s Godhome arena and
choose temporary upgrade cards whenever the Knight levels up.

## Play version 0.14.0

Requires **Hollow Knight 1.5.78.11833** and **Modding API v77**. Use a save with
Hall of Gods access; Godseeker save mode is not required.

1. In the Hall of Gods, **strike the lever beside Gruz Mother’s podium**.
   The native swap animation reveals **The Hollow Trial** idol. F6 also toggles it.
2. Inspect the trial idol and choose **Attuned**. It now leads to **Soul Tyrant’s arena**. The entry remains at Gruz Mother’s podium.
3. After dream arrival, a three-second countdown starts the trial automatically.
   Your inherited equipment disappears with a brief pale screen flash and
   **ALL UPGRADES RESET** displayed for 1.5 seconds. Enemies wait until this cue ends.
4. Start with **five masks, a 5-damage Old Nail, movement, a single jump and Mantis Claw**.
   Dash, double jump, spells, Focus healing and nail arts are locked.
   Soul starts empty with no reserve vessels, charms or partial shards.
5. Watch for orange arrival clouds: enemies appear after a **200 ms warning**, four units
   inside the walls. Arrivals alternate sides every 0.8 seconds, even when the
   Knight or another enemy occupies the portal. Body colliders are placed fully above the floor.
   New arrivals immediately target the Knight: awareness covers the arena,
   flyers start alerted, walkers face you, and Crawlids turn to follow you.
   Native attack ranges, attack timing, movement speed and HP are retained.
6. Each **level-up** awards one card choice immediately, even during a wave.
   **Level reached** lasts one second; cards then rise over 500 ms. Choose with
   **Left/Right + Confirm**. Press **Down** to select Skip, **Up** to return to
   cards, then Confirm. **Tab** also skips. Each skip grants **+10% permanent XP**.
   Multiple levels queue multiple choices. Gameplay freezes for the entire
   selection sequence, preserving living enemies, projectiles and arrivals.
   Glowing Womb cannot consume Soul or hatch minions. Card animations use
   unscaled time; the normal pause menu also freezes the card animation.
   Climb the right wall with Mantis Claw to strike the **upper-right bell**.
   Each valid ring calls the next wave and adds **+40% temporary XP** globally,
   including enemies already alive. Repeated rings stack. A wave's final kill
   receives the current bonus, then the temporary bonus resets. The bell cannot
   call beyond the finale. Waves advance automatically when cleared, without
   awarding a card unless the kill also crossed a level threshold.
   Each new wave displays a **2.6-second wave-number/title banner**, including
   bell calls. Rapid calls immediately replace it with the newest wave.
7. Defeat **both Broken Vessels on wave 47** and watch their native death effects.
   Once all owned boss death sequences finish,
   **TRIAL CONQUERED** appears and waits for **Confirm / Enter** to return to the
   Hall of Gods. There is no return countdown. The Knight is protected during
   the death sequence and victory screen; confirmation unlocks after the reveal.
8. **F7** still returns early. F6 queues a retry after a defeated or aborted run.
   The idol stays selected during this game session. Strike the lever to return
   to Gruz Mother’s regular challenge.

The arena takeover requires the trial’s own selected challenge, so ordinary
Soul Tyrant, Grimm, Elder Hu and Gruz Mother challenges remain available. Trial wins do not award
statue completion. Pantheons remain outside the trial.

## Encounters and rewards

There are **47 unchanged waves**. Card choices follow levels rather than waves. Five new introductory waves use only
Crawlids and Vengeflies. The previous twenty formations follow at waves 6–25,
with a single Great Hopper at wave 10 and elite gates at 15 and 20. Wave 10 has
three regular Hoppers, two Aspid Hunters, and one Great Hopper arriving last.
Wave 17 replaces all five Aspid Hunters with **Primal Aspids**. From wave 18,
Aspid slots alternate Primal/regular, starting with Primal; a single slot uses
a Primal. Both retain their own native HP and attack patterns, including the
Primal’s three-shot spread. Other regular waves have **50% more enemies,
rounded up**: 2 → 3, 3 → 5, 4 → 6, 5 → 8, and 6 → 9. Reinforcements repeat the
start of each formation after its original arrivals. Wave 26 contains **three Gruz Mothers** with native attacks and health.
Waves 1–25 retain their previous formations and progression.

Waves **27–46** form a second chapter: Mantis Warriors, Winged Sentries and Heavy
Sentries join Primal Aspids and Great Hoppers in increasingly large mixed groups.
Platform layouts rotate; floor spike hazards are disabled to keep enemy arrivals clear.
Wave **47** ends with **two Broken Vessels**. Failed Champion was not selected:
his control FSM requires FK Floor, rubble, barrel and crack-pool objects from
his own arena. Broken Vessel's bounds and attack heights follow this arena;
his copied corpse cannot grant achievements or end the native Godhome challenge.

**Enemy HP is preserved from each native prefab**, with no escalating health
multipliers. More enemies and mixed formations supply the later pressure.

At level 2, the first offer is always **Focus / Mothwing Cloak / Vengeful Spirit**.
Later offers draw **three distinct, equally likely upgrade-family buckets**.
Pale Ore’s eight ranks share one bucket; Hiveblood’s single rank has one bucket
as well. Only the next available rank of a drawn family can be selected.

There is **no guaranteed stat slot and no bonus weight for investing in a card**.
The previous offer’s families sit out the next offer while enough alternatives
remain. Locked prerequisites and capped families leave the eligible pool. This
makes nail/spell damage accumulation rarer and encourages a broader build.
See [reward simulations](docs/progression-balance.md) for measured progression.

## Arena ceiling and stage

The trial uses **GG_Soul_Tyrant**, preserving its Sanctum scenery, palette, floor
and native music controller. Both Soul Tyrant phases and the native enemy root
are disabled only for the trial visit. The arena music receives its normal
Godhome start event without activating the boss. The existing Gruz Mother
podium/lever remains the entrance.

The floor spans x=3..37 at y=28: the same 34-unit width as the previous arena.
The native roof lies at y≈44.1. The trial samples the **gameplay camera's top
edge** after dream arrival. Its combat ceiling sits **0.35 world units below
that edge**, replacing the fixed y=38 height. A continuous row of inverted native
Godhome stone platforms makes the ceiling visible; a separate verified terrain
box provides collision. The bell remains 1.3 units inside the right wall and
hangs 1.8 units below the ceiling. The artwork, bell and barrier are recalibrated
on retry and removed on exit/unload. See [geometry evidence](docs/arena-geometry.md).

## Changing stage layouts

Platforms change between waves, using native Godhome floating-platform art and
solid collision. The first five waves stay open. Waves 6–25 cycle through:

1. **Center Dais:** one wide platform, 3.8 units above the floor.
2. **Split Steps:** two small platforms, 3.5 units high.
3. **Staggered Steps:** two small platforms at 3.5 and 4.5 units.
4. **Twin Balconies:** two wide platforms, four units high.
5. **Open Stage:** clear floor, used for the elite gates.

The final boss is also open. Platforms stay away from entrances and are skipped
if they would materialize across the Knight. All planned heights are within the
basic-jump design limit; movement cards are optional. Old platform collision is
removed before the next layout; the ordinary arena floor remains underneath.
Floor spikes are disabled on every wave. When the bell stacks fights, the
existing layout stays in place until that batch clears. The bell has no
supporting platform.

## Charm menu during a trial

Opening the normal inventory pauses combat, including enemy arrivals and projectiles.
Native menu animations and charm navigation continue using unscaled time.
Close inventory to resume. Open the inventory and switch to Charms. Earned charm cards appear as
owned charms, initially equipped; toggle them freely without a bench or notch
cost. Disabled charms stay disabled after new card choices and wave transitions.
Unearned charms remain unavailable. Non-charm upgrades remain active.

Menu toggles preserve remaining health and Soul. Toggling Joni or Fragile Heart
cannot refill health. Disabling Joni permits Focus again when Focus was earned;
disabling Shaman Stone also disables its extra damage ranks. The original save's
charm ownership, equipment and normal bench restrictions return on exit/unload.

## Experience and character levels

Level appears to the left of total XP at the upper left, including during card
selection. Both labels sit 26 logical pixels lower than the old XP counter. Each owned roster enemy awards its value once on confirmed
death. Duplicate/stale callbacks and cleanup grant nothing. XP resets on a new
run. The character starts at level 1. Advancing from level L costs 10 × L XP;
reaching level L requires 5 × L × (L − 1) total XP. Levels display progression
and grant one upgrade choice for each level gained. There are no automatic stat bonuses.

With no bell or skip bonuses, enemy XP gives exactly one level on 43 of 47 waves. The other four
waves grant zero or two; the level after every cleared wave stays within one
of wave number + 1. Unboosted completion yields 11,565 XP, level 48 and 47 card choices. Bell rings
grant no XP directly; XP and levels are awarded from kills using current bonuses. See
[the level balance record](docs/experience-levels.md).

Temporary and permanent bonuses appear below Level/XP. Both bonuses stack
**additively**: kill XP = base enemy XP × (100 + temporary% + permanent%) / 100.
The temporary bonus resets whenever an individual wave clears, even if other
waves remain active. The permanent bonus lasts for the run; both reset on
victory, defeat, abandonment or retry. Fractional XP awards round down per kill.

Skipping a queued choice immediately adds +10% permanent XP. It neither awards
XP retroactively nor advances an unfinished wave. Choosing cards does not repair
fragile charms or refill Joni mid-wave. Fragile charms repair at each wave clear;
Joni refills at five-wave milestones.

| Enemy | XP |
| --- | ---: |
| Crawlid | 5 |
| Vengefly | 10 |
| Husk Hornhead | 15 |
| Aspid Hunter | 20 |
| Hopper | 25 |
| Primal Aspid | 35 |
| Mantis Warrior | 40 |
| Winged Sentry | 45 |
| Heavy Sentry | 60 |
| Great Hopper | 60 |
| Gruz Mother | 100 |
| Broken Vessel | 250 |

## Cards and loadout

The pool has **53 cards, including 31 charm boons**. Charms activate without a
notch cost and show native charm icons. This is a combat-focused selection;
the inventory/map/Geo utility charms are not all in the pool.

| Upgrade | Effect and cap |
| --- | --- |
| Pale Ore | +4 base nail damage per rank, eight ranks: 5 → 9 → 13 → 17 → 21 → 25 → 29 → 33 → **37**. Four ranks beyond Pure Nail. |
| Shaman Stone | First rank equips the native charm. Four more ranks each add +10 spell damage, up to **+40 beyond the native charm**. |
| Mask Shards | +1 maximum mask and heal one per pick, four picks: nine base masks. |
| Vessel Fragments | +33 reserve Soul capacity per pick, three picks. |
| Soul Well / Arcane Spark | +3 Soul per nail hit / +5 spell damage per rank, three ranks each. |
| Quiet Respite | Heal 1–3 masks after taking a card. Disabled with Joni. |
| Patient Shell / Last Light | Periodic hit protection / +15 nail damage per rank at one mask, three ranks (+45). |
| Thorns of Agony | Five ranks. First equips the native charm; subsequent ranks add +10 thorn damage each, up to +40. At maximum rank a single thorn hit can kill a native 35-HP Primal Aspid even with the starting nail. |
| Fragile Strength | Native 50% nail damage bonus. Breaks when a hit costs health; repairs when the wave clears. |
| Fragile Heart | Two extra masks. Breaks on health loss, removing their capacity; wave clear repairs it and restores those two masks. |
| Joni’s Blessing | Initial native blue-mask conversion, then replenishment after 5, 10, 15, … total cleared waves. Bell calls, card choices, ordinary wave clears and inventory redraws preserve remaining HP. Focus and Respite healing are disabled. |
| Kingsoul | Native passive Soul regeneration, one rank, no notch cost. Uses completed Kingsoul rather than Void Heart during the run. |
| Baldur Shell | Native Focus protection, renewed each wave. |

Mantis Claw is granted at run start and removed from the card pool.
Ability cards unlock Focus, dash, double jump, Shade Cloak, crystal
dash, acid resistance, Dream Nail and the three nail arts. Each spell has two
ranks: Vengeful Spirit → Shade Soul, Desolate Dive → Descending Dark and Howling
Wraiths → Abyss Shriek. Shade Cloak requires dash; Dash Slash requires dash;
Sharp Shadow requires Shade Cloak. Healing charms require Focus. Dreamgate
travel stays disabled inside the trial.

Other charm boons include Quick Slash, Longnail, Mark of Pride, Sprintmaster,
Dashmaster, Sharp Shadow, Nailmaster’s Glory, Dream Wielder, Soul Catcher,
Soul Eater, Spell Twister, Grubsong, Quick Focus, Deep Focus, Steady Body,
Thorns of Agony, Defender’s Crest, Flukenest, Heavy Blow, Fury of the Fallen,
Hiveblood, Grubberfly’s Elegy, Weaversong, Glowing Womb and Grimmchild.

Before a run, the mod snapshots health, mask/vessel/nail progression, charms,
movement, spell levels, nail arts, Dream Nail, fragile-charm state and the
Kingsoul/Void Heart variant. It restores
them on victory, abandonment, scene exit or unload. During native death, live
restoration waits for scene exit. Autosaves receive a separate copy with the
original loadout overlaid; the live trial keeps its current build. Unrelated
save progress is retained. This is loadout protection, not a separate save mode.

Cards use the embedded generated filigree frame and native charm icons.
Native particle renderers are hidden throughout card selection and restored afterward.
Temporary full-heal HUD events during charm recalculation are suppressed.
Native health animations and their waits continue in unscaled time during card
selection and inventory pause. Health FSMs retain their animation-completion
callbacks; refreshes no longer force them into other states. Native blue-mask
rebuilds draw remaining HP without refilling it. The shared podium badge displays
the existing Gruz completion shown by the Hall entrance list. See
[asset prompts](docs/asset-prompts.md) and [playtest checks](docs/playtest.md).

## Build and tests

### Requirements and project layout

- A local copy of **Hollow Knight 1.5.78.11833**, with its platform's `Managed`
  folder. Newer Unity 6 game builds are incompatible with these references.
- **Modding API v77**, release
  [1.5.78.11833-77](https://github.com/hk-modding/api/releases/tag/1.5.78.11833-77),
  for the same platform as the game. API archives and game DLLs are not committed.
- Git and the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
  for your OS/CPU. This project's verified environment is Apple Silicon macOS,
  SDK **8.0.427**, runtime **8.0.33**. The tests and reference checker target
  `net8.0`; a newer SDK alone does not necessarily supply the .NET 8 runtime.
- Internet access to GitHub for API downloads and NuGet.org for first restore.
  `dotnet build` and `dotnet run` restore packages automatically.

`src/Gauntlet.Mod` builds `Gauntlet.dll` for .NET Framework 4.7.2 (`net472`).
`src/Gauntlet.Core` builds `Gauntlet.Core.dll` for `netstandard2.0`.
The mod project restores `Microsoft.NETFramework.ReferenceAssemblies` **1.0.3**,
so a separate .NET Framework developer pack, Mono, Unity editor or Visual Studio
installation is not needed to compile. It references API `Assembly-CSharp.dll`,
`MMHOOK_Assembly-CSharp.dll`, `MonoMod.Utils.dll` and `Mono.Cecil.dll`, plus the
installed game's `PlayMaker.dll` and `UnityEngine*.dll`.

The three projects under `tests/` are executable assertion suites using simulated
game boundaries; run them with **`dotnet run`, not `dotnet test`**. They do not
require the game or API. `tools/ReferenceCheck` checks the game version in both
`Assembly-CSharp.dll` files, the API's loader type, and required game references.
It does not independently check the API revision number; use the pinned archive.
`docs/` contains design and playtest records; `plan.md` is historical planning.

### macOS (current development environment)

Install Git (the Xcode Command Line Tools provide it) and the .NET 8 SDK for your
CPU. Bash, curl, unzip and shasum are used by the scripts. From a fresh checkout:

```sh
git clone https://github.com/robinsonaarond/hkmod-the-hollow-trial.git
cd hkmod-the-hollow-trial
dotnet --list-sdks
dotnet --list-runtimes
./scripts/setup-api.sh
./scripts/test.sh
./scripts/build.sh \
  "$HOME/Library/Application Support/Steam/steamapps/common/Hollow Knight/hollow_knight.app/Contents/Resources/Data/Managed" \
  "$PWD/.local/api"
```

Adjust the game path for a custom Steam library. The existing development
workspace can use its private `.tools/dotnet/dotnet` installation instead of a
system `dotnet`; `scripts/common.sh` automatically prefers it. The scripts put
CLI state in `.tools/dotnet-home` and NuGet packages in `.tools/nuget`.
These local tools and caches are ignored by Git and are not included in a clone.

`setup-api.sh` verifies the pinned macOS archive checksum and extracts reference
files into `.local/api`. It **does not install the loader into the game**.
The build runs the reference checker before compiling and stages exactly the
mod's two DLLs and this README in `artifacts/Gauntlet/`.

### Windows (PowerShell)

1. Install [Git for Windows](https://git-scm.com/downloads/win) and the .NET 8 SDK
   linked above (normally x64). Open a new PowerShell window so PATH is refreshed.
   These commands work in Windows PowerShell 5.1 or PowerShell 7; WSL and Bash
   are not needed.
2. Install/select game version **1.5.78.11833**. For Steam, use the game's
   Properties → Betas/Game Versions to select the matching previous version
   where available. Browse the game's local files to locate `hollow_knight_Data\Managed`.
3. Clone and prepare references. Run all following blocks in the same PowerShell
   window, from the repository root. Downloads only prepare build references.

```powershell
$ErrorActionPreference = 'Stop'
git clone https://github.com/robinsonaarond/hkmod-the-hollow-trial.git
if ($LASTEXITCODE -ne 0) { throw 'Clone failed' }
Set-Location hkmod-the-hollow-trial
dotnet --list-sdks
dotnet --list-runtimes
# Confirm an 8.0 SDK and Microsoft.NETCore.App 8.0 runtime are listed.
$env:DOTNET_CLI_HOME = Join-Path $PWD '.tools\dotnet-home'
$env:NUGET_PACKAGES = Join-Path $PWD '.tools\nuget'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
New-Item -ItemType Directory -Force .tools, .local\api | Out-Null
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$url = 'https://github.com/hk-modding/api/releases/download/1.5.78.11833-77/moddingapi.v77.windows.zip'
Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile .tools\api-windows.zip
$expected = 'bc9f0db3d0916b05cd5a2420bb602fb1b239ce3ff6c289fd84bffb682fb8f1d6'
if ((Get-FileHash .tools\api-windows.zip -Algorithm SHA256).Hash -ne $expected) {
    throw 'API checksum mismatch; delete the archive and download it again.'
}
Expand-Archive .tools\api-windows.zip -DestinationPath .local\api -Force
```

4. Run all three test programs. A nonzero exit code stops the sequence.

```powershell
foreach ($project in @('Gauntlet.Tests', 'Gauntlet.Loadout.Tests', 'Gauntlet.Encounter.Tests')) {
    dotnet run --project "tests/$project" --configuration Release -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { throw "Tests failed: $project" }
}
```

5. Set the real game path, validate references, build, then stage the output:

```powershell
# Change this if Steam or the game is on another drive/library.
$game = 'C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed'
$api = (Resolve-Path '.local\api').Path
if (-not (Test-Path $game -PathType Container)) { throw 'Correct the game Managed path first' }
dotnet run --project tools/ReferenceCheck --configuration Release -p:UseSharedCompilation=false -- "$game" "$api"
if ($LASTEXITCODE -ne 0) { throw 'Game/API reference check failed' }
dotnet build src/Gauntlet.Mod --configuration Release -p:UseSharedCompilation=false "-p:GameManagedPath=$game" "-p:ApiManagedPath=$api"
if ($LASTEXITCODE -ne 0) { throw 'Build failed; do not install stale output' }
New-Item -ItemType Directory -Force artifacts\Gauntlet | Out-Null
Copy-Item src\Gauntlet.Mod\bin\Release\net472\Gauntlet.dll artifacts\Gauntlet\
Copy-Item src\Gauntlet.Core\bin\Release\netstandard2.0\Gauntlet.Core.dll artifacts\Gauntlet\
Copy-Item README.md artifacts\Gauntlet\
```

6. To play, close the game and back up saves from
   `%USERPROFILE%\AppData\LocalLow\Team Cherry\Hollow Knight` and any existing
   `Managed\Mods\Gauntlet` folder. Install the **Windows v77 loader** separately:
   back up the game's `Managed` folder, then copy the contents of the verified
   Windows API archive into that folder, retaining the archive's directory
   structure and replacing its matching files. Do not use the macOS/Linux archive.
   The game's main menu should show Modding API v77 after launch; close it again
   before copying the mod.
7. Install the staged mod with the game closed:

```powershell
$mods = Join-Path $game 'Mods'
New-Item -ItemType Directory -Force $mods | Out-Null
Copy-Item artifacts\Gauntlet -Destination $mods -Recurse -Force
```

The result must be `Managed\Mods\Gauntlet\Gauntlet.dll` alongside
`Gauntlet.Core.dll`, not a second nested `Gauntlet` folder. Start the game and
confirm Gauntlet appears in the mod list, then follow the play instructions above.
To uninstall only this mod, close the game and remove `Managed\Mods\Gauntlet`.
The Windows commands are derived from the checked-in project files and macOS
build sequence; they have not been executed on a Windows machine.

### Native Linux

Install the .NET 8 SDK, Git, Bash, curl, unzip and sha256sum through your distro's
supported installation methods. Clone the repository, then run the same setup
and test scripts as macOS. Build with your actual native Steam library path:

```sh
./scripts/setup-api.sh
./scripts/test.sh
./scripts/build.sh \
  "$HOME/.local/share/Steam/steamapps/common/Hollow Knight/hollow_knight_Data/Managed" \
  "$PWD/.local/api"
```

The setup script selects the Linux v77 archive automatically. Native Linux and
Windows under Proton use different API archives; this recipe is for native Linux.
Fedora runtime testing remains pending.

### Installation on macOS and troubleshooting

With the game closed, saves backed up and the matching v77 loader installed,
copy `artifacts/Gauntlet/` into the game's `Managed/Mods/` directory. In Finder,
right-click `hollow_knight.app` → Show Package Contents →
`Contents/Resources/Data/Managed/Mods/`. The optional
`bash scripts/install-macos.sh` helper requires an **existing** Gauntlet installation
at the default Steam path and Python 3; it backs up saves and existing mod files,
then verifies copied file hashes. It is not a first-time loader installer.

- Missing `dotnet` or .NET 8 runtime: install the SDK for your CPU, reopen the
  terminal and inspect `dotnet --list-sdks` / `dotnet --list-runtimes`.
- Reference-check failure: confirm the game version and pass the directories
  directly containing the required DLLs. Do not mix current game files with legacy API files.
- Missing API DLLs: re-extract the correct v77 archive and check `ApiManagedPath`.
- NuGet restore or `NU1900` warnings: check access to NuGet.org. Cached packages
  may permit a build while vulnerability metadata retrieval is unavailable.
- Successful build but no mod: check the loader version and installation folder;
  building only creates artifacts and does not install them into the game.

## Verification and limits

The user confirmed version 0.6.0’s arena and progression changes worked.
Version 0.9.0 restored native boss death effects, victory confirmation, equal
family buckets and platform layouts. Version 0.10.0 moves to Soul Tyrant’s arena
and music, and adds Kingsoul as a notch-free boon with protected variant state. Automated tests cover rules,
seeded offers, caps, transitions, arrivals and the real loadout adapter with
simulated native boundaries. Building checks the installed legacy references.
Version 0.10.1 fixes tightly packed charm artwork on cards and refreshes reserve-vessel visibility after loadout resets, vessel cards and restoration. The user confirmed those visual fixes. Version 0.10.2 uses native inventory icons for Focus, all six spell forms, nail arts and Pale Ore. Cards preview the next spell/nail tier; Pale Ore ranks 5–8 retain Pure Nail artwork. Version 0.10.3 makes Fragile Strength/Heart break on actual red/blue health loss and repair at wave clear, with a broken-charms HUD indicator. Blocked hits preserve them. It also restores Gruz Mother’s native localized title setup. Version 0.10.4 replaces arrival rings/captions with the native orange puff, matches the idol’s depth/lit material to Gruz Mother, and moves a compact Perpetua status display to the upper right. The latest visual changes still need live playtesting. Version 0.11.0 adds the second chapter, wave stacking, XP and the card-selection gameplay freeze. Live acceptance of the new enemies, spikes, bell placement and double-Vessel finale is pending. Version 0.13.0 lowers the ceiling and bell, pauses inventory combat with live menu animations, removes floor spikes and spawn crowd checks, adds controller skip selection, reduces skip XP to 10%, adds five Thorns ranks and buffs Last Light. Live acceptance remains pending. See [the acceptance record](docs/playtest.md).

Fedora runtime remains untested. There is no dedicated save mode, custom boss,
metaprogression, XP or Geo purchasing yet. Spawned kills may update ordinary
journal progress. Save backups are under `.local/backups/`. Continue playtesting
with a backed-up save and no other gameplay mods.
