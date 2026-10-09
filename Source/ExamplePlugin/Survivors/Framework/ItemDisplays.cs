using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Faithful
{
    internal static class ItemDisplays
    {
        internal enum PreparationState
        {
            Unprepared,
            Loading,
            Ready,
            Failed
        }

        private const string sourceRuleSetAddress = "RoR2/Base/Loader/idrsLoader.asset";
        private static Dictionary<string, GameObject> itemDisplayPrefabs = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, Object> keyAssets = new(StringComparer.Ordinal);
        private static Dictionary<object, GameObject> addressedDisplayPrefabs = new();
        private static readonly List<AsyncOperationHandle> retainedHandles = new();
        private static readonly List<GameObject> derivedDisplays = new();
        private static readonly HashSet<string> reportedIssues = new(StringComparer.Ordinal);
        private static Preparation activePreparation;
        private static bool shuttingDown;

        internal static PreparationState State { get; private set; }
        internal static bool HasConsumers { get; private set; }

        private sealed class Preparation
        {
            internal readonly Dictionary<string, GameObject> displays = new(StringComparer.OrdinalIgnoreCase);
            internal readonly Dictionary<string, Object> keys = new(StringComparer.Ordinal);
            internal readonly Dictionary<object, GameObject> addressedDisplays = new();
            internal readonly Dictionary<(Type, object), AsyncOperationHandle> loads = new();
            internal readonly List<AsyncOperationHandle> handles = new();
            internal readonly List<GameObject> clones = new();
        }

        internal static void RegisterConsumer()
        {
            HasConsumers = true;
        }

        // Prepare display assets before onContentPacksAssigned
        internal static IEnumerator PrepareAsync(Action<float> _reportProgress)
        {
            while (State == PreparationState.Loading && !shuttingDown)
            {
                yield return null;
            }

            if (shuttingDown || State == PreparationState.Ready || State == PreparationState.Failed)
            {
                _reportProgress?.Invoke(1f);
                yield break;
            }

            State = PreparationState.Loading;
            IEnumerator preparation = PrepareCoreAsync(_reportProgress);
            try
            {
                while (true)
                {
                    bool hasNext;
                    object next;
                    try
                    {
                        hasNext = preparation.MoveNext();
                        next = hasNext ? preparation.Current : null;
                    }
                    catch (Exception exception)
                    {
                        State = PreparationState.Failed;
                        Log.Error($"[CHARACTER ITEM DISPLAYS] | Preparation failed; survivor displays will be skipped.\n{exception}");
                        break;
                    }

                    if (!hasNext) break;
                    yield return next;
                }
            }
            finally
            {
                (preparation as IDisposable)?.Dispose();
            }

            _reportProgress?.Invoke(1f);
        }

        private static IEnumerator PrepareCoreAsync(Action<float> _reportProgress)
        {
            Preparation preparation = new();
            activePreparation = preparation;
            try
            {
                _reportProgress?.Invoke(0f);
                AsyncOperationHandle<ItemDisplayRuleSet> sourceHandle = LoadAsset<ItemDisplayRuleSet>(preparation, sourceRuleSetAddress);
                if (sourceHandle.IsValid() && !sourceHandle.IsDone) yield return sourceHandle;
                if (shuttingDown) yield break;

                if (!TryGetResult(sourceHandle, sourceRuleSetAddress, out ItemDisplayRuleSet sourceRuleSet) ||
                    sourceRuleSet.keyAssetRuleGroups == null || sourceRuleSet.keyAssetRuleGroups.Length == 0)
                {
                    Log.Error($"[CHARACTER ITEM DISPLAYS] | Could not prepare source rule set '{sourceRuleSetAddress}'; survivor displays will be skipped.");
                    yield break;
                }

                // Copy source rules to keep the shared rule set unchanged
                ItemDisplayRuleSet.KeyAssetRuleGroup[] groups = (ItemDisplayRuleSet.KeyAssetRuleGroup[])sourceRuleSet.keyAssetRuleGroups.Clone();
                for (int i = 0; i < groups.Length; i++)
                {
                    if (groups[i].displayRuleGroup.rules != null)
                    {
                        groups[i].displayRuleGroup.rules = (ItemDisplayRule[])groups[i].displayRuleGroup.rules.Clone();
                    }
                }
                for (int i = 0; i < groups.Length; i++)
                {
                    ItemDisplayRuleSet.KeyAssetRuleGroup group = groups[i];
                    Object keyAsset = group.keyAsset;
                    string groupContext = $"{sourceRuleSet.name}, group {i}";
                    if (!keyAsset && group.keyAssetAddress != null && group.keyAssetAddress.RuntimeKeyIsValid())
                    {
                        AsyncOperationHandle<ScriptableObject> keyHandle = LoadAsset<ScriptableObject>(preparation, group.keyAssetAddress.RuntimeKey);
                        if (keyHandle.IsValid() && !keyHandle.IsDone) yield return keyHandle;
                        if (shuttingDown) yield break;
                        if (TryGetResult(keyHandle, groupContext, out ScriptableObject loadedKey)) keyAsset = loadedKey;
                    }

                    if (!(keyAsset is ItemDef || keyAsset is EquipmentDef || keyAsset is KeyItemDef) || string.IsNullOrEmpty(keyAsset.name))
                    {
                        ReportIssue(groupContext, "The item/equipment key could not be resolved; its display group was omitted.");
                    }
                    else
                    {
                        preparation.keys[keyAsset.name] = keyAsset;
                        ItemDisplayRule[] rules = group.displayRuleGroup.rules;
                        if (rules != null)
                        {
                            for (int j = 0; j < rules.Length; j++)
                            {
                                ItemDisplayRule rule = rules[j];
                                if (rule.ruleType != ItemDisplayRuleType.ParentedPrefab) continue;

                                string ruleContext = $"{keyAsset.name}, source rule {j}";
                                GameObject prefab = rule.followerPrefab;
                                // Prefer addressable references to match CharacterModel
                                if (rule.followerPrefabAddress != null && rule.followerPrefabAddress.RuntimeKeyIsValid())
                                {
                                    AsyncOperationHandle<GameObject> prefabHandle = LoadAsset<GameObject>(preparation, rule.followerPrefabAddress.RuntimeKey);
                                    if (prefabHandle.IsValid() && !prefabHandle.IsDone) yield return prefabHandle;
                                    if (shuttingDown) yield break;
                                    prefab = TryGetResult(prefabHandle, ruleContext, out GameObject loadedPrefab) ? loadedPrefab : null;
                                }

                                if (!prefab || string.IsNullOrEmpty(prefab.name))
                                {
                                    ReportIssue(ruleContext, "The display prefab could not be resolved.");
                                    continue;
                                }

                                if (rule.followerPrefabAddress != null && rule.followerPrefabAddress.RuntimeKeyIsValid())
                                {
                                    preparation.addressedDisplays[rule.followerPrefabAddress.RuntimeKey] = prefab;
                                }
                                if (!preparation.displays.ContainsKey(prefab.name)) preparation.displays.Add(prefab.name, prefab);
                            }
                        }
                    }

                    _reportProgress?.Invoke(0.1f + 0.8f * (i + 1) / groups.Length);
                }

                TryCreateLimbMatchedDisplay(preparation, "DisplayLightningArmRight", "DisplayLightningCustom", "displaylightningarmcustom",
                    "LightningJoint1", "LightningJoint2", "LightningJointEnd");
                if (shuttingDown) yield break;

                // Keep loaded assets for as long as their display rules and clones are needed
                foreach (AsyncOperationHandle handle in preparation.handles)
                {
                    if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded) retainedHandles.Add(handle);
                    else if (handle.IsValid()) Addressables.Release(handle);
                }
                preparation.handles.Clear();
                derivedDisplays.AddRange(preparation.clones);
                preparation.clones.Clear();
                itemDisplayPrefabs = preparation.displays;
                keyAssets = preparation.keys;
                addressedDisplayPrefabs = preparation.addressedDisplays;
                State = PreparationState.Ready;
                Log.Debug($"[CHARACTER ITEM DISPLAYS] | Prepared {itemDisplayPrefabs.Count} displays and {keyAssets.Count} item/equipment keys.");
            }
            finally
            {
                DisposePreparation(preparation);
                activePreparation = null;
                if (State == PreparationState.Loading) State = PreparationState.Failed;
            }
        }

        private static AsyncOperationHandle<T> LoadAsset<T>(Preparation _preparation, object _key) where T : Object
        {
            if (shuttingDown) return default;
            var cacheKey = (typeof(T), _key);
            if (_preparation.loads.TryGetValue(cacheKey, out AsyncOperationHandle existing)) return existing.IsValid() ? existing.Convert<T>() : default;

            try
            {
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(_key);
                _preparation.loads.Add(cacheKey, handle);
                _preparation.handles.Add(handle);
                return handle;
            }
            catch (Exception exception)
            {
                _preparation.loads[cacheKey] = default;
                ReportIssue($"{typeof(T).Name}: {_key}", $"Could not request the asset: {exception.Message}");
                return default;
            }
        }

        private static bool TryGetResult<T>(AsyncOperationHandle<T> _handle, string _context, out T _asset) where T : Object
        {
            _asset = null;
            if (_handle.IsValid() && _handle.Status == AsyncOperationStatus.Succeeded && _handle.Result)
            {
                _asset = _handle.Result;
                return true;
            }

            string reason = _handle.IsValid() ? _handle.OperationException?.Message : "No valid load operation was created.";
            ReportIssue(_context, $"Asset loading failed: {reason ?? "The result was null."}");
            return false;
        }

        private static bool TryCreateLimbMatchedDisplay(Preparation _preparation, string _sourceName, string _prefabName, string _displayName, params string[] _targetLimbs)
        {
            if (!_preparation.displays.TryGetValue(_sourceName, out GameObject source) || !source)
            {
                ReportIssue(_displayName, $"The source display '{_sourceName}' is unavailable; the derived display was omitted.");
                return false;
            }

            LimbMatcher sourceMatcher = source.GetComponent<LimbMatcher>();
            if (!sourceMatcher || sourceMatcher.limbPairs == null || _targetLimbs == null ||
                _targetLimbs.Length == 0 || sourceMatcher.limbPairs.Length != _targetLimbs.Length ||
                Array.Exists(_targetLimbs, string.IsNullOrWhiteSpace))
            {
                ReportIssue(_displayName, $"Source '{_sourceName}' does not have a compatible limb mapping; the derived display was omitted.");
                return false;
            }

            GameObject display = null;
            try
            {
                display = R2API.PrefabAPI.InstantiateClone(source, _prefabName, false);
                LimbMatcher matcher = display.GetComponent<LimbMatcher>();
                matcher.limbPairs = (LimbMatcher.LimbPair[])matcher.limbPairs.Clone();
                for (int i = 0; i < _targetLimbs.Length; i++)
                {
                    matcher.limbPairs[i].targetChildLimb = _targetLimbs[i];
                    matcher.limbPairs[i].targetTransform = null;
                }

                _preparation.displays.Add(_displayName, display);
                _preparation.clones.Add(display);
                return true;
            }
            catch (Exception exception)
            {
                if (display) Object.Destroy(display);
                Log.Error($"[CHARACTER ITEM DISPLAYS] | Could not create derived display '{_displayName}' from '{_sourceName}'.\n{exception}");
                return false;
            }
        }

        internal static bool EnsureReady(string _owner)
        {
            if (!shuttingDown && State == PreparationState.Ready) return true;
            ReportIssue(_owner, $"Display preparation is {State}; display setup was skipped.");
            return false;
        }

        public static GameObject LoadDisplay(string _name)
        {
            if (State == PreparationState.Ready && !string.IsNullOrEmpty(_name) &&
                itemDisplayPrefabs.TryGetValue(_name, out GameObject display) && display) return display;

            ReportIssue(_name ?? "<unnamed display>", "The requested display is unavailable.");
            return null;
        }

        internal static bool TryCreateDisplayRuleGroup(string _owner, string _keyName, ItemDisplayRule[] _rules, out ItemDisplayRuleSet.KeyAssetRuleGroup _group)
        {
            _group = default;
            string context = $"{_owner}, item/equipment '{_keyName}'";
            if (State != PreparationState.Ready || string.IsNullOrEmpty(_keyName) ||
                !keyAssets.TryGetValue(_keyName, out Object keyAsset) || !keyAsset)
            {
                ReportIssue(context, "The key asset is unavailable; the display group was omitted.");
                return false;
            }

            List<ItemDisplayRule> validRules = new();
            if (_rules != null)
            {
                for (int i = 0; i < _rules.Length; i++)
                {
                    ItemDisplayRule rule = _rules[i];
                    if (rule.ruleType == ItemDisplayRuleType.LimbMask)
                    {
                        validRules.Add(rule);
                    }
                    else if (rule.ruleType == ItemDisplayRuleType.ParentedPrefab)
                    {
                        // Resolve prepared references without starting new loads
                        if (rule.followerPrefabAddress != null && rule.followerPrefabAddress.RuntimeKeyIsValid())
                        {
                            addressedDisplayPrefabs.TryGetValue(rule.followerPrefabAddress.RuntimeKey, out GameObject resolvedPrefab);
                            rule.followerPrefab = resolvedPrefab;
                        }
                        rule.followerPrefabAddress = null;
                        if (rule.followerPrefab && !string.IsNullOrWhiteSpace(rule.childName)) validRules.Add(rule);
                        else ReportIssue($"{context}, rule {i}", "The rule has no prepared prefab/attachment; it was omitted.");
                    }
                    else
                    {
                        ReportIssue($"{context}, rule {i}", "The rule has no resolved prefab/attachment or has an unsupported type; it was omitted.");
                    }
                }
            }

            if (validRules.Count == 0) return false;
            _group = new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = keyAsset,
                displayRuleGroup = new DisplayRuleGroup { rules = validRules.ToArray() }
            };
            return true;
        }

        public static ItemDisplayRule CreateDisplayRule(string _prefabName, string _childName, Vector3 _position, Vector3 _rotation, Vector3 _scale) => CreateDisplayRule(LoadDisplay(_prefabName), _childName, _position, _rotation, _scale);
        public static ItemDisplayRule CreateDisplayRule(GameObject _itemPrefab, string _childName, Vector3 _position, Vector3 _rotation, Vector3 _scale)
        {
            return new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = _childName,
                followerPrefab = _itemPrefab,
                limbMask = LimbFlags.None,
                localPos = _position,
                localAngles = _rotation,
                localScale = _scale
            };
        }

        public static ItemDisplayRule CreateLimbMaskDisplayRule(LimbFlags _limb)
        {
            return new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.LimbMask,
                limbMask = _limb,
                childName = ""
            };
        }

        private static void ReportIssue(string _context, string _reason)
        {
            string message = $"[CHARACTER ITEM DISPLAYS] | {_context}: {_reason}";
            if (reportedIssues.Add(message)) Log.Warning(message);
        }

        private static void DisposePreparation(Preparation _preparation)
        {
            foreach (GameObject clone in _preparation.clones)
            {
                if (clone) Object.Destroy(clone);
            }
            _preparation.clones.Clear();
            foreach (AsyncOperationHandle handle in _preparation.handles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _preparation.handles.Clear();
        }

        // Release display assets at shutdown - Rules remain in game content if the plugin is disabled
        internal static void Shutdown()
        {
            if (shuttingDown) return;
            shuttingDown = true;
            State = PreparationState.Failed;
            if (activePreparation != null) DisposePreparation(activePreparation);
            foreach (GameObject display in derivedDisplays)
            {
                if (display) Object.Destroy(display);
            }
            derivedDisplays.Clear();
            itemDisplayPrefabs.Clear();
            keyAssets.Clear();
            addressedDisplayPrefabs.Clear();
            foreach (AsyncOperationHandle handle in retainedHandles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            retainedHandles.Clear();
        }
    }
}
