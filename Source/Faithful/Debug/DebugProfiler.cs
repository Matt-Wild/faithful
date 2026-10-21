using System;
using System.Diagnostics;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Faithful
{
    internal class DebugProfiler : DebugPanel
    {
        // Store the reusable entry list and latest recorded results
        DebugScrollList entryList;
        FaithfulProfilerSnapshot snapshot;
        GameObject historyEntryPrefab;

        // Store the whole game's managed memory readings with the displayed snapshot
        TextMeshProUGUI memoryUsedValue;
        TextMeshProUGUI memoryReservedValue;

        // Store the current view and its labels and controls
        bool history = false;
        GameObject liveLabels;
        GameObject historyLabels;
        GameObject resetPeakButton;
        GameObject resetTotalButton;
        TextMeshProUGUI liveHistoryButtonText;

        // Store the freeze control and whether the displayed snapshot is frozen
        TextMeshProUGUI freezeButtonText;
        bool frozen = false;

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Toggle display updates without stopping runtime profiling
            UnityEngine.UI.Button freezeButton = transform.Find("FreezeButton").GetComponent<UnityEngine.UI.Button>();
            freezeButtonText = freezeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            freezeButtonText.text = "Freeze";
            freezeButton.onClick.AddListener(OnFreezeClicked);

            // Find the memory readings and log the displayed snapshot on demand
            Transform memoryUsedText = Utils.FindChildWithTerm(transform, "MemoryUsedText").transform;
            Transform memoryReservedText = Utils.FindChildWithTerm(transform, "MemoryReservedText").transform;
            memoryUsedValue = Utils.FindChildWithTerm(memoryUsedText, "MemoryUsedValue").GetComponent<TextMeshProUGUI>();
            memoryReservedValue = Utils.FindChildWithTerm(memoryReservedText, "MemoryReservedValue").GetComponent<TextMeshProUGUI>();
            transform.Find("LogButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnLogClicked);

            // Switch between the live window and retained history
            liveLabels = transform.Find("ProfilerLabels/Live").gameObject;
            historyLabels = transform.Find("ProfilerLabels/History").gameObject;
            UnityEngine.UI.Button liveHistoryButton = transform.Find("LiveHistoryButton").GetComponent<UnityEngine.UI.Button>();
            liveHistoryButtonText = liveHistoryButton.GetComponentInChildren<TextMeshProUGUI>(true);
            liveHistoryButton.onClick.AddListener(OnLiveHistoryClicked);

            // Keep the independent history resets available only in history mode
            resetPeakButton = transform.Find("ResetPeakButton").gameObject;
            resetTotalButton = transform.Find("ResetTotalButton").gameObject;
            resetPeakButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnResetPeakClicked);
            resetTotalButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnResetTotalClicked);
            UpdateView();

            // Find the existing scroll view and profiler entry template
            UnityEngine.UI.ScrollRect scrollRect = transform.Find("ProfilerContent")?.GetComponent<UnityEngine.UI.ScrollRect>();
            GameObject entryPrefab = Assets.GetObject("DebugProfilerEntry");
            historyEntryPrefab = Assets.GetObject("DebugProfilerHistoryEntry");
            if (scrollRect == null || entryPrefab == null || historyEntryPrefab == null)
            {
                Log.Error("[DEBUG MENU] | Profiler requires the ProfilerContent scroll view, DebugProfilerEntry and DebugProfilerHistoryEntry prefabs.");
                return;
            }

            // Only create enough entries to cover the visible portion of the profiler
            entryList = scrollRect.gameObject.AddComponent<DebugScrollList>();
            entryList.Init(scrollRect, (RectTransform)entryPrefab.transform, CreateEntry, BindEntry);
        }

        public void Refresh()
        {
            // Only capture and display results while the panel is visible
            if (!Utils.debugMode || !isActiveAndEnabled || minimised || frozen || entryList == null) return;

            snapshot = FaithfulRuntimeProfiler.GenerateSnapshot(history);
            string memoryUsed = FaithfulProfilerSnapshot.FormatMemory(snapshot.MemoryUsedBytes);
            string memoryReserved = FaithfulProfilerSnapshot.FormatMemory(snapshot.MemoryReservedBytes);
            if (memoryUsedValue.text != memoryUsed) memoryUsedValue.text = memoryUsed;
            if (memoryReservedValue.text != memoryReserved) memoryReservedValue.text = memoryReserved;
            entryList.RefreshItems(snapshot.Scopes.Length + 1);
        }

        private void OnLogClicked()
        {
            // Use the captured values, including while the display is frozen
            FaithfulRuntimeProfiler.LogSnapshot(snapshot);
        }

        private void OnFreezeClicked()
        {
            // Retain the current snapshot while frozen, then resume with the latest results
            frozen = !frozen;
            freezeButtonText.text = frozen ? "Unfreeze" : "Freeze";
            if (!frozen) Refresh();
        }

        private void CreateEntry(GameObject _row)
        {
            // Cache the entry's text components once for reuse
            _row.AddComponent<DebugProfilerEntry>().Init(historyEntryPrefab);
        }

        private void BindEntry(GameObject _row, int _index)
        {
            // Show the total first, followed by scopes ordered for the current view
            FaithfulProfilerScopeSnapshot scope = _index == 0 ? snapshot.Total : snapshot.Scopes[_index - 1];
            _row.GetComponent<DebugProfilerEntry>().Bind(scope, history);
        }

        private void UpdateView()
        {
            liveLabels.SetActive(!history);
            historyLabels.SetActive(history);
            resetPeakButton.SetActive(history);
            resetTotalButton.SetActive(history);
            liveHistoryButtonText.text = history ? "Live" : "History";
        }

        private void OnLiveHistoryClicked()
        {
            history = !history;
            UpdateView();

            // Reuse captured values when switching views while frozen
            if (entryList == null || snapshot.Scopes == null) return;
            snapshot = new FaithfulProfilerSnapshot(snapshot.Scopes, snapshot.Total, history, snapshot.MemoryUsedBytes, snapshot.MemoryReservedBytes);
            entryList.RefreshItems(snapshot.Scopes.Length + 1, true);
        }

        private void OnResetPeakClicked()
        {
            FaithfulRuntimeProfiler.ResetPeak();
            Refresh();
        }

        private void OnResetTotalClicked()
        {
            FaithfulRuntimeProfiler.ResetTotal();
            Refresh();
        }
    }

    internal class DebugProfilerEntry : MonoBehaviour
    {
        // Store the entry's text components
        TextMeshProUGUI scopeName;
        TextMeshProUGUI minimum;
        TextMeshProUGUI maximum;
        TextMeshProUGUI average;
        DebugProfilerHistoryEntry historyEntry;

        // Avoid rewriting text when the displayed values have not changed
        double displayedMinimum = double.NaN;
        double displayedMaximum = double.NaN;
        double displayedAverage = double.NaN;

        public void Init(GameObject _historyPrefab)
        {
            // Find the prefab's existing texts and apply the debug menu fonts
            scopeName = Utils.FindChildWithTerm(transform, "ScopeName").GetComponent<TextMeshProUGUI>();
            minimum = Utils.FindChildWithTerm(transform, "Min").GetComponent<TextMeshProUGUI>();
            maximum = Utils.FindChildWithTerm(transform, "Max").GetComponent<TextMeshProUGUI>();
            average = Utils.FindChildWithTerm(transform, "Average").GetComponent<TextMeshProUGUI>();
            Utils.ApplyFonts(transform);

            // Keep both entry prefabs pooled so changing views does not recreate the list
            GameObject historyRow = Instantiate(_historyPrefab, transform, false);
            RectTransform historyRect = (RectTransform)historyRow.transform;
            historyRect.anchorMin = Vector2.zero;
            historyRect.anchorMax = Vector2.one;
            historyRect.pivot = new Vector2(0.5f, 0.5f);
            historyRect.anchoredPosition = Vector2.zero;
            historyRect.sizeDelta = Vector2.zero;
            historyEntry = historyRow.AddComponent<DebugProfilerHistoryEntry>();
            historyEntry.Init();
            historyRow.SetActive(false);
        }

        public void Bind(FaithfulProfilerScopeSnapshot _scope, bool _history)
        {
            // Match the row layout to the labels shown by the panel
            if (historyEntry.gameObject.activeSelf != _history)
            {
                scopeName.gameObject.SetActive(!_history);
                minimum.gameObject.SetActive(!_history);
                maximum.gameObject.SetActive(!_history);
                average.gameObject.SetActive(!_history);
                historyEntry.gameObject.SetActive(_history);
            }
            if (_history)
            {
                historyEntry.Bind(_scope);
                return;
            }

            if (scopeName.text != _scope.Name) scopeName.text = _scope.Name;
            UpdateTime(minimum, _scope.MinElapsedTicks, ref displayedMinimum);
            UpdateTime(maximum, _scope.MaxElapsedTicks, ref displayedMaximum);
            UpdateTime(average, _scope.AverageElapsedTicks, ref displayedAverage);
        }

        internal static void UpdateTime(TextMeshProUGUI _text, double _ticks, ref double _displayed)
        {
            // Use microseconds for small values and at least three significant digits in seconds
            double milliseconds = _ticks * 1000.0 / Stopwatch.Frequency;
            double value;
            if (_ticks == 0) value = 0;
            else if (milliseconds < 0.001) value = -0.5;
            else if (milliseconds < 1) value = -Math.Round(milliseconds * 1000.0, MidpointRounding.AwayFromZero);
            else if (milliseconds < 1000) value = Math.Round(milliseconds, MidpointRounding.AwayFromZero);
            else
            {
                double seconds = milliseconds / 1000.0;
                int decimals = seconds < 10 ? 2 : seconds < 100 ? 1 : 0;
                value = Math.Round(seconds, decimals, MidpointRounding.AwayFromZero) * 1000.0;
            }
            if (_displayed == value) return;

            // Negative cached values represent microseconds; -0.5 represents less than one
            _displayed = value;
            if (value == 0) _text.text = "0\u00B5s";
            else if (value == -0.5) _text.text = "<1\u00B5s";
            else if (value < 0) _text.text = (-value).ToString("0", CultureInfo.InvariantCulture) + "\u00B5s";
            else if (value >= 1000)
            {
                double seconds = value / 1000.0;
                string format = seconds < 10 ? "0.00" : seconds < 100 ? "0.0" : "0";
                _text.text = seconds.ToString(format, CultureInfo.InvariantCulture) + "s";
            }
            else _text.text = value.ToString("0", CultureInfo.InvariantCulture) + "ms";
        }
    }

    internal class DebugProfilerHistoryEntry : MonoBehaviour
    {
        // Store the history prefab's texts and their displayed values
        TextMeshProUGUI scopeName;
        TextMeshProUGUI peak;
        TextMeshProUGUI total;
        double displayedPeak = double.NaN;
        double displayedTotal = double.NaN;

        public void Init()
        {
            scopeName = Utils.FindChildWithTerm(transform, "ScopeName").GetComponent<TextMeshProUGUI>();

            // The history prefab retains the original Min and Average object names
            peak = Utils.FindChildWithTerm(transform, "Min").GetComponent<TextMeshProUGUI>();
            total = Utils.FindChildWithTerm(transform, "Average").GetComponent<TextMeshProUGUI>();
            Utils.ApplyFonts(transform);
        }

        public void Bind(FaithfulProfilerScopeSnapshot _scope)
        {
            if (scopeName.text != _scope.Name) scopeName.text = _scope.Name;
            DebugProfilerEntry.UpdateTime(peak, _scope.PeakElapsedTicks, ref displayedPeak);
            DebugProfilerEntry.UpdateTime(total, _scope.TotalElapsedTicks, ref displayedTotal);
        }
    }
}
