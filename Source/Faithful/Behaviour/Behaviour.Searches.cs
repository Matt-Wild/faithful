using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using UnityEngine;

namespace Faithful
{
    internal delegate bool TryModifyTargetSearchMaskCallback(CharacterBody _character, object _source, ref TeamMask _mask);
    internal delegate void ModifyTargetSearchResultsCallback(CharacterBody _character, object _source, TeamMask _originalMask, List<HurtBox> _candidates);

    internal static partial class Behaviour
    {
        private static readonly List<TryModifyTargetSearchMaskCallback> tryModifyTargetSearchMaskCallbacks = new();
        private static readonly List<ModifyTargetSearchResultsCallback> modifyTargetSearchResultsCallbacks = new();

        // Searches without a viewer use the current state, or an explicit search source
        [ThreadStatic]
        private static TargetSearchContext currentTargetSearchContext;

        private sealed class TargetSearchContext(CharacterBody _character, object _source, Func<HurtBox, bool> _filter = null,
            Func<IEnumerable<HurtBox>, IEnumerable<HurtBox>> _order = null)
        {
            public readonly CharacterBody Character = _character;
            public readonly object Source = _source;
            public readonly EntityStateMachine StateMachine = currentStateMachine;
            public readonly Func<HurtBox, bool> Filter = _filter;
            public readonly Func<IEnumerable<HurtBox>, IEnumerable<HurtBox>> Order = _order;
        }

        // Keep the original mask and source with the collected candidates, without retaining the search itself
        private sealed class TargetSearchData
        {
            public CharacterBody Character;
            public object Source;
            public TeamMask OriginalMask;
            public TargetSearchContext Context;
        }

        private static readonly ConditionalWeakTable<BullseyeSearch, TargetSearchData> bullseyeSearchData = new();
        private static readonly ConditionalWeakTable<SphereSearch, TargetSearchData> sphereSearchData = new();

        private static readonly HookRegistration enemyDetectionTeamHook = new("EnemyDetection.IsEnemyTeam",
            () => On.EnemyDetection.IsEnemyTeam += HookEnemyDetectionTeam,
            () => On.EnemyDetection.IsEnemyTeam -= HookEnemyDetectionTeam);

        private static readonly HookRegistration lightningOrbTargetHook = new("LightningOrb.PickNextTarget",
            () => On.RoR2.Orbs.LightningOrb.PickNextTarget += HookLightningOrbTarget,
            () => On.RoR2.Orbs.LightningOrb.PickNextTarget -= HookLightningOrbTarget);

        private static readonly HookRegistration bullseyeSearchRefreshHook = new("BullseyeSearch.RefreshCandidates",
            () => On.RoR2.BullseyeSearch.RefreshCandidates += HookBullseyeSearchRefresh,
            () => On.RoR2.BullseyeSearch.RefreshCandidates -= HookBullseyeSearchRefresh);
        private static readonly HookRegistration bullseyeSearchResultsHook = new("BullseyeSearch.GetResults(List)",
            () => On.RoR2.BullseyeSearch.GetResults_List1 += HookBullseyeSearchResults,
            () => On.RoR2.BullseyeSearch.GetResults_List1 -= HookBullseyeSearchResults);
        private static readonly HookRegistration sphereSearchRefreshHook = new("SphereSearch.RefreshCandidates",
            () => On.RoR2.SphereSearch.RefreshCandidates += HookSphereSearchRefresh,
            () => On.RoR2.SphereSearch.RefreshCandidates -= HookSphereSearchRefresh);
        private static readonly HookRegistration sphereSearchTeamHook = new("SphereSearch.FilterCandidatesByHurtBoxTeam",
            () => On.RoR2.SphereSearch.FilterCandidatesByHurtBoxTeam += HookSphereSearchTeam,
            () => On.RoR2.SphereSearch.FilterCandidatesByHurtBoxTeam -= HookSphereSearchTeam);
        private static readonly HookRegistration sphereSearchResultsHook = new("SphereSearch.GetHurtBoxes(List)",
            () => On.RoR2.SphereSearch.GetHurtBoxes_List1 += HookSphereSearchResults,
            () => On.RoR2.SphereSearch.GetHurtBoxes_List1 -= HookSphereSearchResults);
        private static readonly HookRegistration sphereSearchArrayHook = new("SphereSearch.GetHurtBoxes()",
            () => On.RoR2.SphereSearch.GetHurtBoxes += HookSphereSearchArray,
            () => On.RoR2.SphereSearch.GetHurtBoxes -= HookSphereSearchArray);

        public static void AddTryModifyTargetSearchMaskCallback(TryModifyTargetSearchMaskCallback _callback)
        {
            tryModifyTargetSearchMaskCallbacks.Add(_callback);
            DebugLog("Added Try Modify Target Search Mask behaviour");
        }

        public static void AddModifyTargetSearchResultsCallback(ModifyTargetSearchResultsCallback _callback)
        {
            modifyTargetSearchResultsCallbacks.Add(_callback);
            DebugLog("Added Modify Target Search Results behaviour");
        }

