using RoR2;
using R2API;
using UnityEngine;
using RoR2.ExpansionManagement;
using IL.RoR2.Items;
using HarmonyLib;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Faithful
{
    internal class Item
    {
        // Toolbox
        protected Toolbox toolbox;

        // Item def
        public ItemDef itemDef;

        // Item token
        public string token;

        // Corrupted item token
        private string corruptToken = null;

        // Constructor
        public Item(Toolbox _toolbox, string _token, ItemTag[] _tags, string _iconName, string _modelName, ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null)
        {
            toolbox = _toolbox;

            // Assign token
            token = _token;

            // Store corrupted item token
            corruptToken = _corruptToken;

            // Create item def
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            // Set item texts
            itemDef.name = $"FAITHFUL_{_token}_NAME";
            itemDef.nameToken = $"FAITHFUL_{_token}_NAME";
            itemDef.pickupToken = $"FAITHFUL_{_token}_PICKUP";
            itemDef.descriptionToken = $"FAITHFUL_{_token}_DESC";
            itemDef.loreToken = $"FAITHFUL_{_token}_LORE";

            // Set item tags
            itemDef.tags = _tags;

            // Set item tier
            itemDef.deprecatedTier = _tier;

            // Banned from Simulacrum?
            if (_simulacrumBanned)
            {
                // Ban from Simulacrum
                toolbox.utils.BanFromSimulacrum(itemDef);
            }

            // Set can remove (Can a shrine of chance or printer etc. take this item)
            itemDef.canRemove = _canRemove;

            // Is item hidden
            itemDef.hidden = _hidden;

            // Set icon and model
            itemDef.pickupIconSprite = toolbox.assets.GetIcon(_iconName);
            itemDef.pickupModelPrefab = toolbox.assets.GetModel(_modelName);

            // Add item to R2API
            // You can add your own display rules here,
            // where the first argument passed are the default display rules:
            // the ones used when no specific display rules for a character are found.
            // For this example, we are omitting them,
            // as they are quite a pain to set up without tools like https://thunderstore.io/package/KingEnderBrine/ItemDisplayPlacementHelper/
            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Log.Debug($"Created item '{_token}'");
        }

        public void AddCorruption()
        {
            // Corrupts item?
            if (corruptToken != null)
            {
                // Get item to corrupt
                ItemDef itemToCorrupt = ItemCatalog.itemDefs.Where(x => x.nameToken == corruptToken).FirstOrDefault();

                // Found item?
                if (!itemToCorrupt)
                {
                    Log.Error($"Failed to add '{corruptToken}' as corrupted by '{token}', unable to find '{corruptToken}'");
                }
                else
                {
                    // Create temp pair array
                    var pair = new ItemDef.Pair[]
                    {
                        new ItemDef.Pair
                        {
                            itemDef1 = itemToCorrupt,
                            itemDef2 = itemDef,
                        }
                    };

                    // Append pair array to contagious items pair array
                    ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddRangeToArray(pair);
                }
            }
        }
    }
}
