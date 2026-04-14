using RoR2;
using System;
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
            // Initialise items list
            buffs = new List<Buff>();

            // Add hooks
            On.RoR2.CharacterModel.UpdateOverlays += OnUpdateOverlays;
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

            // Validate input
            if (self == null || self.body == null) return;
            if (self.visibility != VisibilityLevel.Visible) return;

            // Check if body is alive
            if (self.body.healthComponent == null || !self.body.healthComponent.alive) return;

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
                if (!self.body.HasBuff(buffDef)) continue;

                // Check if there is room for another overlay
                if (self.activeOverlayCount >= CharacterModel.maxOverlays) break;

                // Add overlay material
                self.currentOverlays[self.activeOverlayCount] = overlay.Material;
                self.activeOverlayCount++;
            }
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
