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
        readonly float maxTrackingDistance = 40f;
        readonly float maxTrackingAngle = 20f;
        readonly float trackerUpdateFrequency = 10f;
        readonly BullseyeSearch search = new();

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
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency && !IsLocked())
            {
                // Reset stopwatch
                trackerUpdateStopwatch -= 1f / trackerUpdateFrequency;

                // Search for new target and update indicator
                Ray aimRay = new(inputBank.aimOrigin, inputBank.aimDirection);
                SearchForTarget(aimRay);
                indicator.targetTransform = (trackingTarget ? trackingTarget.transform : null);
            }
        }

        void SearchForTarget(Ray _aimRay)
        {
            // Search for target
            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = _aimRay.origin;
            search.searchDirection = _aimRay.direction;
            search.sortMode = BullseyeSearch.SortMode.Angle;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(gameObject);
            trackingTarget = search.GetResults().FirstOrDefault();
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
