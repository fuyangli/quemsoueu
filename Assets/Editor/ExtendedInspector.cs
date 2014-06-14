using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

public partial class ExtendedInspector : EditorWindow
{
	// Control stuff
	private GameObject _currentSelection;
	private int _selected;
	private int _componentCount;
	private bool _showPrivate;
	private bool _showPrivatePrevious;
	private bool _dirty;
	private bool _isPlaying;
	private readonly Dictionary<Component, List<InspectorInfo>> _components = new Dictionary<Component, List<InspectorInfo>>();
	private readonly Dictionary<Type, bool> _showFields = new Dictionary<Type, bool>();
	private readonly Dictionary<Type, bool> _enabled = new Dictionary<Type, bool>();
	private Dictionary<Type, bool> _enabledPrevious = new Dictionary<Type, bool>();
	private readonly Dictionary<InspectorInfo, bool> _enabledFoldout = new Dictionary<InspectorInfo, bool>();
	private Dictionary<InspectorInfo, bool> _enabledFoldoutPrevious = new Dictionary<InspectorInfo, bool>();
	// Misc properties
	private bool _globalPropertiesEnable = true;
	private bool _componentOptionsEnabled;
	// Vertical scroll
	private Vector2 _scrollPos;
	// Out of routine queues
	private readonly Queue<GUIText> _prefabBreakQueue = new Queue<GUIText>();
	private readonly Queue<UpdateStruct> _updateQueue = new Queue<UpdateStruct>();
	// Undo Stuff
	private Object _objectToUndo;
	private bool _listeningForGuiChanges;
	private bool _guiChanged;
	// Selection 
	private int _componentIndex;
	private int _componentIndexPrevious;
	private int _dropDownIndex;
	private int _dropDownIndexPrevious;
	private Type _previousComponentSelection;
	private bool _repickPrevious;
	// Copy Paste
	private Component _copy;
	private GameObject _copyFrom;
	private bool _cut;
	// Dictionary
	private object _key;
	private object _value;

