using BepInEx;
using RoR2;
using RoR2.Stats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;
using Path = System.IO.Path;

namespace Faithful
{
    internal static class FaithfulProgressSaving
    {
        private static Setting<bool> progressSaving;
        private static Setting<bool> usePersistentDataPath;

        private const string SAVE_DIR = "ModdedSaves/Faithful";

        // Cache each profile separately, including backups that could not be loaded safely
        private static readonly Dictionary<string, ProgressData> loadedProgress = new();
        private static bool ready;

        internal static void Init()
        {
            progressSaving = Config.CreateSetting("PROGRESS_SAVING", "Saving", "Progress Saving", true, "Back up and restore Faithful logbook progress independently of the normal game save.", false, true, _restartRequired: true);
            usePersistentDataPath = Config.CreateSetting("USE_PERSISTENT_DATA_PATH", "Saving", "Use Persistent Data Path", true, "Share Faithful progress between modpacks using the game's persistent data directory. Disable to store it within this modpack's BepInEx directory instead.", false, true, _restartRequired: true);

            if (!progressSaving.Value) return;

            Behaviour.AddProfileSavedCallback(OnProfileSaved);
            Behaviour.AddUserSignInCallback(OnUserSignIn);
            RoR2Application.onLoad += OnGameLoaded;
        }

        private static void OnGameLoaded()
        {
            if (!Faithful.InitializationSucceeded) return;

            // Wait until content catalogs and profile stats are ready
            ready = true;

            foreach (LocalUser user in LocalUserManager.readOnlyLocalUsersList)
            {
                OnUserSignIn(user);
            }
        }

        private static bool CanSave(UserProfile _profile) =>
            ready && progressSaving.Value && _profile != null && _profile.canSave &&
            !_profile.isCorrupted && !string.IsNullOrEmpty(_profile.fileName);

        private static void OnUserSignIn(LocalUser _user)
        {
            UserProfile profile = _user?.userProfile;
            if (!CanSave(profile)) return;

            try
            {
                ProgressData progress = Load(profile);
                if (progress == null) return;

                if (RestoreItems(profile, progress.items))
                {
                    // Let the normal save cycle persist restored progress
                    PlatformSystems.saveSystem.RequestSave(profile);
                }

                // Back up any existing profile progress missing from the custom save
                Save(profile, progress);
            }
            catch (Exception exception)
            {
                Log.Warning($"[Progress] Could not restore profile '{profile.fileName}'.\n{exception}");
            }
        }

        private static void OnProfileSaved(UserProfile _profile)
        {
            if (!CanSave(_profile)) return;

            try
            {
                ProgressData progress = Load(_profile);
                if (progress != null) Save(_profile, progress);
            }
            catch (Exception exception)
            {
                Log.Warning($"[Progress] Could not back up profile '{_profile.fileName}'.\n{exception}");
            }
        }

        private static string GetSavePath(UserProfile _profile)
        {
            // Use the profile being processed, including during logout and profile switching
            string basePath = usePersistentDataPath.Value ? Application.persistentDataPath : Paths.BepInExRootPath;
            return Path.Combine(basePath, SAVE_DIR, _profile.fileName + ".dat");
        }

        private static ProgressData Load(UserProfile _profile)
        {
            string path = GetSavePath(_profile);
            if (loadedProgress.TryGetValue(path, out ProgressData progress)) return progress;

            try
            {
                if (File.Exists(path))
                {
                    progress = Read(path);
                    progress.lastSavedJson = JsonUtility.ToJson(progress, true);
                }
                else if (File.Exists(path + ".bak"))
                {
                    progress = Read(path + ".bak");
                }
                else
                {
                    progress = new ProgressData();
                }
            }
            catch (Exception exception)
            {
                // Leave unreadable saves intact instead of replacing them with empty progress
                Log.Warning($"[Progress] Could not load '{path}'. Progress saving for this profile is unavailable until restart.\n{exception}");
                progress = null;
            }

            loadedProgress[path] = progress;
            return progress;
        }

