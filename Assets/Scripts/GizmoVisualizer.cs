using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoVisualizer : MonoBehaviour
{
    public Color color;
    public Vector3 gizmoPosition;
    public float radius;
    public GameObject player;

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Vector3 newPosition = transform.position + gizmoPosition;
        Gizmos.DrawWireSphere(newPosition, radius);

        if (player != null)
        {
            Vector3 start = transform.position;
            Vector3 end = transform.rotation * (-(player.transform.position - transform.position).normalized);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(new Ray(start, end));
        }
    }
}
