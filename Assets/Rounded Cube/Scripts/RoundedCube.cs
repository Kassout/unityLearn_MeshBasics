using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoundedCube : MonoBehaviour
{
    public int width, height, length;
    public int roundness;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Color32[] cubeUV;

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
        CreateColliders();
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
        normals = new Vector3[vertices.Length];
        cubeUV = new Color32[vertices.Length];
        
        int v = 0;
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                SetVertex(v++, x, y, 0);
            }

            for (int z = 1; z <= length; z++)
            {
                SetVertex(v++, width, y, z);
            }

            for (int x = width - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, length);
            }

            for (int z = length - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
            }
        }

        for (int z = 1; z < length; z++)
        {
            for (int x = 1; x < width; x++)
            {
                SetVertex(v++, x, height, z);
            }
        }

        for (int z = 1; z < length; z++)
        {
            for (int x = 1; x < width; x++)
            {
                SetVertex(v++, x, 0, z);
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors32 = cubeUV;
    }

    private void SetVertex(int i, int x, int y, int z)
    {
        Vector3 inner = vertices[i] = new Vector3(x, y, z);

        if (x < roundness)
        {
            inner.x = roundness;
        }
        else if (x > width - roundness)
        {
            inner.x = width - roundness;
        }

        if (y < roundness)
        {
            inner.y = roundness;
        }
        else if (y > height - roundness)
        {
            inner.y = height - roundness;
        }
        
        if (z < roundness)
        {
            inner.z = roundness;
        }
        else if (z > length - roundness)
        {
            inner.z = length - roundness;
        }
        
        normals[i] = (vertices[i] - inner).normalized;
        vertices[i] = inner + normals[i] * roundness;
        cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }

    private void CreateTriangles()
    {
        // To texture the rounded cube without duplicating vertices
        // we multiple sub-meshes to create separate triangle lists that will use the same vertices
        int[] trianglesZ = new int[(width * height) * 12];
        int[] trianglesX = new int[(height * length) * 12];
        int[] trianglesY = new int[(width * length) * 12];
        
        // ring = cube perimeter || used as offset for vertices index to setup triangles
        int ring = (width + length) * 2;
        int tZ = 0, tX = 0, tY = 0, v = 0;

        for (int y = 0; y < height; y++, v++)
        {
            for (int q = 0; q < width; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }

            for (int q = 0; q < length; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            
            for (int q = 0; q < width; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }

            for (int q = 0; q < length - 1; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }

            tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
        }

        tY = CreateTopFace(trianglesY, tY, ring);
        tY = CreateBottomFace(trianglesY, tY, ring);
        
        mesh.subMeshCount = 3;
        mesh.SetTriangles(trianglesZ, 0);
        mesh.SetTriangles(trianglesX, 1);
        mesh.SetTriangles(trianglesY, 2);
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
    
    private void CreateColliders()
    {
        AddBoxCollider(width, height - roundness * 2, length - roundness * 2);
        AddBoxCollider(width - roundness * 2, height, length - roundness * 2);
        AddBoxCollider(width - roundness * 2, height - roundness * 2, length);
        
        Vector3 min = Vector3.one * roundness;
        Vector3 half = new Vector3(width, height, length) * 0.5f;
        Vector3 max = new Vector3(width, height, length) - min;
        
        AddCapsuleCollider(0, half.x, min.y, min.z);
        AddCapsuleCollider(0, half.x, min.y, max.z);
        AddCapsuleCollider(0, half.x, max.y, min.z);
        AddCapsuleCollider(0, half.x, max.y, max.z);
        
        AddCapsuleCollider(1, min.x, half.y, min.z);
        AddCapsuleCollider(1, min.x, half.y, max.z);
        AddCapsuleCollider(1, max.x, half.y, min.z);
        AddCapsuleCollider(1, max.x, half.y, max.z);
        
        AddCapsuleCollider(2, min.x, min.y, half.z);
        AddCapsuleCollider(2, min.x, max.y, half.z);
        AddCapsuleCollider(2, max.x, min.y, half.z);
        AddCapsuleCollider(2, max.x, max.y, half.z);
    }
    
    private void AddBoxCollider(float x, float y, float z)
    {
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(x, y, z);
    }

    private void AddCapsuleCollider(int direction, float x, float y, float z)
    {
        CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(x, y, z);
        collider.direction = direction;
        collider.radius = roundness;
        collider.height = collider.center[direction] * 2f;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }
        
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertices[i], 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vertices[i], normals[i]);
        }
    }
}