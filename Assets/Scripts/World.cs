using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class World : MonoBehaviour
{
    [SerializeField]
    private Transform player;

    [SerializeField]
    private string seed;

    [SerializeField]
    private BiomeProperties biomes;

    [SerializeField]
    private Material blocksMaterial;
    public Material GetBlocksMaterial { get { return blocksMaterial; } }

    [SerializeField]
    private BlockType[] blockTypes;
    public BlockType[] GetBlockType { get { return blockTypes; } }

    private readonly Chunk[,] chunks = new Chunk[Voxel.WorldSizeInChunks, Voxel.WorldSizeInChunks];

    private readonly List<ChunkCoords> activeChunks = new();
    private ChunkCoords playerCurrentChunkCoord;
    private ChunkCoords playerLastChunkCoord;

    private Vector3 spawnPosition;

    private void Start()
    {
        spawnPosition = new(Voxel.WorldSizeInChunks * Voxel.ChunkWidth / 2.0f, Voxel.ChunkHeight - 50.0f, Voxel.WorldSizeInChunks * Voxel.ChunkWidth / 2.0f);

        if (string.IsNullOrWhiteSpace(seed))
        {
            seed = Noise.GetStringNoise();
        }
        Random.InitState(seed.GetHashCode());

        System.DateTime startTime = System.DateTime.Now;
        GenerateWorld();
        System.DateTime endTime = System.DateTime.Now;
        Debug.Log($"Generation took: {endTime.Subtract(startTime).Milliseconds} milliseconds");

        player.position = spawnPosition;
        playerLastChunkCoord = GetChunkCoordsFromVector3(player.position);
    }

    private void FixedUpdate()
    {
        playerCurrentChunkCoord = GetChunkCoordsFromVector3(player.position);

        if (!playerCurrentChunkCoord.Equals(playerLastChunkCoord))
        {
            UpdateViewDistance();
        }
    }

    private void GenerateWorld()
    {
        for (int x = (Voxel.WorldSizeInChunks / 2) - Voxel.ViewDistanceInChunks; x < (Voxel.WorldSizeInChunks / 2) + Voxel.ViewDistanceInChunks; x++)
        {
            for (int z = (Voxel.WorldSizeInChunks / 2) - Voxel.ViewDistanceInChunks; z < (Voxel.WorldSizeInChunks / 2) + Voxel.ViewDistanceInChunks; z++)
            {
                CreateNewChunk(x, z);
            }
        }
    }

    private ChunkCoords GetChunkCoordsFromVector3(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / Voxel.ChunkWidth);
        int z = Mathf.FloorToInt(position.z / Voxel.ChunkWidth);

        return new(x, z);
    }

    private void UpdateViewDistance()
    {
        ChunkCoords coords = GetChunkCoordsFromVector3(player.position);

        // list of all active chunks on screen
        List<ChunkCoords> prevActiveChunks = new(activeChunks);

        // Checking for chunks within the player's view distance
        for (int x = coords.X - Voxel.ViewDistanceInChunks; x < coords.X + Voxel.ViewDistanceInChunks; x++)
        {
            for (int z = coords.Z - Voxel.ViewDistanceInChunks; z < coords.Z + Voxel.ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new(x, z)))
                {
                    // Chunk is within the world but it is not generated yet 
                    if (chunks[x, z] == null)
                    {
                        CreateNewChunk(x, z);
                    }
                    else if (!chunks[x, z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                        activeChunks.Add(new(x, z));
                    }
                }

                // Chunks that are in the view distance will be removed from the list of previously active chunks 
                for (int i = 0; i < prevActiveChunks.Count; i++)
                {
                    if (prevActiveChunks[i].Equals(new(x, z)))
                    {
                        prevActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach (ChunkCoords chunkCoord in prevActiveChunks)
        {
            chunks[chunkCoord.X, chunkCoord.Z].IsActive = false;
        }
    }

    public bool CheckForVoxel(float xPos, float yPos, float zPos)
    {
        // Gets the lower right corner of the block
        int x = Mathf.FloorToInt(xPos);
        int y = Mathf.FloorToInt(yPos);
        int z = Mathf.FloorToInt(zPos);

        // Gets the chunk coordinates of that block
        int xChunk = x / Voxel.ChunkWidth;
        int zChunk = z / Voxel.ChunkWidth;

        x -= xChunk * Voxel.ChunkWidth;
        z -= zChunk * Voxel.ChunkWidth;

        return blockTypes[chunks[xChunk, zChunk].VoxelMap[x, y, z]].isSolid;
    }

    public byte GetVoxel(Vector3 position)
    {
        int yPos = Mathf.FloorToInt(position.y);

        // Default pass
        if (!IsVoxelInWorld(position))
        {
            return GetBlockIdByName("Air");
        }

        if (yPos == 0)
        {
            return GetBlockIdByName("Unbreakable");
        }

        // First pass
        int terrainHeight = Mathf.FloorToInt(biomes.terrainHeight * Noise.Get2DPerlin(new(position.x, position.z), 0, biomes.terrainScale)) + biomes.solidGroundHeight;

        byte voxel;

        if (yPos == terrainHeight)
        {
            voxel = GetBlockIdByName("Grass");
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxel = GetBlockIdByName("Dirt");
        }
        else if (yPos > terrainHeight)
        {
            return GetBlockIdByName("Air");
        }
        else
        {
            voxel = GetBlockIdByName("Stone");
        }

        // Second pass
        if (voxel == GetBlockIdByName("Stone"))
        {
            foreach (Vein lode in biomes.veins)
            {
                if (yPos > lode.height.x && yPos < lode.height.y)
                {
                    if (Noise.Get3DPerlin(position, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxel = lode.blockID;
                    }
                }
            }
        }

        return voxel;
    }

    private void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new(x, z), this);
        activeChunks.Add(new(x, z));
    }

    private bool IsChunkInWorld(ChunkCoords coords)
    {
        return coords.X > 0 && coords.X < Voxel.WorldSizeInChunks - 1 && coords.Z > 0 && coords.Z < Voxel.WorldSizeInChunks - 1;
    }

    private bool IsVoxelInWorld(Vector3 position)
    {
        return position.x >= 0 && position.x < Voxel.WorldSizeInVoxels && position.y >= 0 && position.y < Voxel.ChunkHeight && position.z >= 0 && position.z < Voxel.WorldSizeInVoxels;
    }

    public byte GetBlockIdByName(string blockName)
    {
        for (byte i = 0; i < blockTypes.Length; i++)
        {
            if (blockName == blockTypes[i].blockName)
            {
                return i;
            }
        }

        Debug.LogWarning($"Block: {blockName} was not found in the block types database, returning block with ID 0");
        return 0;
    }
}
