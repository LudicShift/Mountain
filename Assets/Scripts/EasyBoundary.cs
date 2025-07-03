using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class EasyBoundary : MonoBehaviour
{
    public float height = 3f;
    public bool visible = true;
    public bool closedLoop = true;
    public Material material;      // 일반 구간 머터리얼
    public Material endMaterial;   // 첫/끝 연결 구간 전용 머터리얼

    [HideInInspector] public List<BoundaryNode> nodes = new();

    void OnValidate()
    {
        RefreshNodes();
        UpdateColliders();
        UpdateVisibility();
    }

    void Awake()
    {
        RefreshNodes();
        UpdateColliders();
        UpdateVisibility();
    }

    public void Reconfigure()
    {
        RefreshNodes();
        UpdateColliders();
    }

    public void Reset()
    {
        ClearNodes();
        CreateSquare();
        RefreshNodes();
        UpdateColliders();
    }

    public void Confirm()
    {
        RefreshNodes();

        List<BoundaryNode> nodesCopy = new(nodes); // 복사본 사용
        foreach (var node in nodesCopy)
        {
            if (node != null)
                node.ConfirmFinalize(visible); // visible 상태 넘김
        }

        nodes.Clear();

#if UNITY_EDITOR
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("Node"))
                UnityEditor.Undo.DestroyObjectImmediate(child.gameObject);
        }

        UnityEditor.Undo.DestroyObjectImmediate(this);
#else
        DestroyImmediate(this);
#endif
    }

    public void AddNode()
    {
        RefreshNodes();
        if (nodes.Count < 2)
        {
            Debug.LogWarning("노드가 2개 이상 있어야 추가 가능합니다.");
            return;
        }

        var first = nodes[0];
        var last = nodes[nodes.Count - 1];
        Vector3 midpoint = (first.transform.position + last.transform.position) / 2f;

        GameObject node = new GameObject($"Node{transform.childCount}");
        node.transform.parent = transform;
        node.transform.position = midpoint;

        var bn = node.AddComponent<BoundaryNode>();
        bn.parent = this;

        RefreshNodes();
        UpdateColliders();
    }

    public void RefreshNodes()
    {
        nodes.Clear();
        foreach (Transform child in transform)
        {
            var node = child.GetComponent<BoundaryNode>();
            if (node != null && !nodes.Contains(node))
                nodes.Add(node);
        }

        // 숫자 추출 기반 정렬
        nodes.Sort((a, b) =>
        {
            int aNum = ExtractNumber(a.name);
            int bNum = ExtractNumber(b.name);
            return aNum.CompareTo(bNum);
        });
    }

    private int ExtractNumber(string name)
    {
        // "Node12" -> 12
        string numberPart = System.Text.RegularExpressions.Regex.Match(name, @"\d+").Value;
        return int.TryParse(numberPart, out int num) ? num : 0;
    }

    public void UpdateColliders()
    {
        RefreshNodes();
        for (int i = 0; i < nodes.Count; i++)
            nodes[i].index = i;

        foreach (var node in nodes)
            node.ApplyCollider(height, material);
    }

    public void UpdateVisibility()
    {
        foreach (var node in nodes)
            node.SetVisibility(visible);
    }

    private void ClearNodes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
#if UNITY_EDITOR
            UnityEditor.Undo.DestroyObjectImmediate(child.gameObject);
#else
            DestroyImmediate(child.gameObject);
#endif
        }
        nodes.Clear();
    }

    private void CreateSquare()
    {
        Vector3[] positions = new Vector3[] {
            new Vector3(-5, 0, -5),
            new Vector3(-5, 0, 5),
            new Vector3(5, 0, 5),
            new Vector3(5, 0, -5)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject node = new GameObject("Node" + i);
            node.transform.parent = transform;
            node.transform.localPosition = positions[i];

            var bn = node.AddComponent<BoundaryNode>();
            bn.parent = this;
        }
    }
}
