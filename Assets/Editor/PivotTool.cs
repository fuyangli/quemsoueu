using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class PivotTool : EditorWindow
{
    private Transform _tr;
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private Vector3 _pivot;
    private Vector3 _pivotPrevious;
    private Collider _collider;
    // Undo Stuff
    private Object _objectToUndo;
    private bool _listeningForGuiChanges;
    private bool _saveUndo;

    [MenuItem("Window/Extended Inspector/Pivot")]
    static void Init()
    {
        var window = (PivotTool)GetWindowWithRect(typeof(PivotTool), new Rect(0, 0, 300, 90), false, "Pivot");
        window.autoRepaintOnSceneChange = true;
        window.GetComponents();
    }
    void OnInspectorUpdate()
    {
        Repaint();
    }
    void OnSelectionChange()
    {
        _listeningForGuiChanges = false;
        GetComponents();
    }
    void OnGUI()
    {
        CheckUndo();

        if (!_tr)
        {
            GUILayout.Label("No game object selected.");
            return;
        }
        if (!_mesh)
        {
            GUILayout.Label("Game object doesn't have a mesh");
            return;
        }
        var warning = "";
        if (_tr.eulerAngles != Vector3.zero)
        {
            warning += "Warning: Rotation. ";
        }
        if (_tr.localScale != Vector3.one)
        {
            warning += "Warning: Scale.";
        }
        GUILayout.Label(warning != string.Empty ? warning : "No warnings.");

        // If modified externally
        if (_pivot != _tr.position)
        {
            _pivot = _tr.position;
            _pivotPrevious = _tr.position;
        }

        _pivot = EditorGUILayout.Vector3Field("Pivot", _pivot);
        if (_pivot != _pivotPrevious)
        {
            UpdatePivot(_pivot);
            _pivotPrevious = _pivot;
        }
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset To Centre"))
        {
            var centre = _tr.position + _mesh.bounds.center;
            UpdatePivot(centre);
        }
        EditorGUILayout.EndHorizontal();
        if (GUI.changed)
        {
            _saveUndo = true;
        }
    }
    private void UpdatePivot(Vector3 newPivot)
    {
        var difference = _pivotPrevious - newPivot; // opposite
        _tr.position = newPivot;

        var verts = _mesh.vertices;
        for (var i = 0; i < verts.Length; i++)
        {
            verts[i] += difference;
        }
        _mesh.vertices = verts;
        _mesh.RecalculateBounds();

        if (_collider is BoxCollider)
        {
            ((BoxCollider)_collider).center += difference;
        }
        else if (_collider is CapsuleCollider)
        {
            ((CapsuleCollider)_collider).center += difference;
        }
        else if (_collider is SphereCollider)
        {
            ((SphereCollider)_collider).center += difference;
        }
        // Mesh collider should work fine as long as it's using the same mesh
    }
    private void GetComponents()
    {
        _tr = Selection.activeTransform;
        if (!_tr)
        {
            _tr = null;
            _mesh = null;
            return;
        }
        // May as well just store everything for mild performance improvement
        _meshFilter = _tr.GetComponent<MeshFilter>();
        _mesh = _meshFilter ? _meshFilter.sharedMesh : null;
        _collider = _tr.GetComponent<Collider>();
    }
    private void CheckUndo()
    {
        var e = Event.current;

        if ((e.type == EventType.MouseDown && e.button == 0 || e.type == EventType.KeyUp && (e.keyCode == KeyCode.Tab)))
        {
            var source = EditorUtility.CollectDeepHierarchy(new[] { Selection.activeGameObject });
            Undo.SetSnapshotTarget(source.ToArray(), "Extended Inspector");
            Undo.CreateSnapshot();
            Undo.ClearSnapshotTarget();
            _listeningForGuiChanges = true;
            _saveUndo = false;
        }

        if (_listeningForGuiChanges && _saveUndo)
        {
            var source = EditorUtility.CollectDeepHierarchy(new[] { Selection.activeGameObject });
            Undo.SetSnapshotTarget(source.ToArray(), "Extended Inspector");
            Undo.RegisterSnapshot();
            Undo.ClearSnapshotTarget();
            _listeningForGuiChanges = false;
        }
    }
}
