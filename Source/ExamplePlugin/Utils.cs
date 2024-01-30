using BepInEx;
using HarmonyLib;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Faithful
{
    internal class Utils
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store plugin info
        public PluginInfo pluginInfo;

        // Store debug mode
        private const bool _debugMode = true;

        // Simulacrum banned items
        List<ItemDef> simulacrumBanned = new List<ItemDef>();

        // Corruption item pairs
        List<CorruptPair> corruptionPairs = new List<CorruptPair>();

        // Constructor
        public Utils(Toolbox _toolbox, PluginInfo _pluginInfo)
        {
            toolbox = _toolbox;

            // Set plugin info
            pluginInfo = _pluginInfo;

            // Config Simulacrum
            On.RoR2.InfiniteTowerRun.OverrideRuleChoices += InjectSimulacrumBannedItems;

            // Config item corruptions
            On.RoR2.Items.ContagiousItemManager.Init += SetupItemCorruptions;

            Log.Debug("Utils initialised");
        }

        // Refresh chosen buff on chosen character
        public void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float duration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0)
            {
                return; // Body not valid
            }

            // Cycle through buffs
            foreach (CharacterBody.TimedBuff buff in body.timedBuffs)
            {
                // Check if correct buff
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    // Refresh buff
                    buff.timer = duration;
                }
            }
        }

        public void BanFromSimulacrum(ItemDef _item)
        {
            // Add item def to banned list for Simulacrum
            simulacrumBanned.Add(_item);
        }

        public void AddCorruptionPair(ItemDef _corrupter, string _corruptedToken)
        {
            // Add item pair to corruption pairs
            corruptionPairs.Add(new CorruptPair(_corrupter, _corruptedToken));
        }

        void InjectSimulacrumBannedItems(On.RoR2.InfiniteTowerRun.orig_OverrideRuleChoices orig, InfiniteTowerRun self, RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, ulong runSeed)
        {
            // List of items needing to be banned
            List<ItemDef> newBanned = [];

            // Cycle through current banned items
            foreach (ItemDef target in simulacrumBanned)
            {
                // Contains item?
                if (!self.blacklistedItems.Contains(target))
                {
                    Log.Debug($"Banning {target.nameToken} from Simulacrum");
                    // Add to needed list for banning
                    newBanned.Add(target);
                }
            }

            // Needs to ban extra items
            if (newBanned.Count > 0)
            {
                // Add existing banned items to 
                foreach (ItemDef current in self.blacklistedItems)
                {
                    // Update new banned list
                    newBanned.Add(current);
                }

                // Update blacklisted items
                self.blacklistedItems = newBanned.ToArray();
            }

            orig(self, mustInclude, mustExclude, runSeed);  // Run normal processes
        }

        private void SetupItemCorruptions(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            // Create item pair list
            List<ItemDef.Pair> itemPairs = new List<ItemDef.Pair>();

            // Cycle through corruption pairs
            foreach (CorruptPair pair in corruptionPairs)
            {
                // Add to item pairs
                itemPairs.Add(pair.GetPair());
            }

            // Append corruption pairs to contagious items pair array
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddRangeToArray(itemPairs.ToArray());

            Log.Debug("Added item corruptions");

            orig(); // Run normal processes
        }

        // Return Hurt Boxes from RoR2 Sphere Search
        public HurtBox[] GetHurtBoxesInSphere(Vector3 position, float radius)
        {
            if (radius <= 0)
            {
                return []; // Can't check 0 or negative radius
            }

            // Get hurt boxes in sphere search
            RoR2.HurtBox[] hurtBoxes = new SphereSearch
            {
                radius = radius,
                mask = LayerIndex.entityPrecise.mask,
                origin = position
            }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            return hurtBoxes;   // Return found hurt boxes
        }

        public bool debugMode
        {
            get { return _debugMode; }
        }
    }

    internal struct CorruptPair
    {
        // Store corrupter and corrupted
        private ItemDef corrupter;
        private string corruptedToken;

        public CorruptPair(ItemDef _corrupter, string _corruptedToken)
        {
            // Assign corrupter and corrupted
            corrupter = _corrupter;
            corruptedToken = _corruptedToken;
        }

        public ItemDef.Pair GetPair()
        {
            // Copy corrupted token to local
            string localCorruptedToken = corruptedToken;

            // Get item to corrupt
            ItemDef corrupted = ItemCatalog.itemDefs.Where(x => x.nameToken == localCorruptedToken).FirstOrDefault();

            // Found item?
            if (!corrupted)
            {
                Log.Error($"Failed to add '{localCorruptedToken}' as corrupted by '{corrupter.nameToken}', unable to find '{localCorruptedToken}'");

                // Return empty item pair
                return new ItemDef.Pair();
            }

            // Return setup item pair
            return new ItemDef.Pair { itemDef1 = corrupted, itemDef2 = corrupter };
        }
    }
}
