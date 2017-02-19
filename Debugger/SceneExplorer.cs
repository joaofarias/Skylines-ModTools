using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using ModTools.Explorer;
using UnityEngine;

namespace ModTools
{

	public class SceneExplorer : GUIWindow
    {
        public Dictionary<GameObject, bool> sceneRoots = new Dictionary<GameObject, bool>();
        private string findGameObjectFilter = "";
        private string findObjectTypeFilter = "";
        private string searchDisplayString = "";

        private GUIArea headerArea;
        private GUIArea sceneTreeArea;
        private GUIArea componentArea;

        private Vector2 sceneTreeScrollPosition = Vector2.zero;
        private Vector2 componentScrollPosition = Vector2.zero;
        private SceneExplorerState state;


        private float windowTopMargin = 16.0f;
        private float windowBottomMargin = 8.0f;

        private float headerHeightCompact = 1.65f;
        private float headerHeightExpanded = 17.0f;
        private bool headerExpanded = false;

        private float sceneTreeWidth = 320.0f;



        public SceneExplorer()
            : base("Scene Explorer", new Rect(128, 440, 800, 500), skin)
        {
            onDraw = DrawWindow;
            onException = ExceptionHandler;
            onUnityGUI = GUIComboBox.DrawGUI;

            headerArea = new GUIArea(this);
            sceneTreeArea = new GUIArea(this);
            componentArea = new GUIArea(this);
            state = new SceneExplorerState();

            RecalculateAreas();
        }

        public void RecalculateAreas()
        {
            headerArea.absolutePosition.y = windowTopMargin;
            headerArea.relativeSize.x = 1.0f;

            if (rect.width < (float)Screen.width / 4.0f && state.currentRefChain != null)
            {
                sceneTreeArea.relativeSize = Vector2.zero;
                sceneTreeArea.relativeSize = Vector2.zero;

                componentArea.absolutePosition.x = 0.0f;
                componentArea.relativeSize.x = 1.0f;
                componentArea.relativeSize.y = 1.0f;
                componentArea.absoluteSize.x = 0.0f;
            }
            else
            {
                sceneTreeArea.relativeSize.y = 1.0f;
                sceneTreeArea.absoluteSize.x = sceneTreeWidth;

                componentArea.absolutePosition.x = sceneTreeWidth;
                componentArea.relativeSize.x = 1.0f;
                componentArea.relativeSize.y = 1.0f;
                componentArea.absoluteSize.x = -sceneTreeWidth;
            }

            float headerHeight = (headerExpanded ? headerHeightExpanded : headerHeightCompact);
            headerHeight *= ModTools.Instance.config.fontSize;
            headerHeight += 32.0f;

            headerArea.absoluteSize.y = headerHeight - windowTopMargin;
            sceneTreeArea.absolutePosition.y = headerHeight - windowTopMargin;
            sceneTreeArea.absoluteSize.y = -(headerHeight - windowTopMargin) - windowBottomMargin;
            componentArea.absolutePosition.y = headerHeight - windowTopMargin;
            componentArea.absoluteSize.y = -(headerHeight - windowTopMargin) - windowBottomMargin;
        }

        void ExceptionHandler(Exception ex)
        {
            Debug.LogException(ex);
            state = new SceneExplorerState();
            sceneRoots = GameObjectUtil.FindSceneRoots();
            TypeUtil.ClearTypeCache();
        }

        public void Refresh()
        {
            sceneRoots = GameObjectUtil.FindSceneRoots();
            TypeUtil.ClearTypeCache();
        }

        public void ExpandFromRefChain(ReferenceChain refChain)
        {
            if (refChain == null)
            {
                Log.Error("SceneExplorer: ExpandFromRefChain(): Null refChain");
                return;
            }
            if (refChain.Length == 0)
            {
                Log.Error("SceneExplorer: ExpandFromRefChain(): Invalid refChain, expected Length >= 0");
                return;
            }

            if (refChain.ChainTypes[0] != ReferenceChain.ReferenceType.GameObject)
            {
                Log.Error(String.Format("SceneExplorer: ExpandFromRefChain(): invalid chain type for element [0] - expected {0}, got {1}",
                    ReferenceChain.ReferenceType.GameObject, refChain.ChainTypes[0]));
                return;
            }

            sceneRoots.Clear();
            ClearExpanded();
            searchDisplayString = String.Format("Showing results for \"{0}\"", refChain.ToString());

            var rootGameObject = (GameObject)refChain.ChainObjects[0];
            sceneRoots.Add(rootGameObject, true);

            var expandedRefChain = new ReferenceChain().Add(rootGameObject);
            state.expandedGameObjects.Add(expandedRefChain, true);

            for (int i = 1; i < refChain.Length; i++)
            {
                switch (refChain.ChainTypes[i])
                {
                    case ReferenceChain.ReferenceType.GameObject:
                        var go = (GameObject)refChain.ChainObjects[i];
                        expandedRefChain = expandedRefChain.Add(go);
                        state.expandedGameObjects.Add(expandedRefChain, true);
                        break;
                    case ReferenceChain.ReferenceType.Component:
                        var component = (Component)refChain.ChainObjects[i];
                        expandedRefChain = expandedRefChain.Add(component);
                        state.expandedComponents.Add(expandedRefChain, true);
                        break;
                    case ReferenceChain.ReferenceType.Field:
                        var field = (FieldInfo)refChain.ChainObjects[i];
                        expandedRefChain = expandedRefChain.Add(field);
                        state.expandedObjects.Add(expandedRefChain, true);
                        break;
                    case ReferenceChain.ReferenceType.Property:
                        var property = (PropertyInfo)refChain.ChainObjects[i];
                        expandedRefChain = expandedRefChain.Add(property);
                        state.expandedObjects.Add(expandedRefChain, true);
                        break;
                    case ReferenceChain.ReferenceType.Method:
                        break;
                    case ReferenceChain.ReferenceType.EnumerableItem:
                        var index = (int)refChain.ChainObjects[i];
                        state.selectedArrayStartIndices[expandedRefChain] = index;
                        state.selectedArrayEndIndices[expandedRefChain] = index;
                        expandedRefChain = expandedRefChain.Add(index);
                        state.expandedObjects.Add(expandedRefChain, true);
                        break;
                }
            }

            state.currentRefChain = refChain.Copy();
            state.currentRefChain.IdentOffset = -state.currentRefChain.Length;
        }

