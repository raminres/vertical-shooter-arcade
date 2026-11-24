# Vertical Shooter Arcade

A 3D vertical-scrolling arcade shooter in development with Unity. The project is a personal technical-art portfolio piece, combining WWII-inspired aircraft and environments with a colorful mobile presentation.

The game targets portrait play on Windows and iOS. It uses a top-down perspective camera with movement on a two-dimensional playfield.

## Project status

The project is in the **pre-prototype** stage. The repository currently contains a sample scene and the project setup for input, localization, asset imports, testing, and iOS builds. Gameplay systems and levels are planned.

The design aims for a ten-level campaign with two boss encounters, automatic primary fire, selectable special weapons, enemy formations, and persistent upgrades. See the [game design document](GDD.md) for the current design and open questions.

## Getting started

1. Install **Unity 6000.6.10f1** through Unity Hub. Add Windows and iOS Build Support if you plan to build for those platforms.
2. Clone this repository and add its root folder to Unity Hub.
3. Open the project with the specified Editor version and allow Unity to resolve its packages. Git and network access are needed to resolve the RichHaptic Git dependency.
4. Open `Assets/Scenes/SampleScene.unity` to inspect the current scene.

## Project setup

- **Rendering:** Universal Render Pipeline.
- **Input and UI:** Unity Input System and UI Toolkit.
- **Localization:** English and Turkish locales, with UI string and asset table collections ready to populate.
- **Tweening and haptics:** DOTween and RichHaptic are installed. DOTween's Editor utility is disabled and its optional modules are not configured. Its runtime manager creates a GameObject, which needs to be reconciled with the project's prefab-only rules before gameplay use.
- **Asset imports:** Preset Manager has folder filters for world textures, UI icons, models, sound effects, and music.

## iOS builds

With iOS selected as the active build target, open **Tools → Build → iOS Build**. The window previews the next output folder, app version, and iOS build number. Starting a build creates a numbered folder such as `Builds/_build0` and saves the incremented patch version and build number to Player Settings. A failed build attempt also consumes those numbers.

Unity exports an Xcode project. Building and signing the final iOS app requires Xcode on macOS. The custom build tool has yet to be verified with an export, and haptic feedback has yet to be tested on an iOS device.

## Tests

Open **Window → General → Test Runner** in Unity and run the Edit Mode and Play Mode suites. With the [Unity CLI](https://docs.unity.com/en-us/unity-cli) installed and the project closed in the Editor, the suites can also be run from the project root:

```sh
unity test . --mode EditMode --output ./TestResults/editmode.xml
unity test . --mode PlayMode --output ./TestResults/playmode.xml
```

Project conventions and the latest development handoff are in [AGENTS.md](AGENTS.md).
