using BepInEx;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using HarmonyLib;

namespace Faithful
{
    // This is an example plugin that can be put in
    // BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    // It's a small plugin that adds a relatively simple item to the game,
    // and gives you that item whenever you press F2.

    // This attribute specifies that we have a dependency on a given BepInEx Plugin,
    // We need the R2API ItemAPI dependency because we are using for adding our item to the game.
    // You don't need this if you're not using R2API in your plugin,
    // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(ItemAPI.PluginGUID)]

    // This one is because we use a .language file for language tokens
    // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInDependency(PrefabAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class Faithful : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SpilledSoup";
        public const string PluginName = "Faithful";
        public const string PluginVersion = "1.3.28";

        // Plugin info
        public static PluginInfo PInfo { get; private set; }

        // Toolbox
        Toolbox toolbox;

        // Buffs
        internal static GodMode godMode;
        internal static Vengeance vengeance;
        internal static Patience patience;
        internal static Inspiration inspiration;
        internal static Overclocked overclocked;
        internal static Repair repair;

        // Items
        internal static CopperGear copperGear;
        internal static TargetingMatrix targetingMatrix;
        internal static BrassScrews brassScrews;
        internal static MeltingWarbler meltingWarbler;
        internal static VengefulToaster vengefulToaster;
        internal static HermitsShawl hermitsShawl;
        internal static SpaciousUmbrella spaciousUmbrella;
        internal static SecondHand secondHand;
        internal static LeadersPennon leadersPennon;
        internal static DrownedVisage drownedVisage;
        internal static LongshotGeode longshotGeode;
        internal static HasteningGreave hasteningGreave;
        internal static CauterizingGreave cauterizingGreave;
        internal static NoxiousSlime noxiousSlime;
        internal static CollectorsVision collectorsVision;
        internal static AppraisersEye appraisersEye;
        internal static TJetpack tJetpack;
        internal static MatterAccelerator matterAccelerator;
        internal static RadiantTimepiece radiantTimepiece;
        internal static DebugItem debugItem;

        // Survivors
        internal static Technician technician;

        // Interactables
        internal static Interactable recollectionShrine;

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Update plugin info
            PInfo = Info;

            // Pass GUID and mod name to Risk of Options wrapper
            RiskOfOptionsWrapper.pluginGUID = PluginGUID;
            RiskOfOptionsWrapper.pluginName = PluginName;

            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            // Initialise toolbox
            toolbox = new Toolbox(this, PInfo, Config);

            // Initialise languages
            Languages.Init();

            // Create God Mode if in debug
            godMode = new GodMode(toolbox);

            // Add net utils onto player prefab
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Core/PlayerMaster.prefab").Completed += delegate (AsyncOperationHandle<GameObject> obj)
            {
                if (obj.Result)
                {
                    obj.Result.AddComponent<NetUtils>();
                }
            };

            // Use harmony to patch methods inaccessible to mmhook
            Harmony harmony = new Harmony("com.faithful.patcher");
            AkSoundEngineDynamicPatcher.Init();
            AkSoundEngineDynamicPatcher.PatchAll(harmony);
            if (Utils.verboseConsole) Log.Info("Patched all additional methods for faithful.");

            // Create buffs
            vengeance = new Vengeance(toolbox);
            patience = new Patience(toolbox);
            inspiration = new Inspiration(toolbox);
            overclocked = new Overclocked();
            repair = new Repair();

            // Create items
            copperGear = new CopperGear(toolbox);
            targetingMatrix = new TargetingMatrix(toolbox);
            brassScrews = new BrassScrews(toolbox);
            meltingWarbler = new MeltingWarbler(toolbox);
            vengefulToaster = new VengefulToaster(toolbox, vengeance);
            hermitsShawl = new HermitsShawl(toolbox, patience);
            spaciousUmbrella = new SpaciousUmbrella(toolbox);
            secondHand = new SecondHand(toolbox);
            leadersPennon = new LeadersPennon(toolbox);
            drownedVisage = new DrownedVisage(toolbox);
            longshotGeode = new LongshotGeode(toolbox);
            hasteningGreave = new HasteningGreave(toolbox);
            cauterizingGreave = new CauterizingGreave(toolbox);
            noxiousSlime = new NoxiousSlime(toolbox);
            appraisersEye = new AppraisersEye(toolbox);
            collectorsVision = new CollectorsVision(toolbox, inspiration);
            tJetpack = new TJetpack(toolbox);
            matterAccelerator = new MatterAccelerator(toolbox);
            radiantTimepiece = new RadiantTimepiece(toolbox);
            debugItem = new DebugItem(toolbox);

            // Create WIP content
            WIP.Init();

            // Create interactables
            recollectionShrine = new RecollectionShrine();

            // These survivors are WIP
            if (Utils.WIPContentEnabled)
            {
                technician = new Technician();
            }

            // Log successful load information
            Log.Info("Faithful loaded successfully, thank you for playing our mod!");
        }

        private void Start()
        {
            // Update toolbox
            toolbox.Start();
        }

        private void Update()
        {
            // Update toolbox
            toolbox.Update();
        }

        private void FixedUpdate()
        {
            // Update toolbox
            toolbox.FixedUpdate();
        }
    }
}
