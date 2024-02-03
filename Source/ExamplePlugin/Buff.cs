using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class Buff
    {
        // Toolbox
        protected Toolbox toolbox;

        // Buff def
        public BuffDef buffDef;

        // Buff token
        public string token;

        // Constructor
        public Buff(Toolbox _toolbox, string _token, string _iconName, Color _colour, bool _canStack = true, bool _isDebuff = false, bool _isHidden = false)
        {
            toolbox = _toolbox;

            // Should hide this buff due to temporary assets?
            bool forceHide = !toolbox.utils.debugMode && _iconName == "texbufftemporalcube";
            if (forceHide)
            {
                Log.Debug($"Hiding buff '{_token}' due to use of temporary assets outside of debug mode");
            }

            // Assign token
            token = _token;

            // Create buff def
            buffDef = ScriptableObject.CreateInstance<BuffDef>();

            // Set buff token
            buffDef.name = $"FAITHFUL_{_token}_BUFF";

            // Set buff colour
            buffDef.buffColor = _colour;

            // Set buff configs
            buffDef.canStack = _canStack;
            buffDef.isDebuff = _isDebuff;

            // Set if buff is hidden (Also hide if using temporary assets when not in debug mode)
            buffDef.isHidden = _isHidden || forceHide;

            // Set icon
            buffDef.iconSprite = toolbox.assets.GetIcon(_iconName);

            // Add buff
            ContentAddition.AddBuffDef(buffDef);

            Log.Debug($"Created buff '{_token}'");
        }
    }
}
