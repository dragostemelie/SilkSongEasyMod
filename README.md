# SilkSongEasyMod

> ⚠️ This project is a fork of [HKSEasyMod]([link-to-original-repo](https://github.com/marcmeru11/HKSEasyMod)).  It adds runtime configuration options for gameplay ease.

SilkSongEasyMod is a gameplay mod for Hollow Knight: Silksong designed to reduce difficulty and improve player comfort.
It is intended for players who want a more accessible experience without completely removing the challenge.

The mod is built using BepInEx and Harmony, allowing runtime patching of game behavior without modifying original files.

## ✨ Features

### Gameplay Adjustments

- 💰 **Resource Multiplier:** Geo, Shards, and Silk pickups are automatically doubled.
- 🛡️ **Damage Capping:** Incoming damage is capped to 1 point per hit.
- ⚔️ **Increased Player Damage:** All damage dealt by the player is multiplied (default: x2).
- 🪑 **Silk Regeneration:** Fully restores Silk when resting on benches.

### Quality of Life
- 🧭 **Permanent Compass Visibility:** Compass is always displayed on the map.
- 🧲 **Auto Collection:** Rosaries and Shards are automatically attracted without requiring the magnet tool.

## 🔗 Download

Download the latest version from the [Releases](https://github.com/dragostemelie/SilkSongEasyMod/releases/tag/%23release) page.

## 📦 Installation

1. Install BepInEx for Hollow Knight: Silksong
2. Copy the compiled `SilkSongEasyMod.dll` into: `<game_folder>/BepInEx/plugins/`
4. Launch the game and use F1 ingame to configure the mod settings

This mod leverages **BepInEx** and **Harmony** for seamless integration into the game.

## 🏗️ Technical Implementation

This mod uses runtime patching via Harmony:

- **Prefix patches**
  - Modify values before original game logic executes
  - Example: resource doubling, damage limitation
- **Postfix patches**
  - Modify behavior after execution
  - Example: compass visibility, silk regeneration

Example from the code:
- PlayerData.AddGeo → doubles Geo
- PlayerData.TakeHealth → caps damage
- HealthManager.TakeDamage → multiplies outgoing damage

(See implementation in `ModClass`)

## 📚 Build

This mod depends on the following frameworks:

- BepInEx
  - Plugin framework for Unity games
  - Handles mod loading and lifecycle
- Harmony
  - Allows method patching at runtime (Prefix/Postfix)
- UnityEngine
  - Required for interacting with game objects and components

### Required Assemblies (for development)

When building the mod, ensure references to:

- BepInEx.dll
- 0Harmony.dll
- UnityEngine.dll (from game installation)
- Assembly-CSharp.dll (from game installation)


## ✅ Download

Currently downloadable here -> [EasyMod on Nexus Mods](https://www.nexusmods.com/hollowknightsilksong/mods/588)

---

## 📦 Installation

1. Install **BepInEx** for Hollow Knight: Silksong.
2. Download the compiled `.dll` file of this mod.
3. Place the `.dll` in your game’s `BepInEx/plugins` folder.
4. Launch the game normally and enjoy the improvements!

---

## ⚙️ Configuration

Currently, the mod does not support in-game configurable options.  
To change resource multipliers or damage limits, modify the source code values and recompile.

---

## ⌛ Future implementations

  
---

## 🎉 Credits
Created by **MarcMeRu**.  
Powered by **BepInEx** and **Harmony**.
