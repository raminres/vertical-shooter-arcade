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
- **Current UI and localization:** MainMenu and Gameplay contain prototype menus, a placeholder HUD, and pause flow; Continue and the special action are disabled. English is the default; Turkish is selectable in Options. The UI reference is 1179 × 2556, with iPhone 15 and iPad mini 6 simulator profiles. The iPad simulator safe-area insets are estimates. The owner reports the earlier UI looks good on a physical iPhone 15 and iPad mini 6; the new gameplay controls and framing still need device checks. Device Simulator uses touch, so verify mouse hover in Game view.
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

### 2026-11-24 — Device Simulator rounded corners

- **Completed:** Added transparent rounded-corner overlays and `presentation` data to the custom iPhone 15 and iPad mini 6 simulator profiles. Their corner radii are visual estimates (165 and 43 pixels); resolution, safe areas, and device system data remain unchanged. Updated build guidance: full platform builds are for appropriate milestones or platform-specific work, not every change.
- **Files/systems changed:** Both `.device` profiles, two `TX_` overlay PNGs with metadata, Edit Mode presentation checks, and this handoff. Player Settings remained unchanged.
- **Verification:** Inspected both rounded profiles in the Simulator, verified each profile's simulated resolution and safe area in Play Mode, and confirmed the underlying screen and system data match the previous versions. All 43 Edit Mode and Play Mode tests passed; no new console errors.
- **Untested:** No iOS or Windows builds were run for this Editor-only change; corner radii remain approximate visual representations.
- **Next:** Continue gameplay work and build for the target platforms at the next appropriate milestone or when requested.

### 2026-11-24 — Enemy haptics and DOTween easing

- **Completed:** Added RichHaptic LightImpact on non-lethal enemy hits and MediumImpact on kills, and DOTween-eased plane banking, subtle camera movement zoom and enemy hit recovery. Motion settings are in `Assets/Data/PrototypeFlightSettings.asset`. The owner explicitly allowed RichHaptic's single hidden iOS lifecycle GameObject as the limited exception recorded in section 4. DOTween is used only for easing calculations; its runtime manager is not started. Prior uncommitted Simulator profile changes were retained.
- **Files/systems changed:** Runtime combat/flight code, haptic feedback adapter, runtime assembly references, flight settings, Edit/Play Mode tests, README, GDD and this handoff.
- **Verification:** All 46 Edit Mode and Play Mode tests passed, including impact preset selection, hit/kill events, animation progress and settling, and unchanged scene GameObject count. Inspected plane banking and camera FOV in the Simulator. The iOS Xcode export succeeded with the same four existing localization/TextMesh Pro warnings; Player Settings and unrelated graphics reserialization were restored. No new console errors.
- **Untested:** Haptic feel on physical iPhone, Xcode compilation/signing, and a new Windows build were not checked. Apple documents that iPad lacks Core Haptics support, so feedback is silently skipped there.
- **Next:** Test the short hit/kill taps and movement feel on iPhone; tune motion settings or haptic presets based on the owner's feedback.

### 2026-11-24 — Touch aircraft banking

- **Completed:** Changed touch banking to use swipe speed instead of per-frame distance, keeping the plane's roll consistent across frame rates. Keyboard banking and camera zoom retain their existing behavior. Added `touchBankFullSpeed` to `Assets/Data/PrototypeFlightSettings.asset` (screen widths per second for full bank).
- **Files/systems changed:** `PrototypeFlight`, `FlightSettings`, the flight settings asset, Play Mode banking regression test, and this handoff. Existing uncommitted haptic and Simulator work was retained.
- **Verification:** The new regression reproduced the weak roll before the fix and passed afterward for left/right swipes at 30 and 60 FPS. Inspected the banked plane in the iPhone Simulator. All 31 Edit Mode and 16 Play Mode tests passed in separate runs; earlier attempts encountered the existing intermittent UI animation tests and a Unity Test Runner scene-restoration error. No new errors appeared after recovery. Player Settings stayed unchanged.
- **Untested:** Real-device touch banking; no player builds were made for this change.
- **Next:** Ask the owner to try side-to-side touch movement on the iPhone/iPad and tune `touchBankFullSpeed` or `bankAngle` if needed.

### 2026-11-24 — Gameplay lighting and post-processing

- **Completed:** Added a prefab-authored warm directional sun and sky/ground ambient gradient to Gameplay, increased prototype material color contrast, and enabled a gameplay-only URP volume with bloom, low-quality camera-only motion blur, color adjustments and neutral tonemapping. Settings are in `Assets/Settings/GameplayDaylightProfile.asset`; the lighting prefab is `Assets/Prefabs/Environment/PF_GameplayLighting.prefab`.
- **Files/systems changed:** Gameplay scene and camera rig prefab, prototype materials, new lighting prefab and volume profile, Edit Mode configuration tests and assembly references, README, GDD, and this handoff. Existing uncommitted touch, haptic and simulator work was retained. Player Settings and render-pipeline project assets remained unchanged.
- **Verification:** All 51 Edit Mode and Play Mode tests passed. Inspected the iPhone and iPad Simulator previews, confirmed HDR and active volume settings, and compared the same paused frame with the volume on and off. A transient player-position NaN occurred once while opening Simulator before the lighting changes; no subsequent errors were observed.
- **Untested:** Physical-device appearance, motion-blur visibility and performance; no new platform builds were run.
- **Next:** Review the lighting and subtle effects on the owner's iPhone and iPad; tune or disable effects as needed for visual clarity and mobile performance.

### 2026-11-24 — Reference camera framing

- **Implemented:** Tilted and repositioned the gameplay camera, scaled the player and pooled enemies, adjusted the enemy hit radius and perspective-aware movement bounds. Preserved the owner's reference screenshots and exempted those reference images from runtime asset naming checks.
- **Files/systems changed:** Gameplay rig prefab, flight settings and movement, flight/combat and naming tests, this handoff. Existing uncommitted changes were retained.
- **Verification:** Inspected the iPhone 15 and iPad mini 6 Simulator views. All 17 Play Mode tests passed. Edit Mode: 34/35 passed; `GameplayLightingTests.GameplayCameraAndProfileEnableRequestedEffects` expects camera-only motion blur, while the existing profile is set to camera-and-objects. This pre-existing settings/test mismatch was left unchanged.
- **Untested / incomplete:** Physical-device framing and performance; no platform builds. The mandatory full test gate remains unmet until the lighting settings/test mismatch is resolved.
- **Next:** Decide whether the current motion-blur mode is intentional, reconcile its test, rerun both suites, and review framing on iPhone and iPad.
