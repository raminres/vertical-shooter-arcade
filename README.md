# Vertical Shooter Arcade

A 3D vertical-scrolling arcade shooter in development with Unity. This personal technical-art portfolio project combines WWII-inspired aircraft and environments with a colorful mobile presentation.

The game targets portrait play on Windows and iOS, using a perspective camera and a two-dimensional playfield.

## Project status

The project has a **prototype menu and basic flight scene**. `MainMenu` contains New Game, Continue, Options, and Credits; New Game opens the `Gameplay` scene. The gameplay scene has a placeholder environment and HUD, plus a pause menu with Resume, Options, and Main Menu. Continue and the special action are disabled until their underlying systems exist. HUD values are placeholders. The gameplay scene has a forward-scrolling camera and ground, a keyboard- or touch-controlled placeholder aircraft, and a small pool of light fighters following straight and diagonal paths. The plane automatically fires straight shots, and light fighters are defeated after two hits. Enemy attacks, player damage, progression, and levels remain to be implemented.

The design aims for a ten-level campaign with two boss encounters, automatic primary fire, selectable special weapons, enemy formations, and persistent upgrades. See the [game design document](GDD.md) for the agreed design, open questions, and current implementation status.

## Getting started

1. Install **Unity 6000.6.10f1** through Unity Hub. Add Windows and iOS Build Support to build for those platforms.
2. Clone the repository and add its root folder to Unity Hub.
3. Open the project with that Editor version and allow Unity to resolve its packages. Git and network access are needed for the RichHaptic Git dependency.
4. Open `Assets/Scenes/MainMenu.unity` and enter Play Mode. `MainMenu` and `Gameplay` are the enabled scenes in the shared build scene list, in that order.

## Project setup

- **Rendering, input, and UI:** Universal Render Pipeline, the new Unity Input System, and UI Toolkit. Buttons have mouse-hover and press transitions; simulated touch uses press feedback.
- **Localization:** English and Turkish UI strings are available. English is the default, and Options can switch languages during a session. No persistent language preference is currently implemented.
- **Portrait layout:** The UI reference resolution is 1179 × 2556 (iPhone 15). Custom Device Simulator profiles are available for iPhone 15 and iPad mini 6; the iPad safe-area insets are estimates. The owner has checked the UI on a physical iPhone 15 and iPad mini 6; the new gameplay controls and framing still need device testing. Device Simulator simulates touch; use Game view to check mouse hover.
- **Tweening and haptics:** DOTween and RichHaptic are installed. DOTween's Editor utility is disabled and its optional modules are not configured. Its standard runtime manager creates a GameObject, which conflicts with the project's prefab-only rules and must be resolved before using it for gameplay. Haptics have not been verified on an iOS device.
- **Asset imports:** Preset Manager folder filters cover world textures, UI icons, models, sound effects, and music. Existing imports need an importer Reset to apply updated defaults.

## Build profiles and iOS builds

Open **File → Build Profiles** to select the **iOS** or **Windows** profile and switch to it before building. Both inherit the shared scene list and existing Player Settings. The profiles were created and the iOS configuration passed a dry run; new builds using the profiles have not yet been verified.

For numbered iOS exports, select iOS as the active build target and open **Tools → Build → iOS Build**. Starting a build creates a folder such as `Builds/_build0` and saves an incremented patch version and iOS build number to Player Settings. A failed attempt also consumes those numbers. Building directly from a Build Profile does not use this tool's automatic numbering. The `Builds/` folder is ignored by Git.

Unity exports an Xcode project; compiling and signing the final iOS app requires Xcode on macOS. An earlier iOS export succeeded, but the custom build tool itself has not yet been verified with an export.

## Tests

Open **Window → General → Test Runner** in Unity and run the Edit Mode and Play Mode suites. With the [Unity CLI](https://docs.unity.com/en-us/unity-cli) installed and the project closed in the Editor, run from the project root:

```sh
unity test . --mode EditMode --output ./TestResults/editmode.xml
unity test . --mode PlayMode --output ./TestResults/playmode.xml
```

The latest full run passed all 41 tests. Two UI animation tests have previously shown intermittent failures. Project conventions and the latest development handoff are in [AGENTS.md](AGENTS.md).
