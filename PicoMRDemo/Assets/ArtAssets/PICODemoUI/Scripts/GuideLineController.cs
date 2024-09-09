using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideLineController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform startPoint;
    public Transform endPoint;

    private void Update()
    {
        DrawLine();
    }

    private void DrawLine()
    {
        float numPoints = 5;
        lineRenderer.positionCount = 5;
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints-1);
            Vector3 pointPosition = Vector3.Lerp(startPoint.position, endPoint.position, t);
            lineRenderer.SetPosition(i, pointPosition);
        }
    }
}
