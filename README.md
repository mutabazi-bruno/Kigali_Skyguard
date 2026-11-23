# Kigali Skyguard

A VR wave shooter for Meta Quest 2. It's 2040, a program called HIVE has taken over the delivery drones that run Kigali, and you're standing at the edge of a rooftop above the city with a blaster you reload by hand. Hold the three relay towers until dawn.

Built in Unity 6 LTS (6000.0.80f1) with URP, OpenXR and the XR Interaction Toolkit 3.

<!-- links: gameplay video, GDD, APK download -->

## What you do

You stand near the front edge of a rooftop with the relay tower right beside you on the left and the city below. Drones come in ten waves:

| Drone | What it does | How to deal with it |
| --- | --- | --- |
| Scout | Dives straight at the tower, 1 hit | Shoot it before it gets there |
| Gunner | Hovers 8 to 15 m away, charges for 1.5 s and fires a slow laser at your head | Shoot it, block the laser with your hand shield, or lean out of the way |
| Shielded | Keeps a hex shield turned towards you and turns slowly | Hit it from the side, or break the shield (4 hits) first |
| Carrier | Wave 10. Circles the tower and drops three Scouts every 8 s | Shoot the four glowing weak points underneath |

Each drone type has its own coloured glow and light trail (orange Scouts, red Gunners, cyan Shielded, pink Carrier), so you can tell them apart from far away.

The blaster holds 12 shots. Press A or X to drop the empty magazine, take a new one from the pouch on your belt and push it into the grip until it clicks. The light strip on the magazine drains and turns red when you're low.

Your other hand carries a hex energy shield. Hold it up in front of a Gunner's laser to stop it (50 points per block). Press Y, or B if you play left-handed, to switch the shield on or off, or change it in Settings. Lasers that get past it take one of your three shield points, which come back after every wave.

After every wave you pick one of three upgrades by shooting its card: extended magazine (18 shots), quick load, piercing shot, tower repair, slow lasers or overdrive (double points next wave). Waves 1 to 4 are on Tower 1 in the city centre, 5 to 7 on Tower 2 on a hill and 8 to 10 on the main relay. Between towers the screen fades to black for a second. There's no artificial movement at all.

Ishema, the tower's assistant, talks you through the night, with subtitles in English, French or Kinyarwanda. Clear wave 10 and the sun comes up. Your score goes on a local top 10 with arcade-style initials, and Endless mode unlocks.

## Running it

### On a Quest 2, 3 or 3S

1. Turn on developer mode for the headset (Meta Horizon app on your phone).
2. Plug the headset in and install the APK with SideQuest, or with adb:
   ```
   adb install -r KigaliSkyguard.apk
   ```
3. Open it from Library > Unknown Sources.

Play standing in a 2 m x 2 m space. The first launch plays the intro (hold the trigger to skip) and asks which hand holds the blaster, whether you're standing or seated, and whether you want snap turn.

### In the Unity editor

1. Install Unity 6000.0.80f1 with Android Build Support (OpenJDK, SDK and NDK).
2. Clone the repo. The models, audio and video are in Git LFS, so run `git lfs install` before cloning or `git lfs pull` after.
3. Open the project and load `Assets/_Skyguard/Scenes/Intro.unity`.
4. Either play through Quest Link, or drag `Assets/Samples/XR Interaction Toolkit/3.1.3/XR Device Simulator/XR Device Simulator.prefab` into the scene to test with mouse and keyboard.

### Building the APK

File > Build Profiles > Android, check that `Intro` and `Skyguard` are both in the scene list, then Build. Everything else (IL2CPP, ARM64, Vulkan, ASTC, OpenXR with the Meta Quest feature group) is already set in the project.

## Controls

| Input | Action |
| --- | --- |
| Grip | Grab the blaster from the holster, grab a magazine from the pouch. Your glove's fingers close with it |
| Trigger | Fire. In menus, click |
| A / X (on the hand holding the blaster) | Drop the magazine |
| Y / B (on the other hand) | Hand shield on or off |
| Menu button (left controller) | Pause |
| Turn your off-hand wrist up | Wrist display: shield, score, wave, pause button |
| Right thumbstick | Snap turn 45 degrees (only if turned on in settings) |

Your body is part of the controls: raise the shield, lean, duck or step sideways to deal with lasers.

## How the code is put together

The code follows the four layers from the GDD:

- Presentation (`Presentation/`, `UI/`): audio and music, effects, haptics, Ishema's voice, the gloves' finger poses, menus. It only listens to events and never changes game rules.
- Flow (`Flow/`): `GameManager` and the state machine (main menu, tutorial, wave, upgrade, paused, results).
- Gameplay (`Weapons/`, `Drones/`, `Waves/`, `Gameplay/`, `Upgrades/`): the blaster, drones, waves, tower, hand shield and scoring. It publishes events and never calls the presentation layer.
- Data and platform (`Data/`, `Platform/`): save file, leaderboard, rooftop rig and fades, comfort settings, Quest display settings.