        private static ProgressData Read(string _path)
        {
            ProgressData progress = JsonUtility.FromJson<ProgressData>(File.ReadAllText(_path));
            if (progress == null || progress.version != 1 || progress.items == null)
                throw new InvalidDataException("Unsupported or incomplete Faithful progress save.");

            HashSet<string> names = new();
            foreach (ItemProgress item in progress.items)
            {
                if (item == null || string.IsNullOrEmpty(item.name) || !names.Add(item.name))
                    throw new InvalidDataException("Invalid or duplicate item in Faithful progress save.");
            }

            return progress;
        }

        private static void Save(UserProfile _profile, ProgressData _progress)
        {
            CaptureItems(_profile, _progress.items);
            string json = JsonUtility.ToJson(_progress, true);
            if (json == _progress.lastSavedJson) return;

            string path = GetSavePath(_profile);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Retain one backup
            string temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, json);
            if (File.Exists(path)) File.Replace(temporaryPath, path, path + ".bak");
            else File.Move(temporaryPath, path);

            _progress.lastSavedJson = json;
        }

        private static void CaptureItems(UserProfile _profile, List<ItemProgress> _items)
        {
            foreach (Item item in Items.AllItems)
            {
                ItemDef itemDef = item.itemDef;
                if (!itemDef || itemDef.itemIndex == ItemIndex.None) continue;

                PickupIndex pickup = PickupCatalog.FindPickupIndex(itemDef.itemIndex);
                bool discovered = _profile.HasDiscoveredPickup(pickup);
                ulong total = _profile.statSheet.GetStatValueULong(PerItemStatDef.totalCollected.FindStatDef(itemDef.itemIndex));
                ulong highest = _profile.statSheet.GetStatValueULong(PerItemStatDef.highestCollected.FindStatDef(itemDef.itemIndex));

                ItemProgress saved = _items.Find(entry => entry.name == itemDef.name);
                if (saved == null)
                {
                    if (!discovered && total == 0 && highest == 0) continue;

                    saved = new ItemProgress { name = itemDef.name };
                    _items.Add(saved);
                }

                // Preserve progress for missing content or a profile saved without Faithful
                saved.discovered |= discovered;
                saved.totalCollected = Math.Max(saved.totalCollected, total);
                saved.highestCollected = Math.Max(saved.highestCollected, highest);
            }
        }

        private static bool RestoreItems(UserProfile _profile, List<ItemProgress> _items)
        {
            bool changed = false;

            foreach (Item item in Items.AllItems)
            {
                ItemDef itemDef = item.itemDef;
                if (!itemDef || itemDef.itemIndex == ItemIndex.None) continue;

                ItemProgress saved = _items.Find(entry => entry.name == itemDef.name);
                if (saved == null) continue;

                PickupIndex pickup = PickupCatalog.FindPickupIndex(itemDef.itemIndex);
                if (saved.discovered && pickup.isValid && !_profile.HasDiscoveredPickup(pickup))
                {
                    _profile.DiscoverPickup(pickup);
                    changed = true;
                }

                changed |= RestoreStat(_profile.statSheet, PerItemStatDef.totalCollected.FindStatDef(itemDef.itemIndex), saved.totalCollected);
                changed |= RestoreStat(_profile.statSheet, PerItemStatDef.highestCollected.FindStatDef(itemDef.itemIndex), saved.highestCollected);
            }

            return changed;
        }

        private static bool RestoreStat(StatSheet _stats, StatDef _stat, ulong _value)
        {
            if (_stat == null || _stats.GetStatValueULong(_stat) >= _value) return false;

            // Restore the value directly so repeated loads cannot add the same total again
            _stats.SetStatValueFromString(_stat, _value.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        [Serializable]
        private class ProgressData
        {
            public int version = 1;
            public List<ItemProgress> items = new();

            [NonSerialized] public string lastSavedJson;
        }

        [Serializable]
        private class ItemProgress
        {
            public string name;
            public bool discovered;
            public ulong totalCollected;
            public ulong highestCollected;
        }
    }
}
