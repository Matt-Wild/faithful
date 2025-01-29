using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
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

        // Store dictionary of string and elite def relationships
        protected Dictionary<string, EliteDef> m_eliteLookup = null;

        // Store reference to spawn button
        protected Button spawnButton;

        // Store reference to amount input
        protected InputField amountInput;

        // Store reference to power input
        protected InputField powerInput;

        // Store reference to category dropdown
        protected Dropdown categoryDropdown;

        // Store reference to team dropdown
        protected Dropdown teamDropdown;

        // Store reference to elite type dropdown
        protected Dropdown eliteDropdown;

        // Store reference to selection dropdowns
        protected List<SelectionDropdown> selectionDropdowns = new List<SelectionDropdown>();

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Find spawn button
            spawnButton = transform.Find("SpawnButton").gameObject.GetComponent<Button>();

            // Find amount input
            amountInput = transform.Find("AmountInputField").gameObject.GetComponent<InputField>();

            // Find amount input
            powerInput = transform.Find("PowerInputField").gameObject.GetComponent<InputField>();

            // Find category dropdown
            categoryDropdown = transform.Find("CategoryDropdown").gameObject.GetComponent<Dropdown>();

            // Add on spawn behaviour
            spawnButton.onClick.AddListener(OnSpawn);

            // Add on chagne category behaviour
            categoryDropdown.onValueChanged.AddListener(OnChangeCategory);
        }

        void Start()
        {
            // Populate elite dropdown
            eliteDropdown.AddOptions(eliteLookup.Keys.ToList());
        }

        public override void Init(DebugController _debugController, bool _startOpen = false)
        {
            // Call base init method
            base.Init(_debugController, _startOpen);

            // Find team dropdown
            teamDropdown = transform.Find("TeamDropdown").gameObject.GetComponent<Dropdown>();

            // Find elite dropdown
            eliteDropdown = transform.Find("EliteTypeDropdown").gameObject.GetComponent<Dropdown>();

            // Create selection dropdowns
            RegisterSelectionDropdowns();

            // Enable correct selection dropdown
            EnableCorrectSelection();
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

        protected void RegisterSelectionDropdowns()
        {
            // Register essence selection dropdown
            Dropdown essenceSelectionDropdown = transform.Find("EssenceSelectionDropdown").gameObject.GetComponent<Dropdown>();
            selectionDropdowns.Add(new SelectionDropdown(essenceSelectionDropdown, "Essence", ["Common", "Uncommon", "Legendary", "Boss/Planet", "Lunar", "Void Common", "Void Uncommon", "Void Legendary", "Void Boss/Planet", "Equipment", "Lunar Equipment"]));

            // Register character selection dropdown
            Dropdown characterSelectionDropdown = transform.Find("CharacterSelectionDropdown").gameObject.GetComponent<Dropdown>();
            selectionDropdowns.Add(new SelectionDropdown(characterSelectionDropdown, "Character", Utils.characterCardNames));

            // Register team selection dropdown
            selectionDropdowns.Add(new SelectionDropdown(teamDropdown, "Character"));

            // Register elite selection dropdown
            selectionDropdowns.Add(new SelectionDropdown(eliteDropdown, "Character"));
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
            Utils.SpawnCharacterCard(localBody.transform, selection, spawnAmount, teamLookup[teamDropdown.options[teamDropdown.value].text], CharacterCardSpawned);
        }

        protected void CharacterCardSpawned(SpawnCard.SpawnResult result)
        {
            // Check if spawn was successful
            if (!result.success) return;

            // Get elite selection
            string eliteSelection = eliteDropdown.options[eliteDropdown.value].text;

            // Check for elite type
            if (eliteSelection != "None")
            {
                // Get character master
                CharacterMaster master = result.spawnedInstance.GetComponent<CharacterMaster>();
                if (master == null) return;

                // Get character body
                CharacterBody body = master.GetBody();
                if (body == null) return;

                // Get character inventory
                Inventory inventory = master.inventory;
                if (inventory == null) return;

                // Make elite
                Utils.MakeElite(body, inventory, eliteLookup[eliteSelection]);

                // Modify stats by power
                body.baseDamage *= spawnPower;
                body.baseMaxHealth *= spawnPower;
            }

            // Not elite
            else
            {
                // Get character master
                CharacterMaster master = result.spawnedInstance.GetComponent<CharacterMaster>();
                if (master == null) return;

                // Get character body
                CharacterBody body = master.GetBody();
                if (body == null) return;

                // Modify stats by power
                body.baseDamage *= spawnPower;
                body.baseMaxHealth *= spawnPower;
            }
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

        protected void OnChangeCategory(int _index)
        {
            // Enable correct selection dropdown
            EnableCorrectSelection();
        }

        protected Dictionary<string, EliteDef> eliteLookup
        {
            get
            {
                // Check elite lookup and elite defs
                if (m_eliteLookup == null && EliteCatalog.eliteDefs != null)
                {
                    // Create dictionary
                    m_eliteLookup = new Dictionary<string, EliteDef>();

                    // Cycle through elite defs
                    foreach (EliteDef def in EliteCatalog.eliteDefs)
                    {
                        // Check for elite def
                        if (def == null) continue;

                        // Check for equipment def
                        if (def.eliteEquipmentDef == null) continue;

                        // Add to dictionary
                        m_eliteLookup[def.name] = def;
                    }
                }

                // Return lookup
                return m_eliteLookup;
            }
        }

        protected int spawnAmount
        {
            get
            {
                // Return clamped spawn amount field value
                return Mathf.Clamp(int.Parse(amountInput.text), 1, 100);
            }
        }

        protected float spawnPower
        {
            get
            {
                // Return spawn power limited to 0.0001
                return Mathf.Max(float.Parse(powerInput.text), 0.0001f);
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

        public SelectionDropdown(Dropdown _dropdown, string _tag, List<string> _options = null)
        {
            // Assign dropdown
            dropdown = _dropdown;

            // Assign tag
            tag = _tag;

            // Check for additional options
            if (_options != null)
            {
                // Add options to dropdown
                dropdown.AddOptions(_options);
            }
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
