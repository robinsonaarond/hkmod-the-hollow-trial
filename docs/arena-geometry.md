# Camera-aligned visible ceiling — 0.14.0

- Sample the gameplay camera at viewport y=1 and the combat plane's depth after
  dream arrival; ceiling underside = sampled world Y − 0.35.
- Replaces the fixed floor+10/y=38 ceiling. No pixel-to-world guess is required.
- Solid terrain box spans walls plus two units on each side, thickness two units.
- Dense upward rays verify its underside at the calculated height.
- Native wide Godhome stone-platform artwork tiles across the underside, rotated
  180 degrees with overlapping joins. Tile colliders and gameplay scripts are
  disabled; the independent verified box supplies collision.
- Bell: right−1.3, ceiling−1.8. Spawn-body fitting uses the calculated ceiling.
- Retry recalibrates both collision/artwork and recreates the bell. Scene exit
  and unload dispose all owned geometry and artwork.
- Native arena geometry remains intact. Visual placement still needs live acceptance.

---

# Trial ceiling update — 0.13.0

- Arena floor remains y=28; walls remain x=3..37.
- A trial-owned solid terrain box always lowers the roof underside to y=38.
- Ceiling width: 38 units, overlapping both walls; thickness: 2 units.
- Bell center: x=35.7, y=36.2; Mantis Claw provides access.
- Native roof and scenery remain intact. Added collision is removed on exit/unload.
- Upward terrain rays verify the lowered roof across the interior.
- Arrival height uses transformed box-collider corners, including disabled intro
  colliders. A 0.15-unit gap clears the floor/roof. Large roots are raised as needed.
- Every roster prefab was checked locally: all use solid box body colliders.
  Mantis Warrior and Heavy Sentry have body bottoms well below their root pivots;
  Broken Vessel starts with its collider disabled during the intro.
- Floor spikes and arrival occupancy checks are removed. Portals alternate at
  x=7 and x=33, retaining the 0.2-second cloud and 0.8-second stagger.
- Live visibility and boss behavior require in-game acceptance.

---

# Soul Tyrant arena geometry — 0.10.0

Verified against the local macOS Hollow Knight 1.5.78.11833 scene assets.
Current room: `GG_Soul_Tyrant`, BuildSettings index 462.

| Property | Observed value |
| --- | --- |
| Combat floor | y=28 |
| Side walls at floor+2 | x=3 and x=37 |
| Floor width | 34 |
| Solid roof | Active, enabled, non-trigger terrain `World Edge v2`, layer 8 |
| Dense ceiling audit | 331 samples, x=3.5..36.5, zero gaps |
| Physical roof underside | y≈44.1 throughout |

`SoulTyrantArena` disables `Dream Mage Lord`, `Dream Mage Lord Phase2`, and
`_Enemies` while preserving the rest of the room. The separate `_SceneManager`
contains the native environment MonoBehaviour and music FSM. Keep both enabled;
the music FSM has the Boss Statue/Wait for Event states and receives `GG MUSIC`
when the trial begins. This replaces the native boss intro's event without
activating either phase or its floor-transition sequence.

The original active states are restored on release. Only the trial idol's
pending scene is eligible for takeover; ordinary Soul Tyrant and Pantheon visits
keep their native encounters. The current runtime ceiling test and conditional
barrier described below remain active. For this room the barrier's underside is
y=44, overlapping both walls if native coverage is missing.

## Historical Grimm geometry — 0.8.0

Verified against the local macOS Hollow Knight 1.5.78.11833 scene assets.
Godhome Grimm is `GG_Grimm`, BuildSettings index 443. Coordinates are world units.

| Property | Observed value |
| --- | --- |
| Dream entrance | x≈83.1, y≈5.69 |
| Combat floor | y=5 |
| Side walls at floor+2 | x=69 and x=103 |
| Floor width | 34 |
| Solid polygon ceiling | Active, enabled, non-trigger `Roof Collider`, terrain layer 8 |
| Dense ceiling audit | 331 samples, x=69.5..102.5, zero gaps |
| Nearest physical roof underside | y≈13.959..23.451, arching across the room |
| Additional tile-map roof | Terrain chunks around y=23..25 |

The audit transforms collider-local points through their full parent transforms
and includes BoxCollider2D, EdgeCollider2D and PolygonCollider2D paths. The earlier
arena comparison omitted polygon paths, so its tile-map ceiling heights were
higher than the actual nearest physical roof. The detailed audit includes both.
Elder Hu’s room had no solid roof across the same horizontal check.

## Runtime checks

`ArenaCeiling` casts upward at approximately half-unit spacing across the arena,
filtering for solid terrain with a downward-facing surface. Triggers do not
count. It runs after terrain bounds are located and again before Gruz Mother’s
wave. If coverage is incomplete, a two-unit-thick invisible BoxCollider2D is
created on terrain layer 8. It spans 38 units, overlaps both walls, and has its
underside at floor+16 (y=21). Physics transforms are synchronized and coverage
is checked again. Failure stops the trial before spawning the boss.

The fallback belongs to the trial visit and is removed on scene exit or unload.
It is not installed when the native roof passes. Boundary tests exercise intact
roof coverage, a central hole, misleading trigger/upward hits, fallback size and
layer, repeat checks, failed repair and cleanup.

## Encounter and scenery ownership

Keep `Grimm Scene`, `Godseeker Crowd`, the tent, curtains, stage and terrain active.
Disable the root Grimm Scene FSM plus these encounter objects, retaining their
original enabled/active values for restoration:

- Grimm Boss, Grimm NPC and Grimm Bow.
- Both Grimm teleport effect roots.
- Grimm Spike Holder and Grimm Bats.

Bind the foreground crowd’s Target to the Knight. Restore the prior target and
original component/object states when the takeover ends. The normal Grimm
statue and Pantheons do not meet the trial’s pending-scene identity check.

The native Gruz Mother encounter still needs a live playtest in this room;
asset and boundary verification do not replace observing her ceiling collisions.
