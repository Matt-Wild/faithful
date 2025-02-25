using R2API;
using RoR2;
using RoR2.Hologram;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using RoR2.ExpansionManagement;
using RoR2.UI;

namespace Faithful
{
    // Used for determining and fetching the ping icon
    internal enum PingIconType
    {
        Drone,
        Inventory,
        Loot,
        Shrine,
        Teleporter,
        Mystery,
        Custom
    }

    // Used for determining and fetching the inspect icon
    internal enum InspectIconType
    {
        Chest,
        Drone,
        Lunar,
        Pillar,
        Printer,
        RadioScanner,
        Scrapper,
        Shrine,
        Meridian,
        Void,
        Mystery,
        Custom
    }

    // Used for determining cost type or custom cost type
    internal enum InteractableCostType
    {
        None,
        Money,
        PercentHealth,
        LunarCoin,
        WhiteItem,
        GreenItem,
        RedItem,
        Equipment,
        VolatileBattery,
        LunarItemOrEquipment,
        BossItem,
        ArtifactShellKillerItem,
        TreasureCacheItem,
        TreasureCacheVoidItem,
        VoidCoin,
        SoulCost,
        Count,
        Custom
    }

    // Used for determining what expansion the interactable should be dependent on
    internal enum InteractableRequiredExpansion
    {
        None,
        SurvivorsOfTheVoid,
        SeekersOfTheStorm
    }

    internal class Interactable
    {
        // Token of the interactable used for finding and identifying it
        private string m_token;

        // Name of the model used to find the interatable in the asset bund
        private string m_modelName;

        // The type of ping icon this interactable has
        private PingIconType m_pingIconType;

        // The type of inspection icon this interactable has
        private InspectIconType m_inspectIconType;

        // The asset name for custom ping icon
        private string m_customPingIconAssetName;

        // The asset name for custom inspect icon
        private string m_customInspectIconAssetName;

        // The asset name and colour for the interactable symbol
        private string m_symbolAssetName;
        private Color m_symbolAssetColour;

        // Purchase interaction customisation
        private CostTypeIndex m_costType;
        private int m_cost;
        private bool m_startAvailable;
        private bool m_setUnavailableOnTeleporterActivated;
        private bool m_isShrine;

        // Custom cost type definition customisation
        private string m_customCostString;
        private ColorCatalog.ColorIndex m_customCostColour;
        private bool m_saturateWorldStyledCustomCost;
        private bool m_darkenWorldStyledCustomCost;

        // Whether the cost hologram is able to rotate
        private bool m_disableHologramRotation;

        // The model of the interactable from the asset bundle
        private GameObject m_model;

        // The prefab that's spawned in the game world for this interactable
        private GameObject m_prefab;

        // The definition for this interactables custom cost type
        private CostTypeDef m_customCostType;

        // The expansion that this interactable depends on
        private InteractableRequiredExpansion m_requiredExpansion;

        // Inspection related references
        private bool m_allowInspect;
        private InspectDef m_inspectDef;

        // Dictionary of stages in which this interactables of set spawns (as well as spawn info such as position and rotation)
        private Dictionary<string, List<SetSpawnInfo>> m_setSpawns = new Dictionary<string, List<SetSpawnInfo>>();

        // Default settings for interactable
        private Setting<bool> m_disableSetting;

