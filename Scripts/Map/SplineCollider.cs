using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[ExecuteInEditMode]
public class SplineCollider : MonoBehaviour
{
    private EdgeCollider2D[] edgeColliders;
    private SplineContainer splineContainer;
    [SerializeField] private int pointsCount = 100;
    [SerializeField] private float thickness = 0.5f;

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        GenerateColliders();
    }

    private void CleanupOldColliders()
    {
        var children = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
            children[i] = transform.GetChild(i);

        foreach (Transform child in children)
        {
            if (child.name.StartsWith("SplineCollider_"))
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }


    private bool CheckAmountSplineCollider() {
        return transform.Find("SplineColliders").childCount == splineContainer.Splines.Count;
    }
    private void GenerateColliders()
    {
        if (splineContainer == null) return;
        if (CheckAmountSplineCollider()) return;

        edgeColliders = new EdgeCollider2D[splineContainer.Splines.Count * 2];

        for (int i = 0; i < splineContainer.Splines.Count; i++)
        {
            Spline spline = splineContainer.Splines[i];
            GameObject splineObj = new GameObject($"SplineCollider_{i}");
            
            splineObj.transform.SetParent(transform.Find("SplineColliders"));
            splineObj.transform.localPosition = Vector3.zero;
            splineObj.transform.localRotation = Quaternion.identity;
            splineObj.transform.localScale = Vector3.one;

            // Thêm Rigidbody2D để EdgeCollider hoạt động đúng
            Rigidbody2D rb = splineObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            Vector2[] topPoints = new Vector2[pointsCount];
            Vector2[] bottomPoints = new Vector2[pointsCount];

            for (int j = 0; j < pointsCount; j++)
            {
                float t = j / (float)(pointsCount - 1);
                Vector3 position = spline.EvaluatePosition(t);
                float3 tangentFloat3 = spline.EvaluateTangent(t);
                Vector3 tangent = new Vector3(tangentFloat3.x, tangentFloat3.y, tangentFloat3.z).normalized;
                Vector3 normal = new Vector3(-tangent.y, tangent.x, 0);

                topPoints[j] = position + normal * thickness;
                bottomPoints[j] = position - normal * thickness;
            }

            // Cạnh trên
            EdgeCollider2D topEdge = splineObj.AddComponent<EdgeCollider2D>();
            topEdge.points = topPoints;
            topEdge.isTrigger = false; // Đổi thành false để check va chạm thực
            edgeColliders[i * 2] = topEdge;

            // Cạnh dưới
            EdgeCollider2D bottomEdge = splineObj.AddComponent<EdgeCollider2D>();
            bottomEdge.points = bottomPoints;
            bottomEdge.isTrigger = false; // Đổi thành false để check va chạm thực
            edgeColliders[i * 2 + 1] = bottomEdge;

            // Thêm debug để kiểm tra va chạm
            // SplineColliderDebug debug = splineObj.AddComponent<SplineColliderDebug>();
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        GenerateColliders();
    }

    private void OnDestroy()
    {
        CleanupOldColliders();
    }
}

// Thêm class debug để kiểm tra va chạm
// public class SplineColliderDebug : MonoBehaviour
// {
//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         Debug.Log($"Collision with: {collision.gameObject.name}");
//     }

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         Debug.Log($"Trigger with: {other.gameObject.name}");
//     }
// }