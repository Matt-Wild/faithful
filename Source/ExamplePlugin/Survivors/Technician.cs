using RoR2;
using UnityEngine;

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
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BearVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBearVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BeetleGland",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBeetleGland"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Behemoth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBehemoth"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BleedOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTriTip"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BleedOnHitAndExplode",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBleedOnHitAndExplode"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BleedOnHitVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTriTipVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BonusGoldPackOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTome"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BossDamageBonus",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAPRound"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BounceNearby",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHook"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ChainLightning",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayUkulele"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ChainLightningVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayUkuleleVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Clover",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayClover"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("CloverVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayCloverVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("CooldownOnCrit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySkull"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("CritDamage",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLaserSight"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("CritGlasses",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGlasses"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("CritGlassesVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGlassesVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Crowbar",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayCrowbar"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Dagger",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDagger"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("DeathMark",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDeathMark"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ElementalRingVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayVoidRing"),
                "FingerR",
                new Vector3(0f, 0.025f, 0f),
                new Vector3(90f, 105f, 0f),
                new Vector3(0.25f, 0.25f, 0.25f)
                )
                );
            AddDefaultItemDisplay("EmpowerAlways",
            ItemDisplays.CreateLimbMaskDisplayRule(LimbFlags.Head),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySunHeadNeck"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySunHead"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EnergizedOnEquipmentUse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWarHorn"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EquipmentMagazine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBattery"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EquipmentMagazineVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFuelCellVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ExecuteLowHealthElite",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGuillotine"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ExplodeOnDeath",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWilloWisp"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ExplodeOnDeathVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWillowWispVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ExtraLife",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHippo"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ExtraLifeVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHippoVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("FallBoots",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGravBoots"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGravBoots"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Feather",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFeather"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("FireballsOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFireballsOnHit"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("FireRing",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFireRing"),
                "FingerL",
                new Vector3(0f, 0.025f, 0f),
                new Vector3(90f, 0f, 0f),
                new Vector3(0.25f, 0.25f, 0.25f)
                )
                );
            AddDefaultItemDisplay("Firework",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFirework"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("FlatHealth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySteakCurved"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("FocusConvergence",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFocusedConvergence"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("FragileDamageBonus",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDelicateWatch"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("FreeChest",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShippingRequestForm"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("GhostOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMask"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("GoldOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBoneCrown"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("GoldOnHurt",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRollOfPennies"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("HalfAttackSpeedHalfCooldowns",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLunarShoulderNature"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("HalfSpeedDoubleHealth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLunarShoulderStone"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("HeadHunter",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySkullcrown"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("HealingPotion",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHealingPotion"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("HealOnCrit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayScythe"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("HealWhileSafe",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySnail"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Hoof",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayHoof"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateLimbMaskDisplayRule(LimbFlags.RightCalf)
                );
            AddDefaultItemDisplay("IceRing",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayIceRing"),
                "FingerR",
                new Vector3(0f, 0.025f, 0f),
                new Vector3(90f, 0f, 0f),
                new Vector3(0.25f, 0.25f, 0.25f)
                )
                );
            AddDefaultItemDisplay("Icicle",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFrostRelic"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("IgniteOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGasoline"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ImmuneToDebuff",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRainCoatBelt"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("IncreaseHealing",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAntler"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAntler"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Incubator",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAncestralIncubator"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Infusion",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayInfusion"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("JumpBoost",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWaxBird"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("KillEliteFrenzy",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBrainstalk"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Knurl",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayKnurl"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("LaserTurbine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLaserTurbine"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("LightningStrikeOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayChargedPerforator"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("LunarDagger",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLunarDagger"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("LunarPrimaryReplacement",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBirdEye"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("LunarSecondaryReplacement",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBirdClaw"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("LunarSpecialReplacement",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBirdHeart"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("LunarTrinket",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBeads"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("LunarUtilityReplacement",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBirdFoot"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Medkit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMedkit"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("MinorConstructOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDefenseNucleus"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Missile",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMissileLauncher"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("MissileVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMissileLauncherVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("MonstersOnShrineUse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMonstersOnShrineUse"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("MoreMissile",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayICBM"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("MoveSpeedOnKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGrappleHook"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Mushroom",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMushroom"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("MushroomVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMushroomVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("NearbyDamageBonus",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDiamond"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("NovaOnHeal",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDevilHorns"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDevilHorns"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("NovaOnLowHealth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayJellyGuts"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("OutOfCombatArmor",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayOddlyShapedOpal"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ParentEgg",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayParentEgg"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Pearl",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayPearl"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("PermanentDebuffOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayScorpion"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("PersonalShield",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShieldGenerator"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Phasing",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayStealthkit"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Plant",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayInterstellarDeskPlant"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("PrimarySkillShuriken",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShuriken"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("RandomDamageZone",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRandomDamageZone"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("RandomEquipmentTrigger",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBottledChaos"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("RandomlyLunar",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDomino"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("RegeneratingScrap",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRegeneratingScrap"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("RepeatHeal",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayCorpseflower"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("SecondarySkillMagazine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDoubleMag"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Seed",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySeed"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ShieldOnly",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShieldBug"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShieldBug"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ShinyPearl",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayShinyPearl"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ShockNearby",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTeslaCoil"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("SiphonOnLowHealth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySiphonOnLowHealth"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("SlowOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBauble"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("SlowOnHitVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBaubleVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("SprintArmor",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBuckler"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("SprintBonus",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySoda"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("SprintOutOfCombat",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWhip"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("SprintWisp",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBrokenMask"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Squid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySquidTurret"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("StickyBomb",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayStickyBomb"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("StrengthenBurn",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGasTank"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("StunChanceOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayStunGrenade"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Syringe",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySyringeCluster"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Talisman",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTalisman"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Thorns",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRazorwireLeft"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("TitanGoldDuringTP",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGoldHeart"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Tooth",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothNecklaceDecal"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshLarge"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshSmall1"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshSmall2"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshSmall2"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayToothMeshSmall1"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("TPHealingNova",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGlowFlower"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("TreasureCache",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayKey"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("TreasureCacheVoid",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayKeyVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("UtilitySkillMagazine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("VoidMegaCrabItem",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMegaCrabItem"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("WarCryOnMultiKill",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayPauldron"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("WardOnLevel",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWarbanner"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BFG",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBFG"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Blackhole",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGravCube"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BossHunter",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTricornGhost"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBlunderbuss"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BossHunterConsumed",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTricornUsed"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("BurnNearby",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayPotion"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Cleanse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayWaterPack"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("CommandMissile",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMissileRack"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("CrippleWard",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEffigy"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("CritOnUse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayNeuralImplant"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("DeathProjectile",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayDeathProjectile"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("DroneBackup",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRadio"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EliteEarthEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteMendingAntlers"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EliteFireEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteHorn"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteHorn"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EliteHauntedEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteStealthCrown"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EliteIceEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteIceCrown"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EliteLightningEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EliteLunarEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteLunar,Eye"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("ElitePoisonEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEliteUrchinCrown"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("EliteVoidEquipment",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayAffixVoid"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("FireBallDash",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayEgg"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Fruit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayFruit"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("GainArmor",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayElephantFigure"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Gateway",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayVase"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("GoldGat",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGoldGat"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("GummyClone",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayGummyClone"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("IrradiatingLaser",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayIrradiatingLaser"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Jetpack",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBugWings"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("LifestealOnHit",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLifestealOnHit"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Lightning",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLightningArmRight"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                ),
                ItemDisplays.CreateLimbMaskDisplayRule(LimbFlags.RightArm)
                );
            AddDefaultItemDisplay("LunarPortalOnUse",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayLunarPortalOnUse"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Meteor",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMeteor"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Molotov",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayMolotov"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("MultiShopCard",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayExecutiveCard"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("QuestVolatileBattery",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayBatteryArray"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Recycle",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayRecycler"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Saw",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplaySawmerangFollower"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Scanner",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayScanner"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("TeamWarCry",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTeamWarCry"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("Tonic",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayTonic"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
            AddDefaultItemDisplay("VendingMachine",
            ItemDisplays.CreateDisplayRule(ItemDisplays.LoadDisplay("DisplayVendingMachine"),
                "Chest",
                new Vector3(2, 2, 2),
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
                )
                );
        }
    }
}
