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
- **Player Settings:** Preserve the owner's Player Settings unless the owner explicitly requests a change.
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

### 2026-11-24 — Localization and importer setup

- **Completed:** Created English and Turkish locales, empty UI string and asset table collections, and five folder-based importer presets with an Editor postprocessor. Verified imports for world textures, UI icons, sound effects, music and models.
- **Files/systems changed:** `Assets/Localization`, `Assets/AddressableAssetsData`, `Assets/Settings/Presets`, `Assets/Scripts/Editor`, and Edit Mode setup tests.
- **Verification:** Both test suites passed, six tests total. The string completeness check is inconclusive because no UI strings exist yet. An Addressables content build invoked through Pipeline exceeded its 60-second timeout and may still be running. Final Editor state and content build remain unverified.
- **Open:** The three supplied font files are imported but there is no runtime UI to style. Localization content build and device checks remain unverified.
- **Next:** Inspect the Editor after the timed-out Addressables build, verify its outcome, and use the fonts and localized entries when UI is created.

### 2026-11-24 — Preset Manager folder filters

- **Completed:** Registered all five import presets in Preset Manager with folder glob filters and removed the redundant asset postprocessor.
- **Files/systems changed:** `ProjectSettings/PresetManager.asset`, `Assets/Scripts/Editor`, and this handoff.
- **Verification:** Fresh sample imports received the expected settings for all five categories. Both test suites passed, six tests total.
- **Open:** Existing assets need an importer Reset to receive changed defaults. The earlier Addressables content build timed out and its outcome remains unverified.
- **Next:** Add the first requested UI or gameplay feature and populate localization tables as text is designed.

### 2026-11-24 — iOS build tool

- **Completed:** Added a UI Toolkit Editor window under Tools > Build > iOS Build. It reserves numbered build folders and increments the patch version and iOS build number when a build is started. The existing Builds ignore rule was verified.
- **Files/systems changed:** `Assets/Scripts/Editor`, Edit Mode tests, and this handoff.
- **Verification:** Unity compiled with no console errors or warnings. All 13 Edit Mode and one Play Mode tests passed. Inspected the Editor window and verified that Player Settings remained byte-for-byte unchanged.
- **Untested:** An iOS export through the new tool and iOS hardware deployment were not run. The tool intentionally updates Player Settings when invoked.
- **Next:** Verify an iOS export through the tool when the owner is ready to consume the next version and build number.

### 2026-11-24 — Tweening and haptics

- **Completed:** Installed DOTween 1.3.045 and RichHaptic 0.1.0, configured DOTween's DLL for the Editor, Windows, and iOS, and added assembly and safe Editor usage checks.
- **Files/systems changed:** `Assets/Plugins/Demigiant`, vendor-generated `Assets/Resources/DOTweenSettings.asset`, UPM manifest and lock, and Edit Mode tests. DOTween automatically added Player Settings defines and the iOS export temporarily changed preloaded assets; both were restored to the original Player Settings hash. The DOTween Editor utility was disabled to stop it adding the defines again. Build-generated graphics reserialization was also restored.
- **Verification:** All 16 Edit Mode and Play Mode tests passed after the final importer change. An iOS Xcode project export succeeded before that final change and included RichHaptic's native source and Core Haptics framework; its build report contained four localization/TextMesh Pro warnings. Final Unity console had no errors or warnings.
- **Open:** DOTween's tween runner creates a GameObject at runtime, conflicting with section 4. The DOTween Editor utility is disabled and optional modules have not been set up. Xcode compilation and physical iOS haptics remain unverified.
- **Next:** Resolve the runtime-object rule before introducing DOTween tweens, then configure any needed modules without overriding the owner's Player Settings preferences and test haptics on hardware.

### 2026-11-24 — Repository README

- **Completed:** Replaced the placeholder README with a project overview, current status, setup, iOS build tool, and test instructions.
- **Files/systems changed:** `README.md` and this handoff.
- **Verification:** Checked the README against the project files and existing handoff. No Unity tests or builds were run for this documentation-only change.
- **Open:** Existing iOS device and DOTween integration checks remain outstanding as noted above.
- **Next:** Continue with the next requested project feature.
