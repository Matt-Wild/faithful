using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace Faithful
{
    internal class AppraisersEye : ItemBase
    {
        // Store item and buff
        static Item appraisersEyeItem;
        static Buff scrutinizedBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store item settings
        Setting<bool> hiddenFromLogbookSetting;
        Setting<int> maxDebuffsSetting;
        Setting<int> maxDebuffsStackingSetting;
        Setting<float> critDamageSetting;

        // Store item stats
        bool hiddenFromLogbook;
        int maxDebuffs;
        int maxDebuffsStacking;
        static float critDamage;

        // Overlay for Scrutinized debuff
        static readonly Overlays.Overlay scrutinizedOverlay = Overlays.CreateOverlay(new Overlays.OverlaySettings
        {
            MaterialAddress = "RoR2/Base/CritOnUse/matFullCrit.mat",
            Colour = new Color(0.16f, 0.0f, 0.32f, 0.64f),
            Persistent = true,
            AnimateShaderAlpha = false
        });

        // IL hook identifiers
        const string takeDamageProcessHookName = "AppraisersEye.TakeDamageProcess";

        // Constructor
        public AppraisersEye(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("appraiserseyedisplaymesh");

            // Create item and buff
            appraisersEyeItem = Items.AddItem("APPRAISERS_EYE", "Appraisers Eye", [ItemTag.Damage, ItemTag.Technology, ItemTag.WorldUnique], "texappraiserseyeicon", "appraiserseyemesh", ItemTier.VoidTier3, _displaySettings: displaySettings, _namePrefix: "Collectors Vision", _hiddenFromLogbook: true);
            scrutinizedBuff = Buffs.AddBuff("SCRUTINIZED", "Scrutinized", "texBuffScrutinizedEye", Color.white, _isDebuff: true);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link behaviour
            Behaviour.AddOnHitEnemyLateCallback(OnHitEnemyLate);

            // More specific IL hooks
            IL.RoR2.HealthComponent.TakeDamageProcess += IL_HealthComponent_TakeDamageProcess;
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
            displaySettings.AddCharacterDisplay("Commando", "Head", new Vector3(0.0925F, 0.25F, 0.13375F), new Vector3(355F, 37.5F, 352.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "Head", new Vector3(0F, 0.1621F, 0.12725F), new Vector3(344F, 0F, 0F), new Vector3(0.0825F, 0.0825F, 0.0825F));
            displaySettings.AddCharacterDisplay("Bandit", "Head", new Vector3(0F, 0.055F, 0.12375F), new Vector3(0F, 0F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(-0.93F, 3.3725F, -0.46F), new Vector3(303.75F, 180F, 180F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0F, 0.375F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0F, 0.375F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0.5745F, -0.26F), new Vector3(0F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.79125F, 0.6F), new Vector3(0F, 0F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("Artificer", "Head", new Vector3(0F, 0.099F, 0.1025F), new Vector3(338.5F, 0F, 0F), new Vector3(0.0475F, 0.0475F, 0.0475F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(0.0541F, 0.14375F, 0.12125F), new Vector3(345.75F, 37.5F, 358.75F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("REX", "Eye", new Vector3(0F, 0.8375F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "Head", new Vector3(0.05375F, 0.121F, 0.112F), new Vector3(345F, 31.25F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, 2.145F, 1.1125F), new Vector3(350F, 0F, 0F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "Head", new Vector3(0F, 0.0675F, 0.1325F), new Vector3(355F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Head", new Vector3(0F, 0.125F, 0.1075F), new Vector3(337.5F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Hand", new Vector3(-0.019F, 0.09F, 0.0145F), new Vector3(350F, 270F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Seeker", "Head", new Vector3(0F, 0.1615F, 0.1215F), new Vector3(351.25F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "HandR", new Vector3(0.01525F, 0.144F, -0.0404F), new Vector3(348.75F, 191.25F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Head", new Vector3(-0.2065F, 0.1195F, -0.08675F), new Vector3(287.5F, 180F, 180F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Operator", "Head", new Vector3(-0.12555F, -0.1748F, 0.05195F), new Vector3(67.5F, 313.75F, 317.5F), new Vector3(0.05125F, 0.05125F, 0.05125F));
            displaySettings.AddCharacterDisplay("Drifter", "Head", new Vector3(-0.121F, 0.1885F, -0.06F), new Vector3(290F, 202.5F, 158.75F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Technician", "ChestEye", new Vector3(0F, 0F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            hiddenFromLogbookSetting = appraisersEyeItem.CreateSetting("HIDDEN_FROM_LOGBOOK", "Hide From Logbook?", true, "Should this item be hidden from the logbook?", false, _canRandomise: false, _restartRequired: true);
            maxDebuffsSetting = appraisersEyeItem.CreateSetting("MAX_DEBUFFS", "Max Debuffs", 1, "What's the maximum amount of scrutinized you should be able to apply to a single enemy using this item? (1 = 1 scrutinized)");
            maxDebuffsStackingSetting = appraisersEyeItem.CreateSetting("MAX_DEBUFFS_STACKING", "Max Debuffs Stacking", 1, "What's the maximum amount of scrutinized you should be able to apply to a single enemy using further stacks of this item? (1 = 1 scrutinized)");
            critDamageSetting = appraisersEyeItem.CreateSetting("CRIT_DAMAGE_MULT", "Crit Damage Multiplier", 20.0f, "How much should each stack of scrutinized increase crit damage? (20.0 = 20% increase)");
        }

        public override void FetchSettings()
        {
            // Get item settings
            hiddenFromLogbook = hiddenFromLogbookSetting.Value;
            maxDebuffs = maxDebuffsSetting.Value;
            maxDebuffsStacking = maxDebuffsStackingSetting.Value;
            critDamage = critDamageSetting.Value / 100.0f;

            // Update if hidden in logbook
            appraisersEyeItem.hiddenFromLogbook = hiddenFromLogbook;

            // Update item texts with new settings
            appraisersEyeItem.UpdateItemTexts();
        }

        private void OnHitEnemyLate(DamageInfo _info, GameObject _victim)
        {
            // Validate input
            if (_info == null || _victim == null) return;

            // Get victim body
            CharacterBody victimBody = _victim.GetComponent<CharacterBody>();
            if (victimBody == null) return;

            // Only crits should apply the debuff and damage bonus
            if (_info.crit == false) return;

            // Get attacker body
            if (_info.attacker == null) return;

            CharacterBody attackerBody = _info.attacker.GetComponent<CharacterBody>();
            if (attackerBody == null) return;

            // Get attacker inventory
            Inventory attackerInventory = attackerBody.inventory;
            if (attackerInventory == null) return;

            // Get item count
            int attackerItemCount = attackerInventory.GetItemCountEffective(appraisersEyeItem.itemDef.itemIndex);
            if (attackerItemCount <= 0) return;

            // Calculate max debuffs this attacker can apply
            int currentMaxDebuffs = maxDebuffs + ((attackerItemCount - 1) * maxDebuffsStacking);

            // Get current debuff count
            int victimDebuffCount = victimBody.GetBuffCount(scrutinizedBuff.buffDef.buffIndex);

            // Apply debuff if below cap
            if (victimDebuffCount < currentMaxDebuffs)
            {
                // Check if first debuff
                if (victimDebuffCount == 0)
                {
                    // Apply Scrutinized overlay
                    Overlays.ApplyOverlay(scrutinizedOverlay, victimBody);
                }

                victimBody.AddBuff(scrutinizedBuff.buffDef.buffIndex);
            }
        }

        private static void IL_HealthComponent_TakeDamageProcess(ILContext _il)
        {
            ILCursor c = new ILCursor(_il);

            if (!ILHelper.TryGotoNext(
                c,
                takeDamageProcessHookName,
                x => ILHelper.MatchFieldLoadOrPropertyGetter(x, "RoR2.CharacterBody", "critMultiplier"),
                x => x.OpCode == OpCodes.Mul))
            {
                ILHelper.DumpInstructions(_il, takeDamageProcessHookName);
                return;
            }

            ILHelper.EmitFloatModifier<HealthComponent, DamageInfo>(
                c,
                OpCodes.Ldarg_0,
                OpCodes.Ldarg_1,
                ModifyFinalCritDamage);

            if (Utils.verboseConsole) Log.Info($"[{takeDamageProcessHookName}] Successfully hooked crit multiplier.");
        }

        private static float ModifyFinalCritDamage(float _currentDamage, HealthComponent _healthComponent, DamageInfo _info)
        {
            // Validate input
            if (_healthComponent == null || _info == null)
            {
                return _currentDamage;
            }

            // Ignore if not a crit
            if (_info.crit == false)
            {
                return _currentDamage;
            }

            // Attempt to get character body
            CharacterBody characterBody = _healthComponent.body;
            if (characterBody == null)
            {
                return _currentDamage;
            }

            // Get debuff count
            int debuffCount = characterBody.GetBuffCount(scrutinizedBuff.buffDef.buffIndex);

            // Apply bonus damage
            if (debuffCount > 0)
            {
                _currentDamage *= 1.0f + (critDamage * debuffCount);
            }

            return _currentDamage;
        }
    }
}
