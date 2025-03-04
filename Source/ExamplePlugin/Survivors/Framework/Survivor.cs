using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal class Survivor
    {
        // Token used to identify survivor and find language strings
        private string m_token;

        // Name of the survivor model prefab
        private string m_modelName;

        // Name of the portrait asset for this survivor
        private string m_portraitName;

        // Survivor model prefab
        private GameObject m_modelPrefab;

        // Crosshair of survivor
        private GameObject m_crosshair;

        // Survivor portrait
        private Texture m_portrait;

        // Body colour of survivor
        private Color m_bodyColour;

        // Sort position of survivor
        private int m_sortPosition;

        // Key used to find the crosshair asset
        private string m_crosshairName;

        // Survivor stats
        private float m_maxHealth;
        private float m_healthRegen;
        private float m_armour;
        private float m_shield;
        private int m_jumpCount;
        private float m_damage;
        private float m_attackSpeed;
        private float m_crit;
        private float m_moveSpeed;
        private float m_acceleration;
        private float m_jumpPower;

        // If this survivor has it's level stats automatically calculated
        private bool m_autoCalculateLevelStats;

        // Camera settings
        private Vector3 m_aimOriginPosition;
        private Vector3 m_modelBasePosition;
        private Vector3 m_cameraPivotPosition;
        private float m_cameraVerticalOffset;
        private float m_cameraDepth;

        // RoR2 Camera Params class
        private CharacterCameraParams m_cameraParams;

        // A cloned character body from the main game with it's bits and bobs replaced with custom stuff
        private GameObject m_bodyPrefab;

        // The character body for the cloned and adjusted prefab
        private CharacterBody m_characterBody;

        // The character model component for the body prefab
        private CharacterModel m_characterModel;

        public void Init(string _token, string _modelName, string _portraitName, Color? _bodyColor = null, int _sortPosition = 100, string _crosshairName = "Standard", float _maxHealth = 110.0f,
                         float _healthRegen = 1.0f, float _armour = 0.0f, float _shield = 0.0f, int _jumpCount = 1, float _damage = 12.0f, float _attackSpeed = 1.0f, float _crit = 1.0f,
                         float _moveSpeed = 7.0f, float _acceleration = 80.0f, float _jumpPower = 15.0f, bool _autoCalculateLevelStats = true, Vector3? _aimOriginPosition = null,
                         Vector3? _modelBasePosition = null, Vector3? _cameraPivotPosition = null, float _cameraVerticalOffset = 1.37f, float _cameraDepth = -10.0f)
        {
            // Assign token
            m_token = _token;

            // Assign model name
            m_modelName = _modelName;

            // Assign portrait name
            m_portraitName = _portraitName;

            // Assign body colour
            m_bodyColour = _bodyColor ?? Color.white;

            // Assign sort position
            m_sortPosition = _sortPosition;

            // Assign crosshair name
            m_crosshairName = _crosshairName;

            // Assign survivor stats
            m_maxHealth = _maxHealth;
            m_healthRegen = _healthRegen;
            m_armour = _armour;
            m_shield = _shield;
            m_jumpCount = _jumpCount;
            m_damage = _damage;
            m_attackSpeed = _attackSpeed;
            m_crit = _crit;
            m_moveSpeed = _moveSpeed;
            m_acceleration = _acceleration;
            m_jumpPower = _jumpPower;

            // Assign if this character has it's level stats automatically calculated
            m_autoCalculateLevelStats = _autoCalculateLevelStats;

            // Assign camera settings
            m_aimOriginPosition = _aimOriginPosition ?? new Vector3(0f, 1.6f, 0f);
            m_modelBasePosition = _modelBasePosition ?? new Vector3(0f, -0.92f, 0f);
            m_cameraPivotPosition = _cameraPivotPosition ?? new Vector3(0f, 0.8f, 0f);
            m_cameraVerticalOffset = _cameraVerticalOffset;
            m_cameraDepth = _cameraDepth;
        }

        private void CreateBodyPrefab()
        {
            // Check if body prefab already exists
            if (m_bodyPrefab != null)
            {
                // Already done
                return;
            }

            // Check for model
            if (modelPrefab == null)
            {
                // Error and return - unsuccessful
                Log.Error($"Could not create '{name}' survivor! Model: {modelPrefab}");
                return;
            }

            // Create clone body to built off of
            CreateCloneBody();

            // Setup the clone body
            SetupCloneBody();

            // Setup character model
            SetupCharacterModel();
        }

        private void CreateCloneBody()
        {
            // Clone Commando (safest character)
            GameObject clonedBody = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommmandoBody");

            // Check for valid clone
            if (clonedBody == null)
            {
                // Error and return null - unsuccessful
                Log.Error("Was unable to clone Commando character body to create an empty clone!");
            }

            // Use Prefab API to clone the Commando body and treat as prefab
            GameObject newBodyPrefab = PrefabAPI.InstantiateClone(clonedBody, bodyName);

            // Cycle through children of character body
            for (int i = newBodyPrefab.transform.childCount - 1; i >= 0; i--)
            {
                // Delete child
                UnityEngine.Object.DestroyImmediate(newBodyPrefab.transform.GetChild(i).gameObject);
            }

            // Assign clones body as this survivor's body prefab
            m_bodyPrefab = newBodyPrefab;
        }

        private void SetupCloneBody()
        {
            // Transfer new information to character body
            ConfigureCharacterBody();

            // Load the model into the cloned character body and set it up
            IntegrateCharacterModel();

            // Setup camera for cloned character body
            SetupCamera();

            // Setup collider for this character
            SetupCollider();
        }

        private void ConfigureCharacterBody()
        {
            // Set character body identity
            m_characterBody.baseNameToken = nameToken;
            m_characterBody.subtitleNameToken = subtitleToken;
            m_characterBody.portraitIcon = portrait;
            m_characterBody.bodyColor = bodyColour;

            // Set crosshair and drop pod prefab
            m_characterBody._defaultCrosshairPrefab = crosshair;
            m_characterBody.hideCrosshair = false;
            m_characterBody.preferredPodPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");

            // Set survivor stats
            m_characterBody.baseMaxHealth = maxHealth;
            m_characterBody.baseRegen = healthRegen;
            m_characterBody.baseArmor = armour;
            m_characterBody.baseMaxShield = shield;

            m_characterBody.baseDamage = damage;
            m_characterBody.baseAttackSpeed = attackSpeed;
            m_characterBody.baseCrit = crit;

            m_characterBody.baseMoveSpeed = moveSpeed;
            m_characterBody.baseJumpPower = jumpPower;
            m_characterBody.baseAcceleration = acceleration;
            m_characterBody.baseJumpCount = jumpCount;
            m_characterBody.sprintingSpeedMultiplier = 1.45f;

            // Setup levelling
            m_characterBody.autoCalculateLevelStats = autoCalculateLevelStats;

            if (m_characterBody.autoCalculateLevelStats)
            {
                m_characterBody.levelMaxHealth = Mathf.Round(m_characterBody.baseMaxHealth * 0.3f);
                m_characterBody.levelMaxShield = Mathf.Round(m_characterBody.baseMaxShield * 0.3f);
                m_characterBody.levelRegen = m_characterBody.baseRegen * 0.2f;

                m_characterBody.levelMoveSpeed = 0f;
                m_characterBody.levelJumpPower = 0f;

                m_characterBody.levelDamage = m_characterBody.baseDamage * 0.2f;
                m_characterBody.levelAttackSpeed = 0f;
                m_characterBody.levelCrit = 0f;

                m_characterBody.levelArmor = 0f;
            }
            else
            {
                m_characterBody.levelMaxHealth = healthGrowth;
                m_characterBody.levelMaxShield = shieldGrowth;
                m_characterBody.levelRegen = regenGrowth;

                m_characterBody.levelMoveSpeed = moveSpeedGrowth;
                m_characterBody.levelJumpPower = jumpPowerGrowth;

                m_characterBody.levelDamage = damageGrowth;
                m_characterBody.levelAttackSpeed = attackSpeedGrowth;
                m_characterBody.levelCrit = critGrowth;

                m_characterBody.levelArmor = armourGrowth;
            }

            // Misc character body settings
            m_characterBody.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            m_characterBody.rootMotionInMainState = false;
            m_characterBody.hullClassification = HullClassification.Human;
            m_characterBody.isChampion = false;
        }

        private void IntegrateCharacterModel()
        {
            // Get model base
            Transform modelBase = bodyPrefab.transform.Find("ModelBase");

            // Check for model base
            if (modelBase == null)
            {
                // Create model base if one is not found
                modelBase = new GameObject("ModelBase").transform;
                modelBase.parent = bodyPrefab.transform;
                modelBase.localPosition = modelBasePosition;
                modelBase.localRotation = Quaternion.identity;
            }

            // Integrate model into clone
            modelPrefab.transform.parent = modelBase;
            modelPrefab.transform.localPosition = Vector3.zero;
            modelPrefab.transform.localRotation = Quaternion.identity;

            // Get camera pivot point
            Transform cameraPivot = bodyPrefab.transform.Find("CameraPivot");

            // Check for camera pivot point
            if (cameraPivot == null)
            {
                // Create camera pivot point if one is not found
                cameraPivot = new GameObject("CameraPivot").transform;
                cameraPivot.parent = bodyPrefab.transform;
                cameraPivot.localPosition = cameraPivotPosition;
                cameraPivot.localRotation = Quaternion.identity;
            }

            // Get character aim origin
            Transform aimOrigin = bodyPrefab.transform.Find("AimOrigin");

            // Check for character aim origin
            if (aimOrigin == null)
            {
                // Create character aim origin if one is not found
                aimOrigin = new GameObject("AimOrigin").transform;
                aimOrigin.parent = bodyPrefab.transform;
                aimOrigin.localPosition = aimOriginPosition;
                aimOrigin.localRotation = Quaternion.identity;
            }

            // Pass reference to aim origin into character body
            m_characterBody.aimOriginTransform = aimOrigin;

            // Setup model locator
            ModelLocator modelLocator = bodyPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = modelPrefab.transform;
            modelLocator.modelBaseTransform = modelBase;

            // Get character direction component
            CharacterDirection characterDirection = bodyPrefab.GetComponent<CharacterDirection>();

            // Check for character direction component
            if (characterDirection != null)
            {
                // Setup character direction component
                characterDirection.targetTransform = modelBase;
                characterDirection.overrideAnimatorForwardTransform = null;
                characterDirection.rootMotionAccumulator = null;
                characterDirection.modelAnimator = modelPrefab.GetComponent<Animator>();
                characterDirection.driveFromRootRotation = false;
                characterDirection.turnSpeed = 720f;
            }
        }

        private void SetupCamera()
        {
            // Get camera target component and pass in new camera parameters and reference to new camera pivot
            CameraTargetParams cameraTargetParams = bodyPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = cameraParams;
            cameraTargetParams.cameraPivotTransform = bodyPrefab.transform.Find("CameraPivot");
        }

        private void SetupCollider()
        {
            // Character collider must be as Commando's
            CapsuleCollider capsuleCollider = bodyPrefab.GetComponent<CapsuleCollider>();
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;
        }

        private void SetupCharacterModel()
        {
            // Try get character model
            m_characterModel = bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject.GetComponent<CharacterModel>();

            // Check if character model was pre-attached
            bool preAttached = m_characterModel != null;
            if (!preAttached)
            {
                // Create new character model
                m_characterModel = bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject.AddComponent<CharacterModel>();
            }
                
            // Pass character body reference to character model
            m_characterModel.body = m_characterBody;

            // Setup some misc properties
            m_characterModel.autoPopulateLightInfos = true;
            m_characterModel.invisibilityCount = 0;
            m_characterModel.temporaryOverlays = new List<TemporaryOverlayInstance>();

            // Setup renderer infos depending on if an existing character model was found
            if (!preAttached)
            {
                SetupCustomRendererInfos();
            }
            else
            {
                SetupPreAttachedRendererInfos();
            }
        }

        private void SetupCustomRendererInfos()
        {
            // Get child locator
            ChildLocator childLocator = m_characterModel.GetComponent<ChildLocator>();

            // Check for child locator
            if (childLocator == null)
            {
                // Error and return - unsuccessful
                Log.Error($"Was unable to setup custom renderer infos for survivor '{name}' - Child locator could not be found!");
                return;
            }

            // Store a list of linked renderers from the child locator
            List<CharacterModel.RendererInfo> rendererInfos = [];

            // Cycle through children in child locator
            for (int i = 0; i < childLocator.Count; i++)
            {
                // Get child game object
                GameObject child = childLocator.FindChildGameObject(i);

                // Get renderer from child
                Renderer renderer = child.GetComponent<Renderer>();

                // Check for renderer
                if (renderer == null) continue;

                // Get material from renderer (convert to HG shader)
                Material material = renderer.sharedMaterial.ConvertDefaultShaderToHopoo();

                // Add to renderer infos
                rendererInfos.Add(new CharacterModel.RendererInfo
                {
                    renderer = renderer,
                    defaultMaterial = material,
                    ignoreOverlays = false,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On
                });
            }

            // Pass renderer infos to character model
            m_characterModel.baseRendererInfos = rendererInfos.ToArray();
        }

        private void SetupPreAttachedRendererInfos()
        {
            // Cycle through existing renderer infos
            for (int i = 0; i < m_characterModel.baseRendererInfos.Length; i++)
            {
                // Check for default material
                if (m_characterModel.baseRendererInfos[i].defaultMaterial == null)
                {
                    // Set default material as current material
                    m_characterModel.baseRendererInfos[i].defaultMaterial = m_characterModel.baseRendererInfos[i].renderer.sharedMaterial;
                }

                // Check again for default material
                if (m_characterModel.baseRendererInfos[i].defaultMaterial == null)
                {
                    // Error - No material found for renderer info
                    Log.Error($"No material found for renderer info on renderer '{m_characterModel.baseRendererInfos[i].renderer.gameObject.name}'!");
                }
                else
                {
                    // Convert material to HG shader
                    m_characterModel.baseRendererInfos[i].defaultMaterial.ConvertDefaultShaderToHopoo();
                }
            }
        }

        // Accessors
        public GameObject modelPrefab
        {
            get
            {
                // Check for model prefab
                if (m_modelPrefab == null)
                {
                    // Fetch model prefab from asset bundle
                    m_modelPrefab = Assets.GetModel(m_modelName);
                }

                // Return model prefab
                return m_modelPrefab;
            }
        }
        public GameObject bodyPrefab
        {
            get
            {
                // Check for body prefab
                if (m_bodyPrefab == null)
                {
                    // Create body prefab
                    CreateBodyPrefab();
                }

                // Return body prefab
                return m_bodyPrefab;
            }
        }
        public GameObject crosshair
        {
            get
            {
                // Check for crosshair
                if (m_crosshair == null)
                {
                    // Get crosshair
                    m_crosshair = Assets.GetCrosshair(m_crosshairName);
                }

                // Return crosshair
                return m_crosshair;
            }
        }
        public Texture portrait
        {
            get
            {
                // Check for portrait
                if (m_portrait == null)
                {
                    // Get portrait from asset bundle
                    m_portrait = Assets.GetTexture(m_portraitName);
                }

                // Return portrait
                return m_portrait;
            }
        }
        public Color bodyColour => m_bodyColour;
        public string name { get { return Utils.GetLanguageString(nameToken); } }
        public string bodyName { get { return $"{name}Body"; } }
        public string nameToken { get { return $"FAITHFUL_SURVIVOR_{token}_NAME"; } }
        public string subtitleToken { get { return $"FAITHFUL_SURVIVOR_{token}_SUBTITLE"; } }
        public string token => m_token;
        public int sortPosition => m_sortPosition;
        public bool autoCalculateLevelStats => m_autoCalculateLevelStats;

        // Stat accessors
        public float maxHealth => m_maxHealth;
        public float healthRegen => m_healthRegen;
        public float armour => m_armour;
        public float shield => m_shield;
        public int jumpCount => m_jumpCount;
        public float damage => m_damage;
        public float attackSpeed => m_attackSpeed;
        public float crit => m_crit;
        public float moveSpeed => m_moveSpeed;
        public float acceleration => m_acceleration;
        public float jumpPower => m_jumpPower;
        public float healthGrowth => maxHealth * 0.3f;
        public float regenGrowth => healthRegen * 0.2f;
        public float armourGrowth => 0.0f;
        public float shieldGrowth => 0.0f;
        public float damageGrowth => damage * 0.2f;
        public float attackSpeedGrowth => 0.0f;
        public float critGrowth => 0.0f;
        public float moveSpeedGrowth => 0.0f;
        public float jumpPowerGrowth => 0.0f;

        // Camera settings accessors
        public CharacterCameraParams cameraParams
        {
            get
            {
                // Check for camera params
                if (m_cameraParams == null)
                {
                    // Create camera params
                    m_cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
                    m_cameraParams.data.minPitch = -70;
                    m_cameraParams.data.maxPitch = 70;
                    m_cameraParams.data.wallCushion = 0.1f;
                    m_cameraParams.data.pivotVerticalOffset = cameraVerticalOffset;
                    m_cameraParams.data.idealLocalCameraPos = new Vector3(0, 0, cameraDepth);
                }

                // Return camera params
                return m_cameraParams;
            }
        }
        public Vector3 aimOriginPosition => m_aimOriginPosition;
        public Vector3 modelBasePosition => m_modelBasePosition;
        public Vector3 cameraPivotPosition => m_cameraPivotPosition;
        public float cameraVerticalOffset => m_cameraVerticalOffset;
        public float cameraDepth => m_cameraDepth;
    }
}
