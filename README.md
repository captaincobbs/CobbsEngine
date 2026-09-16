Made this repo public despite it being a largely "By me, for me" project in case anyone wants to judge my work, submit pull requests, or just generally see what I'm up to.

## About
A game engine written using MonoGame 3.8 and .NET 8. The goal is to do mostly everything from scratch as a learning experience (excluding largely tedious tasks like Serialization and Rendering), using MonoGame as a base and the advice from random game engine books I find.

## Goals
The general goal is a usable game engine I can release a commercial product with. My general goals are to keep things scalable, cross platform, and well documented. This engine is largely intended for large 2D projects, and will likely not include an ECS simply because the kinds of games I want to make likely won't require them.
Technical goals:
- Robust modding system that supports loading game data, assets, and assemblies
- Basic multiplayer / state syncing
- Keep everything multithreaded / async as needed to make the game / engine reasonably performant
- Robust logging

Implementation details / general design thoughts are available in the Documentation folder, as I remember to write it.

The idea is also 100% code coverage by tests.. we'll see how that works out.

## How to Use
Clone the project, then write your code in the "Template" project. The other projects are intended to be used for backend libraries and testing. Otherwise, it should be mostly usable as a regular MonoGame project but with additional utilities available. I'll write further when the project is more developed.
