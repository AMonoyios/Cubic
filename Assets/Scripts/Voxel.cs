using UnityEngine;

public static class Voxel
{
    public static readonly int ChunkWidth = 5;
    public static readonly int ChunkHeight = 10;

    public static readonly int WorldSizeInChunks = 4;

    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get
        {
            return 1.0f / TextureAtlasSizeInBlocks;
        }
    }

    public static readonly Vector3[] Verts = new Vector3[8]
    {
        new(0.0f, 0.0f, 0.0f),
        new(1.0f, 0.0f, 0.0f),
        new(1.0f, 1.0f, 0.0f),
        new(0.0f, 1.0f, 0.0f),
        new(0.0f, 0.0f, 1.0f),
        new(1.0f, 0.0f, 1.0f),
        new(1.0f, 1.0f, 1.0f),
        new(0.0f, 1.0f, 1.0f)
    };

    public static readonly int[,] Tris = new int[6,4]
    {
        // Offset order of duplicate vertices for shared tris
        // 0, 1, 2, 2, 1, 3

        {0, 3, 1, 2}, // Back
        {5, 6, 4, 7}, // Front
        {3, 7, 2, 6}, // Top
        {1, 5, 0, 4}, // Bottom
        {4, 7, 0, 3}, //Left
        {1, 2, 5, 6}  // Right
    };

    public static readonly Vector2[] UVs = new Vector2[4]
    {
        new(0.0f, 0.0f),
        new(0.0f, 1.0f),
        new(1.0f, 0.0f),
        new(1.0f, 1.0f)
    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new(0.0f, 0.0f, -1.0f),
        new(0.0f, 0.0f, 1.0f),
        new(0.0f, 1.0f, 0.0f),
        new(0.0f, -1.0f, 0.0f),
        new(-1.0f, 0.0f, 0.0f),
        new(1.0f, 0.0f, 0.0f)
    };
}
