This project is a port of [UnitySpy](https://github.com/hackf5/unityspy) to the .NET 4.5 framework version and adds a Hearthstone-specific layer.

https://medium.com/@hackf5/hacking-into-unity-games-ca99f87954c
https://discord.gg/myk6Zn8rnY

# Supporting new Unity versions

UnitySpy supports a few versions of Unity out of the box (2018.4, 2019.4, 2021.3). If you want to use on a version that is not supported, you will probably need to update the lib.
Most likely, you will only need to update the MonoLibraryOffsets class.

## What are offsets? Why do we need to update them?

See https://medium.com/@hackf5/hacking-into-unity-games-ca99f87954c for a better explanation.

In short, in order to parse the Mono structure, UnitySpy reads the info it needs by accessing the memory by its direct address in memory. Or, more specifically, its address relative to the start of the current structure. The offsets are these relative addresses.
And when the
