# Vertical Shooter — Game Design Document

**Version:** 0.2 — design and prototype status

**Updated:** 24 November 2026

**Status:** Basic flight and combat prototype with placeholder visuals. Enemy fire, player health, retry, and the three selectable special weapons are implemented with provisional tuning; shields, progression, and numerical targets remain to be implemented and playtested.

## Current implementation status

The sections below record agreed design intent, not a list of implemented gameplay features. The current project contains:

- A `MainMenu` scene with New Game, Continue, Options, and Credits. New Game opens special-weapon selection before the `Gameplay` scene; Continue is disabled until save/progress functionality exists.
- A `Gameplay` scene with a placeholder environment and HUD for health, stars, and a special action. Health and special charges reflect the current run; stars remain placeholders. The pause menu provides Resume, Options, and Main Menu.
- English and Turkish UI strings, with English selected by default and a language selector in Options. Language selection is not yet persisted between sessions.
- UI Toolkit button hover and press transitions, portrait scaling, and safe-area handling. The reference resolution is iPhone 15 (1179 × 2556); iPhone 15 and iPad mini 6 simulator profiles are present, with estimated iPad safe-area insets. The owner checked the earlier UI on an iPhone 15 and iPad mini 6; the new gameplay controls and framing still need physical-device testing.
- iOS and Windows build profiles that use the shared scene list and Player Settings. The custom iOS Editor tool provides numbered output folders and version/build increments when invoked.
- The gameplay scene has warm daylight, sky-colored ambient fill, subtle bloom and camera motion blur, a forward-scrolling perspective camera, repeating ground, a placeholder player plane with keyboard and touch-drag movement, automatic straight fire, and eight pre-authored Light Fighters reused for simple straight and diagonal groups. A Light Fighter takes two shots; projectiles are pooled. Enemy defeats and player hits produce distinct pooled particle bursts. Hits and kills use short haptic taps on supported devices, while the plane, camera, and hit reactions use eased motion. Movement and combat values are provisional and configurable in `Assets/Data/PrototypeFlightSettings.asset`.

Special-ammo pickups, shields, full wave progression, levels, saving, and the other mechanics described below remain **planned, not implemented**. The opening enemy groups are a movement test rather than the complete Level 1 encounter.

## 1. Overview and Scope

A compact, 3D-presented, vertically scrolling arcade shooter inspired by classic aircraft shmups and the modern mobile presentation of *Sky Force Reloaded*.

- **Purpose:** Personal technical-art portfolio game, intended to demonstrate polished Unity gameplay, asset work, VFX, UI, and optimization.
- **Presentation:** Top-down **perspective** camera; gameplay movement is bounded to two screen-space axes. The aircraft visually rolls during horizontal movement and may pitch during vertical movement.
- **Visual direction:** WWII-inspired aircraft and environments, interpreted with a colorful, readable hybrid-casual mobile look. Exact palette and art treatment are **not yet defined**.
- **Target release scope:** **10 levels**, with boss encounters at **Levels 5 and 10**; **3 regular enemy models plus 2 boss models**.
- **Original personal production goal:** Approximately 1–2 months at 16–20 hours per week. Feasibility of the expanded 10-level scope has **not been validated**.
- **Not planned:** Multiplayer or in-app purchases.
- **Possible environment scope:** Up to two scrolling environments was an earlier target, **not finalized**.

## 2. Core Gameplay Loop

1. Select a level and equip **one** special weapon.
2. Move the aircraft, automatically fire its straight shot, and avoid enemy attacks.
3. Manually activate the equipped special when useful.
4. Destroy enemies and collect stars, occasional repair pickups, and rare special-ammo pickups.
5. Clear all encounter waves, then defeat the boss on Levels 5 and 10.
6. Bank earned stars on successful completion; spend stars on small, persistent aircraft upgrades between levels.

**Intended feel:** Responsive movement, satisfying destruction, clearly readable projectiles, meaningful weapon differences, steadily rising campaign difficulty, and rising/falling pressure within each level.

## 3. Player Movement and Weapons

### Movement

- Two-axis movement within the visible playfield; camera uses 3D top-down perspective rather than orthographic projection.
- Aircraft banking/roll accompanies left-right movement; slight pitch may accompany forward-back movement.
- Exact movement speed, acceleration, bounds, and touch controls are **open for prototyping**.

