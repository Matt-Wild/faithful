using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
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

        // A cloned character body from the main game that's been emptied of all it's bits and bobs
        private GameObject m_emptyClonePrefab;

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
        public GameObject emptyClonePrefab
        {
            get
            {
                // Check for empty clone prefab
                if (m_emptyClonePrefab == null)
                {
                    // Clone Commando (safest character)
                    GameObject clonedBody = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommmandoBody");

                    // Check for valid clone
                    if (clonedBody == null)
                    {
                        // Error and return null - unsuccessful
                        Log.Error("Was unable to clone Commando character body to create an empty clone!");
                        return null;
                    }

                    // Use Prefab API to clone the Commando body and treat as prefab
                    GameObject newBodyPrefab = PrefabAPI.InstantiateClone(clonedBody, bodyName);

                    // Cycle through children of character body
                    for (int i = newBodyPrefab.transform.childCount - 1; i >= 0; i--)
                    {
                        // Delete child
                        UnityEngine.Object.DestroyImmediate(newBodyPrefab.transform.GetChild(i).gameObject);
                    }

                    // Assign new emptied clone prefab
                    m_emptyClonePrefab = newBodyPrefab;
                }

                // Return empty clone prefab
                return m_emptyClonePrefab;
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
