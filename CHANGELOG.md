### v1.4.2

**Balance**
- Increased Drowned Visage chance and chance stacking from 2.5% to 5%.
- Drowned Visage now charges the teleporter by 10% instead of 5% after large or elite kills.

**Visuals**
- Added dithering to Shrine of Recollection.

### v1.4.1

**Compatibility**
- Fixed a compatibility issue between v1.4.0 and Hypercrit2.

**Fixes**
- Fixed minor missing language tokens for buffs.

## v1.4.0

**Added**
- Added 3 new items: Matter Accelerator, Radiant Timepiece, and Appraiser's Eye.

**Changed**
- Collector's Vision now awards Appraiser's Eye at 100% Inspiration (can be disabled via config).
- Increased Vengeful Toaster's buff duration stacking from 1s to 2s.
- Updated Shrine of Recollection interaction text to better match the current Inspiration buff.
- Second Hand, Copper Gear, and Brass Screws buffs no longer visually stack, to better align with the base game.

**Visuals**
- Improved the Jetpack item display for Seeker.
- Changed the Inspiration buff icon.

**Fixes**
- Fixed some personalisation settings randomising in Randomizer Mode.

**Note**
- Config adjustment or reset required for the Vengeful Toaster duration stacking change.

### v1.3.27

**Changed**
- Targeting Matrix now shows through terrain to make the target easier to find (can be disabled via config).
- Jetpack now shows a fuel remaining buff (can be disabled via config).
- Jetpack now mitigates fall damage while in use (can be disabled via config).

### v1.3.26

**Changed**
- Updated items to include newly introduced item tags.
- Replaced item lore quotes with quotes more faithful to the base game.

### v1.3.25

**Config**
- Added config options for changing item pickups and descriptions.

**Fixes**
- Fixed mod managers flattening Faithful's folder structure.

### v1.3.24

**Fixes**
- Fixed language files not being found when installation via mod managers altered file paths.

### v1.3.23

**Localization**
- Added Russian and Simplified Chinese locales.

**Changed**
- Changed Collector's Vision settings, description, and buff to be percent-based and tied to additional critical chance.

### v1.3.22

**Added**
- Added a WIP content setting that allows WIP Faithful content to load into the game (default off).

**Visuals**
- Improved Shrine of Recollection visuals.

**Fixes**
- Fixed buff count not updating correctly inside the teleporter zone for Copper Gear and Brass Screws.
- Fixed item displays no longer appearing on Engineer's alternate Turrets.

### v1.3.21

**Visuals**
- Improved the visuals of many icons, pickup models, and display models through better use of in-game shaders.

### v1.3.20

**Changed**
- Targeting Matrix has been redesigned as a green item by popular request.

**Fixes**
- Fixed Operator's alternate secondary not being affected by Targeting Matrix.

### v1.3.19

**Config**
- Added a Collector's Vision config option for counting temporary items (default on).

### v1.3.18

**Changed**
- Added additional Shrine of Recollection spawn positions for new Alloyed Collective stages.
- Disabled void items no longer receive a corruption pair.

### v1.3.17

**Changed**
- Removed the annoying "PING" logs left in after debugging. Whoops.

### v1.3.16

**Visuals**
- Added missing item displays for Alloyed Collective.

<details><summary><strong>Old Changelogs</strong></summary>

### v1.3.15

**Compatibility**
- Faithful is now functional with Alloyed Collective.

### v1.3.14

**Fixes**
- Railgunner's Concussion Device no longer grants vengeance.

### v1.3.13

**Balance**
- Spacious Umbrella has been buffed with reduced stacking falloff.

### v1.3.12

**Changed**
- Changed item and buff internal names to be more robust for future locale support and to avoid mod conflicts.

### v1.3.11

**Fixes**
- Vengeful Toaster and Hermit's Shawl no longer waste their associated buffs on damage events that do no actual damage.

### v1.3.10

**Config**
- Separated verbose console output into its own setting.

**Fixes**
- Fixed some assets needlessly trying to become networked by the Prefab API.

### v1.3.9

**Added**
- Added an optional radius indicator for Longshot Geode (default off).

### v1.3.8

**Config**
- Added additional configs for Leader's Pennon attack speed and regeneration stacking.

### v1.3.7

**Fixes**
- Fixed the broken Jetpack material.
- Switched some asset fetch operations to use GUIDs.

### v1.3.6

**Compatibility**
- Updated Faithful to work with the new "Memory Optimisation Update".

### v1.3.5

**Fixes**
- Fixed an issue with the Carbonizer laser sometimes disappearing. Oops.

### v1.3.4

**Fixes**
- Fixed a minor error with Hermit's Shawl when some modded characters are destroyed.

### v1.3.3

**Fixes**
- Fixed an error with Targeting Matrix on modded characters with no model locator.

### v1.3.2

**Changed**
- Slightly buffed 4-T0N Jetpack.
- Added pickup spawning tools to the debug UI.

### v1.3.1

**Fixes**
- Made the code for finding item corruptions more robust.

## v1.3.0

**Added**
- Added the Collector's Vision interactable: Shrine of Recollection.

### v1.2.15

**Changed**
- Refined the debug UI to better match the rest of the game.

### v1.2.14

**Config**
- Added configs for overriding item names.

**Changed**
- Changed the Leader's Pennon description and details to make its use clearer.

