using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineDrawer : MonoBehaviour
{
    [Header("Các điểm điều khiển (Control Points)")]
    public Transform[] controlPoints;

    // Vẽ spline bằng Gizmos
    void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Length < 2) return;

        Gizmos.color = Color.green;  // Chọn màu bạn muốn (ở đây là màu xanh lá)

        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            if (controlPoints[i] && controlPoints[i + 1])
            {
                Gizmos.DrawLine(controlPoints[i].position, controlPoints[i + 1].position);
            }
        }
    }
}
