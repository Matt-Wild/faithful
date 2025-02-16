using IL.RoR2.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugObjectAnalysis : DebugPanel
    {
        // Store title reference
        TextMeshProUGUI titleText;

        // Store tag and layer text elements
        TextMeshProUGUI tagText;
        TextMeshProUGUI layerText;

        // Store parent section behaviour
        DebugObjectAnalysisParentSection parentSection;

        // Store object state buttons behaviour
        DebugObjectAnalysisStateButtons stateButtons;

        // Store object components section behaviour
        DebugObjectAnalysisComponentsSection componentsSection;

        // Store object children section behaviour
        DebugObjectAnalysisChildrenSection childrenSection;

        // Store object search section behaviour
        DebugObjectAnalysisSearchSection searchSection;

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
            titleText = Utils.FindChildWithTerm(transform, "PanelTitle")?.GetComponent<TextMeshProUGUI>();

            // Get tag and layer text
            tagText = Utils.FindChildWithTerm(transform, "TagTitle").GetComponent<TextMeshProUGUI>();
            layerText = Utils.FindChildWithTerm(transform, "LayerTitle").GetComponent<TextMeshProUGUI>();

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

            // Add search section behaviour
            searchSection = transform.Find("SearchSection").gameObject.AddComponent<DebugObjectAnalysisSearchSection>();
            searchSection.Init(this);

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

                // Disable analysis sections
                tagText.gameObject.SetActive(false);
                layerText.gameObject.SetActive(false);
                parentSection.gameObject.SetActive(false);
                componentsSection.gameObject.SetActive(false);
                childrenSection.gameObject.SetActive(false);

                // Enable search section if not minimised
                searchSection.gameObject.SetActive(true && !minimised);
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

                // Enable analysis sections if not minimised
                tagText.gameObject.SetActive(true && !minimised);
                layerText.gameObject.SetActive(true && !minimised);
                parentSection.gameObject.SetActive(true && !minimised);
                componentsSection.gameObject.SetActive(true && !minimised);
                childrenSection.gameObject.SetActive(true && !minimised);

                // Disable search section
                searchSection.gameObject.SetActive(false);
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

        public void AnalyseComponent(object _component, List<ComponentLookupPair> _lookupTree = null)
        {
            // Open component analysis panel
            componentAnalyser.Open();

            // Maximise component analysis panel
            componentAnalyser.Maximise();

            // Analyse component
            componentAnalyser.AnalyseComponent(_component, _lookupTree);
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
        TextMeshProUGUI titleText;

        // Store namespace text reference
        TextMeshProUGUI namespaceText;

        // Store owner text reference
        TextMeshProUGUI ownerText;

        // Store component state buttons behaviour
        DebugComponentAnalysisStateButtons stateButtons;

        // Store component methods section behaviour
        DebugComponentAnalysisMethodsSection methodsSection;

        // Store component attributes section behaviour
        DebugComponentAnalysisAttributesSection attributesSection;

        // Store current component that is being analysed
        object analysedComponent;

        // Store if currently analysing a component
        bool analysing = false;

        // Store lookup tree for value type attributes
        List<ComponentLookupPair> lookupTree = new List<ComponentLookupPair>();

        public override void Awake()
        {
            // Call base awake
            base.Awake();

            // Get title text
            titleText = Utils.FindChildWithTerm(transform, "PanelTitle")?.GetComponent<TextMeshProUGUI>();

            // Get namespace text
            namespaceText = Utils.FindChildWithTerm(transform, "NamespaceTitle").GetComponent<TextMeshProUGUI>();

            // Get owner text
            ownerText = Utils.FindChildWithTerm(transform, "OwnerTitle").GetComponent<TextMeshProUGUI>();

            // Add state buttons behaviour
            stateButtons = gameObject.AddComponent<DebugComponentAnalysisStateButtons>();
            stateButtons.Init(this);

            // Add components section behaviour
            methodsSection = transform.Find("MethodsSection").gameObject.AddComponent<DebugComponentAnalysisMethodsSection>();

            // Add children section behaviour
            attributesSection = transform.Find("AttributesSection").gameObject.AddComponent<DebugComponentAnalysisAttributesSection>();

            // Set to analysing nothing by default
            AnalyseComponent();
        }

        private void FixedUpdate()
        {
            // Check if analysing but analysed component reference is null
            if (analysing && (analysedComponent == null || ReferenceEquals(analysedComponent, null) || analysedComponent.Equals(null) || (lookupTree.Count > 0 && lookupTree[0].GetValue() == null)))
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
            
            // Initialise scroll sections
            methodsSection.Init(objectAnalysis);
            attributesSection.Init(objectAnalysis);
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

        public void AnalyseComponent(object _component = null, List<ComponentLookupPair> _lookupTree = null)
        {
            // Check for lookup tree
            if (_lookupTree != null)
            {
                // Assign lookup tree
                lookupTree = _lookupTree;
            }
            else
            {
                // Clear lookup tree
                lookupTree.Clear();
            }

            // Check if panel is minimised
            if (minimised)
            {
                // Don't analyse object
                _component = null;
            }

            // Check if unity component
            bool isComponent = _component is Component;

            // Assign analysed object
            analysedComponent = _component;

            // Check if null object
            if (analysedComponent == null || ReferenceEquals(analysedComponent, null) || analysedComponent.Equals(null))
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

                // Check if unity component
                if (isComponent)
                {
                    // Set owner text
                    ownerText.text = $"Owner: '{(analysedComponent as Component).gameObject.name}'";
                }
                
                // Not a unity component
                else
                {
                    // Check for lookup tree
                    if (_lookupTree != null && _lookupTree.Count > 0)
                    {
                        // Get owner name args
                        string[] ownerArgs = _lookupTree[_lookupTree.Count - 1].type.ToString().Split('.');

                        // Set owner text
                        ownerText.text = $"Owner: {ownerArgs[ownerArgs.Length - 1]}";
                    }

                    // No lookup tree
                    else
                    {
                        // Set owner text
                        ownerText.text = "Owner: N/A";
                    }
                }

                // Set as analysing a component
                analysing = true;
            }

            // Set analysed component for state buttons
            stateButtons.AnalyseComponent(analysedComponent);

            // Set analysed component for methods section
            methodsSection.AnalyseComponent(analysedComponent, _lookupTree);

            // Set analysed component for attributes section
            attributesSection.AnalyseComponent(analysedComponent, _lookupTree);
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
        TextMeshProUGUI title;

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
            title = Utils.FindChildWithTerm(transform, "Title").GetComponent<TextMeshProUGUI>();

            Debug.Log($"ABC: {title}");

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
        object analysedComponent;

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

        public void AnalyseComponent(object _component)
        {
            // Assign analysed component
            analysedComponent = _component;

            // Update buttons
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            // Check if component is null
            if (analysedComponent == null || ReferenceEquals(analysedComponent, null) || analysedComponent.Equals(null))
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

            // Check if unity engine object
            Object unityObjectCast = analysedComponent as Object;
            if (unityObjectCast != null)
            {
                // Enable only delete button
                enableButton.gameObject.SetActive(false);
                disableButton.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(true);
                return;
            }

            // Otherwise disable all buttons
            enableButton.gameObject.SetActive(false);
            disableButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
        }

        private void OnEnablePressed()
        {
            // Check if no object being analysed
            if (analysedComponent == null || ReferenceEquals(analysedComponent, null) || analysedComponent.Equals(null)) return;

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
            if (analysedComponent == null || ReferenceEquals(analysedComponent, null) || analysedComponent.Equals(null)) return;

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
            if (analysedComponent == null || ReferenceEquals(analysedComponent, null) || analysedComponent.Equals(null)) return;

            // Check if transform
            Transform transformCast = analysedComponent as Transform;
            if (transformCast != null)
            {
                // Cannot delete transform
                return;
            }

            // Check if unity engine object
            Object unityObjectCast = analysedComponent as Object;
            if (unityObjectCast != null)
            {
                // Destroy component
                Destroy(unityObjectCast);

                // Analyse null object
                analysisBehaviour.RefreshAnalysedObject();
            }
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
        protected TextMeshProUGUI title;

        // Store list of current scroll entries
        List<DebugObjectAnalysisScrollEntry> scrollEntries = new List<DebugObjectAnalysisScrollEntry>();

        public void Init(DebugObjectAnalysis _analysisBehaviour)
        {
            // Assign analysis behaviour
            analysisBehaviour = _analysisBehaviour;

            // Get scroll entry prefab
            scrollEntryPrefab = Assets.GetObject("DebugObjectAnalysisEntry");

            // Get title
            title = Utils.FindChildWithTerm(transform, "Title").GetComponent<TextMeshProUGUI>();
        }

        protected virtual void Awake()
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

        public void AnalyseComponent(object _component)
        {
            // Analyse component
            analysisBehaviour.AnalyseComponent(_component);
        }

        public void ChangeAnalysedObject(GameObject _newObject)
        {
            // Analyse new game object
            analysisBehaviour.AnalyseObject(_newObject);
        }

        protected void DestroyScrollEntries()
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

        protected DebugObjectAnalysisScrollEntry CreateScrollEntry(Object _entryObject, bool _searchEntry = false)
        {
            // Create new scroll entry
            GameObject scrollEntry = Instantiate(scrollEntryPrefab);
            scrollEntry.transform.SetParent(scrollPanel, false);

            // Add scroll entry behaviour
            DebugObjectAnalysisScrollEntry behaviour = scrollEntry.AddComponent<DebugObjectAnalysisScrollEntry>();
            behaviour.Init(this, _entryObject, _searchEntry);

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

    internal class DebugObjectAnalysisSearchSection : DebugObjectAnalysisScrollSection
    {
        // Store reference to search input field
        TMP_InputField inputField;

        // Store reference to search button
        Button searchButton;

        protected override void Awake()
        {
            base.Awake();

            // Get input field
            inputField = transform.Find("SearchField").GetComponent<TMP_InputField>();

            // Get search button
            searchButton = transform.Find("SearchButton").GetComponent<Button>();

            // Add input field behaviour
            inputField.onEndEdit.AddListener(OnSearchInputGiven);

            // Add search button behaviour
            searchButton.onClick.AddListener(OnSearchPressed);
        }

        private void OnSearchPressed()
        {
            // Do search behaviour with current input field text
            OnSearchInputGiven(inputField.text);
        }

        private void OnSearchInputGiven(string _inputText)
        {
            // Destroy old scroll entries
            DestroyScrollEntries();

            // Check for search term
            if (_inputText == "") return;

            // Find game objects using search term
            GameObject[] matchingObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.ToLower().Contains(_inputText.ToLower())).ToArray();

            // Cycle through matching objects
            foreach (GameObject matching in matchingObjects)
            {
                // Create scroll entry
                DebugObjectAnalysisScrollEntry entry = CreateScrollEntry(matching, true);

                // Set entry title
                entry.SetTitle($"'{matching.name}'");
            }

            // Update title text
            title.text = $"Search Results ({matchingObjects.Length}):";
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
        TextMeshProUGUI activeTitle;
        TextMeshProUGUI inactiveTitle;
        TextMeshProUGUI nullTitle;

        // Store reference to buttons
        Button selectButton;
        Button enableButton;
        Button disableButton;
        Button deleteButton;

        // Store if entry object is enabled
        bool objectEnabled = true;

        // Store if entry object exists
        bool objectExists = true;

        // Store if this entry is a search entry
        bool searchEntry = false;

        public void Init(DebugObjectAnalysisScrollSection _scrollSection, Object _entryObject, bool _searchEntry = false)
        {
            // Assign scroll section
            scrollSection = _scrollSection;

            // Assign entry object
            entryObject = _entryObject;

            // Update if search entry
            searchEntry = _searchEntry;

            // Assign parent transform
            parentTransform = GetParentTransform();

            // Get titles
            activeTitle = Utils.FindChildWithTerm(transform, "ActiveTitle").GetComponent<TextMeshProUGUI>();
            inactiveTitle = Utils.FindChildWithTerm(transform, "InactiveTitle").GetComponent<TextMeshProUGUI>();
            nullTitle = Utils.FindChildWithTerm(transform, "NullTitle").GetComponent<TextMeshProUGUI>();

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

            // Apply fonts based on GameObject name tags
            Utils.ApplyFonts(transform);

            // Update Titles
            UpdateTitles();

            // Update buttons
            UpdateButtons();
        }

        private void FixedUpdate()
        {
            // Check if search entry
            if (searchEntry)
            {
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

                // Check if entry object still exists
                bool currentExists = !(entryObject == null || ReferenceEquals(entryObject, null) || entryObject.Equals(null));

                // Check if difference from stored state
                if (currentExists != objectExists)
                {
                    // Update object exists state
                    objectExists = currentExists;

                    // Update Titles
                    UpdateTitles();

                    // Update buttons
                    UpdateButtons();
                }
            }

            // Not a search entry
            else
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
        }

        private void UpdateTitles()
        {
            // Update which title is enabled
            activeTitle.gameObject.SetActive(objectExists && objectEnabled);
            inactiveTitle.gameObject.SetActive(objectExists && !objectEnabled);
            nullTitle.gameObject.SetActive(!objectExists);
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

            // Otherwise check if null
            return !(entryObject == null || ReferenceEquals(entryObject, null) || entryObject.Equals(null));
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
            nullTitle.text = _title;
        }
    }

    internal class DebugComponentAnalysisScrollSection : MonoBehaviour
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
        protected TextMeshProUGUI title;

        // Store list of current scroll entries
        List<DebugComponentAnalysisScrollEntry> scrollEntries = new List<DebugComponentAnalysisScrollEntry>();

        public void Init(DebugObjectAnalysis _analysisBehaviour)
        {
            // Assign analysis behaviour
            analysisBehaviour = _analysisBehaviour;
        }

        private void Awake()
        {
            // Get scroll entry prefab
            scrollEntryPrefab = Assets.GetObject("DebugComponentAnalysisEntry");

            // Get title
            title = Utils.FindChildWithTerm(transform, "Title").GetComponent<TextMeshProUGUI>();

            // Get scroll rect
            scrollRect = transform.Find("ScrollMenu").gameObject.GetComponent<ScrollRect>();

            // Get scroll panel transform
            scrollPanel = transform.Find("ScrollMenu").Find("ScrollPanel");
        }

        public virtual void AnalyseComponent(object _component, List<ComponentLookupPair> _lookupTree = null)
        {
            // Destroy existing scroll entries
            DestroyScrollEntries();
        }

        public void ChangeAnalysedComponent(object _component, List<ComponentLookupPair> _lookupTree = null)
        {
            // Ask object analysis to change analysed component
            analysisBehaviour.AnalyseComponent(_component, _lookupTree);
        }

        public void ChangeAnalysedObject(GameObject _newObject)
        {
            // Analyse new game object
            analysisBehaviour.AnalyseObject(_newObject);
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

        protected DebugComponentAnalysisScrollEntry CreateScrollEntry(object _component, MemberInfo _memberInfo, List<ComponentLookupPair> _lookupTree = null)
        {
            // Create new scroll entry
            GameObject scrollEntry = Instantiate(scrollEntryPrefab);
            scrollEntry.transform.SetParent(scrollPanel, false);

            // Add scroll entry behaviour
            DebugComponentAnalysisScrollEntry behaviour = scrollEntry.AddComponent<DebugComponentAnalysisScrollEntry>();
            behaviour.Init(this, _component, _memberInfo, _lookupTree);

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
        public override void AnalyseComponent(object _component, List<ComponentLookupPair> _lookupTree = null)
        {
            // Call base method
            base.AnalyseComponent(_component);

            // Check for null component
            if (_component == null || ReferenceEquals(_component, null) || _component.Equals(null))
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
                DebugComponentAnalysisScrollEntry entry = CreateScrollEntry(_component, method, _lookupTree);

                // Set entry title
                entry.SetTitle($"{method.Name} [{method.GetParameters().Length}]");
            }
        }
    }

    internal class DebugComponentAnalysisAttributesSection : DebugComponentAnalysisScrollSection
    {
        public override void AnalyseComponent(object _component, List<ComponentLookupPair> _lookupTree = null)
        {
            // Call base method
            base.AnalyseComponent(_component);

            // Check for null object
            if (_component == null || ReferenceEquals(_component, null) || _component.Equals(null))
            {
                // Set title
                title.text = "Attributes:";

                return;
            }

            // Get properties
            PropertyInfo[] properties = _component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Initialise filtered properties list
            List<PropertyInfo> filteredProperties = new List<PropertyInfo>();

            // Cycle through properties
            foreach (PropertyInfo property in properties)
            {
                // Exclude indexers
                if (property.GetIndexParameters().Length == 0)
                {
                    // Add to filtered properties
                    filteredProperties.Add(property);
                }
            }

            // Cycle through properties
            foreach (PropertyInfo property in filteredProperties)
            {
                // Create scroll entry
                DebugComponentAnalysisScrollEntry entry = CreateScrollEntry(_component, property, _lookupTree);

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
                DebugComponentAnalysisScrollEntry entry = CreateScrollEntry(_component, field, _lookupTree);

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
        object component;

        // Store member info
        MemberInfo entryMemberInfo;

        // Store reference to entry titles
        TextMeshProUGUI publicMethodTitle;
        TextMeshProUGUI privateMethodTitle;
        TextMeshProUGUI publicAttributeTitle;
        TextMeshProUGUI privateAttributeTitle;

        // Store reference to buttons
        Button callButton;
        Button analyseButton;
        Button displayValueButton;

        // Store reference to input fields
        TMP_InputField displayValueInput;

        // Store reference to value holders
        TextMeshProUGUI displayValue;
        TextMeshProUGUI displayValueButtonText;

        // Store previously sampled member value
        object memberValue;

        // Store lookup tree for value type attributes
        List<ComponentLookupPair> lookupTree = new List<ComponentLookupPair>();

        // Store input validation flags
        List<ValueValidations> inputValidations = new List<ValueValidations>();

        public void Init(DebugComponentAnalysisScrollSection _scrollSection, object _component, MemberInfo _entryMemberInfo, List<ComponentLookupPair> _lookupTree = null)
        {
            // Assign scroll section
            scrollSection = _scrollSection;

            // Assign component
            component = _component;

            // Assign member info
            entryMemberInfo = _entryMemberInfo;

            // Check for lookup tree
            if (_lookupTree != null)
            {
                // Assign lookup tree
                lookupTree = new List<ComponentLookupPair>(_lookupTree);
            }

            // Get titles
            publicMethodTitle = Utils.FindChildWithTerm(transform, "PublicMethodTitle").GetComponent<TextMeshProUGUI>();
            privateMethodTitle = Utils.FindChildWithTerm(transform, "PrivateMethodTitle").GetComponent<TextMeshProUGUI>();
            publicAttributeTitle = Utils.FindChildWithTerm(transform, "PublicAttributeTitle").GetComponent<TextMeshProUGUI>();
            privateAttributeTitle = Utils.FindChildWithTerm(transform, "PrivateAttributeTitle").GetComponent<TextMeshProUGUI>();

            // Get buttons
            callButton = transform.Find("CallButton").GetComponent<Button>();
            analyseButton = transform.Find("AnalyseButton").GetComponent<Button>();
            displayValueButton = transform.Find("DisplayValueButton").GetComponent<Button>();

            // Get input fields
            displayValueInput = transform.Find("DisplayValueInput").GetComponent<TMP_InputField>();

            // Get value holders
            displayValue = Utils.FindChildWithTerm(transform.Find("DisplayValue"), "Text").GetComponent<TextMeshProUGUI>();
            displayValueButtonText = Utils.FindChildWithTerm(displayValueButton.transform, "Text").GetComponent<TextMeshProUGUI>();

            // Add button behaviours
            callButton.onClick.AddListener(OnCallPressed);
            analyseButton.onClick.AddListener(OnAnalysePressed);
            displayValueButton.onClick.AddListener(OnValueButtonPressed);

            // Add input field behaviour
            displayValueInput.onEndEdit.AddListener(OnAttributeSet);
            displayValueInput.onValueChanged.AddListener(OnInputValueChanged);

            // Apply fonts based on GameObject name tags
            Utils.ApplyFonts(transform);

            // Update Titles
            UpdateTitles();

            // Update display value
            UpdateDisplayValue(true);
        }

        private void FixedUpdate()
        {
            // Update display value
            UpdateDisplayValue();
        }

        private void UpdateDisplayValue(bool _forceUpdate = false)
        {
            // Get member value
            object newValue = GetValue();

            // Check if value has changed or update is forced
            if (newValue != memberValue || _forceUpdate)
            {
                // Update member value
                memberValue = newValue;

                // Get new value string
                string value = memberValue == null ? "Null" : memberValue.ToString();

                // Update display value
                displayValue.text = displayValueButtonText.text = value;

                // Check if the user is not editting the input field
                if (!displayValueInput.isFocused)
                {
                    // Update the display value input field
                    displayValueInput.text = value;
                }

                // Update buttons
                UpdateElements();
            }
        }

        private void UpdateTitles()
        {
            // Check if method info
            MethodInfo methodInfoCast = entryMemberInfo as MethodInfo;
            if (methodInfoCast != null)
            {
                // Select correct title
                publicMethodTitle.gameObject.SetActive(methodInfoCast.IsPublic);
                privateMethodTitle.gameObject.SetActive(!methodInfoCast.IsPublic);
                publicAttributeTitle.gameObject.SetActive(false);
                privateAttributeTitle.gameObject.SetActive(false);
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
                publicMethodTitle.gameObject.SetActive(false);
                privateMethodTitle.gameObject.SetActive(false);
                publicAttributeTitle.gameObject.SetActive(isPublic);
                privateAttributeTitle.gameObject.SetActive(!isPublic);
                return;
            }

            // Check if field info
            FieldInfo fieldInfoCast = entryMemberInfo as FieldInfo;
            if (fieldInfoCast != null)
            {
                // Select correct title
                publicMethodTitle.gameObject.SetActive(false);
                privateMethodTitle.gameObject.SetActive(false);
                publicAttributeTitle.gameObject.SetActive(fieldInfoCast.IsPublic);
                privateAttributeTitle.gameObject.SetActive(!fieldInfoCast.IsPublic);
                return;
            }

            // Assume private attribute
            publicMethodTitle.gameObject.SetActive(false);
            privateMethodTitle.gameObject.SetActive(false);
            publicAttributeTitle.gameObject.SetActive(false);
            privateAttributeTitle.gameObject.SetActive(true);
        }

        private void UpdateElements()
        {
            // Clear input validations
            inputValidations.Clear();

            // Check if method info
            MethodInfo methodInfoCast = entryMemberInfo as MethodInfo;
            if (methodInfoCast != null)
            {
                // Ensure display value is hidden
                displayValue.transform.parent.gameObject.SetActive(false);

                // Hide property and field buttons
                analyseButton.gameObject.SetActive(false);
                displayValueButton.gameObject.SetActive(false);

                // Hide input field
                displayValueInput.gameObject.SetActive(false);

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

            // Show analyse button if value is not null
            analyseButton.gameObject.SetActive(memberValue != null);

            // Check if member value is writable
            if (canWrite)
            {
                // Check if member value is a bool
                if (memberValue is bool)
                {
                    // Enable only display value button
                    displayValue.transform.parent.gameObject.SetActive(false);
                    displayValueButton.gameObject.SetActive(true);
                    displayValueInput.gameObject.SetActive(false);
                    return;
                }

                // Check if member value is a signed int
                if (memberValue is sbyte || memberValue is short || memberValue is int || memberValue is long)
                {
                    // Enable only display value input
                    displayValue.transform.parent.gameObject.SetActive(false);
                    displayValueButton.gameObject.SetActive(false);
                    displayValueInput.gameObject.SetActive(true);

                    // Set input field content type
                    displayValueInput.contentType = TMP_InputField.ContentType.IntegerNumber;
                    return;
                }

                // Check if member value is an unsigned int
                if (memberValue is byte || memberValue is ushort || memberValue is uint || memberValue is ulong)
                {
                    // Enable only display value input
                    displayValue.transform.parent.gameObject.SetActive(false);
                    displayValueButton.gameObject.SetActive(false);
                    displayValueInput.gameObject.SetActive(true);

                    // Set input field content type
                    displayValueInput.contentType = TMP_InputField.ContentType.IntegerNumber;

                    // Add input validation
                    inputValidations.Add(ValueValidations.PositiveInteger);
                    return;
                }

                // Check if member value is a float
                if (memberValue is float)
                {
                    // Enable only display value input
                    displayValue.transform.parent.gameObject.SetActive(false);
                    displayValueButton.gameObject.SetActive(false);
                    displayValueInput.gameObject.SetActive(true);

                    // Set input field content type
                    displayValueInput.contentType = TMP_InputField.ContentType.DecimalNumber;
                    return;
                }

                // Check if member value is a string
                if (memberValue is string)
                {
                    // Enable only display value input
                    displayValue.transform.parent.gameObject.SetActive(false);
                    displayValueButton.gameObject.SetActive(false);
                    displayValueInput.gameObject.SetActive(true);

                    // Set input field content type
                    displayValueInput.contentType = TMP_InputField.ContentType.Standard;
                    return;
                }
            }

            // Otherwise only enable display value
            displayValue.transform.parent.gameObject.SetActive(true);
            displayValueButton.gameObject.SetActive(false);
            displayValueInput.gameObject.SetActive(false);
        }

        private void UpdateComponent()
        {
            // Check for lookup tree (exists if this is a value type and needs to be looked up)
            if (lookupTree.Count == 0) return;

            // Get first component
            object currentComponent = lookupTree[0].component;

            // Cycle through lookup tree
            foreach (ComponentLookupPair pair in lookupTree)
            {
                // Update current component
                currentComponent = pair.GetValue(currentComponent);
            }

            // Set as component
            component = currentComponent;
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

        private void OnAnalysePressed()
        {
            // Check if member value is a game object
            GameObject gameObjectCast = memberValue as GameObject;
            if (gameObjectCast != null)
            {
                // Analyse new game object
                scrollSection.ChangeAnalysedObject(gameObjectCast);
                return;
            }

            // Check if member value is a value type
            if (isValueType)
            {
                // Add to lookup tree
                lookupTree.Add(new ComponentLookupPair(component, entryMemberInfo));

                // Change analysed component
                scrollSection.ChangeAnalysedComponent(memberValue, lookupTree);
            }

            // Not a value type
            else
            {
                // Change analysed component
                scrollSection.ChangeAnalysedComponent(memberValue);
            }
        }

        private void OnValueButtonPressed()
        {
            // Check if property info
            PropertyInfo propertyInfoCast = entryMemberInfo as PropertyInfo;
            if (propertyInfoCast != null)
            {
                // Check if a bool and can write
                if (propertyInfoCast.PropertyType == typeof(bool) && canWrite)
                {
                    // Set value
                    SetValue(!System.Convert.ToBoolean(memberValue));
                }
                return;
            }

            // Check if field info
            FieldInfo fieldInfoCast = entryMemberInfo as FieldInfo;
            if (fieldInfoCast != null)
            {
                // Check if a bool and can write
                if (fieldInfoCast.FieldType == typeof(bool) && canWrite)
                {
                    // Set value
                    SetValue(!System.Convert.ToBoolean(memberValue));
                }
                return;
            }
        }

        private void OnAttributeSet(string _newValue)
        {
            // Ensure can write
            if (!canWrite) return;

            // Check if property info
            PropertyInfo propertyInfoCast = entryMemberInfo as PropertyInfo;
            if (propertyInfoCast != null)
            {
                // Attempt to convert to appropriate type
                try
                {
                    // Store reference to converted value
                    object convertedValue;

                    // Get type converter
                    System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(propertyInfoCast.PropertyType);

                    // Convert value
                    convertedValue = converter.ConvertFromInvariantString(_newValue);

                    // Set value
                    SetValue(convertedValue);
                }
                catch
                {
                    // Send warning
                    Log.Warning($"[OBJECT ANALYSIS] - Was not able to update property '{propertyInfoCast.Name}' to value '{_newValue}'.");
                }
                return;
            }

            // Check if field info
            FieldInfo fieldInfoCast = entryMemberInfo as FieldInfo;
            if (fieldInfoCast != null)
            {
                // Attempt to convert to appropriate type
                try
                {
                    // Store reference to converted value
                    object convertedValue;

                    // Get type converter
                    System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(fieldInfoCast.FieldType);

                    // Convert value
                    convertedValue = converter.ConvertFromInvariantString(_newValue);

                    // Set value
                    SetValue(convertedValue);
                }
                catch
                {
                    // Send warning
                    Log.Warning($"[OBJECT ANALYSIS] - Was not able to update field '{fieldInfoCast.Name}' to value '{_newValue}'.");
                }
                return;
            }
        }

        private void OnInputValueChanged(string _newValue)
        {
            // Check for positive integer validation
            if (inputValidations.Contains(ValueValidations.PositiveInteger))
            {
                // Remove non-digit characters and ensure it starts with a non-zero (unless input is "0").
                string validated = System.Text.RegularExpressions.Regex.Replace(_newValue, "[^0-9]", "");
                if (validated.StartsWith("0") && validated.Length > 1)
                {
                    validated = validated.TrimStart('0');
                }

                // Update input
                displayValueInput.text = validated;
            }
        }

        private object GetValue()
        {
            // Update component reference
            UpdateComponent();

            // Check if component is null
            if (component == null || ReferenceEquals(component, null) || component.Equals(null))
            {
                // Return null
                return null;
            }

            // Check if property info
            PropertyInfo propertyInfoCast = entryMemberInfo as PropertyInfo;
            if (propertyInfoCast != null)
            {
                // Ensure can read property without parameters
                if (propertyInfoCast.CanRead && propertyInfoCast.GetIndexParameters().Length == 0)
                {
                    // Return property value
                    return propertyInfoCast.GetValue(component);
                }

                // Can't read property
                return "???";
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

        public void SetValue(object _value)
        {
            // Check if can write
            if (!canWrite) return;

            // Check if property info
            if (entryMemberInfo is PropertyInfo propertyInfo)
            {
                // Set value
                propertyInfo.SetValue(component, _value);
            }

            // Check if field info
            else if (entryMemberInfo is FieldInfo fieldInfo)
            {
                // Set value
                fieldInfo.SetValue(component, _value);
            }

            // Check if value type attribute
            if (lookupTree.Count > 0)
            {
                // Apply value through lookup tree
                ApplyValue(component);
            }
        }

        private void ApplyValue(object _value)
        {
            // Cycle backwards through lookup tree
            for (int lookupTreeIndex = lookupTree.Count - 1; lookupTreeIndex >= 0; lookupTreeIndex--)
            {
                // Set value
                lookupTree[lookupTreeIndex].SetValue(_value);

                // Update value
                _value = lookupTree[lookupTreeIndex].component;
            }
        }

        public void SetTitle(string _title)
        {
            // Set titles
            publicMethodTitle.text = _title;
            privateMethodTitle.text = _title;
            publicAttributeTitle.text = _title;
            privateAttributeTitle.text = _title;
        }

        public bool canWrite
        {
            get
            {
                // Check if property info
                PropertyInfo propertyInfoCast = entryMemberInfo as PropertyInfo;
                if (propertyInfoCast != null)
                {
                    // Return if can write
                    return propertyInfoCast.CanWrite;
                }

                // Check if field info
                FieldInfo fieldInfoCast = entryMemberInfo as FieldInfo;
                if (fieldInfoCast != null)
                {
                    // Fields are writable
                    return true;
                }

                // Assume unwritable method
                return false;
            }
        }

        public bool isValueType
        {
            get
            {
                // Define member type
                System.Type memberType = null;

                // Check if property info
                if (entryMemberInfo is PropertyInfo propertyInfo)
                {
                    // Update member type
                    memberType = propertyInfo.PropertyType;
                }

                // Check if field info
                else if (entryMemberInfo is FieldInfo fieldInfo)
                {
                    // Update member type
                    memberType = fieldInfo.FieldType;
                }

                // Check if the type is a value type
                return memberType != null && memberType.IsValueType;
            }
        }
    }

    internal class ComponentLookupPair
    {
        // Store component and member info
        public object component;
        public MemberInfo memberInfo;

        public ComponentLookupPair(object _component, MemberInfo _memberInfo)
        {
            // Assign component and member info
            component = _component;
            memberInfo = _memberInfo;
        }

        public object GetValue()
        {
            // Get value using own component
            return GetValue(component);
        }

        public object GetValue(object _component)
        {
            // Check for component
            if (_component == null || ReferenceEquals(_component, null) || _component.Equals(null)) return null;

            // Check if property info
            PropertyInfo propertyInfoCast = memberInfo as PropertyInfo;
            if (propertyInfoCast != null)
            {
                // Return property value
                return propertyInfoCast.GetValue(_component);
            }

            // Check if field info
            FieldInfo fieldInfoCast = memberInfo as FieldInfo;
            if (fieldInfoCast != null)
            {
                // Return field value
                return fieldInfoCast.GetValue(_component);
            }

            // Otherwise return null
            return null;
        }

        public void SetValue(object _value)
        {
            // Check if property info
            PropertyInfo propertyInfoCast = memberInfo as PropertyInfo;
            if (propertyInfoCast != null)
            {
                // Set property value
                propertyInfoCast.SetValue(component, _value);
            }

            // Check if field info
            FieldInfo fieldInfoCast = memberInfo as FieldInfo;
            if (fieldInfoCast != null)
            {
                // Set field value
                fieldInfoCast.SetValue(component, _value);
            }
        }

        public System.Type type
        {
            get
            {
                // Check for component
                if (component == null || ReferenceEquals(component, null) || component.Equals(null)) return null;

                // No type found
                return component.GetType();
            }
        }
    }

    internal enum ValueValidations
    {
        PositiveInteger
    }
}