	[MenuItem("Window/Extended Inspector/Extended Inspector")]
	static void Init()
	{
		var window = GetWindow(typeof(ExtendedInspector));
		window.autoRepaintOnSceneChange = true;
	}
	void OnInspectorUpdate()
	{
		Repaint();

		if (Application.isPlaying != _isPlaying) {
			_dirty = true;
			_isPlaying = Application.isPlaying;
		}

		// Destroy prefab queue
		foreach (var p in _prefabBreakQueue) {
			DestroyImmediate(p);
		}
		_prefabBreakQueue.Clear();

		// Dictionary add
		foreach (var update in _updateQueue) {
			update.Update();
		}
		_updateQueue.Clear();

		// Undo requirement check
		var doUndo = true;
		if (_enabled.Any(e => _enabledPrevious [e.Key] != e.Value) ||
			_enabledFoldout.Any(foldout => foldout.Value != _enabledFoldoutPrevious [foldout.Key])
			|| _dropDownIndex != _dropDownIndexPrevious
			|| _componentIndex != _componentIndexPrevious) {
			doUndo = false;
		}

		_dropDownIndexPrevious = _dropDownIndex;
		_componentIndexPrevious = _componentIndex;
		_enabledPrevious = new Dictionary<Type, bool>(_enabled);
		_enabledFoldoutPrevious = new Dictionary<InspectorInfo, bool>(_enabledFoldout);

		if (doUndo) {
			SaveUndo();
		} else {
			_listeningForGuiChanges = false;
		}

		SpecialCasesUpdate();
	}
	void OnGUI()
	{
		if (!Selection.activeGameObject) {
			return;
		}
		var components = Selection.activeGameObject.GetComponents<Component>();

		if (_dirty) {
			RebuildReflection(components);
			_dirty = false;
		}
		if ((_currentSelection != Selection.activeGameObject || _selected != Selection.gameObjects.Length) || _componentCount != components.Length) {
			_currentSelection = Selection.activeGameObject;
			_selected = Selection.gameObjects.Length;
			_componentCount = components.Length;

			_componentIndex = 0;
			_dropDownIndex = 1;

			RebuildReflection(components);
		} else {
			CheckUndo(Selection.activeGameObject);
		}

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.BeginVertical();
		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
		SetAll();
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();

		if (GUI.changed) {
			_guiChanged = true;
		}
	}
	private void RebuildReflection(IEnumerable<Component> components)
	{
		_components.Clear();
		_enabledFoldout.Clear();
		_enabledFoldoutPrevious.Clear();

		foreach (var component in components) {
			var flag = BindingFlags.Instance | BindingFlags.Public; // | BindingFlags.DeclaredOnly;
			if (_showPrivate) {
				flag = flag | BindingFlags.NonPublic;
			}

			if (component != null) {
				var properties = component.GetType().GetProperties(flag);
				var fields = component.GetType().GetFields(flag);

				var inspectorInfo = fields.Select(field => new InspectorInfo(field)).ToList();
				inspectorInfo.AddRange(properties.Select(property => new InspectorInfo(property)));

				//inspectorInfo.Sort();

				_components.Add(component, inspectorInfo);

				if (!_enabled.ContainsKey(component.GetType())) {
					var cEnabled = EISettings.DisplayMode == EISettings.DisplayModes.DropDown;
					_enabled.Add(component.GetType(), cEnabled);
					_enabledPrevious.Add(component.GetType(), cEnabled);
					var fEnabled = EISettings.ShowField == EISettings.ShowFields.Always;
					_showFields.Add(component.GetType(), fEnabled);
				}


				foreach (
                    var inspector in
                        inspectorInfo.Where(
                            inspector =>
                            inspector.InspectorType.IsArray || inspector.InspectorType.IsGenericType)) {
					_enabledFoldout.Add(inspector, false);
					_enabledFoldoutPrevious.Add(inspector, false);
				}
			} else {
				Debug.Log("This gameobject is missing a component");
			}
		}
		_repickPrevious = true;
	}
	private void SetAll()
	{
		_componentOptionsEnabled = EditorGUILayout.Foldout(_componentOptionsEnabled, "Component Control");
		if (_componentOptionsEnabled) {
			ExtendInspectorOptions();
		}
        
		// Build gameobject properties
		_globalPropertiesEnable = EditorGUILayout.Foldout(_globalPropertiesEnable, "Game Object Properties");
		if (_globalPropertiesEnable) {
			GameObjectProperties();
		}
        
		if (EISettings.DisplayMode == EISettings.DisplayModes.TitleBars) {
			TitleBars();
		} else if (EISettings.DisplayMode == EISettings.DisplayModes.DropDown) {
			DropDown();
		}
	}
	private void ExtendInspectorOptions()
	{
		var unfilteredComponents = new List<Component>(_components.Keys);
		var components = unfilteredComponents.Where(hit => !(hit is Transform)).ToList();

		var displayOptions = new List<string> { "None" };
		displayOptions.AddRange(components.Select(a => a.GetType().ToString()).ToList());
		_componentIndex = EditorGUILayout.Popup("Component", _componentIndex, displayOptions.ToArray(), EditorStyles.popup);

		Component component = null;

		if (_componentIndex != 0 &&
			_componentIndex <= components.Count) { // Just stops the error when scripts rebuild when debugging.
			component = components [_componentIndex - 1];
		}

		CopyPasteComponent(component);
		RemoveComponent(component);

		EditorGUILayout.Space();
	}
	private void CopyPasteComponent(Component component)
	{
		BeginHorizontal();
		EditorGUILayout.PrefixLabel("");
		if (GUILayout.Button("Cut", EditorStyles.miniButtonLeft)) {
			if (component != null) {
				_copy = component;
				_copyFrom = Selection.activeGameObject;
				_cut = true;
			}
		}
		if (GUILayout.Button("Copy", EditorStyles.miniButtonMid)) {
			if (component != null) {
				_copy = component;
				_copyFrom = Selection.activeGameObject;
				_cut = false;
			}
		}
		if (GUILayout.Button("Paste", EditorStyles.miniButtonRight)) {
			if (_copy != null && _copyFrom != null) {
				SaveMassUndo("Paste Component");
				// Get all selected
				foreach (var go in Selection.gameObjects) {
					var newComponent = go.AddComponent(_copy.GetType());

					foreach (var f in _copy.GetType().GetFields()) {
						f.SetValue(newComponent, f.GetValue(_copy));
					}

					if (_cut) { // Only do 1 copy if we cutting
						break;
					}
				}

				if (_cut) {
					DestroyImmediate(_copy);
					_cut = false;
				}
			}
		}
		EndHorizontal();
	}
	private void RemoveComponent(Component component)
	{
		BeginHorizontal();
		EditorGUILayout.PrefixLabel("Remove");
		if (GUILayout.Button("Selection", EditorStyles.miniButtonLeft)) {
			if (component != null) {
				SaveMassUndo("Remove Component");
				foreach (var c in
                    from obj in Selection.transforms
                    select obj.GetComponents(component.GetType())
                        into comp
                        from c in comp
                        select c) {
					DestroyImmediate(c);
				}
				_componentIndex = 0;
			}
		}
		if (GUILayout.Button("Children", EditorStyles.miniButtonMid)) {
			if (component != null) {
				SaveMassUndo("Remove Component");
				// Need to destroy current selection as well
				var destroy = Selection.activeTransform.GetComponent(component.GetType());
				DestroyImmediate(destroy);

				// Find children components
				foreach (var c in
                    from Transform obj in Selection.activeTransform
                    select obj.GetComponents(component.GetType())
                        into comp
                        from c in comp
                        select c) {
					DestroyImmediate(c);
				}


				_componentIndex = 0;
			}
		}
		if (GUILayout.Button("Both", EditorStyles.miniButtonRight)) {
			if (component != null) {
				SaveMassUndo("Remove Component");
				// Remove selection
				foreach (var c in
                    from obj in Selection.transforms
                    select obj.GetComponents(component.GetType())
                        into comp
                        from c in comp
                        select c) {
					DestroyImmediate(c);
				}

				// Find children component in all selections. Might be a little slow.
				foreach (var parent in Selection.transforms)
					foreach (var c in
                        from Transform obj in parent
                        select obj.GetComponents(component.GetType())
                            into comp
                            from c in comp
                            select c) {
						DestroyImmediate(c);
					}
				_componentIndex = 0;
			}
		}
		EndHorizontal();
	}
	private void GameObjectProperties()
	{
		var fixedLabel = new GUIStyle(EditorStyles.miniLabel)
        {
            fixedWidth = 35
        };

		BeginHorizontal();
		var beforeActive = Selection.activeGameObject.active;
		Selection.activeGameObject.active = EditorGUILayout.Toggle(Selection.activeGameObject.active,
                                                                   GUILayout.Width(16));

		var name = EditorGUILayout.TextField(Selection.activeGameObject.name);

		if (name != Selection.activeGameObject.name) {
			Selection.activeGameObject.name = name;
		}

		EndHorizontal();

		if (Selection.activeGameObject.active != beforeActive && !EditorApplication.isPlaying && 
			(Selection.gameObjects.Length > 1 || Selection.activeGameObject.transform.childCount != 0)) {
			var state = Selection.activeGameObject.active ? "activate" : "deactivate";
			//message box
			if (EditorUtility.DisplayDialog("Apply to?",
            "Do you want to " + state + " other game objects?",
            "Activate Other",
            "Only This")) {
				var option = EditorUtility.DisplayDialogComplex(
                    "Apply to?",
                    "What other Game Objects do you want to " + state + "?",
                    "Selection",
                    "Children",
                    "Both");

				switch (option) {
				// Selection
					case 0:
						SaveMassUndo("Apply to Selection");
						foreach (var selection in Selection.gameObjects) {
							selection.gameObject.active = Selection.activeGameObject.active;
						}
						break;
				// Children
					case 1:
						SaveMassUndo("Apply to Children");
						foreach (Transform child in Selection.activeTransform) {

							child.gameObject.active = Selection.activeGameObject.active;
						}
						break;

				// Both
					case 2:
						SaveMassUndo("Apply to Both");
						foreach (var selection in Selection.gameObjects) {
							selection.active = Selection.activeGameObject.active;
							foreach (Transform child in selection.transform) {
								child.gameObject.active = Selection.activeGameObject.active;
							}
						}
						break;
					default:
						Debug.LogError("Unrecognized option.");
						break;
				}
			}
		}
        
		BeginHorizontal();
		GUILayout.Label("Static", fixedLabel);
		var style = EditorStyles.toggle;
		style.fixedWidth = 16;
		style.font = EditorStyles.miniButtonMid.font;
        
		if (ToggleChangeButton(Selection.activeGameObject.isStatic, "", style)) {
			Selection.activeGameObject.isStatic = !Selection.activeGameObject.isStatic;
		}

		if (GUILayout.Button("Selection", EditorStyles.miniButtonLeft)) {
			SaveMassUndo("Apply to Selection");
			foreach (var selection in Selection.gameObjects) {
				selection.isStatic = Selection.activeGameObject.isStatic;
			}
		}
		if (GUILayout.Button("Children", EditorStyles.miniButtonMid)) {
			SaveMassUndo("Apply to Children");
			foreach (Transform child in Selection.activeTransform) {
				child.gameObject.isStatic = Selection.activeGameObject.isStatic;
			}
		}
		if (GUILayout.Button("Both", EditorStyles.miniButtonRight)) {
			SaveMassUndo("Apply to Both");
			foreach (var selection in Selection.gameObjects) {
				selection.isStatic = Selection.activeGameObject.isStatic;
				foreach (Transform child in selection.transform) {
					child.gameObject.isStatic = Selection.activeGameObject.isStatic;
				}
			}
		}
		EndHorizontal();
        
		BeginHorizontal();
		GUILayout.Label("Tag", fixedLabel);
		var newTag = string.Empty;
		if (ToggleTagField(Selection.activeGameObject.tag, EditorStyles.popup, out newTag)) {
			Selection.activeGameObject.tag = newTag;
		}

		if (GUILayout.Button("Selection", EditorStyles.miniButtonLeft)) {
			SaveMassUndo("Apply to Selection");
			foreach (var selection in Selection.gameObjects) {
				selection.tag = Selection.activeGameObject.tag;
			}
		}
		if (GUILayout.Button("Children", EditorStyles.miniButtonMid)) {
			SaveMassUndo("Apply to Children");
			foreach (Transform child in Selection.activeTransform) {
				child.gameObject.tag = Selection.activeGameObject.tag;
			}
		}
		if (GUILayout.Button("Both", EditorStyles.miniButtonRight)) {
			foreach (var selection in Selection.gameObjects) {
				SaveMassUndo("Apply to Both");
				selection.tag = Selection.activeGameObject.tag;
				foreach (Transform child in selection.transform) {
					child.gameObject.tag = Selection.activeGameObject.tag;
				}
			}
		}
		EndHorizontal();
        
		BeginHorizontal();
		GUILayout.Label("Layer", fixedLabel);

		int newLayer;
		if (ToggleLayerField(Selection.activeGameObject.layer, EditorStyles.popup, out newLayer)) {
			Selection.activeGameObject.layer = newLayer;
		}

		if (GUILayout.Button("Selection", EditorStyles.miniButtonLeft)) {
			SaveMassUndo("Apply to Selection");
			foreach (var selection in Selection.gameObjects) {
				selection.layer = Selection.activeGameObject.layer;
			}
		}
		if (GUILayout.Button("Children", EditorStyles.miniButtonMid)) {
			SaveMassUndo("Apply to Children");
			foreach (Transform child in Selection.activeTransform) {
				child.gameObject.layer = Selection.activeGameObject.layer;
			}
		}
		if (GUILayout.Button("Both", EditorStyles.miniButtonRight)) {
			SaveMassUndo("Apply to Both");
			foreach (var selection in Selection.gameObjects) {
				selection.layer = Selection.activeGameObject.layer;
				foreach (Transform child in selection.transform) {
					child.gameObject.layer = Selection.activeGameObject.layer;
				}
			}
		}
		EndHorizontal();

#if UNITY_3_5
        switch (PrefabUtility.GetPrefabType(Selection.activeObject))
#else
		switch (EditorUtility.GetPrefabType(Selection.activeObject))
#endif 
		{
			case PrefabType.PrefabInstance:
				BeginHorizontal();
				GUILayout.Label("Prefab", fixedLabel);
				if (GUILayout.Button("Select", EditorStyles.miniButtonLeft)) {
#if UNITY_3_5
                    Selection.activeObject = PrefabUtility.GetPrefabParent(Selection.activeGameObject);
#else
					Selection.activeObject = EditorUtility.GetPrefabParent(Selection.activeGameObject);
#endif
				}
				if (GUILayout.Button("Reset", EditorStyles.miniButtonMid)) {
#if UNITY_3_5
                    PrefabUtility.ResetToPrefabState(Selection.activeGameObject);
#else
					PrefabUtility.ResetToPrefabState(Selection.activeGameObject);
#endif

				}
				if (GUILayout.Button("Apply", EditorStyles.miniButtonMid)) {
#if UNITY_3_5
                    var prefab = PrefabUtility.GetPrefabParent(Selection.activeGameObject);
                    PrefabUtility.ReplacePrefab(Selection.activeGameObject, prefab);
#else
					var prefab = EditorUtility.GetPrefabParent(Selection.activeGameObject);
					EditorUtility.ReplacePrefab(Selection.activeGameObject, prefab);
#endif

				}
				if (GUILayout.Button("Break", EditorStyles.miniButtonRight)) {
					BreakPrefab(Selection.activeGameObject);
				}
				EndHorizontal();
				break;
			case PrefabType.ModelPrefabInstance:
				BeginHorizontal();
				GUILayout.Label("Prefab", fixedLabel);
				if (GUILayout.Button("Select", EditorStyles.miniButtonLeft)) {
#if UNITY_3_5
                    Selection.activeObject = PrefabUtility.GetPrefabParent(Selection.activeGameObject);
#else
					Selection.activeObject = EditorUtility.GetPrefabParent(Selection.activeGameObject);
#endif

				}
				if (GUILayout.Button("Reset", EditorStyles.miniButtonMid)) {
#if UNITY_3_5
                    PrefabUtility.ResetToPrefabState(Selection.activeGameObject);
#else
					PrefabUtility.ResetToPrefabState(Selection.activeGameObject);
#endif

				}
				if (GUILayout.Button("Break", EditorStyles.miniButtonRight)) {
					BreakPrefab(Selection.activeGameObject);
				}
				EndHorizontal();
				break;
			case PrefabType.DisconnectedPrefabInstance:
				BeginHorizontal();
				GUILayout.Label("Prefab", fixedLabel);
				if (GUILayout.Button("Select", EditorStyles.miniButtonLeft)) {
#if UNITY_3_5
                    Selection.activeObject = PrefabUtility.GetPrefabParent(Selection.activeGameObject);
#else
					Selection.activeObject = EditorUtility.GetPrefabParent(Selection.activeGameObject);
#endif
				}
				if (GUILayout.Button("Reconnect", EditorStyles.miniButtonMid)) {
#if UNITY_3_5
                    PrefabUtility.ReconnectToLastPrefab(Selection.activeGameObject);
#else
					EditorUtility.ReconnectToLastPrefab(Selection.activeGameObject);
#endif
				}
				if (GUILayout.Button("Apply", EditorStyles.miniButtonRight)) {
#if UNITY_3_5
                    var prefab = PrefabUtility.GetPrefabParent(Selection.activeGameObject);
                    PrefabUtility.ReplacePrefab(Selection.activeGameObject, prefab);
#else
					var prefab = EditorUtility.GetPrefabParent(Selection.activeGameObject);
					EditorUtility.ReplacePrefab(Selection.activeGameObject, prefab);
#endif
					AssetDatabase.Refresh();

					DestroyImmediate(Selection.activeGameObject);
#if UNITY_3_5
                    PrefabUtility.InstantiatePrefab(prefab);
#else
					EditorUtility.InstantiatePrefab(prefab);
#endif
				}
				EndHorizontal();
				break;
			case PrefabType.DisconnectedModelPrefabInstance:
				BeginHorizontal();
				GUILayout.Label("Prefab", fixedLabel);
				if (GUILayout.Button("Select", EditorStyles.miniButtonLeft)) {
#if UNITY_3_5
                    Selection.activeObject = PrefabUtility.GetPrefabParent(Selection.activeGameObject);
#else
					Selection.activeObject = EditorUtility.GetPrefabParent(Selection.activeGameObject);
#endif
				}
				if (GUILayout.Button("Reconnect", EditorStyles.miniButtonRight)) {
#if UNITY_3_5
                    PrefabUtility.ReconnectToLastPrefab(Selection.activeGameObject);
#else
					EditorUtility.ReconnectToLastPrefab(Selection.activeGameObject);
#endif
				}
				EndHorizontal();
				break;
		}

		EditorGUILayout.Space();
	}
	private void TitleBars()
	{
		// Loop through all each component and get it's property list
		foreach (var componentInspector in _components) {
			if (componentInspector.Key != null) {
				var type = componentInspector.Key.GetType();
				// Add a title bar for the component
				CreateComponent(componentInspector, type);
			} else {
				Debug.Log("Missing script");
			}
		}
	}
	private void DropDown()
	{
		var components = new List<Component>(_components.Keys);
		var displayOptions = new List<string> { "None" };
		displayOptions.AddRange(components.Select(c => c.GetType().ToString()));

		if (_repickPrevious) {
			for (var index = 0; index < components.Count; index++) {
				var c = components [index];
				if (c.GetType() == _previousComponentSelection) {
					_dropDownIndex = index + 1; // because components is missing a field
					break;
				}
			}
			_repickPrevious = false;
		}


		_dropDownIndex = EditorGUILayout.Popup("Component", _dropDownIndex, displayOptions.ToArray(), EditorStyles.popup);

		if (_dropDownIndex == 0) {
			return;
		}
		if (_dropDownIndex > components.Count) {
			return;
		} // Just stops the error when scripts rebuild when debugging.

		var component = components [_dropDownIndex - 1];
		var componentInspector = new KeyValuePair<Component, List<InspectorInfo>>(component, _components [component]);
		var type = component.GetType();

		_previousComponentSelection = type;

		CreateComponent(componentInspector, type);
	}
	private void CreateComponent(KeyValuePair<Component, List<InspectorInfo>> componentInspector, Type type)
	{
		// Add a title bar for the component
		_enabled [type] = EditorGUILayout.InspectorTitlebar(_enabled [type], componentInspector.Key);

		if (_enabled [type]) {
			// If it's not a special case, proceed like normal
			if (!SpecialInspector(componentInspector)) {
				BeginHorizontal();
				switch (EISettings.ShowField) {
					case EISettings.ShowFields.Switch:
						_showFields [type] = EditorGUILayout.Toggle("Show fields", _showFields [type]);
						break;
					case EISettings.ShowFields.Never:
						_showFields [type] = false;
						break;
					case EISettings.ShowFields.Always:
						_showFields [type] = true;
						break;
				}
				_showPrivate = EditorGUILayout.Toggle("Show private", _showPrivate);
				EndHorizontal();

				if (_showPrivate != _showPrivatePrevious) {
					_dirty = true;
					_showPrivatePrevious = _showPrivate;
				}

				if (_showFields [type]) {
					ShowFields(componentInspector);
				}

				BeginHorizontal();
				if (GUILayout.Button("Apply to Selection", EditorStyles.miniButtonLeft)) {
					SaveMassUndo("Apply to Selection");
					SetAllSelection(componentInspector);
				}
				if (GUILayout.Button("Apply to Children", EditorStyles.miniButtonMid)) {
					SaveMassUndo("Apply to Children");
					SetAllChildren(componentInspector);
				}
				if (GUILayout.Button("Apply to Both", EditorStyles.miniButtonRight)) {
					SaveMassUndo("Apply to Both");
					SetAllBoth(componentInspector);
				}
				EndHorizontal();
				EditorGUILayout.Space();
			}
		}
	}
	private void ShowFields(KeyValuePair<Component, List<InspectorInfo>> componentInspector)
	{
		var monoBehaviour = componentInspector.Key as MonoBehaviour;
		if (monoBehaviour != null) {
			var monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);
			EditorGUILayout.ObjectField("Script", monoScript, typeof(MonoScript), false);
		}

