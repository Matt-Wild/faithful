using RoR2;
using UnityEngine;

namespace Faithful
{
    internal static class Overlays
    {
        internal sealed class OverlaySettings
        {
            public Material BaseMaterial = null;
            public string MaterialAddress = "RoR2/Base/CritOnUse/matFullCrit.mat";

            public Color Colour = new(0.75f, 1.0f, 0.95f, 0.35f);
            public string ColourPropertyName = "_TintColor";
        }

        internal sealed class Overlay
        {
            public readonly Material Material;

            public Overlay(Material _material)
            {
                Material = _material;
            }
        }

        internal static Overlay CreateOverlay(OverlaySettings _settings = null)
        {
            // Create default settings if not provided
            _settings ??= new OverlaySettings();

            // Attempt to fetch source material
            Material sourceMaterial = _settings.BaseMaterial;
            if (!sourceMaterial)
            {
                sourceMaterial = LoadMaterial(_settings.MaterialAddress);
            }

            if (!sourceMaterial)
            {
                Log.Error("[OVERLAYS] | Overlays.CreateOverlay failed - could not load a source material!");
                return null;
            }

            // Create a new material instance so we don't modify the source material
            Material overlayMaterial = new(sourceMaterial)
            {
                name = $"{sourceMaterial.name} (FaithfulOverlay)"
            };

            // Apply colour to the new material instance
            TryApplyColour(overlayMaterial, _settings.ColourPropertyName, _settings.Colour);

            // Create and return the overlay
            return new Overlay(overlayMaterial);
        }

        private static Material LoadMaterial(string _materialAddress)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(_materialAddress)) return null;

            // Attempt to load from assets first
            Material material = null;
            try
            {
                material = Assets.FetchAsset<Material>(_materialAddress);
            }
            catch
            {
            }

            // Attempt to load from legacy resources if not found in assets
            if (!material)
            {
                try
                {
                    material = LegacyResourcesAPI.Load<Material>(_materialAddress);
                }
                catch
                {
                }
            }

            return material;
        }

        private static void TryApplyColour(Material _material, string _colourPropertyName, Color _colour)
        {
            // Validate input
            if (!_material) return;

            // Attempt to apply the colour using the specified property name
            if (!string.IsNullOrWhiteSpace(_colourPropertyName) && _material.HasProperty(_colourPropertyName))
            {
                _material.SetColor(_colourPropertyName, _colour);
                return;
            }

            // Fallback to common property names if the specified one is not found
            if (_material.HasProperty("_TintColor"))
            {
                _material.SetColor("_TintColor", _colour);
                return;
            }

            if (_material.HasProperty("_Color"))
            {
                _material.SetColor("_Color", _colour);
            }
        }
    }
}