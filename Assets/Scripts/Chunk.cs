using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class Chunk
{
    public ChunkCoords coords;

    private GameObject chunk;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private int vertexIndex;
    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();
    private readonly List<Vector2> uvs = new();

    public byte[,,] VoxelMap { get; } = new byte[Voxel.ChunkWidth, Voxel.ChunkHeight, Voxel.ChunkWidth];

    private bool isActive;
    public bool isVoxelMapPopulated;

    public Chunk(ChunkCoords coords, bool generateOnLoad)
    {
        this.coords = coords;
        isActive = true;

        if (generateOnLoad)
        {
            Init();
        }
    }

    public void Init()
    {
        chunk = new();
        meshFilter = chunk.AddComponent<MeshFilter>();
        meshRenderer = chunk.AddComponent<MeshRenderer>();

        meshRenderer.material = World.Instance.GetBlocksMaterial;

        chunk.transform.SetParent(World.Instance.transform);

        chunk.transform.position = new(coords.X * Voxel.ChunkWidth, 0.0f, coords.Z * Voxel.ChunkWidth);
        chunk.name = $"Chunk {coords.X}, {coords.Z}";

        PopulateVoxelMap();
        CalculateMeshData();
        CreateMesh();
    }

    private void PopulateVoxelMap()
    {
        for (int y = 0; y < Voxel.ChunkHeight; y++)
        {
            for (int x = 0; x < Voxel.ChunkWidth; x++)
            {
                for (int z = 0; z < Voxel.ChunkWidth; z++)
                {
                    VoxelMap[x, y, z] = World.Instance.GetVoxel(new Vector3(x, y, z) + Position);
                }
            }
        }

        isVoxelMapPopulated = true;
    }

    private void CalculateMeshData()
    {
        for (int y = 0; y < Voxel.ChunkHeight; y++)
        {
            for (int x = 0; x < Voxel.ChunkWidth; x++)
            {
                for (int z = 0; z < Voxel.ChunkWidth; z++)
                {
                    if (World.Instance.GetBlockTypes[VoxelMap[x, y, z]].isSolid)
                    {
                        AddVoxelToChunk(new(x, y, z));
                    }
                }
            }
        }
    }

    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            isActive = value;
            if (chunk != null)
            {
                chunk.SetActive(value);
            }
        }
    }

    public Vector3 Position
    {
        get
        {
            return chunk.transform.position;
        }
    }

    private bool IsVoxelInChunk(int x, int y, int z)
    {
        return x >= 0 && x <= Voxel.ChunkWidth - 1 && y >= 0 && y <= Voxel.ChunkHeight - 1 && z >= 0 && z <= Voxel.ChunkWidth - 1;
    }

    private bool CheckVoxel(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            return World.Instance.CheckForVoxel(position + Position);
        }

        return World.Instance.GetBlockTypes[VoxelMap[x, y, z]].isSolid;
    }

    public byte GetVoxelFromGlobalVector3(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        x -= Mathf.FloorToInt(chunk.transform.position.x);
        z -= Mathf.FloorToInt(chunk.transform.position.z);

        return VoxelMap[x, y, z];
    }

    private void AddVoxelToChunk(Vector3 position)
    {
        for (byte i = 0; i < 6; i++)
        {
            if (!CheckVoxel(position + Voxel.faceChecks[i]))
            {
                byte blockID = VoxelMap[(int)position.x, (int)position.y, (int)position.z];

                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 0]]);
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 1]]);
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 2]]);
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 3]]);

                AddTexture(World.Instance.GetBlockTypes[blockID].GetTextureID(i));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }
    }

    private void CreateMesh()
    {
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private void AddTexture(int textureID)
    {
        float y = textureID / Voxel.TextureAtlasSizeInBlocks;
        float x = textureID - (y * Voxel.TextureAtlasSizeInBlocks);

        x *= Voxel.NormalizedBlockTextureSize;
        y *= Voxel.NormalizedBlockTextureSize;

        y = 1.0f - y - Voxel.NormalizedBlockTextureSize;

        uvs.Add(new(x, y));
        uvs.Add(new(x, y + Voxel.NormalizedBlockTextureSize));
        uvs.Add(new(x + Voxel.NormalizedBlockTextureSize, y));
        uvs.Add(new(x + Voxel.NormalizedBlockTextureSize, y + Voxel.NormalizedBlockTextureSize));
    }
}
