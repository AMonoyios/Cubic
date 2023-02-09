using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Material blocksMaterial;
    public Material GetBlocksMaterial { get { return blocksMaterial; } }

    [SerializeField]
    private BlockType[] blockTypes;
    public BlockType[] GetBlockType { get { return blockTypes; } }

    private readonly Chunk[,] chunks = new Chunk[Voxel.WorldSizeInChunks, Voxel.WorldSizeInChunks];

    private Vector3 spawnPosition;

    private void Start()
    {
        spawnPosition = new(Voxel.WorldSizeInChunks * Voxel.ChunkWidth / 2.0f, Voxel.ChunkHeight + 2, Voxel.WorldSizeInChunks * Voxel.ChunkWidth / 2.0f);

        System.DateTime startTime = System.DateTime.Now;
        GenerateWorld();
        System.DateTime endTime = System.DateTime.Now;
        Debug.Log($"Generation took: {endTime.Subtract(startTime).Milliseconds} milliseconds");

        player.position = spawnPosition;
    }

    private void Update()
    {
        CheckViewDistance();
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

    private void CheckViewDistance()
    {
        ChunkCoords coords = GetChunkCoordsFromVector3(player.position);

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
                }
            }
        }
    }

    public byte GetVoxel(Vector3 position)
    {
        if (!IsVoxelInWorld(position))
        {
            return 0;
        }

        if (position.y < 1)
        {
            return 1;
        }
        else if (position.y == Voxel.ChunkHeight - 1)
        {
            return 3;
        }
        else
        {
            return 2;
        }
    }

    private void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new(x, z), this);
    }

    private bool IsChunkInWorld(ChunkCoords coords)
    {
        return coords.X > 0 && coords.X < Voxel.WorldSizeInChunks - 1 && coords.Z > 0 && coords.Z < Voxel.WorldSizeInChunks - 1;
    }

    private bool IsVoxelInWorld(Vector3 position)
    {
        return position.x >= 0 && position.x < Voxel.WorldSizeInVoxels && position.y >= 0 && position.y < Voxel.ChunkHeight && position.z >= 0 && position.z < Voxel.WorldSizeInVoxels;
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    // Block faces order
    // Back, Front, Top, Bottom, Left, Right

    public byte backFaceTextureID;
    public byte frontFaceTextureID;
    public byte topFaceTextureID;
    public byte bottomFaceTextureID;
    public byte leftFaceTextureID;
    public byte rightFaceTextureID;

    public byte GetTextureID(byte faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
            {
                return backFaceTextureID;
            }
            case 1:
            {
                return frontFaceTextureID;
            }
            case 2:
            {
                return topFaceTextureID;
            }
            case 3:
            {
                return bottomFaceTextureID;
            }
            case 4:
            {
                return leftFaceTextureID;
            }
            case 5:
            {
                return rightFaceTextureID;
            }
            default:
            {
                Debug.LogError($"Texture ID for {blockName} is out of range.");
                return 9;
            }
        }
    }
}
