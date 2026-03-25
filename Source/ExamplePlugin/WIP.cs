using RoR2;
using UnityEngine;
using System.Collections.Generic;

namespace Faithful
{
    internal static class WIP
    {
        // List of WIP items
        private static List<WIPItem> wipItems = [];

        public static void Init()
        {
            // Create WIP items to test models etc
            CreateItem(_name: "Hexagonal Cluster",
                _pickup: "Killing an enemy releases a swarm of bees.",
                _description: "Killing an enemy releases a <style=cIsDamage>swarm</style> of <style=cIsDamage>6</style> bees that attack surrounding enemies. Each bee deals <style=cIsDamage>25%</style> base damage up to <style=cIsDamage>3</style> <style=cStack>(+3 per stack)</style> times.",
                _lore: "<style=cMono>\r//--AUTO-TRANSCRIPTION FROM UES [Redacted] --//</style>\r\n\r\n\u0022You signed off on live storage for that thing?\u0022\n\n\u0022I signed off on a sealed specimen cluster. Emphasis on sealed.\u0022\n\n\u0022Then why is the cargo bay humming?\u0022\n\n\u0022Humming?\u0022\n\n\u0022Yes. Humming. Loudly. And, unless my eyes are failing me, leaking.\u0022\n\n\u0022That\u2019s not possible. It only responds to kinetic agitation.\u0022\n\n\u0022...Kinetic agitation such as firing it repeatedly into a crowd?\u0022\n\n\u0022In hindsight, perhaps the briefing could have been more specific.\u0022\n\n\u0022I don\u2019t need hindsight, doctor, I need those things out of the ventilation.\u0022\n\n\u0022Well don\u2019t shout. They become excited by raised voices.\u0022\n\n\u0022They become excited by everything! Gunfire, footsteps, light, shadows, me thinking too hard in their general direction—\u0022\n\n\u0022Remarkable, aren\u2019t they?\u0022\n\n\u0022There are thousands of them.\u0022\n\n\u0022Yes. More than we anticipated.\u0022\n\n\u0022You told me it would release a defensive swarm.\u0022\n\n\u0022I did.\u0022\n\n\u0022This is not a swarm. This is a medical emergency with wings.\u0022\n\n\u0022That seems a touch dramatic.\u0022\n\n\u0022They have filled my locker, doctor.\u0022\n\n\u0022Then I would advise against opening it.\u0022",
                _tier: ItemTier.Tier1,
                _tags: [ItemTag.Damage, ItemTag.OnKillEffect],
                _iconDir: "texhexclustericon",
                _modelDir: "hexclustermesh",
                _displayDir: "hexclusterdisplaymesh",
                _displaySettingsCallback: _displaySettings =>
                {
                    // Add character display settings
                    _displaySettings.AddCharacterDisplay("Commando", "ThighL", new Vector3(0.1125F, 0.1125F, 0.04F), new Vector3(270F, 257.5F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("Huntress", "Pelvis", new Vector3(0.1075F, 0.015F, 0.13F), new Vector3(80F, 210F, 180F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Bandit", "CalfR", new Vector3(0.1025F, 0.066F, 0.03F), new Vector3(72.5F, 55F, 336.75F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "CalfL", new Vector3(0F, 0.905F, 0.815F), new Vector3(87.50002F, 180F, 180F), new Vector3(1.25F, 1.25F, 1.25F));
                    _displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0.005F, 0.4575F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
                    _displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(-0.005F, 0.4575F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
                    _displaySettings.AddCharacterDisplay("Turret", "LegBar3", new Vector3(0F, 0.725F, 0.2F), new Vector3(0F, 270F, 255F), new Vector3(0.425F, 0.425F, 0.425F));
                    _displaySettings.AddCharacterDisplay("Walker Turret", "LegBar3", new Vector3(0F, 0.725F, 0.2F), new Vector3(0F, 270F, 255F), new Vector3(0.425F, 0.425F, 0.425F));
                    _displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.115F, 0.3325F, -0.1775F), new Vector3(0F, 87.5F, 7.5F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Mercenary", "ThighL", new Vector3(0.0675F, 0.22F, -0.111F), new Vector3(277.5F, 150F, 180F), new Vector3(0.15F, 0.175F, 0.15F));
                    _displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(-0.3275F, 0.8275F, 0.0315F), new Vector3(0F, 20F, 52.5F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("Loader", "MechHandR", new Vector3(0F, 0.24F, 0F), new Vector3(0F, 175F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Acrid", "Jaw", new Vector3(0F, 2F, 0.2F), new Vector3(85F, 0F, 0F), new Vector3(1.25F, 1.25F, 1.25F));
                    _displaySettings.AddCharacterDisplay("Captain", "ThighL", new Vector3(0.0225F, 0.305F, -0.11F), new Vector3(357.5F, 77.75F, 268.75F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Railgunner", "Battery1R", new Vector3(0F, 0.225F, 0F), new Vector3(0F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Void Fiend", "UpperArmR", new Vector3(0.1825F, -0.06F, 0.1575F), new Vector3(77.5F, 307.5F, 266.25F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.268F, 0.179F, -0.226F), new Vector3(357F, 357F, 46.75F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("False Son", "UpperArmL", new Vector3(-0.123F, 0.257F, -0.0675F), new Vector3(277.5F, 65.75F, 349.5F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("Chef", "Base", new Vector3(0.0236F, -0.036F, -0.511F), new Vector3(314F, 234.25F, 177.25F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Operator", "ClawSpin", new Vector3(0F, 0F, 0.032F), new Vector3(0F, 270F, 270F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Drifter", "BagBottom", new Vector3(-0.19F, 0.128F, 0F), new Vector3(0F, 2.5F, 70F), new Vector3(0.25F, 0.25F, 0.25F));
                    _displaySettings.AddCharacterDisplay("Scavenger", "Backpack", new Vector3(-7.855F, 5.63F, -1.275F), new Vector3(0F, 340F, 84F), new Vector3(2F, 2F, 2F));
                    _displaySettings.AddCharacterDisplay("Technician", "Shin.L", new Vector3(-0.09F, 0.025F, 0F), new Vector3(0F, 180F, 270F), new Vector3(0.1125F, 0.1125F, 0.1125F));
                });

            CreateItem(_name: "Matter Accelerator",
                _pickup: "Increase movement speed while you have a shield or barrier.",
                _description: "Gain a <style=cIsHealing>shield</style> equal to <style=cIsHealing>5%</style> of your maximum health. While you have a <style=cIsHealing>shield</style> or <style=cIsHealing>barrier</style>, <style=cIsUtility>movement speed</style> is increased by <style=cIsUtility>20%</style> <style=cStack>(+20% per stack)</style>.",
                _lore: "\u0022Day 17 of making a homemade matter accelerator.\r\n\r\nThere's some good and bad news on my progress so far.\r\n\r\nThe good news is that I've finally got it working consistently enough to start testing its specs. The bad news is that I'm going to need a better camera in order to record any actual results. The one I have now is way too blurry.\r\n\r\nOh, I also managed to trigger my neighbor's home security system. So I should probably work on avoiding that.\u0022",
                _tier: ItemTier.Tier1,
                _tags: [ItemTag.Utility, ItemTag.Healing, ItemTag.Technology, ItemTag.MobilityRelated],
                _iconDir: "texmatteracceleratoricon",
                _modelDir: "matteracceleratormesh",
                _displayDir: "matteracceleratordisplaymesh",
                _displaySettingsCallback: _displaySettings =>
                {
                    // Add character display settings
                    _displaySettings.AddCharacterDisplay("Commando", "LowerArmR", new Vector3(-0.01F, 0.1725F, 0F), new Vector3(87.5F, 90F, 90F), new Vector3(0.225F, 0.225F, 0.225F));
                    _displaySettings.AddCharacterDisplay("Huntress", "UpperArmR", new Vector3(0.0185F, 0.22F, 0.005F), new Vector3(270F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("Bandit", "CalfL", new Vector3(0F, 0.275F, 0.0125F), new Vector3(87.5F, 0F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "LowerArmL", new Vector3(0.2625F, 1.9F, 0F), new Vector3(270F, 0F, 0F), new Vector3(2.75F, 2.75F, 2.75F));
                    _displaySettings.AddCharacterDisplay("Engineer", "LowerArmR", new Vector3(-0.005F, 0.145F, 0F), new Vector3(90F, 0F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
                    _displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0.575F, 1.5625F), new Vector3(0F, 0F, 0F), new Vector3(0.6F, 0.6F, 0.6F));
                    _displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.785F, 0.395F), new Vector3(0F, 0F, 0F), new Vector3(0.9F, 0.9F, 1F));
                    _displaySettings.AddCharacterDisplay("Artificer", "CalfL", new Vector3(-0.0075F, 0.16125F, 0.05F), new Vector3(270F, 0F, 0F), new Vector3(0.2125F, 0.2125F, 0.2125F));
                    _displaySettings.AddCharacterDisplay("Mercenary", "ThighR", new Vector3(-0.0115F, 0.3275F, 0F), new Vector3(90F, 0F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
                    _displaySettings.AddCharacterDisplay("REX", "CalfBackR", new Vector3(0F, 0.475F, -0.0325F), new Vector3(90F, 0F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
                    _displaySettings.AddCharacterDisplay("Loader", "LowerArmR", new Vector3(-0.005F, 0.15F, 0.0025F), new Vector3(85F, 180F, 180F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("Acrid", "UpperArmR", new Vector3(0.15F, 3.1F, 0F), new Vector3(90F, 0F, 0F), new Vector3(3F, 3F, 3F));
                    _displaySettings.AddCharacterDisplay("Captain", "HandL", new Vector3(-0.00375F, 0.134F, 0.0035F), new Vector3(90F, 0F, 0F), new Vector3(0.1275F, 0.1275F, 0.1275F));
                    _displaySettings.AddCharacterDisplay("Railgunner", "MuzzleSniper", new Vector3(0F, 0F, -0.0635F), new Vector3(0F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.06F));
                    _displaySettings.AddCharacterDisplay("Void Fiend", "ThighL", new Vector3(0.01F, 0.26675F, 0F), new Vector3(90F, 0F, 0F), new Vector3(0.275F, 0.275F, 0.225F));
                    _displaySettings.AddCharacterDisplay("Seeker", "CalfL", new Vector3(0F, 0.1365F, -0.015F), new Vector3(90F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(0.01115F, 0.288F, 0F), new Vector3(271.25F, 63.25F, 63.250F), new Vector3(0.45F, 0.45F, 0.45F));
                    _displaySettings.AddCharacterDisplay("Chef", "Pelvis", new Vector3(0.06875F, 0F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.375F, 0.375F, 0.175F));
                    _displaySettings.AddCharacterDisplay("Operator", "UpperArmL", new Vector3(-0.136F, -0.0065F, -0.009F), new Vector3(10F, 262.5F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("Drifter", "LowerArmR", new Vector3(-0.175F, 0F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.225F, 0.225F, 0.15F));
                    _displaySettings.AddCharacterDisplay("Best Buddy", "CableR", new Vector3(0.0075F, 0.00025F, 0.0075F), new Vector3(315F, 307.5F, 35F), new Vector3(0.25F, 0.25F, 0.25F));
                    _displaySettings.AddCharacterDisplay("Scavenger", "LowerArmR", new Vector3(0F, 1.375F, 0F), new Vector3(90F, 0F, 0F), new Vector3(1.75F, 1.75F, 1.75F));
                    _displaySettings.AddCharacterDisplay("Technician", "LowerArmL", new Vector3(0F, 0.225F, 0F), new Vector3(275F, 90F, 270F), new Vector3(0.15F, 0.15F, 0.125F));
                });

            CreateItem(_name: "Radiant Timepiece",
                _pickup: "Increase the duration of temporary buffs.",
                _description: "Increase the <style=cIsUtility>duration</style> of temporary buffs by <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> second(s).",
                _lore: "Order: Henlein Brand Pocket Watch 222nd Edition (Silver Gray)\r\nTracking Number: 1510******\r\nEstimated Delivery: 08/23/2057\r\nShipping Method: High Priority/Fragile\r\nShipping Address: Antiques Workshop, Continuum St., Saturn\r\nShipping Details:\r\n\r\nThank you for your purchase of:\r\n > 1 Henlein Brand Pocket Watch 222nd Edition, Silver Gray\r\n\r\nWe hope you are satisfied with its craftsmanship. We pride ourselves on our work as the largest luxury watch retailer of Earth so do contact our customer support line if you notice any defects. Our traditional horologists will always be available to repair any products for a small* fee.\r\n\r\nFurther details can be found in the handbook alongside your order.\r\n\r\nWe wish you a timely day.\r\n\r\n- Henlein",
                _tier: ItemTier.Tier1,
                _tags: [ItemTag.Utility, ItemTag.Technology, ItemTag.AIBlacklist],
                _iconDir: "texradianttimepieceicon",
                _modelDir: "radianttimepiecemesh",
                _displayDir: "radianttimepiecedisplaymesh",
                _displaySettingsCallback: _displaySettings =>
                {
                    // Add character display settings
                    _displaySettings.AddCharacterDisplay("Commando", "Stomach", new Vector3(-0.08F, 0F, -0.12975F), new Vector3(352.5F, 185F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0.1685F, 0.0205F, 0.0575F), new Vector3(0F, 90F, 348.75F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Bandit", "Stomach", new Vector3(0.035F, -0.0275F, -0.17F), new Vector3(340F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(1.53F, 0.45F, -1.825F), new Vector3(0F, 180F, 0F), new Vector3(1F, 1F, 1F));
                    _displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(-0.0745F, -0.105F, -0.3336F), new Vector3(345F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(-0.42F, 0.15F, -1.4375F), new Vector3(0F, 215F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
                    _displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(-0.63F, 0.92F, -1.595F), new Vector3(0F, 180F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
                    _displaySettings.AddCharacterDisplay("Artificer", "Pelvis", new Vector3(0.195F, -0.0075F, -0.0125F), new Vector3(17.25F, 75.5F, 195F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Mercenary", "Pelvis", new Vector3(0.1125F, 0.104F, 0.079F), new Vector3(7.5F, 0F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(-0.375F, 0.025F, -0.5475F), new Vector3(0F, 195F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
                    _displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(0.1705F, 0.315F, -0.175F), new Vector3(0F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Acrid", "SpineChest3", new Vector3(1.5425F, 1.475F, -0.05F), new Vector3(335F, 90F, 270F), new Vector3(1F, 1F, 1F));
                    _displaySettings.AddCharacterDisplay("Captain", "Pelvis", new Vector3(0.1475F, -0.0615F, -0.175F), new Vector3(357.5F, 167.5F, 183.75F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Railgunner", "Pelvis", new Vector3(0.07F, 0.12F, 0.135F), new Vector3(22.5F, 20F, 182.5F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(0.15125F, -0.07375F, -0.15F), new Vector3(17.5F, 159.25F, 339.25F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Seeker", "Pelvis", new Vector3(0.23475F, -0.0175F, -0.027F), new Vector3(332.5F, 98.75F, 1F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("False Son", "Pelvis", new Vector3(-0.15875F, 0.024F, -0.2425F), new Vector3(355F, 185F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(-0.1375F, -0.245F, -0.22F), new Vector3(90F, 270F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Operator", "Stomach", new Vector3(-0.017F, 0.0925F, -0.0705F), new Vector3(292F, 212F, 237.5F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(0.25675F, -0.09575F, -0.3425F), new Vector3(60F, 207.5F, 257.5F), new Vector3(0.12F, 0.12F, 0.12F));
                    _displaySettings.AddCharacterDisplay("Technician", "Pelvis", new Vector3(-0.11625F, 0.0475F, -0.194F), new Vector3(355F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                },
                _modifyItemDisplayPrefabCallback: _prefab =>
                {
                    // Get first timepiece object
                    GameObject shawl = Utils.FindChildByName(_prefab.transform, "Chain.001");

                    // Add dynamic bone behaviour
                    DynamicBone dynamicBone = shawl.AddComponent<DynamicBone>();

                    // Set up dynamic bone
                    Utils.ConfigureScarfDynamicBone(dynamicBone);
                });

            CreateItem(_name: "Reserve Battery",
                _pickup: "Killing an enemy adds a temporary extra charge of a random skill.",
                _description: "Killing an enemy adds <style=cIsUtility>+1 temporary</style> charge of either your <style=cIsUtility>Secondary</style>, <style=cIsUtility>Utility</style> or <style=cIsUtility>Special skill</style>, up to a maximum of <style=cIsUtility>+1</style> <style=cStack>(+1 per stack)</style> for each skill.",
                _lore: "> PRIMARY POWER STATUS : <style=cMono>100%</style>\r\n> RESERVE [<style=cMono>1</style>] POWER STATUS : <style=cMono>100%</style>\r\n> RESERVE [<style=cMono>2</style>] POWER STATUS : <style=cMono>100%</style>\r\n> RESERVE [<style=cMono>3</style>] POWER STATUS : <style=cMono>097%</style>\r\n\r\n> COMMAND : <style=cMono>RECHARGE RESERVE [3]</style>\r\n\r\n> DAILY REMINDER : <style=cMono>\u0022A prepper is a fool every day but doomsday.\u0022</style>",
                _tier: ItemTier.Tier1,
                _tags: [ItemTag.Utility, ItemTag.Technology, ItemTag.OnKillEffect, ItemTag.AIBlacklist],
                _iconDir: "texreservebatteryicon",
                _modelDir: "reservebatterymesh",
                _displayDir: "reservebatterydisplaymesh",
                _displaySettingsCallback: _displaySettings =>
                {
                    // Add character display settings
                    _displaySettings.AddCharacterDisplay("Commando", "GunL", new Vector3(-0.025F, 0.056F, 0F), new Vector3(0F, 0F, 315F), new Vector3(0.05F, 0.05F, 0.05F));
                    _displaySettings.AddCharacterDisplay("Commando", "GunR", new Vector3(0.025F, 0.056F, 0F), new Vector3(0F, 180F, 315F), new Vector3(0.05F, 0.05F, 0.05F));
                    _displaySettings.AddCharacterDisplay("Huntress", "ThighL", new Vector3(0.1075F, 0.15F, -0.0105F), new Vector3(357.75F, 297.375F, 186.375F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Bandit", "ThighL", new Vector3(0.10825F, 0.265F, -0.016F), new Vector3(0F, 93.75F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "UpperArmR", new Vector3(0.5675F, 2.225F, -0.025F), new Vector3(0F, 270F, 187.5F), new Vector3(0.8F, 0.8F, 0.8F));
                    _displaySettings.AddCharacterDisplay("Engineer", "UpperArmL", new Vector3(0.07F, 0.1925F, -0.06125F), new Vector3(350F, 120F, 5F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Turret", "LegBar2", new Vector3(-0.325F, 0.9125F, -0.03F), new Vector3(0F, 90F, 0F), new Vector3(0.35F, 0.35F, 0.35F));
                    _displaySettings.AddCharacterDisplay("Walker Turret", "LegBar2", new Vector3(-0.325F, 0.9125F, -0.03F), new Vector3(0F, 90F, 0F), new Vector3(0.35F, 0.35F, 0.35F));
                    _displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(-0.1495F, 0.08375F, 0.0695F), new Vector3(13.75F, 280F, 323.75F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Mercenary", "UpperArmL", new Vector3(-0.016F, 0.152F, -0.115F), new Vector3(345F, 165F, 20F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("REX", "CalfFrontL", new Vector3(-0.0725F, 0.525F, -0.025F), new Vector3(0F, 90F, 180F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(-0.1575F, 0.2625F, 0.4315F), new Vector3(0F, 0F, 0F), new Vector3(0.06F, 0.06F, 0.06F));
                    _displaySettings.AddCharacterDisplay("Acrid", "Jaw", new Vector3(0F, 2.4F, -0.525F), new Vector3(2.5F, 0F, 90F), new Vector3(1F, 1F, 1F));
                    _displaySettings.AddCharacterDisplay("Captain", "Stomach", new Vector3(0.235F, 0.13F, -0.24F), new Vector3(350.5F, 156.25F, 352.5F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Railgunner", "ThighL", new Vector3(0.09715F, 0.251F, 0.0013F), new Vector3(0F, 82.5F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(-0.2275F, 0.1825F, 0.07672F), new Vector3(33.75F, 144F, 293.75F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Seeker", "ThighR", new Vector3(0.01225F, 0.3335F, 0.096F), new Vector3(352.5F, 0F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(-0.39125F, 0.0768F, 0.19725F), new Vector3(7.75F, 317.5F, 20F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("Chef", "LowerArmR", new Vector3(0F, 0F, -0.065F), new Vector3(0F, 355F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Operator", "CalfL", new Vector3(-0.345F, -0.0014F, 0.105F), new Vector3(88.25F, 255F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Drifter", "BagBottom", new Vector3(-0.123F, 0.1985F, 0.3025F), new Vector3(337.95F, 299F, 30.5F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Technician", "Backpack", new Vector3(-0.29F, 0F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Technician", "Backpack", new Vector3(0.29F, 0F, 0F), new Vector3(0F, 180F, 270F), new Vector3(0.08F, 0.08F, 0.08F));
                });

            CreateItem(_name: "Service Start-up Drive",
                _pickup: "Gain access to a machine capsule each stage that contains a powerful drone.",
                _description: "A <style=cIsUtility>machine capsule</style> containing a drone (79%/<style=cIsHealing>20%</style>/<style=cIsHealth>1%</style>) will appear in a random location <style=cIsUtility>on each stage</style>. <style=cStack>(Increases rarity chances of the drone per stack).</style>",
                _lore: "Order: Maintenance Access Drive\r\nTracking Number: 44******\r\nEstimated Delivery: 08/03/2057\r\nShipping Method: Priority Internal\r\nShipping Address: |||||||, Base Camp Olympus, Mars\r\nShipping Details:\r\n\r\nYou didn't get this from me.\r\n\r\nUESC stopped using physical override keys on machine capsules after some genius decided a maintenance drone counted as a \u0022personal mobility device\u0022 and tried to expense one. Now the capsules only accept start-up authorization drives issued to licensed service staff.\r\n\r\nOfficially, this one is for diagnostics, firmware recovery, and limited deployment testing. Unofficially, the capsule does not seem to know the difference between \u0022test the drone\u0022 and \u0022hand the nearest contractor a heavily armed flying machine.\u0022\r\n\r\nIf anyone asks, you are performing routine maintenance. If the drone asks, you are its supervisor. If legal asks, I have never met you.",
                _tier: ItemTier.Tier2,
                _tags: [ItemTag.Utility, ItemTag.Technology, ItemTag.AIBlacklist, ItemTag.OnStageBeginEffect],
                _iconDir: "texservicestartupdriveicon",
                _modelDir: "servicestart-updrivemesh",
                _displayDir: "servicestart-updrivedisplaymesh",
                _displaySettingsCallback: _displaySettings =>
                {
                    // Add character display settings
                    _displaySettings.AddCharacterDisplay("Commando", "Stomach", new Vector3(0.168F, 0.0375F, -0.0775F), new Vector3(60F, 200F, 80F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Huntress", "Pelvis", new Vector3(-0.10625F, -0.0605F, 0.08875F), new Vector3(310F, 320F, 5F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Bandit", "Pelvis", new Vector3(0.185F, 0.00845F, -0.09F), new Vector3(310F, 50F, 260F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-2.135F, 1.275F, -1.16F), new Vector3(0F, 90F, 0F), new Vector3(0.5F, 0.5F, 0.5F));
                    _displaySettings.AddCharacterDisplay("Engineer", "ThighR", new Vector3(-0.1075F, 0.155F, -0.09F), new Vector3(275F, 180F, 235F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0.688F, -1.4785F), new Vector3(315F, 0F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(-0.72F, 1.0825F, -1.25375F), new Vector3(0F, 0F, 90F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.185F, 0.0835F, -0.21425F), new Vector3(279F, 0F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Mercenary", "ThighL", new Vector3(0.068F, 0.1615F, -0.087F), new Vector3(282.5F, 111.5F, 216F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("REX", "ClavicleL", new Vector3(0.14F, 0.555F, 0.0075F), new Vector3(18.5F, 270F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(0.1575F, 0.2665F, 0.4315F), new Vector3(271.25F, 0F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Acrid", "SpineChest1", new Vector3(-1.01F, 3.5925F, 3.1625F), new Vector3(40F, 47.5F, 35F), new Vector3(0.75F, 0.75F, 0.75F));
                    _displaySettings.AddCharacterDisplay("Captain", "HandL", new Vector3(-0.07125F, -0.0316F, 0.00415F), new Vector3(271F, 273F, 90F), new Vector3(0.05F, 0.05F, 0.05F));
                    _displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(-0.2595F, -0.0995F, 0.08F), new Vector3(90F, 267.5F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Void Fiend", "LargeExhaust2L", new Vector3(-0.00965F, -0.004F, -0.01425F), new Vector3(72.5F, 190F, 85F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.033F, 0.0256F, -0.4245F), new Vector3(16.75F, 343F, 314F), new Vector3(0.07F, 0.07F, 0.07F));
                    _displaySettings.AddCharacterDisplay("False Son", "LowerArmR", new Vector3(-0.17025F, 0.254F, -0.001F), new Vector3(277.5F, 45F, 36.5F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(-0.06275F, 0F, 0.04975F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Operator", "Backpack", new Vector3(0.1965F, -0.1005F, -0.1575F), new Vector3(0F, 270F, 90F), new Vector3(0.06F, 0.06F, 0.06F));
                    _displaySettings.AddCharacterDisplay("Drifter", "BagBottom", new Vector3(-0.192F, 0.0175F, 0.23F), new Vector3(295F, 35.5F, 75.5F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Technician", "UpperArmL", new Vector3(0F, 0.23825F, 0.1F), new Vector3(275F, 148.5F, 32F), new Vector3(0.09F, 0.09F, 0.09F));
                });

            CreateItem(_name: "Chakrams",
                _pickup: "Activating your Primary skill also throws a piercing chakram that returns to you. <style=cIsVoid>Corrupts all Shurikens</style>.",
                _description: "Activating your <style=cIsUtility>Primary skill</style> also throws a <style=cIsDamage>piercing chakram</style> that deals <style=cIsDamage>400%</style> <style=cStack>(+100% per stack)</style> base damage and returns to you. You can hold up to <style=cIsUtility>3</style> <style=cStack>(+1 per stack)</style> <style=cIsDamage>chakrams</style>. <style=cIsVoid>Corrupts all Shurikens</style>.",
                _lore: "Order: Golden Steel Chakrams\r\nTracking Number: 78*******\r\nEstimated Delivery: 06/16/2060\r\nShipping Method: Priority\r\nShipping Address: \u0022Secret Hideout\u0022, Earth\r\nShipping Details:\r\n\r\nShouldn't soar off to one side like the others did. Lemme know if there's anything else you want, we've got a new stock of some classics coming in next month by the time you read this. Nunchucks, nagamaki, kunai, all that classic stuff.\r\n\r\nAlso, please change your address. It looks stupid every time I see it.",
                _tier: ItemTier.VoidTier2,
                _tags: [ItemTag.Damage],
                _iconDir: "texchakramicon",
                _modelDir: "chakrammesh",
                _displayDir: "chakramdisplaymesh",
                _corruptToken: "ITEM_PRIMARYSKILLSHURIKEN_NAME",
                _displaySettingsCallback: _displaySettings =>
                {
                    // Add character display settings
                    _displaySettings.AddCharacterDisplay("Commando", "HandR", new Vector3(-0.0045F, 0.07375F, 0.0425F), new Vector3(50.5F, 262.5F, 268F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Huntress", "BowHinge2L", new Vector3(-0.042F, 0.37125F, 0.0075F), new Vector3(90F, 90F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Huntress", "BowHinge2R", new Vector3(-0.042F, 0.37125F, -0.0075F), new Vector3(90F, 90F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Bandit", "FootL", new Vector3(-0.034F, 0.0005F, 0.0003F), new Vector3(345.75F, 181F, 83.75F), new Vector3(0.0625F, 0.0625F, 0.0625F));
                    _displaySettings.AddCharacterDisplay("Bandit", "FootR", new Vector3(0.034F, 0.0005F, 0.0003F), new Vector3(14.25F, 359F, 263.25F), new Vector3(0.0625F, 0.0625F, 0.0625F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-2.507F, 1.905F, 0F), new Vector3(30F, 0F, 90F), new Vector3(1F, 1F, 1F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(2.507F, 1.905F, 0F), new Vector3(30F, 0F, 90F), new Vector3(1F, 1F, 1F));
                    _displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(-0.1393F, 0.384F, 0.1444F), new Vector3(60F, 225F, 270F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Artificer", "LowerArmL", new Vector3(0.02F, 0.317F, 0.014F), new Vector3(0F, 335F, 180F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("Artificer", "LowerArmR", new Vector3(0.02F, 0.317F, -0.014F), new Vector3(0F, 205F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
                    _displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0.169F, -0.221F), new Vector3(45F, 257F, 252F), new Vector3(0.3F, 0.3F, 0.3F));
                    _displaySettings.AddCharacterDisplay("REX", "AimOriginSyringe", new Vector3(0F, -0.104F, -0.97F), new Vector3(90F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Loader", "MechUpperArmL", new Vector3(0.09F, 0.07625F, -0.0035F), new Vector3(318.75F, 0F, 90F), new Vector3(0.175F, 0.175F, 0.175F));
                    _displaySettings.AddCharacterDisplay("Acrid", "LowerArmR", new Vector3(0.0875F, 2.65F, -0.1F), new Vector3(0F, 45F, 4.25F), new Vector3(1.75F, 1.75F, 1.75F));
                    _displaySettings.AddCharacterDisplay("Captain", "LowerArmL", new Vector3(0.00875F, 0.2F, 0F), new Vector3(350F, 75F, 182.5F), new Vector3(0.15F, 0.15F, 0.15F));
                    _displaySettings.AddCharacterDisplay("Railgunner", "CalfR", new Vector3(-0.014F, 0.138F, -0.1115F), new Vector3(310F, 87F, 281.75F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Void Fiend", "ForeArmR", new Vector3(0.29F, 0.1525F, 0.06F), new Vector3(77.5F, 190F, 216.25F), new Vector3(0.11F, 0.11F, 0.11F));
                    _displaySettings.AddCharacterDisplay("Seeker", "HandR", new Vector3(-0.005F, 0.09F, 0.045F), new Vector3(334F, 72F, 95F), new Vector3(0.0625F, 0.0625F, 0.0625F));
                    _displaySettings.AddCharacterDisplay("False Son", "HandR", new Vector3(0.0033F, 0.09F, 0.0525F), new Vector3(338.75F, 82.5F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Chef", "HandR", new Vector3(-0.09775F, -0.01385F, 0.0575F), new Vector3(60F, 107.5F, 110F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Operator", "HandR", new Vector3(0.0635F, 0F, 0F), new Vector3(270F, 88.75F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Drifter", "HandR", new Vector3(-0.0845F, -0.01505F, -0.09925F), new Vector3(342F, 11.65F, 109.5F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Technician", "Shin.L", new Vector3(0.08325F, 0.1225F, 0F), new Vector3(296.75F, 187.5F, 261.5F), new Vector3(0.055F, 0.055F, 0.055F));
                });
        }

        public static void CreateItem(bool _createDisplaySettings = true, string _name = "WIP ITEM", ItemTag[] _tags = default, 
            string _iconDir = "texTemporalCubeIcon", string _modelDir = "temporalcubemesh", string _displayDir = "temporalcubemesh", 
            ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null,
            ItemDisplayCallback _displaySettingsCallback = null, ModifyPrefabCallback _modifyItemModelPrefabCallback = null,
            ModifyPrefabCallback _modifyItemDisplayPrefabCallback = null, bool _canNeverBeTemporary = false, bool _debugOnly = false,
            string _pickup = null, string _description = null, string _lore = null)
        {
            // Don't create item is WIP content is disabled
            if (!Utils.WIPContentEnabled) return;

            // Create WIP item and add to list
            WIPItem wipItem = new(_createDisplaySettings, _name, _tags, _iconDir, _modelDir, _displayDir, _tier, _simulacrumBanned, _canRemove, _hidden,
                _corruptToken, _displaySettingsCallback, _modifyItemModelPrefabCallback, _modifyItemDisplayPrefabCallback, _canNeverBeTemporary,
                _debugOnly, _pickup, _description, _lore);
            wipItems.Add(wipItem);
        }
    }
}
