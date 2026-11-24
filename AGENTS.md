# AGENTS.md — Unity Vertical Shooter Project

**Scope:** This is the shared, agent-neutral instruction file for any AI assistant, IDE integration, or human contributor working in this Unity project. Follow it regardless of which assistant executes a task. Do not create separate agent-specific policy files or silently override these rules.

## 1. Project baseline

- **Unity Editor:** `6000.6.10f1`. Respect `ProjectSettings/ProjectVersion.txt`; do not upgrade or downgrade Unity without explicit approval.
- **Developer tooling:** Official **Unity CLI** (`unity`) with the **Unity Pipeline package** (`com.unity.pipeline`) for Editor interaction and automation. The Pipeline package is the Editor automation integration, not a rendering-pipeline selection.
- **Rendering:** Follow the project's installed/configured render pipeline; do not change rendering packages or project graphics settings without approval.
- **Platforms:** Windows/PC and iOS. Gameplay and UI must be usable in **portrait orientation**. On PC, retain the designed portrait aspect ratio or adapt safely without stretching.
- **Input:** Unity's **new Input System** (`com.unity.inputsystem`) exclusively; no new `UnityEngine.Input` / legacy Input Manager usage.
- **UI:** **UI Toolkit** for runtime and Editor UI. Prefer UXML for structure, USS for styling, and C# for behavior.
- **Tweening and haptics:** DOTween 1.3.045 is installed under `Assets/Plugins/Demigiant/DOTween` from the vendor download. RichHaptic 0.1.0 is installed via Unity Package Manager for iOS haptic feedback. Reference `RichHaptic.Runtime` from assemblies that use it. DOTween's standard tween runner creates a runtime GameObject, which conflicts with section 4; resolve that conflict with the owner before using the runner in gameplay. DOTween's Editor utility is disabled because it automatically changes Player Settings. Its optional modules are not configured. The plugin generated `Assets/Resources/DOTweenSettings.asset` as part of its installation.
- **Player Settings:** Preserve the owner's Player Settings unless the owner explicitly requests a change. The iOS build tool intentionally increments version and build number when the owner starts a build through it.
- **Build profiles:** `Assets/Settings/Build Profiles/iOS.asset` and `Windows.asset` inherit Player Settings and the shared MainMenu/Gameplay scene list. A build from the Build Profiles window does not invoke the custom iOS numbering tool.
- **Current UI and localization:** MainMenu and Gameplay contain prototype menus, a placeholder HUD, and pause flow; Continue is disabled; New Game offers a special-weapon choice and the gameplay button consumes charges. English is the default; Turkish is selectable in Options. The UI reference is 1179 × 2556, with iPhone 15 and iPad mini 6 simulator profiles. The iPad simulator safe-area insets are estimates. The owner reports the earlier UI looks good on a physical iPhone 15 and iPad mini 6; the new gameplay controls and framing still need device checks. Device Simulator uses touch, so verify mouse hover in Game view.
- **Design scope:** A small 3D vertical-scrolling shooter with movement constrained to a 2D playfield and a perspective camera. Follow the approved GDD when it is present; do not infer that an unapproved idea is a requirement.

## 2. Mandatory session workflow

At the **start of every session**, before editing:

1. Read this `AGENTS.md`, especially **Session handoff** at the bottom.
2. Briefly review the previous session's summary and relevant recent code, scene, asset, and Git changes. Check the repository status before editing; do not overwrite unrelated work.
3. Check the Unity version, installed packages, relevant assembly definitions, and any tests related to the requested work.
4. Summarize the immediate goal and identify the smallest sensible change.

At the **end of every session**:

1. Append/update a concise entry in **Session handoff**: date, completed changes, affected files/systems, tests and outcomes, open issues, and exact next step.
2. Preserve a **short rolling handoff** (most recent five sessions); use Git history for older details. Do not turn this file into a full diary.
3. Clearly distinguish **completed**, **untested**, and **blocked** work. Never claim tests, builds, or Editor operations were run unless they actually ran.

## 3. Test-first and verification gate — mandatory