        public void Init(string _token, string _modelName, PingIconType _pingIconType, string _customPingIconAssetName = "", string _symbolName = "", Color? _symbolColour = null,
                         InteractableCostType _costType = InteractableCostType.Money, int _cost = 1, bool _startAvailable = true, bool _setUnavailableOnTeleporterActivated = false, bool _isShrine = true,
                         bool _disableHologramRotation = true, string _customCostString = null, ColorCatalog.ColorIndex _customCostColour = ColorCatalog.ColorIndex.None, bool _saturateWorldStyledCustomCost = false,
                         bool _darkenWorldStyledCustomCost = false, InteractableRequiredExpansion _requiredExpansion = InteractableRequiredExpansion.None, bool _allowInspect = true,
                         InspectIconType _inspectIconType = InspectIconType.Mystery, string _customInspectIconAssetName = "")
        {
            // Assign token
            m_token = _token;

            // Assign model name
            m_modelName = _modelName;

            // Assign ping icon type
            m_pingIconType = _pingIconType;

            // Assign inspect icon type
            m_inspectIconType = _inspectIconType;

            // Assign custom ping icon asset name
            m_customPingIconAssetName = _customPingIconAssetName;

            // Assign custom inspect icon asset name
            m_customInspectIconAssetName = _customInspectIconAssetName;

            // Assign symbol asset name and colour
            m_symbolAssetName = _symbolName;
            m_symbolAssetColour = _symbolColour ?? Color.white;

            // Assign cost type
            AssignCostType(_costType);

            // Assign purchase interaction customisation
            m_cost = _cost;
            m_startAvailable = _startAvailable;
            m_setUnavailableOnTeleporterActivated = _setUnavailableOnTeleporterActivated;
            m_isShrine = _isShrine;

            // Assign custom cost type definition customisation
            m_customCostString = _customCostString;
            m_customCostColour = _customCostColour;
            m_saturateWorldStyledCustomCost = _saturateWorldStyledCustomCost;
            m_darkenWorldStyledCustomCost = _darkenWorldStyledCustomCost;

            // Assign whether the hologram of this interactable can rotate
            m_disableHologramRotation = _disableHologramRotation;

            // Assign required expansion
            m_requiredExpansion = _requiredExpansion;

            // Assign if this interactable allows you to inspect it
            m_allowInspect = _allowInspect;

            // Create default settings that every interactable has
            CreateDefaultSettings();

            // Register with interactables
            Interactables.RegisterInteractable(this);

            // Don't immediately prewarm interactables with custom cost types
            if (_costType != InteractableCostType.Custom)
            {
                // Prewarm interactable
                Prewarm();
            }
        }

        private void AssignCostType(InteractableCostType _costType)
        {
            // Check if custom
            if (_costType == InteractableCostType.Custom)
            {
                // Add custom cost type to cost type catalog
                CostTypeCatalog.modHelper.getAdditionalEntries += AddCustomCostType;
            }

            // Standard cost type
            else
            {
                // Assign cost type
                m_costType = (CostTypeIndex)_costType;
            }
        }

        private void AddCustomCostType(List<CostTypeDef> _list)
        {
            // Create custom cost type definition
            m_customCostType = new CostTypeDef();
            m_customCostType.costStringFormatToken = $"FAITHFUL_COST_{token}_FORMAT";
            m_customCostType.isAffordable = new CostTypeDef.IsAffordableDelegate(CustomIsAffordable);
            m_customCostType.payCost = new CostTypeDef.PayCostDelegate(CustomPayCost);
            m_customCostType.colorIndex = m_customCostColour;
            m_customCostType.saturateWorldStyledCostString = m_saturateWorldStyledCustomCost;
            m_customCostType.darkenWorldStyledCostString = m_darkenWorldStyledCustomCost;
            m_costType = (CostTypeIndex)(CostTypeCatalog.costTypeDefs.Length + _list.Count);

            // Add custom cost formatting to language API
            LanguageAPI.Add($"FAITHFUL_COST_{token}_FORMAT", m_customCostString == null ? "?" : m_customCostString);

            // Add custom cost type definition to the list of additional cost type definitions
            _list.Add(m_customCostType);

            // Prewarm interactable
            Prewarm();
        }

        public void Prewarm()
        {
            // Check for prefab
            if (prefab == null)
            {
                // Warn
                Log.Warning($"[Interactable] | Interactable '{name}' was unable to construct it's prefab.");
            }
        }

