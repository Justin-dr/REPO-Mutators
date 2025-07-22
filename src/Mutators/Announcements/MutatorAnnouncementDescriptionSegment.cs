namespace Mutators.Announcements
{
    /// <summary>
    /// A description segment of a <see cref="MutatorAnnouncement"/>.
    /// </summary>
    /// <param name="key">The unique identifier of this segment</param>
    /// <param name="priority">The priority that will be used to sort segments, higher values come first</param>
    /// <param name="value">The text that will be appended to the description. Spaces and line breaks are to be included manually</param>
    public class MutatorAnnouncementDescriptionSegment(string key, ushort priority, string value)
    {
        /// <summary>
        /// The unique identifier of this description segment.
        /// </summary>
        public string Key { get; } = key;
        
        /// <summary>
        /// The priority that will be used to sort segments, higher values come first.
        /// </summary>
        public ushort Priority { get; internal set; } = priority;
        
        /// <summary>
        /// The text that will be appended to the description.
        /// </summary>
        public string Value { get; internal set; } = value;
    }
}