using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulHoldoutZoneBehaviour : MonoBehaviour
    {
        // Store Holdout Zone
        private HoldoutZoneController zone;

        // Store Holdout Zone
        private List<OnHoldoutZoneCalcRadiusCallback> onHoldoutZoneCalcRadiusCallbacks;

        private void Awake()
        {
            // Get Holdout Zone
            zone = GetComponent<HoldoutZoneController>();
        }

        private void OnEnable()
        {
            // Ensure it isn't the escape ship (Just called "HoldoutZone" for some reason lol)
            if (zone.name != "HoldoutZone")
            {
                // Add Holdout Zone hooks
                zone.calcRadius += OnCalcRadius;
            }
        }

        private void OnDisable()
        {
            // Ensure it isn't the escape ship (Just called "HoldoutZone" for some reason lol)
            if (zone.name != "HoldoutZone")
            {
                // Remove Holdout Zone hooks
                zone.calcRadius -= OnCalcRadius;
            }
        }

        public void Init(List<OnHoldoutZoneCalcRadiusCallback> _onHoldoutZoneCalcRadiusCallbacks)
        {
            // Pass reference to Holdout Zone callbacks
            onHoldoutZoneCalcRadiusCallbacks = _onHoldoutZoneCalcRadiusCallbacks;
        }

        void OnCalcRadius(ref float _radius)
        {
            // Cycle through on Holdout Zone calc radius callbacks
            foreach (OnHoldoutZoneCalcRadiusCallback callback in onHoldoutZoneCalcRadiusCallbacks)
            {
                // Call
                callback(ref _radius, zone);
            }
        }
    }
}
