using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class SplineCollider : MonoBehaviour
{
    private EdgeCollider2D[] edgeColliders;
    private SplineContainer splineContainer;
    [SerializeField] private int pointsCount = 100;

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

        edgeColliders = new EdgeCollider2D[splineContainer.Splines.Count];

        for (int i = 0; i < splineContainer.Splines.Count; i++)
        {
            Spline spline = splineContainer.Splines[i];

            GameObject splineObj = new GameObject($"SplineCollider_{i}");

            splineObj.transform.SetParent(transform.Find("SplineColliders"));
            splineObj.transform.localPosition = Vector3.zero;
            splineObj.transform.localRotation = Quaternion.identity;
            splineObj.transform.localScale = Vector3.one;


            edgeColliders[i] = splineObj.AddComponent<EdgeCollider2D>();

            Vector2[] points = new Vector2[pointsCount];

            for (int j = 0; j < pointsCount; j++)
            {
                float t = j / (float)(pointsCount - 1);
                Vector3 position = spline.EvaluatePosition(t);
                points[j] = new Vector2(position.x, position.y);
            }

            edgeColliders[i].points = points;
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