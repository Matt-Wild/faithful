using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Faithful
{
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(TeamComponent))]
    [RequireComponent(typeof(CharacterBody))]
    public class TechnicianTracker : MonoBehaviour
    {
        // Skill states that lock the tracker
        Dictionary<SkillSlot, bool> locks = new Dictionary<SkillSlot, bool>()
        {
            { SkillSlot.Primary, false },
            { SkillSlot.Secondary, false },
            { SkillSlot.Utility, false },
            { SkillSlot.Special, false }
        };

        // Target references
        GameObject trackingPrefab;
        HurtBox trackingTarget;

        // Owner references
        CharacterBody characterBody;
        TeamComponent teamComponent;
        InputBankTest inputBank;

        float trackerUpdateStopwatch;

        Indicator indicator;

        // Target searching logic
        readonly float maxTrackingDistance = 40.0f;
        readonly float maxTrackingAngle = 20.0f;
        readonly float trackerUpdateFrequency = 10.0f;
        readonly BullseyeSearch search = new();

        // Target searching logic for locked target
        readonly float maxTrackingDistanceLocked = 50.0f;
        readonly float maxTrackingAngleLocked = 26.0f;

        // Target searching logic for locking onto a new target after kill
        readonly float maxTrackingAngleContinued = 25.0f;

        void Awake()
        {
            // Create tracking indicator
            trackingPrefab = Assets.technicianTrackingIndicatorPrefab;
            indicator = new Indicator(gameObject, trackingPrefab);
        }

        void Start()
        {
            // Fetch owner components
            characterBody = GetComponent<CharacterBody>();
            inputBank = GetComponent<InputBankTest>();
            teamComponent = GetComponent<TeamComponent>();
        }

        public HurtBox GetTrackingTarget()
        {
            // Return the current tracked target
            return trackingTarget;
        }

        void OnEnable()
        {
            indicator.active = true;
        }

        void OnDisable()
        {
            indicator.active = false;
        }

        void FixedUpdate()
        {
            // Increment stopwatch and check if tracking update is required (do not change target if target is locked)
            trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                // Reset stopwatch
                trackerUpdateStopwatch -= 1f / trackerUpdateFrequency;

                // Search for new target and update indicator
                SearchForTarget();
                indicator.targetTransform = (trackingTarget ? trackingTarget.transform : null);
            }
        }

        public void SearchForTarget()
        {
            // Get aim ray for target search
            Ray aimRay = new(inputBank.aimOrigin, inputBank.aimDirection);

            // Check if target is locket
            if (trackingTarget != null && IsLocked())
            {
                // Search for target
                search.teamMaskFilter = TeamMask.all;
                search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
                search.filterByLoS = true;
                search.searchOrigin = aimRay.origin;
                search.searchDirection = aimRay.direction;
                search.sortMode = BullseyeSearch.SortMode.Angle;
                search.maxDistanceFilter = maxTrackingDistanceLocked;
                search.maxAngleFilter = maxTrackingAngleLocked;
                search.RefreshCandidates();
                search.FilterOutGameObject(gameObject);

                // Check if search does not contain current target
                if (!search.GetResults().Contains(trackingTarget))
                {
                    // Get new target
                    search.teamMaskFilter = TeamMask.all;
                    search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
                    search.filterByLoS = true;
                    search.searchOrigin = aimRay.origin;
                    search.searchDirection = aimRay.direction;
                    search.sortMode = BullseyeSearch.SortMode.Angle;
                    search.maxDistanceFilter = maxTrackingDistance;
                    search.maxAngleFilter = maxTrackingAngleContinued;
                    search.RefreshCandidates();
                    search.FilterOutGameObject(gameObject);
                    trackingTarget = search.GetResults().FirstOrDefault();
                }
            }

            // Target not locked
            else
            {
                // Search for target
                search.teamMaskFilter = TeamMask.all;
                search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
                search.filterByLoS = true;
                search.searchOrigin = aimRay.origin;
                search.searchDirection = aimRay.direction;
                search.sortMode = BullseyeSearch.SortMode.Angle;
                search.maxDistanceFilter = maxTrackingDistance;
                search.maxAngleFilter = maxTrackingAngle;
                search.RefreshCandidates();
                search.FilterOutGameObject(gameObject);
                trackingTarget = search.GetResults().FirstOrDefault();
            }
        }

        bool IsLocked()
        {
            // Return if target is locked by any skill
            return locks[SkillSlot.Primary] || locks[SkillSlot.Secondary] || locks[SkillSlot.Utility] || locks[SkillSlot.Special];
        }

        public void SetLock(SkillSlot _skill, bool _lock)
        {
            // Set lock in dictionary
            locks[_skill] = _lock;
        }
    }
}