- **Every new feature, bug fix, refactor, asset/prefab change, and behavior change requires tests to pass before being called complete.** Add or update appropriate tests when behavior changes.
- Use the **Unity Test Framework** with Edit Mode tests for deterministic rules, calculations, configuration/validation, and editor utilities; Play Mode tests for runtime integration, scene/prefab behavior, input, pooling, collisions, and UI where appropriate.
- Run relevant tests while developing, then run **both available Edit Mode and Play Mode suites** after the final change. Compile with no new errors; investigate new warnings.
- Preferred project-root CLI commands (verify against installed CLI `unity --help` if syntax changes):

  ```bash
  unity test . --mode EditMode --output ./TestResults/editmode.xml
  unity test . --mode PlayMode --output ./TestResults/playmode.xml
  ```

- Check the actual CLI exit status **and** test report; do not treat a command launch or an empty result as success.
- For visual, import, or interaction changes, additionally inspect the result in the Editor or a playable build. Automated tests do not replace required visual and device checks.
- If an Editor, device, platform toolchain, or test runner is unavailable, report **not verified / blocked**, with the reason. Do not mark the change complete or fabricate passing results.
- Build Windows/PC and iOS at suitable platform or release milestones, after platform-specific changes, or when the owner requests builds; do not build both platforms for every change. Editor-only Simulator profile changes do not require player builds. Test touch input and portrait layouts on real iOS hardware when available; report clearly if only Editor simulation was checked.

## 4. Prefab-only game objects; no runtime object creation

- **Every in-game GameObject must be authored as a prefab asset in the project.** Place prefab instances into scenes or prefab-based, editor-prepared pools before entering Play Mode.
- **Do not create GameObjects or components at runtime.** No `new GameObject(...)`, `Object.Instantiate(...)`, runtime `AddComponent(...)`, or equivalent dynamic GameObject creation for gameplay.
- **Owner-approved exception:** RichHaptic creates one hidden iOS lifecycle helper GameObject at startup. This exception is limited to that package helper; gameplay objects and other runtime GameObjects must still follow the prefab-only rule.
- Enemies, bullets, pickups, impact effects, and other reusable objects must use **preallocated, prefab-authored pools**. At runtime activate/deactivate and reposition existing instances instead of creating/destroying them. Establish and document pool capacities and exhaustion behavior.
- Do not use runtime `Destroy` as the normal lifecycle for pooled objects.
- UI Toolkit `VisualElement` instances and UXML visual trees are **UI data, not gameplay GameObjects**; use documented UI Toolkit lifecycle APIs while keeping all actual gameplay GameObjects prefab-authored.
- Make sure every scene reference resolves and each prefab is reusable and appropriately configured. Test for unintended runtime creation and pool exhaustion.

## 5. Code design and maintainability

- Follow **SRP**, **KISS**, **YAGNI**, **DRY where it improves clarity**, composition over unnecessary inheritance, and clear separation of concerns.
- Prefer straightforward, readable C# and explicit dependencies over clever abstractions or hidden global state. Make the smallest change that solves the actual problem.
- Keep classes and methods cohesive; use descriptive names and consistent project conventions. Avoid unexplained constants; put tunable gameplay parameters in suitable configuration assets.
- Prefer events/callbacks when they genuinely reduce coupling; unsubscribe correctly. Avoid reflection, service locators, and framework-like architectures unless clearly justified.
- Comments should be **short and precise**, explaining *why*, constraints, or non-obvious decisions. Do not restate obvious code or add verbose generated commentary. Use XML documentation only when it helps explain public APIs.
- **Remove unused methods, fields, imports, obsolete code paths, abandoned helper classes, and dead configuration** when safe. Check references, serialization, tests, and prefab/scene bindings before deleting.
- Do not casually rename serialized fields or move assets; preserve Unity serialization references and GUIDs. When a rename is needed, use supported migration mechanisms and validate assets afterward.
- Keep performance reasonable for mobile: avoid avoidable per-frame allocations, repeated expensive scene searches, unnecessary Update methods, excessive overdraw, and uncontrolled object counts. Measure before complicated optimizations.

## 6. ScriptableObject-driven game data

- Use **ScriptableObjects** for authorable gameplay configuration: levels, waves, enemy types, movement profiles, projectile patterns, weapons, pickups, upgrades, and shared tuning parameters, as applicable.
- Treat ScriptableObjects as **design-time configuration**, not the storage location for mutable per-run/player state. Do not modify asset data to represent runtime state or saved progress.
- Keep implementation logic in testable C# classes/components; do not turn every value into a separate asset unless doing so improves editing or reuse.
- Validate authored data (missing prefab references, invalid ranges, unsupported combinations) before it reaches gameplay.

## 7. UI Toolkit and USS correctness

