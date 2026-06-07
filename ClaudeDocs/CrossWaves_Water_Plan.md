# CrossWaves — Ocean & Coastline Water Effect: Implementation Plan

> This plan is written to be executed by Claude Code in strict, linear order.
> Do not reorder steps. Do not merge an editor step into a code step.
> Each step is tagged. Finish, verify, and (where noted) commit before moving on.

**Step tags**
- `[INFO]` — record/confirm information; no change to the project.
- `[ASSET]` — a human must supply or import an asset (Claude Code cannot author it).
- `[CODE]` — write or edit a text file (script or shader). Claude Code's main job.
- `[EDITOR]` — an action performed in the Unity Editor UI. **Always its own step.**
- `[TEST]` — enter play mode and verify a specific outcome before continuing.
- `[DECISION]` — a human judgement call to make before proceeding.

---

## 1. What we are building

A calm, **deep ocean blue** water surface that fills the entire screen and sits
**behind the single Screen Space – Camera canvas**, present on every screen of the app.
Wherever the crossword's filled (white) letter-cells sit, they read as a **single
connected landmass**; the empty (black) cells and the area outside the grid read as
**sea**, with **animated white foam forming a coastline** where land meets water.

The effect is built in four phases. Phases 1–3 are the always-visible, non-negotiable
look and form a shippable milestone on their own. Phase 4 (tap ripples, completion
splash) is a **separated, optional final layer** that is auditioned last and kept only
if it fits the calm mood.

---

## 2. Locked decisions & principles (do not relitigate)

These were settled deliberately. If something later seems to argue for changing one,
stop and ask the human — do not "helpfully" switch it.

1. **Render pipeline: URP 17.4.0 (Unity 6).**
2. **Renderer: Universal Renderer — NOT the 2D Renderer.**
   The renderer choice is about the lighting/feature model, not how flat the game looks.
   Water needs a standard directional-lit specular look and a Full Screen Pass Renderer
   Feature; both are the Universal Renderer's home turf. The 2D Renderer (built for
   Light2D) would fight us. Top-down look comes from the camera/UI, never the renderer.
3. **Camera stays on Perspective. Do not change it.**
   Water is computed in screen space, so projection is irrelevant to it. The UI is a
   single Screen Space – Camera canvas already laid out around this camera; changing
   projection only risks the layout for no benefit.
4. **Shaders are hand-written HLSL `.shader` files, NOT Shader Graph.**
   Shader Graph is easier for a human in the editor but its files are not text-editable
   in any sane way. This workflow is agent-driven, so HLSL text is the correct choice.
5. **No changes to the JSON databases.**
   The land mask already exists in each grid's `rows` (`.` = land/white cell,
   `#` = sea/black cell). The distance field is computed at load into ONE reused
   texture. Nothing is baked into or read from JSON for this feature.
6. **Calm, not dramatic.** Gentle lapping, low foam, slow motion, soft glints.
   "Water hitting the words" = lazy shoreline lapping, never crashing surf.
7. **One coastline.** White cells are a single connected landmass; black cells are
   bays/inlets. (They already render as "off", so the water behind shows through them
   for free.)
8. **Foam appears only when a puzzle is on screen.** Menus show plain calm water.
9. **Wide device target (App Store, incl. older iPhones on Metal).** Keep the
   full-screen fragment shader cheap: a small, fixed number of texture samples, half
   precision where safe. A half-resolution water pass is an available optimization if
   needed (see §8).
10. **Phase ordering is a principle, not just an order.** Ambient water and the
    coastline are *spatial* problems; ripples/splashes are *event* problems. Build and
    freeze the spatial layer first; add the event layer last so the base never gets
    disturbed and the negotiable layer can be cut cleanly.

---

## 3. Scene facts this plan relies on (confirmed)

- One scene, one canvas, **Screen Space – Camera**. Menus are toggled via **CanvasGroups**
  within that one canvas. The water object lives once in this scene and never needs to
  persist across loads or be toggled off.
- The grid is built at runtime by a **load script** that reads a crossword file and lays
  out cells. White (letter) cells are opaque Images; **black cells are turned off** (not
  drawn), so the area behind them is already transparent to whatever is behind the canvas.
- The grid sits in a **fixed-size container**, the same on all screens. Its on-screen
  rectangle is therefore effectively constant and can be read once from the container's
  `RectTransform`.

> **Action for the executor before Phase 2:** open the load script and record (a) the
> name/type of the grid container object and its `RectTransform`, (b) the data structure
> that holds which cells are white vs black, and (c) any event/callback fired when a grid
> finishes loading and when a puzzle is closed. These three hooks are needed in Phase 2.
> Do not guess them — read the code.

