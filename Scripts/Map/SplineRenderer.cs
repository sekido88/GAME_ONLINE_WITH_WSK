using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
[RequireComponent(typeof(SplineContainer))]
public class SplineRenderer : MonoBehaviour
{
    private LineRenderer[] lineRenderers;
    private SplineContainer splineContainer;

    [Header("Track Visual Settings")]
    [SerializeField] private float trackWidth = 1f;
    [SerializeField] private Material trackMaterial;
    [SerializeField] private int pointsCount = 100;

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        SetupLineRenderer();
    }

    private void CleanupOldRenderers()
    {
        var children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            children[i] = transform.GetChild(i);
            
        foreach (Transform child in children)
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


    private bool CheckAmonutSplineRenderer() {
        return transform.Find("SplineRenderers").childCount == splineContainer.Splines.Count;
    }

    private void SetupLineRenderer()
    {
        if(splineContainer == null) return;

        if (CheckAmonutSplineRenderer()) return;

        lineRenderers = new LineRenderer[splineContainer.Splines.Count];

        for(int i = 0; i < splineContainer.Splines.Count; i++) 
        {
            GameObject splineObj = new GameObject($"SplineRenderer_{i}");
            splineObj.transform.SetParent(transform.Find("SplineRenderers"));

            splineObj.transform.localPosition = Vector3.zero;
            splineObj.transform.localRotation = Quaternion.identity;
            splineObj.transform.localScale = Vector3.one;

            LineRenderer lineRenderer = splineObj.AddComponent<LineRenderer>();
            lineRenderer.material = trackMaterial;
            lineRenderer.startWidth = trackWidth;
            lineRenderer.endWidth = trackWidth;
            lineRenderer.positionCount = pointsCount;
            lineRenderer.useWorldSpace = false;

            lineRenderers[i] = lineRenderer;

            Spline spline = splineContainer.Splines[i];
            for(int j = 0; j < pointsCount; j++) {
                float t = j / (float)(pointsCount - 1);
                Vector3 position = spline.EvaluatePosition(t);
                lineRenderers[i].SetPosition(j, position);
            }
        }

    }

    private void OnValidate()
    {
        SetupLineRenderer();
    }
}
