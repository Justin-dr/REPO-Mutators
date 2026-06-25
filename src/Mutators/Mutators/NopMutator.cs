using System;
using System.Collections.Generic;
using Mutators.Enums;
using Mutators.Settings;

namespace Mutators.Mutators
{
    /// <summary>
    /// Implementation of <see cref="IMutator"/> that does nothing.
    /// </summary>
    public sealed class NopMutator : IMutator
    {
        private static readonly IReadOnlyList<Type> EmptyPatches = [];
        private static readonly IReadOnlyList<Func<bool>> EmptyConditions = [];

        /// <summary>
        /// <inheritdoc cref="IMutator.NamespacedName"/>
        /// </summary>
        public string NamespacedName => Settings.NamespacedName;
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Name"/>
        /// </summary>
        public string Name => Mutators.NopMutatorName;

        /// <summary>
        /// <inheritdoc cref="IMutator.Description"/>
        /// </summary>
        public string Description => Mutators.NopMutatorDescription;
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Difficulty"/>
        /// <remarks>
        /// This is always <see cref="MutatorDifficulty.Negligible"/>.
        /// </remarks>
        /// </summary>
        public MutatorDifficulty Difficulty => MutatorDifficulty.Negligible;

        /// <summary>
        /// <inheritdoc cref="IMutator.Source"/>
        /// <remarks>
        /// This is always <see cref="MutatorSource.Mod"/>.
        /// </remarks>
        /// </summary>
        public MutatorSource Source => MutatorSource.Mod;
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Active"/>
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// <inheritdoc cref="IMutator.Settings"/>
        /// </summary>
        public AbstractMutatorSettings Settings { get; }

        /// <summary>
        /// <inheritdoc cref="IMutator.Patches"/>
        /// <remarks>
        /// This is always an empty list.
        /// </remarks>
        /// </summary>
        public IReadOnlyList<Type> Patches => EmptyPatches;

        /// <summary>
        /// <inheritdoc cref="IMutator.Conditions"/>
        /// <remarks>
        /// This is always an empty list.
        /// </remarks>
        /// </summary>
        public IReadOnlyList<Func<bool>> Conditions => EmptyConditions;

        /// <summary>
        /// <inheritdoc cref="IMutator.HasSpecialAction"/>
        /// <remarks>
        /// This is always false.
        /// </remarks>
        /// </summary>
        public bool HasSpecialAction => false;

        internal NopMutator(AbstractMutatorSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// <inheritdoc cref="IMutator.Patch"/>
        /// <remarks>
        /// This mutator has no runtime behaviour.
        /// </remarks>
        /// </summary>
        public void Patch()
        {
            Active = true;
        }

        /// <summary>
        /// <inheritdoc cref="IMutator.Unpatch"/>
        /// <remarks>
        /// This mutator has no runtime behaviour.
        /// </remarks>
        /// </summary>
        public void Unpatch()
        {
            Active = false;
        }

        /// <summary>
        /// <inheritdoc cref="IMutator.ConsumeMetadata"/>
        /// </summary>
        /// <remarks>
        /// This method has no implementation.
        /// </remarks>
        /// <param name="metadata"><inheritdoc/></param>
        public void ConsumeMetadata(IDictionary<string, object> metadata)
        {

        }
    }
}