        public void DrawHeader()
        {
            headerArea.Begin();

            if (headerExpanded)
            {
                DrawExpandedHeader();
            }
            else
            {
                DrawCompactHeader();
            }

            headerArea.End();
        }

        public void DrawCompactHeader()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("▼", GUILayout.ExpandWidth(false)))
            {
                headerExpanded = true;
                RecalculateAreas();
            }

            if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
            {
                Refresh();
            }

            if (GUILayout.Button("Fold all/ Clear", GUILayout.ExpandWidth(false)))
            {
                ClearExpanded();
                Refresh();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public void DrawExpandedHeader()
        {
            GUILayout.BeginHorizontal();

            GUI.contentColor = Color.green;
            GUILayout.Label("Show:", GUILayout.ExpandWidth(false));
            GUI.contentColor = Color.white;

            GUILayout.Label("Fields");
            var showFields = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowFields, "");
            if (ModTools.Instance.config.sceneExplorerShowFields != showFields)
            {
                ModTools.Instance.config.sceneExplorerShowFields = showFields;
                ModTools.Instance.SaveConfig();
            }

            GUILayout.Label("Properties");
            var showProperties = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowProperties, "");
            if (ModTools.Instance.config.sceneExplorerShowProperties != showProperties)
            {
                ModTools.Instance.config.sceneExplorerShowProperties = showProperties;
                ModTools.Instance.SaveConfig();
            }

