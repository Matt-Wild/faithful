using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal enum AIType
    {
        Custom,
        Acrid,
        Artificer,
        Bandit,
        Captain,
        Chef,
        Commando,
        Engineer,
        FalseSon,
        Heretic,
        Huntress,
        Loader,
        Mercenary,
        Mult,
        Railgunner,
        Rex,
        Seeker,
        VoidFiend
    }

    internal class Survivor : IPrintable
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

        // AI master for this survivor (used for dopplegangers etc)
        private AIType m_aiType;

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

        // Display prefab for this survivor
        private GameObject m_displayPrefab;

        // The master prefab for this survivor (used for dopplegangers etc)
        private GameObject m_masterPrefab;

        // The character body for the cloned and adjusted prefab
        private CharacterBody m_characterBody;

        // The character model component for the body prefab
        private CharacterModel m_characterModel;

        // The survivor definition for this survivor
        private SurvivorDef m_survivorDef;

        public void Init(string _token, string _modelName, string _portraitName, Color? _bodyColor = null, int _sortPosition = 100, string _crosshairName = "Standard", float _maxHealth = 110.0f,
                         float _healthRegen = 1.0f, float _armour = 0.0f, float _shield = 0.0f, int _jumpCount = 1, float _damage = 12.0f, float _attackSpeed = 1.0f, float _crit = 1.0f,
                         float _moveSpeed = 7.0f, float _acceleration = 80.0f, float _jumpPower = 15.0f, bool _autoCalculateLevelStats = true, Vector3? _aimOriginPosition = null,
                         Vector3? _modelBasePosition = null, Vector3? _cameraPivotPosition = null, float _cameraVerticalOffset = 1.37f, float _cameraDepth = -10.0f,
                         AIType _aiType = AIType.Commando)
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

            // Assign AI type
            m_aiType = _aiType;

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

            try
            {
                // Create survivor
                CreateSurvivor();

                // Log that interactable has been created
                Print.Debug(this, "Survivor created");
            }

            // Unsuccessful
            catch (Exception e)
            {
                // Log reason
                Print.Error(this, $"Survivor creation failed: {e.Message}\n{e.StackTrace}");
            }
        }

        private void CreateSurvivor()
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
                Print.Error(this, $"Could not create '{name}' survivor! Model: {modelPrefab}");
                return;
            }

            // Create clone body to built off of
            CreateCloneBody();

            // Setup the clone body
            SetupCloneBody();

            // Setup character model
            SetupCharacterModel();

            // Setup character item displays
            SetupItemDisplays();

            // Setup display prefab
            SetupDisplayPrefab();

            // Setup survivor definition
            SetupSurvivorDefinition();

            // Setup AI master for character
            SetupMaster();
        }

        private void CreateCloneBody()
        {
            // Clone Commando (safest character)
            GameObject clonedBody = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody");

            // Check for valid clone
            if (clonedBody == null)
            {
                // Error and return null - unsuccessful
                Print.Error(this, "Was unable to clone Commando character body to create an empty clone");
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

            // Add to body prefab content pack
            ContentAddition.AddBody(bodyPrefab);
        }

        private void ConfigureCharacterBody()
        {
            // Fetch character body if not already found
            m_characterBody ??= bodyPrefab.GetComponent<CharacterBody>();

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

            // Set up the hurt boxes
            SetupHurtBoxGroup();

            // Setup aim animator
            SetupAimAnimator();

            // Setup footstep controller
            SetupFootstepController();

            // Setup ragdoll
            SetupRagdoll();
        }

        private void SetupCustomRendererInfos()
        {
            // Get child locator
            ChildLocator childLocator = m_characterModel.GetComponent<ChildLocator>();

            // Check for child locator
            if (childLocator == null)
            {
                // Error and return - unsuccessful
                Print.Error(this, $"Was unable to setup custom renderer infos for survivor '{name}' - Child locator could not be found");
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
                    Print.Error(this, $"No material found for renderer info on renderer '{m_characterModel.baseRendererInfos[i].renderer.gameObject.name}'");
                }
                else
                {
                    // Convert material to HG shader
                    m_characterModel.baseRendererInfos[i].defaultMaterial.ConvertDefaultShaderToHopoo();
                }
            }
        }

        private void SetupHurtBoxGroup()
        {
            // Check for already existing hurt box group
            if (bodyPrefab.GetComponent<HurtBoxGroup>() != null)
            {
                Print.Debug(this, $"Hurt Box Group already exists on model prefab - No need to set up Hurt Boxes");
                return;
            }

            // Fetch child locator
            ChildLocator childLocator = m_characterModel.gameObject.GetComponent<ChildLocator>();

            // Check for main hurtbox
            if (string.IsNullOrEmpty(childLocator.FindChildNameInsensitive("MainHurtbox")))
            {
                // Error and return - unsuccessful
                Print.Error(this, "Could not set up main hurtbox - Missing transform pair in ChildLocator called 'MainHurtbox'");
                return;
            }

            // Create hurt box group
            HurtBoxGroup hurtBoxGroup = m_characterModel.gameObject.AddComponent<HurtBoxGroup>();

            // Attempt to fetch head hurtbox
            HurtBox headHurtbox = null;
            GameObject headHurtboxObject = childLocator.FindChildGameObjectInsensitive("HeadHurtbox");
            if (headHurtboxObject)
            {
                // Head hurtbox found - Setup
                Print.Debug(this, "HeadHurtboxFound - Setting up");
                headHurtbox = headHurtboxObject.AddComponent<HurtBox>();
                headHurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
                headHurtbox.healthComponent = bodyPrefab.GetComponent<HealthComponent>();
                headHurtbox.isBullseye = false;
                headHurtbox.isSniperTarget = true;
                headHurtbox.damageModifier = HurtBox.DamageModifier.Normal;
                headHurtbox.hurtBoxGroup = hurtBoxGroup;
                headHurtbox.indexInGroup = 1;
            }

            // Setup main hurtbox
            HurtBox mainHurtbox = childLocator.FindChildGameObjectInsensitive("MainHurtbox").AddComponent<HurtBox>();
            mainHurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            mainHurtbox.healthComponent = bodyPrefab.GetComponent<HealthComponent>();
            mainHurtbox.isBullseye = true;
            mainHurtbox.isSniperTarget = headHurtbox == null;
            mainHurtbox.damageModifier = HurtBox.DamageModifier.Normal;
            mainHurtbox.hurtBoxGroup = hurtBoxGroup;
            mainHurtbox.indexInGroup = 0;

            // Check if head hurtbox exists
            if (headHurtbox)
            {
                // Setup hurt box group
                hurtBoxGroup.hurtBoxes =
                [
                    mainHurtbox,
                    headHurtbox
                ];
            }

            // No head hurtbox
            else
            {
                // Setup hurt box group
                hurtBoxGroup.hurtBoxes =
                [
                    mainHurtbox,
                ];
            }

            // Assign hurt box group
            hurtBoxGroup.mainHurtBox = mainHurtbox;
            hurtBoxGroup.bullseyeCount = 1;

            // Get health component
            HealthComponent healthComponent = bodyPrefab.GetComponent<HealthComponent>();

            // Cycle through all hurt box groups in survivor body
            foreach (HurtBoxGroup currentHurtBoxGroup in bodyPrefab.GetComponentsInChildren<HurtBoxGroup>())
            {
                // Assign health component for hurt box group main hurt box
                currentHurtBoxGroup.mainHurtBox.healthComponent = healthComponent;

                // Cycle through hurt boxes in hurt box group
                for (int i = 0; i < currentHurtBoxGroup.hurtBoxes.Length; i++)
                {
                    // Assign health component to each individual hurt box
                    currentHurtBoxGroup.hurtBoxes[i].healthComponent = healthComponent;
                }
            }
        }

        private void SetupAimAnimator()
        {
            // Set up aim animator
            AimAnimator aimAnimator = m_characterModel.gameObject.AddComponent<AimAnimator>();
            aimAnimator.directionComponent = bodyPrefab.GetComponent<CharacterDirection>();
            aimAnimator.pitchRangeMax = 60f;
            aimAnimator.pitchRangeMin = -60f;
            aimAnimator.yawRangeMin = -80f;
            aimAnimator.yawRangeMax = 80f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 3f;
            aimAnimator.inputBank = bodyPrefab.GetComponent<InputBankTest>();
        }

        private void SetupFootstepController()
        {
            // Setup footstep controller
            FootstepHandler footstepHandler = m_characterModel.gameObject.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");
        }

        private void SetupRagdoll()
        {
            // Fetch ragdoll controller
            RagdollController ragdollController = m_characterModel.gameObject.GetComponent<RagdollController>();

            // Skip if no ragdoll controller found
            if (ragdollController == null) return;

            // Cycle through bone transforms in ragdoll controller
            foreach (Transform boneTransform in ragdollController.bones)
            {
                // Check for bone transform
                if (boneTransform != null)
                {
                    // Set bone transform layer
                    boneTransform.gameObject.layer = LayerIndex.ragdoll.intVal;

                    // Fetch bone collider
                    Collider boneCollider = boneTransform.GetComponent<Collider>();

                    // Check for bone collider
                    if (boneCollider)
                    {
                        // Set bone physics material
                        boneCollider.sharedMaterial = Assets.ragdollMaterial;
                    }

                    // No bone collider found
                    else
                    {
                        // Log error
                        Print.Error(this, $"Ragdoll bone '{boneTransform.gameObject.name}' doesn't have a collider - Ragdoll will break");
                    }
                }
            }
        }

        private void SetupItemDisplays()
        {
            // Create item display rule set
            ItemDisplayRuleSet itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
            itemDisplayRuleSet.name = "idrs" + bodyName;

            // Assign rule set into character model
            m_characterModel.itemDisplayRuleSet = itemDisplayRuleSet;

            /*if (itemDisplays != null)
            {
                Modules.ItemDisplays.queuedDisplays++;
                RoR2.ContentManagement.ContentManager.onContentPacksAssigned += SetItemDisplays;
            }*/
        }

        private void SetupDisplayPrefab()
        {
            // Attempt to fetch display object
            m_displayPrefab = Assets.GetObject(displayPrefabName);
            if (m_displayPrefab == null)
            {
                // Could not find display object - error and return
                Print.Error(this, $"Could not find display prefab '{displayPrefabName}'");
            }

            // Apply base render infos to character model
            m_characterModel.baseRendererInfos = bodyPrefab.GetComponentInChildren<CharacterModel>().baseRendererInfos;

            // Convert all materials in all renderers for this game object to HG shader
            Utils.ConvertAllRenderersToHopooShader(m_displayPrefab);
        }

        private void SetupSurvivorDefinition()
        {
            // Create survivor definition
            m_survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            m_survivorDef.bodyPrefab = bodyPrefab;
            m_survivorDef.displayPrefab = m_displayPrefab;
            m_survivorDef.primaryColor = bodyColour;

            // Setup language tokens
            m_survivorDef.cachedName = bodyPrefab.name.Replace("Body", "");
            m_survivorDef.displayNameToken = nameToken;
            m_survivorDef.descriptionToken = descriptionToken;
            m_survivorDef.outroFlavorToken = outroFlavourToken;
            m_survivorDef.mainEndingEscapeFailureFlavorToken = outroFailureToken;

            // Add sort position for survivor
            m_survivorDef.desiredSortPosition = sortPosition;

            // TODO
            //survivorDef.unlockableDef = unlockableDef;

            // Add to content pack
            ContentAddition.AddSurvivorDef(m_survivorDef);
        }

        private void SetupMaster()
        {
            // Check for custom AI
            if (aiType == AIType.Custom)
            {
                // Setup custom AI master
                SetupCustomMaster();

                // Done
                return;
            }

            // Check AI type
            string masterString = "Merc";
            switch (aiType)
            {
                case AIType.Acrid:
                    masterString = "Croco";
                    break;
                case AIType.Artificer:
                    masterString = "Mage";
                    break;
                case AIType.Bandit:
                    masterString = "Bandit2";
                    break;
                case AIType.Captain:
                    masterString = "Captain";
                    break;
                case AIType.Chef:
                    masterString = "Chef";
                    break;
                case AIType.Commando:
                    masterString = "Commando";
                    break;
                case AIType.Engineer:
                    masterString = "Engi";
                    break;
                case AIType.FalseSon:
                    masterString = "FalseSon";
                    break;
                case AIType.Heretic:
                    masterString = "Heretic";
                    break;
                case AIType.Huntress:
                    masterString = "Huntress";
                    break;
                case AIType.Loader:
                    masterString = "Loader";
                    break;
                case AIType.Mercenary:
                    masterString = "Merc";
                    break;
                case AIType.Mult:
                    masterString = "Toolbot";
                    break;
                case AIType.Railgunner:
                    masterString = "Railgunner";
                    break;
                case AIType.Rex:
                    masterString = "Treebot";
                    break;
                case AIType.Seeker:
                    masterString = "Seeker";
                    break;
                case AIType.VoidFiend:
                    masterString = "VoidSurvivor";
                    break;
            }

            // Get cloned master
            m_masterPrefab = Assets.GetClonedDopplegangerMaster(bodyPrefab, masterName, masterString);
        }

        protected virtual void SetupCustomMaster()
        {
            // Check not overriden but AI type is custom
            if (aiType == AIType.Custom)
            {
                // Error
                Print.Error(this, "AI type is set to custom but no custom master behaviour is provided");
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
                    // Create survivor
                    CreateSurvivor();
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
        public AIType aiType => m_aiType;
        public string name { get { return Utils.GetLanguageString(nameToken); } }
        public string bodyName { get { return $"{name}Body"; } }
        public string masterName { get { return $"{name}MonsterMaster"; } }
        public string displayPrefabName { get { return $"{name}Display"; } }
        public string nameToken { get { return $"FAITHFUL_SURVIVOR_{token}_NAME"; } }
        public string descriptionToken { get { return $"FAITHFUL_SURVIVOR_{token}_DESCRIPTION"; } }
        public string subtitleToken { get { return $"FAITHFUL_SURVIVOR_{token}_SUBTITLE"; } }
        public string outroFlavourToken { get { return $"FAITHFUL_SURVIVOR_{token}_OUTRO_FLAVOR"; } }
        public string outroFailureToken { get { return $"FAITHFUL_SURVIVOR_{token}_OUTRO_FAILURE"; } }
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

        // Interface requirements
        public string printIdentifier => name;
    }
}