### Firing — confirmed

| Weapon | Role | Activation / behavior |
| --- | --- | --- |
| **Straight shot** | Standard primary fire | Fires **automatically** at all times, including during specials. |
| **Focused laser** | Concentrated damage on tough targets | Manually activated short-duration narrow laser; moves with the aircraft. |
| **Spread barrage** | Wide coverage against formations | Manually activated short fan-shaped projectile volley. |
| **Homing missiles** | Attack scattered or moving targets | Manually activated volley of guided missiles seeking on-screen enemies. |

- Choose **one** of the three specials before a level; no switching during the level.
- Activate the selected special with a button; each use consumes **one charge**.
- Start each level with **two special charges**.
- Rare special-ammo drops from enemies replenish charges.
- **Open:** Maximum charge capacity, precise weapon damage/duration/projectile count, pickup rates, exact input layout.

## 4. Player Health, Shield and Failure

- **Health:** 10 HP; **no separate lives**.
- **Standard enemy projectiles:** Deal 1 damage per hit, unless shield protection absorbs the hit.
- **Armor is a permanent shield upgrade**, not an armor pickup:

| Armor upgrade level | Shield hits absorbed at full capacity |
| --- | ---: |
| Level 1 (starting) | 0 |
| Level 2 | 1 |
| Level 3 | 2 |

- Shield absorbs incoming hits before HP; shield **does not regenerate** during gameplay.
- Starting or restarting a level restores **full health and the upgraded shield capacity**.
- After **any** shield or HP hit, the player receives **1 second of invulnerability**. Shield and hull hits have different feedback.
- Each level includes approximately **1–2 scripted enemy drops** that restore **+1 HP** (never exceeding the 10-HP maximum); these do not repair shield points.
- At **0 HP**, the level fails. Retry restarts the level from the beginning.
- Currency collected during a **failed attempt is lost**; previously banked stars are safe.
- **Open:** Contact/collision damage rules, exact damage-feedback animation/audio, final pickup locations and rates.

## 5. Upgrades and Stars

Three persistent upgrade categories, each with a starting level and two purchasable increases:

| Category | Level 1 | Level 2 | Level 3 |
| --- | ---: | ---: | ---: |
| **Main Gun Power** | Starting | 25 stars | 50 stars |
| **Special Weapon Power** (shared across all specials) | Starting | 25 stars | 50 stars |
| **Armor / Shield** | 0 shield | 1 shield | 2 shields |

Armor upgrades use the same **25/50-star** costs. **Total cost to maximize all three categories: 225 stars.** No branching skill tree.

### Star earning and replay rule — confirmed

- Selected defeated enemies physically drop stars; stronger enemies may award more.
- Completing a level also provides a star reward.
- Stars are only banked after **successful** completion; failed runs award no permanent stars.
- Each level tracks its **best star total from a successful clear**. Replaying it only awards the improvement: `max(0, current successful run total - previous successful best)`.
- This lets players recover missed stars but prevents unlimited replay farming from any single level.
- Earlier earning target: **45–55 stars per level**, **provisional**.

**Balancing conflict to revisit:** With 10 levels, 45–55 stars per level yields about **450–550 total**, versus **225** stars needed for all upgrades. Rebalance the rewards/costs **after prototyping**; do not add upgrade systems merely to consume excess currency.

## 6. Enemy System

### Model roster and roles — confirmed

| Regular model | Main combat role | Hits-to-kill with starting straight shot |
| --- | --- | ---: |
| **Light Fighter** | Popcorn, formations, occasional aimed shot | **1–2** |
| **Heavy Fighter/Bomber** | Tougher priority target and attack-pattern source | **3–4** |
| **Ground Turret** | Fixed terrain threat; aimed and spread attacks | **5** |

Two separate boss encounters are planned; their detailed mechanics and models are **not designed**.

The five enemy behavior categories are **popcorn**, **formation**, **turret**, **aimed**, and **pattern**. These are configurable roles, not five unique meshes. Movement, firing, and formation participation can be combined independently.

### Movement progression — confirmed

| Model | Levels 1–5 | Additional movement available at Levels 6–10 |
| --- | --- | --- |
| Light Fighter | Straight, diagonal | S-shaped, circular/looping |
| Heavy Fighter/Bomber | Straight, curved/arcing | Diagonal, S-shaped |
| Ground Turret | Fixed to scrolling environment | Remains fixed to scrolling environment |

