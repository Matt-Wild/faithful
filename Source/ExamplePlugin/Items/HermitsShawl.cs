using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class HermitsShawl : ItemBase
    {
        // Store item and buff
        Buff buff;
        Item item;

        // Store reference to buff behaviour
        Patience buffBehaviour;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<int> maxBuffsStackingSetting;
        Setting<float> buffCooldownSetting;
        Setting<float> damageSetting;

        // Constructor
        public HermitsShawl(Toolbox _toolbox, Patience _patience) : base(_toolbox)
        {
            // Assign vengeance behaviour
            buffBehaviour = _patience;

            // Get buff
            buff = Buffs.GetBuff("PATIENCE");

            // Create display settings
            CreateDisplaySettings("HermitShawlDisplayMesh");

            // Create item
            item = Items.AddItem("HERMITS_SHAWL", [ItemTag.Damage], "texHermitShawlIcon", "HermitShawlMesh", ItemTier.Tier2, _displaySettings: displaySettings, _modifyItemDisplayPrefabCallback: ModifyModelPrefab, _debugOnly: true);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link On Damage Dealt behaviour
            Behaviour.AddOnDamageDealtCallback(OnDamageDealt);
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = Utils.CreateItemDisplaySettings(_displayMeshName, _useHopooShader: false);

            // Check for required asset
            if (!Assets.HasAsset(_displayMeshName))
            {
                return;
            }

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0.3375F, 0F), new Vector3(20F, 0F, 0F), new Vector3(0.2375F, 0.2375F, 0.2375F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0F, 0.25025F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Bandit", "Chest", new Vector3(0F, 0.37F, 0.004F), new Vector3(0F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 1.925F, 0.645F), new Vector3(0F, 0F, 0F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.4125F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.33F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0F, 0.25F, 0F), new Vector3(15F, 0F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0.295F, 0.0285F), new Vector3(10F, 0F, 0F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, 0.265F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Loader", "Chest", new Vector3(0F, 0.3F, 0.0125F), new Vector3(0F, 0F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, -0.6F, -0.64F), new Vector3(0F, 270F, 340F), new Vector3(2.125F, 2.125F, 2.125F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0.37125F, 0.037F), new Vector3(340F, 175F, 5F), new Vector3(0.1675F, 0.1675F, 0.1675F));
            displaySettings.AddCharacterDisplay("Railgunner", "Neck", new Vector3(-0.01168F, -0.02155F, 0.00168F), new Vector3(345F, 180F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(-0.0045F, 0.26625F, -0.01544F), new Vector3(0F, 22.5F, 0F), new Vector3(0.2125F, 0.2125F, 0.2125F));
            displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0.2805F, -0.07725F), new Vector3(357.5F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(0.0075F, 0.45F, -0.01213F), new Vector3(0F, 220F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Chef", "Head", new Vector3(-0.305F, 0F, 0F), new Vector3(290F, 180F, 270F), new Vector3(0.2375F, 0.2375F, 0.2375F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            maxBuffsStackingSetting = item.CreateSetting("MAX_BUFFS_STACKING", "Max Buffs", 4, "How many stacks of patience should the player receive per stack of this item? (4 = 4 stacks)");
            buffCooldownSetting = item.CreateSetting("BUFF_RECHARGE", "Buff Recharge Time", 10.0f, "After leaving combat how long does it take to receive the maximum amount of patience? (10.0 = 10 seconds)");
            damageSetting = item.CreateSetting("DAMAGE", "Damage", 25.0f, "How much should each stack of patience increase damage? (25.0 = 25% increase)");
        }

        public override void FetchSettings()
        {
            // Apply damage to buff
            buffBehaviour.damage = damageSetting.Value / 100.0f;

            // Update item texts with new settings
            item.UpdateItemTexts();
        }

        void ModifyModelPrefab(GameObject _prefab)
        {
            // Get first shawl object
            GameObject shawl = Utils.FindChildByName(_prefab.transform, "Shawl.001");

            // Add dynamic bone behaviour
            DynamicBone dynamicBone = shawl.AddComponent<DynamicBone>();
            
            // Set up dynamic bone
            dynamicBone.m_Root = shawl.transform;
            dynamicBone.m_UpdateRate = 60.0f;
            dynamicBone.m_UpdateMode = DynamicBone.UpdateMode.Normal;
            dynamicBone.m_Damping = 0.576f;
            dynamicBone.m_DampingDistrib = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            dynamicBone.m_Elasticity = 0.016f;
            dynamicBone.m_ElasticityDistrib = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            dynamicBone.m_Stiffness = 0.0f;
            dynamicBone.m_StiffnessDistrib = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            dynamicBone.m_Inert = 0.0f;
            dynamicBone.m_InertDistrib = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            dynamicBone.m_Radius = 0.0f;
            dynamicBone.m_RadiusDistrib = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            dynamicBone.m_EndLength = 0.69f;
            dynamicBone.m_EndOffset = Vector3.zero;
            dynamicBone.m_Gravity = new Vector3(0.0f, -0.01f, 0.0f);
            dynamicBone.m_Force = Vector3.zero;
            dynamicBone.m_Colliders = new System.Collections.Generic.List<DynamicBoneCollider>();
            dynamicBone.m_Exclusions = new System.Collections.Generic.List<Transform>();
            dynamicBone.m_FreezeAxis = DynamicBone.FreezeAxis.None;
            dynamicBone.m_DistantDisable = false;
            dynamicBone.m_ReferenceObject = null;
            dynamicBone.m_DistanceToObject = 20.0f;
            dynamicBone.neverOptimize = false;
        }

        void OnDamageDealt(DamageReport _report)
        {
            // Ignore DoTs
            if (_report.dotType != DotController.DotIndex.None) return;
            
            // Check for attacker body
            CharacterBody attacker = _report.attackerBody;
            if (attacker != null)
            {
                // Get patience buff count
                int buffCount = attacker.GetBuffCount(buff.buffDef);
                
                // Check for buff
                if (buffCount > 0)
                {
                    // Remove patience buff
                    attacker.SetBuffCount(buff.buffDef.buffIndex, 0);

                    // Get faithful helper
                    FaithfulCharacterBodyBehaviour helper = Utils.FindCharacterBodyHelper(attacker);
                    if (helper != null)
                    {
                        // Get hermit's shawl behaviour
                        FaithfulHermitsShawlBehaviour behaviour = helper.hermitsShawl;
                        if (behaviour != null)
                        {
                            // Force attacker into combat
                            behaviour.ForceIntoCombat();
                        }
                    }
                }
            }

            // Check for victim body
            CharacterBody victim = _report.victimBody;
            if (victim != null)
            {
                // Get patience buff count
                int buffCount = victim.GetBuffCount(buff.buffDef);

                // Check for buff
                if (buffCount > 0)
                {
                    // Remove patience buff
                    victim.SetBuffCount(buff.buffDef.buffIndex, 0);
                }
            }
        }
    }
}
