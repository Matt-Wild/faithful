﻿using Newtonsoft.Json.Linq;
using RewiredConsts;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugSpawnMenu : DebugPanel
    {
        // Store dictionary of string and team index relationships
        protected Dictionary<string, TeamIndex> teamLookup = new Dictionary<string, TeamIndex>()
        {
            { "Lunar", TeamIndex.Lunar },
            { "Monster", TeamIndex.Monster },
            { "Neutral", TeamIndex.Neutral },
            { "None", TeamIndex.None },
            { "Player", TeamIndex.Player },
            { "Void", TeamIndex.Void }
        };

        // Store reference to spawn button
        protected Button spawnButton;

        // Store reference to amount input
        protected InputField amountInput;

        // Store reference to category dropdown
        protected Dropdown categoryDropdown;

        // Store reference to team dropdown
        protected Dropdown teamDropdown;

        // Store reference to selection dropdowns
        protected List<SelectionDropdown> selectionDropdowns = new List<SelectionDropdown>();

        // Store dictionary of additional dropdowns and their corresponding categories
        protected Dictionary<string, List<Dropdown>> additionalDropdowns = new Dictionary<string, List<Dropdown>>();

        // Store list of all additional dropdowns
        protected List<Dropdown> allAdditionalDropdowns = new List<Dropdown>();

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Find spawn button
            spawnButton = transform.Find("SpawnButton").gameObject.GetComponent<Button>();

            // Find amount input
            amountInput = transform.Find("AmountInputField").gameObject.GetComponent<InputField>();

            // Find category dropdown
            categoryDropdown = transform.Find("CategoryDropdown").gameObject.GetComponent<Dropdown>();

            // Add on spawn behaviour
            spawnButton.onClick.AddListener(OnSpawn);

            // Add on chagne category behaviour
            categoryDropdown.onValueChanged.AddListener(OnChangeCategory);
        }

        public override void Init(DebugController _debugController, bool _startOpen = false)
        {
            // Call base init method
            base.Init(_debugController, _startOpen);

            // Create selection dropdowns
            CreateSelectionDropdowns();

            // Find team dropdown
            teamDropdown = transform.Find("TeamDropdown").gameObject.GetComponent<Dropdown>();

            // Register additional dropdowns
            RegisterAdditionalDropdown("Character", teamDropdown);

            // Enable correct selection dropdown
            EnableCorrectSelection();

            // Create additional dropdowns
            CreateAdditionalDropdowns();
        }

        void Update()
        {
            // Check for F4
            if (Input.GetKeyDown(KeyCode.F4))
            {
                // Do spawn logic
                OnSpawn();
            }
        }

        protected void CreateSelectionDropdowns()
        {
            // Create essence selection dropdown
            Dropdown essenceSelectionDropdown = transform.Find("EssenceSelectionDropdown").gameObject.GetComponent<Dropdown>();
            selectionDropdowns.Add(new SelectionDropdown(essenceSelectionDropdown, "Essence", ["Common", "Uncommon", "Legendary", "Boss/Planet", "Lunar", "Void Common", "Void Uncommon", "Void Legendary", "Void Boss/Planet", "Equipment", "Lunar Equipment"]));

            // Create character selection dropdown
            Dropdown characterSelectionDropdown = transform.Find("CharacterSelectionDropdown").gameObject.GetComponent<Dropdown>();
            selectionDropdowns.Add(new SelectionDropdown(characterSelectionDropdown, "Character", Utils.characterCardNames));
        }

        protected void RegisterAdditionalDropdown(string _category, Dropdown _dropdown)
        {
            // Add dropdown to all dropdowns list
            allAdditionalDropdowns.Add(_dropdown);

            // Check for category in dictionary
            if (additionalDropdowns.ContainsKey(_category))
            {
                // Add to list
                additionalDropdowns[_category].Add(_dropdown);
                return;
            }

            // Create new list for category
            additionalDropdowns[_category] =
            [
                // Add dropdown to list
                _dropdown
            ];
        }

        protected void OnSpawn()
        {
            // Check spawn category
            switch (category)
            {
                case "Essence":
                    // Spawn essence
                    SpawnEssence();
                    break;
                case "Character":
                    // Spawn character
                    SpawnCharacter();
                    break;
                default:
                    break;
            }
        }

        protected void SpawnEssence()
        {
            // Get local player body
            CharacterBody localBody = Utils.localPlayerBody;

            // Skip if no local player body
            if (localBody == null)
            {
                return;
            }

            // Get pickup index
            PickupIndex index = PickupCatalog.FindPickupIndex(ItemTier.Tier1);
            switch (selection)
            {
                case "Uncommon":
                    index = PickupCatalog.FindPickupIndex(ItemTier.Tier2);
                    break;
                case "Legendary":
                    index = PickupCatalog.FindPickupIndex(ItemTier.Tier3);
                    break;
                case "Boss/Planet":
                    index = PickupCatalog.FindPickupIndex(ItemTier.Boss);
                    break;
                case "Lunar":
                    index = PickupCatalog.FindPickupIndex(ItemTier.Lunar);
                    break;
                case "Void Common":
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidTier1);
                    break;
                case "Void Uncommon":
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidTier2);
                    break;
                case "Void Legendary":
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidTier3);
                    break;
                case "Void Boss/Planet":
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidBoss);
                    break;
                case "Equipment":
                    index = PickupCatalog.FindPickupIndex(EquipmentCatalog.equipmentDefs.Where(x => x.nameToken == "EQUIPMENT_COMMANDMISSILE_NAME").FirstOrDefault().equipmentIndex);
                    break;
                case "Lunar Equipment":
                    index = PickupCatalog.FindPickupIndex(EquipmentCatalog.equipmentDefs.Where(x => x.nameToken == "EQUIPMENT_METEOR_NAME").FirstOrDefault().equipmentIndex);
                    break;
                default:
                    break;
            }

            Log.Debug($"Spawning {spawnAmount} item(s) at coordinates {localBody.transform.position}");

            // Spawn requested amount
            for (int i = 0; i < spawnAmount; i++)
            {
                // Create essence object
                GameObject essence = Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/CommandCube"), localBody.transform.position, localBody.transform.rotation);
                essence.GetComponent<PickupIndexNetworker>().NetworkpickupIndex = index;
                essence.GetComponent<PickupPickerController>().SetOptionsFromPickupForCommandArtifact(index);
                NetworkServer.Spawn(essence);
            }
        }

        protected void SpawnCharacter()
        {
            // Get local player body
            CharacterBody localBody = Utils.localPlayerBody;

            // Skip if no local player body
            if (localBody == null)
            {
                return;
            }

            Log.Debug($"Spawning {spawnAmount} character(s) at target {localBody.transform.position}");

            // Request spawn from utils
            Utils.SpawnCharacterCard(localBody.transform, selection, spawnAmount, teamLookup[teamDropdown.options[teamDropdown.value].text]);
        }

        protected void EnableCorrectSelection()
        {
            // Cycle through selection dropdowns
            foreach (SelectionDropdown selectionDropdown in selectionDropdowns)
            {
                // Correct selection dropdown?
                if (selectionDropdown.tag == category)
                {
                    // Enable dropdown
                    selectionDropdown.Enable();
                }
                else
                {
                    // Otherwise disable
                    selectionDropdown.Disable();
                }
            }
        }

        protected void CreateAdditionalDropdowns()
        {
            // Cycle through all additional dropdowns
            foreach (Dropdown dropdown in allAdditionalDropdowns)
            {
                // Disable dropdown
                dropdown.gameObject.SetActive(false);
            }

            // Check for additional dropdowns for current category
            if (additionalDropdowns.ContainsKey(category))
            {
                // Cycle through additional dropdowns for category
                foreach (Dropdown dropdown in additionalDropdowns[category])
                {
                    // Enable dropdown
                    dropdown.gameObject.SetActive(true);
                }
            }
        }

        protected void OnChangeCategory(int _index)
        {
            // Enable correct selection dropdown
            EnableCorrectSelection();

            // Create additional dropdowns
            CreateAdditionalDropdowns();
        }

        protected int spawnAmount
        {
            get
            {
                // Return clamped spawn amount field value
                return Mathf.Clamp(int.Parse(amountInput.text), 1, 100);
            }
        }

        protected string category
        {
            get
            {
                // Return category dropdown value
                return categoryDropdown.options[categoryDropdown.value].text;
            }
        }

        protected string selection
        {
            get
            {
                // Get current selection dropdown
                foreach (SelectionDropdown selectionDropdown in selectionDropdowns)
                {
                    // Correct selection dropdown?
                    if (selectionDropdown.tag == category)
                    {
                        // Return value
                        return selectionDropdown.value;
                    }
                }

                // Otherwise return null
                return null;
            }
        }
    }

    internal struct SelectionDropdown
    {
        // Store reference to dropdown
        private Dropdown dropdown;

        // Store selection dropdown tag
        public string tag;

        public SelectionDropdown(Dropdown _dropdown, string _tag, List<string> _options)
        {
            // Assign dropdown
            dropdown = _dropdown;

            // Assign tag
            tag = _tag;

            // Add options to dropdown
            dropdown.AddOptions(_options);
        }

        public void Enable()
        {
            // Enable dropdown
            dropdown.gameObject.SetActive(true);
        }

        public void Disable()
        {
            // Disable dropdown
            dropdown.gameObject.SetActive(false);
        }

        public string value
        {
            get
            {
                // Return current value of dropdown
                return dropdown.options[dropdown.value].text;
            }
        }
    }
}