- UI Toolkit is the required UI system. Do not introduce uGUI/Canvas or IMGUI for in-game UI without explicit approval. IMGUI may be used for an Editor-only tool only when there is a concrete reason.
- Before using a USS property, selector, pseudo-class, transition, styling API, or UI Toolkit feature, **verify it exists and is supported in the Unity 6000.6 documentation and installed package version**. USS is **not full browser CSS**; do not assume web CSS properties are supported.
- Prefer built-in UXML/USS/UI Toolkit functionality over inventing custom solutions to a supported feature.
- Keep UXML layout, USS styling, and behavior C# separate. Reuse shared styles and UI components where appropriate.
- Test navigation and interaction with the new Input System, touch and mouse; check portrait resolution changes, safe areas, and readability on phones.
- Official reference: https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-uss/properties/reference

## 8. Assets, import presets, and folder discipline

Keep imported assets in the designated category folders. Suggested project-root layout (create the folders only when needed):

```text
Assets/
  Art/
    Models/                 # .fbx and related source imports
    Textures/               # game-world textures
    Materials/
    VFX/
  Audio/
    SFX/
    Music/
  UI/
    UXML/
    USS/
    Icons/                  # UI sprites and icons
  Prefabs/
    Player/
    Enemies/
    Projectiles/
    Pickups/
    Environment/
    Effects/
  Data/
    Levels/
    Enemies/
    Weapons/
    Upgrades/
  Scenes/
  Scripts/
    Runtime/
    Editor/
    Tests/
      EditMode/
      PlayMode/
  Settings/
    Presets/
```

- Name all new project-owned assets using these case-sensitive prefixes: **`PF_`** prefab, **`MT_`** base material, **`MI_`** material variant or intentionally derived material instance, **`TX_`** texture, **`SM_`** static mesh/model, **`SK_`** skinned mesh/model, and **`AU_`** audio clip. For an imported model containing skinned meshes use `SK_`; use `SM_` for a static-only model. Imported mesh subassets are named by their source model; do not break model imports to rename them. Unity material variants with a parent must use `MI_`.
- Apply prefixes to asset filenames while preserving `.meta` GUIDs via Unity's asset move/rename APIs. Preserve vendor-managed assets under `Assets/Plugins` and Unity tutorial assets under `Assets/TutorialInfo`, and owner-provided reference screenshots under `Assets/Screenshots` without renaming them. Run `AssetNamingTests` and the required full suites to catch violations; project-owned assets belong in the designated folders.
- Create and maintain **Unity Preset assets** for imported textures, UI icons/sprites, audio, and FBX/model assets; use separate presets for materially different categories (for example UI icons vs world textures, SFX vs music).
- Configure **Preset Manager importer filters** or a documented, tested folder-based preset application workflow so the correct preset is applied based on the intended asset folder. Merely storing `.preset` files in folders is not assumed to auto-apply them.
- Preset Manager applies the presets in `Assets/Settings/Presets` to new assets under `Assets/Art/Textures`, `Assets/UI/Icons`, `Assets/Audio/SFX`, `Assets/Audio/Music`, and `Assets/Art/Models` using folder glob filters. To apply updated defaults to an existing asset, use Reset in the importer's Inspector context menu. Reset replaces the asset's importer settings with the applicable defaults.
- After import, verify the importer actually received the expected settings. Unity's default presets do not automatically reapply merely because an existing asset was reimported; explicitly handle changes to existing assets.
- Do not mix source assets, generated assets, and runtime-ready imports arbitrarily. Keep `.meta` files and GUID references intact.
- **Avoid `Resources/`**. Prefer serialized prefab/asset references, explicit scene references, and other supported project mechanisms. Do not introduce a new asset-loading system without a demonstrated need.
- Keep imported art appropriate for target platforms and record intentional exceptions to import presets.

## 9. Assembly definitions and dependency boundaries

- Use `.asmdef` files for all project C# scripts rather than letting gameplay scripts accumulate in the default assembly.
- Separate **Runtime**, **Editor**, **EditMode Tests**, and **PlayMode Tests** assemblies. Add more modules only if they solve a real dependency or compilation problem.
- Runtime assemblies must not reference `UnityEditor` or Editor-only assemblies. Editor and test assemblies may depend on runtime assemblies, not the reverse.
- Keep `.asmdef` references explicit, minimal, and acyclic. Respect platform restrictions and test-assembly settings; include required Unity Test Framework references.
- When adding, moving, or editing scripts, verify assembly references, package references, successful compilation, and test discovery. Do not leave scripts inaccessible due to broken or missing `.asmdef` references.
- Official reference: https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/script-compilation/assembly-definition-files