        private static TargetSearchData PrepareTargetSearch(CharacterBody _viewer, ref TeamMask _mask)
        {
            TargetSearchContext context = currentTargetSearchContext;

            // A nested state belongs to its own source, even if entered during another search
            if (context != null && context.StateMachine != currentStateMachine) context = null;

            // An explicit source with no owner must not inherit the character firing it
            CharacterBody character = _viewer ? _viewer : (context != null ? context.Character : currentStateCharacter);
            object source = context != null ? context.Source : character;

            // Projectile states also have an owner, but no CharacterBody of their own
            if (!_viewer && context == null && currentStateMachine && currentStateMachine.commonComponents.projectileController)
            {
                ProjectileController projectile = currentStateMachine.commonComponents.projectileController;
                character = projectile.owner ? projectile.owner.GetComponent<CharacterBody>() : null;
                source = projectile;
            }

            if (!character || tryModifyTargetSearchMaskCallbacks.Count == 0) return null;
            if (context != null && context.Character != character)
            {
                context = null;
                source = character;
            }

            return PrepareTargetSearch(character, source, context, ref _mask);
        }

        private static TargetSearchData PrepareTargetSearch(CharacterBody _character, object _source, TargetSearchContext _context, ref TeamMask _mask)
        {
            if (!_character || tryModifyTargetSearchMaskCallbacks.Count == 0) return null;

            TeamMask originalMask = _mask;
            bool handled = false;
            foreach (TryModifyTargetSearchMaskCallback callback in tryModifyTargetSearchMaskCallbacks)
            {
                handled |= callback(_character, _source, ref _mask);
            }

            // Returning true also permits changing results without changing the mask
            return handled ? new TargetSearchData { Character = _character, Source = _source, OriginalMask = originalMask, Context = _context } : null;
        }

        private static bool HookEnemyDetectionTeam(On.EnemyDetection.orig_IsEnemyTeam orig, EnemyDetection self, GameObject otherGameObject)
        {
            if (!CanRunHook(enemyDetectionTeamHook)) return orig(self, otherGameObject);

            enemyDetectionTeamHook.StartProfiling();
            try
            {
                TeamMask originalMask = self.enemyTeamsMask;
                TeamMask mask = originalMask;

                // Trigger detection supplies its own source instead of inheriting a running state
                TargetSearchData data = PrepareTargetSearch(self.characterBody, self, null, ref mask);
                if (data == null)
                {
                    enemyDetectionTeamHook.StopProfiling();
                    try
                    {
                        return orig(self, otherGameObject);
                    }
                    finally
                    {
                        enemyDetectionTeamHook.StartProfiling();
                    }
                }

                bool detected;
                self.enemyTeamsMask = mask;
                enemyDetectionTeamHook.StopProfiling();
                try
                {
                    detected = orig(self, otherGameObject);
                }
                finally
                {
                    enemyDetectionTeamHook.StartProfiling();

                    // Keep the cached mask unchanged so charm expiry needs no refresh
                    self.enemyTeamsMask = originalMask;
                }

                if (!detected || !otherGameObject || !otherGameObject.TryGetComponent(out HurtBox hurtBox)) return false;

                // Apply the same candidate restrictions as the shared searches, including self and dead targets
                List<HurtBox> candidates = [hurtBox];
                ModifyTargetSearchResults(candidates, 0, data);
                return candidates.Contains(hurtBox);
            }
            finally
            {
                enemyDetectionTeamHook.StopProfiling();
            }
        }

        private static HurtBox HookLightningOrbTarget(On.RoR2.Orbs.LightningOrb.orig_PickNextTarget orig, LightningOrb self, Vector3 position)
        {
            if (!CanRunHook(lightningOrbTargetHook)) return orig(self, position);

            lightningOrbTargetHook.StartProfiling();
            TargetSearchContext previous = currentTargetSearchContext;
            try
            {
                // Later bounces run outside character states, but the orb still knows its attacker
                CharacterBody attacker = self.attacker ? self.attacker.GetComponent<CharacterBody>() : null;
                currentTargetSearchContext = new(attacker, self, candidate => !self.bouncedObjects.Contains(candidate.healthComponent));

                // Measure our context changes without including the original method
                lightningOrbTargetHook.StopProfiling();
                try
                {
                    return orig(self, position);
                }
                finally
                {
                    lightningOrbTargetHook.StartProfiling();
                }
            }
            finally
            {
                currentTargetSearchContext = previous;
                lightningOrbTargetHook.StopProfiling();
            }
        }

        private static void HookBullseyeSearchRefresh(On.RoR2.BullseyeSearch.orig_RefreshCandidates orig, BullseyeSearch self)
        {
            if (!CanRunHook(bullseyeSearchRefreshHook))
            {
                bullseyeSearchData.Remove(self);
                orig(self);
                return;
            }

            bullseyeSearchRefreshHook.StartProfiling();
            TeamMask originalMask = self.teamMaskFilter;
            try
            {
                bullseyeSearchData.Remove(self);
                TargetSearchData data = PrepareTargetSearch(self.viewer, ref self.teamMaskFilter);
                if (data != null) bullseyeSearchData.Add(self, data);

                bullseyeSearchRefreshHook.StopProfiling();
                try
                {
                    orig(self);
                }
                finally
                {
                    bullseyeSearchRefreshHook.StartProfiling();
                }
            }
            finally
            {
                // Reused searches, including missiles, must keep their original mask
                self.teamMaskFilter = originalMask;
                bullseyeSearchRefreshHook.StopProfiling();
            }
        }