            GUILayout.Label("Methods");
            var showMethods = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowMethods, "");
            if (ModTools.Instance.config.sceneExplorerShowMethods != showMethods)
            {
                ModTools.Instance.config.sceneExplorerShowMethods = showMethods;
                ModTools.Instance.SaveConfig();
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Configure font & colors", GUILayout.ExpandWidth(false)))
            {
                ModTools.Instance.sceneExplorerColorConfig.visible = true;
                ModTools.Instance.sceneExplorerColorConfig.rect.position = rect.position + new Vector2(32.0f, 32.0f);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Show field/ property modifiers:", GUILayout.ExpandWidth(false));
            var showModifiers = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowModifiers, "");
            if (showModifiers != ModTools.Instance.config.sceneExplorerShowModifiers)
            {
                ModTools.Instance.config.sceneExplorerShowModifiers = showModifiers;
                ModTools.Instance.SaveConfig();
            }

            GUI.contentColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Show inherited members:", GUILayout.ExpandWidth(false));
            var showInheritedMembers = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowInheritedMembers, "");
            if (showInheritedMembers != ModTools.Instance.config.sceneExplorerShowInheritedMembers)
            {
                ModTools.Instance.config.sceneExplorerShowInheritedMembers = showInheritedMembers;
                ModTools.Instance.SaveConfig();
                TypeUtil.ClearTypeCache();
            }

            GUI.contentColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Evaluate properties automatically:", GUILayout.ExpandWidth(false));
            var evaluatePropertiesAutomatically = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerEvaluatePropertiesAutomatically, "");
            if (evaluatePropertiesAutomatically != ModTools.Instance.config.sceneExplorerEvaluatePropertiesAutomatically)
            {
                ModTools.Instance.config.sceneExplorerEvaluatePropertiesAutomatically = evaluatePropertiesAutomatically;
                ModTools.Instance.SaveConfig();
            }

            GUI.contentColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Sort alphabetically:", GUILayout.ExpandWidth(false));
            GUI.contentColor = Color.white;
            var sortAlphabetically = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerSortAlphabetically, "");
            if (sortAlphabetically != ModTools.Instance.config.sceneExplorerSortAlphabetically)
            {
                ModTools.Instance.config.sceneExplorerSortAlphabetically = sortAlphabetically;
                ModTools.Instance.SaveConfig();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            DrawFindGameObjectPanel();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("▲", GUILayout.ExpandWidth(false)))
            {
                headerExpanded = false;
                RecalculateAreas();
            }

            if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
            {
                Refresh();
            }

            if (GUILayout.Button("Fold all/ Clear", GUILayout.ExpandWidth(false)))
            {
                ClearExpanded();
                Refresh();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DrawFindGameObjectPanel()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("GameObject.Find");
            findGameObjectFilter = GUILayout.TextField(findGameObjectFilter, GUILayout.Width(256));

            if (findGameObjectFilter.Trim().Length == 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Find"))
            {
                ClearExpanded();
                var go = GameObject.Find(findGameObjectFilter.Trim());
                if (go != null)
                {
                    sceneRoots.Clear();
                    state.expandedGameObjects.Add(new ReferenceChain().Add(go), true);
                    sceneRoots.Add(go, true);
                    sceneTreeScrollPosition = Vector2.zero;
                    searchDisplayString = String.Format("Showing results for GameObject.Find(\"{0}\")", findGameObjectFilter);
                }
            }

            if (GUILayout.Button("Reset"))
            {
                ClearExpanded();
                sceneRoots = GameObjectUtil.FindSceneRoots();
                sceneTreeScrollPosition = Vector2.zero;
                searchDisplayString = "";
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("GameObject.FindObjectsOfType");
            findObjectTypeFilter = GUILayout.TextField(findObjectTypeFilter, GUILayout.Width(256));

            if (findObjectTypeFilter.Trim().Length == 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Find"))
            {
                var gameObjects = GameObjectUtil.FindComponentsOfType(findObjectTypeFilter.Trim());

                sceneRoots.Clear();
                foreach (var item in gameObjects)
                {
                    ClearExpanded();
                    state.expandedGameObjects.Add(new ReferenceChain().Add(item.Key), true);
                    if (gameObjects.Count == 1)
                    {
                        state.expandedComponents.Add(new ReferenceChain().Add(item.Key).Add(item.Value), true);
                    }
                    sceneRoots.Add(item.Key, true);
                    sceneTreeScrollPosition = Vector2.zero;
                    searchDisplayString = String.Format("Showing results for GameObject.FindObjectsOfType({0})", findObjectTypeFilter);
                }
            }

            if (GUILayout.Button("Reset"))
            {
                ClearExpanded();
                sceneRoots = GameObjectUtil.FindSceneRoots();
                sceneTreeScrollPosition = Vector2.zero;
                searchDisplayString = "";
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public void DrawSceneTree()
        {
            sceneTreeArea.Begin();

            if (searchDisplayString != "")
            {
                GUI.contentColor = Color.green;
                GUILayout.Label(searchDisplayString);
                GUI.contentColor = Color.white;
            }

            sceneTreeScrollPosition = GUILayout.BeginScrollView(sceneTreeScrollPosition);

            var gameObjects = sceneRoots.Keys.ToArray();

            if (ModTools.Instance.config.sceneExplorerSortAlphabetically)
            {
                try
                {
                    Array.Sort(gameObjects, (o, o1) =>
                    {
                        if (o.name == null)
                        {
                            return 1;
                        }

                        if (o1.name == null)
                        {
                            return -1;
                        }

                        return o.name.CompareTo(o1.name);
                    });
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }

            foreach (var obj in gameObjects)
            {
                GUIRecursiveTree.OnSceneTreeRecursive(gameObject, state, new ReferenceChain().Add(obj), obj);
            }

            GUILayout.EndScrollView();

            sceneTreeArea.End();
        }

        public void DrawComponent()
        {
            componentArea.Begin();

            componentScrollPosition = GUILayout.BeginScrollView(componentScrollPosition);

            if (state.currentRefChain != null)
            {
                try
                {
                    GUIReflect.OnSceneTreeReflect(state, state.currentRefChain, state.currentRefChain.Evaluate());
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    state.currentRefChain = null;
                    throw;
                }
            }

            GUILayout.EndScrollView();

            componentArea.End();
        }

        public void DrawWindow()
        {
            RecalculateAreas();

            bool enterPressed = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

            if (enterPressed)
            {
                GUI.FocusControl(null);
            }

            state.preventCircularReferences.Clear();

            DrawHeader();
            DrawSceneTree();
            DrawComponent();
        }

        private void ClearExpanded()
        {
            state.expandedGameObjects.Clear();
            state.expandedComponents.Clear();
            state.expandedObjects.Clear();
            state.evaluatedProperties.Clear();
            state.selectedArrayStartIndices.Clear();
            state.selectedArrayEndIndices.Clear();
            searchDisplayString = "";
            sceneTreeScrollPosition = Vector2.zero;
            state.currentRefChain = null;
            TypeUtil.ClearTypeCache();
        }
    }

}