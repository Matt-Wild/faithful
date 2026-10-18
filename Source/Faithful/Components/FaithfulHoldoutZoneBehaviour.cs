using RoR2;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulHoldoutZoneBehaviour : MonoBehaviour
    {
        // Store Holdout Zone
        private HoldoutZoneController zone;

        // Store Holdout Zone
        private List<OnHoldoutZoneCalcRadiusCallback> onHoldoutZoneCalcRadiusCallbacks;

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
        }

        private void FixedUpdate()
        {
            // This is Quality behaviour
            if (!Utils.qualityEnabled) return;

            // Get character bodies needed inside the holdout zone for quality buff to apply
            holders.Clear();
            foreach (CharacterBody body in Utils.GetAlivePlayerBodies())
            {
                // Add to holders if they have any spacious umbrellas
                if (body.inventory == null) continue;
                if (body.inventory.GetItemCountEffective(Faithful.spaciousUmbrella.MainItem.itemDef.itemIndex) > 0) holders.Add(body);
            }
            if (holders.Count == 0) return;

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
                playerInside = false;
                qualityTimer = 0.0f;
                qualityBuffsApplied = 0;
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

            // Quality behaviour, increase radius if buffs are applied and quality is enabled
            if (Utils.qualityEnabled && qualityBuffsApplied > 0)
            {
                _radius += GetQualityRadiusIncrease();
            }
        }

        float GetQualityRadiusIncrease()
        {
            // Ignore if no holders or quality buffs
            if (holders.Count == 0 || qualityBuffsApplied == 0) return 0.0f;

            // Get valid holders (ones inside zone)
            List<CharacterBody> validHolders = [];
            foreach (CharacterBody holder in holders)
            {
                if (Utils.IsCharacterInHoldoutZone(holder, zone)) validHolders.Add(holder);
            }
            if (validHolders.Count == 0) return 0.0f;

            // Sum up quality counts for all relevant holders
            int uncommonCount = 0;
            int rareCount = 0;
            int epicCount = 0;
            int legendaryCount = 0;
            foreach (CharacterBody holder in validHolders)
            {
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
