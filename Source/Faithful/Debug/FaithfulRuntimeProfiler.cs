using System.Collections.Generic;
using System.Diagnostics;

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

            // Start scope
            scopes[_name].Start();
        }

        public static void StopScope(string _name)
        {
            // Ignore if debug mode not enabled
            if (!Utils.debugMode) return;

            // Stop scope
            if (scopes.TryGetValue(_name, out FaithfulProfilerScope scope)) scope.Stop();
        }

        public static void Record()
        {
            // Ignore if debug mode not enabled
            if (!Utils.debugMode) return;

            // Record each scope's accumulated time once per frame, including unused scopes
            foreach (FaithfulProfilerScope scope in scopes.Values) totalScope.AddElapsedTicks(scope.Record());
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

            return new FaithfulProfilerSnapshot(snapshots, totalScope.Snapshot, _history);
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

            public void Stop()
            {
                if (depth == 0) return;

                // Accumulate once all nested calls have finished
                depth--;
                if (depth == 0)
                {
                    AddElapsedTicks(Stopwatch.GetTimestamp() - startTime);
                }
            }

            public void AddElapsedTicks(long _elapsedTicks)
            {
                accumulatedTicks += _elapsedTicks;
            }

            public long Record()
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
                return elapsedTicks;
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

        public FaithfulProfilerSnapshot(FaithfulProfilerScopeSnapshot[] _scopes, FaithfulProfilerScopeSnapshot _total, bool _history = false)
        {
            Scopes = _scopes;
            Total = _total;

            // Show the most expensive scopes first
            if (_history) System.Array.Sort(Scopes, (left, right) => right.TotalElapsedTicks.CompareTo(left.TotalElapsedTicks));
            else System.Array.Sort(Scopes, (left, right) => right.AverageElapsedTicks.CompareTo(left.AverageElapsedTicks));
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