        private static void HookBullseyeSearchResults(On.RoR2.BullseyeSearch.orig_GetResults_List1 orig, BullseyeSearch self, List<HurtBox> results)
        {
            if (!CanRunHook(bullseyeSearchResultsHook)) { orig(self, results); return; }

            bullseyeSearchResultsHook.StartProfiling();
            int start = results.Count;
            try
            {
                bullseyeSearchResultsHook.StopProfiling();
                try
                {
                    orig(self, results);
                }
                finally
                {
                    bullseyeSearchResultsHook.StartProfiling();
                }

                // The parameterless overload calls this overload, so both paths are handled once
                if (CanRunHook(bullseyeSearchResultsHook) && bullseyeSearchData.TryGetValue(self, out TargetSearchData data))
                    ModifyTargetSearchResults(results, start, data);
            }
            finally
            {
                bullseyeSearchResultsHook.StopProfiling();
            }
        }

        private static SphereSearch HookSphereSearchRefresh(On.RoR2.SphereSearch.orig_RefreshCandidates orig, SphereSearch self)
        {
            // A new query must not inherit a previous source or team filter
            sphereSearchRefreshHook.StartProfiling();
            try
            {
                sphereSearchData.Remove(self);
            }
            finally
            {
                sphereSearchRefreshHook.StopProfiling();
            }

            return orig(self);
        }

        private static SphereSearch HookSphereSearchTeam(On.RoR2.SphereSearch.orig_FilterCandidatesByHurtBoxTeam orig, SphereSearch self, TeamMask mask)
        {
            if (!CanRunHook(sphereSearchTeamHook)) return orig(self, mask);

            sphereSearchTeamHook.StartProfiling();
            try
            {
                TargetSearchData data = PrepareTargetSearch(null, ref mask);
                if (data != null)
                {
                    sphereSearchData.Remove(self);
                    sphereSearchData.Add(self, data);
                }
            }
            finally
            {
                sphereSearchTeamHook.StopProfiling();
            }

            return orig(self, mask);
        }

        private static void HookSphereSearchResults(On.RoR2.SphereSearch.orig_GetHurtBoxes_List1 orig, SphereSearch self, List<HurtBox> dest)
        {
            if (!CanRunHook(sphereSearchResultsHook)) { orig(self, dest); return; }

            sphereSearchResultsHook.StartProfiling();
            int start = dest.Count;
            try
            {
                sphereSearchResultsHook.StopProfiling();
                try
                {
                    orig(self, dest);
                }
                finally
                {
                    sphereSearchResultsHook.StartProfiling();
                }

                if (CanRunHook(sphereSearchResultsHook) && sphereSearchData.TryGetValue(self, out TargetSearchData data))
                    ModifyTargetSearchResults(dest, start, data);
            }
            finally
            {
                sphereSearchResultsHook.StopProfiling();
            }
        }

        private static HurtBox[] HookSphereSearchArray(On.RoR2.SphereSearch.orig_GetHurtBoxes orig, SphereSearch self)
        {
            HurtBox[] results = orig(self);

            // This overload does not call the list overload
            if (!CanRunHook(sphereSearchArrayHook) || !sphereSearchData.TryGetValue(self, out TargetSearchData data)) return results;

            sphereSearchArrayHook.StartProfiling();
            try
            {
                List<HurtBox> candidates = new(results);
                ModifyTargetSearchResults(candidates, 0, data);

                return candidates.ToArray();
            }
            finally
            {
                sphereSearchArrayHook.StopProfiling();
            }
        }

        private static void ModifyTargetSearchResults(List<HurtBox> _results, int _start, TargetSearchData _data)
        {
            if (!_data.Character || modifyTargetSearchResultsCallbacks.Count == 0) return;

            // Preserve entries already in a destination list and all native search restrictions
            List<HurtBox> candidates = _results.GetRange(_start, _results.Count - _start);
            TargetSearchContext context = _data.Context;
            candidates.RemoveAll(candidate => !candidate || !candidate.healthComponent || (context?.Filter != null && !context.Filter(candidate)));
            if (context?.Order != null) candidates = context.Order(candidates).ToList();

            foreach (ModifyTargetSearchResultsCallback callback in modifyTargetSearchResultsCallbacks)
            {
                callback(_data.Character, _data.Source, _data.OriginalMask, candidates);
            }

            // Known single-target callers may otherwise override the callback's preference afterwards
            if (context != null && candidates.Count > 1) candidates.RemoveRange(1, candidates.Count - 1);

            _results.RemoveRange(_start, _results.Count - _start);
            _results.AddRange(candidates);
        }
    }
}
