using RoR2;
using UnityEngine;
using EntityStates;

namespace Faithful
{
    internal class Technician : Survivor
    {
        public Technician()
        {
            // Initialise survivor
            Init("TECHNICIAN", "mdlTechnician", "texTechnicianIcon", "texTechnicianMainIcon", _bodyColor: new Color(0.9414f, 0.7578f, 0.1953f), _sortPosition: 25, _maxHealth: 120.0f,
                 _healthRegen: 1.0f, _armour: 0.0f, _shield: 0.0f, _jumpCount: 1, _damage: 12, _attackSpeed: 1.0f, _crit: 1.0f, _moveSpeed: 7.0f, _acceleration: 80.0f, _jumpPower: 15.0f,
                 _aiType: AIType.Commando);
        }

        protected override void SetupAdditionalSkins()
        {
            // Add mastery skin
            AddSkin("MASTERY", Assets.GetSprite("texTechnicianMasteryIcon"), _skinReplacements: [
                new Skins.SkinReplacement
                {
                    childName = "TechnicianBody",
                    replacementMesh = "TechnicianMasterySkinTechnician",
                    replacementMaterial = "texTechnicianMastery"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianBodyLights",
                    replacementMesh = "TechnicianMasterySkinTechnicianLights"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianRobot",
                    replacementMesh = "TechnicianMasterySkinRobot",
                    replacementMaterial = "texTechnicianMastery"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianRobotLights",
                    replacementMesh = "TechnicianMasterySkinRobotLights"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianTurret",
                    replacementMesh = "TechnicianMasterySkinTurret",
                    replacementMaterial = "texTechnicianMastery"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianTurretLights",
                    replacementMesh = "TechnicianMasterySkinTurretLights"
                }
            ]);

            // Add prime skin
            AddSkin("PRIME", Assets.GetSprite("texTechnicianPrimeIcon"), _skinReplacements: [
                new Skins.SkinReplacement
                {
                    childName = "TechnicianBody",
                    replacementMesh = "TechnicianPrimeSkinTechnician",
                    replacementMaterial = "texTechnicianPrime"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianBodyLights",
                    replacementMesh = "TechnicianPrimeSkinTechnicianLights"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianRobot",
                    replacementMesh = "TechnicianPrimeSkinRobot",
                    replacementMaterial = "texTechnicianPrime"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianRobotLights",
                    replacementMesh = "TechnicianPrimeSkinRobotLights"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianTurret",
                    replacementMesh = "TechnicianPrimeSkinTurret",
                    replacementMaterial = "texTechnicianPrime"
                },
                new Skins.SkinReplacement
                {
                    childName = "TechnicianTurretLights",
                    replacementMesh = "TechnicianPrimeSkinTurretLights"
                }
            ]);
        }

        protected override void SetupSkills()
        {
            // No skills made yet
            SetupTempSkills();

            // Add technician skills
            AddSkill("ARC", "texTechnicianArcIcon", SkillSlot.Primary, new SerializableEntityStateType(typeof(Skills.Technician.ArcPrimary)));
            AddSkill("POWERVOLT", "texTechnicianPowervoltIcon", SkillSlot.Secondary, new SerializableEntityStateType(typeof(Skills.Temp.TempSecondary)), _baseRechargeInterval: 4.0f,
                     _interruptPriority: InterruptPriority.Skill, _mustKeyPress: true);
            AddSkill("CHARGE_FLOW", "texTechnicianChargeFlowIcon", SkillSlot.Utility, new SerializableEntityStateType(typeof(Skills.Temp.TempUtility)), _baseRechargeInterval: 5.0f,
                     _interruptPriority: InterruptPriority.PrioritySkill, _mustKeyPress: true);
            AddSkill("CONDUIT", "texTechnicianConduitIcon", SkillSlot.Special, new SerializableEntityStateType(typeof(Skills.Temp.TempSpecial)), _baseRechargeInterval: 20.0f,
                     _interruptPriority: InterruptPriority.PrioritySkill, _mustKeyPress: true);
        }