---

## PHASE 0 — Pipeline foundation

Goal: project is on URP with the Universal Renderer, and nothing visually broke.

- **0.1 `[INFO]`** Record the exact Unity Editor version (expected Unity 6 / 6000.x) and
  confirm `com.unity.render-pipelines.universal` is **17.4.0** in the Package Manager.
  Write both into this plan or a NOTES file.
- **0.2 `[INFO]`** Make a full backup or commit a clean checkpoint of the project before
  any pipeline change. Pipeline switches are disruptive; we want a clean rollback point.
- **0.3 `[EDITOR]`** Create a **URP Pipeline Asset using the Universal Renderer**
  (Project window → Create → Rendering → URP Asset (with Universal Renderer)).
  This produces a URP Asset and a Universal Renderer Data asset. **Do not pick 2D
  Renderer.**
- **0.4 `[EDITOR]`** Assign the new URP Asset in **Project Settings → Graphics** (Scriptable
  Render Pipeline Settings) and in **Project Settings → Quality** for each quality level.
- **0.5 `[EDITOR]`** Select the Main Camera. Leave **Projection = Perspective**. Set
  **Clear Flags = Solid Color** (the color won't be seen once water draws, but keeps a
  clean base). Confirm the camera's Renderer is the new Universal Renderer.
- **0.6 `[TEST]`** Enter play mode. Confirm: the UI/canvas still renders correctly; no
  magenta/pink materials anywhere; menus still toggle. Fix any pink materials (they'd be
  non-UI materials needing URP-equivalent shaders) before continuing.

---

## PHASE 1 — Ambient calm ocean

Goal: a calm deep-blue water surface filling the screen, behind the UI, with no
awareness of the grid yet. This alone should make the app "look like the sea".

- **1.1 `[ASSET]`** Import a **seamless tiling water normal map** (a single normal map is
  enough; we sample it twice at different scales/speeds). Free options exist online.
  Import settings: **Texture Type = Normal map**, **Wrap Mode = Repeat**, mipmaps on.
  Record its asset path.
