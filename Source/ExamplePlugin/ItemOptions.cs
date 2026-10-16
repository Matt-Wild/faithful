using RoR2;

namespace Faithful
{
    /// <summary>
    /// Inputs used to create an item. Defaults match the item registration defaults.
    /// </summary>
    internal sealed class ItemOptions
    {
        public string Token { get; }
        public string SafeName { get; }
        public ItemTag[] Tags { get; }
        public string IconName { get; }
        public string ModelName { get; }

        public ItemTier Tier { get; set; } = ItemTier.Tier1;
        public bool SimulacrumBanned { get; set; } = false;
        public bool CanRemove { get; set; } = true;
        public bool Hidden { get; set; } = false;
        public string CorruptToken { get; set; } = null;
        public ItemDisplaySettings DisplaySettings { get; set; } = null;
        public ModifyPrefabCallback ModifyItemModelPrefabCallback { get; set; } = null;
        public ModifyPrefabCallback ModifyItemDisplayPrefabCallback { get; set; } = null;
        public bool CanNeverBeTemporary { get; set; } = false;
        public bool CanNeverBeEthereal { get; set; } = false;
        public bool DebugOnly { get; set; } = false;
        public bool WIP { get; set; } = false;
        public string OverrideName { get; set; } = null;
        public string OverridePickup { get; set; } = null;
        public string OverrideDescription { get; set; } = null;
        public string OverrideLore { get; set; } = null;
        public string NamePrefix { get; set; } = null;
        public bool HiddenFromLogbook { get; set; } = false;
        public bool SupportsQuality { get; set; } = false;

        public ItemOptions(string token, string safeName, ItemTag[] tags, string iconName, string modelName)
        {
            Token = token;
            SafeName = safeName;
            Tags = tags;
            IconName = iconName;
            ModelName = modelName;
        }

        // Snapshot property values before callbacks run, retaining the original referenced objects
        internal ItemOptions Copy() => (ItemOptions)MemberwiseClone();
    }
}
