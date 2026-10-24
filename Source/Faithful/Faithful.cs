using BepInEx;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using HarmonyLib;

namespace Faithful
{
    // Required R2API modules
    [BepInDependency(ItemAPI.PluginGUID)]

    // Language token overlays
    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]

    // For Quality support
    [BepInDependency("com.Gorakh.ItemQualities", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class Faithful : BaseUnityPlugin
    {
        // Stable plugin identity used by BepInEx and configuration
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SpilledSoup";
        public const string PluginName = "Faithful";
        public const string PluginVersion = "1.5.11";

        // Plugin info
        public static PluginInfo PInfo { get; private set; }

        // Hook failures can be tolerated; callbacks still require complete core initialization
        internal static bool InitializationSucceeded { get; private set; }

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

        // The Awake() method is run at the very start when the game is initialized
        public void Awake()
        {
            // Update plugin info
            PInfo = Info;

            // Pass GUID and mod name to Risk of Options wrapper
            RiskOfOptionsWrapper.pluginGUID = PluginGUID;
            RiskOfOptionsWrapper.pluginName = PluginName;

            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            try
            {
                Initialize();
                InitializationSucceeded = true;
                Utils.EnableMenuHooks();

                string[] failedHooks = HookRegistration.GetFailedHookNames();
                if (failedHooks.Length == 0)
                {
                    Log.Info("Faithful loaded successfully, thank you for playing our mod!");
                }
                else
                {
                    Log.Warning($"Faithful loaded with {failedHooks.Length} failed hook(s): {string.Join(", ", failedHooks)}. Features using these hooks may not work.");
                }
            }
            catch (System.Exception exception)
            {
                InitializationSucceeded = false;
                enabled = false;
                Log.Error($"Faithful core initialization failed.\n{exception}");
                Utils.DisableMenuHooks();
                Behaviour.Disable();
            }
        }

        private void Initialize()
        {
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

            // Attempt to init Quality support
            QualityCompatBootstrap.TryInit();
        }

        private void OnApplicationQuit()
        {
            ItemDisplays.Shutdown();
        }

        private void Start()
        {
            if (!InitializationSucceeded) return;

            // Update toolbox
            toolbox.Start();
        }

        private void Update()
        {
            if (!InitializationSucceeded) return;

            // Update toolbox
            toolbox.Update();
        }

        private void FixedUpdate()
        {
            if (!InitializationSucceeded) return;

            // Update toolbox
            toolbox.FixedUpdate();
        }

        private void LateUpdate()
        {
            if (!InitializationSucceeded) return;

            // Record runtime profiling data for frame (debug only)
            if (Utils.debugMode)
            {
                FaithfulRuntimeProfiler.Record();
                toolbox.debugMode.RefreshProfiler();
            }
        }
    }
}
