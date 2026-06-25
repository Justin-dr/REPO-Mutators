# R.E.P.O. Mutators – Developer Guide

If you're a mod developer and want to add your own mutators, this guide shows you how to integrate with the Mutators system using the public API.

## Installation and References
- Runtime requirements: Mutators and its dependencies must be installed.
- Compile-time requirements: 'Xepos.Mutators' NuGet package or the mod DLL.
  - Note that Unity and the R.E.P.O. Game libs are not listed as explicit dependencies. A R.E.P.O. modding project is expected to have these by default.

---

## Quick Start: Registering a mutator
To register your own mutator into Mutators, you can do the following:

```csharp
using BepInEx;
using Mutators.Enums;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Settings;
using YourMod.Patches;

[BepInDependency(Mutators.MyPluginInfo.PLUGIN_GUID, Mutators.MyPluginInfo.PLUGIN_VERSION)]
public class MyMod : BaseUnityPlugin
{
    private void Awake()
    {
        IMutator myMutator = new Mutator(
            new GenericMutatorSettings(                         // Settings, including weight for random selection
                MyPluginInfo.PLUGIN_GUID,                       // Your plugin's GUID, used to build the mutator's namespaced identifier
                "Explosive Ducks",                              // Mutator display name
                "Duck go boom",                                 // Mutator description
                Config
            ),
            typeof(ExplosiveDucksPatch),                        // Patch type implementing the mutator logic
            MutatorDifficulty.Normal                            // Perceived difficulty of the mutator
        );

        MutatorManager.Instance.RegisterMutator(myMutator);     // Use the MutatorManager singleton to register your mutator
    }
}
```
This automatically makes your mutator eligible to be selected by the weighted random system during a game.
The patches passed to the `Mutator` constructor will also be applied automatically.

It is highly recommended to register your mutator in the `Awake()` method of your mod.
Registering your mutator in the `Start()` method or later will make it ineligible to be part of any user-defined multi-mutators, as those are built in Mutator's `Start()` method.

---

