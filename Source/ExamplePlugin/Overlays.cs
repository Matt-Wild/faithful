using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal static class Overlays
    {
        // Store active overlays so they can be removed early
        private static readonly Dictionary<(CharacterBody body, Overlay overlay), ActiveOverlay> m_activeOverlays = [];

        // Reused list to avoid allocating on each death
        private static readonly List<Overlay> m_overlaysToRemove = [];

        // Use a very long duration for "persistent" overlays
        private const float PersistentDuration = 999999.0f;

        static Overlays()
        {
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
        }

        private sealed class ActiveOverlay
        {
            public CharacterModel CharacterModel;
            public TemporaryOverlayInstance OverlayInstance;
        }

        internal sealed class OverlaySettings
        {
            public Material BaseMaterial = null;
            public string MaterialAddress = "RoR2/Base/CritOnUse/matFullCrit.mat";

            public Color Colour = new(0.75f, 1.0f, 0.95f, 0.35f);
            public string ColourPropertyName = "_TintColor";

            public bool AnimateShaderAlpha = false;
            public AnimationCurve AlphaCurve = null;

            public float Duration = 1.0f;
            public bool Persistent = false;
            public bool RemoveOnDeath = true;

            public bool DestroyComponentOnEnd = true;
        }

        internal sealed class Overlay
        {
            public readonly Material Material;
            public readonly bool AnimateShaderAlpha;
            public readonly AnimationCurve AlphaCurve;
            public readonly float Duration;
            public readonly bool Persistent;
            public readonly bool RemoveOnDeath;
            public readonly bool DestroyComponentOnEnd;

            public Overlay(
                Material _material,
                bool _animateShaderAlpha,
                AnimationCurve _alphaCurve,
                float _duration,
                bool _persistent,
                bool _removeOnDeath,
                bool _destroyComponentOnEnd)
            {
                Material = _material;
                AnimateShaderAlpha = _animateShaderAlpha;
                AlphaCurve = _alphaCurve;
                Duration = _duration;
                Persistent = _persistent;
                RemoveOnDeath = _removeOnDeath;
                DestroyComponentOnEnd = _destroyComponentOnEnd;
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

            // Use a default linear alpha curve if not provided
            AnimationCurve alphaCurve = _settings.AlphaCurve ?? AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

            // Create and return the overlay
            return new Overlay(
                overlayMaterial,
                _settings.AnimateShaderAlpha,
                alphaCurve,
                _settings.Duration,
                _settings.Persistent,
                _settings.RemoveOnDeath,
                _settings.DestroyComponentOnEnd);
        }

        internal static TemporaryOverlayInstance ApplyOverlay(Overlay _overlay, CharacterBody _body, float? _duration = null, bool? _persistent = null)
        {
            // Validate input
            if (_overlay == null || !_body) return null;

            // Do not apply death-cleared overlays to bodies that are already dead
            if (_overlay.RemoveOnDeath)
            {
                HealthComponent healthComponent = _body.healthComponent;
                if (healthComponent == null || !healthComponent.alive) return null;
            }

            // Attempt to fetch the character model for this body
            CharacterModel characterModel = Utils.GetCharacterModel(_body);
            if (!characterModel) return null;

            // Replace any existing copy of this same overlay on this body
            RemoveOverlay(_overlay, _body);

            // Add the overlay to the character model
            TemporaryOverlayInstance overlayInstance = TemporaryOverlayManager.AddOverlay(characterModel.gameObject);
            if (overlayInstance == null) return null;

            // Override overlay duration and persistence if specified
            bool persistent = _persistent ?? _overlay.Persistent;
            float duration = persistent ? PersistentDuration : Mathf.Max(0.0f, _duration ?? _overlay.Duration);

            // Configure the overlay instance
            overlayInstance.duration = duration;
            overlayInstance.animateShaderAlpha = _overlay.AnimateShaderAlpha;
            overlayInstance.alphaCurve = _overlay.AlphaCurve;
            overlayInstance.destroyComponentOnEnd = _overlay.DestroyComponentOnEnd;
            overlayInstance.originalMaterial = _overlay.Material;
            overlayInstance.AddToCharacterModel(characterModel);

            // Store the active overlay instance and the exact model it was attached to
            m_activeOverlays[(_body, _overlay)] = new ActiveOverlay
            {
                CharacterModel = characterModel,
                OverlayInstance = overlayInstance
            };

            return overlayInstance;
        }

        internal static bool RemoveOverlay(Overlay _overlay, CharacterBody _body)
        {
            // Validate input
            if (_overlay == null || !_body) return false;

            // Check if this overlay is currently active on this body
            (CharacterBody body, Overlay overlay) key = (_body, _overlay);
            if (!m_activeOverlays.TryGetValue(key, out ActiveOverlay activeOverlay)) return false;

            // Stop tracking it first
            m_activeOverlays.Remove(key);

            // Remove from the exact model this overlay was added to
            if (activeOverlay.CharacterModel != null && activeOverlay.CharacterModel.temporaryOverlays != null)
            {
                activeOverlay.CharacterModel.temporaryOverlays.Remove(activeOverlay.OverlayInstance);
            }

            // Tell the overlay instance to expire
            if (activeOverlay.OverlayInstance != null)
            {
                activeOverlay.OverlayInstance.destroyComponentOnEnd = true;
                activeOverlay.OverlayInstance.duration = 0.0f;
            }

            return true;
        }

        private static void OnCharacterDeathGlobal(DamageReport _report)
        {
            // Validate input
            if (_report == null) return;

            // Attempt to fetch the victim body
            CharacterBody victimBody = _report.victimBody;
            if (!victimBody) return;

            // Collect overlays on this body that should be removed on death
            m_overlaysToRemove.Clear();

            foreach (KeyValuePair<(CharacterBody body, Overlay overlay), ActiveOverlay> pair in m_activeOverlays)
            {
                if (pair.Key.body == victimBody && pair.Key.overlay.RemoveOnDeath)
                {
                    m_overlaysToRemove.Add(pair.Key.overlay);
                }
            }

            // Remove them after enumeration
            for (int i = 0; i < m_overlaysToRemove.Count; i++)
            {
                RemoveOverlay(m_overlaysToRemove[i], victimBody);
            }

            m_overlaysToRemove.Clear();
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

            // Attempt to load from legacy resources if not found in addressables
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