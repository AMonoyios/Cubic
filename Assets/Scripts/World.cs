using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public sealed class World : Singleton<World>
{
    [SerializeField]
    private GameObject player;
    public Player GetPlayer { get; private set; }
    private Vector3 spawnPosition;

    [HorizontalLine]

    [SerializeField]
    private string seed;
    [SerializeField, Expandable]
    private BiomeProperties biome;
    public BiomeProperties GetBiome { get { return biome; } }

    [HorizontalLine]

    [SerializeField]
    private Material blocksMaterial;
    public Material GetBlocksMaterial { get { return blocksMaterial; } }

    [SerializeField]
    private BlockType[] blockTypes;
    public BlockType[] GetBlockTypes { get { return blockTypes; } }

    private readonly Chunk[,] chunks = new Chunk[Voxel.WorldSizeInChunks, Voxel.WorldSizeInChunks];

    private readonly List<ChunkCoords> activeChunks = new();
    public int GetEnabledActiveChunksCount { get; private set; }
    private ChunkCoords playerCurrentChunkCoords;
    public ChunkCoords GetPlayerCurrentChunkCoords { get { return playerCurrentChunkCoords; } }
    private ChunkCoords playerLastChunkCoords;

    private readonly List<ChunkCoords> chunksToCreate = new();
    public int GetChunksToCreateCount { get { return chunksToCreate.Count; } }
    private bool isCreatingChunks;

    [HorizontalLine]

    [SerializeField]
    private GameObject debugScreen;

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            seed = Noise.GetStringNoise();
        }
        Random.InitState(seed.GetHashCode());

        GenerateWorld();

        SpawnPlayer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }

        // HACK: Updates the active and enabled chunks count whenever a new chunk is loaded
        if (GetChunksToCreateCount != 0)
        {
            int count = 1;
            foreach (Transform chunk in transform)
            {
                if (chunk.gameObject.activeInHierarchy)
                {
                    count++;
                }
            }
            GetEnabledActiveChunksCount = count;

            EventsManager.Instance.UpdateDebugScreenUI();
        }
    }

    private void FixedUpdate()
    {
        playerCurrentChunkCoords = GetChunkCoordsFromVector3(GetPlayer.transform.position);

        if (!playerCurrentChunkCoords.Equals(playerLastChunkCoords))
        {
            UpdateViewDistance();
        }

        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine(CreateChunks());
        }
    }

    private void GenerateWorld()
    {
        for (int x = (Voxel.WorldSizeInChunks / 2) - Voxel.ViewDistanceInChunks; x < (Voxel.WorldSizeInChunks / 2) + Voxel.ViewDistanceInChunks; x++)
        {
            for (int z = (Voxel.WorldSizeInChunks / 2) - Voxel.ViewDistanceInChunks; z < (Voxel.WorldSizeInChunks / 2) + Voxel.ViewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new(x, z), true);
                activeChunks.Add(new(x, z));
            }
        }
    }

    private void SpawnPlayer()
    {
        spawnPosition = new(Voxel.WorldSizeInChunks * Voxel.ChunkWidth / 2.0f, Voxel.ChunkHeight - 50.0f, Voxel.WorldSizeInChunks * Voxel.ChunkWidth / 2.0f);

        GetPlayer = Instantiate(player, spawnPosition, Quaternion.identity).GetComponent<Player>();

        playerLastChunkCoords = GetChunkCoordsFromVector3(GetPlayer.transform.position);
    }

    private IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].X, chunksToCreate[0].Z].Init();
            chunksToCreate.RemoveAt(0);

            EventsManager.Instance.UpdateDebugScreenUI();

            yield return null;
        }

        isCreatingChunks = false;
    }

    private ChunkCoords GetChunkCoordsFromVector3(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / Voxel.ChunkWidth);
        int z = Mathf.FloorToInt(position.z / Voxel.ChunkWidth);

        return new(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / Voxel.ChunkWidth);
        int z = Mathf.FloorToInt(position.z / Voxel.ChunkWidth);

        return chunks[x, z];
    }

    private void UpdateViewDistance()
    {
        ChunkCoords coords = GetChunkCoordsFromVector3(GetPlayer.transform.position);
        playerLastChunkCoords = playerCurrentChunkCoords;

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
                        chunks[x, z] = new Chunk(new ChunkCoords(x, z), false);
                        chunksToCreate.Add(new(x, z));

                        EventsManager.Instance.UpdateDebugScreenUI();
                    }
                    else if (!chunks[x, z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                    }

                    activeChunks.Add(new(x, z));
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

    public bool CheckForVoxel (Vector3 position)
    {
        ChunkCoords chunk = new(position);

        if (!IsChunkInWorld(chunk) || position.y < 0 || position.y > Voxel.ChunkHeight)
        {
            return false;
        }

        if (chunks[chunk.X, chunk.Z] != null && chunks[chunk.X, chunk.Z].isVoxelMapPopulated)
        {
            return blockTypes[chunks[chunk.X, chunk.Z].GetVoxelFromGlobalVector3(position)].isSolid;
        }

        return blockTypes[GetVoxel(position)].isSolid;
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
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new(position.x, position.z), 0, biome.terrainScale)) + biome.solidGroundHeight;

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
            foreach (Vein vein in biome.veins)
            {
                if (yPos > vein.height.x && yPos < vein.height.y)
                {
                    if (Noise.Get3DPerlin(position, vein.noiseOffset, vein.scale, vein.threshold))
                    {
                        voxel = vein.blockID;
                    }
                }
            }
        }

        return voxel;
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
