using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mutators.Enums;
using Mutators.Settings;

namespace Mutators.Mutators
{
    /// <summary>
    /// Default <see cref="IMutator"/> implementation for multiple mutators, each backed by one or more Harmony patch types.
    /// <para>
    /// Serves as an orchestrator for its sub-mutators. Except providing runtime settings overrides,
    /// the multi-mutator provides no behavior of its own and simply delegates to its sub-mutators.
    /// </para>
    /// </summary>
    public class MultiMutator : IMultiMutator
    {
        private readonly IList<Func<bool>> _conditions;

        /// <summary>
        /// <inheritdoc cref="IMutator.NamespacedName"/>
        /// </summary>
        public string NamespacedName => Settings.NamespacedName;
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Name"/>
        /// </summary>
        public string Name => Settings.MutatorName;

        /// <summary>
        /// <inheritdoc cref="IMutator.Description"/>
        /// </summary>
        public string Description => Settings.MutatorDescription;

        /// <summary>
        /// <inheritdoc cref="IMutator.Difficulty"/>
        /// <remarks>
        /// For multi-mutators, this is the highest difficulty of the sub-mutators.
        /// </remarks>
        /// </summary>
        public MutatorDifficulty Difficulty { get; }

        /// <summary>
        /// <inheritdoc cref="IMutator.Source"/>
        /// </summary>
        public MutatorSource Source { get; }

        /// <summary>
        /// <inheritdoc cref="IMutator.Active"/>
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// <inheritdoc cref="IMutator.HasSpecialAction"/>
        /// <remarks>
        /// For multi-mutators, this is true if any of the sub-mutators has a special action.
        /// </remarks>
        /// </summary>
        public bool HasSpecialAction { get; }

        /// <summary>
        /// <inheritdoc cref="IMutator.Settings"/>
        /// </summary>
        public AbstractMutatorSettings Settings { get; }

        /// <summary>
        /// <inheritdoc cref="IMutator.Patches"/>
        /// </summary>
        public IReadOnlyList<Type> Patches { get; }

        /// <summary>
        /// <inheritdoc cref="IMutator.Conditions"/>
        /// <remarks>
        /// For multi-mutators, this is the union of all conditions of the sub-mutators plus any additional conditions passed to the constructor.
        /// </remarks>
        /// </summary>
        public IReadOnlyList<Func<bool>> Conditions => new ReadOnlyCollection<Func<bool>>(_conditions);

        /// <summary>
        /// <inheritdoc cref="IMultiMutator.SubMutators"/>
        /// </summary>
        public IReadOnlyDictionary<IMutator, IDictionary<string, object>> SubMutators { get; }

        /// <param name="settings">The settings for the Mutator.</param>
        /// <param name="mutators">The sub-mutators the current multi-mutator consists of.</param>
        /// <param name="difficulty">Difficulty of the mutator. If not, this is automatically set based on the highest tier of the sub-mutator difficulties.</param>
        /// <param name="conditions">The conditions under which the current multi-mutator can be activated, must all be true to pass.</param>
        /// <param name="source">Whether the mutator was created by a mod or self-configured by the user.</param>
        public MultiMutator(AbstractMutatorSettings settings, IDictionary<IMutator, IDictionary<string, object>> mutators, MutatorDifficulty? difficulty = null, IList<Func<bool>> conditions = null!, MutatorSource source = MutatorSource.Mod)
        {
            Settings = settings;
            SubMutators = new ReadOnlyDictionary<IMutator, IDictionary<string, object>>(mutators);

            HasSpecialAction = SubMutators.Any(mutator => mutator.Key.HasSpecialAction);

            _conditions = mutators.SelectMany(mut => mut.Key.Conditions).ToList();
            if (conditions is { Count: > 0 })
            {
                foreach (Func<bool> condition in conditions)
                {
                    _conditions.Add(condition);
                }
            }

            Source = source;
            Difficulty = difficulty ?? mutators.Select(mut => mut.Key.Difficulty).Max();
            Patches = mutators.SelectMany(mut => mut.Key.Patches).ToList().AsReadOnly();
        }

        /// <summary>
        /// <inheritdoc cref="IMutator.Patch"/>
        /// <para>
        /// Actives the Harmony patches of this multi-mutator's sub-mutators. See <see cref="Mutator.Patch"/> for more details.
        /// </para>
        /// <remarks>
        /// This also applies any runtime overrides that were set by the host.
        /// </remarks>
        /// </summary>
        public void Patch()
        {
            if (Active) return;

            Active = true;

            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                foreach (KeyValuePair<IMutator, IDictionary<string, object>> subMutator in SubMutators)
                {
                    subMutator.Key.Settings.ClearRuntimeOverrides();
                    subMutator.Key.Settings.ApplyRuntimeOverrides(subMutator.Value);
                }
            }

            foreach (IMutator subMutator in SubMutators.Keys)
            {
                subMutator.Patch();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IMutator.Unpatch"/>
        /// <para>
        /// Deactivates the Harmony patches of this multi-mutator's sub-mutators. See <see cref="Mutator.Unpatch"/> for more details.
        /// </para>
        /// <remarks>
        /// This also clears any runtime overrides that were set by the host during <see cref="Patch"/>.
        /// </remarks>
        /// </summary>
        public void Unpatch()
        {
            if (!Active) return;

            foreach (IMutator subMutator in SubMutators.Keys)
            {
                subMutator.Unpatch();
                subMutator.Settings.ClearRuntimeOverrides();
            }

            Active = false;
        }

        /// <summary>
        /// <inheritdoc cref="IMutator.ConsumeMetadata"/>
        /// <para>
        /// For multi-mutators, this method delegates to the <see cref="Mutator.ConsumeMetadata">ConsumeMetadata</see> methods of its sub-mutators.
        /// </para>
        /// </summary>
        /// <param name="metadata"><inheritdoc cref="IMutator.ConsumeMetadata"/></param>
        public void ConsumeMetadata(IDictionary<string, object> metadata)
        {
            foreach (IMutator subMutator in SubMutators.Keys)
            {
                subMutator.ConsumeMetadata(metadata);
            }
        }
    }
}
