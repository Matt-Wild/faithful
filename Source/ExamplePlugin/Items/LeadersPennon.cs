using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class LeadersPennon
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item and buff
        Buff leadersPennonBuff;
        Item leadersPennonItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public LeadersPennon(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            //CreateDisplaySettings();

            // Create Leader's Pennon item and buff
            leadersPennonBuff = toolbox.buffs.AddBuff("LEADERS_PENNON", "texbufftemporalcube", Color.white, false);
            leadersPennonItem = toolbox.items.AddItem("LEADERS_PENNON", [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.CannotCopy], "textemporalcubeicon", "temporalcubemesh", ItemTier.VoidTier1, _corruptToken: "ITEM_WARDONLEVEL_NAME");
            //_displaySettings: displaySettings

            // Add ally to ally behaviour
            toolbox.behaviour.AddAllyToAllyCallback(leadersPennonItem, AllyWithItemToAlly);

            // Add stats modification
            toolbox.behaviour.AddStatsMod(leadersPennonBuff, LeadersPennonStatsMod);
        }

        private void CreateDisplaySettings()
        {
            // Create display settings
            displaySettings = toolbox.utils.CreateItemDisplaySettings("meltingwarblerdisplaymesh");

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "Head", new Vector3(0F, 0.46155F, 0.00827F), new Vector3(15F, 0F, 0F), new Vector3(0.13F, 0.13F, 0.13F));
            displaySettings.AddCharacterDisplay("Huntress", "Head", new Vector3(0F, 0.3575F, -0.025F), new Vector3(10F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Bandit", "Head", new Vector3(0F, 0.275F, 0.02F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(0F, 3F, 2.15F), new Vector3(305F, 180F, 0F), new Vector3(0.6F, 0.6F, 0.6F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.7715F, 0.0975F), new Vector3(15F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Artificer", "Head", new Vector3(0F, 0.25F, -0.0375F), new Vector3(25F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(0F, 0.325F, 0.07F), new Vector3(12.5F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "HandL", new Vector3(0F, 0.3F, 0.125F), new Vector3(280F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Loader", "Head", new Vector3(0F, 0.3F, 0.0375F), new Vector3(5F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, 1.4325F, 2.15F), new Vector3(280F, 180F, 0F), new Vector3(1.35F, 1.35F, 1.35F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0.3415F, 0.5145F, -0.045F), new Vector3(5F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunStock", new Vector3(-0.001F, -0.015F, 0.09F), new Vector3(85.00005F, 180F, 180F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Head", new Vector3(0F, 0.25F, -0.065F), new Vector3(345F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Scavenger", "Chest", new Vector3(0F, 7.25F, 1.4F), new Vector3(350F, 180F, 0F), new Vector3(2.4F, 2.4F, 2.4F));
        }

        void LeadersPennonStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += 0.3f;

            // Modify regen speed
            _stats.regenMultAdd += 0.3f;
        }

        void AllyWithItemToAlly(int _count, CharacterMaster _holder, CharacterMaster _other)
        {
            // Calculate effect radius
            float radius = 15.0f + (_count - 1) * 5.0f;

            // Other ally in radius
            if ((_holder.GetBodyObject().transform.position - _other.GetBodyObject().transform.position).magnitude <= radius)
            {
                // Get body of other
                CharacterBody body = _other.GetBody();

                // If other ally doesn't have buff already
                if (body.GetBuffCount(leadersPennonBuff.buffDef) == 0)
                {
                    // Grant buff
                    body.AddTimedBuff(leadersPennonBuff.buffDef, 1);
                }
                else
                {
                    // Refresh Leader's Pennon buffs on other ally
                    toolbox.utils.RefreshTimedBuffs(body, leadersPennonBuff.buffDef, 1);
                }
            }
        }
    }
}