### v1.2.13

**Fixes**
- Faithful no longer requires Survivors of the Void to be enabled.

### v1.2.12

**Config**
- Added configs for overriding corrupted items for void items.

### v1.2.11

**Fixes**
- Fixed various issues by switching to overlay language tokens from the Language API.

### v1.2.10

**Compatibility**
- LookingGlass now only overrides the extended item descriptions config when full pickup descriptions is enabled.

### v1.2.9

**Compatibility**
- LookingGlass now overrides the extended item descriptions config.

### v1.2.8

**Compatibility**
- Added Risk of Options compatibility.

### v1.2.7

**Visuals**
- Added all missing item displays for Engineer's turrets.

### v1.2.6

**Config**
- Added a separate config value for Hermit's Shawl max buff stacking.

### v1.2.5

**Changed**
- Made 4-T0N Jetpack stronger over a shorter duration.
- Decreased fuel time from 4s to 3s.
- Decreased fuel time stacking from 2s to 1.5s.
- Decreased recharge time from 12s to 8s.
- 4-T0N Jetpack is now affected by jump power increasing items.

### v1.2.4

**Changed**
- Noxious Slimes now has a small chance to blight enemies on hit.

### v1.2.3

**Balance**
- Decreased Longshot Geode's range condition from 50m to 40m.
- Increased Leader's Pennon radius stacking from 5m to 7.5m.
- Increased Melting Warbler's jump height bonus from 1m to 2m.
- Vengeance from Vengeful Toaster is no longer removed by damage-over-time effects.

**Note**
- Balance changes require a config reset or manual config adjustment to apply.

### v1.2.2

**Fixes**
- Fixed Hermit's Shawl being left as debug only. Oops.

### v1.2.1

**Config**
- Added override configs for all items, including disabling all items, disabling all item displays, and enabling extended pickup descriptions with one config change.

## v1.2.0

**Added**
- Added the Hermit's Shawl item.

### v1.1.6

**Fixes**
- Fixed rogue config entries for debug items (delete old config to clean).

### v1.1.5

**Added**
- The debug spawn menu can now spawn elites.
- The debug spawn menu can now modify the power of spawned characters.

### v1.1.4

**Fixes**
- Targeting Matrix can no longer target Healing Cores.

### v1.1.3

**Changed**
- Leader's Pennon, Copper Gear, and Brass Screws buff durations no longer get randomised in Randomizer Mode.
- Leader's Pennon hidden regeneration multiplier buff no longer gets randomised in Randomizer Mode.

### v1.1.2

**Fixes**
- Fixed NRE errors that sometimes occurred when dying with Targeting Matrix.

### v1.1.1

**Added**
- Added an animation for Targeting Matrix activation and deactivation.

## v1.1.0

**Changed**
- Finalised adjustments for Targeting Matrix.

### v1.0.21

**Added**
- Added the Targeting Matrix item.
- Added a unique visual effect for Leader's Pennon.

**Fixes**
- Fixed some minor config issues.

### v1.0.20

**Changed**
- Improved the mod changelog.

### v1.0.19

**Added**
- Added an option to randomise the stats of all items added by this mod (disabled by default).

**Fixes**
- Fixed Leader's Pennon radius indicator persisting after the owning player dies.
- Fixed multiple issues when some players in a multiplayer lobby have godmode enabled while others do not.

### v1.0.18

**Changed**
- The host's config now gets passed to clients who use it while in the host's game.

### v1.0.17

**Fixes**
- Fixed an inconsistency in the README.

### v1.0.16

**Config**
- Melting Warbler and Collector's Vision can now be customised in the config.

### v1.0.15

**Config**
- Longshot Geode and Leader's Pennon can now be customised in the config.

### v1.0.14

**Added**
- Added alternate item pickup descriptions for adjustments that make the original descriptions inaccurate.

### v1.0.13

**Config**
- Hastening Greave and Cauterizing Greave can now be customised in the config.

### v1.0.12

**Changed**
- Changed Faithful item placements so they appear in their proper positions in the logbook and command menus.

### v1.0.11

**Config**
- Noxious Slimes can now be adjusted in the config.

### v1.0.10

**Changed**
- Adjusted 4-T0N Jetpack to make it "bouncier".

**Config**
- Vengeful Toaster, Second Hand, and 4-T0N Jetpack can now be customised in the config.

### v1.0.9

**Fixes**
- Fixed Copper Gear and Brass Screw sometimes not providing their buffs inside the teleporter zone.

**Config**
- Copper Gear and Brass Screw can now be customised in the config.

### v1.0.8

**Config**
- Spacious Umbrella and Drowned Visage can now be customised in the config.

### v1.0.7

**Added**
- Added Faithful content as its own expansion that can be enabled and disabled in the lobby.

### v1.0.6

**Changed**
- Replaced the config text file with a BepInEx config.

### v1.0.5

**Fixes**
- Fixed the warbanner temporary visual effect getting yeeted. Oops.

### v1.0.4

**Fixes**
- Fixed errors with debugging tools when modded spawn cards are created.

### v1.0.3

**Fixes**
- Fixed the README for the store page.

### v1.0.2

**Fixes**
- Fixed a minor version mismatch issue.

### v1.0.1

**Changed**
- Forgor smth...

## v1.0.0

**Release**
- Praying I didn't forget anything...

</details>
