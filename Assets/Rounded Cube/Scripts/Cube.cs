using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    public int width, height, length;

    private Mesh mesh;
    private Vector3[] vertices;

    private void Awake()
    {
        Generate();
    }

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";
        CreateVertices();
        CreateTriangles();
    }

    private void CreateVertices()
    {
        int cornerVertices = 8;
        // edge vertices count = 4 width edges + 4 height edges + 4 length edges - 12 corners
        int edgeVertices = (width + height + length - 3) * 4;
        // face vertices count = 2 width faces + 2 height faces + 2 length faces - 12 edges
        int faceVertices = (
            (width - 1) * (height - 1) +
            (width - 1) * (length - 1) +
            (height - 1) * (length - 1)) * 2;
        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

        int v = 0;
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                vertices[v++] = new Vector3(x, y, 0);
            }

            for (int z = 1; z <= length; z++)
            {
                vertices[v++] = new Vector3(width, y, z);
            }

            for (int x = width - 1; x >= 0; x--)
            {
                vertices[v++] = new Vector3(x, y, length);
            }

            for (int z = length - 1; z > 0; z--)
            {
                vertices[v++] = new Vector3(0, y, z);
            }
        }

        for (int z = 1; z < length; z++)
        {
            for (int x = 1; x < width; x++)
            {
                vertices[v++] = new Vector3(x, height, z);
            }
        }

        for (int z = 1; z < length; z++)
        {
            for (int x = 1; x < width; x++)
            {
                vertices[v++] = new Vector3(x, 0, z);
            }
        }

        mesh.vertices = vertices;
    }

    private void CreateTriangles()
    {
        int quads = (width * height + width * length + height * length) * 2;
        // triangles = 2 triangles per quad
        int[] triangles = new int[quads * 6];
        // ring = cube perimeter || used as offset for vertices index to setup triangles
        int ring = (width + length) * 2;
        int t = 0, v = 0;

        for (int y = 0; y < height; y++, v++)
        {
            for (int q = 0; q < ring - 1; q++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
            }

            t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
        }

        t = CreateTopFace(triangles, t, ring);
        t = CreateBottomFace(triangles, t, ring);
        mesh.triangles = triangles;
    }
    
    private int CreateTopFace(int[] triangles, int t, int ring)
    {
        int v = ring * height;
        
        // first top face triangles row
        for (int x = 0; x < width - 1; x++, v++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

        // middle triangle rows
        int vMin = ring * (height + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        for (int z = 1; z < length - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + width - 1);
            for (int x = 1; x < width - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + width - 1, vMid + width);
            }
            t = SetQuad(triangles, t, vMid, vMax, vMid + width - 1, vMax + 1);

        }
        
        // last top face triangles row
        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < width - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

        return t;
    }

    private int CreateBottomFace(int[] triangles, int t, int ring)
    {
        int v = 1;
        
        // first bottom face triangles row
        int vMid = vertices.Length - (width - 1) * (length - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (int x = 1; x < width - 1; x++, v++, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

        // middle triangle rows
        int vMin = ring - 2;
        vMid -= width - 2;
        int vMax = v + 2;

        for (int z = 1; z < length - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + width - 1, vMin + 1, vMid);
            for (int x = 1; x < width - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid + width - 1, vMid + width, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + width - 1, vMax + 1, vMid, vMax);
        }

        // last bottom face triangles row
        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < width - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

        return t;
    }
    
    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }

        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}