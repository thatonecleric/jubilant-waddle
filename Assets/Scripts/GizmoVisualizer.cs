using UnityEngine;

public class GizmoVisualizer : MonoBehaviour
{
    public Color color;
    public Vector3 gizmoPosition;
    public float radius;

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Vector3 newPosition = transform.position + gizmoPosition;
        Gizmos.DrawWireSphere(newPosition, radius);
    }
}
