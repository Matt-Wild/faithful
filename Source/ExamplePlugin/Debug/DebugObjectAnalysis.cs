using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugObjectAnalysis : DebugPanel
    {
        // Store title reference
        Text titleText;

        // Store parent section behaviour
        DebugObjectAnalysisParentSection parentSection;

        // Store object state buttons behaviour
        DebugObjectAnalysisStateButtons stateButtons;

        // Store object components section behaviour
        DebugObjectAnalysisComponentsSection componentsSection;

        // Store object children section behaviour
        DebugObjectAnalysisChildrenSection childrenSection;

        // Store current object that is being analysed
        GameObject analysedObject;

        // Store if currently analysing an object
        bool analysing = false;

        public override void Awake()
        {
            // Call base awake
            base.Awake();

            // Get title text
            titleText = transform.Find("PanelTitle").GetComponent<Text>();

            // Add parent section behaviour
            parentSection = transform.Find("ParentSection").gameObject.AddComponent<DebugObjectAnalysisParentSection>();
            parentSection.Init(this);

            // Add state buttons behaviour
            stateButtons = gameObject.AddComponent<DebugObjectAnalysisStateButtons>();
            stateButtons.Init(this);

            // Add components section behaviour
            componentsSection = transform.Find("ComponentsSection").gameObject.AddComponent<DebugObjectAnalysisComponentsSection>();
            componentsSection.Init(this);

            // Add children section behaviour
            childrenSection = transform.Find("ChildrenSection").gameObject.AddComponent<DebugObjectAnalysisChildrenSection>();
            childrenSection.Init(this);

            // Set to analysing nothing by default
            AnalyseObject(null);
        }

        private void OnEnable()
        {
            // Add hooks
            On.RoR2.PingerController.AttemptPing += OnAttemptPing;
        }

        private void OnDisable()
        {
            // Remove hooks
            On.RoR2.PingerController.AttemptPing -= OnAttemptPing;
        }

        private void FixedUpdate()
        {
            // Check if analysing but analysed object reference is null
            if (analysing && analysedObject == null)
            {
                // Stop analysing
                AnalyseObject();
            }
        }

        public override void OnMinimise()
        {
            // Call base close function
            base.OnMinimise();

            // Stop analysing object
            AnalyseObject();
        }

        public override void OnMaximise()
        {
            // Call base close function
            base.OnMaximise();

            // Stop analysing object
            AnalyseObject();
        }

        public void AnalyseObject(GameObject _object = null)
        {
            // Check if panel is minimised
            if (minimised)
            {
                // Don't analyse object
                _object = null;
            }

            // Assign analysed object
            analysedObject = _object;

            // Check if null object
            if (analysedObject == null)
            {
                // Set panel title
                titleText.text = $"Object Analysis";

                // Set as not analysing an object
                analysing = false;
            }

            // Object exists
            else
            {
                // Set panel title
                titleText.text = $"Analysing '{analysedObject.name}'";

                // Set as analysing an object
                analysing = true;
            }

            // Set analysed game object for parent section
            parentSection.AnalyseObject(analysedObject);

            // Set analysed game object for state buttons
            stateButtons.AnalyseObject(analysedObject);

            // Set analysed game object for components section
            componentsSection.AnalyseObject(analysedObject);

            // Set analysed game object for children section
            childrenSection.AnalyseObject(analysedObject);
        }

        public void RefreshAnalysedObject()
        {
            // Analyse same object again
            AnalyseObject(analysedObject);
        }

        private void OnAttemptPing(On.RoR2.PingerController.orig_AttemptPing orig, RoR2.PingerController self, Ray aimRay, GameObject bodyObject)
        {
            // Prepare for ping
            float num;
            aimRay = RoR2.CameraRigController.ModifyAimRayIfApplicable(aimRay, bodyObject, out num);
            float maxDistance = 1000f + num;

            // Store if object to be analysed has been found
            bool found = false;

            // Initialise raycast hit data
            RaycastHit raycastHit;

            // Check for character
            if (RoR2.Util.CharacterRaycast(bodyObject, aimRay, out raycastHit, maxDistance, RoR2.LayerIndex.entityPrecise.mask | RoR2.LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
            {
                // Check for hurtbox
                RoR2.HurtBox component = raycastHit.collider.GetComponent<RoR2.HurtBox>();
                if (component && component.healthComponent)
                {
                    // Analyse object
                    AnalyseObject(component.healthComponent.body.gameObject);

                    // Object has been found
                    found = true;
                }
            }

            // Check for other
            if (!found && RoR2.Util.CharacterRaycast(bodyObject, aimRay, out raycastHit, maxDistance, RoR2.LayerIndex.world.mask | RoR2.LayerIndex.CommonMasks.characterBodiesOrDefault | RoR2.LayerIndex.pickups.mask, QueryTriggerInteraction.Collide))
            {
                // Mimic ping behaviour
                GameObject gameObject = raycastHit.collider.gameObject;
                NetworkIdentity networkIdentity = gameObject.GetComponentInParent<NetworkIdentity>();
                RoR2.ForcePingable component2 = gameObject.GetComponent<RoR2.ForcePingable>();
                if (!networkIdentity && (component2 == null || !component2.bypassEntityLocator))
                {
                    Transform parent = gameObject.transform.parent;
                    RoR2.EntityLocator entityLocator = parent ? parent.GetComponentInChildren<RoR2.EntityLocator>() : gameObject.GetComponent<RoR2.EntityLocator>();
                    if (entityLocator)
                    {
                        gameObject = entityLocator.entity;
                    }
                }

                // Analyse object
                AnalyseObject(gameObject);
            }

            // Run original processes
            orig(self, aimRay, bodyObject);
        }
    }

    internal class DebugObjectAnalysisParentSection : MonoBehaviour
    {
        // Store reference to debug object analysis behaviour
        DebugObjectAnalysis analysisBehaviour;

        // Store reference to title
        Text title;

        // Store reference to select parent button
        Button selectParentButton;

        // Store reference to target parent game object
        GameObject targetParent = null;

        public void Init(DebugObjectAnalysis _analysisBehaviour)
        {
            // Assign analysis behaviour
            analysisBehaviour = _analysisBehaviour;
        }

        private void Awake()
        {
            // Get title text
            title = transform.Find("Title").GetComponent<Text>();

            // Get select parent button
            selectParentButton = transform.Find("SelectParentButton").GetComponent<Button>();

            // Add button behaviour
            selectParentButton.onClick.AddListener(OnParentSelectPressed);
        }

        public void AnalyseObject(GameObject _object)
        {
            // Check if null object
            if (_object == null)
            {
                // Set title text
                title.text = "Parent: None";

                // Hide select parent button
                selectParentButton.gameObject.SetActive(false);

                return;
            }

            // Get target parent
            targetParent = _object.transform.parent?.gameObject;

            // Check if no parent
            if (targetParent == null)
            {
                // Set title text
                title.text = "Parent: None";

                // Hide select parent button
                selectParentButton.gameObject.SetActive(false);

                return;
            }

            // Set title text
            title.text = $"Parent: '{targetParent.name}'";

            // Show select parent button
            selectParentButton.gameObject.SetActive(true);
        }

        private void OnParentSelectPressed()
        {
            // Check for target parent
            if (targetParent != null)
            {
                // Analyse target parent
                analysisBehaviour.AnalyseObject(targetParent);
            }
        }
    }

    internal class DebugObjectAnalysisStateButtons : MonoBehaviour
    {
        // Store reference to debug object analysis behaviour
        DebugObjectAnalysis analysisBehaviour;

        // Store reference to buttons
        Button enableButton;
        Button disableButton;
        Button deleteButton;

        // Store current object that is being analysed
        GameObject analysedObject;

        public void Init(DebugObjectAnalysis _analysisBehaviour)
        {
            // Assign analysis behaviour
            analysisBehaviour = _analysisBehaviour;
        }

        private void Awake()
        {
            // Get buttons
            enableButton = transform.Find("EnableButton").GetComponent<Button>();
            disableButton = transform.Find("DisableButton").GetComponent<Button>();
            deleteButton = transform.Find("DeleteButton").GetComponent<Button>();

            // Add button behaviours
            enableButton.onClick.AddListener(OnEnablePressed);
            disableButton.onClick.AddListener(OnDisablePressed);
            deleteButton.onClick.AddListener(OnDeletePressed);
        }

        public void AnalyseObject(GameObject _object)
        {
            // Assign analysed object
            analysedObject = _object;

            // Check if null object
            if (analysedObject == null)
            {
                // Hide buttons
                enableButton.gameObject.SetActive(false);
                disableButton.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(false);

                return;
            }

            // Show buttons
            enableButton.gameObject.SetActive(true);
            disableButton.gameObject.SetActive(true);
            deleteButton.gameObject.SetActive(true);
        }

        private void OnEnablePressed()
        {
            // Check if no object being analysed
            if (analysedObject == null) return;

            // Enable object
            analysedObject.SetActive(true);
        }

        private void OnDisablePressed()
        {
            // Check if no object being analysed
            if (analysedObject == null) return;

            // Disable object
            analysedObject.SetActive(false);
        }

        private void OnDeletePressed()
        {
            // Check if no object being analysed
            if (analysedObject == null) return;

            // Destroy object
            Destroy(analysedObject);

            // Analyse null object
            analysisBehaviour.AnalyseObject();
        }
    }

    internal class DebugObjectAnalysisScrollSection : MonoBehaviour
    {
        // Store reference to debug object analysis behaviour
        DebugObjectAnalysis analysisBehaviour;

        // Store scroll entry prefab
        GameObject scrollEntryPrefab;

        // Store reference to scroll rect
        ScrollRect scrollRect;

        // Store reference to scroll panel transform
        Transform scrollPanel;

        // Store reference to title
        protected Text title;

        // Store list of current scroll entries
        List<DebugObjectAnalysisScrollEntry> scrollEntries = new List<DebugObjectAnalysisScrollEntry>();

        public void Init(DebugObjectAnalysis _analysisBehaviour)
        {
            // Assign analysis behaviour
            analysisBehaviour = _analysisBehaviour;

            // Get scroll entry prefab
            scrollEntryPrefab = Assets.GetObject("DebugObjectAnalysisEntry");

            // Get title
            title = transform.Find("Title").GetComponent<Text>();
        }

        private void Awake()
        {
            // Get scroll rect
            scrollRect = transform.Find("ScrollMenu").gameObject.GetComponent<ScrollRect>();

            // Get scroll panel transform
            scrollPanel = transform.Find("ScrollMenu").Find("ScrollPanel");
        }

        public virtual void AnalyseObject(GameObject _object)
        {
            // Destroy existing scroll entries
            DestroyScrollEntries();
        }

        public void RefreshAnalysedObject()
        {
            // Refresh analysed object
            analysisBehaviour.RefreshAnalysedObject();
        }

        public void ChangeAnalysedObject(GameObject _newObject)
        {
            // Analyse new game object
            analysisBehaviour.AnalyseObject(_newObject);
        }

        private void DestroyScrollEntries()
        {
            // Cycle through scroll entries
            foreach (DebugObjectAnalysisScrollEntry entry in scrollEntries)
            {
                // Destroy entry
                Destroy(entry.gameObject);
            }

            // Clear scroll entries list
            scrollEntries.Clear();
        }

        protected DebugObjectAnalysisScrollEntry CreateScrollEntry(Object _entryObject)
        {
            // Create new scroll entry
            GameObject scrollEntry = Instantiate(scrollEntryPrefab);
            scrollEntry.transform.SetParent(scrollPanel, false);

            // Add scroll entry behaviour
            DebugObjectAnalysisScrollEntry behaviour = scrollEntry.AddComponent<DebugObjectAnalysisScrollEntry>();
            behaviour.Init(this, _entryObject);

            // Add to scroll entries list
            scrollEntries.Add(behaviour);

            // Reset position of scroll rect
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition = 1.0f;

            // Return newly created scroll entry
            return behaviour;
        }
    }

    internal class DebugObjectAnalysisComponentsSection : DebugObjectAnalysisScrollSection
    {
        public override void AnalyseObject(GameObject _object)
        {
            // Call base method
            base.AnalyseObject(_object);

            // Check for null object
            if (_object == null)
            {
                // Set title
                title.text = "Components:";

                return;
            }

            // Get all components
            Component[] components = _object.GetComponents<Component>();

            // Set title
            title.text = $"Components ({components.Length}):";

            // Cycle through components
            foreach (Component component in components)
            {
                // Create scroll entry
                DebugObjectAnalysisScrollEntry entry = CreateScrollEntry(component);

                // Set entry title
                entry.SetTitle(component.GetType().ToString());
            }
        }
    }

    internal class DebugObjectAnalysisChildrenSection : DebugObjectAnalysisScrollSection
    {
        public override void AnalyseObject(GameObject _object)
        {
            // Call base method
            base.AnalyseObject(_object);

            // Check for null object
            if (_object == null)
            {
                // Set title
                title.text = "Children:";

                return;
            }

            // Get children
            List<GameObject> children = GetChildren(_object);

            // Set title
            title.text = $"Children ({children.Count}):";

            // Cycle through children
            foreach (GameObject child in children)
            {
                // Create scroll entry
                DebugObjectAnalysisScrollEntry entry = CreateScrollEntry(child);

                // Set entry title
                entry.SetTitle($"'{child.name}'");
            }
        }

        private List<GameObject> GetChildren(GameObject _parent)
        {
            // Initialise list
            List<GameObject> children = new List<GameObject>();

            // Cycle through children
            foreach (Transform child in _parent.transform)
            {
                // Add to list
                children.Add(child.gameObject);
            }

            // Return list
            return children;
        }
    }

    internal class DebugObjectAnalysisScrollEntry : MonoBehaviour
    {
        // Store reference to scroll section
        DebugObjectAnalysisScrollSection scrollSection;

        // Store entry object
        Object entryObject;

        // Store reference to entry titles
        Text activeTitle;
        Text inactiveTitle;

        // Store reference to select button
        Button selectButton;

        // Store if entry object is enabled
        bool objectEnabled = true;

        public void Init(DebugObjectAnalysisScrollSection _scrollSection, Object _entryObject)
        {
            // Assign scroll section
            scrollSection = _scrollSection;

            // Assign entry object
            entryObject = _entryObject;

            // Get title
            activeTitle = transform.Find("ActiveTitle").GetComponent<Text>();
            inactiveTitle = transform.Find("InactiveTitle").GetComponent<Text>();

            // Get select button
            selectButton = transform.Find("SelectButton").GetComponent<Button>();

            // Add select button behaviour
            selectButton.onClick.AddListener(OnSelectPressed);

            // Update Titles
            UpdateTitles();

            // Update buttons
            UpdateButtons();
        }

        private void FixedUpdate()
        {
            // Check if entry object is null
            if (entryObject == null)
            {
                // Refresh analysed object
                scrollSection.RefreshAnalysedObject();
            }

            // Check if entry object is active on this frame
            bool currentEnabled = CheckEntryObjectActive();

            // Check if different from stored state
            if (currentEnabled != objectEnabled)
            {
                // Update object enabled
                objectEnabled = currentEnabled;

                // Update Titles
                UpdateTitles();
            }
        }

        private void UpdateTitles()
        {
            // Update which title is enabled
            activeTitle.gameObject.SetActive(objectEnabled);
            inactiveTitle.gameObject.SetActive(!objectEnabled);
        }

        private void UpdateButtons()
        {
            // Check if game object
            GameObject gameObjectCast = entryObject as GameObject;
            if (gameObjectCast != null)
            {
                // Enable select button
                selectButton.gameObject.SetActive(true);
                return;
            }

            // Disable select button
            selectButton.gameObject.SetActive(false);
            return;
        }

        private void OnSelectPressed()
        {
            // Check if game object
            GameObject gameObjectCast = entryObject as GameObject;
            if (gameObjectCast != null)
            {
                // Analyse new game object
                scrollSection.ChangeAnalysedObject(gameObjectCast);
                return;
            }
        }

        private bool CheckEntryObjectActive()
        {
            // Check if game object
            GameObject gameObjectCast = entryObject as GameObject;
            if (gameObjectCast != null)
            {
                // Return if game object if active in hierarchy
                return gameObjectCast.activeInHierarchy;
            }

            // Check if unity engine behaviour
            UnityEngine.Behaviour behaviourCast = entryObject as UnityEngine.Behaviour;
            if (behaviourCast != null)
            {
                // Return if behaviour is active and enabled
                return behaviourCast.isActiveAndEnabled;
            }

            // Check if component
            Component componentCast = entryObject as Component;
            if (componentCast != null)
            {
                // Return if component game object is active in hierarchy
                return componentCast.gameObject.activeInHierarchy;
            }

            // Otherwise assume active
            return true;
        }

        public void SetTitle(string _title)
        {
            // Set titles
            activeTitle.text = _title;
            inactiveTitle.text = _title;
        }
    }
}
