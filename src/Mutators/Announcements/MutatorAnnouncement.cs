using System;
using System.Collections.Generic;
using System.Linq;
using Mutators.Extensions;

namespace Mutators.Announcements
{
    /// <summary>
    /// Represents a single mutator announcement, consisting of the name and the description of a Mutator.
    /// <para>
    /// <see cref="MutatorAnnouncementDescriptionSegment"/>s can be provided to add additional text to the description.
    /// These segments are sorted by priority, with the highest priority segments appearing first.
    /// </para>
    /// </summary>
    public sealed class MutatorAnnouncement
    {
        private bool _isDirty;
        private string _name;
        private string _description = string.Empty;
        private readonly List<MutatorAnnouncementDescriptionSegment> _descriptionSegments;
        
        /// <summary>
        /// The base description of the mutator. Can be added upon using <see cref="AddSegment(MutatorAnnouncementDescriptionSegment)"/>.
        /// </summary>
        public MutatorAnnouncementDescriptionSegment DescriptionBase { get; }
        
        /// <summary>
        /// Whether it is possible to update the base description of the mutator. Segments may still be added or removed regardless.
        /// </summary>
        public bool IsUpdatingBaseDescriptionAllowed { get; }

        internal event Action<MutatorAnnouncement>? Changed;

        internal MutatorAnnouncement(string name, string description, bool allowEditBaseDescription)
        {
            _name = name;
            DescriptionBase = new MutatorAnnouncementDescriptionSegment("description".ToSlug(name), 0, description);
            IsUpdatingBaseDescriptionAllowed = allowEditBaseDescription;
            _descriptionSegments = [];
            
            BuildDescription();
        }
        

        /// <summary>
        /// The display name of the mutator, with any additional modifications.
        /// </summary>
        /// <remarks>
        /// The value returned by this method is cached and will not be rebuilt
        /// until any modifiers are added or removed.
        /// </remarks>
        public string GetName() => _name;

        /// <summary>
        /// Returns the base description of the mutator, with any additional segments added.
        /// <remarks>
        /// The value returned by this method is cached and will not be rebuilt
        /// until either the base description or any additional segments are changed.
        /// </remarks>
        /// </summary>
        public string GetDescription()
        {
            if (!_isDirty)
            {
                return _description;
            }

            BuildDescription();
            _isDirty = false;
            
            return _description;
        }

        /// <summary>
        /// Updates the base description.
        /// If changes were made, the description will be rebuilt next time <see cref="GetDescription"/> is called.
        /// </summary>
        /// <param name="description">The new base description value</param>
        public void UpdateBaseDescription(string description)
        {
            if (!IsUpdatingBaseDescriptionAllowed || DescriptionBase.Value == description) return;
            
            DescriptionBase.Value = description;
            NotifyChanged();
        }

        /// <summary>
        /// Updates a segment of the description.
        /// If changes were made, the description will be rebuilt next time <see cref="GetDescription"/> is called.
        /// </summary>
        /// <param name="key">The key of the segment that should be updated</param>
        /// <param name="value">The new value for the description segment</param>
        /// <param name="priority">The new priority, or null to keep the current priority.</param>
        public void UpdateSegment(string key, string value, ushort? priority = null)
        {
            MutatorAnnouncementDescriptionSegment? segment = GetDescriptionSegment(key);
            
            if (segment == null) return;

            bool isValueChanged = segment.Value != value;
            ushort newPriority = priority.GetValueOrDefault();
            bool isPriorityChanged = priority.HasValue && segment.Priority != newPriority;
            if (!isValueChanged && !isPriorityChanged) return;

            if (isPriorityChanged)
            {
                segment.Priority = newPriority;
                SortSegments();
            }

            if (isValueChanged)
            {
                segment.Value = value;
            }

            NotifyChanged();
        }

        /// <summary>
        /// Removes a segment from the description.
        /// If changes were made, the description will be rebuilt next time <see cref="GetDescription"/> is called.
        /// </summary>
        public bool RemoveSegment(string key)
        {
            MutatorAnnouncementDescriptionSegment? segment = GetDescriptionSegment(key);
            
            if (segment == null) return false;
            if (!_descriptionSegments.Remove(segment)) return false;

            NotifyChanged();
            
            return true;
        }

        /// <summary>
        /// Adds a segment to the description.
        /// If changes were made, the description will be rebuilt next time <see cref="GetDescription"/> is called.
        /// </summary>
        /// <returns>
        /// True if the segment was added successfully, false if the key is already in use.
        /// </returns>
        public bool AddSegment(MutatorAnnouncementDescriptionSegment segment)
        {
            if (!IsKeyUnique(segment.Key))
            {
                return false;
            }

            _descriptionSegments.Add(segment);
            SortSegments();

            NotifyChanged();
            
            return true;
        }

        /// <summary>
        /// Adds/updates a segment to/of the description.
        /// If changes were made, the description will be rebuilt next time <see cref="GetDescription"/> is called.
        /// </summary>
        /// <returns>
        /// True if the changes were made, false if a segment with the same key already exists.
        /// </returns>
        public bool AddOrUpdateSegment(MutatorAnnouncementDescriptionSegment segment)
        {
            MutatorAnnouncementDescriptionSegment? existingSegment = GetDescriptionSegment(segment.Key);

            bool success = true;
            
            if (existingSegment != null)
            {
                bool isPriorityChanged = existingSegment.Priority != segment.Priority;
                if (existingSegment.Value == segment.Value && !isPriorityChanged) return false;
                existingSegment.Priority = segment.Priority;
                existingSegment.Value = segment.Value;

                if (isPriorityChanged)
                {
                    SortSegments();
                }
                
                NotifyChanged();
            }
            else
            {
                success = AddSegment(segment);
            }
            
            return success;
        }

        /// <summary>
        /// Gets a segment from the description by its key.
        /// If changes were made, the description will be rebuilt next time <see cref="GetDescription"/> is called.
        /// </summary>
        /// <returns>The segment with the specified key, or null if no segment with the specified key exists.</returns>
        public MutatorAnnouncementDescriptionSegment? GetDescriptionSegment(string key)
        {
            foreach (MutatorAnnouncementDescriptionSegment segment in _descriptionSegments)
            {
                if (segment.Key == key)
                {
                    return segment;
                }
            }
            return null;
        }

        private bool IsKeyUnique(string key)
        {
            return _descriptionSegments.All(segment => segment.Key != key);
        }

        private void SortSegments()
        {
            _descriptionSegments.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        private void BuildDescription()
        {
            // This should be fine here? If not can be changed to StringBuilder
            _description = DescriptionBase.Value + string.Join("", _descriptionSegments.Select(segment => segment.Value));
        }

        private void NotifyChanged()
        {
            _isDirty = true;
            Changed?.Invoke(this);
        }
    }
}
