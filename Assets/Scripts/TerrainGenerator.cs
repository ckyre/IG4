using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int chunkSize = new Vector2Int(20, 20);
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private NoiseProfile noiseProfile;

    [Space] [SerializeField] private Transform viewer;

    private Dictionary<Vector2Int, TerrainChunk> visibleChunks = new Dictionary<Vector2Int, TerrainChunk>();
    
    void Start()
    {
    }

    private void Update()
    {
        List<Vector2Int> visibleChunksPositions = GetVisibleChunks(viewer.position);
        foreach (Vector2Int visibleChunkPosition in visibleChunksPositions)
        {
            if (visibleChunks.ContainsKey(visibleChunkPosition) == false)
            {
                TerrainChunk chunk = CreateChunk(visibleChunkPosition);
                visibleChunks.Add(visibleChunkPosition, chunk);
            }
        }
    }

    private TerrainChunk CreateChunk(Vector2Int chunkPosition)
    {
        GameObject chunkGameObject = new GameObject("Chunk");
        chunkGameObject.transform.parent = transform;
        
        TerrainChunk chunk = chunkGameObject.AddComponent<TerrainChunk>();
        Vector3 worldPosition = ChunkSpaceToWorldSpace(chunkPosition);
        chunk.Create(worldPosition, chunkSize, terrainMaterial);
        
        chunk.GenerateFlatMesh();

        return chunk;
    }

    private List<Vector2Int> GetVisibleChunks(Vector3 viewerWorldPosition)
    {
        List<Vector2Int> visibleChunks = new List<Vector2Int>();

        Vector2Int viewerChunkPosition = WorldSpaceToChunkSpace(viewerWorldPosition);
        visibleChunks.Add(viewerChunkPosition);
        
        // TODO: Add left, right, top, bottom chunks.

        return visibleChunks;
    }

    private Vector2Int WorldSpaceToChunkSpace(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / (chunkSize.x / 2.0f));
        int z = Mathf.FloorToInt(worldPosition.z / (chunkSize.y / 2.0f));
        return new Vector2Int(x, z);
    }

    private Vector3 ChunkSpaceToWorldSpace(Vector2Int chunkPosition)
    {
        float x = (chunkPosition.x * chunkSize.x) - (chunkSize.x / 2.0f);
        float z = (chunkPosition.y * chunkSize.y) - (chunkSize.y / 2.0f);
        return new Vector3(x, 0.0f, z);
    }
}
