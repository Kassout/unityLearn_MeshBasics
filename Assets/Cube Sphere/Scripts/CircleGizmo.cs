using UnityEngine;

public enum MappingType
{
    HETEROGENOUS,
    HOMOGENOUS
}

// Visualize the mapping from square to circle.
public class CircleGizmo : MonoBehaviour
{
    public int resolution;
    public MappingType mappingType;
    
    private void OnDrawGizmosSelected()
    {
        float step = 2f / resolution;
        for (int i = 0; i <= resolution; i++)
        {
            ShowPoint(i * step - 1f, -1f);
            ShowPoint(i * step - 1f, 1f);
        }

        for (int i = 0; i < resolution; i++)
        {
            ShowPoint(-1f, i * step - 1f);
            ShowPoint(1f, i * step - 1f);
        }
    }

    private void ShowPoint(float x, float y)
    {
        Vector2 square = new Vector2(x, y);
        Vector2 circle;
        if (mappingType == MappingType.HETEROGENOUS)
        {
            circle = square.normalized;
        }
        else
        {
            circle.x = square.x * Mathf.Sqrt(1f - Mathf.Pow(square.y, 2f) * 0.5f);
            circle.y = square.y * Mathf.Sqrt(1f - Mathf.Pow(square.x, 2f) * 0.5f);
        }
        
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(square, 0.025f);
        
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(circle, 0.025f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(square, circle);
        
        Gizmos.color = Color.grey;
        Gizmos.DrawLine(circle, Vector3.zero);
    }
}