        protected override void SetupDefaultItemDisplays()
        {
            // Add item display information for default game item displays

            AddDefaultItemDisplay("AlienHead",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAlienHead"),
                "Pelvis",
                new Vector3(-0.14F, 0.0645F, -0.1175F),
                new Vector3(270F, 225F, 0F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("ArmorPlate",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRepulsionArmorPlate"),
                "Shin.L",
                new Vector3(-0.0225F, 0.165F, -0.0455F),
                new Vector3(85F, 180F, 180F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("ArmorReductionOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWarhammer"),
                "HandL",
                new Vector3(0.04F, 0.3795F, -0.367F),
                new Vector3(326F, 179.25F, 106F),
                new Vector3(0.175F, 0.175F, 0.175F)
                )
                );
            AddDefaultItemDisplay("AttackSpeedAndMoveSpeed",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayCoffee"),
                "Pelvis",
                new Vector3(0.155F, 0.09675F, -0.1185F),
                new Vector3(0F, 0F, 346.75F),
                new Vector3(0.15F, 0.15F, 0.15F)
                )
                );
            AddDefaultItemDisplay("AttackSpeedOnCrit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWolfPelt"),
                "Head",
                new Vector3(0F, 0.15F, 0.0975F),
                new Vector3(345F, 0F, 0F),
                new Vector3(0.7F, 0.7F, 0.7F)
                )
                );
            AddDefaultItemDisplay("AutoCastEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFossil"),
                "Pelvis",
                new Vector3(0.13525F, 0.1135F, -0.17625F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("Bandolier",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBandolier"),
                "Pelvis",
                new Vector3(0.0175F, 0.075F, 0F),
                new Vector3(272.5F, 270F, 90F),
                new Vector3(0.4F, 0.45F, 0.45F)
                )
                );
            AddDefaultItemDisplay("BarrierOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBrooch"),
                "Chest",
                new Vector3(-0.0745F, 0.06625F, 0.135F),
                new Vector3(84.5F, 213F, 228F),
                new Vector3(0.35F, 0.35F, 0.35F)
                )
                );
            AddDefaultItemDisplay("BarrierOnOverHeal",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAegis"),
                "LowerArmR",
                new Vector3(0.08F, 0.0575F, 0F),
                new Vector3(85.00005F, 270F, 0F),
                new Vector3(0.2F, 0.15F, 0.165F)
                )
                );
            AddDefaultItemDisplay("Bear",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBear"),
                "Head",
                new Vector3(0F, 0.1625F, -0.1825F),
                new Vector3(0F, 180F, 0F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("BearVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBearVoid"),
                "Head",
                new Vector3(0F, 0.1625F, -0.1825F),
                new Vector3(0F, 180F, 0F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("BeetleGland",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBeetleGland"),
                "Pelvis",
                new Vector3(0.145F, 0.0365F, -0.1275F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("Behemoth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBehemoth"),
                "UpperArmL",
                new Vector3(-0.1045F, 0.1825F, 0.07875F),
                new Vector3(0F, 310F, 0F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("BleedOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTriTip"),
                "UpperArmL",
                new Vector3(-0.0825F, 0.0175F, 0.15125F),
                new Vector3(350F, 150F, 0F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("BleedOnHitAndExplode",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBleedOnHitAndExplode"),
                "ThighR",
                new Vector3(0.095F, 0.005F, 0.02525F),
                new Vector3(0F, 0F, 190F),
                new Vector3(0.04F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("BleedOnHitVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTriTipVoid"),
                "UpperArmL",
                new Vector3(-0.0825F, 0.0125F, 0.15125F),
                new Vector3(347.5F, 150F, 310F),
                new Vector3(0.15F, 0.15F, 0.15F)
                )
                );
            AddDefaultItemDisplay("BonusGoldPackOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTome"),
                "Chest",
                new Vector3(0.005F, 0.20825F, -0.16575F),
                new Vector3(2.5F, 180F, 0F),
                new Vector3(0.1F, 0.1F, 0.1F)
                )
                );
            AddDefaultItemDisplay("BossDamageBonus",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAPRound"),
                "Pelvis",
                new Vector3(-0.1075F, 0.0775F, 0.1205F),
                new Vector3(280.5F, 125F, 196.75F),
                new Vector3(0.325F, 0.325F, 0.325F)
                )
                );
            AddDefaultItemDisplay("BounceNearby",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHook"),
                "Pelvis",
                new Vector3(-0.085F, 0.119F, -0.17625F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.225F, 0.225F, 0.225F)
                )
                );
            AddDefaultItemDisplay("ChainLightning",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayUkulele"),
                "Chest",
                new Vector3(0F, -0.00715F, -0.1775F),
                new Vector3(350F, 180F, 270F),
                new Vector3(0.375F, 0.375F, 0.375F)
                )
                );
            AddDefaultItemDisplay("ChainLightningVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayUkuleleVoid"),
                "Chest",
                new Vector3(0F, -0.00715F, -0.1775F),
                new Vector3(350F, 180F, 270F),
                new Vector3(0.375F, 0.375F, 0.375F)
                )
                );
            AddDefaultItemDisplay("Clover",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayClover"),
                "Head",
                new Vector3(-0.0875F, 0.255F, 0.0775F),
                new Vector3(30F, 300F, 0F),
                new Vector3(0.375F, 0.375F, 0.375F)
                )
                );
            AddDefaultItemDisplay("CloverVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayCloverVoid"),
                "Head",
                new Vector3(-0.0875F, 0.255F, 0.0775F),
                new Vector3(30F, 300F, 0F),
                new Vector3(0.375F, 0.375F, 0.375F)
                )
                );
            AddDefaultItemDisplay("CooldownOnCrit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySkull"),
                "Chest",
                new Vector3(-0.0025F, 0.225F, 0.125F),
                new Vector3(270F, 0F, 0F),
                new Vector3(0.1F, 0.1F, 0.1F)
                )
                );
            AddDefaultItemDisplay("CritDamage",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLaserSight"),
                "HandL",
                new Vector3(-0.0495F, 0.1F, -0.01625F),
                new Vector3(0F, 180F, 272.5F),
                new Vector3(0.0375F, 0.0375F, -0.0375F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLaserSight"),
                "HandR",
                new Vector3(0.0495F, 0.1F, -0.01625F),
                new Vector3(0F, 0F, 272.5F),
                new Vector3(0.0375F, 0.0375F, 0.0375F)
                )
                );
            AddDefaultItemDisplay("CritGlasses",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGlasses"),
                "Head",
                new Vector3(0F, 0.1575F, 0.145F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.275F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("CritGlassesVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGlassesVoid"),
                "Head",
                new Vector3(0F, 0.159F, 0.1395F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.3125F, 0.325F, 0.325F)
                )
                );
            AddDefaultItemDisplay("Crowbar",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayCrowbar"),
                "Chest",
                new Vector3(-0.1665F, 0.105F, -0.1275F),
                new Vector3(10F, 185F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("Dagger",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDagger"),
                "Chest",
                new Vector3(0.00375F, 0.25F, -0.0625F),
                new Vector3(0F, 30F, 5F),
                new Vector3(0.825F, 0.825F, 0.825F)
                )
                );
            AddDefaultItemDisplay("DeathMark",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDeathMark"),
                "HandL",
                new Vector3(-0.024F, 0.0165F, 0F),
                new Vector3(85F, 180.0001F, 90F),
                new Vector3(0.025F, 0.025F, 0.025F)
                )
                );
            AddDefaultItemDisplay("ElementalRingVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayVoidRing"),
                "FingerR",
                new Vector3(0F, 0.025F, 0F),
                new Vector3(90F, 105F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("EmpowerAlways",
            ItemDisplays.CreateLimbMaskDisplayRule(LimbFlags.Head),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySunHeadNeck"),
                "Chest",
                new Vector3(-0.015F, 0.325F, 0.0085F),
                new Vector3(0F, 270F, 345F),
                new Vector3(1F, 1F, 1F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySunHead"),
                "Head",
                new Vector3(0F, 0.1385F, 0F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.975F, 0.975F, 0.975F)
                )
                );
            AddDefaultItemDisplay("EnergizedOnEquipmentUse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWarHorn"),
                "ThighL",
                new Vector3(-0.11F, -0.00675F, -0.0485F),
                new Vector3(3.25F, 90F, 290F),
                new Vector3(0.3F, 0.3F, 0.3F)
                )
                );
            AddDefaultItemDisplay("EquipmentMagazine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBattery"),
                "Pelvis",
                new Vector3(0.044F, 0.111F, -0.18F),
                new Vector3(0F, 270F, 192.5F),
                new Vector3(0.175F, 0.175F, 0.175F)
                )
                );
            AddDefaultItemDisplay("EquipmentMagazineVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFuelCellVoid"),
                "Pelvis",
                new Vector3(0.04F, 0.111F, -0.18F),
                new Vector3(270F, 90F, 0F),
                new Vector3(0.175F, 0.2F, 0.175F)
                )
                );
            AddDefaultItemDisplay("ExecuteLowHealthElite",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGuillotine"),
                "Pelvis",
                new Vector3(0.00125F, 0.0725F, -0.1775F),
                new Vector3(275F, 0F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("ExplodeOnDeath",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWilloWisp"),
                "Pelvis",
                new Vector3(0.205F, 0.03F, 0.0425F),
                new Vector3(5F, 335F, 10F),
                new Vector3(0.0375F, 0.0375F, 0.0375F)
                )
                );
            AddDefaultItemDisplay("ExplodeOnDeathVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWillowWispVoid"),
                "Pelvis",
                new Vector3(0.205F, 0.03F, 0.0425F),
                new Vector3(350F, 127.5F, 352.5F),
                new Vector3(0.0375F, 0.0375F, 0.0375F)
                )
                );
            AddDefaultItemDisplay("ExtraLife",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHippo"),
                "Chest",
                new Vector3(0F, 0.4425F, -0.2025F),
                new Vector3(350F, 180F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("ExtraLifeVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHippoVoid"),
                "Chest",
                new Vector3(0F, 0.425F, -0.2025F),
                new Vector3(350F, 180F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("FallBoots",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGravBoots"),
                "Shin.L",
                new Vector3(0F, 0.575F, 0F),
                new Vector3(0F, 180F, 180F),
                new Vector3(0.15F, 0.15F, 0.15F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGravBoots"),
                "Shin.R",
                new Vector3(0F, 0.575F, 0F),
                new Vector3(0F, 180F, 180F),
                new Vector3(0.15F, 0.15F, 0.15F)
                )
                );
            AddDefaultItemDisplay("Feather",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFeather"),
                "UpperArmR",
                new Vector3(0.055F, 0.1625F, 0.0825F),
                new Vector3(75F, 80F, 40F),
                new Vector3(0.015F, 0.015F, 0.015F)
                )
                );
            AddDefaultItemDisplay("FireballsOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFireballsOnHit"),
                "HandL",
                new Vector3(-0.05775F, 0.165F, 0.00575F),
                new Vector3(272.5F, 185F, 270F),
                new Vector3(0.025F, 0.025F, 0.0375F)
                )
                );
            AddDefaultItemDisplay("FireRing",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFireRing"),
                "FingerL",
                new Vector3(0F, 0.025F, 0F),
                new Vector3(90F, 0F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("Firework",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFirework"),
                "Chest",
                new Vector3(0.1325F, 0.41F, -0.05375F),
                new Vector3(305F, 130.5F, 187.5F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("FlatHealth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySteakCurved"),
                "Chest",
                new Vector3(0.0075F, 0.32F, 0.04625F),
                new Vector3(290F, 0F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("FocusConvergence",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFocusedConvergence"),
                "Base",
                new Vector3(-0.625F, 2.25F, -0.625F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.075F, 0.075F, 0.075F)
                )
                );
            AddDefaultItemDisplay("FragileDamageBonus",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDelicateWatch"),
                "HandR",
                new Vector3(0.00878F, 0.00125F, 0.00175F),
                new Vector3(270F, 265.5F, 0F),
                new Vector3(0.375F, 0.55F, 0.375F)
                )
                );
            AddDefaultItemDisplay("FreeChest",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShippingRequestForm"),
                "Pelvis",
                new Vector3(-0.2015F, -0.00435F, 0.0335F),
                new Vector3(75F, 277.5F, 349F),
                new Vector3(0.3F, 0.3F, 0.3F)
                )
                );
            AddDefaultItemDisplay("GhostOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMask"),
                "Head",
                new Vector3(0F, 0.1525F, 0.095F),
                new Vector3(350F, 0F, 0F),
                new Vector3(0.75F, 0.625F, 0.75F)
                )
                );
            AddDefaultItemDisplay("GoldOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBoneCrown"),
                "Head",
                new Vector3(0F, 0.1975F, 0F),
                new Vector3(0F, 0F, 0F),
                new Vector3(1F, 1F, 1F)
                )
                );
            AddDefaultItemDisplay("GoldOnHurt",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRollOfPennies"),
                "Pelvis",
                new Vector3(-0.12375F, 0.0776F, -0.12F),
                new Vector3(8.5F, 0F, 6.5F),
                new Vector3(0.5F, 0.5F, 0.5F)
                )
                );
            AddDefaultItemDisplay("HalfAttackSpeedHalfCooldowns",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLunarShoulderNature"),
                "UpperArmR",
                new Vector3(0.105F, 0.02075F, 0.0275F),
                new Vector3(1.25F, 345F, 220F),
                new Vector3(0.825F, 0.825F, 0.825F)
                )
                );
            AddDefaultItemDisplay("HalfSpeedDoubleHealth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLunarShoulderStone"),
                "UpperArmL",
                new Vector3(-0.105F, 0.02075F, 0.0275F),
                new Vector3(358.75F, 200F, 205F),
                new Vector3(0.825F, 0.825F, 0.825F)
                )
                );
            AddDefaultItemDisplay("HeadHunter",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySkullcrown"),
                "Pelvis",
                new Vector3(0F, 0.0375F, 0F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.575F, 0.1F, 0.175F)
                )
                );
            AddDefaultItemDisplay("HealingPotion",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHealingPotion"),
                "Pelvis",
                new Vector3(0.14425F, 0.075F, 0.1005F),
                new Vector3(350F, 0F, 20F),
                new Vector3(0.025F, 0.025F, 0.025F)
                )
                );
            AddDefaultItemDisplay("HealOnCrit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayScythe"),
                "Chest",
                new Vector3(0F, 0.16525F, -0.17125F),
                new Vector3(300F, 100F, 80F),
                new Vector3(0.3F, 0.3F, 0.3F)
                )
                );
            AddDefaultItemDisplay("HealWhileSafe",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySnail"),
                "Chest",
                new Vector3(0.175F, 0.29F, -0.035F),
                new Vector3(62.5F, 101F, 336F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("Hoof",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHoof"),
                "Shin.R",
                new Vector3(0F, 0.55F, -0.0575F),
                new Vector3(67.5F, 0F, 0F),
                new Vector3(0.085F, 0.09F, 0.05F)
                )
                );
            AddDefaultItemDisplay("IceRing",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayIceRing"),
                "FingerR",
                new Vector3(0F, 0.025F, 0F),
                new Vector3(90F, 0F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("Icicle",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFrostRelic"),
                "Base",
                new Vector3(0.625F, 2.25F, -0.625F),
                new Vector3(270F, 0F, 0F),
                new Vector3(1F, 1F, 1F)
                )
                );
            AddDefaultItemDisplay("IgniteOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGasoline"),
                "ThighR",
                new Vector3(0.1075F, 0.125F, 0.025F),
                new Vector3(75F, 180F, 180F),
                new Vector3(0.45F, 0.45F, 0.45F)
                )
                );
            AddDefaultItemDisplay("ImmuneToDebuff",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRainCoatBelt"),
                "Pelvis",
                new Vector3(0F, 0.05F, 0.015F),
                new Vector3(2.6F, 0F, 0F),
                new Vector3(1F, 1F, 0.95F)
                )
                );
            AddDefaultItemDisplay("IncreaseHealing",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAntler"),
                "Head",
                new Vector3(-0.1275F, 0.1725F, -0.02875F),
                new Vector3(0F, 267.5F, 0F),
                new Vector3(-0.25F, 0.25F, 0.25F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAntler"),
                "Head",
                new Vector3(0.1275F, 0.1725F, -0.02875F),
                new Vector3(0F, 92.5F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("Incubator",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAncestralIncubator"),
                "Shin.R",
                new Vector3(0F, 0.4875F, 0.0575F),
                new Vector3(30F, 180F, 180F),
                new Vector3(0.015F, 0.015F, 0.015F)
                )
                );
            AddDefaultItemDisplay("Infusion",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayInfusion"),
                "Pelvis",
                new Vector3(0.131F, 0.07375F, 0.1055F),
                new Vector3(353F, 36.5F, 1.25F),
                new Vector3(0.325F, 0.325F, 0.325F)
                )
                );
            AddDefaultItemDisplay("JumpBoost",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWaxBird"),
                "Head",
                new Vector3(0F, 0F, -0.03F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.5F, 0.5F, 0.5F)
                )
                );
            AddDefaultItemDisplay("KillEliteFrenzy",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBrainstalk"),
                "Head",
                new Vector3(0F, 0.125F, 0.0125F),
                new Vector3(340F, 0F, 0F),
                new Vector3(0.3F, 0.3F, 0.3F)
                )
                );
            AddDefaultItemDisplay("Knurl",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayKnurl"),
                "Chest",
                new Vector3(0F, 0.2175F, 0.13575F),
                new Vector3(282.5F, 90F, 270F),
                new Vector3(0.05F, 0.025F, 0.05F)
                )
                );
            AddDefaultItemDisplay("LaserTurbine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLaserTurbine"),
                "ThighL",
                new Vector3(-0.11075F, 0.1105F, 0F),
                new Vector3(2.5F, 90F, 270F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("LightningStrikeOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayChargedPerforator"),
                "HandR",
                new Vector3(0.0475F, 0.1375F, 0.00625F),
                new Vector3(0.5F, 84.50001F, 177.5F),
                new Vector3(0.475F, 0.625F, 0.475F)
                )
                );
            AddDefaultItemDisplay("LunarDagger",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLunarDagger"),
                "Chest",
                new Vector3(-0.075F, 0.16525F, -0.17125F),
                new Vector3(0F, 100F, 80F),
                new Vector3(0.625F, 0.625F, 0.625F)
                )
                );
            AddDefaultItemDisplay("LunarPrimaryReplacement",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBirdEye"),
                "Head",
                new Vector3(0F, 0.1375F, 0.145F),
                new Vector3(280F, 180F, 180F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("LunarSecondaryReplacement",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBirdClaw"),
                "LowerArmR",
                new Vector3(0.0675F, 0.07375F, 0.002F),
                new Vector3(0F, 10F, 225F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("LunarSpecialReplacement",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBirdHeart"),
                "Base",
                new Vector3(-0.775F, 1.9F, -0.45F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("LunarTrinket",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBeads"),
                "HandL",
                new Vector3(0.0025F, 0F, 0.02F),
                new Vector3(0F, 0F, 315F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("LunarUtilityReplacement",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBirdFoot"),
                "Shin.L",
                new Vector3(0F, 0.4675F, 0.08F),
                new Vector3(0F, 90F, 65F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("Medkit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMedkit"),
                "Pelvis",
                new Vector3(0F, 0.03F, -0.1815F),
                new Vector3(270F, 180F, 0F),
                new Vector3(0.375F, 0.375F, 0.375F)
                )
                );
            AddDefaultItemDisplay("MinorConstructOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDefenseNucleus"),
                "Base",
                new Vector3(0.775F, 1.9F, -0.45F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("Missile",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMissileLauncher"),
                "Chest",
                new Vector3(-0.37F, 0.42825F, -0.024F),
                new Vector3(15F, 0F, 50F),
                new Vector3(0.075F, 0.075F, 0.075F)
                )
                );
            AddDefaultItemDisplay("MissileVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMissileLauncherVoid"),
                "Chest",
                new Vector3(-0.37F, 0.42825F, -0.024F),
                new Vector3(15F, 0F, 50F),
                new Vector3(0.075F, 0.075F, 0.075F)
                )
                );
            AddDefaultItemDisplay("MonstersOnShrineUse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMonstersOnShrineUse"),
                "Shin.R",
                new Vector3(0.1F, 0.11875F, 0.01375F),
                new Vector3(50F, 15F, 10F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("MoreMissile",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayICBM"),
                "UpperArmR",
                new Vector3(0.1045F, 0.1825F, 0.07875F),
                new Vector3(357.5F, 180F, 180F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("MoveSpeedOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGrappleHook"),
                "Shin.L",
                new Vector3(-0.098F, 0.01F, 0F),
                new Vector3(0F, 190F, 180F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("Mushroom",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMushroom"),
                "Chest",
                new Vector3(-0.152F, 0.29825F, -0.0875F),
                new Vector3(310F, 0F, 45F),
                new Vector3(0.0375F, 0.0375F, 0.0375F)
                )
                );
            AddDefaultItemDisplay("MushroomVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMushroomVoid"),
                "Chest",
                new Vector3(-0.152F, 0.29825F, -0.0875F),
                new Vector3(310F, 0F, 45F),
                new Vector3(0.0375F, 0.0375F, 0.0375F)
                )
                );
            AddDefaultItemDisplay("NearbyDamageBonus",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDiamond"),
                "HandR",
                new Vector3(0.046F, 0.02185F, 0.00425F),
                new Vector3(0F, 345F, 0F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("NovaOnHeal",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDevilHorns"),
                "Head",
                new Vector3(-0.1075F, 0.13375F, 0.07375F),
                new Vector3(15F, 0F, 0F),
                new Vector3(-0.375F, 0.375F, 0.375F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDevilHorns"),
                "Head",
                new Vector3(0.1075F, 0.13375F, 0.07375F),
                new Vector3(15F, 0F, 0F),
                new Vector3(0.375F, 0.375F, 0.375F)
                )
                );
            AddDefaultItemDisplay("NovaOnLowHealth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayJellyGuts"),
                "Chest",
                new Vector3(-0.025F, 0.4F, -0.0665F),
                new Vector3(10F, 0F, 0F),
                new Vector3(0.15F, 0.15F, 0.15F)
                )
                );
            AddDefaultItemDisplay("OutOfCombatArmor",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayOddlyShapedOpal"),
                "Chest",
                new Vector3(0.07665F, 0.0589F, 0.13175F),
                new Vector3(6.25F, 12.5F, 344.5F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("ParentEgg",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayParentEgg"),
                "Pelvis",
                new Vector3(-0.1575F, 0.065F, 0.12825F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("Pearl",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayPearl"),
                "Shin.R",
                new Vector3(0F, 0.125F, 0F),
                new Vector3(270F, 0F, 0F),
                new Vector3(0.045F, 0.045F, 0.045F)
                )
                );
            AddDefaultItemDisplay("PermanentDebuffOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayScorpion"),
                "Chest",
                new Vector3(0F, 0.325F, 0F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("PersonalShield",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShieldGenerator"),
                "Chest",
                new Vector3(0F, 0.1905F, 0.12575F),
                new Vector3(80F, 0F, 180F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("Phasing",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayStealthkit"),
                "Shin.R",
                new Vector3(0.0625F, 0.375F, 0F),
                new Vector3(0F, 0F, 90F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("Plant",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayInterstellarDeskPlant"),
                "Pelvis",
                new Vector3(-0.1705F, 0.0995F, -0.06275F),
                new Vector3(270F, 65F, 0F),
                new Vector3(0.03F, 0.03F, 0.03F)
                )
                );
            AddDefaultItemDisplay("PrimarySkillShuriken",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShuriken"),
                "Shin.L",
                new Vector3(0.0825F, 0.1235F, 0F),
                new Vector3(357.5F, 90F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("RandomDamageZone",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRandomDamageZone"),
                "Head",
                new Vector3(0F, 0.23575F, -0.135F),
                new Vector3(15F, 0F, 0F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("RandomEquipmentTrigger",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBottledChaos"),
                "Pelvis",
                new Vector3(-0.2F, 0.04F, -0.032F),
                new Vector3(10F, 90F, 15F),
                new Vector3(0.1F, 0.1F, 0.1F)
                )
                );
            AddDefaultItemDisplay("RandomlyLunar",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDomino"),
                "Base",
                new Vector3(-0.85F, 1.475F, -0.275F),
                new Vector3(90F, 0F, 0F),
                new Vector3(1F, 1F, 1F)
                )
                );
            AddDefaultItemDisplay("RegeneratingScrap",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRegeneratingScrap"),
                "Chest",
                new Vector3(0.2F, 0.492F, -0.1225F),
                new Vector3(345F, 345F, 345F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("RepeatHeal",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayCorpseflower"),
                "Chest",
                new Vector3(0.1285F, 0.16525F, 0.1515F),
                new Vector3(90F, 15F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("SecondarySkillMagazine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDoubleMag"),
                "Shin.R",
                new Vector3(0.084F, 0.1215F, 0.03575F),
                new Vector3(0F, 270F, 90F),
                new Vector3(0.0375F, 0.0375F, 0.0375F)
                )
                );
            AddDefaultItemDisplay("Seed",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySeed"),
                "Stomach",
                new Vector3(0.049F, 0.06325F, 0.128F),
                new Vector3(345F, 320F, 45F),
                new Vector3(0.025F, 0.025F, 0.025F)
                )
                );
            AddDefaultItemDisplay("ShieldOnly",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShieldBug"),
                "Head",
                new Vector3(-0.085F, 0.25925F, 0.09725F),
                new Vector3(0F, 270F, 0F),
                new Vector3(0.125F, 0.125F, -0.125F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShieldBug"),
                "Head",
                new Vector3(0.085F, 0.25925F, 0.09725F),
                new Vector3(0F, 270F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("ShinyPearl",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShinyPearl"),
                "Shin.L",
                new Vector3(0F, 0.125F, 0F),
                new Vector3(90F, 0F, 0F),
                new Vector3(0.045F, 0.045F, 0.045F)
                )
                );
            AddDefaultItemDisplay("ShockNearby",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTeslaCoil"),
                "Chest",
                new Vector3(0F, 0.475F, -0.168F),
                new Vector3(280F, 0F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("SiphonOnLowHealth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySiphonOnLowHealth"),
                "Chest",
                new Vector3(-0.2F, 0.165F, 0.10125F),
                new Vector3(2.5F, 300F, 354F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("SlowOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBauble"),
                "ThighL",
                new Vector3(-0.0725F, 0.25F, -0.16F),
                new Vector3(0F, 270F, 180F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("SlowOnHitVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBaubleVoid"),
                "ThighL",
                new Vector3(-0.0725F, 0.25F, -0.16F),
                new Vector3(0F, 270F, 180F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("SprintArmor",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBuckler"),
                "LowerArmR",
                new Vector3(0.076F, 0.076F, 0F),
                new Vector3(1.25F, 90F, 93.75F),
                new Vector3(0.175F, 0.175F, 0.175F)
                )
                );
            AddDefaultItemDisplay("SprintBonus",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySoda"),
                "Shin.L",
                new Vector3(0F, 0.335F, 0F),
                new Vector3(90F, 180F, 0F),
                new Vector3(0.3625F, 0.3625F, 0.3625F)
                )
                );
            AddDefaultItemDisplay("SprintOutOfCombat",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWhip"),
                "Pelvis",
                new Vector3(0.21F, -0.017F, -0.019F),
                new Vector3(0F, 0F, 12.5F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("SprintWisp",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBrokenMask"),
                "UpperArmR",
                new Vector3(0.09325F, -0.0045F, 0.02375F),
                new Vector3(32.5F, 80F, 192.5F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("Squid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySquidTurret"),
                "ThighR",
                new Vector3(0.07875F, 0.125F, 0F),
                new Vector3(0F, 0F, 270F),
                new Vector3(0.0375F, 0.0375F, 0.0375F)
                )
                );
            AddDefaultItemDisplay("StickyBomb",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayStickyBomb"),
                "Shin.L",
                new Vector3(-0.04525F, 0.1665F, 0.0835F),
                new Vector3(7.5F, 330F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("StrengthenBurn",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGasTank"),
                "Shin.R",
                new Vector3(0F, 0.33F, 0F),
                new Vector3(0F, 45F, 180F),
                new Vector3(0.155F, 0.155F, 0.155F)
                )
                );
            AddDefaultItemDisplay("StunChanceOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayStunGrenade"),
                "ThighL",
                new Vector3(-0.0845F, 0.0395F, -0.0775F),
                new Vector3(75F, 144F, 80F),
                new Vector3(0.5F, 0.5F, 0.5F)
                )
                );
            AddDefaultItemDisplay("Syringe",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySyringeCluster"),
                "ThighR",
                new Vector3(0.0575F, 0.0725F, 0.0675F),
                new Vector3(77.5F, 50F, 350F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("Talisman",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTalisman"),
                "Base",
                new Vector3(0.85F, 1.475F, -0.275F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.625F, 0.625F, 0.625F)
                )
                );
            AddDefaultItemDisplay("Thorns",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRazorwireLeft"),
                "UpperArmL",
                new Vector3(0F, 0F, 0F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("TitanGoldDuringTP",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGoldHeart"),
                "Chest",
                new Vector3(-0.0158F, 0.1455F, 0.1684F),
                new Vector3(0F, 0F, 345F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("Tooth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothNecklaceDecal"),
                "Chest",
                new Vector3(0F, 0.2825F, 0.065F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshLarge"),
                "Chest",
                new Vector3(0F, 0.18945F, 0.15055F),
                new Vector3(0F, 0F, 0F),
                new Vector3(1F, 1F, 1F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshSmall1"),
                "Chest",
                new Vector3(0.042F, 0.196F, 0.146F),
                new Vector3(0F, 0F, 20F),
                new Vector3(1F, 1F, 1F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshSmall2"),
                "Chest",
                new Vector3(0.07675F, 0.216F, 0.13375F),
                new Vector3(0F, 0F, 35F),
                new Vector3(1F, 1F, 1F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshSmall2"),
                "Chest",
                new Vector3(-0.042F, 0.196F, 0.146F),
                new Vector3(0F, 0F, 340F),
                new Vector3(1F, 1F, 1F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshSmall1"),
                "Chest",
                new Vector3(-0.07675F, 0.216F, 0.13375F),
                new Vector3(0F, 0F, 325F),
                new Vector3(1F, 1F, 1F)
                )
                );
            AddDefaultItemDisplay("TPHealingNova",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGlowFlower"),
                "Chest",
                new Vector3(0.1285F, 0.16525F, 0.1515F),
                new Vector3(0F, 15F, 0F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("TreasureCache",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayKey"),
                "Pelvis",
                new Vector3(-0.1825F, 0.07625F, 0.0275F),
                new Vector3(25F, 15F, 77.5F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("TreasureCacheVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayKeyVoid"),
                "Pelvis",
                new Vector3(-0.1825F, 0.07625F, 0.0275F),
                new Vector3(25F, 15F, 77.5F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("UtilitySkillMagazine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                "Shin.L",
                new Vector3(0F, 0.12125F, -0.02465F),
                new Vector3(275F, 180F, 180F),
                new Vector3(0.375F, 0.375F, 0.375F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                "Shin.R",
                new Vector3(0F, 0.12125F, -0.02465F),
                new Vector3(275F, 180F, 180F),
                new Vector3(0.375F, 0.375F, 0.375F)
                )
                );
            AddDefaultItemDisplay("VoidMegaCrabItem",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMegaCrabItem"),
                "Pelvis",
                new Vector3(0F, 0F, 0.13F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.1F, 0.1F, 0.1F)
                )
                );
            AddDefaultItemDisplay("WarCryOnMultiKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayPauldron"),
                "UpperArmL",
                new Vector3(-0.105F, 0.02075F, 0.025F),
                new Vector3(75F, 290F, 0F),
                new Vector3(1F, 1F, 1F)
                )
                );
            AddDefaultItemDisplay("WardOnLevel",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWarbanner"),
                "Pelvis",
                new Vector3(0.016F, 0.099F, -0.235F),
                new Vector3(270F, 90F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("BFG",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBFG"),
                "Chest",
                new Vector3(0.16675F, 0.25375F, -0.06975F),
                new Vector3(0F, 0F, 300F),
                new Vector3(0.3F, 0.3F, 0.3F)
                )
                );
            AddDefaultItemDisplay("Blackhole",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGravCube"),
                "Base",
                new Vector3(-0.445F, 1.845F, -0.25F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.5F, 0.5F, 0.5F)
                )
                );
            AddDefaultItemDisplay("BossHunter",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTricornGhost"),
                "Head",
                new Vector3(0F, 0.29275F, -0.09F),
                new Vector3(0F, 0F, 0F),
                new Vector3(1F, 1F, 1F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBlunderbuss"),
                "Base",
                new Vector3(-0.445F, 1.845F, -0.25F),
                new Vector3(90F, 0F, 0F),
                new Vector3(0.5F, 0.5F, 0.5F)
                )
                );
            AddDefaultItemDisplay("BossHunterConsumed",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTricornUsed"),
                "Head",
                new Vector3(0F, 0.29275F, -0.09F),
                new Vector3(0F, 0F, 0F),
                new Vector3(1F, 1F, 1F)
                )
                );
            AddDefaultItemDisplay("BurnNearby",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayPotion"),
                "Pelvis",
                new Vector3(0.17365F, 0.085F, -0.08675F),
                new Vector3(0F, 315F, 0F),
                new Vector3(0.025F, 0.025F, 0.025F)
                )
                );
            AddDefaultItemDisplay("Cleanse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWaterPack"),
                "Chest",
                new Vector3(0F, 0.09F, -0.2F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("CommandMissile",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMissileRack"),
                "Pelvis",
                new Vector3(0F, 0.11F, -0.22F),
                new Vector3(80F, 0F, 180F),
                new Vector3(0.3F, 0.3F, 0.3F)
                )
                );
            AddDefaultItemDisplay("CrippleWard",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEffigy"),
                "Pelvis",
                new Vector3(0.1775F, 0.03275F, -0.1F),
                new Vector3(350F, 137.5F, 355F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("CritOnUse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayNeuralImplant"),
                "Head",
                new Vector3(0F, 0.15675F, 0.265F),
                new Vector3(350F, 0F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("DeathProjectile",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDeathProjectile"),
                "Pelvis",
                new Vector3(0.1925F, 0.0725F, -0.09975F),
                new Vector3(0F, 130F, 0F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("DroneBackup",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRadio"),
                "Pelvis",
                new Vector3(0.17625F, 0.083F, -0.0665F),
                new Vector3(342.5F, 120F, 30F),
                new Vector3(0.375F, 0.375F, 0.375F)
                )
                );
            AddDefaultItemDisplay("EliteEarthEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteMendingAntlers"),
                "Head",
                new Vector3(0F, 0.23875F, 0.0775F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.625F, 0.625F, 0.625F)
                )
                );
            AddDefaultItemDisplay("EliteFireEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteHorn"),
                "Head",
                new Vector3(-0.0745F, 0.20625F, 0.07825F),
                new Vector3(0F, 0F, 0F),
                new Vector3(-0.1F, 0.1F, 0.1F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteHorn"),
                "Head",
                new Vector3(0.0745F, 0.20625F, 0.07825F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.1F, 0.1F, 0.1F)
                )
                );
            AddDefaultItemDisplay("EliteHauntedEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteStealthCrown"),
                "Head",
                new Vector3(0F, 0.1985F, -0.0205F),
                new Vector3(290F, 180F, 180F),
                new Vector3(0.0625F, 0.0625F, 0.0625F)
                )
                );
            AddDefaultItemDisplay("EliteIceEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteIceCrown"),
                "Head",
                new Vector3(0F, 0.3175F, -0.035F),
                new Vector3(285F, 180F, 180F),
                new Vector3(0.025F, 0.025F, 0.025F)
                )
                );
            AddDefaultItemDisplay("EliteLightningEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
                "Head",
                new Vector3(0F, 0.25F, 0.145F),
                new Vector3(310F, 0F, 0F),
                new Vector3(0.2F, 0.2F, 0.2F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
                "Head",
                new Vector3(0F, 0.29625F, 0.091F),
                new Vector3(290F, 0F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("EliteLunarEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteLunar,Eye"),
                "Head",
                new Vector3(0F, 0.15F, 0.2F),
                new Vector3(350F, 0F, 0F),
                new Vector3(0.175F, 0.175F, 0.175F)
                )
                );
            AddDefaultItemDisplay("ElitePoisonEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteUrchinCrown"),
                "Head",
                new Vector3(0F, 0.25F, 0F),
                new Vector3(270F, 0F, 0F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("EliteVoidEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAffixVoid"),
                "Head",
                new Vector3(0F, 0.121F, 0.13525F),
                new Vector3(80F, 0F, 0F),
                new Vector3(0.15F, 0.15F, 0.15F)
                )
                );
            AddDefaultItemDisplay("FireBallDash",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEgg"),
                "Pelvis",
                new Vector3(0.205F, 0.0725F, -0.056F),
                new Vector3(295F, 295.75F, 80F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("Fruit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFruit"),
                "Chest",
                new Vector3(0.0415F, 0.05635F, -0.03185F),
                new Vector3(0F, 45F, 0F),
                new Vector3(0.15F, 0.15F, 0.15F)
                )
                );
            AddDefaultItemDisplay("GainArmor",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayElephantFigure"),
                "Shin.L",
                new Vector3(-0.07125F, 0.308F, 0F),
                new Vector3(270F, 90F, 0F),
                new Vector3(0.45F, 0.45F, 0.45F)
                )
                );
            AddDefaultItemDisplay("Gateway",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayVase"),
                "Head",
                new Vector3(0F, 0.22425F, -0.115F),
                new Vector3(332.5F, 283.75F, 61.25F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("GoldGat",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGoldGat"),
                "Chest",
                new Vector3(0.3775F, 0.4055F, -0.11875F),
                new Vector3(45F, 90F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("GummyClone",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGummyClone"),
                "Pelvis",
                new Vector3(0.1725F, 0.08575F, -0.048F),
                new Vector3(27.5F, 269F, 337.5F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("IrradiatingLaser",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayIrradiatingLaser"),
                "Head",
                new Vector3(0.17F, 0.13F, -0.015F),
                new Vector3(350F, 350F, 276.25F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("Jetpack",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBugWings"),
                "Chest",
                new Vector3(0F, 0.125F, -0.14F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.15F, 0.15F, 0.15F)
                )
                );
            AddDefaultItemDisplay("LifestealOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLifestealOnHit"),
                "Head",
                new Vector3(-0.144F, 0.2775F, -0.133F),
                new Vector3(20F, 45F, 300F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("Lightning",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("displaylightningarmcustom"),
                "LightningJoint1",
                new Vector3(0F, 0F, 0F),
                new Vector3(0F, 0F, 0F),
                new Vector3(1F, 1F, 1F)
                ),
                ItemDisplays.CreateLimbMaskDisplayRule(LimbFlags.RightArm)
                );
            AddDefaultItemDisplay("LunarPortalOnUse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLunarPortalOnUse"),
                "Base",
                new Vector3(-0.445F, 1.845F, -0.25F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.625F, 0.625F, 0.625F)
                )
                );
            AddDefaultItemDisplay("Meteor",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMeteor"),
                "Base",
                new Vector3(-0.445F, 1.845F, -0.25F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.625F, 0.625F, 0.625F)
                )
                );
            AddDefaultItemDisplay("Molotov",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMolotov"),
                "Pelvis",
                new Vector3(0.165F, 0.025F, -0.1075F),
                new Vector3(5F, 315F, 330F),
                new Vector3(0.15F, 0.15F, 0.15F)
                )
                );
            AddDefaultItemDisplay("MultiShopCard",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayExecutiveCard"),
                "Pelvis",
                new Vector3(0.18F, 0.062F, -0.05825F),
                new Vector3(27F, 25.5F, 288.5F),
                new Vector3(0.5F, 0.5F, 0.5F)
                )
                );
            AddDefaultItemDisplay("QuestVolatileBattery",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBatteryArray"),
                "Chest",
                new Vector3(0F, 0.165F, -0.225F),
                new Vector3(5F, 180F, 180F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("Recycle",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRecycler"),
                "Pelvis",
                new Vector3(0.20125F, 0.0185F, -0.106F),
                new Vector3(0F, 35F, 10F),
                new Vector3(0.05F, 0.05F, 0.05F)
                )
                );
            AddDefaultItemDisplay("Saw",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySawmerangFollower"),
                "Base",
                new Vector3(-0.65F, 2.05F, 0.3075F),
                new Vector3(90F, 0F, 0F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("Scanner",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayScanner"),
                "Chest",
                new Vector3(0.1275F, 0.2355F, -0.01575F),
                new Vector3(295F, 105F, 265F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("TeamWarCry",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTeamWarCry"),
                "ThighR",
                new Vector3(0.0875F, 0.045F, -0.07375F),
                new Vector3(358F, 124.5F, 93F),
                new Vector3(0.0375F, 0.0375F, 0.0375F)
                )
                );
            AddDefaultItemDisplay("Tonic",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTonic"),
                "Pelvis",
                new Vector3(0.174F, 0.07825F, -0.069F),
                new Vector3(20F, 307.5F, 325F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("VendingMachine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayVendingMachine"),
                "Pelvis",
                new Vector3(0.155F, 0.0925F, -0.06F),
                new Vector3(347.5F, 128F, 17.5F),
                new Vector3(0.1F, 0.1F, 0.1F)
                )
                );
            AddDefaultItemDisplay("KnockBackHitEnemies",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayKnockbackFin"),
                "Chest",
                new Vector3(0F, 0.3025F, -0.14F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.5F, 0.5F, 0.5F)
                )
                );
            AddDefaultItemDisplay("AttackSpeedPerNearbyAllyOrEnemy",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRageCrystal"),
                "Pelvis",
                new Vector3(0.1545F, 0.08375F, 0.0675F),
                new Vector3(13.5F, 315.5F, 20F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("SpeedBoostPickup",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayElusiveAntlersLeft"),
                "Head",
                new Vector3(-0.07275F, 0.2F, 0.06475F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.75F, 0.75F, 0.75F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayElusiveAntlersRight"),
                "Head",
                new Vector3(0.07275F, 0.2F, 0.06475F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("IncreaseDamageOnMultiKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayIncreaseDamageOnMultiKill"),
                "Pelvis",
                new Vector3(0.137F, 0.0055F, -0.1265F),
                new Vector3(285F, 330F, 172.5F),
                new Vector3(0.125F, 0.125F, 0.125F)
                )
                );
            AddDefaultItemDisplay("DelayedDamage",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDelayedDamage"),
                "Chest",
                new Vector3(0F, 0.04325F, 0.1425F),
                new Vector3(7F, 0F, 0F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("TriggerEnemyDebuffs",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayNoxiousThorn"),
                "Chest",
                new Vector3(0.1675F, 0.2215F, -0.12875F),
                new Vector3(10F, 55F, 290F),
                new Vector3(0.2F, 0.2F, 0.2F)
                )
                );
            AddDefaultItemDisplay("LowerPricedChests",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLowerPricedChests"),
                "Base",
                new Vector3(-0.46F, 1.7F, -0.86F),
                new Vector3(270F, 0F, 0F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("ExtraShrineItem",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayChanceDoll"),
                "Shin.R",
                new Vector3(-0.08065F, 0.11375F, 0F),
                new Vector3(352.5F, 270F, 180F),
                new Vector3(0.075F, 0.075F, 0.075F)
                )
                );
            AddDefaultItemDisplay("IncreasePrimaryDamage",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayIncreasePrimaryDamage"),
                "UpperArmR",
                new Vector3(0.0995F, 0.1335F, 0.1175F),
                new Vector3(0F, 40F, 0F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("ExtraStatsOnLevelUp",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayPrayerBeads"),
                "Chest",
                new Vector3(0F, 0.3375F, 0.02575F),
                new Vector3(7.5F, 0F, 0F),
                new Vector3(1F, 1F, 1F)
                )
                );
            AddDefaultItemDisplay("TeleportOnLowHealth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTeleportOnLowHealth"),
                "Chest",
                new Vector3(0.18165F, 0.159F, 0.0785F),
                new Vector3(2F, 32.5F, 2.5F),
                new Vector3(0.625F, 0.625F, 0.625F)
                )
                );
            AddDefaultItemDisplay("ItemDropChanceOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySonorousEcho"),
                "Head",
                new Vector3(0.1925F, 0.12F, -0.00275F),
                new Vector3(9F, 248.5F, 22F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("BoostAllStats",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGrowthNectar"),
                "Head",
                new Vector3(0F, 0.275F, 0F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.5F, 0.5F, 0.5F)
                )
                );
            AddDefaultItemDisplay("StunAndPierce",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayElectricBoomerang"),
                "LowerArmL",
                new Vector3(-0.08F, 0.0325F, 0F),
                new Vector3(15F, 0F, 85F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("MeteorAttackOnHighDamage",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMeteorAttackOnHighDamage"),
                "Chest",
                new Vector3(0F, 0.4925F, -0.115F),
                new Vector3(345F, 0F, 20F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("BarrageOnBoss",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTreasuryDividends"),
                "Shin.L",
                new Vector3(-0.0025F, 0.37625F, -0.0675F),
                new Vector3(356F, 180F, 90F),
                new Vector3(0.75F, 0.75F, 0.75F)
                )
                );
            AddDefaultItemDisplay("OnLevelUpFreeUnlock",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayOnLevelUpFreeUnlock"),
                "Base",
                new Vector3(0.445F, 1.845F, -0.25F),
                new Vector3(0F, 0F, 0F),
                new Vector3(1F, 1F, 1F)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayOnLevelUpFreeUnlockTablet"),
                "Chest",
                new Vector3(0.0125F, 0.1675F, -0.195F),
                new Vector3(352.5F, 180F, 100F),
                new Vector3(1F, 1F, 1F)
                )
                );
            AddDefaultItemDisplay("HealAndRevive",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHealAndRevive"),
                "ThighL",
                new Vector3(-0.0625F, 0.09275F, 0.1015F),
                new Vector3(80F, 320F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("EliteAurelioniteEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteAurelioniteEquipment"),
                "Head",
                new Vector3(0F, 0.28F, 0.1295F),
                new Vector3(0F, 0F, 0F),
                new Vector3(0.25F, 0.25F, 0.25F)
                )
                );
            AddDefaultItemDisplay("EliteBeadEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteBeadSpike"),
                "Head",
                new Vector3(-0.0975F, 0.235F, -0.0645F),
                new Vector3(343.25F, 344.5F, 45F),
                new Vector3(0.0175F, 0.015F, 0.0175F)
                )
                );
        }
    }
}
