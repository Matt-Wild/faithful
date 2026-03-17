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
                _description: "Killing an enemy releases a <style=cIsDamage>swarm</style> of <style=cIsDamage>6</style> bees that attack surrounding enemies. Each bee deals <style=cIsDamage>25%</style> base damage up to <style=cIsDamage>3</style> (+3 per stack) times.", 
                _lore: "<style=cMono>\r//--AUTO-TRANSCRIPTION FROM UES [Redacted] --//</style>\r\n\r\n\u201CYou signed off on live storage for that thing?\u201D\n\n\u201CI signed off on a sealed specimen cluster. Emphasis on sealed.\u201D\n\n\u201CThen why is the cargo bay humming?\u201D\n\n\u201CHumming?\u201D\n\n\u201CYes. Humming. Loudly. And, unless my eyes are failing me, leaking.\u201D\n\n\u201CThat\u2019s not possible. It only responds to kinetic agitation.\u201D\n\n\u201C...Kinetic agitation such as firing it repeatedly into a crowd?\u201D\n\n\u201CIn hindsight, perhaps the briefing could have been more specific.\u201D\n\n\u201CI don\u2019t need hindsight, doctor, I need those things out of the ventilation.\u201D\n\n\u201CWell don\u2019t shout. They become excited by raised voices.\u201D\n\n\u201CThey become excited by everything! Gunfire, footsteps, light, shadows, me thinking too hard in their general direction—\u201D\n\n\u201CRemarkable, aren\u2019t they?\u201D\n\n\u201CThere are thousands of them.\u201D\n\n\u201CYes. More than we anticipated.\u201D\n\n\u201CYou told me it would release a defensive swarm.\u201D\n\n\u201CI did.\u201D\n\n\u201CThis is not a swarm. This is a medical emergency with wings.\u201D\n\n\u201CThat seems a touch dramatic.\u201D\n\n\u201CThey have filled my locker, doctor.\u201D\n\n\u201CThen I would advise against opening it.\u201D", 
                _tier: ItemTier.Tier1,
                _tags: [ItemTag.Damage, ItemTag.OnKillEffect],
                _iconDir: "texhexclustericon",
                _modelDir: "hexclustermesh",
                _displayDir: "hexclusterdisplaymesh",
                _displaySettingsCallback: _displaySettings =>
                {
                    // Add character display settings
                    _displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Bandit", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F), "EngiTurretBody");
                    _displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F), "EngiWalkerTurretBody");
                    _displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("REX", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Loader", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Railgunner", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Scavenger", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Technician", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Operator", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                });
        }

        public static void CreateItem(bool _createDisplaySettings = true, string _name = "WIP ITEM", ItemTag[] _tags = default, 
            string _iconDir = "texTemporalCubeIcon", string _modelDir = "temporalcubemesh", string _displayDir = "temporalcubemesh", 
            ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null,
            ItemDisplayCallback _displaySettingsCallback = null, ModifyPrefabCallback _modifyItemModelPrefabCallback = null,
            ModifyPrefabCallback _modifyItemDisplayPrefabCallback = null, bool _canNeverBeTemporary = false, bool _debugOnly = false,
            string _pickup = null, string _description = null, string _lore = null)
        {
            // Don't create item is WIP items are disabled
            if (!Utils.debugWIPItems) return;

            // Create WIP item and add to list
            WIPItem wipItem = new(_createDisplaySettings, _name, _tags, _iconDir, _modelDir, _displayDir, _tier, _simulacrumBanned, _canRemove, _hidden,
                _corruptToken, _displaySettingsCallback, _modifyItemModelPrefabCallback, _modifyItemDisplayPrefabCallback, _canNeverBeTemporary,
                _debugOnly, _pickup, _description, _lore);
            wipItems.Add(wipItem);
        }
    }
}
