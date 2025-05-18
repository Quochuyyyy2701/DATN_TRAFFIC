using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(LineRenderer))]
public class WayPoint : CustomMonoBehaviour
{
    [SerializeField] protected Transform startPoint;
    [SerializeField] protected Transform endPoint;
    protected LineRenderer lineRenderer;

    public override void LoadComponent()
    {
        this.LoadPoint();
        this.LoadLineRenderer();
        this.DrawLine();
    }

    public void LoadPoint()
    {
        if (startPoint == null)
            startPoint = transform.Find("StartPoint");

        if (endPoint == null)
            endPoint = transform.Find("EndPoint");
    }

    public void LoadLineRenderer()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }

    public void DrawLine()
    {
        if (lineRenderer == null || startPoint == null || endPoint == null) return;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, endPoint.position);

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.red;
    }

    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(startPoint.position, endPoint.position);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.position, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPoint.position, 0.2f);

#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.fontStyle = FontStyle.Bold;

            Handles.Label(startPoint.position + Vector3.up * 0.3f, transform.parent.name, style);

#endif
        }
    }

    public Transform GetStartPoint() => startPoint;
    public Transform GetEndPoint() => endPoint;
}
