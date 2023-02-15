using UnityEngine;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI leftText;
    [SerializeField]
    private TextMeshProUGUI rightText;

    private float frameRate;
    private float timer;

    private int halfWorldSizeInVoxels;
    private int halfWorldSizeInChunks;

    private const uint decimalPrecision = 4;

    private void Start()
    {
        halfWorldSizeInVoxels = Voxel.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = Voxel.WorldSizeInChunks / 2;
    }

    private void Update()
    {
        leftText.text = $"Cubic {Application.version} \n" +
                        $"{Round(frameRate, decimalPrecision)} fps \n" +
                        $"Local @ {Round(Time.deltaTime, decimalPrecision)} ms ticks \n" +
                        "\n" +
                        $"C: {World.Instance.GetChunksToCreateCount}/{World.Instance.GetEnabledActiveChunksCount} \n" +
                        "\n" +
                        $"XYZ: {PlayerPosition} \n" +
                        $"Block: {BlockPosition} \n" +
                        $"Chunk: {ChunkCoords} \n" +
                        $"Facing: {FacingDirection} \n" +
                        "\n" +
                        $"Player: {PlayerMovement} \n" +
                        "\n" +
                        $"Light: {15} \n" +
                        $"Biome: {World.Instance.GetBiome.biomeName}";

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
            frameRate = 1.0f / Time.unscaledDeltaTime;
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
            return $"{Round(World.Instance.GetPlayer.transform.position.x - halfWorldSizeInVoxels, decimalPrecision)} / " +
                $"{Round(Camera.main.transform.position.y, decimalPrecision)} / " +
                $"{Round(World.Instance.GetPlayer.transform.position.z - halfWorldSizeInVoxels, decimalPrecision)}";
        }
    }

    private string BlockPosition
    {
        get
        {
            return $"{Mathf.FloorToInt(World.Instance.GetPlayer.transform.position.x) - halfWorldSizeInVoxels} / " +
                $"{Mathf.FloorToInt(World.Instance.GetPlayer.transform.position.y)} / " +
                $"{Mathf.FloorToInt(World.Instance.GetPlayer.transform.position.z) - halfWorldSizeInVoxels}";
        }
    }

    private string ChunkCoords
    {
        get
        {
            return $"{World.Instance.GetPlayerCurrentChunkCoords.X - halfWorldSizeInChunks} / " +
                $"{World.Instance.GetPlayerCurrentChunkCoords.Z - halfWorldSizeInChunks}";
        }
    }

    private string FacingDirection
    {
        get
        {
            string direction;

            float rotation = World.Instance.GetPlayer.transform.rotation.eulerAngles.y;
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

            direction += $" ({Round(rotation, decimalPrecision)} / {Round(Camera.main.transform.rotation.eulerAngles.x, decimalPrecision)})";

            return direction;
        }
    }

    private string PlayerMovement
    {
        get
        {
            string movementStatus = "";
            movementStatus += World.Instance.GetPlayer.IsRunning ? "Running / " : "Walking / ";
            movementStatus += World.Instance.GetPlayer.IsCrouching ? "Crouching / " : "Standing / ";
            movementStatus += World.Instance.GetPlayer.IsFalling ? "Falling / " : "Stable / ";
            movementStatus += World.Instance.GetPlayer.IsJumping ? "Jumping" : "Grounded";
            return movementStatus;
        }
    }

    //TODO: memory usage not implemented yet
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

    private float Round(float value, uint digits)
    {
        float mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }
}
