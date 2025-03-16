using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal static class Skins
    {
        internal static SkinDef CreateSkinDef(string _skinToken, Sprite _skinIcon, Survivor _survivor, UnlockableDef _unlockableDef = null, SkinReplacement[] _skinReplacements = null)
        {
            // Get default renderer infos
            CharacterModel.RendererInfo[] defaultRendererInfos = _survivor.characterModel.baseRendererInfos;

            // Get child locator
            ChildLocator childLocator = _survivor.characterModel.GetComponent<ChildLocator>();

            // Check for child locator
            if (childLocator == null)
            {
                // Error and return - unsuccessful
                Log.Error($"[SKINS] | Was unable to create skin definition for skin '{Utils.GetLanguageString(_skinToken)}' - Child locator not found on survivor!");
                return null;
            }

            // Get survivor root
            GameObject root = _survivor.characterModel.gameObject;

            // Create skin renderer infos
            CharacterModel.RendererInfo[] skinRendererInfos = new CharacterModel.RendererInfo[defaultRendererInfos.Length];
            defaultRendererInfos.CopyTo(skinRendererInfos, 0);

            // Don't know why but Henry mod does this lol
            On.RoR2.SkinDef.Awake += DoNothing;

            // Create skin def and assign properties
            SkinDef skinDef = ScriptableObject.CreateInstance<SkinDef>();
            skinDef.baseSkins = Array.Empty<SkinDef>();
            skinDef.icon = _skinIcon;
            skinDef.unlockableDef = _unlockableDef;
            skinDef.rootObject = root;
            skinDef.rendererInfos = skinRendererInfos;
            skinDef.gameObjectActivations = new SkinDef.GameObjectActivation[0];
            skinDef.projectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDef.minionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDef.nameToken = _skinToken;
            skinDef.name = Utils.GetLanguageString(_skinToken);

            // Initialise mesh replacements list
            List<SkinDef.MeshReplacement> meshReplacements = new List<SkinDef.MeshReplacement>();

            // Index used to find renderer infos
            int rendererInfosIndex = 0;

            // Cycle through children in child locator - DO NOT MODIFY UNLESS ALSO MODIFYING SURVIVOR SCRIPT - ORDER OF RENDERER INFO CREATION IS VITAL
            for (int i = 0; i < childLocator.Count; i++)
            {
                // Get child game object
                GameObject child = childLocator.FindChildGameObject(i);

                // Check for child
                if (child == null) continue;

                // Get child name
                string childName = childLocator.FindChildName(i);

                // Get renderer from child
                Renderer renderer = child.GetComponent<Renderer>();

                // Check for renderer
                if (renderer == null) continue;

                // Initialise replacement mesh
                Mesh replacementMesh = renderer.gameObject.GetComponent<MeshFilter>()?.sharedMesh ?? renderer.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;

                // Check for skin replacements
                if (_skinReplacements != null)
                {
                    // Cycle through skin replacements
                    foreach (SkinReplacement skinReplacement in _skinReplacements)
                    {
                        // Check if child names are matching
                        if (skinReplacement.childName == childName)
                        {
                            // Check for replacement mesh
                            if (skinReplacement.replacementMesh != null)
                            {
                                // Override replacement mesh
                                replacementMesh = Assets.GetMesh(skinReplacement.replacementMesh);
                            }
                            
                            // Check for replacement material
                            if (skinReplacement.replacementMaterial != null)
                            {
                                // Override material for corresponding render info (convert to HG shader)
                                skinDef.rendererInfos[rendererInfosIndex].defaultMaterial = Assets.GetMaterial(skinReplacement.replacementMaterial).ConvertDefaultShaderToHopoo();
                            }
                        }
                    }
                }

                // Add default mesh to mesh replacements list
                meshReplacements.Add(
                    new SkinDef.MeshReplacement
                    {
                        renderer = defaultRendererInfos[rendererInfosIndex].renderer,
                        mesh = replacementMesh
                    }
                );

                // Increment render infos index
                rendererInfosIndex++;
            }

            // Assign mesh replacements
            skinDef.meshReplacements = [.. meshReplacements];

            // Unsubscribe the spooky nothing method
            On.RoR2.SkinDef.Awake -= DoNothing;

            // Return skin def
            return skinDef;
        }

        // It resides here
        private static void DoNothing(On.RoR2.SkinDef.orig_Awake orig, RoR2.SkinDef self)
        {
        }

        // Stores all the information to create mesh replacement information and stores corresponding material replacements
        internal struct SkinReplacement
        {
            internal string childName;
            internal string replacementMesh;
            internal string replacementMaterial;
        }
    }
}
