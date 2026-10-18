using RoR2;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class DebugCharacterInventory : MonoBehaviour
    {
        // Store the current character and the inventory being watched
        CharacterBody character;
        Inventory watchedInventory;

        // Store the reusable entry list and pickup quantities
        DebugScrollList entryList;
        Dictionary<(PickupIndex pickupIndex, PickupModifier type), int> quantities = new Dictionary<(PickupIndex, PickupModifier), int>();
        List<(PickupDef pickup, PickupModifier type)> pickups = new List<(PickupDef, PickupModifier)>();
        Dictionary<PickupIndex, string> pickupNames = new Dictionary<PickupIndex, string>();

        // Store the pickup giving controls and their selection values
        TMP_InputField quantityInput;
        TMP_Dropdown pickupDropdown;
        TMP_Dropdown modifierDropdown;
        UnityEngine.UI.Button giveButton;
        UnityEngine.UI.Button clearButton;
        Dictionary<string, PickupDef> pickupLookup = new Dictionary<string, PickupDef>();
        PickupModifier[] pickupModifiers = [PickupModifier.Normal, PickupModifier.Temporary, PickupModifier.Ethereal];

        // Coalesce inventory changes and reset scrolling when the target changes
        bool inventoryDirty = true;
        bool resetPosition = true;

        public bool Init()
        {
            // Find the existing scroll view and pickup entry template
            UnityEngine.UI.ScrollRect scrollRect = Utils.FindChildByName(transform, "Entries").GetComponent<UnityEngine.UI.ScrollRect>();
            GameObject entryPrefab = Assets.GetObject("DebugCharacterPickupEntry");
            if (scrollRect == null || entryPrefab == null)
            {
                Log.Error("[DEBUG MENU] | Character inventory requires the Entries scroll view and DebugCharacterPickupEntry prefab.");
                return false;
            }

            // Find the inventory controls added to the character tools prefab
            quantityInput = Utils.FindChildByName(transform, "ItemQuantityInputField")?.GetComponent<TMP_InputField>();
            pickupDropdown = Utils.FindChildByName(transform, "PickupSelectionDropdown")?.GetComponent<TMP_Dropdown>();
            modifierDropdown = Utils.FindChildByName(transform, "PickupModifierDropdown")?.GetComponent<TMP_Dropdown>();
            giveButton = Utils.FindChildByName(transform, "GivePickupsButton")?.GetComponent<UnityEngine.UI.Button>();
            clearButton = Utils.FindChildByName(transform, "ClearInventoryButton")?.GetComponent<UnityEngine.UI.Button>();
            if (quantityInput == null || pickupDropdown == null || modifierDropdown == null || giveButton == null || clearButton == null)
            {
                Log.Error("[DEBUG MENU] | Character inventory controls are missing. Rebuild the asset bundle with the updated character tools prefab.");
                return false;
            }

            // Use the spawn menu's pickup labels and pooled selection for inventory pickups
            pickupDropdown = DebugPickupSelection.Create(pickupDropdown, pickupLookup, pickup => pickup.itemIndex != ItemIndex.None || pickup.equipmentIndex != EquipmentIndex.None || pickup.keyItemIndex != KeyItemIndex.None);
            modifierDropdown.ClearOptions();
            modifierDropdown.AddOptions(new List<string> { "None", "Temporary", "Ethereal" });
            modifierDropdown.SetValueWithoutNotify(0);

            // Apply pickup changes only when a button is pressed
            quantityInput.onEndEdit.AddListener(OnQuantityChanged);
            pickupDropdown.onValueChanged.AddListener(OnPickupSelected);
            giveButton.onClick.AddListener(OnGivePickupsClicked);
            clearButton.onClick.AddListener(OnClearInventoryClicked);
            OnQuantityChanged(quantityInput.text);

            // Only create enough entries to cover the visible portion of the inventory
            entryList = scrollRect.gameObject.AddComponent<DebugScrollList>();
            entryList.Init(scrollRect, (RectTransform)entryPrefab.transform, CreateEntry, BindEntry);
            UpdateControls();
            return true;
        }

        public void SetCharacter(CharacterBody _character)
        {
            // Replace the inventory subscription when the selected character changes
            if (ReferenceEquals(character, _character)) return;
            character = _character;
            resetPosition = true;
            if (isActiveAndEnabled) WatchInventory();
        }

        private void OnEnable()
        {
            // Refresh changes made while the inventory panel was closed
            WatchInventory();
        }

        private void OnDisable()
        {
            // Stop watching inventories while the panel is hidden
            StopWatchingInventory();
        }

        private void WatchInventory()
        {
            // Watch only the current target's inventory
            StopWatchingInventory();
            watchedInventory = character != null ? character.inventory : null;
            if (watchedInventory != null) watchedInventory.onInventoryChanged += OnInventoryChanged;
            inventoryDirty = true;
        }

        private void StopWatchingInventory()
        {
            // Remove the managed callback even if Unity has already destroyed the inventory
            if (!ReferenceEquals(watchedInventory, null)) watchedInventory.onInventoryChanged -= OnInventoryChanged;
            watchedInventory = null;
        }

        private void OnInventoryChanged()
        {
            // Rebuild once after all inventory changes for this frame have completed
            inventoryDirty = true;
        }

        private void LateUpdate()
        {
            // Also handle the character receiving a different inventory object
            Inventory currentInventory = character != null ? character.inventory : null;
            if (!ReferenceEquals(currentInventory, watchedInventory)) WatchInventory();
            if (entryList != null) UpdateControls();
            if (!inventoryDirty || entryList == null) return;

            // Refresh entries only when the inventory actually changes
            inventoryDirty = false;
            RefreshInventory();
        }

        private void RefreshInventory()
        {
            // Keep normal, ethereal and temporary copies of each item separate
            quantities.Clear();
            pickups.Clear();
            pickupNames.Clear();
            if (watchedInventory != null)
            {
                for (int i = 0; i < ItemCatalog.itemCount; i++)
                {
                    ItemIndex itemIndex = (ItemIndex)i;
                    PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
                    int normalCount = (int)System.Math.Min(int.MaxValue, (long)watchedInventory.GetItemCountPermanent(itemIndex) + Mathf.Max(0, watchedInventory.GetItemCountChanneled(itemIndex)));
                    AddPickup(pickupIndex, normalCount);
                    AddPickup(pickupIndex, watchedInventory.GetItemCountRented(itemIndex), PickupModifier.Ethereal);
                    AddPickup(pickupIndex, watchedInventory.GetItemCountTemp(itemIndex), PickupModifier.Temporary);
                }

                // Count each occupied equipment location, including alternate slots and sets
                for (uint slot = 0; slot < watchedInventory.GetEquipmentSlotCount(); slot++)
                {
                    for (uint set = 0; set < watchedInventory.GetEquipmentSetCount(); set++)
                    {
                        EquipmentLocation location = new EquipmentLocation { slot = slot, set = set };
                        EquipmentIndex equipmentIndex = watchedInventory.GetEquipment(location).equipmentIndex;
                        if (equipmentIndex != EquipmentIndex.None) AddPickup(PickupCatalog.FindPickupIndex(equipmentIndex), 1);
                    }
                }

                // Key items occupy their own inventory slot
                KeyItemIndex keyItemIndex = watchedInventory.GetKeyItemIndex();
                if (keyItemIndex != KeyItemIndex.None) AddPickup(PickupCatalog.FindPickupIndex(keyItemIndex), 1);
            }

            // Find display names and keep each item type as its own entry
            foreach (var entry in quantities.Keys)
            {
                PickupDef pickup = PickupCatalog.GetPickupDef(entry.pickupIndex);
                if (!pickupNames.ContainsKey(entry.pickupIndex))
                {
                    pickupNames[entry.pickupIndex] = Utils.GetPickupDisplayName(pickup);
                }
                pickups.Add((pickup, entry.type));
            }

            // Show equipment first, then sort names and group each pickup's normal, ethereal and temporary copies
            pickups.Sort((left, right) =>
            {
                bool leftEquipment = left.pickup.equipmentIndex != EquipmentIndex.None;
                bool rightEquipment = right.pickup.equipmentIndex != EquipmentIndex.None;
                if (leftEquipment != rightEquipment) return leftEquipment ? -1 : 1;
                int order = System.StringComparer.CurrentCultureIgnoreCase.Compare(pickupNames[left.pickup.pickupIndex], pickupNames[right.pickup.pickupIndex]);
                if (order != 0) return order;
                order = left.pickup.pickupIndex.value.CompareTo(right.pickup.pickupIndex.value);
                return order != 0 ? order : left.type.CompareTo(right.type);
            });

            // Refresh existing rows without recreating the whole list or losing its scroll position
            entryList.RefreshItems(pickups.Count, resetPosition);
            resetPosition = false;
        }

        private bool CanChangeInventory()
        {
            // Inventory modifications require a living target and server authority
            return NetworkServer.active && isActiveAndEnabled && character != null && character.healthComponent != null && character.healthComponent.alive && watchedInventory != null && watchedInventory == character.inventory;
        }

        private PickupDef GetSelectedPickup()
        {
            // Resolve the selected caption through the same lookup used by the spawn menu
            if (pickupDropdown.value < 0 || pickupDropdown.value >= pickupDropdown.options.Count) return null;
            pickupLookup.TryGetValue(pickupDropdown.options[pickupDropdown.value].text, out PickupDef pickup);
            return pickup;
        }

        private void UpdateControls()
        {
            // Clients can inspect inventories, but only the host can give or clear pickups
            bool canChange = CanChangeInventory();
            PickupDef pickup = GetSelectedPickup();
            quantityInput.interactable = canChange;
            pickupDropdown.interactable = canChange;
            modifierDropdown.interactable = canChange && pickup != null && pickup.itemIndex != ItemIndex.None;
            giveButton.interactable = canChange && pickup != null;
            clearButton.interactable = canChange;
        }

        private void OnQuantityChanged(string _value)
        {
            // Keep quantity as a positive integer, including empty or overflowing input
            int quantity = int.TryParse(_value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? Mathf.Max(1, value) : 1;
            quantityInput.SetTextWithoutNotify(quantity.ToString(CultureInfo.InvariantCulture));
        }

        private void OnPickupSelected(int _index)
        {
            // Equipment and key items do not have temporary or ethereal variants
            PickupDef pickup = GetSelectedPickup();
            if (pickup == null || pickup.itemIndex == ItemIndex.None) modifierDropdown.SetValueWithoutNotify(0);
            UpdateControls();
        }

        private void OnGivePickupsClicked()
        {
            // Recheck the target and authority at the time the button is pressed
            if (!CanChangeInventory()) return;
            PickupDef pickup = GetSelectedPickup();
            if (pickup == null) return;
            OnQuantityChanged(quantityInput.text);
            int quantity = int.Parse(quantityInput.text, CultureInfo.InvariantCulture);
            int modifierIndex = modifierDropdown.value;
            if (modifierIndex < 0 || modifierIndex >= pickupModifiers.Length) return;
            PickupModifier type = pickupModifiers[modifierIndex];

            // Apply the request through the shared inventory operations
            Utils.GivePickup(watchedInventory, pickup, quantity, type);
            inventoryDirty = true;
        }

        private void OnClearInventoryClicked()
        {
            // Keep panel and target validation separate from the shared full inventory wipe
            if (!CanChangeInventory()) return;
            Utils.ClearInventory(watchedInventory);
            inventoryDirty = true;
        }

        private void AddPickup(PickupIndex _pickupIndex, int _count, PickupModifier _type = PickupModifier.Normal)
        {
            // Ignore empty holdings and values that do not have a pickup definition
            if (_count <= 0 || PickupCatalog.GetPickupDef(_pickupIndex) == null) return;
            var entry = (_pickupIndex, _type);
            quantities.TryGetValue(entry, out int count);
            quantities[entry] = count + _count;
        }

        private void CreateEntry(GameObject _row)
        {
            // Prepare controls and navigation once for each reusable entry
            _row.AddComponent<DebugCharacterPickupEntry>().Init(this);
            entryList.AddNavigation(_row);
        }

        private void BindEntry(GameObject _row, int _index)
        {
            // Bind the row to its pickup and the inventory it was created for
            var entry = pickups[_index];
            PickupIndex pickupIndex = entry.pickup.pickupIndex;
            _row.GetComponent<DebugCharacterPickupEntry>().Bind(watchedInventory, entry.pickup, entry.type, pickupNames[pickupIndex], quantities[(pickupIndex, entry.type)]);
        }

        public void RemovePickup(Inventory _inventory, PickupDef _pickup, PickupModifier _type, bool _all)
        {
            // Only the host can remove pickups from the same living character shown by this entry
            if (!NetworkServer.active || !isActiveAndEnabled || character == null || character.healthComponent == null || !character.healthComponent.alive || _inventory == null || _inventory != character.inventory || _inventory != watchedInventory || _pickup == null) return;

            // Apply removal only after checking that this row still represents the current target
            Utils.RemovePickup(_inventory, _pickup, _type, _all);
            inventoryDirty = true;
        }
    }

    internal class DebugCharacterPickupEntry : MonoBehaviour
    {
        // Store the owning list, inventory and pickup represented by this entry
        DebugCharacterInventory inventoryPanel;
        Inventory inventory;
        PickupDef pickup;
        PickupModifier type;

        // Store the entry's display and delete controls
        TextMeshProUGUI equipmentTitle;
        TextMeshProUGUI normalTitle;
        TextMeshProUGUI etherealTitle;
        TextMeshProUGUI temporaryTitle;
        TextMeshProUGUI quantity;
        UnityEngine.UI.Button deleteButton;

        public void Init(DebugCharacterInventory _inventoryPanel)
        {
            // Find the prefab's existing controls and apply the debug menu fonts
            inventoryPanel = _inventoryPanel;
            equipmentTitle = Utils.FindChildWithTerm(transform, "EquipmentTitle").GetComponent<TextMeshProUGUI>();
            normalTitle = Utils.FindChildWithTerm(transform, "NormalTitle").GetComponent<TextMeshProUGUI>();
            etherealTitle = Utils.FindChildWithTerm(transform, "EtherealTitle").GetComponent<TextMeshProUGUI>();
            temporaryTitle = Utils.FindChildWithTerm(transform, "TemporaryTitle").GetComponent<TextMeshProUGUI>();
            quantity = Utils.FindChildWithTerm(transform, "Quantity").GetComponent<TextMeshProUGUI>();
            deleteButton = transform.Find("DeleteButton").GetComponent<UnityEngine.UI.Button>();
            deleteButton.onClick.AddListener(OnDeleteClicked);
            Utils.ApplyFonts(transform);
        }

        public void Bind(Inventory _inventory, PickupDef _pickup, PickupModifier _type, string _name, int _quantity)
        {
            // Display the pickup name and quantity separately using the prefab's text fields
            inventory = _inventory;
            pickup = _pickup;
            type = _type;

            // Show only the title matching this entry's pickup and item type
            bool equipment = pickup.equipmentIndex != EquipmentIndex.None;
            equipmentTitle.gameObject.SetActive(equipment);
            normalTitle.gameObject.SetActive(!equipment && type == PickupModifier.Normal);
            etherealTitle.gameObject.SetActive(!equipment && type == PickupModifier.Ethereal);
            temporaryTitle.gameObject.SetActive(!equipment && type == PickupModifier.Temporary);
            TextMeshProUGUI title = equipment ? equipmentTitle : type == PickupModifier.Ethereal ? etherealTitle : type == PickupModifier.Temporary ? temporaryTitle : normalTitle;
            title.text = $"{_name} [{_pickup.pickupIndex.value}]";
            quantity.text = _quantity.ToString();
            deleteButton.interactable = NetworkServer.active;
        }

        private void OnDeleteClicked()
        {
            // Either shift key changes a normal removal into removal of the whole stack
            bool removeAll = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            inventoryPanel.RemovePickup(inventory, pickup, type, removeAll);
        }
    }
}
