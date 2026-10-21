using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Faithful
{
    internal static class FaithfulRuntimeProfiler
    {
        private static Dictionary<string, FaithfulProfilerScope> scopes = [];
        private static FaithfulProfilerScope totalScope = new("Total");
        
        private static void RegisterScope(string _name)
        {
            // Check if scope already exists
            if (scopes.ContainsKey(_name)) return;

            // Create new scope
            scopes[_name] = new(_name);
        }

        public static void StartScope(string _name)
        {
            // Ignore if debug mode not enabled
            if (!Utils.debugMode) return;

            // Ensure scope is registered
            RegisterScope(_name);

            // Measure the total while any scope is active (avoids counting nested work twice)
            totalScope.Start();
            scopes[_name].Start();
        }

        public static void StopScope(string _name)
        {
            // Ignore if debug mode not enabled
            if (!Utils.debugMode) return;

            // Only finish a total start when this scope has a matching active start
            if (scopes.TryGetValue(_name, out FaithfulProfilerScope scope) && scope.Stop()) totalScope.Stop();
        }

        public static void Record()
        {
            // Ignore if debug mode not enabled
            if (!Utils.debugMode) return;

            // Record each scope's accumulated time, including unused scopes
            foreach (FaithfulProfilerScope scope in scopes.Values) scope.Record();

            // The shared scope records the combined active intervals independently
            totalScope.Record();
        }

        public static FaithfulProfilerSnapshot GenerateSnapshot(bool _history = false)
        {
            // Ignore if debug mode not enabled
            if (!Utils.debugMode) return default;

            // Capture the recorded results for each scope
            FaithfulProfilerScopeSnapshot[] snapshots = new FaithfulProfilerScopeSnapshot[scopes.Count];

            int index = 0;
            foreach (FaithfulProfilerScope scope in scopes.Values) snapshots[index++] = scope.Snapshot;

            // Capture managed memory for the whole game without forcing garbage collection
            long memoryUsed = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
            long memoryReserved = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong();
            return new FaithfulProfilerSnapshot(snapshots, totalScope.Snapshot, _history, memoryUsed, memoryReserved);
        }

        public static void LogSnapshot(FaithfulProfilerSnapshot _snapshot)
        {
            if (!Utils.debugMode || _snapshot.Scopes == null) return;

            // Include every scope and both views in one log entry
            StringBuilder message = new("[Profiler] Snapshot\n");
            message.AppendLine("Managed memory (whole game, including other mods):");
            message.AppendLine($"  Used: {FaithfulProfilerSnapshot.FormatMemory(_snapshot.MemoryUsedBytes)} ({_snapshot.MemoryUsedBytes} bytes)");
            message.AppendLine($"  Reserved: {FaithfulProfilerSnapshot.FormatMemory(_snapshot.MemoryReservedBytes)} ({_snapshot.MemoryReservedBytes} bytes)");
            message.AppendLine("Live timings are per frame. History retains cumulative time and the highest frame.");
            message.AppendLine("The Total scope counts overlapping scopes once; individual scopes can overlap.");

            // Use labelled blocks so proportional console fonts do not misalign values
            for (int i = 0; i <= _snapshot.Scopes.Length; i++)
            {
                FaithfulProfilerScopeSnapshot scope = i == 0 ? _snapshot.Total : _snapshot.Scopes[i - 1];
                message.AppendLine();
                message.AppendLine(scope.Name);
                message.AppendLine($"  Live: Avg {FormatTime(scope.AverageElapsedTicks)} | Min {FormatTime(scope.MinElapsedTicks)} | Max {FormatTime(scope.MaxElapsedTicks)}");
                message.AppendLine($"  History: Total {FormatTime(scope.TotalElapsedTicks)} | Peak {FormatTime(scope.PeakElapsedTicks)}");
            }
            Log.Info(message.ToString());
        }

        private static string FormatTime(double _ticks)
        {
            // Keep small timings readable without lengthy fractions of a millisecond
            if (_ticks == 0) return "0\u00B5s";
            double microseconds = _ticks * 1000000.0 / Stopwatch.Frequency;
            if (microseconds < 1) return "<1\u00B5s";
            if (microseconds < 1000) return microseconds.ToString("0.##", CultureInfo.InvariantCulture) + "\u00B5s";
            double milliseconds = microseconds / 1000.0;
            if (milliseconds < 1000) return milliseconds.ToString("0.##", CultureInfo.InvariantCulture) + "ms";

            // Retain at least three significant digits for cumulative times in seconds
            double seconds = milliseconds / 1000.0;
            string format = seconds < 10 ? "0.00" : seconds < 100 ? "0.0" : "0";
            return seconds.ToString(format, CultureInfo.InvariantCulture) + "s";
        }

        public static void ResetPeak()
        {
            if (!Utils.debugMode) return;

            // Reset retained peaks without changing the live window or accumulated totals
            foreach (FaithfulProfilerScope scope in scopes.Values) scope.ResetPeak();
            totalScope.ResetPeak();
        }

        public static void ResetTotal()
        {
            if (!Utils.debugMode) return;

            // Reset accumulated totals without changing the live window or retained peaks
            foreach (FaithfulProfilerScope scope in scopes.Values) scope.ResetTotal();
            totalScope.ResetTotal();
        }

        private class FaithfulProfilerScope
        {
            private string name;

            private readonly long[] elapsedTimes = new long[128];
            private int nextRecordIndex = 0;
            private int recordCount = 0;
            private long accumulatedTicks = 0;
            private long startTime;
            private int depth = 0;
            private long totalElapsedTicks = 0;
            private long peakElapsedTicks = 0;

            public FaithfulProfilerScope(string _name)
            {
                name = _name;

                if (Utils.verboseConsole) Log.Debug($"[Profiler] Created scope: {name}");
            }

            public void Start()
            {
                // Begin timing only the outermost call
                if (depth == 0) startTime = Stopwatch.GetTimestamp();
                depth++;
            }

            public bool Stop()
            {
                if (depth == 0) return false;

                // Accumulate once all nested calls have finished
                depth--;
                if (depth == 0) AddElapsedTicks(Stopwatch.GetTimestamp() - startTime);
                return true;
            }

            public void AddElapsedTicks(long _elapsedTicks)
            {
                accumulatedTicks += _elapsedTicks;
            }

            public void Record()
            {
                // Store the total from completed measurements and begin a new frame
                long elapsedTicks = accumulatedTicks;
                elapsedTimes[nextRecordIndex] = elapsedTicks;
                nextRecordIndex = (nextRecordIndex + 1) % elapsedTimes.Length;
                if (recordCount < elapsedTimes.Length) recordCount++;
                accumulatedTicks = 0;

                // Retain history independently of the rolling live window
                totalElapsedTicks += elapsedTicks;
                if (elapsedTicks > peakElapsedTicks) peakElapsedTicks = elapsedTicks;
            }

            public void ResetPeak() => peakElapsedTicks = 0;

            public void ResetTotal() => totalElapsedTicks = 0;

            public FaithfulProfilerScopeSnapshot Snapshot => new(name, MinElapsedTicks, MaxElapsedTicks, AverageElapsedTicks, totalElapsedTicks, peakElapsedTicks);

            public long MinElapsedTicks
            {
                get
                {
                    if (recordCount == 0) return 0;

                    long minimum = elapsedTimes[0];
                    for (int i = 1; i < recordCount; i++)
                    {
                        if (elapsedTimes[i] < minimum) minimum = elapsedTimes[i];
                    }

                    return minimum;
                }
            }

            public long MaxElapsedTicks
            {
                get
                {
                    long maximum = 0;
                    for (int i = 0; i < recordCount; i++)
                    {
                        if (elapsedTimes[i] > maximum) maximum = elapsedTimes[i];
                    }

                    return maximum;
                }
            }

            public double AverageElapsedTicks
            {
                get
                {
                    if (recordCount == 0) return 0;

                    double total = 0;
                    for (int i = 0; i < recordCount; i++) total += elapsedTimes[i];

                    return total / recordCount;
                }
            }
        }
    }

    internal readonly struct FaithfulProfilerSnapshot
    {
        public readonly FaithfulProfilerScopeSnapshot[] Scopes { get; }
        public readonly FaithfulProfilerScopeSnapshot Total { get; }
        public readonly long MemoryUsedBytes { get; }
        public readonly long MemoryReservedBytes { get; }

        public FaithfulProfilerSnapshot(FaithfulProfilerScopeSnapshot[] _scopes, FaithfulProfilerScopeSnapshot _total, bool _history = false, long _memoryUsedBytes = 0, long _memoryReservedBytes = 0)
        {
            Scopes = _scopes;
            Total = _total;
            MemoryUsedBytes = _memoryUsedBytes;
            MemoryReservedBytes = _memoryReservedBytes;

            // Show the most expensive scopes first
            if (_history) System.Array.Sort(Scopes, (left, right) => right.TotalElapsedTicks.CompareTo(left.TotalElapsedTicks));
            else System.Array.Sort(Scopes, (left, right) => right.AverageElapsedTicks.CompareTo(left.AverageElapsedTicks));
        }

        internal static string FormatMemory(long _bytes)
        {
            // Choose a readable unit for the current memory size
            if (_bytes < 1024) return _bytes.ToString(CultureInfo.InvariantCulture) + "B";
            if (_bytes < 1024L * 1024) return (_bytes / 1024.0).ToString("0.##", CultureInfo.InvariantCulture) + "KiB";
            if (_bytes < 1024L * 1024 * 1024) return (_bytes / (1024.0 * 1024)).ToString("0.##", CultureInfo.InvariantCulture) + "MiB";
            return (_bytes / (1024.0 * 1024 * 1024)).ToString("0.##", CultureInfo.InvariantCulture) + "GiB";
        }
    }

    internal readonly struct FaithfulProfilerScopeSnapshot
    {
        public readonly string Name { get; }
        public readonly long MinElapsedTicks { get; }
        public readonly long MaxElapsedTicks { get; }
        public readonly double AverageElapsedTicks { get; }
        public readonly long TotalElapsedTicks { get; }
        public readonly long PeakElapsedTicks { get; }

        public FaithfulProfilerScopeSnapshot(string _name, long _min, long _max, double _avg, long _total = 0, long _peak = 0)
        {
            Name = _name;
            MinElapsedTicks = _min;
            MaxElapsedTicks = _max;
            AverageElapsedTicks = _avg;
            TotalElapsedTicks = _total;
            PeakElapsedTicks = _peak;
        }
    }
}
