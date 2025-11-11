
# MissingTextureMan101's Baldi Modding Dev API

An API designed for the development and cross-compatibility between multiple Baldi's Basics Plus mods.
You'll need [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2) to utilize this API, and [FixPluginTypesSerialization](https://github.com/xiaoxiao921/FixPluginTypesSerialization) for all custom components to behave correctly. Documentation is still very sparse and WIP, and may be out of date.
I suggest looking at my [content packs](https://github.com/benjaminpants/BaldiContentPacks) or other similar mods for a decent reference.

## Features
The API features a lot of useful features for modders.
* Easy to use methods for loading images/audio/models/midis
* A metadata system used to allow for easy resource gathering and storage, along with a tagging system for easy cross mod compatability
* Methods for "extending" enums (managing int to enum casts), which BB+ uses for checking items to avoid potential enum clashes.
* Builder patterns for various different prefab types
* Fixes for the BB+ midi engine to allow for use of the entire standard midi soundfont
* Various different extensions and additional systems added to BB+ to prevent incompatibilities with mods that try to do the same general thing. (EX: PlayerStatModifiers for when mods want to modify the player stats directly)
* A custom mod loading screen
* A system specifically designed for handling the modifications of the level generator, including priority and order systems to prevent conflicts and to allow for easier cross-mod compatibility.
* An extension of Harmony that adds conditional patches, patches that only patch if a certain condition is met. Useful for mod compatibility without needing to add manual conditional patching code.
* And more!

# Credits/Thanks
PixelGuy for various minor portions of the codebase (Especially during it's early days)
Dummiesman for OBJ Importer, which was adapted for use in the API.
Thanks to [this project](https://github.com/xiaoxiao921/FixPluginTypesSerialization) for allowing for serialization to work correctly for custom classes, thus letting the CustomAnimator components serialize correctly.