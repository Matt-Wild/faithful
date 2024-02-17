using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.TextCore;
using static RoR2.UI.HGHeaderNavigationController;

namespace Faithful
{
    internal class NoxiousSlime
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item
        Item noxiousSlimeItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public NoxiousSlime(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("noxiousslimedisplaymesh");

            // Create Noxious Slime item
            noxiousSlimeItem = toolbox.items.AddItem("NOXIOUS_SLIME", [ItemTag.Damage], "texnoxiousslimeicon", "noxiousslimemesh", ItemTier.Tier3, _displaySettings: displaySettings);

            // Inject buff, timed buff and DoT behaviour
            toolbox.behaviour.AddOnAddBuffCallback(OnAddBuff);
            toolbox.behaviour.AddOnAddTimedBuffCallback(OnAddTimedBuff);
            toolbox.behaviour.AddOnInflictDamageOverTimeRefCallback(OnInflictDamageOverTimeRef);
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = toolbox.utils.CreateItemDisplaySettings(_displayMeshName);

            // Check for required asset
            if (!toolbox.assets.HasAsset(_displayMeshName))
            {
                return;
            }

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "HandL", new Vector3(0.00635F, 0.0875F, 0.0875F), new Vector3(355F, 15F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Huntress", "HandL", new Vector3(-0.005F, 0.079F, 0.065F), new Vector3(5F, 355F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Bandit", "MainWeapon", new Vector3(-0.05F, 0.25F, -0.0425F), new Vector3(0F, 180F, 180F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("MUL-T", "LowerArmL", new Vector3(0.135F, 3.2F, 1.5F), new Vector3(0F, 0F, 55F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Engineer", "HandL", new Vector3(0.005F, 0.11F, 0.09F), new Vector3(0F, 345F, 0F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Artificer", "LowerArmL", new Vector3(0F, 0.1775F, -0.1325F), new Vector3(25F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "HandL", new Vector3(-0.005F, 0.124F, 0.097F), new Vector3(12.5F, 345F, 10F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, -0.087F, 0.559F), new Vector3(10F, 0F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Loader", "MechHandL", new Vector3(-0.075F, 0.15F, 0.18F), new Vector3(5F, 325F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acrid", "HandL", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 120F), new Vector3(0.8F, 0.8F, 0.8F));
            //displaySettings.AddCharacterDisplay("Acrid", "HandR", new Vector3(0F, 0F, 0F), new Vector3(45F, 0F, 0F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Captain", "HandL", new Vector3(0F, 0.125F, -0.064F), new Vector3(0F, 95F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Railgunner", "TopRail", new Vector3(0F, 0.6525F, 0.0585F), new Vector3(0F, 0F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ElbowR", new Vector3(0.01F, -0.0125F, 0.005F), new Vector3(345F, 90F, 60F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Scavenger", "Weapon", new Vector3(0F, 18.25F, 0F), new Vector3(280F, 330F, 90F), new Vector3(2.5F, 2.5F, 2.5F));
        }

        void OnAddBuff(BuffIndex _buff, CharacterBody _character)
        {
            // Check for host, Character Body and Buff Type
            if (!toolbox.utils.hosting || _character == null || _buff == BuffIndex.None)
            {
                return;
            }

            // Try to get last attacker
            CharacterMaster attacker = toolbox.utils.GetLastAttacker(_character);
            if (attacker == null || !attacker.hasBody)
            {
                return;
            }

            // Get attacker body
            CharacterBody attackerBody = attacker.GetBody();

            // Check for attacker inventory
            if (!attackerBody.inventory)
            {
                return;
            }

            // Get item count
            int count = attackerBody.inventory.GetItemCount(noxiousSlimeItem.itemDef);

            // Has item?
            if (count > 0)
            {
                // Try to get Faithful behaviour
                FaithfulCharacterBodyBehaviour helper = _character.gameObject.GetComponent<FaithfulCharacterBodyBehaviour>();
                if (helper == null)
                {
                    return;
                }

                // Try get Buff Def and ensure not timed debuff using flag system
                BuffDef buffDef = BuffCatalog.GetBuffDef(_buff);
                if (buffDef != null && !helper.flags.Get($"NS_{buffDef.name}_TDA"))
                {
                    // Check if Debuff
                    if (!buffDef.isDebuff)
                    {
                        return;
                    }

                    // Apply more debuffs
                    _character.SetBuffCount(_buff, _character.buffs[(int)_buff] + count);
                }
            }
        }

        void OnAddTimedBuff(BuffDef _buff, float _duration, CharacterBody _character)
        {
            // Check for host, Character Body, Buff and if Debuff
            if (!toolbox.utils.hosting || _character == null || _buff == null || !_buff.isDebuff)
            {
                return;
            }

            // Try to get Faithful behaviour
            FaithfulCharacterBodyBehaviour helper = _character.gameObject.GetComponent<FaithfulCharacterBodyBehaviour>();
            if (helper == null)
            {
                return;
            }

            // Check for flag
            if (helper.flags.Get($"NS_{_buff.name}_TDA"))
            {
                return;
            }

            // Try to get last attacker
            CharacterMaster attacker = toolbox.utils.GetLastAttacker(_character);
            if (attacker == null || !attacker.hasBody)
            {
                return;
            }

            // Get attacker body
            CharacterBody attackerBody = attacker.GetBody();

            // Check for attacker inventory
            if (!attackerBody.inventory)
            {
                return;
            }

            // Set flag to avoid infinite recursion
            helper.flags.Set($"NS_{_buff.name}_TDA", true);

            // Get item count
            int count = attackerBody.inventory.GetItemCount(noxiousSlimeItem.itemDef);

            // Add extra debuffs
            while (count > 0)
            {
                // Apply extra debuff
                toolbox.utils.AddPulverizeBuildup(_character, attackerBody, _duration);

                // Decrease count
                count--;
            }
        }

        void OnInflictDamageOverTimeRef(ref InflictDotInfo _inflictDotInfo)
        {
            // Check if hosting
            if (!toolbox.utils.hosting)
            {
                return;
            }

            // Check for victim and attacker
            if (_inflictDotInfo.victimObject == null || _inflictDotInfo.attackerObject == null)
            {
                return;
            }

            // Try to get Faithful behaviour
            FaithfulCharacterBodyBehaviour helper = _inflictDotInfo.victimObject.GetComponent<FaithfulCharacterBodyBehaviour>();
            if (helper == null)
            {
                return;
            }

            // Check for flag
            if (helper.flags.Get($"NS_{_inflictDotInfo.dotIndex}_IEDOT"))
            {
                return;
            }

            // Attempt to get attacker body
            CharacterBody attackerBody = _inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
            if (attackerBody == null)
            {
                return;
            }

            // Set flag to avoid infinite recursion
            helper.flags.Set($"NS_{_inflictDotInfo.dotIndex}_IEDOT", true);

            // Get item count
            int count = attackerBody.inventory.GetItemCount(noxiousSlimeItem.itemDef);

            // Inflict extra DoT
            while (count > 0)
            {
                // Inflict copied DoT
                InflictDotInfo inflictDotInfo = new InflictDotInfo
                {
                    victimObject = _inflictDotInfo.victimObject,
                    attackerObject = _inflictDotInfo.attackerObject,
                    dotIndex = _inflictDotInfo.dotIndex,
                    duration = _inflictDotInfo.duration,
                    damageMultiplier = _inflictDotInfo.damageMultiplier,
                    maxStacksFromAttacker = _inflictDotInfo.maxStacksFromAttacker
                };
                DotController.InflictDot(ref inflictDotInfo);

                // Decrease count
                count--;
            }
        }
    }
}
