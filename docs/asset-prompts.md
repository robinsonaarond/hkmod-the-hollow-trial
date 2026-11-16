# Card artwork

Generated with the built-in imagegen tool (no API-key CLI fallback), 2026-11-09.
Project asset: `src/Gauntlet.Mod/Assets/card-frame.png` (1024 × 1536 PNG).
Embedded resource: `Gauntlet.card-frame.png`; consumed by `CardPresentation`.

## Final generation prompt

Use case: stylized-concept. Asset type: finished reusable portrait upgrade-card
background for a Hollow Knight fan mod. Create ONE portrait card panel, 2:3
aspect ratio, fully filling the canvas. A restrained hand-drawn gothic insect
kingdom aesthetic matching Hollow Knight: midnight blue-black velvet interior,
thin ivory shell filigree around all four edges, delicate symmetrical curling
horns and moth-wing motifs, small pale crest at top center, subtle faded Godhome
gold accents, ethereal soft glow. The middle 75 percent should remain very dark
and visually quiet for readable live game text and icons that we will draw on
top. Decorate mainly the perimeter; bottom center small symmetrical ornamental
flourish. Flat front-facing 2D UI asset, crisp fine ink lines, graceful and
understated rather than ornate metal fantasy armor. NO text, NO lettering,
NO numbers, NO icons in the central text area, NO mockup screenshot,
NO perspective, NO surrounding background outside the card. High contrast
border with subtle worn ink texture.

## Integration

The six distinct upgrade emblems are drawn as line art by the UI, so they scale
with resolution. Titles, descriptions and rank markers remain live text. The
selected card lifts slightly and gets a warm ivory/gold outline. Its artwork is
brighter than the other two cards. Cards preserve the generated frame's 2:3 ratio.

# Gauntlet podium idol

Generated with the built-in imagegen tool, 2026-11-10. Project asset:
`src/Gauntlet.Mod/Assets/gauntlet-idol.png` (1024 × 1536 RGBA PNG).
Embedded resource: `Gauntlet.idol.png`. Alpha channel verified present, with
transparent corner pixels. Consumed as a SpriteRenderer below the native
Gruz Mother alternate statue root, using its existing Down/Up animator.

## Final generation prompt

Use case: stylized-concept. Asset type: transparent 2D game sprite, replacement
Godhome podium idol for a Hollow Knight fan mod named The Hollow Trial. One
original front-facing carved stone idol matching the muted hand-inked Godhome
statues: a solemn small horned insect mask mounted over three tall overlapping
card-shaped stone tablets, a pair of crossed narrow nails behind, and a compact
ribbed shell body. Strong readable silhouette, balanced symmetry, arched horn
shapes, worn pale gray limestone with subdued antique gold incised edges, black
fine outlines and flat hand-painted gray shadows, no photorealism, graceful
Hollow Knight environmental prop aesthetic. Portrait 2:3 composition, sculpture
occupies central 80 percent, complete horns and base visible. The idol ends in a
short flat base so it can sit on the existing in-game pedestal. No separate
pedestal, no ground, no scene, no cast shadow outside sculpture, no glow cloud,
no text, no lettering, no logo. Genuine transparent background with a clean alpha
silhouette. Front-facing side-scrolling game art, no perspective, no UI card frame.

## Version 0.10.4 presentation changes

The idol bitmap is unchanged. Native Gruz statue inspection found the sculpture
at local z=2.29, behind the plinth at z≈1.959, with sorting layer/order 0 and
Sprites-Diffuse Unity material. The idol now copies the actual statue renderer’s
relative depth, material, layer, sorting and mask interaction at runtime. Its tint
uses the native color multiplied by 0.25 RGB, preserving alpha. Inspection of
opaque atlas pixels found native sculpture p90 luminance ≈44/255 versus ≈180/255
in the generated stone art; this matches the restrained highlight brightness.
This replaces its brighter default unlit rendering and z=1.5 foreground depth.

Arrival clouds reuse EnemyDeathEffects.deathPuffMedPrefab from the existing
Gruz Mother preload: Death Puff Med, a native orange particle effect. Its source
prefab and material are unchanged; the owned copy is scaled to 60%, with pooling
scripts disabled, looping off and explicit cleanup. No generated artwork added.
