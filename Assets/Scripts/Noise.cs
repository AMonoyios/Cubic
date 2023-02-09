using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    private const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public static string GetStringNoise(int length = 16)
    {
        char[] randomCharacters = new char[length];

        for (int i = 0; i < length; i++)
        {
            randomCharacters[i] = characters[Random.Range(0, characters.Length)];
        }

        return new string(randomCharacters);
    }

    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise(((position.x + 0.1f) / Voxel.ChunkWidth * scale) + offset, ((position.y + 0.1f) / Voxel.ChunkWidth * scale) + offset);
    }
}