Earlier movement patterns remain available after Level 5. Circular/looping refers to the enemy's **path across the screen**, not necessarily a 3D aerobatic flip.

### Projectile pattern vocabulary

- **Aimed shot:** Fired in the direction of the player's position at launch.
- **Fan/spread:** Several projectiles emitted across an angular range.
- **Radial:** Projectiles emitted around the source in multiple directions.

**Initial Level 1 assignments:** Light Fighter uses at most one aimed shot per pass; Heavy Fighter uses a simple fan attack; Ground Turret uses single aimed shots. **No radial attacks in Level 1.** Later pattern allocation, projectile speed, and cadence are **open**.

**Balancing caution:** Low hits-to-kill can make heavy enemies and turrets die before repeated attacks occur; firing timing must be tested with the actual primary firing rate.

## 7. Campaign, Waves, and Pacing

- **Ten levels**, with bosses at **Levels 5 and 10**.
- Intended overall difficulty rises progressively across the campaign.
- Within a level, pressure rises to a crescendo, drops for a brief breather, then builds again.
- **Regular levels:** Target **2–3 minutes**; **boss levels:** Target **3–4 minutes including the boss**.
- **Starting target:** **8 waves per level**, subject to runtime testing.
- Each wave contains **one or two sequential spawn groups**. The next group starts when the preceding group is resolved.
- A group is resolved when its enemies have been **destroyed or have exited** the playfield. Escaped enemies do not block progression.
- When all groups resolve, transition to the next wave after roughly **1–2 seconds**.
- After major intensity peaks, provide **3–5 seconds** of low pressure, not necessarily an empty screen.
- Level completion time varies because spawning is enemy-resolution-gated, not tied exclusively to a fixed clock.
- Exact wave counts, durations, and group timings remain **initial targets for testing**.

## 8. Level 1 — Approved Illustrative Composition

**LF = Light Fighter; HB = Heavy Fighter/Bomber; GT = Ground Turret.** Group A resolves before Group B begins. The table is an approved starting encounter layout, **not a tested timing or balance specification**.

| Wave | Group A | Group B |
| --- | --- | --- |
| 1 | 2 LF, straight from top-center | — |
| 2 | 3 LF, diagonal from left | 3 LF, diagonal from right |
| 3 | 4 LF, V formation | 3 LF, straight line |
| 4 | 2 LF, split flank | 1 HB + 2 LF escorts |
| **Breather** | **3–5 seconds of reduced pressure** | |
| 5 | 3 LF, diagonal from right | 3 LF, diagonal from left |
| 6 | 4 LF, staggered column | 4 LF, V formation |
| 7 | 1 GT + 2 LF | 3 LF, diagonal entry |
| 8 | 3 LF, split flank | 1 HB + 1 GT + 2 LF |

**Level 1 totals:** 43 Light Fighters, 2 Heavy Fighters, 2 Ground Turrets **spawned over the whole stage**. These are not simultaneous enemy counts. First and second crescendos occur around Waves 4 and 8.

**Level 2 and later:** Not yet designed or approved. The earlier Level 2 proposal of adding six light fighters was **not accepted as a GDD decision**.

## 9. Current Design Unknowns and Prototype Checks

Do **not** finalize the following from written estimates alone:

- Player movement speed, responsiveness, camera projection/readability, and mobile input behavior.
- Straight-shot rate/damage and the actual time needed to destroy each enemy.
- Enemy shot speed, attack frequency, travel time, simultaneous on-screen count, and projectile readability.
- Special damage, duration, ammo cap/drop frequency, and weapon feel.
- Actual survivability with 10 HP, shield hits, 1-second i-frames, and repair drops.
- Whether eight gated waves produce 2–3 minutes of enjoyable play.
- Star earnings and upgrade costs for the expanded ten-level campaign.
- Ground-turret placement synchronized with encounter-gated spawning while terrain scrolls continuously.
- Boss patterns, environment arrangement, final visual palette, UI styling, SFX, and detailed level layouts.

**Design status:** Basic flight, automatic fire, selectable specials, enemy shots, player health, invulnerability, and retry are in place; shields and the rest of the shooter combat loop are still planned. Exact balance values and further level-by-level design should remain provisional until the mechanics have been tested.
