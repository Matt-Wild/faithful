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

        // Support beacon radius contribution
        private bool supportBeacon;
        public float SupportBeaconRadiusIncrease { get; private set; }

        // Players holding umbrellas
        private List<CharacterBody> holders = [];

        // Quality behaviour state
        private bool playerInside = false;
        private float qualityTimer = 0.0f;
        private int qualityBuffsApplied = 0;

        // Quality settings
        private QualityValues<float> sizeQualityValues = new();
        private QualityValues<float> sizeStackingQualityValues = new();
        private float intervalQuality;
        private int maxInstancesQuality;

        private void Awake()
        {
            // Get Holdout Zone
            zone = GetComponent<HoldoutZoneController>();
            supportBeacon = GetComponent<SupportBeaconController>() != null;

            // Update quality settings if quality is enabled
            if (Utils.qualityEnabled)
            {
                sizeQualityValues = Faithful.spaciousUmbrella.SizeQualityValues;
                sizeStackingQualityValues = Faithful.spaciousUmbrella.SizeStackingQualityValues;
                intervalQuality = Faithful.spaciousUmbrella.IntervalQuality;
                maxInstancesQuality = Faithful.spaciousUmbrella.MaxInstancesQuality;
            }
        }

        private void OnEnable()
        {
            // Ensure it isn't the escape ship (Just called "HoldoutZone" for some reason lol)
            if (zone.name != "HoldoutZone")
            {
                // Add Holdout Zone hooks
                zone.calcRadius += OnCalcRadius;
            }

            Utils.RegisterActiveHoldoutZone(zone);
        }

        private void OnDisable()
        {
            // Ensure it isn't the escape ship (Just called "HoldoutZone" for some reason lol)
            if (zone.name != "HoldoutZone")
            {
                // Remove Holdout Zone hooks
                zone.calcRadius -= OnCalcRadius;
            }

            Utils.UnregisterActiveHoldoutZone(zone);

            // Clear this helper's contribution when it is disabled
            ResetQualityState();
            UpdateSupportBeaconRadius();
        }

        private void FixedUpdate()
        {
            // Ordinary zones only need the Quality tick
            if (!Utils.qualityEnabled && !supportBeacon) return;

            FaithfulRuntimeProfiler.StartScope("FaithfulHoldoutZoneBehaviour.FixedUpdate");
            try
            {
                // Update Quality state before refreshing the beacon radius
                if (Utils.qualityEnabled && zone.isActiveAndEnabled && (!supportBeacon || (Behaviour.supportBeaconRadiusAvailable && Faithful.spaciousUmbrella.AffectRallyPoint))) UpdateQualityState();
                else ResetQualityState();

                UpdateSupportBeaconRadius();
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("FaithfulHoldoutZoneBehaviour.FixedUpdate");
            }
        }

        private void UpdateQualityState()
        {
            // Get character bodies needed inside the holdout zone for quality buff to apply
            Utils.GetAlivePlayerBodies(holders);
            for (int i = holders.Count - 1; i >= 0; i--)
            {
                // Remove players without spacious umbrellas
                CharacterBody body = holders[i];
                if (body.inventory == null || body.inventory.GetItemCountEffective(Faithful.spaciousUmbrella.MainItem.itemDef.itemIndex) <= 0) holders.RemoveAt(i);
            }
            if (holders.Count == 0)
            {
                ResetQualityState();
                return;
            }

            // Check if at least one holder is inside the holdout zone
            bool inside = false;
            foreach (CharacterBody holder in holders)
            {
                if (Utils.IsCharacterInHoldoutZone(holder, zone))
                {
                    inside = true;
                    break;
                }
            }

            // If no holders are inside the holdout zone, remove quality buffs and reset quality timer
            if (!inside)
            {
                ResetQualityState();
                return;
            }

            // If a holder has just entered the holdout zone, start quality timer
            if (!playerInside)
            {
                playerInside = true;
                qualityTimer = Time.time;
                qualityBuffsApplied = 0;
            }

            // If at least one holder has been inside the holdout zone for the required interval, apply quality buff and reset timer
            else if (Time.time - qualityTimer >= intervalQuality && qualityBuffsApplied < maxInstancesQuality)
            {
                qualityBuffsApplied++;
                qualityTimer = Time.time;
            }
        }

        private void ResetQualityState()
        {
            playerInside = false;
            qualityTimer = 0.0f;
            qualityBuffsApplied = 0;
        }

        public void UpdateSupportBeaconRadius()
        {
            // Ordinary holdout zones use calcRadius
            if (!supportBeacon || zone == null) return;

            FaithfulRuntimeProfiler.StartScope("FaithfulHoldoutZoneBehaviour.UpdateSupportBeaconRadius");
            try
            {
                // Cache the increase separately so other items still see the original base radius
                SupportBeaconRadiusIncrease = 0.0f;
                if (isActiveAndEnabled && zone.isActiveAndEnabled && zone.ignoreRadiusModifiers && Behaviour.supportBeaconRadiusAvailable && Faithful.spaciousUmbrella.AffectRallyPoint)
                {
                    SupportBeaconRadiusIncrease = Faithful.spaciousUmbrella.GetRadiusIncrease(zone.baseRadius);
                    if (Utils.qualityEnabled && qualityBuffsApplied > 0) SupportBeaconRadiusIncrease += GetQualityRadiusIncrease();
                }
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("FaithfulHoldoutZoneBehaviour.UpdateSupportBeaconRadius");
            }
        }

        public void Init(List<OnHoldoutZoneCalcRadiusCallback> _onHoldoutZoneCalcRadiusCallbacks)
        {
            // Pass reference to Holdout Zone callbacks
            onHoldoutZoneCalcRadiusCallbacks = _onHoldoutZoneCalcRadiusCallbacks;
        }

        void OnCalcRadius(ref float _radius)
        {
            // Fixed radius bonuses are supplied before the game's calcRadius notifications
            if (supportBeacon && zone.ignoreRadiusModifiers) return;

            FaithfulRuntimeProfiler.StartScope("FaithfulHoldoutZoneBehaviour.OnCalcRadius");
            try
            {
                // Cycle through on Holdout Zone calc radius callbacks
                foreach (OnHoldoutZoneCalcRadiusCallback callback in onHoldoutZoneCalcRadiusCallbacks)
                {
                    // Call
                    callback(ref _radius, zone);
                }

                // Quality behaviour, increase radius if buffs are applied and quality is enabled
                if (Utils.qualityEnabled && qualityBuffsApplied > 0)
                {
                    _radius += GetQualityRadiusIncrease();
                }
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("FaithfulHoldoutZoneBehaviour.OnCalcRadius");
            }
        }

        float GetQualityRadiusIncrease()
        {
            // Ignore if no holders or quality buffs
            if (holders.Count == 0 || qualityBuffsApplied == 0) return 0.0f;

            // Sum up quality counts for all relevant holders
            int uncommonCount = 0;
            int rareCount = 0;
            int epicCount = 0;
            int legendaryCount = 0;
            foreach (CharacterBody holder in holders)
            {
                // Only count valid holders inside the zone
                if (holder == null || holder.inventory == null || !Utils.IsCharacterInHoldoutZone(holder, zone)) continue;

                QualityCounts counts = QualityCompat.GetItemCountsEffective(holder.inventory, Faithful.spaciousUmbrella.MainItem);
                uncommonCount += counts.UNCOMMON;
                rareCount += counts.RARE;
                epicCount += counts.EPIC;
                legendaryCount += counts.LEGENDARY;
            }

            // Skip if no quality items are held
            if (uncommonCount == 0 && rareCount == 0 && epicCount == 0 && legendaryCount == 0) return 0.0f;

            // Calculate radius increase per buff instance
            float radiusIncreasePerBuff = 0.0f;
            radiusIncreasePerBuff += Utils.CalculateStackingValue(uncommonCount, sizeQualityValues.UNCOMMON, sizeStackingQualityValues.UNCOMMON);
            radiusIncreasePerBuff += Utils.CalculateStackingValue(rareCount, sizeQualityValues.RARE, sizeStackingQualityValues.RARE);
            radiusIncreasePerBuff += Utils.CalculateStackingValue(epicCount, sizeQualityValues.EPIC, sizeStackingQualityValues.EPIC);
            radiusIncreasePerBuff += Utils.CalculateStackingValue(legendaryCount, sizeQualityValues.LEGENDARY, sizeStackingQualityValues.LEGENDARY);

            // Return total radius increase based on number of quality buff instances applied
            return radiusIncreasePerBuff * qualityBuffsApplied;
        }
    }
}