- **1.2 `[CODE]`** Write the fullscreen water shader, e.g.
  `Assets/Shaders/OceanFullscreen.shader`, as a **URP fullscreen-compatible HLSL shader**
  (use the URP Blit fullscreen vertex from
  `Packages/com.unity.render-pipelines.universal/ShaderLibrary/Blit.hlsl`; the fragment
  receives screen UV via the Blit varyings). Algorithm for this phase only:
  - Properties: `_DeepColor` (deep blue), `_ShallowColor` (slightly lighter blue),
    `_NormalMap`, `_NormalScaleA`, `_NormalScaleB`, `_ScrollSpeedA`, `_ScrollSpeedB`
    (different directions/speeds), `_SpecColor`, `_SpecPower`, `_LightDir` (a fixed
    virtual light direction), `_SwellStrength`.
  - Sample the normal map twice using screen UV scaled by `_NormalScaleA/B` and offset by
    `_ScrollSpeedA/B * _Time`. Blend the two into one perturbed surface normal.
  - Compute a cheap Blinn-Phong-style specular glint from `_LightDir` and the perturbed
    normal → moving highlights. Tint base between `_DeepColor` and `_ShallowColor` by a
    subtle factor of the normal so the surface has gentle variation.
  - Optional very subtle large-scale UV wobble (`_SwellStrength`) for slow swell.
  - **Keep it to 2 normal samples total.** Use `half` precision.
  - The shader ignores the existing camera color (this is a background fill), or blends
    over it if there is opaque scene content (there normally isn't).
- **1.3 `[EDITOR]`** Create a **Material** from `OceanFullscreen.shader` (e.g.
  `Assets/Materials/Ocean.mat`). Assign the normal map from 1.1. Set `_DeepColor` to a
  deep ocean blue and reasonable starting values for scroll speeds (slow) and specular.
- **1.4 `[EDITOR]`** On the **Universal Renderer Data** asset, **Add Renderer Feature →
  Full Screen Pass Renderer Feature**. Set its **Material = Ocean.mat**. Set
  **Injection Point = Before Rendering Transparents** (so the canvas, which renders in the
  transparent queue, draws on top of the water). Leave requirements default.
- **1.5 `[TEST]`** Enter play mode. **Expected:** calm deep-blue water fills the whole
  screen, gently moving, with the UI drawn on top of it. Tune scroll speed, normal scale,
  and specular in `Ocean.mat` until it reads as calm ocean.
- **1.6 `[TEST] / RISK CHECK`** Confirm the water is **behind** the canvas, not on top of
  it. If the water draws over the UI, the injection-point/queue ordering is wrong for this
  setup → switch to the documented fallback in §7 (world-space water quad) before
  proceeding. Do not continue to Phase 2 until water is correctly behind the UI.

---

## PHASE 2 — The coastline (foam where land meets sea)

Goal: feed the grid's land mask to the water shader and draw an animated white foam band
along the single connected coastline (outer edge + black-cell bays), only when a puzzle
is on screen. This is the payoff.

- **2.1 `[CODE]`** Write `Assets/Scripts/Water/CoastlineSDF.cs` — a pure data utility that:
  - Takes the 13×13 land mask (white = land) plus a configurable **padding** (a few cells
    of margin so the outer coastline has room for foam).
  - Computes a **signed distance field** (distance from each texel to the nearest land
    edge) via a CPU distance transform, at a modest resolution (start ~4 texels per cell,
    i.e. roughly a 64–80px field; tunable). This is tiny and fast.
  - Writes the result into a **single reused `Texture2D`** (R8 or RFloat). The texture is
    allocated **once** and rewritten in place (`SetPixels`/`Apply`) on each new grid — no
    per-load allocation, so menu/puzzle transitions don't hitch.
- **2.2 `[CODE]`** Write `Assets/Scripts/Water/WaterController.cs` — the runtime glue:
  - Holds references to the `Ocean.mat` material, the grid **container RectTransform**, and
    the camera.
  - On **grid loaded** (hook the event identified in §3): read the white/black mask from
    the load script's data, call `CoastlineSDF` to (re)fill the SDF texture, and push to
    the material: `_SDFTex`, and `_GridRectUV` = the grid container's rectangle expressed
    in **screen UV** (use `RectTransformUtility.WorldToScreenPoint` on the rect corners,
    divided by screen size; since the container is fixed this can be computed once and
    cached).
  - Set foam params on the material: `_FoamColor` (white), `_FoamWidth`, `_FoamSoftness`,
    `_FoamSpeed`.
  - Expose `EnableFoam(bool)` which sets a material keyword/float `_FoamOn`.
- **2.3 `[CODE]`** Extend `OceanFullscreen.shader` to add the coastline:
  - New properties matching 2.2: `_SDFTex`, `_GridRectUV`, `_FoamColor`, `_FoamWidth`,
    `_FoamSoftness`, `_FoamSpeed`, `_FoamOn`.
  - In the fragment: map the current screen UV into the SDF's UV using `_GridRectUV`.
    If outside that rect (with padding), output plain water (no foam). Otherwise sample
    `_SDFTex` once to get distance-to-land.
  - Draw foam where distance is within `_FoamWidth` of the coastline, using
    `smoothstep` with `_FoamSoftness` for a soft edge, modulated by a slow scrolling noise
    (reuse the normal map or a cheap noise) and a gentle sine over `_Time * _FoamSpeed` so
    the foam breathes. Blend `_FoamColor` over the water color. Gate the whole foam term
    by `_FoamOn`.
  - **Budget:** this adds **one** SDF sample (and at most one cheap noise lookup). Stay
    within the device budget in §2/§8.
- **2.4 `[EDITOR]`** Create an empty GameObject in the scene (e.g. `WaterController`),
  add `WaterController.cs`, and **wire references in the Inspector**: the `Ocean.mat`
  material, the grid container `RectTransform`, the Main Camera. Hook it to the load
  script's grid-loaded event (in code in 2.2 if the event is C#, or via Inspector if it's
  a UnityEvent).
- **2.5 `[TEST]`** Load a puzzle. **Expected:** white foam hugs the outer edge of the
  letter-cell landmass and laps into the black-cell bays between words; foam lines up with
  the actual cells; water elsewhere is calm and foam-free. Tune `_FoamWidth`,
  `_FoamSoftness`, `_FoamSpeed`, and the SDF resolution until the coastline reads calm and
  clean.
- **2.6 `[CODE]`** Wire `EnableFoam`: call `EnableFoam(true)` when the puzzle panel becomes
  active and `EnableFoam(false)` when it's hidden (hook the same CanvasGroup/panel
  show-hide logic the menus use). Confirm menus show plain calm water with no foam.
- **2.7 `[INFO]`** Commit. **Phases 0–2 are a complete, shippable look.**

---

