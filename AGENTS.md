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
- After changes touching platform behavior, verify a Windows/PC build and an iOS build at suitable milestones. Test touch input and portrait layouts on real iOS hardware when available; report clearly if only Editor simulation was checked.

## 4. Prefab-only game objects; no runtime object creation

- **Every in-game GameObject must be authored as a prefab asset in the project.** Place prefab instances into scenes or prefab-based, editor-prepared pools before entering Play Mode.
- **Do not create GameObjects or components at runtime.** No `new GameObject(...)`, `Object.Instantiate(...)`, runtime `AddComponent(...)`, or equivalent dynamic GameObject creation for gameplay.
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
- Apply prefixes to asset filenames while preserving `.meta` GUIDs via Unity's asset move/rename APIs. Preserve vendor-managed assets under `Assets/Plugins` and Unity tutorial assets under `Assets/TutorialInfo` without renaming them. Run `AssetNamingTests` and the required full suites to catch violations; project-owned assets belong in the designated folders.
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

### 2026-11-24 — iOS and Windows build profiles

- **Completed:** Created iOS and Windows 64-bit Player Build Profiles in `Assets/Settings/Build Profiles`; both inherit the shared scene list and Player Settings. Neither profile was activated, and Player Settings remained byte-for-byte unchanged.
- **Files/systems changed:** Build Profile assets and metadata; Edit Mode configuration checks; this handoff.
- **Verification:** Unity compiled, the Build Profiles window lists both profiles, and an iOS profile dry run resolved MainMenu and Gameplay in order. All 25 Edit Mode and Play Mode tests passed on the second run; the first run failed two existing UI animation tests, indicating intermittent behavior. Console ended without current errors or warnings.
- **Untested:** No new iOS or Windows builds or device checks were run; the Windows profile was not activated or build-validated.
- **Next:** Switch to the desired profile in File > Build Profiles and build; investigate the intermittent UI animation tests if they recur. The custom iOS build tool remains the path that increments version and build numbers.

### 2026-11-24 — Documentation and handoff refresh

- **Completed:** Updated README and GDD to distinguish the prototype scenes and menus from planned gameplay, and documented localization, device layouts, build profiles, and the custom iOS build tool. Refreshed this file's project baseline and rolling handoff.
- **Files/systems changed:** `README.md`, `GDD.md`, and `AGENTS.md` only. The project was on `feature/scene-menu-foundation` with a clean working tree before these edits.
- **Verification:** Compared the documents with the current scenes, UI, localization settings, build profiles, project configuration, Git history, and prior test results. The connected Unity Editor reported ready. No tests or builds were run for this documentation-only session.
- **Open:** Physical iPhone 15/iPad mini 6 layout and touch checks, new profile-based Windows/iOS builds, and an export through the custom iOS tool remain unverified. Two existing UI animation tests failed intermittently on the first run in the previous session, then all 25 tests passed. DOTween's runtime GameObject conflicts with the prefab-only rule.
- **Next:** Ask the owner which gameplay feature to prototype next; no specific next feature has been approved. Before implementing it, review the GDD and current branch, preserve Player Settings, and run the required tests.

### 2026-11-24 — Flight and enemy movement prototype

- **Completed:** Extended Gameplay with a forward-moving perspective camera, three looping prefab-authored ground sections, a placeholder plane using keyboard and touch-drag input, and three preallocated Light Fighters in repeating straight/diagonal groups. The pool waits for a slot when full and reuses enemies after they leave the view; no runtime gameplay GameObjects are created. Tunable values are in `Assets/Data/PrototypeFlightSettings.asset`. Updated README and GDD.
- **Files/systems changed:** Gameplay scene; player, enemy, ground and flight-rig prefabs; prototype materials; flight settings and runtime scripts; Input System assembly references; Play Mode tests. The owner reports the earlier UI looked good on iPhone 15 and iPad mini 6.
- **Verification:** Unity compiled; all 29 Edit Mode and Play Mode tests passed after final changes, covering keyboard and simulated touch, bounds on both target aspect ratios, pool reuse, and pause. Inspected gameplay in iPhone 15 and iPad mini simulator profiles. An iOS Xcode export and Windows build succeeded. The iOS report contained four existing localization/TextMesh Pro warnings; the Windows report contained six URP shadow-array warnings after switching targets. An initial Windows build was interrupted during target reload; the retry succeeded. Restored the iOS target, original Player Settings hash, and unrelated build-generated graphics changes.
- **Untested:** The new flight controls and gameplay framing have not been tested on physical devices; the Xcode project has not been compiled or signed. No shooting, collisions, damage, enemy attacks or full wave system has been implemented.
- **Next:** Have the owner try the new flight controls and camera on iPhone and iPad, tune movement via the settings asset, then agree on the next feature such as automatic fire and enemy hits.

### 2026-11-24 — Asset naming convention

- **Completed:** Renamed project-owned prefab and material assets to `PF_` and `MT_` using Unity's asset database; preserved all 13 GUIDs. Recorded `PF_`, `MT_`, `MI_`, `TX_`, `SM_`, `SK_`, and `AU_` naming rules and vendor exceptions above. Added Edit Mode checks for naming, imported models and material variants.
- **Files/systems changed:** Prefab and material asset names and metadata; `AssetNamingTests`; this handoff. Prior uncommitted flight work was retained.
- **Verification:** Unity confirmed zero GUID mismatches and no missing Gameplay dependencies; the renamed gameplay scene was inspected in Play Mode. The full 34-test Edit/Play Mode run passed on retry; the first run failed the previously intermittent button-animation test.
- **Untested:** No new platform builds or physical-device checks were run for these asset renames.
- **Next:** Continue the flight prototype after the owner tests movement and framing; enforce the naming rules for all new project-owned assets.

### 2026-11-24 — Automatic fire and light-fighter hits

- **Completed:** Added automatic straight shots, a 16-instance prefab-authored projectile pool, swept hit detection against moving light fighters, two-hit health with a brief non-lethal hit reaction, enemy reuse/health reset, and pause support. When the projectile pool is full, a shot is skipped until the next interval. Fire rate, speed, radii, damage and enemy health are tunable in `Assets/Data/PrototypeFlightSettings.asset`. Updated README and GDD.
- **Files/systems changed:** Flight and combat scripts/configuration, `PF_PrototypeFlightRig` and new `PF_PrototypeProjectile`/`MT_PrototypeProjectile` assets, Edit Mode and Play Mode tests, docs and this handoff. Existing uncommitted changes were retained.
- **Verification:** All 41 Edit Mode and Play Mode tests passed after the final changes, covering swept collisions, actual hits and defeat, enemy reuse/health reset, projectile pool exhaustion/reuse and pausing. Inspected shots in iPhone 15 and iPad mini simulator profiles. Windows build and iOS Xcode export succeeded with the previously seen six URP shadow-array and four localization/TextMesh Pro warnings, respectively. A transient NullReference during the intermediate unwired prefab state was resolved when the prefab was wired; no subsequent captured errors. Restored iOS target, original Player Settings hash and unrelated build-generated graphics changes.
- **Untested:** Physical-device combat and Xcode compilation/signing. Enemy attacks, player damage, special weapons and full waves are not implemented.
- **Next:** Have the owner try automatic fire and enemy hits on device, tune values if needed, and choose the next gameplay feature.
