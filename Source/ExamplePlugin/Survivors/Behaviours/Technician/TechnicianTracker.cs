using Rewired.ComponentControls.Effects;
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

        // Whether the player is focusing mechanical ally targets
        bool allyFocus = false;

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

            // Check if target is locked
            if (IsLocked())
            {
                // Check if target still exists
                if (trackingTarget != null)
                {
                    // Search for potential targets
                    search.teamMaskFilter = TeamMask.all;
                    search.filterByLoS = true;
                    search.searchOrigin = aimRay.origin;
                    search.searchDirection = aimRay.direction;
                    search.sortMode = BullseyeSearch.SortMode.Angle;
                    search.maxDistanceFilter = maxTrackingDistanceLocked;
                    search.maxAngleFilter = maxTrackingAngleLocked;
                    search.RefreshCandidates();
                    search.FilterOutGameObject(gameObject);

                    // Don't acquire new target if current target is still in search
                    if (search.GetResults().Contains(trackingTarget)) return;
                }

                // Get new target
                if (allyFocus)
                {
                    search.teamMaskFilter = TeamMask.none;
                    search.teamMaskFilter.AddTeam(teamComponent.teamIndex);
                }
                else
                {
                    search.teamMaskFilter = TeamMask.all;
                    search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
                }
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

            // Target not locked
            else
            {
                // Search for target
                search.teamMaskFilter = TeamMask.all;
                search.filterByLoS = true;
                search.searchOrigin = aimRay.origin;
                search.searchDirection = aimRay.direction;
                search.sortMode = BullseyeSearch.SortMode.Angle;
                search.maxDistanceFilter = maxTrackingDistance;
                search.maxAngleFilter = maxTrackingAngle;
                search.RefreshCandidates();
                search.FilterOutGameObject(gameObject);

                // Filter out non-mechanical allies
                search.candidatesEnumerable.RemoveAll(delegate (BullseyeSearch.CandidateInfo _candidateInfo)
                {
                    // Get character body and team index
                    CharacterBody body = _candidateInfo.hurtBox.healthComponent.body;
                    TeamIndex teamIndex = _candidateInfo.hurtBox.teamIndex;

                    // Filter out if no character body or non-mechanical ally
                    return body == null || (teamIndex == teamComponent.teamIndex && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical));
                });

                trackingTarget = search.GetResults().FirstOrDefault();
            }
        }

        void UpdateIndicator()
        {
            // Skip if no visualiser instance
            if (indicator?.visualizerInstance == null) return;

            // Get indicator components
            RotateAroundAxis rotator = indicator.visualizerInstance.transform.Find("Holder").GetComponent<RotateAroundAxis>();
            GameObject[] technicianTrackerNibHolders = Utils.FindChildrenWithTerm(indicator.visualizerInstance.transform.Find("Holder"), "Nib Holder");
            Transform nib1 = technicianTrackerNibHolders[0].transform.Find("Nib");
            Transform nib2 = technicianTrackerNibHolders[1].transform.Find("Nib");

            // Check if locked
            if (IsLocked())
            {
                // Set as locked visual effect
                rotator.SetSpeed(RotateAroundAxis.Speed.Slow);
                nib1.localPosition = new Vector3(0.16f, 0.0f, 0.0f);
                nib2.localPosition = new Vector3(-0.16f, 0.0f, 0.0f);

                return;
            }

            // Not locked

            // Set as locked visual effect
            rotator.SetSpeed(RotateAroundAxis.Speed.Fast);
            nib1.localPosition = new Vector3(0.22f, 0.0f, 0.0f);
            nib2.localPosition = new Vector3(-0.22f, 0.0f, 0.0f);
        }

        bool IsLocked()
        {
            // Return if target is locked by any skill
            return locks[SkillSlot.Primary] || locks[SkillSlot.Secondary] || locks[SkillSlot.Utility] || locks[SkillSlot.Special];
        }

        public void SetLock(SkillSlot _skill, bool _lock)
        {
            // Get if target is previously considered locked
            bool prevLocked = IsLocked();

            // Set lock in dictionary
            locks[_skill] = _lock;

            // Does skill want to lock target and target exists
            if (_lock && trackingTarget != null)
            {
                // Update player intent (whether player is trying to lock allies)
                allyFocus = trackingTarget.teamIndex == teamComponent.teamIndex;
            }

            // Check if locked condition has changed
            if (IsLocked() != prevLocked)
            {
                // Update tracker visuals
                UpdateIndicator();
            }
        }
    }
}
