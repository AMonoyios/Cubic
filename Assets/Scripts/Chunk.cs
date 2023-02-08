using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk
{
    public ChunkCoords coords;

    private readonly GameObject chunk;

    private readonly MeshFilter meshFilter;
    private readonly MeshRenderer meshRenderer;

    private int vertexIndex = 0;
    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();
    private readonly List<Vector2> uvs = new();

    private readonly byte[,,] voxelMap = new byte[Voxel.ChunkWidth, Voxel.ChunkHeight, Voxel.ChunkWidth];

    private readonly World world;

    public Chunk(ChunkCoords coords, World world)
    {
        this.coords = coords;
        this.world = world;

        chunk = new GameObject();
        meshFilter = chunk.AddComponent<MeshFilter>();
        meshRenderer = chunk.AddComponent<MeshRenderer>();

        meshRenderer.material = world.GetBlocksMaterial;

        chunk.transform.SetParent(world.transform);

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
                    if (y < 1)
                    {
                        voxelMap[x, y, z] = 1;
                    }
                    else if (y == Voxel.ChunkHeight - 1)
                    {
                        voxelMap[x, y, z] = 3;
                    }
                    else
                    {
                        voxelMap[x, y, z] = 2;
                    }
                }
            }
        }
    }

    private void CalculateMeshData()
    {
        for (int y = 0; y < Voxel.ChunkHeight; y++)
        {
            for (int x = 0; x < Voxel.ChunkWidth; x++)
            {
                for (int z = 0; z < Voxel.ChunkWidth; z++)
                {
                    AddVoxelToChunk(new(x, y, z));
                }
            }
        }
    }

    private bool CheckVoxel(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        if (x < 0 || x > Voxel.ChunkWidth - 1 || y < 0 || y > Voxel.ChunkHeight - 1 || z < 0 || z > Voxel.ChunkWidth - 1)
        {
            return false;
        }

        return world.GetBlockType[voxelMap[x, y, z]].isSolid;
    }

    private void AddVoxelToChunk(Vector3 position)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!CheckVoxel(position + Voxel.faceChecks[i]))
            {
                byte blockID = voxelMap[(int)position.x, (int)position.y, (int)position.z];

                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 0]]);
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 1]]);
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 2]]);
                vertices.Add(position + Voxel.Verts[Voxel.Tris[i, 3]]);

                AddTexture(world.GetBlockType[blockID].GetTextureID((byte)i));

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
}

/// <summary>
///     Chunk position within the chunks map
/// </summary>
public class ChunkCoords
{
    public int X { get; }
    public int Z { get; }

    public ChunkCoords(int x, int z)
    {
        X = x;
        Z = z;
    }
}