		// Loop through each property and create it
		foreach (
            var property in
                componentInspector.Value.Where(property => property.CanWrite && ValidProperty(property))
            ) {
			CreateField(componentInspector.Key, property); // todo undo
		}
	}
	private void BreakPrefab(GameObject obj)
	{
		_prefabBreakQueue.Enqueue(obj.AddComponent<GUIText>());
	}
	private void CreateField(object component, InspectorInfo field)
	{
		var type = field.InspectorType;
		var labelName = field.Name.UppercaseFirst();
		var value = field.GetValue(component);

		if (type.IsArray) {
			_enabledFoldout [field] = EditorGUILayout.Foldout(_enabledFoldout [field], labelName);
			CreateArrayGroup(component, field);
		} else if (type.IsGenericType) {
			_enabledFoldout [field] = EditorGUILayout.Foldout(_enabledFoldout [field], labelName);
			CreateGenericGroup(component, field, type);
		} else {
			var v = CreateGuiElement(value, type, labelName);
			if (v != null) {
				field.SetValue(component, v);
			}
		}
	}
	private void CreateArrayGroup(object component, InspectorInfo field)
	{
		if (!_enabledFoldout [field]) {
			return;
		}

		EditorGUI.indentLevel = 2;
		var objectList = (Array)field.GetValue(component);
		var newSize = EditorGUILayout.IntField("Size", objectList.Length);
		newSize = Mathf.Clamp(newSize, 0, int.MaxValue);
		if (objectList.Length - newSize != 0) {
			Resize(ref objectList, newSize);
			field.SetValue(component, objectList);
		}

		if (objectList.Length != 0) {
			var arrayType = objectList.GetValue(0).GetType();
			for (var index = 0; index < objectList.Length; index++) {
				var f = objectList.GetValue(index);
				var v = CreateGuiElement(f, arrayType, "Element " + index);
				objectList.SetValue(v, index);
			}
			field.SetValue(component, objectList);
		}
		EditorGUI.indentLevel = 0;
	}
	private void CreateGenericGroup(object component, InspectorInfo field, Type type)
	{
		if (!_enabledFoldout [field]) {
			return;
		}

		EditorGUI.indentLevel = 2;
		if (type.GetGenericTypeDefinition() == typeof(List<>)) {
			var objectList = field.GetValue(component);
			var constructed = typeof(List<>).MakeGenericType(type.GetGenericArguments());
			var o = (IList)Activator.CreateInstance(constructed, objectList);

			var newSize = EditorGUILayout.IntField("Size", o.Count);
			newSize = Mathf.Clamp(newSize, 0, int.MaxValue);
			if (o.Count - newSize != 0) {
				Resize(ref o, newSize, type.GetGenericArguments() [0]);
				field.SetValue(component, o);
			}

			if (o.Count != 0) {
				var arrayType = o [0].GetType();
				for (var index = 0; index < o.Count; index++) {
					var f = o [index];
					o [index] = CreateGuiElement(f, arrayType, "Element " + index);
				}
				field.SetValue(component, o);
			}
		} else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
			var objectList = field.GetValue(component);
			var constructed = typeof(Dictionary<,>).MakeGenericType(type.GetGenericArguments());
			try {
				var o = (IDictionary)Activator.CreateInstance(constructed, objectList);
				var arrayType = type.GetGenericArguments();

				if (o.Count != 0) {
					var valueType = arrayType [1];

					EditorGUILayout.LabelField("Key", "Value");

					var keys = new ArrayList(o.Keys);

					foreach (var key in keys) {
						BeginHorizontal();
						var f = o [key];
						o [key] = CreateGuiElement(f, valueType, key.ToString());
						EndHorizontal();
					}
					field.SetValue(component, o);
				}
				EditorGUILayout.Space();
            
				// Editing Dictionary Coming Soon
				if (_key == null) {
					_key = DefaultValue(arrayType [0]);
				}
				if (_value == null) {
					_value = DefaultValue(arrayType [1]);
				}

				BeginHorizontal();
				_key = CreateGuiElement(_key, arrayType [0], "Key");
				_value = CreateGuiElement(_value, arrayType [1], "Value");
				EndHorizontal();
				BeginHorizontal();
				EditorGUILayout.PrefixLabel("Action");
				if (GUILayout.Button("Add", EditorStyles.miniButtonLeft)) {
					if (!o.Contains(_key)) {
						o.Add(_key, _value);
						_key = null;
						_value = null;
						AddToUpdateQueue(component, field, o);
						// Need to serialize dictionary to disk
					} else {
						Debug.Log("Key already exists in dictionary");
					}
				}
				if (GUILayout.Button("Remove", EditorStyles.miniButtonRight)) {
					if (o.Contains(_key)) {
						o.Remove(_key);
						_key = null;
						_value = null;
						AddToUpdateQueue(component, field, o);
						// Need to serialize dictionary to disk
					} else {
						Debug.Log("Key doesn't exist in dictionary");
					}
				}
				EndHorizontal();
			} catch
            (AmbiguousMatchException) {
				Debug.Log("Dictionary not initialized");
				_enabledFoldout [field] = false; // Closing so message doesn't loop
			}
		} else {
			GUILayout.Label(type.GetGenericTypeDefinition() + " - No support atm");
		}
        
		EditorGUI.indentLevel = 0;

	}
	private static object CreateGuiElement(object value, Type type, string labelName)
	{
		if (type == typeof(bool)) {
			return EditorGUILayout.Toggle(labelName, (bool)value);
		}
		if (type == typeof(string)) {
			return EditorGUILayout.TextField(labelName, (string)value);
		}
		if (type == typeof(float)) {
			return EditorGUILayout.FloatField(labelName, (float)value);
		}
		if (type == typeof(double)) {
			var changeType = Convert.ChangeType(value, typeof(float));
			if (changeType != null) {
				var val = (float)changeType;
				return EditorGUILayout.FloatField(labelName, val);
			}
		}
		if (type == typeof(long)) {
			var changeType = Convert.ChangeType(value, typeof(int));
			if (changeType != null) {
				var val = (int)changeType;
				return EditorGUILayout.IntField(labelName, val);
			}
		}
		if (type == typeof(uint)) {
			var changeType = Convert.ChangeType(value, typeof(int));
			if (changeType != null) {
				var val = (int)changeType;
				val = EditorGUILayout.IntField(labelName, val);
				val = Mathf.Clamp(val, 0, int.MaxValue);
				return Convert.ChangeType(val, typeof(uint));
			}
		}
		if (type == typeof(int)) {
			return EditorGUILayout.IntField(labelName, (int)value);
		}
		if (type == typeof(byte)) {
			return (byte)EditorGUILayout.IntField(labelName, (byte)value);
		}
		if (type == typeof(Vector2)) {
			return EditorGUILayout.Vector2Field(labelName, (Vector2)value);
		}
		if (type == typeof(Vector3)) {
			return EditorGUILayout.Vector3Field(labelName, (Vector3)value);
		}
		if (type == typeof(Vector4)) {
			return EditorGUILayout.Vector4Field(labelName, (Vector4)value);
		}
		if (type == typeof(Rect)) {
			return EditorGUILayout.RectField(labelName, (Rect)value);
		}
		if (type == typeof(Bounds)) {
			return EditorGUILayout.BoundsField(labelName, (Bounds)value);
		}
		if (type == typeof(Color)) {
			return EditorGUILayout.ColorField(labelName, (Color)value);
		}
		if (type == typeof(AnimationCurve)) {
			return EditorGUILayout.CurveField(labelName, (AnimationCurve)value);
		}
		if (type == typeof(SerializedProperty)) {
			return EditorGUILayout.PropertyField((SerializedProperty)value);
		}
		if (type.IsEnum) {
			try {
				return EditorGUILayout.EnumPopup(labelName, (Enum)value);
			} catch (Exception) {
				return null;
			}
		}
		// weird stuff
		if (type == typeof(Quaternion)) {
			var quat = (Quaternion)value;
			var vec4 = new Vector4(quat.x, quat.y, quat.z, quat.w);

			var v = EditorGUILayout.Vector4Field(labelName, vec4);
			return new Quaternion(v.x, v.y, v.z, v.w);
		}
		if (type.BaseType == typeof(Object)) {
			return EditorGUILayout.ObjectField(labelName, (Object)value, type, true);
		}

		return null;
	}
	private void SetAllSelection(KeyValuePair<Component, List<InspectorInfo>> componentInspector)
	{
		// Get all selected gameobjects
		foreach (var gameObject in Selection.gameObjects.Where(gameObject => gameObject != Selection.activeGameObject)) {
			// Get the components that match current active component
			foreach (var component in gameObject.GetComponents(componentInspector.Key.GetType())) {
				// Loop through all the properties of current active component
				foreach (var property in componentInspector.Value.Where(property => property.CanWrite && ValidProperty(property))) {
					// Grab the value of the active component
					var value = property.GetValue(componentInspector.Key);
					// Set the other game object to the current active component
					property.SetValue(component, value);
				}
			}
		}
	}
	private void SetAllChildren(KeyValuePair<Component, List<InspectorInfo>> componentInspector)
	{
		// Get all children transform
		foreach (Transform tr in Selection.activeTransform) {
			// Get the components that match current active component
			foreach (var component in tr.GetComponents(componentInspector.Key.GetType())) {
				// Loop through all the properties of current active component
				foreach (var property in componentInspector.Value.Where(property => property.CanWrite && ValidProperty(property))) {
					// Grab the value of the active component
					var value = property.GetValue(componentInspector.Key);
					// Set the other game object to the current active component
					property.SetValue(component, value);
				}
			}
		}
	}
	private void SetAllBoth(KeyValuePair<Component, List<InspectorInfo>> componentInspector)
	{
		foreach (var gameObject in Selection.gameObjects) {
			// First remove from selection
			foreach (var component in gameObject.GetComponents(componentInspector.Key.GetType())) {
				// Loop through all the properties of current active component
				foreach (var property in componentInspector.Value.Where(property => property.CanWrite && ValidProperty(property))) {
					// Grab the value of the active component
					var value = property.GetValue(componentInspector.Key);
					// Set the other game object to the current active component
					property.SetValue(component, value);
				}
			}

			// Then get all children in that selection
			foreach (Transform tr in gameObject.transform) {
				// Get the components that match current active component
				foreach (var component in tr.GetComponents(componentInspector.Key.GetType())) {
					// Loop through all the properties of current active component
					foreach (var property in componentInspector.Value.Where(property => property.CanWrite && ValidProperty(property))) {
						// Grab the value of the active component
						var value = property.GetValue(componentInspector.Key);
						// Set the other game object to the current active component
						property.SetValue(component, value);
					}
				}
			}
		}
	}
	private static void Resize(ref Array array, int newSize)
	{
		var elementType = array.GetType().GetElementType();
		if (elementType == null) {
			return;
		}

		var newArray = Array.CreateInstance(elementType, newSize);
		Array.Copy(array, newArray, Math.Min(array.Length, newArray.Length));
		array = newArray;
	}
	private static void Resize(ref IList array, int newSize, Type type)
	{
		var difference = newSize - array.Count;
		if (difference > 0) {
			for (var i = 0; i < difference; i++) {
				array.Add(DefaultValue(type));
			}
		} else {
			for (var i = 0; i < Math.Abs(difference); i++) {
				array.RemoveAt(array.Count - 1);
			}
		}
	}
	private static object DefaultValue(Type type)
	{
		return type == typeof(string) ? "" : Activator.CreateInstance(type);
	}
	private void CreatePrefab(GameObject obj, ref Object prefab)
	{
		const string localPath = "Assets/hold.prefab";

#if UNITY_3_5
        prefab = PrefabUtility.CreateEmptyPrefab(localPath);
        PrefabUtility.ReplacePrefab(obj, prefab);
#else
		prefab = EditorUtility.CreateEmptyPrefab(localPath);
		EditorUtility.ReplacePrefab(obj, prefab);        
#endif

		AssetDatabase.Refresh();

	}
	private void RestorePrefab(GameObject obj, Object prefab)
	{
		DestroyImmediate(obj);
#if UNITY_3_5
        PrefabUtility.InstantiatePrefab(prefab);
#else
		EditorUtility.InstantiatePrefab(prefab);        
#endif

	}
	private void AddToUpdateQueue(object component, InspectorInfo field, object o)
	{
		_updateQueue.Enqueue(new UpdateStruct(component, field, o));
	}
	private static bool ValidProperty(InspectorInfo inspectorInfo)
	{
		switch (inspectorInfo.Name.ToLower()) {
			case "active":
			case "enabled":
			case "hideflags":
			case "name":
			case "tag":
			case "material":
			case "materials":
			case "mesh":
			case "useguilayout":
				return false;
			default:
				return true;
		}
	}
	private void BeginHorizontal()
	{
		//if (!_newSelection)
		EditorGUILayout.BeginHorizontal();
	}
	private void EndHorizontal()
	{
		//if (!_newSelection)
		EditorGUILayout.EndHorizontal();
	}
	private void CheckUndo(Object obj)
	{
		var e = Event.current;

		if ((e.type == EventType.MouseDown && e.button == 0 || e.type == EventType.KeyUp && (e.keyCode == KeyCode.Tab))) {
			var source = EditorUtility.CollectDeepHierarchy(new[] { obj });

			Undo.SetSnapshotTarget(source.ToArray(), "Extended Inspector");
			Undo.CreateSnapshot();
			Undo.ClearSnapshotTarget();
			_listeningForGuiChanges = true;
			_guiChanged = false;
			_objectToUndo = obj;
		}
	}
	private void SaveUndo()
	{
		if (_listeningForGuiChanges && _guiChanged) {
			var source = EditorUtility.CollectDeepHierarchy(new[] { _objectToUndo });
			Undo.SetSnapshotTarget(source.ToArray(), "Extended Inspector");
			Undo.RegisterSnapshot();
			Undo.ClearSnapshotTarget();
			_listeningForGuiChanges = false;
		}
	}
	private void SaveMassUndo(string message)
	{
		_listeningForGuiChanges = false;
		Undo.RegisterSceneUndo(message);
	}
	private struct UpdateStruct
	{
		private readonly object _component;
		private readonly InspectorInfo _field;
		private readonly object _obj;
		public UpdateStruct(object component, InspectorInfo field, object o)
		{
			_component = component;
			_field = field;
			_obj = o;
		}
		public void Update()
		{
			_field.SetValue(_component, _obj);
		}
	}
	public static bool ToggleChangeButton(bool state, string text, GUIStyle style)
	{
		var changed = false;

		var newState = GUILayout.Toggle(state, text, style);
		if (newState != state) {
			changed = true;
		}
        
		return changed;
	}
	public static bool ToggleTagField(string tag, GUIStyle style, out string newTag)
	{
		var changed = false;

		newTag = EditorGUILayout.TagField(tag, style);
		if (newTag != tag) {
			changed = true;
		}

		return changed;
	}
	public static bool ToggleLayerField(int layer, GUIStyle style, out int newLayer)
	{
		var changed = false;

		newLayer = EditorGUILayout.LayerField(Selection.activeGameObject.layer, EditorStyles.popup);
		if (newLayer != layer) {
			changed = true;
		}

		return changed;
	}
}