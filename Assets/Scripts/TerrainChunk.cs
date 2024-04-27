using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Vector2Int position;
    private Vector2Int size;
    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<int> triangles;
    
    public void Create(Vector3 worldPosition, Vector2Int chunkSize, Material material)
    {
        size = chunkSize;
        transform.position = worldPosition;
        
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
    }
    
    public void GenerateFlatMesh()
    {
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
        // meshFilter.mesh.GetVertices(vertices);
        
        for (int x = 0; x <= size.x; x++)
        {
            for (int z = 0; z <= size.y; z++)
            {
                int index = Space2DTo1D(x, z);
                float height = noise.Sample(x, z);
                vertices[index] = new Vector3(vertices[index].x, height, vertices[index].z);
            }
        }
        
        meshFilter.mesh.SetVertices(vertices);
    }

    public void CalculateNormals()
    {
        meshFilter.mesh.RecalculateNormals();
    }
    
    private int Space2DTo1D(int x, int z)
    {
        return (x * (size.y + 1)) + z;
    }
}
