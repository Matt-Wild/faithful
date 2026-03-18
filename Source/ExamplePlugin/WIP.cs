using RoR2;
using UnityEngine;
using System.Collections.Generic;

namespace Faithful
{
    internal static class WIP
    {
        // List of WIP items
        private static List<WIPItem> wipItems = [];

        public static void Init()
        {
            // Create WIP items to test models etc
            CreateItem(_name: "Hexagonal Cluster",
                _pickup: "Killing an enemy releases a swarm of bees.",
                _description: "Killing an enemy releases a <style=cIsDamage>swarm</style> of <style=cIsDamage>6</style> bees that attack surrounding enemies. Each bee deals <style=cIsDamage>25%</style> base damage up to <style=cIsDamage>3</style> <style=cStack>(+3 per stack)</style> times.", 
                _lore: "<style=cMono>\r//--AUTO-TRANSCRIPTION FROM UES [Redacted] --//</style>\r\n\r\n\u201CYou signed off on live storage for that thing?\u201D\n\n\u201CI signed off on a sealed specimen cluster. Emphasis on sealed.\u201D\n\n\u201CThen why is the cargo bay humming?\u201D\n\n\u201CHumming?\u201D\n\n\u201CYes. Humming. Loudly. And, unless my eyes are failing me, leaking.\u201D\n\n\u201CThat\u2019s not possible. It only responds to kinetic agitation.\u201D\n\n\u201C...Kinetic agitation such as firing it repeatedly into a crowd?\u201D\n\n\u201CIn hindsight, perhaps the briefing could have been more specific.\u201D\n\n\u201CI don\u2019t need hindsight, doctor, I need those things out of the ventilation.\u201D\n\n\u201CWell don\u2019t shout. They become excited by raised voices.\u201D\n\n\u201CThey become excited by everything! Gunfire, footsteps, light, shadows, me thinking too hard in their general direction—\u201D\n\n\u201CRemarkable, aren\u2019t they?\u201D\n\n\u201CThere are thousands of them.\u201D\n\n\u201CYes. More than we anticipated.\u201D\n\n\u201CYou told me it would release a defensive swarm.\u201D\n\n\u201CI did.\u201D\n\n\u201CThis is not a swarm. This is a medical emergency with wings.\u201D\n\n\u201CThat seems a touch dramatic.\u201D\n\n\u201CThey have filled my locker, doctor.\u201D\n\n\u201CThen I would advise against opening it.\u201D", 
                _tier: ItemTier.Tier1,
                _tags: [ItemTag.Damage, ItemTag.OnKillEffect],
                _iconDir: "texhexclustericon",
                _modelDir: "hexclustermesh",
                _displayDir: "hexclusterdisplaymesh",
                _displaySettingsCallback: _displaySettings =>
                {
                    // Add character display settings
                    _displaySettings.AddCharacterDisplay("Commando", "ThighL", new Vector3(0.1125F, 0.1125F, 0.04F), new Vector3(270F, 257.5F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("Huntress", "Pelvis", new Vector3(0.1075F, 0.015F, 0.13F), new Vector3(80F, 210F, 180F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Bandit", "CalfR", new Vector3(0.1025F, 0.066F, 0.03F), new Vector3(72.5F, 55F, 336.75F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "CalfL", new Vector3(0F, 0.905F, 0.815F), new Vector3(87.50002F, 180F, 180F), new Vector3(1.25F, 1.25F, 1.25F));
                    _displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0.005F, 0.4575F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
                    _displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(-0.005F, 0.4575F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
                    _displaySettings.AddCharacterDisplay("Turret", "LegBar3", new Vector3(0F, 0.725F, 0.2F), new Vector3(0F, 270F, 255F), new Vector3(0.425F, 0.425F, 0.425F));
                    _displaySettings.AddCharacterDisplay("Walker Turret", "LegBar3", new Vector3(0F, 0.725F, 0.2F), new Vector3(0F, 270F, 255F), new Vector3(0.425F, 0.425F, 0.425F));
                    _displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.115F, 0.3325F, -0.1775F), new Vector3(0F, 87.5F, 7.5F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Mercenary", "ThighL", new Vector3(0.0675F, 0.22F, -0.111F), new Vector3(277.5F, 150F, 180F), new Vector3(0.15F, 0.175F, 0.15F));
                    _displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(-0.3275F, 0.8275F, 0.0315F), new Vector3(0F, 20F, 52.5F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("Loader", "MechHandR", new Vector3(0F, 0.24F, 0F), new Vector3(0F, 175F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Acrid", "Jaw", new Vector3(0F, 2F, 0.2F), new Vector3(85F, 0F, 0F), new Vector3(1.25F, 1.25F, 1.25F));
                    _displaySettings.AddCharacterDisplay("Captain", "ThighL", new Vector3(0.0225F, 0.305F, -0.11F), new Vector3(357.5F, 77.75F, 268.75F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Railgunner", "Battery1R", new Vector3(0F, 0.225F, 0F), new Vector3(0F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Void Fiend", "UpperArmR", new Vector3(0.1825F, -0.06F, 0.1575F), new Vector3(77.5F, 307.5F, 266.25F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.268F, 0.179F, -0.226F), new Vector3(357F, 357F, 46.75F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("False Son", "UpperArmL", new Vector3(-0.123F, 0.257F, -0.0675F), new Vector3(277.5F, 65.75F, 349.5F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("Chef", "Base", new Vector3(0.0236F, -0.036F, -0.511F), new Vector3(314F, 234.25F, 177.25F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Operator", "ClawSpin", new Vector3(0F, 0F, 0.032F), new Vector3(0F, 270F, 270F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Drifter", "BagBottom", new Vector3(-0.19F, 0.128F, 0F), new Vector3(0F, 2.5F, 70F), new Vector3(0.25F, 0.25F, 0.25F));
                    _displaySettings.AddCharacterDisplay("Scavenger", "Backpack", new Vector3(-7.855F, 5.63F, -1.275F), new Vector3(0F, 340F, 84F), new Vector3(2F, 2F, 2F));
                    _displaySettings.AddCharacterDisplay("Technician", "Shin.L", new Vector3(-0.09F, 0.025F, 0F), new Vector3(0F, 180F, 270F), new Vector3(0.1125F, 0.1125F, 0.1125F));
                });
        }

        public static void CreateItem(bool _createDisplaySettings = true, string _name = "WIP ITEM", ItemTag[] _tags = default, 
            string _iconDir = "texTemporalCubeIcon", string _modelDir = "temporalcubemesh", string _displayDir = "temporalcubemesh", 
            ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null,
            ItemDisplayCallback _displaySettingsCallback = null, ModifyPrefabCallback _modifyItemModelPrefabCallback = null,
            ModifyPrefabCallback _modifyItemDisplayPrefabCallback = null, bool _canNeverBeTemporary = false, bool _debugOnly = false,
            string _pickup = null, string _description = null, string _lore = null)
        {
            // Don't create item is WIP content is disabled
            if (!Utils.debugWIPContent) return;

            // Create WIP item and add to list
            WIPItem wipItem = new(_createDisplaySettings, _name, _tags, _iconDir, _modelDir, _displayDir, _tier, _simulacrumBanned, _canRemove, _hidden,
                _corruptToken, _displaySettingsCallback, _modifyItemModelPrefabCallback, _modifyItemDisplayPrefabCallback, _canNeverBeTemporary,
                _debugOnly, _pickup, _description, _lore);
            wipItems.Add(wipItem);
        }
    }
}
