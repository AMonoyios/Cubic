using UnityEngine;
using TMPro;
using System.Diagnostics;

public class DebugScreen : MonoBehaviour
{
    [SerializeField]
    private World world;
    [SerializeField]
    private TextMeshProUGUI leftText;
    [SerializeField]
    private TextMeshProUGUI rightText;

    private float frameRate;
    private float timer;

    private int halfWorldSizeInVoxels;
    private int halfWorldSizeInChunks;

    private void Start()
    {
        halfWorldSizeInVoxels = Voxel.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = Voxel.WorldSizeInChunks / 2;
    }

    private void Update()
    {
        leftText.text = $"Cubic {Application.version} \n" +
                        $"{frameRate} fps \n" +
                        $"Local @ {Round(Time.deltaTime, 4)} ms ticks \n" +
                        "\n" +
                        $"C: {world.GetChunksToCreateCount}/{world.GetActiveChunksCount} \n" +
                        "\n" +
                        $"XYZ: {PlayerPosition} \n" +
                        $"Block: {BlockPosition} \n" +
                        $"Chunk: {ChunkCoords} \n" +
                        $"Facing: {FacingDirection} \n" +
                        $"Light: {15} \n" +
                        $"Biome: {world.GetBiome.biomeName}";

        rightText.text = $"Unity: {Application.unityVersion} \n" +
                        $"OS: {SystemInfo.operatingSystem} \n" +
                        "\n" +
                        $"Mem: {CurrentMemoryUsage}/{SystemInfo.systemMemorySize} \n" +
                        $"CPU: {SystemInfo.processorCount}x {SystemInfo.processorType} \n" +
                        "\n" +
                        $"Display: {Screen.currentResolution} \n" +
                        $"GPU: {SystemInfo.graphicsDeviceName} \n" +
                        $"{SystemInfo.graphicsDeviceVersion}";

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    private string PlayerPosition
    {
        get
        {
            return $"{Round(world.GetPlayer.transform.position.x - halfWorldSizeInVoxels, 4)} / {Round(world.GetPlayer.transform.position.y - halfWorldSizeInVoxels, 4)} / {Round(world.GetPlayer.transform.position.z - halfWorldSizeInVoxels, 4)}";
        }
    }

    private string BlockPosition
    {
        get
        {
            return $"{Mathf.FloorToInt(world.GetPlayer.transform.position.x) - halfWorldSizeInVoxels} / {Mathf.FloorToInt(world.GetPlayer.transform.position.y)} / {Mathf.FloorToInt(world.GetPlayer.transform.position.z) - halfWorldSizeInVoxels}";
        }
    }

    private string ChunkCoords
    {
        get
        {
            return $"{world.GetPlayerCurrentChunkCoords.X - halfWorldSizeInChunks} / {world.GetPlayerCurrentChunkCoords.Z - halfWorldSizeInChunks}";
        }
    }

    private string FacingDirection
    {
        get
        {
            string direction;

            float rotation = world.GetPlayer.transform.rotation.eulerAngles.y;
            if (rotation < 0)
            {
                rotation += 360.0f;
            }

            if (0.0f <= rotation && rotation < 22.5f)
            {
                direction = "North";
            }
            else if (22.5f <= rotation && rotation < 67.5f)
            {
                direction = "North East";
            }
            else if (67.5f <= rotation && rotation < 112.5f)
            {
                direction = "East";
            }
            else if (112.5f <= rotation && rotation < 157.5f)
            {
                direction = "South East";
            }
            else if (157.5f <= rotation && rotation < 202.5f)
            {
                direction = "South";
            }
            else if (202.5f <= rotation && rotation < 247.5f)
            {
                direction = "South West";
            }
            else if (247.5f <= rotation && rotation < 292.5f)
            {
                direction = "West";
            }
            else if (292.5f <= rotation && rotation < 337.5f)
            {
                direction = "North West";
            }
            else if (337.5f <= rotation && rotation < 360.0f)
            {
                direction = "North";
            }
            else
            {
                direction = "Unknown";
            }

            direction += $"({Round(rotation, 2)} / {Round(world.GetPlayer.transform.rotation.eulerAngles.z, 2)}";

            return direction;
        }
    }

    private string CurrentMemoryUsage
    {
        get
        {
            return "TODO";
            // Process proc = Process.GetCurrentProcess();
            // float memory = proc.PrivateMemorySize64 / (1024 * 1024);
            // proc.Dispose();

            // return memory.ToString();
        }
    }

    private float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }
}
