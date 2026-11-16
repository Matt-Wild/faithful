using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class DebugCharacterLoadouts : MonoBehaviour
    {
        // Store the selected character and entry list
        CharacterBody character;
        DebugScrollList entryList;

        // Store the loadout saving controls
        TMP_InputField nameInput;
        UnityEngine.UI.Button saveButton;
        UnityEngine.UI.Toggle ignoreBlacklistToggle;

        // Store saved loadouts and their file path
        LoadoutFile saved = new();
        string savePath;
        bool savingAvailable = true;

        // Items and buffs excluded from loadouts unless the blacklist is ignored
        readonly HashSet<ItemDef> itemBlacklist =
        [
            RoR2Content.Items.CaptainDefenseMatrix,
            DLC1Content.Items.ConvertCritChanceToCritDamage,
            DLC1Content.Items.VoidmanPassiveItem,
            DLC3Content.Items.StatsFromScrap,
            DLC3Content.Items.Junk,
            DLC4Content.Items.PassiveBigMan,
            DLC4Content.Items.StealBackHealth,
            DLC4Content.Items.ConvertHealingToAttackSpeed,
            RoR2Content.Items.DrizzlePlayerHelper,
            RoR2Content.Items.MonsoonPlayerHelper,
            RoR2Content.Items.BoostEquipmentRecharge,
            RoR2Content.Items.UseAmbientLevel,
            RoR2Content.Items.MinionLeash,
            RoR2Content.Items.BoostDamage,
            RoR2Content.Items.BoostHp,
            RoR2Content.Items.CutHp,
            RoR2Content.Items.AdaptiveArmor,
            RoR2Content.Items.Karma
        ];

        readonly HashSet<BuffDef> buffBlacklist =
        [
            DLC2Content.Buffs.ChakraBuff,
            DLC2Content.Buffs.RevitalizeBuff,
            DLC2Content.Buffs.TamperingASAndArmorBonus,
            DLC2Content.Buffs.TamperingMSAndHRBonus,
            DLC3Content.Buffs.TrashToTreasureWhite,
            DLC3Content.Buffs.TrashToTreasureGreen,
            DLC3Content.Buffs.TrashToTreasureRed,
            DLC3Content.Buffs.TrashToTreasureYellow
        ];

        [Serializable]
        internal class LoadoutFile
        {
            public List<Loadout> loadouts = [];
        }

        [Serializable]
        internal class Loadout
        {
            public string name;
            public List<SavedItem> items = [];
            public List<SavedBuff> buffs = [];
        }

        [Serializable]
        internal class SavedItem
        {
            public string name;
            public PickupModifier modifier;
            public int count;
        }

        [Serializable]
        internal class SavedBuff
        {
            public string name;
            public int count;
        }

        public bool Init()
        {
            // Load saved loadouts
            ReadLoadouts();

            // Find the scroll view, entry prefab and controls
            UnityEngine.UI.ScrollRect scrollRect = Utils.FindChildByName(transform, "Entries")?.GetComponent<UnityEngine.UI.ScrollRect>();
            GameObject prefab = Assets.GetObject("DebugCharacterLoadoutEntry");
            nameInput = Utils.FindChildByName(transform, "LoadoutNameInputField")?.GetComponent<TMP_InputField>();
            saveButton = Utils.FindChildByName(transform, "LoadoutSaveButton")?.GetComponent<UnityEngine.UI.Button>();
            ignoreBlacklistToggle = Utils.FindChildByName(transform, "IgnoreBlacklistToggle")?.GetComponent<UnityEngine.UI.Toggle>();
            if (scrollRect == null || prefab == null || nameInput == null || saveButton == null || ignoreBlacklistToggle == null)
            {
                Log.Error("[DEBUG MENU] | Character loadout controls are missing. Rebuild the asset bundle with the character tools and loadout entry prefabs.");
                return false;
            }

            // Add control behaviour
            nameInput.onValueChanged.AddListener(_ => UpdateControls());
            saveButton.onClick.AddListener(OnSaveClicked);

            // Create the reusable entry list
            entryList = scrollRect.gameObject.AddComponent<DebugScrollList>();
            entryList.Init(scrollRect, (RectTransform)prefab.transform, CreateEntry, BindEntry);
            RefreshEntries();

            return true;
        }

        public void SetCharacter(CharacterBody _character)
        {
            // Update the selected character and entry controls
            character = _character;
            entryList?.RefreshItems(saved.loadouts.Count);
            UpdateControls();
        }

        private void Update()
        {
            UpdateControls();
        }

        internal bool CanUseCharacter()
        {
            // Saving and applying require the host
            return NetworkServer.active && character != null && character.inventory != null && character.healthComponent != null && character.healthComponent.alive;
        }

        private bool IsBlacklisted(ItemDef _item)
        {
            return !ignoreBlacklistToggle.isOn && itemBlacklist.Contains(_item);
        }

        private bool IsBlacklisted(BuffDef _buff)
        {
            return !ignoreBlacklistToggle.isOn && buffBlacklist.Contains(_buff);
        }

        private bool IsNameAvailable(string _name)
        {
            return !string.IsNullOrWhiteSpace(_name) && !saved.loadouts.Exists(entry => string.Equals(entry.name, _name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private void UpdateControls()
        {
            if (saveButton != null) saveButton.interactable = savingAvailable && CanUseCharacter() && IsNameAvailable(nameInput.text);
        }

        private void ReadLoadouts()
        {
            // Use the same directory as progress saving, shared between profiles
            savePath = Path.Combine(FaithfulProgressSaving.GetSaveDirectory(), "DebugCharacterLoadouts.json");
            try
            {
                if (!File.Exists(savePath)) return;

                saved = JsonUtility.FromJson<LoadoutFile>(File.ReadAllText(savePath));

                if (saved?.loadouts == null) throw new InvalidDataException("Missing loadout list.");
            }
            catch (Exception exception)
            {
                // Disable saving if the existing file could not be read
                saved = new();
                savingAvailable = false;
                Log.Error($"[DEBUG MENU] | Could not read loadouts: {exception.Message}");
            }
        }

        private bool WriteLoadouts(List<Loadout> _loadouts)
        {
            if (!savingAvailable) return false;

            try
            {
                // Write the updated list before replacing the current file
                LoadoutFile data = new() { loadouts = _loadouts };
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                string temporaryPath = savePath + ".tmp";
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));

                if (File.Exists(savePath)) File.Replace(temporaryPath, savePath, null);
                else File.Move(temporaryPath, savePath);

                // Update the displayed list
                saved = data;
                RefreshEntries();

                return true;
            }
            catch (Exception exception)
            {
                Log.Error($"[DEBUG MENU] | Could not save loadouts: {exception.Message}");
                return false;
            }
        }

        private void OnSaveClicked()
        {
            if (!savingAvailable || !CanUseCharacter() || !IsNameAvailable(nameInput.text)) return;

            // Save the current character's items and their modifiers
            Loadout loadout = new() { name = nameInput.text.Trim() };
            Inventory inventory = character.inventory;
            for (int i = 0; i < ItemCatalog.itemCount; i++)
            {
                ItemDef item = ItemCatalog.GetItemDef((ItemIndex)i);

                if (item == null || IsBlacklisted(item)) continue;

                int normal = (int)Math.Min(int.MaxValue, (long)inventory.GetItemCountPermanent(item.itemIndex) + Mathf.Max(0, inventory.GetItemCountChanneled(item.itemIndex)));
                AddItem(loadout, item, normal, PickupModifier.Normal);
                AddItem(loadout, item, inventory.GetItemCountTemp(item.itemIndex), PickupModifier.Temporary);
                AddItem(loadout, item, inventory.GetItemCountRented(item.itemIndex), PickupModifier.Ethereal);
            }

            // Save non-timed buffs
            int[] normalCounts = new int[BuffCatalog.buffCount];
            int[] timedCounts = new int[BuffCatalog.buffCount];
            Utils.GetCharacterBuffCounts(character, normalCounts, timedCounts);
            for (int i = 0; i < normalCounts.Length; i++)
            {
                BuffDef buff = BuffCatalog.GetBuffDef((BuffIndex)i);

                if (buff != null && !IsBlacklisted(buff) && normalCounts[i] > 0) loadout.buffs.Add(new SavedBuff { name = buff.name, count = normalCounts[i] });
            }

            // Add the loadout to the saved list
            List<Loadout> updated = [.. saved.loadouts, loadout];

            if (WriteLoadouts(updated)) nameInput.SetTextWithoutNotify("");

            UpdateControls();
        }

        private static void AddItem(Loadout _loadout, ItemDef _item, int _count, PickupModifier _modifier)
        {
            if (_count > 0) _loadout.items.Add(new SavedItem { name = _item.name, count = _count, modifier = _modifier });
        }

        internal void Load(Loadout _loadout)
        {
            if (!CanUseCharacter() || !saved.loadouts.Contains(_loadout)) return;

            // Find available items and buffs, keeping missing content in the saved loadout
            List<(PickupDef pickup, SavedItem item)> items = [];
            List<(BuffDef buff, SavedBuff entry)> buffs = [];
            foreach (SavedItem item in _loadout.items)
            {
                ItemIndex index = ItemCatalog.FindItemIndex(item.name);
                PickupDef pickup = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(index));

                if (index == ItemIndex.None || pickup == null || IsBlacklisted(ItemCatalog.GetItemDef(index))) continue;

                items.Add((pickup, item));
            }
            foreach (SavedBuff entry in _loadout.buffs)
            {
                BuffDef buff = BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex(entry.name));

                if (buff == null || IsBlacklisted(buff)) continue;

                buffs.Add((buff, entry));
            }

            // Replace the selected character's items, keeping blacklisted items
            CharacterBody target = character;
            Inventory inventory = target.inventory;
            inventory.InventoryChangedHandler(_ =>
            {
                for (int i = 0; i < ItemCatalog.itemCount; i++)
                {
                    PickupDef pickup = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex((ItemIndex)i));

                    if (pickup == null || IsBlacklisted(ItemCatalog.GetItemDef((ItemIndex)i))) continue;

                    Utils.RemovePickup(inventory, pickup, PickupModifier.Normal, true);
                    Utils.RemovePickup(inventory, pickup, PickupModifier.Temporary, true);
                    Utils.RemovePickup(inventory, pickup, PickupModifier.Ethereal, true);
                }

                foreach ((PickupDef pickup, SavedItem item) in items)
                {
                    Utils.GivePickup(inventory, pickup, item.count, item.modifier);
                }
            });

            // Replace non-timed buffs, keeping blacklisted buffs
            for (int i = 0; i < BuffCatalog.buffCount; i++)
            {
                BuffDef buff = BuffCatalog.GetBuffDef((BuffIndex)i);

                if (buff == null || IsBlacklisted(buff)) continue;

                Utils.RemoveBuff(target, buff, false, true);
            }

            foreach ((BuffDef buff, SavedBuff entry) in buffs)
            {
                Utils.GiveBuff(target, buff, entry.count);
            }
        }

        internal void Delete(Loadout _loadout)
        {
            if (!savingAvailable || !saved.loadouts.Contains(_loadout)) return;

            // Remove the loadout from the saved list
            List<Loadout> updated = [.. saved.loadouts];
            updated.Remove(_loadout);

            WriteLoadouts(updated);
        }

        private void RefreshEntries()
        {
            // Sort loadouts by name and refresh the entries
            saved.loadouts.Sort((left, right) => StringComparer.CurrentCultureIgnoreCase.Compare(left.name, right.name));
            entryList?.RefreshItems(saved.loadouts.Count, true);

            UpdateControls();
        }

        private void CreateEntry(GameObject _row)
        {
            Utils.ApplyFonts(_row.transform);
            entryList.AddNavigation(_row);
        }

        private void BindEntry(GameObject _row, int _index)
        {
            // Update the entry title
            Loadout loadout = saved.loadouts[_index];
            TextMeshProUGUI title = Utils.FindChildWithTerm(_row.transform, "NormalTitle").GetComponent<TextMeshProUGUI>();
            title.richText = false;
            title.text = loadout.name;

            // Update the load button
            UnityEngine.UI.Button loadButton = _row.transform.Find("LoadButton").GetComponent<UnityEngine.UI.Button>();
            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => Load(loadout));
            loadButton.interactable = CanUseCharacter();

            // Update the delete button
            UnityEngine.UI.Button deleteButton = _row.transform.Find("DeleteButton").GetComponent<UnityEngine.UI.Button>();
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => Delete(loadout));
            deleteButton.interactable = savingAvailable;
        }
    }
}
