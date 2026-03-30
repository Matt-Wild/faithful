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

            CreateItem(_name: "Plasma Field Generator",
                _pickup: "Gain a shield and reduce its recharge time. <style=cIsVoid>Corrupts all Personal Shield Generators</style>.",
                _description: "Gain a <style=cIsHealing>shield</style> equal to <style=cIsHealing>4%</style> <style=cStack>(+4% per stack)</style> of your maximum health and reduce its <style=cIsUtility>recharge time</style> by <style=cIsUtility>5%</style> <style=cStack>(+5% per stack)</style>. Recharges outside of danger. <style=cIsVoid>Corrupts all Personal Shield Generators</style>.",
                _lore: "<style=cMono>\r//--AUTO-TRANSCRIPTION FROM CARGO BAY 4 OF UES [Redacted] --//</style>\r\n\r\n\u0022...The heck's up with all these defective shield devices? ...Hinon? I thought the stuff they made was alright.\u0022\r\n\r\n\u0022These things? I wouldn't try any on if I were you. Defective shield devices are no fun to mess with in the first place but these ones... best not even touch them without equipment just to be safe.\u0022\r\n\r\n\u0022Yeesh. How'd they manage to mess up all of these?\u0022\r\n\r\n\u0022If I recall correctly, they went straight to human testing immediately with this particular model since they were so confident and... uh... it couldn't have gone worse, I'd say.\u0022\r\n\r\n\u0022Right. You said you used to work there, didn't you? What happened?\u0022\r\n\r\n\u0022They put it on the first guy and BOOM! He's gone the second they turned the thing on. Then the device just fell to the floor and deactivated, apparently.\u0022\r\n\r\n\u0022Gone? Like, 'gone' gone or...?\u0022\r\n\r\n\u0022Just... poof. They thought they'd teleported him somehow. It took them an embarrassingly long time to figure that they'd simply vaporized the poor guy on the spot. Given not a trace of him was left.\u0022\r\n\r\n\u0022Holy [REDACTED]. How are they still in business?\u0022\r\n\r\n\u0022Oh, yeah, nobody ever got put away for the incident. Heck, nobody even got fired! They couldn't exactly maintain their image if word got out so they tried covering up the whole thing.\u0022\r\n\r\n\u0022...You're kidding. Sounds like you'd be better off working clean-up on the Contact Light than being there.\u0022\r\n\r\n\u0022You're telling me. I quit on the spot when the internal report reached my department.\u0022\r\n\r\n\u0022...Don't tell the captain I said that by the way.\u0022\r\n\r\n\u0022Trust me, buddy. My lips are sealed.\u0022",
                _tier: ItemTier.VoidTier1,
                _tags: [ItemTag.Utility, ItemTag.Technology],
                _iconDir: "texplasmafieldgeneratoricon",
                _modelDir: "plasmafieldgeneratormesh",
                _displayDir: "plasmafieldgeneratordisplaymesh",
                _corruptToken: "ITEM_PERSONALSHIELD_NAME",
                _displaySettingsCallback: _displaySettings =>
                {
                    // Add character display settings
                    _displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0.315F, 0.184F), new Vector3(326.75F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(-0.001F, 0.1715F, 0.1635F), new Vector3(330F, 0F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Bandit", "UpperArmR", new Vector3(-0.00115F, 0.2015F, -0.092F), new Vector3(345F, 180F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-0.05F, 1.2325F, 3.325F), new Vector3(0F, 0F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
                    _displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.27862F, 0.24375F), new Vector3(355F, 0F, 180F), new Vector3(0.125F, 0.125F, 0.125F));
                    _displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, 0.65375F, -0.275F), new Vector3(0F, 180F, 180F), new Vector3(0.325F, 0.325F, 0.325F));
                    _displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.61625F, -1.1275F), new Vector3(21.25F, 180F, 180F), new Vector3(0.325F, 0.325F, 0.325F));
                    _displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0F, 0.1955F, 0.1285F), new Vector3(332.5F, 0F, 0F), new Vector3(0.065F, 0.065F, 0.065F));
                    _displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0.175F, 0.175F), new Vector3(336.25F, 0F, 0F), new Vector3(0.09F, 0.09F, 0.09F));
                    _displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0.61F, -0.19F, 0.149F), new Vector3(0F, 78.75F, 0F), new Vector3(0.1125F, 0.1125F, 0.1125F));
                    _displaySettings.AddCharacterDisplay("Loader", "Chest", new Vector3(0F, 0.19925F, 0.182F), new Vector3(333.75F, 0F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
                    _displaySettings.AddCharacterDisplay("Acrid", "ClavicleR", new Vector3(1.005F, 1.925F, 1.6F), new Vector3(326.75F, 45F, 92.5F), new Vector3(0.8F, 0.8F, 0.8F));
                    _displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0.2285F, 0.204F), new Vector3(349.5F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
                    _displaySettings.AddCharacterDisplay("Railgunner", "UpperArmL", new Vector3(0.047F, 0.129F, -0.019F), new Vector3(0F, 105F, 0F), new Vector3(0.0625F, 0.0625F, 0.0625F));
                    _displaySettings.AddCharacterDisplay("Void Fiend", "UpperArmR", new Vector3(0.196F, -0.0585F, -0.0075F), new Vector3(15F, 103.375F, 345F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0.06125F, 0.085F), new Vector3(350F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(0F, 0.4175F, 0.17625F), new Vector3(322.5F, 0F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(-0.28375F, 0.22775F, 0F), new Vector3(280F, 270F, 180F), new Vector3(0.09F, 0.09F, 0.09F));
                    _displaySettings.AddCharacterDisplay("Operator", "Chest", new Vector3(-0.01F, -0.1025F, -0.08575F), new Vector3(67.5F, 158.75F, 270F), new Vector3(0.04F, 0.04F, 0.04F));
                    _displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(-0.0145F, 0.285F, 0F), new Vector3(297.5F, 257.5F, 195F), new Vector3(0.1F, 0.1F, 0.1F));
                    _displaySettings.AddCharacterDisplay("Scavenger", "Head", new Vector3(-2.5375F, 2.05F, -5.725F), new Vector3(302.5F, 206.25F, 157.5F), new Vector3(1.2F, 1.2F, 1.2F));
                    _displaySettings.AddCharacterDisplay("Technician", "Chest", new Vector3(0F, 0.115F, 0.145F), new Vector3(3.75F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
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
                    _displaySettings.AddCharacterDisplay("Huntress", "BowHinge2L", new Vector3(-0.042F, 0.37125F, 0.0075F), new Vector3(90F, 270F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
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
