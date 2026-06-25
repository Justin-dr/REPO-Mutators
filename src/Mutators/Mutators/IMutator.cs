using System;
using System.Collections.Generic;
using System.Linq;
using Mutators.Enums;
using Mutators.Managers;
using Mutators.Settings;

namespace Mutators.Mutators
{
    /// <summary>
    /// Represents a mutator that can be registered, selected, activated, and deactivated by the <see cref="MutatorManager"/>.
    /// </summary>
    public interface IMutator
    {
        /// <summary>
        /// <para>
        /// Activates the mutator's runtime behaviour.
        /// </para>
        /// Called when transitioning into a level, if the mutator is part of <see cref="MutatorManager.CurrentMutator"/>.
        /// </summary>
        void Patch();

        /// <summary>
        /// <para>
        /// Deactivates the mutator's runtime behaviour.
        /// </para>
        /// Called when a new <see cref="MutatorManager.CurrentMutator"/> is applied, if this mutator was part of <see cref="MutatorManager.CurrentMutator"/> beforehand.
        /// </summary>
        void Unpatch();

        /// <summary>
        /// Applies supplied metadata used to configure this mutator.
        /// </summary>
        /// <param name="metadata">The metadata dictionary to consume.</param>
        void ConsumeMetadata(IDictionary<string, object> metadata);
        
        /// <summary>
        /// Unique namespaced identifier used for registration, lookup, and host-to-client communication.
        /// </summary>
        string NamespacedName { get; }

        /// <summary>
        /// Display name of the mutator.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the mutator, in its most basic form.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Perceived difficulty of the mutator.
        /// </summary>
        MutatorDifficulty Difficulty { get; }
        
        /// <summary>
        /// Source of the mutator. Indicates who added the mutator. (E.g. User, Mod)
        /// </summary>
        MutatorSource Source { get; }

        /// <summary>
        /// Whether the mutator is currently active.
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Whether the mutator uses the supplied special action HUD element.
        /// </summary>
        public bool HasSpecialAction { get; }

        /// <summary>
        /// User-configurable settings for the mutator.
        /// </summary>
        AbstractMutatorSettings Settings { get; }

        /// <summary>
        /// Harmony patch types applied while the mutator is active.
        /// </summary>
        IReadOnlyList<Type> Patches { get; }

        /// <summary>
        /// Conditions that must pass before the mutator is eligible for selection.
        /// <remarks>
        /// These apply to all types of mutators. (E.g. multi-mutators)
        /// </remarks>
        /// </summary>
        IReadOnlyList<Func<bool>> Conditions { get; }
        
        /// <summary>
        /// Whether the mutator is eligible for selection. By default, this calls the mutator's <see cref="AbstractMutatorSettings.IsEligibleForSelection"/> method and all of its <see cref="Conditions"/>.
        /// </summary>
        /// <returns>
        /// True if the mutator's settings and conditions are met, false otherwise.
        /// </returns>
        bool IsEligibleForSelection()
        {
            return Settings.IsEligibleForSelection() && Conditions.All(condition => condition());
        }
    }
}