## PHASE 3 — Land surface (deferred look) `[DECISION]`

Goal: decide how the white cells read as *land* under the letters, without hurting
readability. Deferred by choice; the current cell look is the safe fallback.

- **3.1 `[DECISION]`** Audition: sandy beach tone, light stone/neutral, or keep the
  current cell look with just the foam edge. Judge against the deep-blue water and, above
  all, **text legibility**.
- **3.2 `[EDITOR]` or `[CODE]`** Apply the chosen treatment to the white-cell tile
  (tint/material/sprite). Keep strong contrast between land and letter text.
- **3.3 `[TEST]`** Readability pass on a real device at a few brightness levels. If it
  hurts legibility at all, revert to the current cell look — legibility wins.

---

## PHASE 4 — Reactivity (separated, optional final layer)

Goal: tap ripples and a word-completion splash, built on top of frozen Phases 1–3.
**Audition each; keep only what fits the calm.** Anything that reads as noise on a calm
sea gets cut.

- **4.1 `[CODE]`** Add a small fixed-size ripple buffer to `WaterController.cs`
  (e.g. up to 6 ripples: screen-UV position + start time). Feed it to the material as
  uniform arrays each frame; expire old ripples.
- **4.2 `[CODE]`** Extend the shader: for each active ripple, add a radial sinusoidal
  normal perturbation that decays with time and distance from the ripple center. Keep the
  loop tiny and bounded. `half` precision.
- **4.3 `[CODE]`** Hook tap/touch input → convert touch position to screen UV → spawn a
  ripple. Keep amplitude low (calm).
- **4.4 `[TEST]`** Evaluate on device. If tap ripples read as noise against the calm sea,
  reduce amplitude/decay or **cut** them.
- **4.5 `[CODE]`** Completion splash: on the existing **word-completed** event, spawn a
  short, low foam burst / cluster of ripples along the completed word's cells.
- **4.6 `[TEST]`** Audition. Keep only if it enhances the calm mood; otherwise cut.

---

## 7. Risk register & fallbacks

- **R1 — Full Screen Pass draws over the UI instead of behind it (checked at 1.6).**
  Fallback: render water as a **world-space quad** placed behind the canvas plane. In
  Screen Space – Camera, UI in the same camera draws over scene geometry, so a quad
  farther from the camera than the canvas's plane distance is guaranteed to sit behind the
  UI. Cost: the `_GridRectUV` mapping in 2.2 becomes a screen→quad-UV mapping instead of a
  direct screen-UV mapping (slightly more math, computed once). Everything else is
  unchanged.
- **R2 — URP menu paths differ slightly from this plan.** URP 17.4.0 is Unity 6 and stable,
  but if a menu label differs, verify against the docs for the recorded editor version
  (0.1) rather than guessing. The design does not change.
- **R3 — Foam misaligned with cells.** Almost always a `_GridRectUV` / screen-size or
  canvas-scale issue. Recompute the container rect with `RectTransformUtility` against the
  actual screen size; verify on multiple aspect ratios.
- **R4 — Frame cost on older phones.** See §8.

## 8. Performance notes (wide device target)

- Keep the fragment shader to ~**2 normal samples + 1 SDF sample (+ at most 1 cheap
  noise)**. Use `half` precision throughout.
- If older devices struggle, render the water pass at **half resolution** and upscale
  (the foam edge tolerates it well because distance fields interpolate cleanly). Treat
  this as a switch to flip only if profiling shows a problem — not a default.
- The SDF texture is tiny and rewritten in place; never allocate textures per load.

## 9. Tunable parameters (quick reference)

Water: `_DeepColor`, `_ShallowColor`, `_NormalScaleA/B`, `_ScrollSpeedA/B`, `_SpecColor`,
`_SpecPower`, `_SwellStrength`.
Foam: `_FoamColor`, `_FoamWidth`, `_FoamSoftness`, `_FoamSpeed`, SDF resolution & padding.
Reactivity: ripple amplitude, decay, count; splash size.

## 10. Definition of done per phase

- **Phase 0:** URP + Universal Renderer active; UI intact; no pink materials.
- **Phase 1:** calm deep-blue water fills the screen, behind the UI, moving gently.
- **Phase 2:** foam coastline hugs the letter-cell landmass and bays, aligned to cells,
  foam only during puzzles, plain water on menus. **(Shippable milestone.)**
- **Phase 3:** land surface decided and applied without hurting legibility (or reverted).
- **Phase 4:** ripples/splash either tuned and kept, or deliberately cut. Calm preserved.
