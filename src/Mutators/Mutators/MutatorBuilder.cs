using System;
using System.Collections.Generic;
using Mutators.Enums;
using Mutators.Settings;

namespace Mutators.Mutators
{
    /// <summary>
    /// Builder for <see cref="Mutator"/>s.
    /// </summary>
    /// <remarks>
    /// In addition to lifecycle hooks automatically discovered on patch types, the builder also allows for
    /// lifecycle hooks to be registered as delegates.
    /// </remarks>
    public sealed class MutatorBuilder
    {
        private readonly List<Type> _patches = [];
        private readonly List<Func<bool>> _conditions = [];
        private readonly List<Action> _beforePatchAllHooks = [];
        private readonly List<Action> _afterPatchAllHooks = [];
        private readonly List<Action> _beforeUnpatchAllHooks = [];
        private readonly List<Action> _afterUnpatchAllHooks = [];
        private readonly List<Action<IDictionary<string, object>>> _onMetadataChangedHooks = [];

        internal AbstractMutatorSettings Settings { get; }
        internal MutatorDifficulty Difficulty { get; }
        internal bool SpecialActionOverlay { get; private set; }
        internal IReadOnlyList<Type> Patches => _patches;
        internal IReadOnlyList<Func<bool>> Conditions => _conditions;
        internal IReadOnlyList<Action> BeforePatchAllHooks => _beforePatchAllHooks;
        internal IReadOnlyList<Action> AfterPatchAllHooks => _afterPatchAllHooks;
        internal IReadOnlyList<Action> BeforeUnpatchAllHooks => _beforeUnpatchAllHooks;
        internal IReadOnlyList<Action> AfterUnpatchAllHooks => _afterUnpatchAllHooks;
        internal IReadOnlyList<Action<IDictionary<string, object>>> OnMetadataChangedHooks => _onMetadataChangedHooks;

        /// <summary>
        /// Creates a builder for a mutator with the given settings and difficulty.
        /// </summary>
        /// <param name="settings">The settings that define the mutator's identity, description, weight, and level eligibility.</param>
        /// <param name="difficulty">The difficulty of the mutator.</param>
        public MutatorBuilder(AbstractMutatorSettings settings, MutatorDifficulty difficulty)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Difficulty = difficulty;
        }

        /// <summary>
        /// Adds a Harmony patch type that is applied while the mutator is active.
        /// Lifecycle hooks declared on the patch type are discovered as usual.
        /// </summary>
        /// <param name="patch">The Harmony patch type.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder WithPatch(Type patch)
        {
            _patches.Add(patch ?? throw new ArgumentNullException(nameof(patch)));
            return this;
        }

        /// <summary>
        /// Adds Harmony patch types that are applied while the mutator is active.
        /// Lifecycle hooks declared on the patch types are discovered as usual.
        /// </summary>
        /// <param name="patches">The Harmony patch types.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder WithPatches(IEnumerable<Type> patches)
        {
            foreach (Type patch in patches ?? throw new ArgumentNullException(nameof(patches)))
            { 
                WithPatch(patch);
            }
            return this;
        }

        /// <summary>
        /// Adds a condition that must pass before the mutator is eligible for selection.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder WithCondition(Func<bool> condition)
        {
            _conditions.Add(condition ?? throw new ArgumentNullException(nameof(condition)));
            return this;
        }

        /// <summary>
        /// Adds conditions that must all pass before the mutator is eligible for selection.
        /// </summary>
        /// <param name="conditions">The conditions.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder WithConditions(IEnumerable<Func<bool>> conditions)
        {
            foreach (Func<bool> condition in conditions ?? throw new ArgumentNullException(nameof(conditions)))
            {
                WithCondition(condition);
            }
            return this;
        }

        /// <summary>
        /// Sets whether the mutator uses the built-in special-action overlay.
        /// </summary>
        /// <param name="specialActionOverlay">Whether the mutator uses the built-in special-action overlay.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder WithSpecialActionOverlay(bool specialActionOverlay = true)
        {
            SpecialActionOverlay = specialActionOverlay;
            return this;
        }

        /// <summary>
        /// Registers a hook that runs before the mutator's patches are applied.
        /// </summary>
        /// <param name="hook">The hook.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder BeforePatchAll(Action hook)
        {
            _beforePatchAllHooks.Add(hook ?? throw new ArgumentNullException(nameof(hook)));
            return this;
        }

        /// <summary>
        /// Registers a hook that runs after the mutator's patches are applied.
        /// </summary>
        /// <param name="hook">The hook.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder AfterPatchAll(Action hook)
        {
            _afterPatchAllHooks.Add(hook ?? throw new ArgumentNullException(nameof(hook)));
            return this;
        }

        /// <summary>
        /// Registers a hook that runs before the mutator's patches are removed.
        /// </summary>
        /// <param name="hook">The hook.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder BeforeUnpatchAll(Action hook)
        {
            _beforeUnpatchAllHooks.Add(hook ?? throw new ArgumentNullException(nameof(hook)));
            return this;
        }

        /// <summary>
        /// Registers a hook that runs after the mutator's patches are removed.
        /// </summary>
        /// <param name="hook">The hook.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder AfterUnpatchAll(Action hook)
        {
            _afterUnpatchAllHooks.Add(hook ?? throw new ArgumentNullException(nameof(hook)));
            return this;
        }

        /// <summary>
        /// Registers a hook that receives the mutator's merged metadata whenever it changes.
        /// </summary>
        /// <param name="hook">The hook.</param>
        /// <returns>This builder.</returns>
        public MutatorBuilder OnMetadataChanged(Action<IDictionary<string, object>> hook)
        {
            _onMetadataChangedHooks.Add(hook ?? throw new ArgumentNullException(nameof(hook)));
            return this;
        }

        /// <summary>
        /// Builds a new <see cref="Mutator"/> from the current configuration.
        /// </summary>
        /// <returns>The new mutator.</returns>
        public Mutator Build()
        {
            return new Mutator(this);
        }
    }
}
