using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class DebugCharacterBuffs : MonoBehaviour
    {
        // Store the selected character and reusable buff lists
        CharacterBody character;
        DebugScrollList entryList;
        List<BuffDef> buffs = new List<BuffDef>();
        List<(BuffDef buff, bool timed, int quantity)> entries = new List<(BuffDef, bool, int)>();
        int[] normalCounts;
        int[] timedCounts;
        int[] sampledNormalCounts;
        int[] sampledTimedCounts;

        // Store the buff giving controls
        TMP_InputField quantityInput;
        TMP_InputField durationInput;
        TMP_Dropdown buffDropdown;
        TMP_Dropdown modifierDropdown;
        UnityEngine.UI.Button giveButton;
        UnityEngine.UI.Button clearButton;
        Coroutine givingBuffs;

        // Refresh rows only when counts change and poll server data only while visible
        bool buffsDirty = true;
        bool resetPosition = true;
        float nextSnapshotTime;

        public bool Init()
        {
            // Find the existing scroll view and buff entry template
            UnityEngine.UI.ScrollRect scrollRect = Utils.FindChildByName(transform, "Entries")?.GetComponent<UnityEngine.UI.ScrollRect>();
            GameObject entryPrefab = Assets.GetObject("DebugCharacterBuffEntry");
            quantityInput = Utils.FindChildByName(transform, "BuffQuantityInputField")?.GetComponent<TMP_InputField>();
            durationInput = Utils.FindChildByName(transform, "BuffDurationInputField")?.GetComponent<TMP_InputField>();
            buffDropdown = Utils.FindChildByName(transform, "BuffSelectionDropdown")?.GetComponent<TMP_Dropdown>();
            modifierDropdown = Utils.FindChildByName(transform, "BuffModifierDropdown")?.GetComponent<TMP_Dropdown>();
            giveButton = Utils.FindChildByName(transform, "GiveBuffsButton")?.GetComponent<UnityEngine.UI.Button>();
            clearButton = Utils.FindChildByName(transform, "ClearBuffsButton")?.GetComponent<UnityEngine.UI.Button>();
            if (scrollRect == null || entryPrefab == null || quantityInput == null || durationInput == null || buffDropdown == null || modifierDropdown == null || giveButton == null || clearButton == null)
            {
                Log.Error("[DEBUG MENU] | Character buff controls are missing. Rebuild the asset bundle with the updated character tools and buff entry prefabs.");
                return false;
            }

            // Sort the complete catalog once and reuse the spawn menu's pooled dropdown
            for (int i = 0; i < BuffCatalog.buffCount; i++)
            {
                BuffDef buff = BuffCatalog.GetBuffDef((BuffIndex)i);
                if (buff != null) buffs.Add(buff);
            }
            buffs.Sort((left, right) =>
            {
                int order = System.StringComparer.CurrentCultureIgnoreCase.Compare(left.name, right.name);
                return order != 0 ? order : left.buffIndex.CompareTo(right.buffIndex);
            });
            List<string> options = new List<string>();
            foreach (BuffDef buff in buffs) options.Add($"{buff.name} [{(int)buff.buffIndex}]");
            buffDropdown = DebugSelectionDropdown.Create(buffDropdown);
            buffDropdown.ClearOptions();
            buffDropdown.AddOptions(options);
            buffDropdown.SetValueWithoutNotify(0);
            modifierDropdown.ClearOptions();
            modifierDropdown.AddOptions(new List<string> { "None", "Temporary" });
            modifierDropdown.SetValueWithoutNotify(0);

            // Keep quantities positive and allow fractional durations in seconds
            durationInput.contentType = TMP_InputField.ContentType.DecimalNumber;
            quantityInput.onEndEdit.AddListener(OnQuantityChanged);
            durationInput.onEndEdit.AddListener(OnDurationChanged);
            modifierDropdown.onValueChanged.AddListener(OnModifierChanged);
            giveButton.onClick.AddListener(OnGiveBuffsClicked);
            clearButton.onClick.AddListener(OnClearBuffsClicked);
            OnQuantityChanged(quantityInput.text);
            OnDurationChanged(durationInput.text);
            OnModifierChanged(modifierDropdown.value);

            // Reuse count buffers and create only enough rows to cover the visible list
            normalCounts = new int[BuffCatalog.buffCount];
            timedCounts = new int[BuffCatalog.buffCount];
            sampledNormalCounts = new int[BuffCatalog.buffCount];
            sampledTimedCounts = new int[BuffCatalog.buffCount];
            entryList = scrollRect.gameObject.AddComponent<DebugScrollList>();
            entryList.Init(scrollRect, (RectTransform)entryPrefab.transform, CreateEntry, BindEntry);
            UpdateControls();
            return true;
        }

        public void SetCharacter(CharacterBody _character)
        {
            // Discard the previous target's counts and any unfinished giving request
            if (ReferenceEquals(character, _character)) return;
            StopGivingBuffs();
            character = _character;
            if (normalCounts != null) System.Array.Clear(normalCounts, 0, normalCounts.Length);
            if (timedCounts != null) System.Array.Clear(timedCounts, 0, timedCounts.Length);
            buffsDirty = true;
            resetPosition = true;
            nextSnapshotTime = 0.0f;
        }

        private void OnEnable()
        {
            // Refresh changes made while hidden and accept server snapshots on clients
            Utils.characterBuffCountsReceived += OnBuffCountsReceived;
            buffsDirty = true;
            nextSnapshotTime = 0.0f;
        }

        private void OnDisable()
        {
            // Stop pending work and snapshot subscriptions while this panel is hidden
            Utils.characterBuffCountsReceived -= OnBuffCountsReceived;
            StopGivingBuffs();
        }

        private void LateUpdate()
        {
            if (entryList == null) return;

            // Host counts include timer ownership; clients request that server-only information
            if (NetworkServer.active)
            {
                Utils.GetCharacterBuffCounts(character, sampledNormalCounts, sampledTimedCounts);
                OnBuffCountsReceived(character, sampledNormalCounts, sampledTimedCounts);
            }
            else if (character != null && Utils.netUtils != null && Time.unscaledTime >= nextSnapshotTime)
            {
                nextSnapshotTime = Time.unscaledTime + 0.25f;
                Utils.netUtils.CmdRequestCharacterBuffCounts(character.gameObject);
            }
            UpdateControls();
            if (!buffsDirty) return;

            // Keep normal and timed copies adjacent, with alphabetical order taking priority
            buffsDirty = false;
            entries.Clear();
            foreach (BuffDef buff in buffs)
            {
                int index = (int)buff.buffIndex;
                if (normalCounts[index] > 0) entries.Add((buff, false, normalCounts[index]));
                if (timedCounts[index] > 0) entries.Add((buff, true, timedCounts[index]));
            }
            entryList.RefreshItems(entries.Count, resetPosition);
            resetPosition = false;
        }

        private void OnBuffCountsReceived(CharacterBody _body, int[] _normalCounts, int[] _timedCounts)
        {
            // Ignore replies for a previous target or a panel which has since closed
            if (!isActiveAndEnabled || !ReferenceEquals(character, _body) || normalCounts == null || _normalCounts == null || _timedCounts == null || _normalCounts.Length != normalCounts.Length || _timedCounts.Length != timedCounts.Length) return;
            for (int i = 0; i < normalCounts.Length; i++)
            {
                if (normalCounts[i] == _normalCounts[i] && timedCounts[i] == _timedCounts[i]) continue;
                normalCounts[i] = _normalCounts[i];
                timedCounts[i] = _timedCounts[i];
                buffsDirty = true;
            }
        }

        private bool CanChangeBuffs()
        {
            // Buff modifications require a living target and server authority
            return NetworkServer.active && isActiveAndEnabled && character != null && character.healthComponent != null && character.healthComponent.alive;
        }

        private void UpdateControls()
        {
            // Clients can inspect buffs, while only the host can change them
            bool canChange = CanChangeBuffs();
            quantityInput.interactable = canChange;
            durationInput.interactable = canChange;
            buffDropdown.interactable = canChange;
            modifierDropdown.interactable = canChange;
            giveButton.interactable = canChange && givingBuffs == null && buffDropdown.value >= 0 && buffDropdown.value < buffs.Count;
            clearButton.interactable = canChange;
        }

        private void OnQuantityChanged(string _value)
        {
            // Keep quantity as a positive integer, including empty or overflowing input
            int quantity = int.TryParse(_value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? Mathf.Max(1, value) : 1;
            quantityInput.SetTextWithoutNotify(quantity.ToString(CultureInfo.InvariantCulture));
        }

        private void OnDurationChanged(string _value)
        {
            // Use positive finite seconds so temporary buffs always have an expiry
            float duration = float.TryParse(_value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) && !float.IsNaN(value) && !float.IsInfinity(value) ? Mathf.Max(0.01f, value) : 5.0f;
            durationInput.SetTextWithoutNotify(duration.ToString(CultureInfo.InvariantCulture));
        }

        private void OnModifierChanged(int _index)
        {
            // Normal buffs have no duration input
            durationInput.gameObject.SetActive(_index == 1);
        }

        private void OnGiveBuffsClicked()
        {
            // Resolve the current selection only after validating the target and authority
            if (!CanChangeBuffs() || givingBuffs != null || buffDropdown.value < 0 || buffDropdown.value >= buffs.Count) return;
            BuffDef buff = buffs[buffDropdown.value];
            OnQuantityChanged(quantityInput.text);
            int quantity = int.Parse(quantityInput.text, CultureInfo.InvariantCulture);
            if (modifierDropdown.value == 0) Utils.GiveBuff(character, buff, quantity);
            else if (modifierDropdown.value == 1)
            {
                OnDurationChanged(durationInput.text);
                float duration = float.Parse(durationInput.text, CultureInfo.InvariantCulture);
                if (!buff.canStack) Utils.GiveBuff(character, buff, 1, duration);
                else givingBuffs = StartCoroutine(GiveTimedBuffs(character, buff, quantity, duration));
            }
            UpdateControls();
        }

        private IEnumerator GiveTimedBuffs(CharacterBody _body, BuffDef _buff, int _quantity, float _duration)
        {
            // Store the coroutine before processing and spread large requests over frames
            yield return null;
            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
            int remaining = _quantity;
            while (remaining > 0 && CanChangeBuffs() && character == _body)
            {
                int batch = Mathf.Min(16, remaining);
                Utils.GiveBuff(_body, _buff, batch, _duration);
                remaining -= batch;
                if (timer.Elapsed.TotalMilliseconds >= 2.0)
                {
                    yield return null;
                    timer.Restart();
                }
            }
            givingBuffs = null;
        }

        private void StopGivingBuffs()
        {
            // A hidden panel or different target must not keep applying an old request
            if (givingBuffs == null) return;
            StopCoroutine(givingBuffs);
            givingBuffs = null;
        }

        private void OnClearBuffsClicked()
        {
            // Cancel pending additions before clearing normal and timed buffs together
            if (!CanChangeBuffs()) return;
            StopGivingBuffs();
            Utils.ClearBuffs(character);
        }

        private void CreateEntry(GameObject _row)
        {
            // Prepare controls and navigation once for each reusable entry
            _row.AddComponent<DebugCharacterBuffEntry>().Init(this);
            entryList.AddNavigation(_row);
        }

        private void BindEntry(GameObject _row, int _index)
        {
            // Bind each row to the character and buff group it currently represents
            var entry = entries[_index];
            _row.GetComponent<DebugCharacterBuffEntry>().Bind(character, entry.buff, entry.timed, entry.quantity);
        }

        public void RemoveBuff(CharacterBody _body, BuffDef _buff, bool _timed, bool _all)
        {
            // A recycled row must still refer to the currently selected living character
            if (!CanChangeBuffs() || _body == null || _body != character || _buff == null) return;
            Utils.RemoveBuff(_body, _buff, _timed, _all);
        }
    }

    internal class DebugCharacterBuffEntry : MonoBehaviour
    {
        // Store the owning panel and buff group represented by this entry
        DebugCharacterBuffs buffsPanel;
        CharacterBody character;
        BuffDef buff;
        bool timed;
        TextMeshProUGUI normalTitle;
        TextMeshProUGUI timedTitle;
        TextMeshProUGUI quantity;
        UnityEngine.UI.Button deleteButton;

        public void Init(DebugCharacterBuffs _buffsPanel)
        {
            // Find the prefab's existing controls and apply the debug menu fonts
            buffsPanel = _buffsPanel;
            normalTitle = Utils.FindChildWithTerm(transform, "NormalTitle").GetComponent<TextMeshProUGUI>();
            timedTitle = Utils.FindChildWithTerm(transform, "TimedTitle").GetComponent<TextMeshProUGUI>();
            quantity = Utils.FindChildWithTerm(transform, "Quantity").GetComponent<TextMeshProUGUI>();
            deleteButton = transform.Find("DeleteButton").GetComponent<UnityEngine.UI.Button>();
            deleteButton.onClick.AddListener(OnDeleteClicked);
            Utils.ApplyFonts(transform);
        }

        public void Bind(CharacterBody _character, BuffDef _buff, bool _timed, int _quantity)
        {
            // Show only the title matching this entry's normal or timed buff group
            character = _character;
            buff = _buff;
            timed = _timed;
            normalTitle.gameObject.SetActive(!timed);
            timedTitle.gameObject.SetActive(timed);
            TextMeshProUGUI title = timed ? timedTitle : normalTitle;
            title.text = $"{buff.name} [{(int)buff.buffIndex}]";
            quantity.text = _quantity.ToString();
            deleteButton.interactable = NetworkServer.active;
        }

        private void OnDeleteClicked()
        {
            // Either shift key removes every copy from this group only
            bool removeAll = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            buffsPanel.RemoveBuff(character, buff, timed, removeAll);
        }
    }
}
