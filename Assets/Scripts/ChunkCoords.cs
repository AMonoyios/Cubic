using UnityEngine;

public sealed class ChunkCoords
{
    public int X { get; }
    public int Z { get; }

    public ChunkCoords(int x = 0, int z = 0)
    {
        X = x;
        Z = z;
    }

    public ChunkCoords(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int z = Mathf.FloorToInt(position.z);

        X = x / Voxel.ChunkWidth;
        Z = z / Voxel.ChunkWidth;
    }

    public bool Equals(ChunkCoords other)
    {
        if (other == null)
        {
            return false;
        }
        else if (other.X == X && other.Z == Z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
