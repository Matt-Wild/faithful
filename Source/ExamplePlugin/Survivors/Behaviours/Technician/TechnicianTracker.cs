using RoR2;
using System.Linq;
using UnityEngine;

namespace Faithful
{
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(TeamComponent))]
    [RequireComponent(typeof(CharacterBody))]
    internal class TechnicianTracker : MonoBehaviour
    {
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
            trackingPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
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
            // Increment stopwatch and check if tracking update is required
            trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
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
    }
}
