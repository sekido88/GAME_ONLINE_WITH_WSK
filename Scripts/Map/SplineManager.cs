using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> splineObjects = new List<GameObject>();
    [SerializeField] private Material trackMaterial;
    [SerializeField] private float lineWidth = 0.2f;

    private void Awake()
    {
        SetupSplines();
    }

    private void SetupSplines()
    {
        foreach (var splineObj in splineObjects)
        {
            // Thêm collider nếu chưa có
            if (!splineObj.GetComponent<EdgeCollider2D>())
            {
                splineObj.AddComponent<EdgeCollider2D>();
            }

            // Thêm SplineCollider script
            if (!splineObj.GetComponent<SplineCollider>())
            {
                splineObj.AddComponent<SplineCollider>();
            }

            // Thêm LineRenderer nếu cần visual
            LineRenderer line = splineObj.GetComponent<LineRenderer>();
            if (line == null)
            {
                line = splineObj.AddComponent<LineRenderer>();
                SetupLineRenderer(line);
            }
        }
    }

    private void SetupLineRenderer(LineRenderer line)
    {
        line.material = trackMaterial;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.numCapVertices = 10;
        line.numCornerVertices = 10;
    }


    public void AddSpline(GameObject splineObj)
    {
        if (!splineObjects.Contains(splineObj))
        {
            splineObjects.Add(splineObj);
            SetupSplines();
        }
    }
}