## Mutator Identity
To expand on the example above, your registered mutator will automatically be given a unique identifier based on the namespace and name passed to the `GenericMutatorSettings` constructor.
It is recommended to use your own mod's namespace as this will be used as a prefix for the mutator's namespaced identifier (hereafter referred to as a mutator's `NamespacedName`)

The mutator's `NamespacedName` will be used for the following purposes:
- Registration and unregistration.
- Networking, syncing clients to the host.
- Multi-mutator and selection rules creation.
- Lookup, e.g. to determine whether a mutator is active.

### NamespacedName Slug Creation
A mutator's namespaced name will be turned into a slug with the following format: `namespace:mutator-name`.
If the input contains non-ASCII letters or digits that cannot be normalized into a readable slug, a UTF-8 hex-encoded slug will be created instead, starting with the prefix `hex-`.
As such, it is highly discouraged to use non-ASCII characters in your mutator's name.

---

## Core API
| Class                    | Description                                                  |
|--------------------------|--------------------------------------------------------------|
| `RepoMutators`           | Mutator's main plugin class.                                 |
| `MutatorManager`         | Singleton for all registration and control.                  |
| `MutatorsNetworkManager` | Singleton for communication between master and clients.      |
| `IMutator`               | Interface for mutators.                                      |
| `Mutator`                | Default `IMutator` implementation used by the base mod.      |
| `IMultiMutator`          | Interface for multi-mutators, implements `IMutator`.         |
| `MultiMutator`           | Default `IMultiMutator` implementation used by the base mod. |

### MutatorManager
The `MutatorManager` is a singleton that can be accessed through its static `Instance` property. It is available from the moment Mutator's `Awake()` first runs.
It serves as the central point for keeping track of all registered mutators. Additionally, it exposes SelectionRulesRegistries and GameState.

| Method/Property                               | Description                                                                                                           |
|-----------------------------------------------|-----------------------------------------------------------------------------------------------------------------------|
| `RegisterMutator(IMutator mutator)`           | Registers a new mutator.                                                                                              |
| `UnregisterMutator(string namespacedName)`    | Unregisters a mutator by `NamespacedName`.                                                                            |
| `RegisteredMutators`                          | Returns a read-only dictionary of all registered mutators, keyed by `NamespacedName`.                                 |
| `CurrentMutator`                              | Returns the current mutator. May or may not be active. May or may not be an `IMultiMutator`.                          |
| `HasCurrentMutator(string namespacedName)`    | Whether the `CurrentMutator` contains a mutator with the given `NamespacedName`.                                      |
| `GameState`                                   | The current Mutators game state.                                                                                      |
| `GameStateChanged`                            | Event triggered when the Mutators game state changes.                                                                 |
| `SingleMutatorSelectionRulesRegistry`         | Registry for all single-mutator selection rules.                                                                      |
| `GeneratedMultiMutatorSelectionRulesRegistry` | Registry for all generated multi-mutator selection rules. User-defined multi-mutators are not subject to these rules. |

### MutatorsNetworkManager
The `MutatorsNetworkManager` is a singleton that can be accessed through its static `Instance` property. It becomes available from the moment the lobby is joined/created.
The public API exposes methods for sending mutator metadata to clients.

| Method                                                                             | Description                                                                                            |
|------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------|
| `SendMetadata(string mutatorNamespacedName, IDictionary<string, object> metadata)` | Sends metadata for the mutator with the supplied `NamespacedName` to all clients in the current lobby. |
| `SendMetaToHost(string namespacedName, IDictionary<string, object> meta)`          | Sends metadata for the mutator with the supplied namespacedName from a client to the host.             |

The default implementation of these methods will then automatically apply the metadata to the correct mutator.

### Mutator
The `Mutator` class is the default implementation of `IMutator`.
It manages patching and unpatching of the supplied Harmony patch types, as well as metadata consumption.

| Method                                                  | Description                                                                                          |
|---------------------------------------------------------|------------------------------------------------------------------------------------------------------|
| `Patch()`                                               | Applies the supplied harmony patches and marks the mutator as active.                                |
| `Unpatch()`                                             | Unapplies the supplied harmony patches and marks the mutator as inactive.                            |
| `ConsumeMetadata(IDictionary<string, object> metadata)` | Consumes the supplied metadata. Automatically defers some metadata consumption if needed.            |

Additional notes on metadata consumption:
- Metadata is host-authoritative.
- Metadata that is not (correctly) namespaced will be ignored.
- Supplied metadata will be deep-merged with the mutator's existing metadata.
  - If an incoming metadata key has a null value, it is seen a request for removal of the key from the existing metadata.
- Metadata that is not part of the mutator's settings (`AsMetadata()`) will be deferred until the GameState reaches `LevelReady`.
- Metadata will be supplied to the mutator's patch types through the `OnMetadataChanged` lifecycle hook.
- Utility extension methods are provided for reading data from metadata, such as `Get<T>(string key)` and `GetAsList<T>(string key)`.

Lifecycle hooks are methods that are automatically called by the Mutator class when certain events occur.\
In order for them to be called, the patch type must have a static method with the appropriate signature.

| Lifecycle Hook | Description                                                     |
|----------------|-----------------------------------------------------------------|
| `OnMetadataChanged(IDictionary<string, object> metadata)` | Called when the mutator's metadata changes.                     |
| `BeforePatchAll()`                                        | Called before any of the mutator's patches are applied.         |
| `AfterPatchAll()`                                         | Called after all of the mutator's patches have been applied.    |
| `BeforeUnpatchAll()`                                      | Called before any of the mutator's patches are unapplied.       |
| `AfterUnpatchAll()`                                       | Called after all of the mutator's patches have been unapplied.  |

### Multi-Mutators
Multi-mutators are a special type of mutator that consists of multiple sub-mutators. They can either be user-defined or randomly generated.\
`IMultiMutator` implements the `IMutator` interface. This means that at any moment, the `MutatorManager.CurrentMutator` may be an `IMultiMutator`.

There are a few subtle differences between a generated multi-mutator and a user-defined multi-mutator:
- User-defined multi-mutators are registered with the `MutatorManager`, while generated ones are temporary and therefore won't be registered.
  - Generated multi-mutators will, however, still be set as the `MutatorManager.CurrentMutator`.
- User-defined multi-mutators are not subject to the generated multi-mutator selection rules.
  - Users are responsible for ensuring that their user-defined multi-mutator is valid and, to a lesser extent, fair.
- User-defined multi-mutators may override their sub-mutators' settings, if the sub-mutator and the specific setting in question support it.

For the most part, `MultiMutator` just wraps around a list of sub-mutators and delegates all calls to them.\
However, it does handle the aforementioned setting overrides for the user-defined multi-mutators:\
- When `MultiMutator.Patch()` is called, the setting overrides are applied to the sub-mutators.
- When `MultiMutator.Unpatch()` is called, the setting overrides are cleared.

An important note: a `IMultiMutator`'s sub-mutators may not be `IMultiMutators` themselves.

#### User-defined Multi-Mutator JSON Definition.
A user-defined multi-mutator is defined by a JSON file. These files are loaded from `BepInEx\config\Mutators\MultiMutators` and look like this:
```json
{
  "Name": "Demon Ducks",
  "Description": "There are many ducks, they explode!",
  "Weight": 100,
  "Mutators": {
    "xepos.repo-mutators:there-can-only-be-one": {},
    "xepos.repo-mutators:duck-this": {
      "aggro-cooldown": 1.0
    },
    "xepos.repo-mutators:out-with-a-bang": {
      "tier-1-enemy-explosion-radius": 3,
      "tier-1-enemy-explosion-damage": 200
    }
  }
}
```
The `Name`, `Weight`, and `Mutators` properties are mandatory. For the mutator section, the keys are the namespaced names of the sub-mutators to be used.\
The values are the setting overrides to be applied to the user-defined multi-mutator. Settings that are not present in the JSON file will be left untouched.

In the above example, we have a multi-mutator that consists of three sub-mutators:
- `xepos.repo-mutators:there-can-only-be-one`
- `xepos.repo-mutators:duck-this`
- `xepos.repo-mutators:out-with-a-bang`

The resulting multi-mutator will thus attempt to only spawn ducks that permanently aggro onto the player and additionally explode on death.

---

## Selection and Eligibility
Mutators are selected by the weighted random system. The initial selection happens when the host first creates the lobby.
A current side-effect of this is that mutators which require a certain amount of players (> 1) cannot be selected as the very first mutator.

Furthermore, goes through a series of filters to determine whether a mutator is eligible to be selected:
1. Determine if a Mutator needs to be selected based on the users config. If not, the NopMutator will be used and the selection process is cut short.
2. Filter out every mutator that has a weight of 0.
3. A selection strategy gets chosen based on the user's scaling type config: None, Random or Moon.

### Scaling Type: None
This scaling type is the most basic one and functions as the pre-multi-mutators selection rule:
1. All remaining mutators go through their `IsEligibleForSelection()` method, which runs both the mutator's settings' `IsEligibleForSelection()` as well as the mutator's conditions.
2. Multi-mutators that consist out of more than one sub-mutator are filtered out.
3. Single-mutator selection rules are applied, filtering out mutators that don't pass the rules.
4. A repeat-blocker is applied to the remaining mutators, preventing the same mutator from being selected multiple times in a row.

### Scaling Type: Moon
Unlike the None scaling type, the Moon scaling type does allow for multi-mutators to be selected.
The amount of mutators that can be selected is determined by the user's moon config and can be configured per moon level.
1. If it is determined that only one mutator should be selected, this follows the None scaling type's flow.
2. Else, a check is performed based on the user's moon config to determine whether a multi-mutator should be randomly generated or if a user-defined multi-mutator should be used.
3. If a random multi-mutator should be generated, Generated multi-mutator selection rules are applied.
4. Step 3 is repeated until the right amount of sub-mutators have been selected.

In the case that not enough mutators could be selected, for example due to overly strict rules, the amount of selected mutators may not match the amount of requested mutators.\
If no mutators were selected at all, the NopMutator is used.

### Scaling Type: Random
Like the Moon scaling type, the Random scaling type does allow for multi-mutators to be selected.
The amount of mutators that can be selected is determined by the user's random config.
1. If it is determined that only one mutator should be selected, this follows the None scaling type's flow.
2. Else, a check is performed based on the user's random config to determine whether a multi-mutator should be randomly generated or if a user-defined multi-mutator should be used.
3. If a random multi-mutator should be generated, Generated multi-mutator selection rules are applied.
4. Step 3 is repeated until the right amount of sub-mutators have been selected.

In the case that not enough mutators could be selected, for example due to overly strict rules, the amount of selected mutators may not match the amount of requested mutators.\
If no mutators were selected at all, the NopMutator is used.

---

## Selection Rules
Selection rules are used to determine whether a mutator is eligible to be selected. There are two types of selection rules:
- Single-mutator selection rules
- Generated multi-mutator selection rules

These are generated under `BepInEx\config\Mutators`.

Furthermore, there are two ways of registering selection rules:

### 1. Default rules
Default rules are registered on startup, these get written to either `single-mutator-rules.json` or `multi-mutator-rules.json` depending on whether they are for single-mutators or multi-mutators.
To register a new default rule, you must subscribe to the `RepoMutators.OnLoadSingleMutatorRules` and/or `RepoMutators.OnLoadMultiMutatorRules` events. These must be registered as a `JsonMutatorRule`.\
It is important to subscribe to these events in your mod's `Awake()` method, since rules and strategies will start being loaded/registered in `RepoMutators.Start()`.

```csharp
OnLoadSingleMutatorRules += ruleLoader =>
{
    ruleLoader.AddRuleStrategy(new ExclusionRuleLoadingStrategy());
    ruleLoader.AddDefaultRule(
        new JsonMutatorRule("exclude-null-signal", SingleMutatorRuleType.Exclusion, [MutatorSettings.NullSignal.NamespacedName]) 
    );
};
```

As you can see, in the example above, a new `IRuleLoadingStrategy` is being added to the rule loader. The `IRuleLoadingStrategy` is responsible for transforming the JSON rule into an actual executable rule.\
So in this case, since a new default rule with SingleMutatorRuleType `Exclusion` is being added, the system requires a strategy for this rule type.\
You can easily add your own strategy by implementing the `IRuleLoadingStrategy` interface and registering it as shown above.

The following RuleStrategies are currently provided by default by Mutators:

| Rule Strategy | Description | Availability |
|---------------| ----------- |--------------|
| `Exclusion`   | Excludes the specified mutator from being selected. | `Single`, `Multi` |
| `Mutual Exclusion` | Makes the specified mutators mutually exclusive. The cannot be selected at the same time. | `Multi` |
| `Requires Amount Of Other Mutators` | Requires the specified amount (or more) of other mutators to be selected before the specified mutator can be selected. | `Multi` |

The reason why these are referred to as "Default" rules is because they are only written to the JSON files onces. After which they do not get registered again until they are removed from the JSON rules file.
Furthermore, the user can modify or disable these rules at any time, giving them complete control over how they like to play the game.

```json
[
  {
    "Key": "handle-with-care-less-is-more",
    "Type": "Mutual Exclusion",
    "Mutators": [
      "xepos.repo-mutators:handle-with-care",
      "xepos.repo-mutators:less-is-more"
    ],
    "Enabled": true
  },
  {
    "Key": "handle-with-care-protect-the-president",
    "Type": "Mutual Exclusion",
    "Mutators": [
      "xepos.repo-mutators:handle-with-care",
      "xepos.repo-mutators:protect-the-weak"
    ],
    "Enabled": false
  }
]
```
In the above example, the enabled property is set to `false` by the user for the second rule. This means that the second rule will never run. Thus making it possible for `handle-with-care` to be selected together with `protect-the-weak`.

### 2. Non-default rules
Another way of registering selection rules is by directly registering them with either the `SingleMutatorSelectionRulesRegistry` or the `GeneratedMultiMutatorSelectionRulesRegistry`, which you can get from the `MutatorManager`.
Rules registered this way are not loaded from nor written to JSON files. The user can thus not modify them. Functionally, however, the work the same way as default rules.

```csharp
MutatorManager.Instance.SingleMutatorSelectionRulesRegistry.RegisterRule(
    "my-rules-unique-identifier",
    (namespacedName) -> namespacedName.Contains("c") 
);
```
With the rule registered like this, only mutators where the namespacedName containers the letter `c` will be able to be selected.

---

## Announcements (UI)
When mutators are selected, each player's local `MutatorAnnouncingBag` will be populated with names and descriptions of the selected mutators.
The announcement system was created to allow for cycling between the on-screen mutator name and description, and to allow ease of modification of descriptions.

You can get a specific announcement by its key through `MutatorAnnouncingBag.TryGetAnnouncement(string key, out MutatorAnnouncement announcement )`.\
Through the `MutatorAnnouncement` received, you can update the description of the announcement:
- The base description may be modified, but it is generally recommended to only modify your own descriptions.
- You may add additional `MutatorAnnouncementDescriptionSegment`s to the description.

A description segment consists of a unique key, a priority and a value.\
The value is a string that will be appended to the base description. They will be applied in the order of their priority, with the highest priority being applied first.

For optimization purposes, the resulting description is cached and will only be recalculated the next time the description is requested, and only if the description has changed.

For other built-in UI elements, there is currently no such system. Meaning they are fully shared between mutators. For this reason it is currently recommended not to rely on these.

---

## Contribution

If you want your mutator included in this mod or want to improve the API itself, feel free to submit a pull request or open a discussion on GitHub or Discord!\
This mod uses [SemVer](https://semver.org/) for versioning.
