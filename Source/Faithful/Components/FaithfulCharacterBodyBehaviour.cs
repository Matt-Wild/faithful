using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class FaithfulCharacterBodyBehaviour : NetworkBehaviour
    {
        // Store flags
        public Flags frameFlags = new Flags();
        public Flags stageFlags = new Flags();

        // Store CharacterBody net ID
        [SyncVar(hook = nameof(CharacterIDChanged))] public NetworkInstanceId characterID;

        // Store the adjustment to this character's calculated level
        [SyncVar(hook = nameof(LevelOffsetChanged))] public float levelOffset;

        // Store the level before applying the adjustment
        public float unmodifiedLevel;

        // Store reference to Character Body
        private CharacterBody character;

        // Store if character body has been found
        private bool characterBodyFound = false;

        // Store reference to behaviours
        public FaithfulTJetpackBehaviour tJetpack;
        public FaithfulLeadersPennonBehaviour leadersPennon;
        public FaithfulLongshotGeodeBehaviour longshotGeode;
        public FaithfulTargetingMatrixBehaviour targetingMatrix;
        public FaithfulHermitsShawlBehaviour hermitsShawl;

        // Store if searching for character body
        private bool searchingForCharacterBody = false;

        // List of custom tick behaviour
        private List<CharacterBodyTickBehaviour> m_tickBehaviours = new List<CharacterBodyTickBehaviour>();

        void FixedUpdate()
        {
            FaithfulRuntimeProfiler.StartScope("FaithfulCharacterBodyBehaviour.FixedUpdate");
            try
            {
                // Update tick behaviours
                foreach (CharacterBodyTickBehaviour tickBehaviour in m_tickBehaviours) tickBehaviour.FixedUpdate();
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("FaithfulCharacterBodyBehaviour.FixedUpdate");
            }
        }

        public void AddTickBehaviour(CharacterBodyCallback _callback, float _tickRate)
        {
            // Add tick behaviour
            CharacterBodyTickBehaviour tickBehaviour = new CharacterBodyTickBehaviour(_callback, _tickRate);
            if (characterBodyFound) tickBehaviour.UpdateCharacterBody(character);
            m_tickBehaviours.Add(tickBehaviour);
        }

        public void OnInventoryChanged(Inventory _inventory)
        {
            // Create local item behaviours only when needed
            if (character == null || _inventory == null) return;
            if (leadersPennon == null && _inventory.GetItemCountEffective(Items.GetItem("LEADERS_PENNON").itemDef) > 0) EnsureLeadersPennon();
            if (longshotGeode == null && _inventory.GetItemCountEffective(Items.GetItem("LONGSHOT_GEODE").itemDef) > 0) EnsureLongshotGeode();
            if (hermitsShawl == null && _inventory.GetItemCountEffective(Items.GetItem("HERMITS_SHAWL").itemDef) > 0) EnsureHermitsShawl();

            // Notify this character's item behaviours
            if (leadersPennon != null) leadersPennon.OnInventoryChanged(_inventory);
            if (longshotGeode != null) longshotGeode.OnInventoryChanged(_inventory);
            if (hermitsShawl != null) hermitsShawl.OnInventoryChanged(_inventory);
        }

        public FaithfulLeadersPennonBehaviour EnsureLeadersPennon()
        {
            // Allies can need the visual effect without owning a pennon
            if (leadersPennon == null && character != null)
            {
                leadersPennon = gameObject.AddComponent<FaithfulLeadersPennonBehaviour>();
                leadersPennon.Init(character);
            }
            return leadersPennon;
        }

        public FaithfulLongshotGeodeBehaviour EnsureLongshotGeode()
        {
            // Create and initialise the behaviour if needed
            if (longshotGeode == null && character != null)
            {
                longshotGeode = gameObject.AddComponent<FaithfulLongshotGeodeBehaviour>();
                longshotGeode.Init(character);
            }
            return longshotGeode;
        }

        public FaithfulHermitsShawlBehaviour EnsureHermitsShawl()
        {
            // Create and initialise the behaviour if needed
            if (hermitsShawl == null && character != null)
            {
                hermitsShawl = gameObject.AddComponent<FaithfulHermitsShawlBehaviour>();
                hermitsShawl.Init(character);
            }
            return hermitsShawl;
        }

        public void AssignCharacter(CharacterBody _character)
        {
            if (characterBodyFound) return;
            character = _character;
            StartCoroutine(LinkCharacterBody());
        }

        private void OnCharacterBodyStart(CharacterBody _body)
        {
            // Finish linking if the body arrived after the polling window
            NetworkIdentity identity = _body.GetComponent<NetworkIdentity>();
            if (identity != null && identity.netId == characterID) AssignCharacter(_body);
        }

        public void OnCharacterBodyFixedUpdate()
        {
            // Update this character's Hermit's Shawl
            if (hermitsShawl != null) hermitsShawl.OnCharacterBodyFixedUpdate();
        }

        private void LevelOffsetChanged(float _newValue)
        {
            // Update the adjustment and refresh stats on connected clients
            levelOffset = _newValue;
            if (character != null) character.MarkAllStatsDirty();
        }

        private void CharacterIDChanged(NetworkInstanceId _newValue)
        {
            // Ensure new value is set
            characterID = _newValue;

            // Start coroutine to find and link character body
            StartCoroutine(LinkCharacterBody());
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (characterBodyFound) return;

            // Listen for bodies which arrive after this helper
            CharacterBody.onBodyStartGlobal += OnCharacterBodyStart;

            // Start coroutine to find and link character body
            StartCoroutine(LinkCharacterBody());

            // Check for character body on delay
            Invoke("CheckForCharacterBody", 8.0f);
        }

        private void CheckForCharacterBody()
        {
            // Check if debug mode, not the server and no character body
            if (!characterBodyFound && !NetworkServer.active && Utils.verboseConsole)
            {
                // Send warning
                Log.Warning($"Faithful Character Body behaviour not linked for net ID {characterID}.");
            }
        }

        private void LateUpdate()
        {
            // Reset flags
            frameFlags.Reset();

            // Check if character body is destroyed
            if (characterBodyFound && character == null)
            {
                // Check if server
                if (NetworkServer.active)
                {
                    // Destroy faithful behaviour
                    NetworkServer.Destroy(gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            CharacterBody.onBodyStartGlobal -= OnCharacterBodyStart;

            // Release this character's helper registration
            Utils.UnregisterFaithfulCharacterBodyBehaviour(character, this);
        }

        private IEnumerator LinkCharacterBody()
        {
            // Only attempt to link if not already attempting to link
            if (!searchingForCharacterBody && !characterBodyFound)
            {
                // Beginning search
                searchingForCharacterBody = true;

                // Poll briefly for spawn ordering, then rely on the body start event
                float searchStarted = Time.realtimeSinceStartup;
                WaitForSecondsRealtime retryDelay = new WaitForSecondsRealtime(0.25f);
                while (character == null)
                {
                    character = ClientScene.FindLocalObject(characterID)?.GetComponent<CharacterBody>();
                    if (character != null) break;
                    if (Time.realtimeSinceStartup - searchStarted >= 30.0f)
                    {
                        searchingForCharacterBody = false;
                        yield break;
                    }
                    yield return retryDelay;
                }

                CharacterBody.onBodyStartGlobal -= OnCharacterBodyStart;

                // Set as character body found
                characterBodyFound = true;

                // Cycle through tick behaviours and provide character body
                foreach (CharacterBodyTickBehaviour tickBehaviour in m_tickBehaviours) tickBehaviour.UpdateCharacterBody(character);

                // Register behaviour with utils
                Utils.RegisterFaithfulCharacterBodyBehaviour(character, this);

                // Apply an adjustment received before the character body was linked
                if (levelOffset != 0.0f) character.MarkAllStatsDirty();

                // Fetch TJetpack behaviour
                tJetpack = GetComponent<FaithfulTJetpackBehaviour>();

                // Assign character
                tJetpack.AssignCharacter(character);

                // Fetch targeting matrix behaviour
                targetingMatrix = GetComponent<FaithfulTargetingMatrixBehaviour>();

                // Initialise targeting matrix behaviour
                targetingMatrix.Init(character);

                // Check for inventory
                if (character.inventory != null)
                {
                    // Initialise local behaviours for items already held
                    OnInventoryChanged(character.inventory);

                    // Update jetpack with jetpack item count
                    tJetpack.UpdateItemCount(character.inventory.GetItemCountEffective(Items.GetItem("4T0N_JETPACK").itemDef));
                }

                // Restore effects for buffs received before this helper was linked
                character.UpdateAllTemporaryVisualEffects();

                // Check if debug mode
                if (Utils.verboseConsole)
                {
                    // Get string for if client or server
                    string messageSource = NetworkServer.active ? "SERVER" : "CLIENT";

                    Log.Message($"[{messageSource}] - Faithful Character Body behaviour linked for character '{character.name}' with net ID {characterID}.");
                }

                // Done searching
                searchingForCharacterBody = false;
            }
        }
    }

    internal interface ICharacterBehaviour
    {
        public void FetchSettings();
    }

    internal class CharacterBodyTickBehaviour
    {
        // Associated character body
        CharacterBody m_body;

        // Logic for tick behaviour
        private CharacterBodyCallback m_callback;

        // Ticks per second (tick rate)
        private float m_tickRate;
        private float m_timer = 0.0f;

        public CharacterBodyTickBehaviour(CharacterBodyCallback _callback, float _tickRate)
        {
            m_callback = _callback;
            m_tickRate = _tickRate;

            // Set timer
            m_timer = 1.0f / m_tickRate;
        }

        public void UpdateCharacterBody(CharacterBody _body)
        {
            m_body = _body;
        }

        public void FixedUpdate()
        {
            // Do tick rate
            m_timer -= Time.fixedDeltaTime;
            if (m_timer <= 0f)
            {
                m_timer = 1.0f / m_tickRate;

                // Callback if character body exists
                if (m_body != null)
                {
                    m_callback(m_body);
                }
            }
        }
    }
}
