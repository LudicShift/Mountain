using UnityEngine;

[ExecuteInEditMode]
public class BoundaryNode : MonoBehaviour
{
    public int index;
    public EasyBoundary parent;
    private GameObject wallSegment;

    public void ApplyCollider(float height, Material _)
    {
        if (parent == null || parent.nodes.Count < 2) return;

        int nextIndex = (index + 1);
        if (nextIndex >= parent.nodes.Count)
        {
            if (parent.closedLoop)
                nextIndex = 0;
            else
                return;
        }

        var next = parent.nodes[nextIndex];
        if (next == null) return;

        if (wallSegment == null)
        {
            wallSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallSegment.name = $"Wall_{index}_to_{nextIndex}";
            wallSegment.transform.parent = this.transform;

            var col = wallSegment.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col); // 시각화 용도라 제거
        }

        Vector3 a = transform.position;
        Vector3 b = next.transform.position;
        Vector3 dir = b - a;
        Vector3 center = (a + b) / 2f;
        float length = dir.magnitude;

        wallSegment.transform.position = center;
        wallSegment.transform.rotation = Quaternion.LookRotation(dir);
        wallSegment.transform.localScale = new Vector3(0.02f, height, length);

        Material matToApply = IsEndConnection() ? parent.endMaterial : parent.material;
        var renderer = wallSegment.GetComponent<Renderer>();
        if (renderer != null && matToApply != null)
            renderer.sharedMaterial = matToApply;
    }

    private bool IsEndConnection()
    {
        if (parent == null || parent.nodes.Count < 2) return false;

        int nextIndex = (index + 1);
        if (nextIndex >= parent.nodes.Count)
        {
            if (parent.closedLoop)
                nextIndex = 0;
            else
                return false;
        }

        return (index == 0 && nextIndex == parent.nodes.Count - 1) ||
               (index == parent.nodes.Count - 1 && nextIndex == 0);
    }

    public void ConfirmFinalize(bool keepMesh)
    {
        if (wallSegment != null)
        {
            wallSegment.name = "BoundaryCollider";

            if (keepMesh)
            {
                wallSegment.transform.parent = null;
                var col = wallSegment.GetComponent<BoxCollider>();
                if (col == null) wallSegment.AddComponent<BoxCollider>();
            }
            else
            {
                var renderer = wallSegment.GetComponent<Renderer>();
                if (renderer != null) DestroyImmediate(renderer);
                var filter = wallSegment.GetComponent<MeshFilter>();
                if (filter != null) DestroyImmediate(filter);

                if (wallSegment.GetComponent<BoxCollider>() == null)
                    wallSegment.AddComponent<BoxCollider>();

                wallSegment.transform.parent = null;
            }

            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        UnityEditor.Undo.DestroyObjectImmediate(this);
#endif
    }

    public void SetVisibility(bool visible)
    {
        if (wallSegment == null) return;

        var renderer = wallSegment.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = visible;
    }
}
