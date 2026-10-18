using RoR2;

namespace Faithful
{
    /// <summary>
    /// Shared item options plus the display creation inputs used by WIP items.
    /// </summary>
    internal sealed class WIPItemOptions
    {
        public ItemOptions Item { get; }
        public bool CreateDisplaySettings { get; set; } = true;
        public string DisplayModelName { get; set; } = "temporalcubemesh";
        public ItemDisplayCallback DisplaySettingsCallback { get; set; } = null;

        public WIPItemOptions(string name = "WIP ITEM", ItemTag[] tags = null, string iconName = "texTemporalCubeIcon", string modelName = "temporalcubemesh")
        {
            Item = new ItemOptions(Utils.GetXMLSafeString(name).ToUpper(), name, tags ?? [], iconName, modelName)
            {
                WIP = true,
                OverrideName = name
            };
        }
    }
}
