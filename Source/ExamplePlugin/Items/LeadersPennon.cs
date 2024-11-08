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
            CreateDisplaySettings("leaderspennondisplaymesh");

            // Create Leader's Pennon item and buff
            leadersPennonBuff = Buffs.AddBuff("LEADERS_PENNON", "texbuffleaderarea", Color.white, false);
            leadersPennonItem = Items.AddItem("LEADERS_PENNON", [ItemTag.Utility, ItemTag.AIBlacklist], "texleaderspennonicon", "leaderspennonmesh", ItemTier.VoidTier1, _corruptToken: "ITEM_WARDONLEVEL_NAME", _displaySettings: displaySettings);

            // Add ally to ally behaviour
            Behaviour.AddAllyToAllyCallback(leadersPennonItem, AllyWithItemToAlly);

            // Add stats modification
            Behaviour.AddStatsMod(leadersPennonBuff, LeadersPennonStatsMod);
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = Utils.CreateItemDisplaySettings(_displayMeshName);

            // Check for required asset
            if (!Assets.HasAsset(_displayMeshName))
            {
                return;
            }

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0.3775F, -0.21275F), new Vector3(1F, 180F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0F, 0.125F, -0.1275F), new Vector3(5F, 180F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Bandit", "MainWeapon", new Vector3(-0.06125F, 0.5415F, -0.0255F), new Vector3(1F, 172.75F, 0F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 4F, -1.725F), new Vector3(0F, 180F, 0F), new Vector3(1F, 0.8F, 1F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0.003F, 0.47825F, -0.2905F), new Vector3(0F, 180F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.1165F, 0.448F, -0.275F), new Vector3(0F, 180F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Mercenary", "UpperArmR", new Vector3(-0.16175F, 0.005F, -0.03925F), new Vector3(3.50988F, 257.5009F, 173.5443F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, 0.4F, -0.3725F), new Vector3(0F, 180F, 0F), new Vector3(0.12F, 0.12F, 0.12F));
            displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(0.00098F, 0.06F, -0.1515F), new Vector3(359F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, 0.94F, 1.51255F), new Vector3(348.9F, 0F, 180F), new Vector3(1.35F, 1.35F, 1.35F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0.32F, -0.235F), new Vector3(3.5F, 180F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.055F, -0.035F, -0.11F), new Vector3(0F, 178F, 14F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(-0.005F, 0.345F, -0.205F), new Vector3(5F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.153F, 0.05F, -0.39275F), new Vector3(0F, 180F, 315F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(-0.2405F, 0.4255F, 0.19575F), new Vector3(329.5F, 0.5F, 55.75F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(-0.3335F, -0.2505F, 0.0195F), new Vector3(90F, 270F, 0F), new Vector3(0.11F, 0.1F, 0.11F));
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
            float radius = 15.0f + (_count - 1) * 5.0f;     // REMEMBER TO SYNC THIS WITH FaithfulLeadersPennonBehaviour

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
                    Utils.RefreshTimedBuffs(body, leadersPennonBuff.buffDef, 1);
                }
            }
        }
    }
}