        private void CreatePrefab()
        {
            // Check if interactable already has prefab
            if (m_prefab != null)
            {
                // Warn and return
                Log.Warning($"[Interactable] | Attempted to create prefab for interactable '{name}' but the interactable already has a prefab.");
                return;
            }

            // Check for model
            if (model == null)
            {
                // Error and return
                Log.Error($"[Interactable] | Interactable '{name}' could not create it's prefab because it's missing a model!");
                return;
            }

            // Create prefab from clone of model
            m_prefab = model.InstantiateClone(name);

            // Check if requires an expansion
            if (m_requiredExpansion != InteractableRequiredExpansion.None)
            {
                // Add required expansion component
                ExpansionRequirementComponent expansionRequirementComponent = m_prefab.AddComponent<ExpansionRequirementComponent>();

                // Set required expansion
                expansionRequirementComponent.requiredExpansion = m_requiredExpansion == InteractableRequiredExpansion.SurvivorsOfTheVoid ? Assets.sotvDef : Assets.sotsDef;
            }

            // Add interactable behaviour
            FaithfulInteractableBehaviour interactableBehaviour = m_prefab.AddComponent<FaithfulInteractableBehaviour>();
            interactableBehaviour.token = token;
            interactableBehaviour.startAvailable = m_startAvailable;
            interactableBehaviour.costType = m_costType;

            // Add purchase interaction
            PurchaseInteraction purchaseInteraction = m_prefab.AddComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = nameToken;
            purchaseInteraction.contextToken = contextToken;
            purchaseInteraction.costType = m_costType;
            purchaseInteraction.cost = m_cost;
            purchaseInteraction.available = m_startAvailable;
            purchaseInteraction.setUnavailableOnTeleporterActivated = m_setUnavailableOnTeleporterActivated;
            purchaseInteraction.isShrine = m_isShrine;
            purchaseInteraction.isGoldShrine = false;

            // Add purchase interaction to interactable behaviour
            interactableBehaviour.purchaseInteraction = purchaseInteraction;

            // Add ping info provider
            PingInfoProvider pingInfoProvider = m_prefab.AddComponent<PingInfoProvider>();

            // Check ping icon type and assign ping icon override
            switch (m_pingIconType)
            {
                case PingIconType.Drone:
                    pingInfoProvider.pingIconOverride = Assets.dronePingIcon;
                    break;
                case PingIconType.Inventory:
                    pingInfoProvider.pingIconOverride = Assets.inventoryPingIcon;
                    break;
                case PingIconType.Loot:
                    pingInfoProvider.pingIconOverride = Assets.lootPingIcon;
                    break;
                case PingIconType.Shrine:
                    pingInfoProvider.pingIconOverride = Assets.shrinePingIcon;
                    break;
                case PingIconType.Teleporter:
                    pingInfoProvider.pingIconOverride = Assets.teleporterPingIcon;
                    break;
                case PingIconType.Mystery:
                    pingInfoProvider.pingIconOverride = Assets.mysteryPingIcon;
                    break;
                case PingIconType.Custom:
                    pingInfoProvider.pingIconOverride = Assets.GetIcon(m_customPingIconAssetName);
                    break;
            }

            // Add generic display name provider
            GenericDisplayNameProvider genericDisplayNameProvider = m_prefab.AddComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = nameToken;

            // Check for symbol asset
            if (!string.IsNullOrWhiteSpace(m_symbolAssetName))
            {
                // Attempt to find symbol child transform
                Transform symbolTransform = Utils.FindChildByName(m_prefab.transform, "Symbol")?.transform;
                if (symbolTransform != null)
                {
                    // Add billboard
                    symbolTransform.gameObject.AddComponent<Billboard>();

                    // Get symbol renderer
                    Renderer renderer = symbolTransform.GetComponent<Renderer>();

                    // Modify symbol material
                    renderer.sharedMaterial = Assets.GetShrineSymbolMaterial(Assets.GetTexture(m_symbolAssetName), m_symbolAssetColour);

                    // Add symbol transform reference to interactable behaviour
                    interactableBehaviour.symbolTransform = symbolTransform;
                }
                else
                {
                    // No symbol transform - warn
                    Log.Warning($"[INTERACTABLE] | No symbol transform found on interactable '{name}'.");
                }
            }

            // Cycle through colliders in prefab
            foreach (Collider collider in m_prefab.GetComponentsInChildren<Collider>())
            {
                // Add entity locator
                EntityLocator entityLocator = collider.gameObject.AddComponent<EntityLocator>();
                entityLocator.entity = m_prefab;
            }

            // Add model locator
            ModelLocator modelLocator = m_prefab.AddComponent<ModelLocator>();
            modelLocator.modelTransform = m_prefab.transform;
            modelLocator.dontDetatchFromParent = true;
            modelLocator.autoUpdateModelTransform = true;

            // Attempt to find interactable model base
            Transform modelBase = Utils.FindChildByName(modelLocator.modelTransform, "Base")?.transform;
            if (modelBase != null)
            {
                // Apply model base
                modelLocator.modelBaseTransform = modelBase;
            }
            else
            {
                // No model base - warn
                Log.Warning($"[INTERACTABLE] | No model base found on interactable '{name}'.");
            }

            // Add highlight
            Highlight highlight = m_prefab.GetComponent<Highlight>();
            highlight.strength = 1;
            highlight.highlightColor = Highlight.HighlightColor.interactive;

            // Attempt to find highlight renderer
            Renderer[] validRenderers = m_prefab.GetComponentsInChildren<Renderer>().Where(x => x.gameObject.name.Contains("[HIGHLIGHT]")).ToArray();
            if (validRenderers.Length > 0)
            {
                // Apply target renderer
                highlight.targetRenderer = validRenderers[0];
            }
            else
            {
                // No highlight - warn
                Log.Warning($"[INTERACTABLE] | No highlight renderer found on interactable '{name}'.");
            }

            // Add hologram projector
            HologramProjector hologramProjector = m_prefab.AddComponent<HologramProjector>();
            hologramProjector.displayDistance = (m_costType == CostTypeIndex.None || (m_customCostType != null && m_customCostString == null)) ? 0 : 10;
            hologramProjector.disableHologramRotation = m_disableHologramRotation;

            // Attempt to find hologram pivot
            Transform hologramPivot = Utils.FindChildByName(m_prefab.transform, "HologramPivot")?.transform;
            if (hologramPivot != null)
            {
                // Apply hologram pivot
                hologramProjector.hologramPivot = hologramPivot;
            }
            else
            {
                // No hologram pivot - warn
                Log.Warning($"[INTERACTABLE] | No hologram pivot found on interactable '{name}'.");
            }

            // Add child locator
            ChildLocator childLocator = m_prefab.AddComponent<ChildLocator>();

            // Attempt to find fireworks emitter
            Transform fireworkEmitter = Utils.FindChildByName(m_prefab.transform, "FireworkEmitter")?.transform;
            if (fireworkEmitter != null)
            {
                // Add to child locator
                childLocator.transformPairs =
                [
                    new ChildLocator.NameTransformPair()
                    {
                        name = "FireworkOrigin",
                        transform = fireworkEmitter
                    }
                ];
            }
            else
            {
                // No fireworks emitter - warn
                Log.Warning($"[INTERACTABLE] | No firework emitter found on interactable '{name}'.");
            }

            // Check if this interactable allows the player to inspect it
            if (m_allowInspect)
            {
                // Get icon for inspect info
                Sprite inspectIcon = Assets.mysteryInspectIcon;
                switch (m_inspectIconType)
                {
                    case InspectIconType.Chest:
                        inspectIcon = Assets.chestInspectIcon;
                        break;
                    case InspectIconType.Drone:
                        inspectIcon = Assets.droneInspectIcon;
                        break;
                    case InspectIconType.Lunar:
                        inspectIcon = Assets.lunarInspectIcon;
                        break;
                    case InspectIconType.Pillar:
                        inspectIcon = Assets.pillarInspectIcon;
                        break;
                    case InspectIconType.Printer:
                        inspectIcon = Assets.printerInspectIcon;
                        break;
                    case InspectIconType.RadioScanner:
                        inspectIcon = Assets.radioScannerInspectIcon;
                        break;
                    case InspectIconType.Scrapper:
                        inspectIcon = Assets.scapperInspectIcon;
                        break;
                    case InspectIconType.Shrine:
                        inspectIcon = Assets.shrineInspectIcon;
                        break;
                    case InspectIconType.Meridian:
                        inspectIcon = Assets.meridianInspectIcon;
                        break;
                    case InspectIconType.Void:
                        inspectIcon = Assets.voidInspectIcon;
                        break;
                    case InspectIconType.Mystery:
                        inspectIcon = Assets.mysteryPingIcon;
                        break;
                    case InspectIconType.Custom:
                        inspectIcon = Assets.GetIcon(m_customInspectIconAssetName);
                        break;
                }

                // Create inspect info
                InspectInfo inspectInfo = new InspectInfo
                {
                    Visual = inspectIcon,
                    TitleToken = $"FAITHFUL_INTERACTABLE_{token}_NAME",
                    DescriptionToken = $"FAITHFUL_INTERACTABLE_{token}_DESCRIPTION"
                };

                // Create inspection definition
                m_inspectDef = new InspectDef
                {
                    name = $"{token}_INSPECT_DEF",
                    hideFlags = HideFlags.None,
                    Info = inspectInfo
                };

                // Add generic inspect info provider
                GenericInspectInfoProvider genericInspectInfoProvider = m_prefab.AddComponent<GenericInspectInfoProvider>();
                genericInspectInfoProvider.InspectInfo = m_inspectDef;
            }
        }

