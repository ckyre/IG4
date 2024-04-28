using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int chunkSize = new Vector2Int(20, 20);
    [SerializeField] private NoiseProfile noiseProfile;
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Material waterMaterial;

    [Space] [SerializeField] private Transform viewer;

    private Dictionary<Vector2Int, TerrainChunk> chunksPool = new Dictionary<Vector2Int, TerrainChunk>();
    
    private void Update()
    {
        // Chunks visibility.
        
        Vector2Int currentChunkPosition = WorldSpaceToChunkSpace(viewer.position);

        if (chunksPool.ContainsKey(currentChunkPosition) == true)
        {
            // chunksPool[currentChunkPosition].UpdateHeight(noiseProfile);
        }
        
        List<Vector2Int> visiblePositions = new List<Vector2Int>();
        visiblePositions.Add(currentChunkPosition);
        visiblePositions.Add(new Vector2Int(currentChunkPosition.x + 1, currentChunkPosition.y));
        visiblePositions.Add(new Vector2Int(currentChunkPosition.x - 1, currentChunkPosition.y));
        visiblePositions.Add(new Vector2Int(currentChunkPosition.x, currentChunkPosition.y + 1));
        visiblePositions.Add(new Vector2Int(currentChunkPosition.x, currentChunkPosition.y - 1));
        
        visiblePositions.Add(new Vector2Int(currentChunkPosition.x + 1, currentChunkPosition.y + 1));
        visiblePositions.Add(new Vector2Int(currentChunkPosition.x - 1, currentChunkPosition.y + 1));
        visiblePositions.Add(new Vector2Int(currentChunkPosition.x + 1, currentChunkPosition.y - 1));
        visiblePositions.Add(new Vector2Int(currentChunkPosition.x - 1, currentChunkPosition.y - 1));
        
        List<Vector2Int> borderPositions = new List<Vector2Int>();
        borderPositions.Add(new Vector2Int(currentChunkPosition.x, currentChunkPosition.y + 2));
        borderPositions.Add(new Vector2Int(currentChunkPosition.x + 1, currentChunkPosition.y + 2));
        borderPositions.Add(new Vector2Int(currentChunkPosition.x - 1, currentChunkPosition.y + 2));
        borderPositions.Add(new Vector2Int(currentChunkPosition.x, currentChunkPosition.y - 2));
        borderPositions.Add(new Vector2Int(currentChunkPosition.x + 1, currentChunkPosition.y - 2));
        borderPositions.Add(new Vector2Int(currentChunkPosition.x - 1, currentChunkPosition.y - 2));

        // Pre-load surronding chunks.

        foreach (Vector2Int borderPosition in borderPositions)
        {
            if (chunksPool.ContainsKey(borderPosition) == false)
            {
                // Just load a flat plane.
                TerrainChunk chunk = CreateChunk(borderPosition);
                chunksPool.Add(borderPosition, chunk);
            }
        }
        
        // Load visible chunks.

        foreach (Vector2Int visiblePosition in visiblePositions)
        {
            if (chunksPool.ContainsKey(visiblePosition) == true)
            {
                // Fully load chunk.
                TerrainChunk chunk = chunksPool[visiblePosition];

                if (chunk.IsCompletlyLoaded() == false)
                {
                    chunk.UpdateHeight(noiseProfile);
                    chunk.GenerateWater(-2.0f, waterMaterial);
                    chunk.CalculateNormalsAndBounds();
                }
            }
            else
            {
                // If, for any reasons, the chunk was not pre-loaded, load it fully.
                TerrainChunk chunk = CreateChunk(visiblePosition);
                chunksPool.Add(visiblePosition, chunk);
                chunk.UpdateHeight(noiseProfile);
                chunk.GenerateWater(-2.0f, waterMaterial);
                chunk.CalculateNormalsAndBounds();
            }
        }
        
        // Delete all chunks that are away from view.
        
        List<KeyValuePair<Vector2Int, TerrainChunk>> chunksToDelete = chunksPool.Where(kv => 
            (visiblePositions.Contains(kv.Key) == false && borderPositions.Contains(kv.Key) == false)
        ).ToList();
        
        foreach (KeyValuePair<Vector2Int, TerrainChunk> kv in chunksToDelete)
        {
            Destroy(kv.Value.gameObject);
            chunksPool.Remove(kv.Key);
        }
        
    }

    private TerrainChunk CreateChunk(Vector2Int chunkPosition)
    {
        GameObject chunkGameObject = new GameObject("Chunk");
        chunkGameObject.transform.parent = transform;
        
        TerrainChunk chunk = chunkGameObject.AddComponent<TerrainChunk>();
        Vector3 chunkWorldPosition = ChunkSpaceToWorldSpace(chunkPosition);
        chunk.Create(chunkWorldPosition, chunkSize, terrainMaterial);
        chunk.GenerateFlatMesh();

        return chunk;
    }

    private Vector2Int WorldSpaceToChunkSpace(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x/ chunkSize.x);
        int z = Mathf.FloorToInt(worldPosition.z / chunkSize.y);
        return new Vector2Int(x, z);
    }

    private Vector3 ChunkSpaceToWorldSpace(Vector2Int chunkPosition)
    {
        float x = (chunkPosition.x * chunkSize.x);
        float z = (chunkPosition.y * chunkSize.y);
        return new Vector3(x, 0.0f, z);
    }
}
