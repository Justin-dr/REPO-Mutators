using Mutators.Announcements;
using UnityEngine;

namespace Mutators.Mutators.Behaviours.UI
{
    internal class MutatorAnnouncementControllerBehaviour : MonoBehaviour
    {
        private const float CycleTime = 5f;
        private float _cycleTimer = CycleTime;
        private MutatorAnnouncingBag _announcingBag = null!;

        private void Awake()
        {
            _announcingBag = MutatorAnnouncingBag.Instance;
        }

        private void Start()
        {
            ApplyAnnouncement(_announcingBag.Current);
        }

        private void OnEnable()
        {
            _announcingBag.CurrentChanged += HandleCurrentChanged;
        }

        private void OnDisable()
        {
            _announcingBag.CurrentChanged -= HandleCurrentChanged;
        }

        private void HandleCurrentChanged(MutatorAnnouncement announcement)
        {
            _cycleTimer = CycleTime;
            ApplyAnnouncement(announcement);
        }

        private void Update()
        {
            if (_announcingBag.Count <= 1)
            {
                return;
            }

            _cycleTimer -= Time.deltaTime;
            if (_cycleTimer > 0f)
            {
                return;
            }
            
            _announcingBag.MoveNext();
        }

        private static void ApplyAnnouncement(MutatorAnnouncement announcement)
        {
            MutatorAnnouncingBehaviour.instance?.SetText(announcement.GetName());
            MutatorDescriptionAnnouncingBehaviour.Instance?.SetText(announcement.GetDescription());
        }
    }
}
