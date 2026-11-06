using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal static class BuffOverlayVisuals
    {
        // Shared neutral copy of Soul Brand's original colour ramp
        private static Texture2D symbolRemap;

        internal sealed class BuffOverlayVisualSettings
        {
            public string EffectName = "faithfulBuffOverlayVisualEffect";
            public Sprite Sprite;
            public Color Colour = Color.white;
            public float ScaleMultiplier = 1.0f;
            public float SymbolScaleMultiplier = 1.0f;
            public bool ShowApplicationEffect = true;
            public bool ShowParticles = true;
            public bool UseOriginalRemap = true;
            public Color? SymbolColour;
            public string AttachmentPoint = "";
        }

        internal sealed class BuffOverlayVisual
        {
            // Temporary visual effect prefab
            private readonly GameObject prefab;
            private readonly float scaleMultiplier;
            private readonly string attachmentPoint;

            // Characters that currently have this visual
            private Dictionary<CharacterBody, GameObject> activeVisuals = new();

            public BuffOverlayVisual(GameObject _prefab, float _scaleMultiplier = 1.0f, string _attachmentPoint = "")
            {
                prefab = _prefab;
                scaleMultiplier = Mathf.Max(0.0f, _scaleMultiplier);
                attachmentPoint = _attachmentPoint;
            }

            public void ApplyToCharacter(CharacterBody _character)
            {
                // Only create visuals locally for living characters
                if (!NetworkClient.active || !_character || !prefab) return;
                if (!_character.healthComponent || !_character.healthComponent.alive || _character.hideExtraEffects) return;

                // Keep the existing visual if it is still active
                if (activeVisuals.TryGetValue(_character, out GameObject existingVisual) && existingVisual) return;

                // Create visual effect
                Transform parent = _character.coreTransform;
                if (!string.IsNullOrEmpty(attachmentPoint) && _character.modelLocator && _character.modelLocator.modelChildLocator)
                {
                    Transform child = _character.modelLocator.modelChildLocator.FindChild(attachmentPoint);
                    if (child) parent = child;
                }

                GameObject visual = Object.Instantiate(prefab, parent.position, Quaternion.identity);
                if (!visual.TryGetComponent(out TemporaryVisualEffect visualEffect))
                {
                    Log.Warning($"[BUFF VISUAL] - Prefab '{prefab.name}' is missing TemporaryVisualEffect.");
                    Object.Destroy(visual);
                    return;
                }

                // Follow the character using Soul Brand's sizing
                visualEffect.parentTransform = parent;
                visualEffect.healthComponent = _character.healthComponent;
                visualEffect.radius = _character.bestFitRadiusSqrt * scaleMultiplier;
                visualEffect.visualState = TemporaryVisualEffect.VisualState.Enter;

                // Assign the character for any local camera effect
                if (visual.TryGetComponent(out LocalCameraEffect cameraEffect))
                {
                    cameraEffect.targetCharacter = _character.gameObject;
                }

                // Store visual for this character
                activeVisuals[_character] = visual;

                // Forget effects that finish on their own without removing a newer instance
                OnDestroyCallback.AddCallback(visual, _ =>
                {
                    if (activeVisuals.TryGetValue(_character, out GameObject currentVisual) && ReferenceEquals(currentVisual, visual))
                    {
                        activeVisuals.Remove(_character);
                    }
                });
            }

            public void RemoveFromCharacter(CharacterBody _character)
            {
                // Allow cleanup for destroyed Unity objects, but not a missing reference
                if (ReferenceEquals(_character, null)) return;

                // Remove visual from character if it exists
                if (activeVisuals.TryGetValue(_character, out GameObject visual))
                {
                    activeVisuals.Remove(_character);

                    // Let the effect finish its exit animation before destroying itself
                    if (!visual) return;
                    if (visual.TryGetComponent(out TemporaryVisualEffect visualEffect))
                    {
                        visualEffect.visualState = TemporaryVisualEffect.VisualState.Exit;
                    }
                    else
                    {
                        Object.Destroy(visual);
                    }
                }
            }
        }

        internal static BuffOverlayVisual CreateBuffOverlayVisual(BuffOverlayVisualSettings _settings)
        {
            // Validate settings
            if (_settings == null)
            {
                Log.Warning("[BUFF VISUAL] - BuffOverlayVisualSettings is null while creating visual.");

                // Use default settings if none provided
                _settings = new();
            }

            // Clone the source prefab directly before a live instance can change its scale
            GameObject templateEffect = Assets.FetchAsset<GameObject>("RoR2/DLC4/CasuticPheromones/SoulBrandBuffEffect.prefab");
            GameObject visualEffectPrefab = templateEffect.InstantiateClone(_settings.EffectName, false);

            // Hide the application burst without affecting the persistent symbol
            Transform applicationEffect = visualEffectPrefab.transform.Find("Visual/Application VFX");
            if (applicationEffect && !_settings.ShowApplicationEffect) applicationEffect.gameObject.SetActive(false);

            // Neutral ramp for surrounding particles and optionally the symbol
            Texture neutralRamp = Assets.FetchAsset<Texture>("RoR2/Base/Common/ColorRamps/texRampTwotoneWhite.png");
            Dictionary<Material, Material> materials = new();
            foreach (ParticleSystemRenderer renderer in visualEffectPrefab.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                Material[] rendererMaterials = renderer.sharedMaterials;
                bool isSymbol = false;
                for (int i = 0; i < rendererMaterials.Length; i++)
                {
                    Material source = rendererMaterials[i];
                    bool isSymbolMaterial = source && source.name.StartsWith("matSoulBrandSigil");
                    isSymbol |= isSymbolMaterial;
                    if (!source || !source.HasProperty("_TintColor")) continue;

                    // Copy each material once so other visuals and the original effect remain unchanged
                    if (!materials.TryGetValue(source, out Material material))
                    {
                        material = new Material(source);
                        material.name = $"{source.name} (FaithfulBuffVisual)";
                        material.SetColor("_TintColor", isSymbolMaterial ? _settings.SymbolColour ?? _settings.Colour : _settings.Colour);

                        // Keep particle alpha while removing the template's particle RGB tint
                        material.DisableKeyword("VERTEXCOLOR");
                        if (material.HasProperty("_VertexColorOn")) material.SetFloat("_VertexColorOn", 0.0f);

                        // Preserve the original symbol ramp's brightness and alpha without its red hue
                        if (material.HasProperty("_RemapTex"))
                        {
                            Texture remap = neutralRamp;
                            if (isSymbolMaterial && _settings.UseOriginalRemap && source.GetTexture("_RemapTex"))
                            {
                                if (!symbolRemap) symbolRemap = Utils.CreateGrayscaleTexture(source.GetTexture("_RemapTex"), "texFaithfulNeutralSoulBrandRamp");
                                remap = symbolRemap;
                            }
                            material.SetTexture("_RemapTex", remap);
                        }

                        // Replace both Soul Brand symbols only when a sprite was supplied
                        if (_settings.Sprite && isSymbolMaterial)
                        {
                            Sprite sprite = _settings.Sprite;
                            Rect rect = sprite.textureRect;
                            material.mainTexture = sprite.texture;
                            material.mainTextureScale = new Vector2(rect.width / sprite.texture.width, rect.height / sprite.texture.height);
                            material.mainTextureOffset = new Vector2(rect.x / sprite.texture.width, rect.y / sprite.texture.height);
                        }

                        materials.Add(source, material);
                    }

                    rendererMaterials[i] = material;
                }

                renderer.sharedMaterials = rendererMaterials;

                // Hide surrounding particles without disabling parents of the symbol
                if (!isSymbol && !_settings.ShowParticles) renderer.enabled = false;

                // Resize symbol particles without scaling their child effects
                if (isSymbol && renderer.TryGetComponent(out ParticleSystem particles))
                {
                    float symbolScale = Mathf.Max(0.0f, _settings.SymbolScaleMultiplier);
                    ParticleSystem.MainModule main = particles.main;
                    if (main.startSize3D)
                    {
                        main.startSizeXMultiplier *= symbolScale;
                        main.startSizeYMultiplier *= symbolScale;
                        main.startSizeZMultiplier *= symbolScale;
                    }
                    else
                    {
                        main.startSizeMultiplier *= symbolScale;
                    }
                }
            }

            // Match any lights to the visual colour
            foreach (Light light in visualEffectPrefab.GetComponentsInChildren<Light>(true))
            {
                light.color = _settings.Colour;
                if (!_settings.ShowParticles) light.enabled = false;
            }

            return new BuffOverlayVisual(visualEffectPrefab, _settings.ScaleMultiplier, _settings.AttachmentPoint);
        }
    }
}
