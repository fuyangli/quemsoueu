using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class ExtendedInspector
{
    private void SpecialCasesUpdate()
    {
        if (_transformMathIndex != _transformMathIndexPrevious)
        {
            switch (_transformMathIndex)
            {
                case 0:
                case 1:
                    _transformPosition = Vector3.zero;
                    _transformRotation = Vector3.zero;
                    _transformScale = Vector3.zero;
                    break;
                case 2:
                case 3:
                    _transformPosition = Vector3.one;
                    _transformRotation = Vector3.one;
                    _transformScale = Vector3.one;
                    break;
                default:
                    Debug.Log("Unknown case");
                    break;
            }
            _transformMathIndexPrevious = _transformMathIndex;
        }
    }
    private bool SpecialInspector(KeyValuePair<Component, List<InspectorInfo>> componentInspector)
    {
        var component = componentInspector.Key;

        if (component is Transform)
        {
            TransfromField(componentInspector);
            return true;
        }
        if (component is Rigidbody)
        {
            RigidBodyField(componentInspector);
            return true;
        }
        if (component is Renderer)
        {
            //return true;
        }

        return false;
    }
    private static bool _transformLocal = true;
    private static bool _transformMath;
    private static bool _transformQuaternion;
    private static int _transformMathIndex;
    private static int _transformMathIndexPrevious;

    private void TransfromField(KeyValuePair<Component, List<InspectorInfo>> componentInspector)
    {
        //var component = componentInspector.Key;
        //_transformMath = EditorGUILayout.Toggle("Math", _transformMath);
        BeginHorizontal();
        GUILayout.Label("Math", GUILayout.Width(30));
        _transformMath = GUILayout.Toggle(_transformMath, "");
        GUILayout.Label("    Local", GUILayout.Width(50));
        _transformLocal = GUILayout.Toggle(_transformLocal, "");
        if (!_transformMath)
        {
            GUILayout.Label("    Quaternion", GUILayout.Width(80));
            _transformQuaternion = GUILayout.Toggle(_transformQuaternion, "");
        }
        EndHorizontal();

        if (_transformMath) // math
        {
            TransformMath();
        }
        else // standard 
        {
            TransformStandard(componentInspector);
        }
    }

    private Vector3 _transformPosition;
    private Vector3 _transformRotation;
    private Vector3 _transformScale;
    private void TransformMath()
    {
        _transformMathIndex = GUILayout.SelectionGrid(_transformMathIndex, new[] {"Add", "Sub", "Mul", "Div"}, 4,
                                                      EditorStyles.miniButtonMid);

        var local = _transformLocal ? "Local " : "";

        // Position
        _transformPosition = EditorGUILayout.Vector3Field(local+"Position", _transformPosition);
        // Rotation
        _transformRotation = EditorGUILayout.Vector3Field(local+"Euler Angles", _transformRotation);
        // Scale
        _transformScale = EditorGUILayout.Vector3Field(local+"Scale", _transformScale);

        BeginHorizontal();
        if (GUILayout.Button("Selected", EditorStyles.miniButtonLeft))
        {
            TransformSet(Selection.activeTransform);
        }
        if (GUILayout.Button("Selection", EditorStyles.miniButtonMid))
        {
            foreach(var t in Selection.transforms)
            {
                TransformSet(t);
            }
        }
        if (GUILayout.Button("Children", EditorStyles.miniButtonMid))
        {
            foreach (Transform t in Selection.activeTransform)
            {
                TransformSet(t);
            }
        }
        if (GUILayout.Button("Both", EditorStyles.miniButtonRight))
        {
            foreach (var t in Selection.transforms)
            {
                TransformSet(t);
                foreach (Transform child in Selection.activeTransform)
                {
                    TransformSet(child);
                }
            }
        }
        EndHorizontal();
    }
    private void TransformSet(Transform t)
    {
        if (_transformLocal)
        {
            switch (_transformMathIndex)
            {
                case 0:
                    t.localPosition += _transformPosition;
                    t.localEulerAngles += _transformRotation;
                    t.localScale += _transformScale;
                    break;
                case 1:
                    t.localPosition -= _transformPosition;
                    t.localEulerAngles -= _transformRotation;
                    t.localScale -= _transformScale;
                    break;
                case 2:
                    t.localPosition = Utility.Vector3Multiply(t.localPosition, _transformPosition);
                    t.localEulerAngles = Utility.Vector3Multiply(t.localEulerAngles, _transformRotation);
                    t.localScale = Utility.Vector3Multiply(t.localScale, _transformScale);
                    break;
                case 3:
                    t.localPosition = Utility.Vector3Divide(t.localPosition, _transformPosition);
                    t.localEulerAngles = Utility.Vector3Divide(t.localEulerAngles, _transformRotation);
                    t.localScale = Utility.Vector3Divide(t.localScale, _transformScale);
                    break;
                default:
                    Debug.Log("Unknown case");
                    break;
            }
        }
        else
        {
            switch (_transformMathIndex)
            {
                case 0:
                    t.position += _transformPosition;
                    t.eulerAngles += _transformRotation;
                    //t.localScale += _transformScale;
                    break;
                case 1:
                    t.position -= _transformPosition;
                    t.eulerAngles -= _transformRotation;
                    //t.localScale -= _transformScale;
                    break;
                case 2:
                    t.position = Utility.Vector3Multiply(t.position, _transformPosition);
                    t.eulerAngles = Utility.Vector3Multiply(t.eulerAngles, _transformRotation);
                    //t.localScale = Utility.Vector3Multiply(t.localScale, _transformScale);
                    break;
                case 3:
                    t.position = Utility.Vector3Divide(t.position, _transformPosition);
                    t.eulerAngles = Utility.Vector3Divide(t.eulerAngles, _transformRotation);
                    //t.localScale = Utility.Vector3Divide(t.localScale, _transformScale);
                    break;
                default:
                    Debug.Log("Unknown case");
                    break;
            }
        }
    }
    private void TransformStandard(KeyValuePair<Component, List<InspectorInfo>> componentInspector)
    {
        if (EditorApplication.isPlaying && !EditorApplication.isPaused) return;

        var component = componentInspector.Key;

        if (_transformLocal)
        {
            // Local Position
            component.transform.localPosition = EditorGUILayout.Vector3Field("Local Position",
                                                                             component.transform.localPosition);
            // Local Rotation
            if (_transformQuaternion)
            {
                var quat = component.transform.localRotation;
                var vec4 = new Vector4(quat.x, quat.y, quat.z, quat.w);
                var v = EditorGUILayout.Vector4Field("Local Rotation", vec4);
                component.transform.localRotation = new Quaternion(v.x, v.y, v.z, v.w);
            }
            else
            {
                component.transform.localEulerAngles = EditorGUILayout.Vector3Field("Local Euler Angles",
                                                                                    component.transform.
                                                                                        localEulerAngles);
            }
            // Local Scale
            component.transform.localScale = EditorGUILayout.Vector3Field("Local Scale",
                                                                          component.transform.localScale);
        }
        else
        {
            // Position
            component.transform.position = EditorGUILayout.Vector3Field("Position", component.transform.position);
            // Rotation
            if (_transformQuaternion)
            {
                var quat = component.transform.localRotation;
                var vec4 = new Vector4(quat.x, quat.y, quat.z, quat.w);
                var v = EditorGUILayout.Vector4Field("Rotation", vec4);
                component.transform.localRotation = new Quaternion(v.x, v.y, v.z, v.w);
            }
            else
            {
                component.transform.eulerAngles = EditorGUILayout.Vector3Field("Euler Angles",
                                                                               component.transform.eulerAngles);
            }
            // Scale
            var parentScale = Vector3.one;

            Transform parentTransform = null;
            if (component.transform.parent)
                parentTransform = component.transform.parent;

            while (parentTransform != null)
            {
                parentScale.x *= parentTransform.localScale.x;
                parentScale.y *= parentTransform.localScale.y;
                parentScale.z *= parentTransform.localScale.z;

                parentTransform = parentTransform.parent; // ? parentTransform.parent : null;
            }

            var scale = Vector3.zero;
            scale.x = component.transform.localScale.x*parentScale.x;
            scale.y = component.transform.localScale.y*parentScale.y;
            scale.z = component.transform.localScale.z*parentScale.z;

            scale = EditorGUILayout.Vector3Field("Scale", scale);
            scale.x /= parentScale.x;
            scale.y /= parentScale.y;
            scale.z /= parentScale.z;

            component.transform.localScale = scale;
        }

        BeginHorizontal();
        if (GUILayout.Button("Apply to Selection", EditorStyles.miniButtonLeft))
        {
            SaveMassUndo("Apply to Selection");
            SetAllSelection(componentInspector);
        }
        if (GUILayout.Button("Apply to Children", EditorStyles.miniButtonMid))
        {
            SaveMassUndo("Apply to Children");
            SetAllChildren(componentInspector);
        }
        if (GUILayout.Button("Apply to Both", EditorStyles.miniButtonRight))
        {
            SaveMassUndo("Apply to Both");
            SetAllBoth(componentInspector);
        }
        EndHorizontal();
        EditorGUILayout.Space();
    }

    private bool _isConstraint;
    private void RigidBodyField(KeyValuePair<Component, List<InspectorInfo>> componentInspector)
    {
        var component = componentInspector.Key;

        component.rigidbody.mass = EditorGUILayout.FloatField("Mass", component.rigidbody.mass);
        component.rigidbody.drag = EditorGUILayout.FloatField("Drag", component.rigidbody.drag);
        component.rigidbody.angularDrag = EditorGUILayout.FloatField("Angular Drag", component.rigidbody.angularDrag);
        component.rigidbody.useGravity = EditorGUILayout.Toggle("Use Gravity", component.rigidbody.useGravity);
        component.rigidbody.isKinematic = EditorGUILayout.Toggle("Is Kinematic", component.rigidbody.isKinematic);
        component.rigidbody.interpolation = (RigidbodyInterpolation)EditorGUILayout.EnumPopup("Interpolate", component.rigidbody.interpolation);
        component.rigidbody.collisionDetectionMode = (CollisionDetectionMode)EditorGUILayout.EnumPopup("Collision Detection", component.rigidbody.collisionDetectionMode);

        _isConstraint = EditorGUILayout.Foldout(_isConstraint, "Constraints");
        if (_isConstraint)
        {
            // horrible code
            var constaints = component.rigidbody.constraints;
            var pX = (constaints & RigidbodyConstraints.FreezePositionX) == RigidbodyConstraints.FreezePositionX;
            var pY = (constaints & RigidbodyConstraints.FreezePositionY) == RigidbodyConstraints.FreezePositionY;
            var pZ = (constaints & RigidbodyConstraints.FreezePositionZ) == RigidbodyConstraints.FreezePositionZ;
            var rX = (constaints & RigidbodyConstraints.FreezeRotationX) == RigidbodyConstraints.FreezeRotationX;
            var rY = (constaints & RigidbodyConstraints.FreezeRotationY) == RigidbodyConstraints.FreezeRotationY;
            var rZ = (constaints & RigidbodyConstraints.FreezeRotationZ) == RigidbodyConstraints.FreezeRotationZ;

            BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(20)); // bit of a hack, using guilayout instead of editorlayout so we can use .width
            GUILayout.Label("Freeze Position", GUILayout.Width(100));
            GUILayout.Label("X", GUILayout.Width(10));
            pX = EditorGUILayout.Toggle(pX, GUILayout.Width(16));
            GUILayout.Label("Y", GUILayout.Width(10));
            pY = EditorGUILayout.Toggle(pY, GUILayout.Width(16));
            GUILayout.Label("Z", GUILayout.Width(10));
            pZ = EditorGUILayout.Toggle(pZ, GUILayout.Width(16));
            EndHorizontal();

            BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(20)); // bit of a hack
            GUILayout.Label("Freeze Rotation", GUILayout.Width(100));
            GUILayout.Label("X", GUILayout.Width(10));
            rX = EditorGUILayout.Toggle(rX, GUILayout.Width(16));
            GUILayout.Label("Y", GUILayout.Width(10));
            rY = EditorGUILayout.Toggle(rY, GUILayout.Width(16));
            GUILayout.Label("Z", GUILayout.Width(10));
            rZ = EditorGUILayout.Toggle(rZ, GUILayout.Width(16));
            EndHorizontal();

            var newC = 0;
            newC += pX ? 1 << 1 : 0;
            newC += pY ? 1 << 2 : 0;
            newC += pZ ? 1 << 3 : 0;
            newC += rX ? 1 << 4 : 0;
            newC += rY ? 1 << 5 : 0;
            newC += rZ ? 1 << 6 : 0;

            component.rigidbody.constraints = (RigidbodyConstraints)newC;
        }
        BeginHorizontal();
        if (GUILayout.Button("Apply to Selection", EditorStyles.miniButtonLeft))
        {
            SaveMassUndo("Apply to Selection");
            SetAllSelection(componentInspector);
        }
        if (GUILayout.Button("Apply to Children", EditorStyles.miniButtonMid))
        {
            SaveMassUndo("Apply to Children");
            SetAllChildren(componentInspector);
        }
        if (GUILayout.Button("Apply to Both", EditorStyles.miniButtonRight))
        {
            SaveMassUndo("Apply to Both");
            SetAllBoth(componentInspector);
        }
        EndHorizontal();
        EditorGUILayout.Space();
    }
}