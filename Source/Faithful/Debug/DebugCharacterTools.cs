using RoR2;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugCharacterTools : DebugPanel
    {
        // Main panels
        GameObject characterPanel;
        GameObject noCharacterPanel;

        // Character name text
        TextMeshProUGUI nameText;

        // Store the team dropdown and its team indices
        TMP_Dropdown teamDropdown;
        List<TeamIndex> teamIndices;

        // Character level input and the target being edited
        TMP_InputField levelInput;
        CharacterBody levelInputCharacter;
        bool levelInputChanged;

        // Character health, shield and barrier sliders
        Slider healthSlider;
        Slider shieldSlider;
        Slider barrierSlider;

        // Character health, shield and barrier value text
        TextMeshProUGUI healthValueText;
        TextMeshProUGUI shieldValueText;
        TextMeshProUGUI barrierValueText;

        // Character action buttons
        Button killButton;
        Button objectAnalysisButton;

        // Select the local player's character when no target is selected
        Button selectSelfButton;

        // Character inventory panel and its toggle button
        GameObject inventoryPanel;
        Button inventoryButton;
        DebugCharacterInventory inventory;
        bool inventoryReady;

        // Character buffs panel and its toggle button
        GameObject buffsPanel;
        Button buffsButton;
        DebugCharacterBuffs buffs;
        bool buffsReady;

        // Current target character
        CharacterBody currentCharacter;

        // Previous target character
        CharacterBody previousCharacter;


        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Find main panels
            characterPanel = transform.Find("CharacterPanel").gameObject;
            noCharacterPanel = transform.Find("NoCharacterPanel").gameObject;

            // Find the select self button and add its targeting behaviour
            selectSelfButton = Utils.FindChildByName(noCharacterPanel.transform, "SelectSelfButton").GetComponent<Button>();
            selectSelfButton.onClick.AddListener(OnSelectSelfClicked);

            // Find character name text
            nameText = Utils.FindChildWithTerm(characterPanel.transform.Find("Content"), "CharacterName")?.GetComponent<TextMeshProUGUI>();

            // Find and populate the team dropdown
            teamDropdown = Utils.FindChildByName(characterPanel.transform, "CharacterTeamDropdown").GetComponent<TMP_Dropdown>();
            teamIndices = System.Enum.GetValues(typeof(TeamIndex)).Cast<TeamIndex>().Where(team => team >= TeamIndex.None && team < TeamIndex.Count).OrderBy(team => team.ToString()).ToList();
            teamDropdown.ClearOptions();
            teamDropdown.AddOptions(teamIndices.Select(team => team.ToString()).ToList());

            // Add team selection behaviour
            teamDropdown.onValueChanged.AddListener(OnTeamChanged);

            // Find the level input and add level adjustment behaviour
            levelInput = Utils.FindChildByName(characterPanel.transform, "CharacterLevelInputField").GetComponent<TMP_InputField>();
            levelInput.onSelect.AddListener(OnLevelInputSelected);
            levelInput.onValueChanged.AddListener(OnLevelInputChanged);
            levelInput.onEndEdit.AddListener(OnLevelChanged);

            // Find and configure the health slider
            healthSlider = Utils.FindChildByName(characterPanel.transform, "HealthSlider").GetComponent<Slider>();
            healthSlider.minValue = 0.0f;
            healthSlider.maxValue = 1.0f;
            healthSlider.wholeNumbers = false;

            // Find the health value text
            healthValueText = Utils.FindChildWithTerm(healthSlider.transform, "CharacterHealthValue")?.GetComponent<TextMeshProUGUI>();

            // Add health adjustment behaviour
            healthSlider.onValueChanged.AddListener(OnHealthChanged);

            // Find and configure the shield slider
            shieldSlider = Utils.FindChildByName(characterPanel.transform, "ShieldSlider").GetComponent<Slider>();
            shieldSlider.minValue = 0.0f;
            shieldSlider.maxValue = 1.0f;
            shieldSlider.wholeNumbers = false;

            // Find the shield value text
            shieldValueText = Utils.FindChildWithTerm(shieldSlider.transform, "CharacterShieldValue")?.GetComponent<TextMeshProUGUI>();

            // Add shield adjustment behaviour
            shieldSlider.onValueChanged.AddListener(OnShieldChanged);

            // Find and configure the barrier slider
            barrierSlider = Utils.FindChildByName(characterPanel.transform, "BarrierSlider").GetComponent<Slider>();
            barrierSlider.minValue = 0.0f;
            barrierSlider.maxValue = 1.0f;
            barrierSlider.wholeNumbers = false;

            // Find the barrier value text
            barrierValueText = Utils.FindChildWithTerm(barrierSlider.transform, "CharacterBarrierValue")?.GetComponent<TextMeshProUGUI>();

            // Add barrier adjustment behaviour
            barrierSlider.onValueChanged.AddListener(OnBarrierChanged);

            // Find the character action buttons and add their behaviour
            killButton = Utils.FindChildByName(characterPanel.transform, "KillButton").GetComponent<Button>();
            killButton.onClick.AddListener(OnKillClicked);
            objectAnalysisButton = Utils.FindChildByName(characterPanel.transform, "ObjectAnalysisButton").GetComponent<Button>();
            objectAnalysisButton.onClick.AddListener(OnObjectAnalysisClicked);

            // Prepare the inventory panel and keep it closed until requested
            inventoryPanel = Utils.FindChildByName(characterPanel.transform, "InventoryPanel").gameObject;
            inventoryPanel.SetActive(false);
            inventory = inventoryPanel.AddComponent<DebugCharacterInventory>();
            inventoryReady = inventory.Init();
            inventoryButton = Utils.FindChildByName(characterPanel.transform, "InventoryButton").GetComponent<Button>();
            inventoryButton.onClick.AddListener(OnInventoryClicked);

            // Prepare the buffs panel and keep it closed until requested
            buffsPanel = Utils.FindChildByName(characterPanel.transform, "BuffsPanel").gameObject;
            buffsPanel.SetActive(false);
            buffs = buffsPanel.AddComponent<DebugCharacterBuffs>();
            buffsReady = buffs.Init();
            buffsButton = Utils.FindChildByName(characterPanel.transform, "BuffsButton").GetComponent<Button>();
            buffsButton.onClick.AddListener(OnBuffsClicked);
        }

        private void Update()
        {
            // Keep the level display in sync without interrupting manual input
            UpdateLevelInput();

            // Keep the sliders in sync with the targeted character's health, shield and barrier
            UpdateHealthSliders();

            // Keep character actions available for the current target and server authority
            UpdateActionButtons();
        }

        private void LateUpdate()
        {
            // Clear dead or destroyed targets
            if (currentCharacter == null || currentCharacter.healthComponent == null || !currentCharacter.healthComponent.alive)
            {
                currentCharacter = null;
            }

            // Compare stored references to determine if the character has changed
            if (!ReferenceEquals(currentCharacter, previousCharacter))
            {
                // Update character panel
                UpdateCharacterPanel();

                // Store the current character as previous
                previousCharacter = currentCharacter;
            }
        }

        private void OnEnable()
        {
            // Add hooks
            On.RoR2.PingerController.AttemptPing += OnAttemptPing;

            // Update character panel
            UpdateCharacterPanel();
        }

        private void OnDisable()
        {
            // Remove hooks
            On.RoR2.PingerController.AttemptPing -= OnAttemptPing;
        }

        public override void OnMaximise()
        {
            // Refresh the visible character panels if the target changed while minimised
            base.OnMaximise();
            UpdateCharacterPanel();
        }

        private void UpdateCharacterPanel()
        {
            // Update the inventory target before showing or hiding the character panel
            inventory.SetCharacter(currentCharacter);

            // Keep both inspectors linked to the same target
            buffs.SetCharacter(currentCharacter);

            // Discard unfinished input when switching to another target
            if (levelInput.isFocused && !ReferenceEquals(levelInputCharacter, currentCharacter))
            {
                levelInputChanged = false;
                levelInput.DeactivateInputField();
            }
            UpdateLevelInput(true);

            // Update actions for the new target
            UpdateActionButtons();

            // Check for current character
            if (currentCharacter == null)
            {
                // No character selected
                characterPanel.SetActive(false);
                noCharacterPanel.SetActive(!minimised);
                teamDropdown.interactable = false;

                return;
            }

            // Keep character content hidden until the panel is maximised
            characterPanel.SetActive(!minimised);
            noCharacterPanel.SetActive(false);

            // Update character name
            nameText?.text = $"Name: {currentCharacter.GetDisplayName()} [{currentCharacter.baseNameToken}]";

            // Select the current team
            int teamIndex = currentCharacter.teamComponent != null ? teamIndices.IndexOf(currentCharacter.teamComponent.teamIndex) : -1;
            teamDropdown.SetValueWithoutNotify(teamIndex >= 0 ? teamIndex : teamIndices.IndexOf(TeamIndex.None));
            teamDropdown.interactable = NetworkServer.active && Utils.netUtils != null && teamIndex >= 0;

            // Update the current health display
            UpdateHealthSliders();
        }

        private void UpdateLevelInput(bool _forceUpdate = false)
        {
            // Only the host can adjust a living character's level
            HealthComponent health = currentCharacter != null ? currentCharacter.healthComponent : null;
            levelInput.interactable = NetworkServer.active && health != null && health.alive && Behaviour.characterLevelChangesAvailable;

            // Refresh the display without triggering an adjustment or replacing unfinished input
            if (!levelInput.isFocused || _forceUpdate)
            {
                levelInput.SetTextWithoutNotify(currentCharacter != null ? currentCharacter.level.ToString("0.##", CultureInfo.InvariantCulture) : "");
            }
        }

        private void OnLevelInputSelected(string _value)
        {
            // Remember which character the player started editing
            levelInputCharacter = currentCharacter;
            levelInputChanged = false;
        }

        private void OnLevelInputChanged(string _value)
        {
            // Only manual edits should change the character's level
            levelInputChanged = true;
        }

        private void OnLevelChanged(string _value)
        {
            // Apply completed integer input to the character that was being edited
            HealthComponent health = currentCharacter != null ? currentCharacter.healthComponent : null;
            if (levelInputChanged && !levelInput.wasCanceled && ReferenceEquals(levelInputCharacter, currentCharacter) && NetworkServer.active && health != null && health.alive && int.TryParse(_value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int level))
            {
                Utils.ChangeCharacterLevel(currentCharacter, Mathf.Max(1, level));
            }
            levelInputChanged = false;

            // Restore the actual level if input was invalid or the change could not be applied
            UpdateLevelInput(true);
            UpdateHealthSliders();
        }

        private void UpdateHealthSliders()
        {
            // Find the targeted character's health component
            HealthComponent health = currentCharacter != null ? currentCharacter.healthComponent : null;
            bool canAdjust = NetworkServer.active && health != null && health.alive;

            // Refresh the display without triggering a manual health adjustment
            float value = health != null && health.fullHealth > 0.0f ? Mathf.Clamp01(health.health / health.fullHealth) : 0.0f;
            healthSlider.SetValueWithoutNotify(value);
            healthSlider.interactable = canAdjust && health.fullHealth >= 1.0f;
            healthValueText?.text = health != null ? $"{health.health:0.##}" : "0";

            // Refresh shield using its own maximum capacity
            float shieldValue = health != null && health.fullShield > 0.0f ? Mathf.Clamp01(health.shield / health.fullShield) : 0.0f;
            shieldSlider.SetValueWithoutNotify(shieldValue);
            shieldSlider.interactable = canAdjust && health.fullShield > 0.0f;
            shieldValueText?.text = health != null ? $"{health.shield:0.##}" : "0";

            // Refresh barrier using its own maximum capacity
            float barrierValue = health != null && health.fullBarrier > 0.0f ? Mathf.Clamp01(health.barrier / health.fullBarrier) : 0.0f;
            barrierSlider.SetValueWithoutNotify(barrierValue);
            barrierSlider.interactable = canAdjust && health.fullBarrier > 0.0f;
            barrierValueText?.text = health != null ? $"{health.barrier:0.##}" : "0";
        }

        private void UpdateActionButtons()
        {
            // Analysis is available to everyone, while killing requires server authority
            HealthComponent health = currentCharacter != null ? currentCharacter.healthComponent : null;
            objectAnalysisButton.interactable = currentCharacter != null;
            killButton.interactable = NetworkServer.active && health != null && health.alive;
            inventoryButton.interactable = inventoryReady && currentCharacter != null && currentCharacter.inventory != null;
            buffsButton.interactable = buffsReady && health != null && health.alive;

            // Only allow self targeting when the local player's body is alive
            CharacterBody localBody = Utils.localPlayerBody;
            selectSelfButton.interactable = localBody != null && localBody.healthComponent != null && localBody.healthComponent.alive;
        }

        private void OnSelectSelfClicked()
        {
            // Find the local player's current living body
            CharacterBody localBody = Utils.localPlayerBody;
            if (localBody == null || localBody.healthComponent == null || !localBody.healthComponent.alive) return;

            // Target the player and refresh the character tools immediately
            SelectCharacter(localBody);
        }

        public void SelectCharacter(CharacterBody _character)
        {
            // Share immediate targeting between self selection and other debug panels
            currentCharacter = _character != null && _character.healthComponent != null && _character.healthComponent.alive ? _character : null;
            UpdateCharacterPanel();
            previousCharacter = currentCharacter;
        }

        private void OnInventoryClicked()
        {
            // Toggle the current character's inventory display
            if (!inventoryReady || currentCharacter == null || currentCharacter.inventory == null) return;
            inventory.SetCharacter(currentCharacter);
            bool open = !inventoryPanel.activeSelf;
            if (open) buffsPanel.SetActive(false);
            inventoryPanel.SetActive(open);
        }

        private void OnBuffsClicked()
        {
            // Buffs and inventory occupy the same space, so only one can be open
            if (!buffsReady || currentCharacter == null) return;
            buffs.SetCharacter(currentCharacter);
            bool open = !buffsPanel.activeSelf;
            if (open) inventoryPanel.SetActive(false);
            buffsPanel.SetActive(open);
        }

        private void OnObjectAnalysisClicked()
        {
            // Require a target that still exists
            if (currentCharacter == null) return;

            // Open and maximise object analysis before selecting the character body's game object
            DebugObjectAnalysis objectAnalysis = debugController.debugObjectAnalysis;
            objectAnalysis.Open();
            objectAnalysis.Maximise();
            objectAnalysis.AnalyseObject(currentCharacter.gameObject);

            // Replace the character tools panel with object analysis
            Close();
        }

        private void OnKillClicked()
        {
            // Only the host can kill a living character
            HealthComponent health = currentCharacter != null ? currentCharacter.healthComponent : null;
            if (!NetworkServer.active || health == null || !health.alive) return;

            // Use the game's normal death handling and network synchronization
            health.Suicide();
        }

        private void OnHealthChanged(float _value)
        {
            // Only the host can adjust a living character's health
            HealthComponent health = currentCharacter != null ? currentCharacter.healthComponent : null;
            if (!NetworkServer.active || health == null || !health.alive || health.fullHealth < 1.0f)
            {
                UpdateHealthSliders();
                return;
            }

            // Clamp manual adjustments to at least one health point and sync the change
            health.Networkhealth = Mathf.Clamp(_value * health.fullHealth, 1.0f, health.fullHealth);
            UpdateHealthSliders();
        }

        private void OnShieldChanged(float _value)
        {
            // Only the host can adjust a living character with shield capacity
            HealthComponent health = currentCharacter != null ? currentCharacter.healthComponent : null;
            if (!NetworkServer.active || health == null || !health.alive || health.fullShield <= 0.0f)
            {
                UpdateHealthSliders();
                return;
            }

            // Clamp manual adjustments between zero and maximum shield and sync the change
            health.Networkshield = Mathf.Clamp(_value * health.fullShield, 0.0f, health.fullShield);
            UpdateHealthSliders();
        }

        private void OnBarrierChanged(float _value)
        {
            // Only the host can adjust a living character with barrier capacity
            HealthComponent health = currentCharacter != null ? currentCharacter.healthComponent : null;
            if (!NetworkServer.active || health == null || !health.alive || health.fullBarrier <= 0.0f)
            {
                UpdateHealthSliders();
                return;
            }

            // Clamp manual adjustments between zero and maximum barrier and sync the change
            health.Networkbarrier = Mathf.Clamp(_value * health.fullBarrier, 0.0f, health.fullBarrier);
            UpdateHealthSliders();
        }

        private void OnTeamChanged(int _index)
        {
            // Apply valid selections through the shared team change handling
            if (_index >= 0 && _index < teamIndices.Count)
            {
                Utils.ChangeCharacterTeam(currentCharacter, teamIndices[_index]);
            }

            // Display the current affiliation even if the change could not be applied
            UpdateCharacterPanel();
        }

        private void OnAttemptPing(On.RoR2.PingerController.orig_AttemptPing orig, PingerController self, Ray aimRay, GameObject bodyObject)
        {
            // Prepare for ping
            float num;
            aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, bodyObject, out num);
            float maxDistance = 1000f + num;

            // Initialise raycast hit data
            RaycastHit raycastHit;

            // Check for character
            if (Util.CharacterRaycast(bodyObject, aimRay, out raycastHit, maxDistance, LayerIndex.entityPrecise.mask | LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
            {
                // Check for hurtbox
                HurtBox component = raycastHit.collider.GetComponent<HurtBox>();
                if (component && component.healthComponent)
                {
                    // Make current character
                    currentCharacter = component.healthComponent.body;
                }
            }

            // Run original processes
            orig(self, aimRay, bodyObject);
        }
    }
}
