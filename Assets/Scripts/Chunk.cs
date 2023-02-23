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
    private readonly List<int> transparentTriangles = new();
    private readonly List<Vector2> uvs = new();
    private readonly Material[] materials = new Material[2];

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

        materials[0] = World.Instance.GetBlocksMaterial;
        materials[1] = World.Instance.GetTransparentBlocksMaterial;
        meshRenderer.materials = materials;

        chunk.transform.SetParent(World.Instance.transform);

        chunk.transform.position = new(coords.X * Voxel.ChunkWidth, 0.0f, coords.Z * Voxel.ChunkWidth);
        chunk.name = $"Chunk {coords.X}, {coords.Z}";

        PopulateVoxelMap();
        UpdateChunk();
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

    private void UpdateChunk()
    {
        ClearMeshData();

        for (int y = 0; y < Voxel.ChunkHeight; y++)
        {
            for (int x = 0; x < Voxel.ChunkWidth; x++)
            {
                for (int z = 0; z < Voxel.ChunkWidth; z++)
                {
                    if (World.Instance.GetBlockTypes[VoxelMap[x, y, z]].isSolid)
                    {
                        UpdateMeshData(new(x, y, z));
                    }
                }
            }
        }

        ApplyMesh();
    }

    private void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
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

    public void EditVoxel(Vector3 position, byte newBlockID)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        x -= Mathf.FloorToInt(chunk.transform.position.x);
        z -= Mathf.FloorToInt(chunk.transform.position.z);

        VoxelMap[x, y, z] = newBlockID;

        UpdateSurroundingVoxels(x, y, z);
        UpdateChunk();
    }

    private void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 voxel = new(x, y, z);

        // Finds the side of the voxel that is outside the current chunk and then update the chunk for the chunk next to it as well.
        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = voxel + Voxel.faceChecks[i];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                World.Instance.GetChunkFromVector3(currentVoxel + Position).UpdateChunk();
            }
        }
    }

    private bool CheckVoxel(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            return World.Instance.CheckIfVoxelTransparent(position + Position);
        }

        return World.Instance.GetBlockTypes[VoxelMap[x, y, z]].isTransparent;
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

    private void UpdateMeshData(Vector3 position)
    {
        byte blockID = VoxelMap[(int)position.x, (int)position.y, (int)position.z];
        bool isVoxelTransparent = World.Instance.GetBlockTypes[blockID].isTransparent;

        for (byte i = 0; i < 6; i++)
        {
            if (CheckVoxel(position + Voxel.faceChecks[i]))
            {
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 0]]);
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 1]]);
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 2]]);
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 3]]);

                float uvStartYPos = World.Instance.GetBlockTypes[blockID].GetTextureID(i) / Voxel.TextureAtlasSizeInBlocks;
                float uvStartXPos = World.Instance.GetBlockTypes[blockID].GetTextureID(i) - (uvStartYPos * Voxel.TextureAtlasSizeInBlocks);
                uvStartXPos *= Voxel.NormalizedBlockTextureSize;
                uvStartYPos *= Voxel.NormalizedBlockTextureSize;
                uvStartYPos = 1.0f - uvStartYPos - Voxel.NormalizedBlockTextureSize;
                uvs.Add(new(uvStartXPos, uvStartYPos));
                uvs.Add(new(uvStartXPos, uvStartYPos + Voxel.NormalizedBlockTextureSize));
                uvs.Add(new(uvStartXPos + Voxel.NormalizedBlockTextureSize, uvStartYPos));
                uvs.Add(new(uvStartXPos + Voxel.NormalizedBlockTextureSize, uvStartYPos + Voxel.NormalizedBlockTextureSize));

                if (!isVoxelTransparent)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                else
                {
                    transparentTriangles.Add(vertexIndex);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }

                vertexIndex += 4;
            }
        }
    }

    private void ApplyMesh()
    {
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            subMeshCount = 2,
            uv = uvs.ToArray()
        };
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
