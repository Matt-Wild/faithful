using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugObjectAnalysis : DebugPanel
    {
        // Store title reference
        Text titleText;

        // Store tag and layer text elements
        Text tagText;
        Text layerText;

        // Store parent section behaviour
        DebugObjectAnalysisParentSection parentSection;

        // Store object state buttons behaviour
        DebugObjectAnalysisStateButtons stateButtons;

        // Store object components section behaviour
        DebugObjectAnalysisComponentsSection componentsSection;

        // Store object children section behaviour
        DebugObjectAnalysisChildrenSection childrenSection;

        // Store reference to component analysis panel
        DebugComponentAnalysis componentAnalyser;

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

            // Get tag and layer text
            tagText = transform.Find("TagTitle").GetComponent<Text>();
            layerText = transform.Find("LayerTitle").GetComponent<Text>();

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

        public void AssignComponentAnalyser(DebugComponentAnalysis _componentAnalyser)
        {
            // Assign component analysis
            componentAnalyser = _componentAnalyser;
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
                titleText.text = "Object Analysis";

                // Set tag and layer text
                tagText.text = "Tag: None";
                layerText.text = "Layer: None";

                // Set as not analysing an object
                analysing = false;
            }

            // Object exists
            else
            {
                // Set panel title
                titleText.text = $"Analysing '{analysedObject.name}'";

                // Set tag and layer text
                tagText.text = $"Tag: {analysedObject.tag}";
                layerText.text = $"Layer: {LayerMask.LayerToName(analysedObject.layer)}";

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

        public void AnalyseComponent(Component _component)
        {
            // Open component analysis panel
            componentAnalyser.Open();

            // Maximise component analysis panel
            componentAnalyser.Maximise();

            // Analyse component
            componentAnalyser.AnalyseComponent(_component);
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

    internal class DebugComponentAnalysis : DebugPanel
    {
        // Store reference to object analysis behaviour
        DebugObjectAnalysis objectAnalysis;

        // Store title reference
        Text titleText;

        // Store namespace text reference
        Text namespaceText;

        // Store owner text reference
        Text ownerText;

        // Store component state buttons behaviour
        DebugComponentAnalysisStateButtons stateButtons;

        // Store component methods section behaviour
        DebugComponentAnalysisMethodsSection methodsSection;

        // Store component attributes section behaviour
        DebugComponentAnalysisAttributesSection attributesSection;

        // Store current component that is being analysed
        Component analysedComponent;

        // Store if currently analysing a component
        bool analysing = false;

        public override void Awake()
        {
            // Call base awake
            base.Awake();

            // Get title text
            titleText = transform.Find("PanelTitle").GetComponent<Text>();

            // Get namespace text
            namespaceText = transform.Find("NamespaceTitle").GetComponent<Text>();

            // Get owner text
            ownerText = transform.Find("OwnerTitle").GetComponent<Text>();

            // Add state buttons behaviour
            stateButtons = gameObject.AddComponent<DebugComponentAnalysisStateButtons>();
            stateButtons.Init(this);

            // Add components section behaviour
            methodsSection = transform.Find("MethodsSection").gameObject.AddComponent<DebugComponentAnalysisMethodsSection>();

            // Add children section behaviour
            attributesSection = transform.Find("AttributesSection").gameObject.AddComponent<DebugComponentAnalysisAttributesSection>();

            // Set to analysing nothing by default
            AnalyseComponent(null);
        }

        private void FixedUpdate()
        {
            // Check if analysing but analysed component reference is null
            if (analysing && analysedComponent == null)
            {
                // Stop analysing
                AnalyseComponent();

                // Refresh analysed object
                objectAnalysis.RefreshAnalysedObject();
            }
        }

        public void AssignObjectAnalyser(DebugObjectAnalysis _objectAnalysis)
        {
            // Assign object analysis
            objectAnalysis = _objectAnalysis;
        }

        public override void OnMinimise()
        {
            // Call base close function
            base.OnMinimise();

            // Stop analysing component
            AnalyseComponent();
        }

        public override void OnMaximise()
        {
            // Call base close function
            base.OnMaximise();

            // Stop analysing component
            AnalyseComponent();
        }

        public void AnalyseComponent(Component _component = null)
        {
            // Check if panel is minimised
            if (minimised)
            {
                // Don't analyse object
                _component = null;
            }

            // Assign analysed object
            analysedComponent = _component;

            // Check if null object
            if (analysedComponent == null)
            {
                // Set panel title
                titleText.text = "Component Analysis";

                // Set namespace text
                namespaceText.text = "Namespace: None";

                // Set owner text
                ownerText.text = "Owner: None";

                // Set as not analysing a component
                analysing = false;
            }

            // Component exists
            else
            {
                // Get component name args
                string[] nameArgs = analysedComponent.GetType().ToString().Split('.');

                // Set panel title
                titleText.text = $"Analysing {nameArgs[nameArgs.Length - 1]}";

                // Set namespace text
                namespaceText.text = nameArgs.Length > 1 ? $"Namespace: {string.Join('.', nameArgs.Take(nameArgs.Length - 1))}" : "Namespace: None";

                // Set owner text
                ownerText.text = $"Owner: '{analysedComponent.gameObject.name}'";

                // Set as analysing a component
                analysing = true;
            }

            // Set analysed component for state buttons
            stateButtons.AnalyseComponent(analysedComponent);

            // Set analysed component for methods section
            methodsSection.AnalyseComponent(analysedComponent);

            // Set analysed component for attributes section
            attributesSection.AnalyseComponent(analysedComponent);
        }

        public void RefreshAnalysedObject()
        {
            // Refresh analysed object
            objectAnalysis.RefreshAnalysedObject();
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

    internal class DebugComponentAnalysisStateButtons : MonoBehaviour
    {
        // Store reference to debug component analysis behaviour
        DebugComponentAnalysis analysisBehaviour;

        // Store reference to buttons
        Button enableButton;
        Button disableButton;
        Button deleteButton;

        // Store current component that is being analysed
        Component analysedComponent;

        public void Init(DebugComponentAnalysis _analysisBehaviour)
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

            // Update buttons
            UpdateButtons();
        }

        public void AnalyseComponent(Component _component)
        {
            // Assign analysed component
            analysedComponent = _component;

            // Update buttons
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            // Check if component is null
            if (analysedComponent == null)
            {
                // Disable all buttons
                enableButton.gameObject.SetActive(false);
                disableButton.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(false);
                return;
            }

            // Check if transform
            Transform transformCast = analysedComponent as Transform;
            if (transformCast != null)
            {
                // Disable all buttons
                enableButton.gameObject.SetActive(false);
                disableButton.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(false);
                return;
            }

            // Check if unity engine behaviour
            UnityEngine.Behaviour behaviourCast = analysedComponent as UnityEngine.Behaviour;
            if (behaviourCast != null)
            {
                // Enable all buttons
                enableButton.gameObject.SetActive(true);
                disableButton.gameObject.SetActive(true);
                deleteButton.gameObject.SetActive(true);
                return;
            }

            // Otherwise enable only delete button
            enableButton.gameObject.SetActive(false);
            disableButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(true);
        }

        private void OnEnablePressed()
        {
            // Check if no object being analysed
            if (analysedComponent == null) return;

            // Check if unity engine behaviour
            UnityEngine.Behaviour behaviourCast = analysedComponent as UnityEngine.Behaviour;
            if (behaviourCast != null)
            {
                // Enable component
                behaviourCast.enabled = true;
            }
        }

        private void OnDisablePressed()
        {
            // Check if no object being analysed
            if (analysedComponent == null) return;

            // Check if unity engine behaviour
            UnityEngine.Behaviour behaviourCast = analysedComponent as UnityEngine.Behaviour;
            if (behaviourCast != null)
            {
                // Disable component
                behaviourCast.enabled = false;
            }
        }

        private void OnDeletePressed()
        {
            // Check if no object being analysed
            if (analysedComponent == null) return;

            // Check if transform
            Transform transformCast = analysedComponent as Transform;
            if (transformCast != null)
            {
                // Cannot delete transform
                return;
            }

            // Destroy component
            Destroy(analysedComponent);

            // Analyse null object
            analysisBehaviour.RefreshAnalysedObject();
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

        public void AnalyseComponent(Component _component)
        {
            // Analyse component
            analysisBehaviour.AnalyseComponent(_component);
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

                // Get name args
                string[] nameArgs = component.GetType().ToString().Split('.');

                // Set entry title
                entry.SetTitle(nameArgs[nameArgs.Length - 1]);
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

        // Store entry object parent
        Transform parentTransform = null;

        // Store reference to entry titles
        Text activeTitle;
        Text inactiveTitle;

        // Store reference to buttons
        Button selectButton;
        Button enableButton;
        Button disableButton;
        Button deleteButton;

        // Store if entry object is enabled
        bool objectEnabled = true;

        public void Init(DebugObjectAnalysisScrollSection _scrollSection, Object _entryObject)
        {
            // Assign scroll section
            scrollSection = _scrollSection;

            // Assign entry object
            entryObject = _entryObject;

            // Assign parent transform
            parentTransform = GetParentTransform();

            // Get titles
            activeTitle = transform.Find("ActiveTitle").GetComponent<Text>();
            inactiveTitle = transform.Find("InactiveTitle").GetComponent<Text>();

            // Get buttons
            selectButton = transform.Find("SelectButton").GetComponent<Button>();
            enableButton = transform.Find("EnableButton").GetComponent<Button>();
            disableButton = transform.Find("DisableButton").GetComponent<Button>();
            deleteButton = transform.Find("DeleteButton").GetComponent<Button>();

            // Add button behaviours
            selectButton.onClick.AddListener(OnSelectPressed);
            enableButton.onClick.AddListener(OnEnablePressed);
            disableButton.onClick.AddListener(OnDisablePressed);
            deleteButton.onClick.AddListener(OnDeletePressed);

            // Update Titles
            UpdateTitles();

            // Update buttons
            UpdateButtons();
        }

        private void FixedUpdate()
        {
            // Check if entry object is null or if parent transform has changed
            if (entryObject == null || parentTransform != GetParentTransform())
            {
                // Refresh analysed object
                scrollSection.RefreshAnalysedObject();
                return;
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
                // Enable all buttons
                selectButton.gameObject.SetActive(true);
                enableButton.gameObject.SetActive(true);
                disableButton.gameObject.SetActive(true);
                deleteButton.gameObject.SetActive(true);
                return;
            }

            // Check if unity engine behaviour
            UnityEngine.Behaviour behaviourCast = entryObject as UnityEngine.Behaviour;
            if (behaviourCast != null)
            {
                // Enable all buttons
                selectButton.gameObject.SetActive(true);
                enableButton.gameObject.SetActive(true);
                disableButton.gameObject.SetActive(true);
                deleteButton.gameObject.SetActive(true);
                return;
            }

            // Check if transform
            Transform transformCast = entryObject as Transform;
            if (transformCast != null)
            {
                // Only enable select button
                selectButton.gameObject.SetActive(true);
                enableButton.gameObject.SetActive(false);
                disableButton.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(false);
                return;
            }

            // Check if component
            Component componentCast = entryObject as Component;
            if (componentCast != null)
            {
                // Only enable select and delete buttons
                selectButton.gameObject.SetActive(true);
                enableButton.gameObject.SetActive(false);
                disableButton.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(true);
                return;
            }

            // Disable all buttons
            selectButton.gameObject.SetActive(false);
            enableButton.gameObject.SetActive(false);
            disableButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
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

            // Check if component
            Component componentCast = entryObject as Component;
            if (componentCast != null)
            {
                // Analyse component
                scrollSection.AnalyseComponent(componentCast);

                return;
            }
        }

        private void OnEnablePressed()
        {
            // Check if game object
            GameObject gameObjectCast = entryObject as GameObject;
            if (gameObjectCast != null)
            {
                // Enable game object
                gameObjectCast.SetActive(true);
                return;
            }

            // Check if unity engine behaviour
            UnityEngine.Behaviour behaviourCast = entryObject as UnityEngine.Behaviour;
            if (behaviourCast != null)
            {
                // Enable behaviour
                behaviourCast.enabled = true;
                return;
            }
        }

        private void OnDisablePressed()
        {
            // Check if game object
            GameObject gameObjectCast = entryObject as GameObject;
            if (gameObjectCast != null)
            {
                // Disable game object
                gameObjectCast.SetActive(false);
                return;
            }

            // Check if unity engine behaviour
            UnityEngine.Behaviour behaviourCast = entryObject as UnityEngine.Behaviour;
            if (behaviourCast != null)
            {
                // Disable behaviour
                behaviourCast.enabled = false;
                return;
            }
        }

        private void OnDeletePressed()
        {
            // Check if transform
            Transform transformCast = entryObject as Transform;
            if (transformCast != null)
            {
                // Cannot delete transform
                return;
            }

            // Check if game object
            GameObject gameObjectCast = entryObject as GameObject;
            if (gameObjectCast != null)
            {
                // Delete game object
                Destroy(gameObjectCast);

                // Refresh analysed object
                scrollSection.RefreshAnalysedObject();

                return;
            }

            // Check if component
            Component componentCast = entryObject as Component;
            if (componentCast != null)
            {
                // Delete component
                Destroy(componentCast);

                // Refresh analysed object
                scrollSection.RefreshAnalysedObject();

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

        private Transform GetParentTransform()
        {
            // Check if game object
            GameObject gameObjectCast = entryObject as GameObject;
            if (gameObjectCast != null)
            {
                // Return parent transform of game object
                return gameObjectCast.transform.parent;
            }

            // Otherwise assume no parent transform
            return null;
        }

        public void SetTitle(string _title)
        {
            // Set titles
            activeTitle.text = _title;
            inactiveTitle.text = _title;
        }
    }

    internal class DebugComponentAnalysisScrollSection : MonoBehaviour
    {
        // Store scroll entry prefab
        GameObject scrollEntryPrefab;

        // Store reference to scroll rect
        ScrollRect scrollRect;

        // Store reference to scroll panel transform
        Transform scrollPanel;

        // Store reference to title
        protected Text title;

        // Store list of current scroll entries
        List<DebugComponentAnalysisScrollEntry> scrollEntries = new List<DebugComponentAnalysisScrollEntry>();

        private void Awake()
        {
            // Get scroll entry prefab
            scrollEntryPrefab = Assets.GetObject("DebugComponentAnalysisEntry");

            // Get title
            title = transform.Find("Title").GetComponent<Text>();

            // Get scroll rect
            scrollRect = transform.Find("ScrollMenu").gameObject.GetComponent<ScrollRect>();

            // Get scroll panel transform
            scrollPanel = transform.Find("ScrollMenu").Find("ScrollPanel");
        }

        public virtual void AnalyseComponent(Component _component)
        {
            // Destroy existing scroll entries
            DestroyScrollEntries();
        }
        
        private void DestroyScrollEntries()
        {
            // Cycle through scroll entries
            foreach (DebugComponentAnalysisScrollEntry entry in scrollEntries)
            {
                // Destroy entry
                Destroy(entry.gameObject);
            }

            // Clear scroll entries list
            scrollEntries.Clear();
        }

        protected DebugComponentAnalysisScrollEntry CreateScrollEntry(Component _component, MemberInfo _memberInfo)
        {
            // Create new scroll entry
            GameObject scrollEntry = Instantiate(scrollEntryPrefab);
            scrollEntry.transform.SetParent(scrollPanel, false);

            // Add scroll entry behaviour
            DebugComponentAnalysisScrollEntry behaviour = scrollEntry.AddComponent<DebugComponentAnalysisScrollEntry>();
            behaviour.Init(this, _component, _memberInfo);

            // Add to scroll entries list
            scrollEntries.Add(behaviour);

            // Reset position of scroll rect
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition = 1.0f;

            // Return newly created scroll entry
            return behaviour;
        }
    }

    internal class DebugComponentAnalysisMethodsSection : DebugComponentAnalysisScrollSection
    {
        public override void AnalyseComponent(Component _component)
        {
            // Call base method
            base.AnalyseComponent(_component);

            // Check for null component
            if (_component == null)
            {
                // Set title
                title.text = "Methods:";

                return;
            }

            // Get all methods
            MethodInfo[] methods = _component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Initialise filtered methods list
            List<MethodInfo> filteredMethods = new List<MethodInfo>();

            // Cycle through methods
            foreach (MethodInfo method in methods)
            {
                // Exclude auto-generated property accessors, event methods, lambdas and Unity internal methods
                if (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") && !method.Name.StartsWith("add_") && !method.Name.StartsWith("remove_") && !(method.Name.StartsWith("<") && method.Name.Contains("g__")) && !method.Name.StartsWith("internal_") && !method.Name.StartsWith("Internal_"))
                {
                    // Add to filtered methods list
                    filteredMethods.Add(method);
                }
            }

            // Set title
            title.text = $"Methods ({filteredMethods.Count}):";

            // Cycle through methods
            foreach (MethodInfo method in filteredMethods)
            {
                // Create scroll entry
                DebugComponentAnalysisScrollEntry entry = CreateScrollEntry(_component, method);

                // Set entry title
                entry.SetTitle($"{method.Name} [{method.GetParameters().Length}]");
            }
        }
    }

    internal class DebugComponentAnalysisAttributesSection : DebugComponentAnalysisScrollSection
    {
        public override void AnalyseComponent(Component _component)
        {
            // Call base method
            base.AnalyseComponent(_component);

            // Check for null object
            if (_component == null)
            {
                // Set title
                title.text = "Attributes:";

                return;
            }

            // Get properties
            PropertyInfo[] properties = _component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Cycle through properties
            foreach (PropertyInfo property in properties)
            {
                // Create scroll entry
                DebugComponentAnalysisScrollEntry entry = CreateScrollEntry(_component, property);

                // Set entry title
                entry.SetTitle(property.Name);
            }

            // Get fields
            FieldInfo[] fields = _component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Initialise filtered fields list
            List<FieldInfo> filteredFields = new List<FieldInfo>();

            // Cycle through fields
            foreach (FieldInfo field in fields)
            {
                // Exclude auto-generated backing fields
                if (!field.Name.EndsWith("k__BackingField"))
                {
                    // Add to filtered fields list
                    filteredFields.Add(field);
                }
            }

            // Cycle through fields
            foreach (FieldInfo field in filteredFields)
            {
                // Create scroll entry
                DebugComponentAnalysisScrollEntry entry = CreateScrollEntry(_component, field);

                // Set entry title
                entry.SetTitle(field.Name);
            }

            // Set title
            title.text = $"Attributes ({properties.Length + filteredFields.Count}):";
        }
    }

    internal class DebugComponentAnalysisScrollEntry : MonoBehaviour
    {
        // Store reference to scroll section
        DebugComponentAnalysisScrollSection scrollSection;

        // Store component reference
        Component component;

        // Store member info
        MemberInfo entryMemberInfo;

        // Store reference to entry titles
        Text publicTitle;
        Text privateTitle;

        // Store reference to buttons
        Button callButton;

        // Store reference to value holders
        Text displayValue;

        // Store previously sampled member value
        object memberValue;

        public void Init(DebugComponentAnalysisScrollSection _scrollSection, Component _component, MemberInfo _entryMemberInfo)
        {
            // Assign scroll section
            scrollSection = _scrollSection;

            // Assign component
            component = _component;

            // Assign member info
            entryMemberInfo = _entryMemberInfo;

            // Get titles
            publicTitle = transform.Find("PublicTitle").GetComponent<Text>();
            privateTitle = transform.Find("PrivateTitle").GetComponent<Text>();

            // Get buttons
            callButton = transform.Find("CallButton").GetComponent<Button>();

            // Get value holders
            displayValue = transform.Find("DisplayValue").Find("Text").GetComponent<Text>();

            // Add button behaviours
            callButton.onClick.AddListener(OnCallPressed);

            // Update Titles
            UpdateTitles();

            // Update buttons
            UpdateButtons();

            // Update value holders
            UpdateValueHolders();

            // Update display value
            UpdateDisplayValue();
        }

        private void FixedUpdate()
        {
            // Update display value
            UpdateDisplayValue();
        }

        private void UpdateDisplayValue()
        {
            // Get member value
            object newValue = GetValue();

            // Check if value has changed
            if (newValue != memberValue)
            {
                // Update member value
                memberValue = newValue;

                // Update display value
                displayValue.text = memberValue.ToString();
            }
        }

        private void UpdateTitles()
        {
            // Check if method info
            MethodInfo methodInfoCast = entryMemberInfo as MethodInfo;
            if (methodInfoCast != null)
            {
                // Select correct title
                publicTitle.gameObject.SetActive(methodInfoCast.IsPublic);
                privateTitle.gameObject.SetActive(!methodInfoCast.IsPublic);
                return;
            }

            // Check if property info
            PropertyInfo propertyInfoCast = entryMemberInfo as PropertyInfo;
            if (propertyInfoCast != null)
            {
                // Check if public
                MethodInfo getMethod = propertyInfoCast.GetGetMethod(true);
                MethodInfo setMethod = propertyInfoCast.GetSetMethod(true);
                bool isPublic = (getMethod != null && getMethod.IsPublic) || (setMethod != null && setMethod.IsPublic);

                // Select correct title
                publicTitle.gameObject.SetActive(isPublic);
                privateTitle.gameObject.SetActive(!isPublic);
                return;
            }

            // Check if field info
            FieldInfo fieldInfoCast = entryMemberInfo as FieldInfo;
            if (fieldInfoCast != null)
            {
                // Select correct title
                publicTitle.gameObject.SetActive(fieldInfoCast.IsPublic);
                privateTitle.gameObject.SetActive(!fieldInfoCast.IsPublic);
                return;
            }

            // Assume private
            publicTitle.gameObject.SetActive(false);
            privateTitle.gameObject.SetActive(true);
        }

        private void UpdateButtons()
        {
            // Check if method info
            MethodInfo methodInfoCast = entryMemberInfo as MethodInfo;
            if (methodInfoCast != null)
            {
                // Check how many required parameters the method has
                int parameterCount = methodInfoCast.GetParameters().Count(p => !p.IsOptional);

                // If method doesn't require parameters allow calls
                if (parameterCount == 0)
                {
                    // Show call button
                    callButton.gameObject.SetActive(true);
                    return;
                }

                // Hide call button if method requires parameters
                callButton.gameObject.SetActive(false);
                return;
            }

            // Assume that this member info cannot be called
            callButton.gameObject.SetActive(false);
            return;
        }

        private void UpdateValueHolders()
        {
            // Check if property info
            PropertyInfo propertyInfoCast = entryMemberInfo as PropertyInfo;
            if (propertyInfoCast != null)
            {
                // Show display value
                displayValue.transform.parent.gameObject.SetActive(true);
                return;
            }

            // Check if field info
            FieldInfo fieldInfoCast = entryMemberInfo as FieldInfo;
            if (fieldInfoCast != null)
            {
                // Show display value
                displayValue.transform.parent.gameObject.SetActive(true);
                return;
            }

            // Otherwise don't show display value
            displayValue.transform.parent.gameObject.SetActive(false);
            return;
        }

        private void OnCallPressed()
        {
            // Check if method info
            MethodInfo methodInfoCast = entryMemberInfo as MethodInfo;
            if (methodInfoCast != null)
            {
                // Invoke method
                methodInfoCast.Invoke(component, new object[methodInfoCast.GetParameters().Length]);
            }
        }

        private object GetValue()
        {
            // Check if property info
            PropertyInfo propertyInfoCast = entryMemberInfo as PropertyInfo;
            if (propertyInfoCast != null)
            {
                // Return property value
                return propertyInfoCast.GetValue(component);
            }

            // Check if field info
            FieldInfo fieldInfoCast = entryMemberInfo as FieldInfo;
            if (fieldInfoCast != null)
            {
                // Return field value
                return fieldInfoCast.GetValue(component);
            }

            // Otherwise assume null
            return null;
        }

        public void SetTitle(string _title)
        {
            // Set titles
            publicTitle.text = _title;
            privateTitle.text = _title;
        }
    }
}
