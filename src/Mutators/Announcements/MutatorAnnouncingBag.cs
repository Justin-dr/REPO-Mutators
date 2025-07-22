using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mutators.Managers;
using Mutators.Mutators;

namespace Mutators.Announcements
{
    /// <summary>
    /// Bag holding <see cref="MutatorAnnouncement"/>s.
    /// </summary>
    public class MutatorAnnouncingBag
    {
        private int _index;
        private MutatorAnnouncement[] _announcements = null!;
        private readonly IDictionary<string, MutatorAnnouncement> _announcementsByNamespacedName = new Dictionary<string, MutatorAnnouncement>();
        
        /// <summary>
        /// The singleton instance of the <see cref="MutatorAnnouncingBag"/>.
        /// </summary>
        public static MutatorAnnouncingBag Instance { get; } = new MutatorAnnouncingBag();
        internal MutatorAnnouncement Current => _announcements[_index];
        internal event Action<MutatorAnnouncement>? CurrentChanged;

        internal void Prime(IMutator mutator)
        {
            _index = 0;
            _announcementsByNamespacedName.Clear();

            IList<IMutator> mutators = GetMutators(mutator).ToList();

            if (mutators.Count == 0)
            {
                _announcements = [];
                return;
            }

            _announcements = new MutatorAnnouncement[mutators.Count];
            
            IMutator currentMutator = MutatorManager.Instance.CurrentMutator;
            bool isMultiMutator = currentMutator is IMultiMutator;
            
            for (int i = 0; i < mutators.Count; i++)
            {
                IMutator mut = mutators[i];
                string displayName =
                    isMultiMutator && !string.IsNullOrEmpty(currentMutator.Name)
                        ? currentMutator.Name
                        : mut.Name + $"{(mutators.Count > 1 ? $" + {mutators.Count - 1}" : string.Empty)}";

                string description =
                    isMultiMutator && !string.IsNullOrEmpty(currentMutator.Description)
                        ? currentMutator.Description
                        : mut.Description;
                
                MutatorAnnouncement announcement = new MutatorAnnouncement(displayName, description);
                announcement.Changed += HandleAnnouncementChanged;

                _announcements[i] = announcement;
                _announcementsByNamespacedName[mut.NamespacedName] = announcement;
            }

            CurrentChanged?.Invoke(Current);
        }

        private static IEnumerable<IMutator> GetMutators(IMutator mutator)
        {
            return mutator is not IMultiMutator multiMutator ? [mutator] : multiMutator.SubMutators.Keys;
        }

        internal bool MoveNext()
        {
            if (_announcements.Length <= 1)
            {
                return false;
            }

            _index = (_index + 1) % _announcements.Length;
            CurrentChanged?.Invoke(Current);
            return true;
        }

        internal bool MoveTo(MutatorAnnouncement announcement)
        {
            for (int i = 0; i < _announcements.Length; i++)
            {
                if (announcement == _announcements[i])
                {
                    _index = i;
                    CurrentChanged?.Invoke(Current);
                    return true;
                }
            }
            
            return false;
        }

        private void HandleAnnouncementChanged(MutatorAnnouncement announcement)
        {
            if (announcement == Current)
            {
                CurrentChanged?.Invoke(announcement);
            }
        }

        /// <summary>
        /// Attempt to get an <see cref="MutatorAnnouncement"/> by its namespaced name.
        /// </summary>
        /// <param name="namespacedName">The unique identifier of the announcement.</param>
        /// <param name="announcement">The <see cref="MutatorAnnouncement"/>, or null if no announcement exists with the specified namespacedName.</param>
        /// <returns>True if an announcement was found, otherwise false.</returns>
        public bool TryGetAnnouncement(string namespacedName, [NotNullWhen(true)] out MutatorAnnouncement? announcement)
        {
            if (_announcementsByNamespacedName.TryGetValue(namespacedName, out MutatorAnnouncement? a))
            {
                announcement = a;
                return true;
            }

            announcement = null;
            return false;
        }
        
        /// <summary>
        /// Whether the bag currently holds announcements.
        /// </summary>
        public bool IsEmpty => _announcements.Length == 0;
        
        /// <summary>
        /// The number of announcements currently held.
        /// </summary>
        public int Count => _announcements.Length;
    }
}
