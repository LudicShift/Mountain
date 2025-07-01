using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EasyBoundary))]
public class EasyBoundaryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EasyBoundary boundary = (EasyBoundary)target;

        GUILayout.Space(10);
        GUILayout.Label("¢º Boundary Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Reconfigure"))
        {
            Undo.RecordObject(boundary, "Reconfigure Boundary");
            boundary.Reconfigure();
        }

        if (GUILayout.Button("Add Node"))
        {
            Undo.RecordObject(boundary, "Add Node");
            boundary.AddNode();
        }

        if (GUILayout.Button("Confirm"))
        {
            Undo.RecordObject(boundary, "Confirm Boundary");
            boundary.Confirm();
        }

        if (GUILayout.Button("Reset"))
        {
            Undo.RecordObject(boundary, "Reset Boundary");
            boundary.Reset();
        }
    }

    void OnSceneGUI()
    {
        EasyBoundary boundary = (EasyBoundary)target;

        if (boundary.nodes == null || boundary.nodes.Count < 2)
            return;

        for (int i = 0; i < boundary.nodes.Count; i++)
        {
            var node = boundary.nodes[i];
            if (node == null || node.transform == null)
                continue;

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(node.transform.position, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(node.transform, "Move Node");
                node.transform.position = newPos;
                boundary.UpdateColliders();
            }
        }
    }
}