## 10. Unity CLI + Unity Pipeline workflow

- Prefer the **official Unity CLI** for Editor/project inspection, test execution, build automation, and operations supported by the installed Unity Pipeline package.
- Verify the CLI and `com.unity.pipeline` connection before executing Editor-bound commands. Discover available commands from installed tooling rather than inventing command syntax.
- Use supported Unity Editor APIs, Unity CLI commands, and version-matched package documentation; do not modify generated `Library/`, `Temp/`, or package-cache contents directly.
- Treat Editor-side scripting/evaluation as capable of modifying the project. Check scene/prefab state and Git changes after automated operations; save intentional edits only.
- Never report a successful Editor operation without inspecting the result. Do not change dependency/package versions or install additional tools without permission.
- Official references:
  - https://docs.unity.com/en-us/unity-cli/reference/editor-project-commands
  - https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package

## 11. Completion checklist

Before reporting **done** on a task, confirm:

- [ ] Requested behavior is implemented without unrelated scope expansion.
- [ ] Project compiles and relevant assembly definitions resolve.
- [ ] Appropriate tests were added/updated for the change.
- [ ] Edit Mode and Play Mode tests were executed and passed (or task is reported blocked, **not complete**).
- [ ] Gameplay objects remain prefab-authored and no gameplay GameObjects are created at runtime.
- [ ] Serialized prefab, scene, and ScriptableObject references are valid.
- [ ] Relevant UI Toolkit APIs/USS properties were verified against Unity 6000.6 docs.
- [ ] Asset placement and applicable import presets were checked.
- [ ] Obsolete code and unused references introduced by the change were cleaned up.
- [ ] Portrait PC/iOS impact was checked as appropriate; unverified device/build checks are disclosed.
- [ ] Session handoff below is updated.

## 12. Session handoff (read first next session)

> Maintain a concise rolling log of the last five work sessions. Do not invent changes or test results. If no project was accessible, say so explicitly.

### 2026-11-24 — Enemy fire and player health

- **Implemented:** Light fighters fire straight shots from a 12-instance prefab-authored pool; shots are skipped when the pool is full. Swept hits remove 1 of 10 HP and grant 1 second of invulnerability. The HUD updates, and zero HP stops the run and opens a localized Retry/Main Menu panel. Enemy shot and health tuning are in `PrototypeFlightSettings`. Shields are not implemented.
- **Files/systems changed:** Combat/flight and UI scripts, gameplay rig and scene, `PF_EnemyProjectile`, `MT_EnemyProjectile`, localized UI tables and UXML, Play Mode tests, README and GDD.
- **Verification:** 21 Play Mode tests passed, covering automatic fire, hit damage and invulnerability, pool reuse, pause and retry. Inspected the shot in the iPhone Simulator and the failure panel in both iPhone 15 and iPad mini 6 Simulator profiles. Edit Mode: 34/35 passed; the existing lighting test still expects camera-only motion blur while the saved profile uses camera-and-objects.
- **Untested / incomplete:** No real-device combat feel, performance or platform builds were checked. The mandatory full test gate remains unmet due to the existing lighting test mismatch.
- **Next:** Reconcile the lighting test with the intended blur mode, rerun both suites, and playtest enemy shot readability, rate, and difficulty on device.

### 2026-11-24 — More enemies and wider camera

- **Implemented:** Expanded the prefab-authored fighter pool from 3 to 6, set groups to 6 with 0.5-second spacing, spread the opening group across three lanes, and increased the gameplay camera FOV from 55 to 60 degrees. If all slots are occupied, spawning waits for a slot; groups still wait for the previous group to clear. Group size and spawn timing are tunable in `PrototypeFlightSettings`. Existing enemy fire and health work remains intact.
- **Files/systems changed:** Flight settings and spawning, gameplay rig prefab, flight Play Mode tests, README, GDD and this handoff; prior uncommitted work was preserved.
- **Verification:** All 22 Play Mode tests passed, including at least four visible enemies on both target aspect ratios and pool reuse. Inspected iPhone 15 and iPad mini 6 Simulator views. Edit Mode: 34/35 passed; the pre-existing camera-only versus camera-and-objects motion-blur test mismatch remains.
- **Untested / incomplete:** Physical-device readability, performance and difficulty; no builds made. The full test gate remains unmet because of the existing lighting test mismatch.
- **Next:** Playtest enemy density and zoom on the owner's devices; tune group size, spawn interval and FOV as needed, then reconcile the lighting test with the intended blur mode.

