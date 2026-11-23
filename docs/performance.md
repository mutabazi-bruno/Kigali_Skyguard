# Performance considerations

The target is a steady 72 fps on Quest 2, the headset's refresh rate, which leaves 13.9 ms per frame for everything.

## FPS benchmark

Measured on a Quest 2 with the release APK (IL2CPP, no development build):

| Run | Waves played | fps | Where the number comes from |
| --- | --- | --- | --- |
| Campaign, first city (82 simple buildings) | 1 to 3, tower fell | 72 average | Results screen |
| Campaign, after the city rebuild (140 detailed buildings) | 1 to 5, across two towers | 72 | Wrist readout during the waves |

Both numbers come from `FrameTimer`, which counts every rendered frame. The results screen shows the average for the whole run ("average 72 fps"), and the wrist display shows the live rate when Settings > FPS on wrist is on. `FrameTimer` also writes one line per wave to the log, for example `[perf] wave 3: avg 72.0 fps, worst frame 15.2 ms`, which you can read over USB with:

```
adb logcat -s Unity | findstr "[perf]"
```

72 fps is the cap on Quest 2, so the game holds the full refresh rate through the waves we played, including after the city got about 70% more buildings.

## Techniques used

### IL2CPP and ARM64
C# is compiled to native code instead of running on Mono, and ARM64 is required for Quest builds. Vulkan is the only graphics API, which the Quest needs for foveated rendering.

### Single pass instanced rendering
Both eyes are drawn in one pass (XR Plug-in Management > OpenXR > Render Mode). Both custom shaders, the night sky and the screen fade, use the stereo instancing macros so they work with it.

### Fixed foveated rendering
The OpenXR Foveated Rendering feature is on, and `QuestDisplaySettings` sets the foveation level to 0.75 at startup, so the outer part of each lens renders at lower resolution.

### One texture atlas for the whole world
Every rooftop, building, the tower, the dome, the ground and the hillside houses use one 512 x 1024 atlas (plus a matching emission map) and one material. Windows, glass curtain walls, shop fronts, lit signs, Imigongo panels, neon trims and the dome's rainbow panels are all cells in that texture. One material for the whole city means it batches together.

### Static batching and the SRP Batcher
The 140 city buildings, rooftops, ground, dome and relay towers are marked Batching Static. URP's SRP Batcher is on for everything that moves.

### GPU instancing
The 900 small houses on the hills are not GameObjects. `CityLightsInstancer` draws them with `Graphics.RenderMeshInstanced` in batches of up to 1023, so they cost a couple of draw calls.

### LOD
Every building has two levels. LOD0 is the detailed model (floor ledges, fins, balconies, roof tanks and signs, 200 to 1,500 triangles depending on the building), and LOD1 is a plain shell with a far-facade texture. The dome has two levels. The four drones have three: the full Blender model, then 35% and 12% decimated copies, so a Gunner goes from about 16,700 triangles up close to about 1,500 far away.

### Mipmaps and ASTC
All world textures have mipmaps, which also stops the window grid from shimmering in the headset. The atlas is ASTC 4x4 and the skyline band ASTC 6x6.

### Object pooling
Bolts, lasers, drones, magazines, particle effects and one-shot audio sources all come from `ObjectPool<T>`, so nothing is instantiated or destroyed while a wave is running. Pooled drones clear their light trail when they come back out, so the pool never draws a streak across the sky.

### Cheap lighting instead of baked lightmaps
The night look comes almost entirely from the emissive windows, signs and neon in the atlas, so lightmaps had little to add and would have cost memory. There's one directional moon light, no realtime shadows, no additional lights, flat ambient light, no HDR and no post-processing. Anti-aliasing is MSAA 4x, which is cheap on the Quest's tile-based GPU.

### Smaller things
- Incremental garbage collection is on, so any collection is spread over several frames.
- Collision meshes are prebaked at build time.
- Fog hides the far edge of the world, and the camera's far plane stops at 2.2 km.
- Audio: sound effects decompress on load, Ishema's lines stay compressed in memory, and the music streams from storage.
- The gloves' finger animators use the "cull completely" mode, so they stop updating when the hands are out of view.

## Occlusion culling: not used

I left occlusion culling off on purpose. From a rooftop almost the whole city is in view at once, so there's very little hidden geometry to skip, and the occlusion data would add baking time and its own CPU cost every frame.