Gameplay talks upwards through a typed `EventBus`. When a bolt destroys a drone, the drone publishes `DroneDestroyed`, and the score, explosion, sound and controller buzz all react without the drone knowing about any of them. A blocked laser works the same way: `ShieldBlock` publishes `LaserBlocked`, and the score, spark, sound and haptics pick it up.

| Pattern | Where |
| --- | --- |
| State | `GameStateMachine` with six `IGameState` classes. Pause is an overlay, so resuming doesn't restart the wave |
| Observer | `EventBus` with struct events (`Core/GameEvents.cs`) |
| Strategy | `IFireMode` (single, piercing) and `IUpgrade` (six upgrades) |
| Factory | `DroneFactory` creates drones by `DroneType`, so the wave manager and the carrier never touch prefabs |
| Object pool | `ObjectPool<T>` for bolts, lasers, drones, magazines, particle effects and one-shot audio |
| Template method | `Drone` handles health, pooling, falling and rotors, and each subclass only writes `Behave()` |
| Singleton | `GameManager` only. Everything else gets its references through the inspector |

Encapsulation: the magazine's rounds can only go down through `TakeRound()`, and drone health only changes through `TakeDamage()`. Abstraction: `IDamageable`, `IFireMode`, `IUpgrade`, `IGameState`, `ISaveStorage`, `IPoolable`. Every tuning number lives in the `DroneConfig` and `WaveConfig` ScriptableObjects in `Assets/_Skyguard/Data`.

The most interesting VR piece is `MagazineWell`. It's an `XRSocketInteractor` that only accepts magazines and takes them straight off the hand once you push one far enough into the grip, so you don't have to let go of the grip button to reload.

```
Assets/_Skyguard/
  Art/        models (Blender), gloves and their finger poses, atlas textures, materials
  Audio/      sound effects, music, Ishema's lines
  Data/       DroneConfig, WaveConfig, sound bank, subtitle table
  Prefabs/    drones, weapons, effects, cards
  Scenes/     Intro, Skyguard
  Scripts/    the code, split by layer as above
  Shaders/    night sky, screen fade
  Tests/      edit mode and play mode tests
  Video/      the intro film
docs/
  diagrams/   flowchart, architecture and class diagrams (draw.io)
  performance.md
```

## Performance

The target is a steady 72 fps on Quest 2, and full campaign runs on the headset average 72 fps. What's in place: IL2CPP on ARM64, Vulkan, single pass instanced rendering, fixed foveated rendering, ASTC textures with mipmaps, one shared texture atlas for the whole city, static batching on the city and rooftops, the SRP Batcher, GPU instancing for the 900 hillside houses, LOD groups on every building, the dome and all four drones, pooling for everything spawned during a wave, no realtime shadows, no HDR or post-processing, and MSAA 4x. The details are in [`docs/performance.md`](docs/performance.md).

## Tests

Window > General > Test Runner.

- Edit mode (13 tests): event bus, state machine and pause overlay, leaderboard ranking and saving (with an in-memory `ISaveStorage`, so the real save file isn't touched), endless wave generation.
- Play mode (8 tests): these load the real scene. Clear wave 1, shoot an upgrade card and check wave 2 starts. Let the tower fall and check the run ends. Pause and resume. Check the menu opens on launch, that the hands can reach the blaster and magazines, that pushing a magazine in counts as a reload, that the shield disc stops a Gunner laser, and that the hand shield setting shows and hides it.

## Changes from the GDD

- The relay tower stands beside the player instead of in front, and the player pad sits near the roof edge. With the tower straight ahead, it hid the drones you were trying to shoot.
- The off-hand shield can block lasers, not just absorb hits, and can be switched on and off with Y or B.
- The controller models are replaced by armoured gloves whose fingers follow the grip and trigger.
- The Carrier model has four weak points, so it's 4 x 6 hits instead of 3 x 8. Same 24 hits in total.
- No Addressables. The whole build is small enough that splitting it into groups didn't buy anything.
- No baked lightmaps. The night scene is lit by emissive windows from the atlas, flat ambient light and one directional moon light with no shadows, which is cheaper on Quest than lightmap textures.

## Credits

- Blaster, drones, player shield and the armoured gloves (with their finger poses): modelled by me in Blender.
- Rooftops, relay tower, city buildings, dome, magazine, holster and pouch: made in Blender with Python scripts, all sharing one texture atlas with Imigongo patterns.
- Concept art, the city skyline band and the intro video shots: generated with Higgsfield (Seedance 2.5, Nano Banana, GPT Image). I edited the shots into the intro and mixed it with Ishema's lines and the music.
- Ishema's voice: recorded by a voice actor for this project.
- Music by Kevin MacLeod (incompetech.com), licensed under Creative Commons: By Attribution 4.0 License, http://creativecommons.org/licenses/by/4.0/
  - "Floating Cities" (menu)
  - "Exit the Premises" (waves 1 to 7)
  - "Space Fighter Loop" (waves 8 to 10)
  - "Heroic Age" (dawn)
  - "News of Sorrow" (game over)
  - "Exotic Battle" (intro)
- Sound effects: synthesised with a small numpy script.
- Font: Liberation Sans (comes with TextMeshPro).
- XR Interaction Toolkit Starter Assets by Unity.
