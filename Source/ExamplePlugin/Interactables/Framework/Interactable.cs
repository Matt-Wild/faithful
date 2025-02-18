using R2API;
using RoR2;
using RoR2.Hologram;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// TODO:
// - EXPANSION REQUIREMENTS
// - PURCHASE INTERACTION CUSTOMISATION

namespace Faithful
{
    internal class Interactable
    {
        // Token of the interactable used for finding and identifying it
        private string m_token;

        // Name of the model used to find the interatable in the asset bund
        private string m_modelName;

        // The model of the interactable from the asset bundle
        private GameObject m_model;

        // The prefab that's spawned in the game world for this interactable
        private GameObject m_prefab;

        // Dictionary of stages in which this interactables of set spawns (as well as spawn info such as position and rotation)
        private Dictionary<string, List<SetSpawnInfo>> m_setSpawns = new Dictionary<string, List<SetSpawnInfo>>();

        public void Init(string _token, string _modelName)
        {
            // Assign token
            m_token = _token;

            // Assign model name
            m_modelName = _modelName;

            // Prewarm interactable
            Prewarm();

            // Register with interactables
            Interactables.RegisterInteractable(this);
        }

        private void Prewarm()
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

            // Add purchase interaction
            PurchaseInteraction purchaseInteraction = m_prefab.AddComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = nameToken;
            purchaseInteraction.contextToken = contextToken;
            purchaseInteraction.costType = CostTypeIndex.Money;
            purchaseInteraction.cost = 1;
            purchaseInteraction.available = true;
            purchaseInteraction.setUnavailableOnTeleporterActivated = true;
            purchaseInteraction.isShrine = true;
            purchaseInteraction.isGoldShrine = false;

            // Add ping info provider
            PingInfoProvider pingInfoProvider = m_prefab.AddComponent<PingInfoProvider>();
            pingInfoProvider.pingIconOverride = Assets.GetIcon("TEMP");

            // Add generic display name provider
            GenericDisplayNameProvider genericDisplayNameProvider = m_prefab.AddComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = nameToken;

            // Attempt to find symbol child transform
            Transform symbolTransform = Utils.FindChildByName(m_prefab.transform, "Symbol")?.transform;
            if (symbolTransform != null)
            {
                // Add billboard
                symbolTransform.gameObject.AddComponent<Billboard>();
            }
            else
            {
                // No symbol transform - warn
                Log.Warning($"[INTERACTABLE] | No symbol transform found on interactable '{name}'.");
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
            hologramProjector.displayDistance = 10;
            hologramProjector.disableHologramRotation = true;

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
        }

        public void DoSetSpawn()
        {
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

        // Accessors
        public string token { get { return m_token; } }
        public string modelName { get { return m_modelName; } }
        public string nameToken { get { return $"FAITHFUL_INTERACTABLE_{token}_NAME"; } }
        public string name { get { return Utils.GetLanguageString(nameToken); } }
        public string contextToken { get { return $"FAITHFUL_INTERACTABLE_{token}_CONTEXT"; } }
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

    internal struct SetSpawnInfo
    {
        // Position and rotation for spawn
        private Vector3 m_position;
        private Quaternion m_rotation;

        public SetSpawnInfo(Vector3 _position, Vector3 _rotation)
        {
            // Set position and rotation
            m_position = _position;
            m_rotation = Quaternion.LookRotation(_rotation);
        }

        // Accessors
        public Vector3 position { get { return m_position; } }
        public Quaternion rotation { get { return m_rotation; } }
    }
}
