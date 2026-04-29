using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal static class Buffs
    {
        // List of buffs
        static List<Buff> buffs;

        public static void Init()
        {
            // Initialise buffs list
            buffs = new List<Buff>();

            // Add hooks
            On.RoR2.CharacterModel.UpdateOverlays += OnUpdateOverlays;

            // Mark overlay-backed buff models dirty when the buff appears/disappears
            On.RoR2.CharacterBody.OnBuffFirstStackGained += OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += OnBuffFinalStackLost;
        }

        public static Buff AddBuff(string _token, string _safeName, string _iconDir, Color _colour, bool _canStack = true, bool _isDebuff = false, bool _isHidden = false, bool _hasConfig = true, bool _usePercentageDisplay = false, Overlays.Overlay _overlay = null, string _langTokenOverride = null, bool _qualityBuff = false)
        {
            // Create buff
            Buff newBuff = new Buff(_token, _safeName, _iconDir, _colour, _canStack, _isDebuff, _isHidden, _hasConfig, _usePercentageDisplay, _overlay, _langTokenOverride, _qualityBuff);

            // Add buff to buffs list
            buffs.Add(newBuff);

            // Return new buff
            return newBuff;
        }

        private static void OnUpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            // Run vanilla overlay logic first
            orig(self);

            // Validate model
            if (self == null) return;

            // Remove stale Faithful overlays from previous overlay rebuilds
            // This makes the hook idempotent: every rebuild starts from "no Faithful overlays",
            // then adds only the Faithful overlays that should exist right now
            RemoveFaithfulOverlays(self);

            // Validate body
            CharacterBody body = self.body;
            if (body == null) return;

            // Do not require exact Visible - just avoid applying to invisible models
            if (self.visibility == VisibilityLevel.Invisible) return;

            // Check if body is alive
            if (body.healthComponent == null || !body.healthComponent.alive) return;

            // Cycle through buffs
            foreach (Buff buff in buffs)
            {
                // Check if buff has overlay
                if (buff == null || !buff.overlayEnabled) continue;

                // Fetch overlay and buff def
                Overlays.Overlay overlay = buff.overlay;
                BuffDef buffDef = buff.buffDef;

                // Validate overlay and buff def
                if (overlay == null || buffDef == null) continue;
                if (overlay.Material == null) continue;

                // Check if body has this buff
                if (!body.HasBuff(buffDef)) continue;

                // Check if there is room for another overlay
                if (self.activeOverlayCount >= CharacterModel.maxOverlays) break;

                int existingSameMatCount = 0;

                for (int i = 0; i < self.activeOverlayCount; i++)
                {
                    if (self.currentOverlays[i] == overlay.Material)
                    {
                        existingSameMatCount++;
                    }
                }

                // Add overlay material
                self.currentOverlays[self.activeOverlayCount] = overlay.Material;
                self.activeOverlayCount++;
            }
        }

        private static void OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            // Run vanilla logic first
            orig(self, buffDef);

            // If this is one of our overlay-backed buffs, request a natural material refresh
            MarkOverlayDirtyIfFaithfulOverlayBuff(self, buffDef);
        }

        private static void OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            // Run vanilla logic first
            orig(self, buffDef);

            // If this is one of our overlay-backed buffs, request a natural material refresh
            MarkOverlayDirtyIfFaithfulOverlayBuff(self, buffDef);
        }

        private static void MarkOverlayDirtyIfFaithfulOverlayBuff(CharacterBody _body, BuffDef _buffDef)
        {
            // Validate input
            if (_body == null || _buffDef == null) return;

            // Check if this buff is a Faithful buff with an overlay
            if (!BuffHasFaithfulOverlay(_buffDef)) return;

            // Mark body model dirty
            MarkBodyMaterialsDirty(_body);
        }

        private static bool BuffHasFaithfulOverlay(BuffDef _buffDef)
        {
            // Validate input
            if (_buffDef == null) return false;

            // Check if buff def matches a Faithful buff with an overlay
            foreach (Buff buff in buffs)
            {
                if (buff == null) continue;
                if (!buff.overlayEnabled) continue;
                if (buff.buffDef == null) continue;

                if (buff.buffDef == _buffDef) return true;
            }

            return false;
        }

        private static void MarkBodyMaterialsDirty(CharacterBody _body)
        {
            // Validate input
            if (_body == null) return;

            // Get model locator
            ModelLocator modelLocator = _body.modelLocator;
            if (modelLocator == null) return;

            // Get model transform
            Transform modelTransform = modelLocator.modelTransform;
            if (modelTransform == null) return;

            // Get character model
            CharacterModel characterModel = modelTransform.GetComponent<CharacterModel>() ?? modelTransform.GetComponentInChildren<CharacterModel>();
            if (characterModel == null) return;

            // Do not manually call UpdateOverlays or UpdateMaterials here
            // Just request a normal CharacterModel material refresh
            characterModel.materialsDirty = true;
        }

        private static void RemoveFaithfulOverlays(CharacterModel _model)
        {
            // Validate input
            if (_model == null) return;

            int writeIndex = 0;

            // Compact all non-Faithful overlays towards the start of the overlay array
            for (int readIndex = 0; readIndex < _model.activeOverlayCount; readIndex++)
            {
                Material material = _model.currentOverlays[readIndex];

                // Skip old Faithful overlays
                if (IsFaithfulOverlayMaterial(material)) continue;

                // Keep non-Faithful overlays
                _model.currentOverlays[writeIndex] = material;
                writeIndex++;
            }

            // Clear leftover slots
            for (int i = writeIndex; i < _model.activeOverlayCount; i++)
            {
                _model.currentOverlays[i] = null;
            }

            // Update count
            _model.activeOverlayCount = writeIndex;
        }

        private static bool IsFaithfulOverlayMaterial(Material _material)
        {
            // Validate input
            if (_material == null) return false;

            // Check if material belongs to any Faithful overlay-backed buff
            foreach (Buff buff in buffs)
            {
                if (buff == null) continue;
                if (!buff.overlayEnabled) continue;
                if (buff.overlay == null) continue;

                if (buff.overlay.Material == _material) return true;
            }

            return false;
        }

        public static Buff GetBuff(string _token)
        {
            // Cycle through buffs
            foreach (Buff buff in buffs)
            {
                // Check if correct token
                if (buff.token == _token)
                {
                    // Return buff
                    return buff;
                }
            }

            // Return null if not found
            Log.Error($"Attempted to fetch buff '{_token}' but couldn't find it");
            return null;
        }
    }
}