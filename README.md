# VoxelSystem
Simpleified voxel engine in Unity with companion course! https://github.com/PaperPrototype/Intro-VoxelSystems-in-Unity 
 
 # Getting started
 To get started clone this repo into your machine or download it.
 In the assets folder you will find some folders each of those are a project / sample.
 Those include:
  - JobWorld, endless terrain with super fast loading using Jobs.
  - JobChunk, a single chunk that uses Jobs.
  - JobCube, a single Cube made with Jobs.
  - MeshCube, a single Cube not with Jobs.
  - RecycleChunksFollow
    - 1D, a 1D cube based proof of concept chunk recycling system
    - 2D, a 2D (x, z) cube based proof of concept chunk recycling system
  - SmallPlanet, a small planet generator with a planet gravity script and player orientation (only 4 lines of code)

EDIT: A lot has moved around and changed so the above descriptions may not be correct.

 You can upen up the scenes in each sample and hit play. Some of them have a gameObject called center, if you move the center's position around the chunks will recycle and redraw to preserve haveing the center gameobject in the center.
 
 These samples are all really straight forward and I've made them as simple as possible. So don't be afraid to just jump right in and open them up in Unity.

EDIT: Small Planet Generation!
![voxel planet](/Small_Planet.png)

# Remeshing 256 chunks, each frame, using Jobs. 
 A stress test video. You can copy what I in this video by simply changing the code to redraw the chunks each frame and not checking if the chunk needs redrawn in the chunks draw/complete functions. 
 
https://youtu.be/HvpDE3eM6v4
 
[![Watch the video](https://i.ytimg.com/vi/HvpDE3eM6v4/hqdefault.jpg)](https://youtu.be/HvpDE3eM6v4)

