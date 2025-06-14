﻿# R.E.P.O. Mutators – Developer Guide

If you're a mod developer and want to add your own mutators, this guide shows you how to integrate with the Mutators system using the public `MutatorManager` API.

## 🔗 Overview

The `MutatorManager` class acts as the central registry for all mutators. You can use it to:

* Register custom mutators
* Unregister mutators
* Access metadata and current mutator state

---

## 📦 Dependencies

To get started, include the `Mutators.dll` from the mod download in your own mod project as a reference. A NuGet package may be provided in the future.

---

## 🛠 Registering Your Own Mutator

To define and register a mutator, you’ll need to implement the `IMutator` interface or use the existing `Mutator` class provided in the API.

Here’s a basic example of how to define and register a new mutator:

```csharp
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Settings;
using YourMod.Patches; // Your custom Harmony patch
using UnityEngine;

[BepInDependency(Mutators.MyPluginInfo.PLUGIN_GUID, Mutators.MyPluginInfo.PLUGIN_VERSION, BepInDependency.DependencyFlags.SoftDependency)]
public class MyMod : BaseUnityPlugin
{
    private void Start()
    {
        IMutator myMutator = new Mutator(
            new GenericMutatorSettings(                             // Settings, including weight for random selection
                "Explosive Ducks",                                  // Name and unique identifier of this mutator
                "Duck go boom",                                     // Mutator description
                config
            ),
            typeof(ExplosiveDucksPatch),                            // Patch type implementing the mutator logic
        );

        MutatorManager.Instance.RegisterMutator(myMutator);
    }
}
```

This will make your mutator eligible to be selected by the weighted random system during a game.

---

## ✅ Optional Conditions

You can control whether a mutator can be selected using conditions. Each one must return `true` during the picking process for the mutator to be eligible.

```csharp
var myConditionalMutator = new Mutator(
    new GenericMutatorSettings(
                "Explosive Ducks",
                "Duck go boom",
                config
    ),
    typeof(ExplosiveDucksPatch),
    [
        SemiFunc.IsMultiplayer,                   // Example: only allow in multiplayer
        () => SomePlayerCount > 3                 // Custom condition
    ]
);

MutatorManager.Instance.RegisterMutator(myConditionalMutator);
```

---

## 🚫 Unregistering a Mutator

If needed (but realistically never), you can cleanly remove your mutator:

```csharp
MutatorManager.Instance.UnregisterMutator("Explosive Ducks");
```

---

## 📌 Accessing the Active Mutator

You can query the active mutator like so if needed:

```csharp
IMutator active = MutatorManager.Instance.CurrentMutator;
```

---

## 🎛 `SetActiveMutator(string name, bool applyPatchNow = true)`

The `SetActiveMutator` method is internal API and the central mechanism for **applying a mutator during gameplay**. It ensures that all previously active mutators are cleanly deactivated before enabling the one you've specified by name.
You should never have to deal with this method yourself, but understanding what it does may be fruitful.

### 🔍 Purpose

* Deactivates any currently active mutators
* Activates a new mutator by name
* Optionally applies the mutator's effects immediately via its patch

### 🧪 Signature

```csharp
internal void SetActiveMutator(string name, bool applyPatchNow = true)
```

### ⚙️ Parameters

| Parameter       | Type     | Description                                                                                                                                        |
| --------------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| `name`          | `string` | The internal name of the mutator to activate. Must be previously registered.                                                                       |
| `applyPatchNow` | `bool`   | If `true`, the mutator's `Patch()` method will be called immediately. If `false`, only the `CurrentMutator` will be set without applying behavior. |

---

### 📝 Example Usage

```csharp
MutatorManager.Instance.SetActiveMutator("Explosive Ducks");

// OR, if you want to delay patching manually:
// By default this is executed on RunManager.ChangeLevel (host) when RunIsShop, which will then communicate it to all clients
MutatorManager.Instance.SetActiveMutator("Explosive Ducks", applyPatchNow: false);

// By default this is executed on RunManager.UpdateLevel (client) or RunManager.ChangeLevel (host) when RunIsLevel
MutatorManager.Instance.CurrentMutator.Patch(); // apply later
```

---

### 🧼 What it does under the hood

1. **Unpatches** any previously active mutators.
2. **Looks up** the given name in the internal mutator registry.
3. If found:

   * Updates `CurrentMutator`.
   * Logs debug info.
   * Applies the patch if `applyPatchNow` is `true`.
4. If not found:

   * Logs a warning about unknown mutator activation.
   * Suggests that the client may be desynced if not the host.

---

This method is the **entry point for activating gameplay-altering behavior**.

---

## 🔁 Mutator Lifecycle Hooks

When you register a mutator with patches, the `Mutator` class automatically looks for special lifecycle hooks in each patch class. These let you run custom code **before/after patching** or **before/after unpatching**.

### 🪝 Available Hook Methods

If a patch class defines any of the following **static** methods, they will be invoked at the appropriate stage:

| Method Name          | Called When                   |
| -------------------- | ----------------------------- |
| `BeforePatchAll()`   | Before any patch is applied   |
| `AfterPatchAll()`    | After all patches are applied |
| `BeforeUnpatchAll()` | Before any patch is removed   |
| `AfterUnpatchAll()`  | After all patches are removed |

These methods are optional — the `Mutator` system will only call them if they exist.

---

### 🧪 Example Patch Class With Hooks

```csharp
public static class ExplosiveEnemiesPatch
{
    public static void BeforePatchAll()
    {
        Logger.LogInfo("ExplosiveEnemies: Preparing to patch...");
    }

    public static void AfterPatchAll()
    {
        Logger.LogInfo("ExplosiveEnemies: Patch applied.");
    }

    public static void BeforeUnpatchAll()
    {
        Logger.LogInfo("ExplosiveEnemies: Cleaning up...");
    }

    public static void AfterUnpatchAll()
    {
        Logger.LogInfo("ExplosiveEnemies: Unpatched successfully.");
    }

    [HarmonyPatch(typeof(Enemy), nameof(Enemy.Die))]
    [HarmonyPostfix]
    public static void ExplodeOnDeath(Enemy __instance)
    {
        // Custom explosion logic
    }
}
```

---

### 🔄 How It Works

When you construct a new `Mutator`, its constructor automatically scans the list of patch `Type`s and registers any of the above lifecycle methods it finds:

```csharp
TryAddHook(patchType, "BeforePatchAll", _beforePatchAllHooks);
TryAddHook(patchType, "AfterPatchAll", _afterPatchAllHooks);
...
```

You don’t need to register these manually — just name the static methods correctly in your patch class, and the system will invoke them at the correct time.

---

## 🔄 Versioning Considerations

* Breaking changes can occur on **minor** version bumps before `1.0.0`.
* Changes to mutators should **always** occur on **minor** version bumps, unless these have no chance of causing desync between versions

---

## 📚 References

* **MutatorManager.Instance** – Singleton for all registration and control
* **MutatorsNetworkManager.Instance** – Singleton for communication between master and clients
* **IMutator** – Interface for custom mutators
* **Mutator** – Default implementation used by the base mod
* **Patch Type** – Harmony patch class implementing behaviour

---

## Contribution

If you want your mutator included in this mod or want to improve the API itself, feel free to submit a pull request or open a discussion on GitHub or Discord!