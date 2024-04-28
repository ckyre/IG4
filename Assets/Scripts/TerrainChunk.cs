using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private int status = 0;
    private Vector2Int size;
    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<int> triangles;

    private MeshFilter waterMeshFilter;
    
    public void Create(Vector3 chunkWorldPosition, Vector2Int chunkSize, Material material)
    {
        status = 0;
        size = chunkSize;
        
        transform.position = chunkWorldPosition;
        
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
    }
    
    public void GenerateFlatMesh()
    {
        status = 1;

        int verticesCount = (size.x + 1) * (size.y + 1);
        int trianglesCount = size.x * size.y * 6;
        vertices = new List<Vector3>(new Vector3[verticesCount]);
        uvs = new List<Vector2>(new Vector2[verticesCount]);
        triangles = new List<int>(new int[trianglesCount]);

        // Generate vertices and uvs.
        for (int x = 0; x <= size.x; x++)
        {
            for (int z = 0; z <= size.y; z++)
            {
                int index = Space2DTo1D(x, z);
                vertices[index] = new Vector3(x, 0.0f, z);
                uvs[index] = new Vector2(x, z);
            }
        }

        // Generate triangles.
        int tri = 0;
        int v = 0;
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                int a = Space2DTo1D(x, z);
                int b = a + 1;
                int c = Space2DTo1D(x + 1, z);
                int d = c + 1;
                
                triangles[tri] = a;
                triangles[tri + 1] = b;
                triangles[tri + 2] = d;
                triangles[tri + 3] = d;
                triangles[tri + 4] = c;
                triangles[tri + 5] = a;
            
                tri += 6;
                v++;
            }
        }
        
        // Update mesh.
        meshFilter.mesh.SetVertices(vertices);
        meshFilter.mesh.SetTriangles(triangles, 0);
        meshFilter.mesh.SetUVs(0, uvs);
    }

    public void UpdateHeight(NoiseProfile noise)
    {
        status = 2;
        
        for (int x = 0; x <= size.x; x++)
        {
            for (int z = 0; z <= size.y; z++)
            {
                int index = Space2DTo1D(x, z);
                float height = noise.Sample(transform.position.x + x, transform.position.z + z);
                vertices[index] = new Vector3(vertices[index].x, height, vertices[index].z);
            }
        }
        
        meshFilter.mesh.SetVertices(vertices);
    }

    public void GenerateWater(float waterHeight, Material waterMaterial)
    {
        status = 3;
        
        GameObject waterGameObject = new GameObject("Water");
        waterGameObject.transform.parent = transform;
        waterGameObject.transform.localPosition = new Vector3(0, waterHeight, 0);
        
        waterMeshFilter = waterGameObject.AddComponent<MeshFilter>();
        waterMeshFilter.mesh = new Mesh();
        waterMeshFilter.mesh.vertices = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(size.x, 0.0f, 0.0f),
            new Vector3(size.x, 0.0f, size.y),
            new Vector3(0.0f, 0.0f, size.y)
        };
        waterMeshFilter.mesh.triangles = new int[]
        {
            0, 3, 2,
            2, 1, 0
        };
        
        MeshRenderer waterMeshRenderer = waterGameObject.AddComponent<MeshRenderer>();
        waterMeshRenderer.material = waterMaterial;
    }

    public void CalculateNormalsAndBounds()
    {
        status = 4;
        
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateBounds();
        
        waterMeshFilter.mesh.RecalculateNormals();
        waterMeshFilter.mesh.RecalculateBounds();
    }

    public bool IsCompletlyLoaded()
    {
        return status == 4;
    }
    
    private int Space2DTo1D(int x, int z)
    {
        return (x * (size.y + 1)) + z;
    }
}
