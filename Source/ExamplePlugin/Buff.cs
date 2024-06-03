using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class Buff
    {
        // Buff def
        public BuffDef buffDef;

        // Buff token
        public string token;

        // Constructor
        public Buff(string _token, string _iconName, Color _colour, bool _canStack = true, bool _isDebuff = false, bool _isHidden = false)
        {
            // Should hide this buff due to temporary assets?
            bool forceHide = !Utils.debugMode && (_iconName == "texbufftemporalcube" || !Assets.HasAsset(_iconName));

            // Should hide anyway due to config?
            if (!forceHide)
            {
                forceHide = !Config.CheckTag(_token);
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
            buffDef.iconSprite = Assets.GetIcon(_iconName);

            // Add buff
            ContentAddition.AddBuffDef(buffDef);

            Log.Debug($"Created buff '{_token}'");

            if (forceHide)
            {
                if (!Config.CheckTag(_token))
                {
                    Log.Debug($"Hiding buff '{_token}' due to user preference");
                }
                else
                {
                    Log.Debug($"Hiding buff '{_token}' due to use of temporary assets outside of debug mode");
                }
            }
        }
    }
}