### 2026-11-24 — More fighters and impact particles

- **Implemented:** Expanded the pre-authored fighter pool and group size to eight, with 0.45-second spawn spacing. Saved the owner's Gameplay scene camera placement (local y 16.83, z -3.11) without changing its FOV. Enemy deaths emit orange/yellow bursts and actual player damage emits cyan/blue bursts.
- **Files/systems changed:** Gameplay rig, scene, `PrototypeEffects`, flight/combat references, flight settings, 2 particle prefabs and material, Play Mode tests, README, GDD and this handoff. Preserved earlier uncommitted work. Four enemy effects and two player-hit effects are preallocated; when a pool is busy the visual is skipped, without affecting damage.
- **Verification:** All 24 Play Mode tests passed, including particle triggers, invulnerability, exhaustion and reuse; inspected both effects on iPhone 15 and iPad mini 6 Simulator. Edit Mode 34/35 passed; the previously recorded camera-only versus camera-and-objects motion-blur mismatch remains.
- **Untested / incomplete:** Physical-device readability and performance; no platform builds. Full verification remains incomplete because of the existing lighting test mismatch.
- **Next:** Playtest density and effects on the owner's devices; tune effects in their prefabs and reconcile the lighting test with the intended blur mode.

### 2026-11-24 — Selectable special weapons

- **Implemented:** New Game opens English/Turkish special selection: focused laser, seven-shot spread or four homing missiles. One choice is fixed per run; each run starts with two charges, the HUD consumes and displays them, and Retry keeps the choice and restores charges. Primary fire remains active. Laser and projectiles use prefab-authored objects; the 14-shot pool skips an activation without charging if insufficient slots are free. Values are provisional in `Assets/Data/Weapons/SpecialWeaponSettings.asset`.
- **Files/systems changed:** Special settings and combat scripts, flight rig and Gameplay scene, laser/projectile prefabs and material, MainMenu UXML/controller, Gameplay HUD, localization tables, Play Mode tests, README and GDD. Preserved earlier uncommitted work and the owner's camera placement.
- **Verification:** All 29 Play Mode tests passed, including all selection paths, charge UI, damage, pause, retry, primary fire, homing and object count. Inspected the selection UI in both device simulators, laser on iPad and spread on iPhone. Edit Mode 34/35 passed; the pre-existing motion-blur mode/test mismatch remains.
- **Untested / incomplete:** Special-ammo pickups and full level progression are not implemented. Physical-device touch, weapon feel, balance and performance were not checked; no platform builds. The full test gate remains unmet due to the existing lighting test mismatch.
- **Next:** Playtest all specials and touch controls on iPhone/iPad, tune damage, duration, projectile count and guidance, and reconcile the lighting test with the intended blur mode.

### 2026-11-24 — Special weapon materials and session close

- **Implemented:** Confirmed both special weapon prefabs had a supported URP Unlit material; the chosen magenta resembled Unity's missing-shader color. Changed the laser to cyan and assigned a separate bright green URP material to the spread/missile projectile prefab. Existing weapon gameplay and camera placement were retained.
- **Files/systems changed:** `MT_SpecialWeapon`, new `MT_SpecialProjectile`, `PF_SpecialProjectile`, Edit Mode material configuration tests, README, GDD and this handoff. Earlier work was preserved.
- **Verification:** Inspected the laser in iPhone and iPad Simulator and spread projectiles on iPhone. Both new material tests passed; the full Play Mode suite passed 29/29 on retry after the known intermittent UI press-animation test failed initially. Edit Mode passed 36/37; the pre-existing motion-blur mode/test mismatch remains.
- **Untested / incomplete:** Physical iOS/Windows rendering and builds were not checked. The mandatory full test gate remains unmet because of the existing lighting test mismatch.
- **Next:** Ask the owner to playtest all specials, touch controls, weapon colors and balance on iPhone/iPad; tune from feedback. Then reconcile the lighting test with the intended blur mode and rerun both test suites.
- **Session close:** Updated README and GDD with the current weapons, material colors, and verification status. No tests or builds were run for this documentation-only update.
