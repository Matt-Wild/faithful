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
    }
}
