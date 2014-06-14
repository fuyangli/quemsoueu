using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class ArrayTool : EditorWindow
{
    private const float GroupWidth = 60;

    private Vector2 _scroll;

    private List<Transform> _previews = new List<Transform>();

    private static int _dimension;

    private static float _xMoveInc, _yMoveInc, _zMoveInc;
    private static float _xRotInc, _yRotInc, _zRotInc;
    private static float _xScaleInc = 1, _yScaleInc = 1, _zScaleInc = 1;

    private static float _x2MoveInc, _y2MoveInc, _z2MoveInc;
    private static float _x3MoveInc, _y3MoveInc, _z3MoveInc;

    private static int _1D = 1, _2D = 1, _3D = 1;

    private readonly GUIStyle _guiLabel = new GUIStyle();

    [MenuItem("Window/Extended Inspector/Array")]
	static void Init()
	{
	    GetWindowWithRect(typeof (ArrayTool), new Rect(0,0,480,270), false, "Array");
	}
	
	void OnGUI()
	{
        _guiLabel.alignment = TextAnchor.LowerCenter;
        _guiLabel.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
	    _guiLabel.fixedHeight = 18f;

        EditorGUILayout.BeginVertical();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

	    CreateLayout();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
	}

    private void CreateLayout()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(10));
        ArrayTransform();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(10));
        //TypeOfObject();
        ArrayDimensions();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(10));
        Buttons();
        EditorGUILayout.EndHorizontal();
    }
    private List<Transform> CreateArray()
    {
        var active = Selection.activeTransform;
        var list = new List<Transform>();

        if (active == null)
        {
            Debug.Log("No game object currently selected");
            return list;
        }

        Undo.RegisterSceneUndo("Array");

        for(var i = 0; i < _1D; i++)
        {
            var position1 = active.position + new Vector3(_xMoveInc, _yMoveInc, _zMoveInc)*i;
            var rot1 = active.eulerAngles + new Vector3(_xRotInc, _yRotInc, _zRotInc)*i;
            var rotation1 = Quaternion.Euler(rot1);

            var scalex = active.localScale.x * Mathf.Pow(_xScaleInc, i);
            var scaley = active.localScale.y * Mathf.Pow(_yScaleInc, i);
            var scalez = active.localScale.z * Mathf.Pow(_zScaleInc, i);

            var scale = new Vector3(scalex, scaley, scalez);

            Transform copy;
            if (i != 0)
            {
#if UNITY_3_5
                var prefabRoot = PrefabUtility.GetPrefabParent(active);
#else
                var prefabRoot = EditorUtility.GetPrefabParent(active);
#endif
                if (prefabRoot != null)
                {
#if UNITY_3_5
                    copy = (Transform)PrefabUtility.InstantiatePrefab(prefabRoot);
#else
                    copy = (Transform)EditorUtility.InstantiatePrefab(prefabRoot);
#endif
                }
                else 
                {
                    copy = (Transform) Instantiate(active);
                }
                copy.position = position1;
                copy.rotation = rotation1;
                copy.localScale = scale;
                copy.name = active.name;

                list.Add(copy);
            }
            else
            {
                copy = active;
            }

            if (_dimension <= 0) continue;            // 2D check

            for (var j = 0; j < _2D; j++)
            {
                var position2 = copy.position + new Vector3(_x2MoveInc, _y2MoveInc, _z2MoveInc)*j;
                var rotation2 = copy.rotation;

                Transform copy2;
                if (j != 0)
                {
#if UNITY_3_5
                    var prefabRoot = PrefabUtility.GetPrefabParent(copy);
#else
                    var prefabRoot = EditorUtility.GetPrefabParent(copy);
#endif
                    if (prefabRoot != null)
                    {
#if UNITY_3_5
                        copy2 = (Transform)PrefabUtility.InstantiatePrefab(prefabRoot);
#else
                        copy2 = (Transform)EditorUtility.InstantiatePrefab(prefabRoot);
#endif
                    }
                    else
                    {
                        copy2 = (Transform)Instantiate(copy);
                    }
                    copy2.position = position2;
                    copy2.rotation = rotation2;
                    copy2.localScale = copy.localScale;
                    copy2.name = active.name;

                    list.Add(copy2);
                }
                else
                {
                    copy2 =  copy;
                }

                if (_dimension <= 1) continue;            // 3D check

                for (var k = 0; k < _3D; k++)
                {
                    var position3 = copy2.position + new Vector3(_x3MoveInc, _y3MoveInc, _z3MoveInc) * k;
                    var rotation3 = copy2.rotation;

                    Transform copy3;
                    if (k != 0)
                    {
#if UNITY_3_5
                        var prefabRoot = PrefabUtility.GetPrefabParent(copy2);
#else
                        var prefabRoot = EditorUtility.GetPrefabParent(copy2);
#endif

                        if (prefabRoot != null)
                        {
#if UNITY_3_5
                            copy3 = (Transform)PrefabUtility.InstantiatePrefab(prefabRoot);
#else
                            copy3 = (Transform)EditorUtility.InstantiatePrefab(prefabRoot);
#endif
                        }
                        else
                        {
                            copy3 = (Transform)Instantiate(copy2);
                        }
                        copy3.position = position3;
                        copy3.rotation = rotation3;
                        copy3.localScale = copy2.localScale;
                        copy3.name = active.name;

                        list.Add(copy3);
                    }
                }
            }
        }

        return list;
    }
    private void ArrayTransform()
    {
        //GUILayout.Label("Array Transformation: Local Coordinates");
        EditorGUILayout.BeginVertical();

        _guiLabel.fixedWidth = GroupWidth * 3;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Incremental", _guiLabel);
        GUILayout.Label("", GUILayout.Width(GroupWidth));
        GUILayout.Label("Totals", _guiLabel);
        EditorGUILayout.EndHorizontal();

        _guiLabel.fixedWidth = GroupWidth;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("X", _guiLabel);
        GUILayout.Label("Y", _guiLabel);
        GUILayout.Label("Z", _guiLabel);
        GUILayout.Label("", GUILayout.Width(GroupWidth));
        GUILayout.Label("X", _guiLabel);
        GUILayout.Label("Y", _guiLabel);
        GUILayout.Label("Z", _guiLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _xMoveInc = EditorGUILayout.FloatField(_xMoveInc, GUILayout.Width(GroupWidth));
        _yMoveInc = EditorGUILayout.FloatField(_yMoveInc, GUILayout.Width(GroupWidth));
        _zMoveInc = EditorGUILayout.FloatField(_zMoveInc, GUILayout.Width(GroupWidth));
        GUILayout.Label("Move", _guiLabel);
        _xMoveInc = EditorGUILayout.FloatField(_xMoveInc * _1D, GUILayout.Width(GroupWidth)) / _1D;
        _yMoveInc = EditorGUILayout.FloatField(_yMoveInc * _1D, GUILayout.Width(GroupWidth)) / _1D;
        _zMoveInc = EditorGUILayout.FloatField(_zMoveInc * _1D, GUILayout.Width(GroupWidth)) / _1D;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _xRotInc = EditorGUILayout.FloatField(_xRotInc, GUILayout.Width(GroupWidth));
        _yRotInc = EditorGUILayout.FloatField(_yRotInc, GUILayout.Width(GroupWidth));
        _zRotInc = EditorGUILayout.FloatField(_zRotInc, GUILayout.Width(GroupWidth));
        GUILayout.Label("Rotate", _guiLabel);
        _xRotInc = EditorGUILayout.FloatField(_xRotInc * _1D, GUILayout.Width(GroupWidth)) / _1D;
        _yRotInc = EditorGUILayout.FloatField(_yRotInc * _1D, GUILayout.Width(GroupWidth)) / _1D;
        _zRotInc = EditorGUILayout.FloatField(_zRotInc * _1D, GUILayout.Width(GroupWidth)) / _1D;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _xScaleInc = EditorGUILayout.FloatField(_xScaleInc, GUILayout.Width(GroupWidth));
        _yScaleInc = EditorGUILayout.FloatField(_yScaleInc, GUILayout.Width(GroupWidth));
        _zScaleInc = EditorGUILayout.FloatField(_zScaleInc, GUILayout.Width(GroupWidth));
        GUILayout.Label("Scale", _guiLabel);
        _xScaleInc = Mathf.Pow(EditorGUILayout.FloatField(Mathf.Pow(_xScaleInc, _1D), GUILayout.Width(GroupWidth)), 1f/_1D);
        _yScaleInc = Mathf.Pow(EditorGUILayout.FloatField(Mathf.Pow(_yScaleInc, _1D), GUILayout.Width(GroupWidth)), 1f/_1D);
        _zScaleInc = Mathf.Pow(EditorGUILayout.FloatField(Mathf.Pow(_zScaleInc, _1D), GUILayout.Width(GroupWidth)), 1f/_1D);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
    private void ArrayDimensions()
    {
        EditorGUILayout.BeginHorizontal(); // Begin Array Dimensions

        EditorGUILayout.BeginVertical(); // Begin 1
        GUILayout.Label("", GUILayout.Width(GroupWidth));
        GUILayout.Label("", GUILayout.Width(GroupWidth));
        _dimension = GUILayout.SelectionGrid(_dimension, new[] { "1D", "2D", "3D" }, 1, GUILayout.Width(GroupWidth));
        EditorGUILayout.EndVertical(); // End 1

        EditorGUILayout.BeginVertical(); // Begin 2

        GUILayout.Label("Array Dimensions", GUILayout.Width(GroupWidth * 4));

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Count", GUILayout.Width(GroupWidth));
        _guiLabel.fixedWidth = GroupWidth * 3;
        GUILayout.Label("Incremental Row", _guiLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        _guiLabel.fixedWidth = GroupWidth;

        EditorGUILayout.BeginHorizontal(); // 1D
        _1D = EditorGUILayout.IntField(_1D, GUILayout.Width(GroupWidth));
        _1D = Mathf.Clamp(_1D, 1, int.MaxValue);
        GUILayout.Label("X", _guiLabel);
        GUILayout.Label("Y", _guiLabel);
        GUILayout.Label("Z", _guiLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(); // 2D
        _2D = EditorGUILayout.IntField(_2D, GUILayout.Width(GroupWidth));
        _2D = Mathf.Clamp(_2D, 1, int.MaxValue);
        _x2MoveInc = EditorGUILayout.FloatField(_x2MoveInc, GUILayout.Width(GroupWidth));
        _y2MoveInc = EditorGUILayout.FloatField(_y2MoveInc, GUILayout.Width(GroupWidth));
        _z2MoveInc = EditorGUILayout.FloatField(_z2MoveInc, GUILayout.Width(GroupWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(); // 3D
        _3D = EditorGUILayout.IntField(_3D, GUILayout.Width(GroupWidth));
        _3D = Mathf.Clamp(_3D, 1, int.MaxValue);
        _x3MoveInc = EditorGUILayout.FloatField(_x3MoveInc, GUILayout.Width(GroupWidth));
        _y3MoveInc = EditorGUILayout.FloatField(_y3MoveInc, GUILayout.Width(GroupWidth));
        _z3MoveInc = EditorGUILayout.FloatField(_z3MoveInc, GUILayout.Width(GroupWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical(); // End 2

        EditorGUILayout.EndHorizontal(); // End Array Dimensions
    }
    private void Buttons()
    {
        GUILayout.Label("Total Size", GUILayout.Width(GroupWidth * 1f));
        EditorGUILayout.IntField(_1D * _2D * _3D, GUILayout.Width(GroupWidth));

        if (GUILayout.Button("Preview", GUILayout.Width(GroupWidth * 1f)))
        {
            DestroyPreview();
            _previews = CreateArray();
        }

        GUILayout.Label("", GUILayout.Width(GroupWidth * 0.5f));
        if (GUILayout.Button("Reset", GUILayout.Width(GroupWidth * 1f)))
        {
            _xMoveInc = 0;
            _yMoveInc = 0;
            _zMoveInc = 0;
            _xRotInc = 0;
            _yRotInc = 0;
            _zRotInc = 0;
            _xScaleInc = 1;
            _yScaleInc = 1;
            _zScaleInc = 1;
            _x2MoveInc = 0;
            _y2MoveInc = 0;
            _z2MoveInc = 0;
            _x3MoveInc = 0;
            _y3MoveInc = 0;
            _z3MoveInc = 0;
            _1D = 1;
            _2D = 1;
            _3D = 1;

            _dimension = 0;
        }
        GUILayout.Label("", GUILayout.Width(GroupWidth * 0.5f));
        if (GUILayout.Button("Accept", GUILayout.Width(GroupWidth)))
        {
            /*
            var current = System.DateTime.Now;
            var date = current.Hour + current.Minute + current.Day + current.Month.ToString();

            var prefab = EditorUtility.CreateEmptyPrefab("Assets/Prefabs/" +
                Selection.activeGameObject.gameObject.name +
                date +
                ".prefab");
            EditorUtility.ReplacePrefab(Selection.activeGameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);*/

            DestroyPreview();
            CreateArray();
            Close();
        }
        if (GUILayout.Button("Cancel", GUILayout.Width(GroupWidth)))
        {
            DestroyPreview();
            Close();
        }
    }
    private void DestroyPreview()
    {
        foreach (var t in _previews.Where(t => t))
        {
            DestroyImmediate(t.gameObject);
        }
        _previews.Clear();
    }
}
