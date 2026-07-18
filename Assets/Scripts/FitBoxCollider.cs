using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider))]
public class FitBoxCollider : MonoBehaviour
{
    public void FitCollider()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = transform.InverseTransformVector(bounds.size);

        box.center = localCenter;
        box.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));

#if UNITY_EDITOR
        EditorUtility.SetDirty(box);
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FitBoxCollider))]
[CanEditMultipleObjects]
public class FitBoxColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Fit Box Collider to Children"))
        {
            // Loop through all currently selected objects in the Inspector
            foreach (Object targetObject in targets)
            {
                FitBoxCollider script = (FitBoxCollider)targetObject;
                BoxCollider box = script.GetComponent<BoxCollider>();

                if (box != null)
                {
                    Undo.RecordObject(box, "Fit Box Collider");
                    script.FitCollider();
                }
            }
        }
    }
}
#endif