        private void CreateDefaultSettings()
        {
            // Create the settings which every interactable should have
            m_disableSetting = CreateSetting("DISABLE", "Disable Interactable?", false, "Should this interactable be disabled and avoid spawning during runs?");
        }

        public Setting<T> CreateSetting<T>(string _tokenAddition, string _key, T _defaultValue, string _description, bool _restartRequired = false, string _valueFormatting = "{0:0}")
        {
            // Return new setting
            return Config.CreateSetting($"INTERACTABLE_{token}_{_tokenAddition}", $"Interactable: {name.Replace("'", "")}", _key, _defaultValue, _description, _restartRequired: _restartRequired, _valueFormatting: _valueFormatting);
        }

        public void SendInteractionMessage(CharacterBody _interactor, string[] _paramTokens = null)
        {
            // Check for message params
            if (_paramTokens != null)
            {
                // Send broadcast message
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = _interactor,
                    baseToken = $"FAITHFUL_INTERACTABLE_{token}_MESSAGE",
                    paramTokens = _paramTokens
                });
            }

            // No params
            else
            {
                // Send broadcast message
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = _interactor,
                    baseToken = $"FAITHFUL_INTERACTABLE_{token}_MESSAGE"
                });
            }
        }

        public void DoSetSpawn()
        {
            // Ignore if not enabled
            if (!enabled) return;

            // Get current stage name
            string stageName = SceneManager.GetActiveScene().name;

            // Check if scene has set spawns
            if (m_setSpawns.ContainsKey(stageName))
            {
                // Get list of possible set spawns
                List<SetSpawnInfo> possibleSpawns = m_setSpawns[stageName];

                // Get random set spawn info
                SetSpawnInfo spawnInfo = possibleSpawns[Random.Range(0, possibleSpawns.Count)];

                // Spawn interactable at position and rotation
                GameObject interactableInstance = Object.Instantiate(prefab, spawnInfo.position, spawnInfo.rotation);
                NetworkServer.Spawn(interactableInstance);
            }
        }

        public void AddSetSpawn(string _stageName, Vector3 _position, Vector3 _rotation)
        {
            // Check for set spawns for stage
            if (!m_setSpawns.ContainsKey(_stageName))
            {
                // Create list for this stage
                m_setSpawns[_stageName] = new List<SetSpawnInfo>();
            }

            // Add to set spawns for stage
            m_setSpawns[_stageName].Add(new SetSpawnInfo(_position, _rotation));
        }

        private void FetchModel()
        {
            // Check for model name
            if (string.IsNullOrWhiteSpace(modelName))
            {
                // Error and return
                Log.Error($"[Interactable] | Interactable '{name}' could not find it's model as it was not provided a valid model name!");
                return;
            }

            // Fetch model from asset bundle
            m_model = Assets.GetModel(modelName);

            // Add network identity
            m_model.AddComponent<NetworkIdentity>();

            // Check for model
            if (m_model == null)
            {
                // Error and return
                Log.Error($"[Interactable] | Interactable '{name}' could not find it's model '{modelName}'!");
                return;
            }
        }

        public virtual void OnPurchase(FaithfulInteractableBehaviour _behaviour, Interactor _interactor)
        {
            // Warn that interactable doesn't have on purchase behaviour
            Log.Warning($"[Interactable] | Interactable '{name}' does not have any purchase behaviour.");
        }

        public virtual bool CustomIsAffordable(CostTypeDef _costTypeDef, CostTypeDef.IsAffordableContext _context)
        {
            // Warn that interactable doesn't have custom is affordable behaviour
            Log.Warning($"[Interactable] | Interactable '{name}' has a custom cost type but does not have any custom is affordable behaviour.");

            // Default to true
            return true;
        }

        public virtual void CustomPayCost(CostTypeDef _costTypeDef, CostTypeDef.PayCostContext _context)
        {
            // Warn that interactable doesn't have custom pay cost behaviour
            Log.Warning($"[Interactable] | Interactable '{name}' has a custom cost type but does not have any custom pay cost behaviour.");
        }

        // Accessors
        public string token { get { return m_token; } }
        public string modelName { get { return m_modelName; } }
        public string nameToken { get { return $"FAITHFUL_INTERACTABLE_{token}_NAME"; } }
        public string name { get { return Utils.GetLanguageString(nameToken); } }
        public string contextToken { get { return $"FAITHFUL_INTERACTABLE_{token}_CONTEXT"; } }
        public bool enabled
        {
            get
            {
                // Check if enabled in config
                if (m_disableSetting.Value) return false;

                // Check for required expansion
                if (m_requiredExpansion == InteractableRequiredExpansion.None) return true;

                // Return if required expansion is enabled
                return m_requiredExpansion == InteractableRequiredExpansion.SurvivorsOfTheVoid ? Run.instance.IsExpansionEnabled(Assets.sotvDef) : Run.instance.IsExpansionEnabled(Assets.sotsDef);
            }
        }
        public GameObject model
        {
            get
            {
                // Check for model
                if (m_model == null)
                {
                    // Attempt to fetch model from asset bundle
                    FetchModel();
                }

                // Return model
                return m_model;
            }
        }
        public GameObject prefab
        {
            get
            {
                // Check for prefab
                if (m_prefab == null)
                {
                    // Attempt to create prefab
                    CreatePrefab();
                }

                // Return prefab
                return m_prefab;
            }
        }
        public Dictionary<string, List<SetSpawnInfo>> setSpawns { get { return m_setSpawns; } }
    }

    internal class FaithfulInteractableBehaviour : NetworkBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public Transform symbolTransform;

        // The token used to identify the behaviour of this interactable
        public string token;

        // Whether the interactable starts as available
        public bool startAvailable;

        // The cost type of the interactable
        public CostTypeIndex costType;

        // Callback for purchase interaction
        public event FaithfulInteractorCallback onPurchase;

        private void Start()
        {
            // Check if hosting and run is valid
            if (Utils.hosting && Run.instance)
            {
                // Check if should start as available
                if (startAvailable)
                {
                    // Set as available
                    SetAvailable();
                }
                
            }

            // Update cost type
            purchaseInteraction.costType = costType;

            // Add listener for when interactable is purchased
            purchaseInteraction.onPurchase.AddListener(InteractablePurchaseAttempt);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Get interactable with token
            Interactable interactable = Interactables.FetchInteractable(token);

            // Subscribe interactable's on purchase method to the on purchase event
            onPurchase += interactable.OnPurchase;
        }

        private void InteractablePurchaseAttempt(Interactor _interactor)
        {
            // Return if no valid interactor
            if (!_interactor) return;

            // Return if not hosting
            if (!Utils.hosting) return;

            // Call on purchase callback
            onPurchase.Invoke(this, _interactor);
        }

        public void DoShrineUseEffect()
        {
            // Use effect manager to play the shrine use effect
            EffectManager.SimpleImpactEffect(Assets.shrineUseEffectPrefab, transform.position, new Vector3(0, 0, 0), true);
        }

        public void SetAvailable()
        {
            // Set as available
            purchaseInteraction.SetAvailableTrue();

            // Make symbol active
            symbolTransform.gameObject.SetActive(true);
        }

        public void SetUnavailable()
        {
            // Set as unavailable
            purchaseInteraction.SetAvailable(false);

            // Make symbol inactive
            symbolTransform.gameObject.SetActive(false);
        }
    }

    internal struct SetSpawnInfo
    {
        // Position and rotation for spawn
        private Vector3 m_position;
        private Quaternion m_rotation;

        public SetSpawnInfo(Vector3 _position, Vector3 _rotation)
        {
            // Set position and rotation
            m_position = _position;
            m_rotation = Quaternion.Euler(_rotation);
        }

        // Accessors
        public Vector3 position { get { return m_position; } }
        public Quaternion rotation { get { return m_rotation; } }
    }
}
