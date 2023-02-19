using UnityEngine;
using TMPro;
using System.Diagnostics;
using NaughtyAttributes;

public sealed class DebugScreen : MonoBehaviour
{
    [SerializeField, Required]
    private TextMeshProUGUI performaceGUI;
    [SerializeField, Required]
    private TextMeshProUGUI playerGUI;
    [SerializeField, Required]
    private TextMeshProUGUI specsGUI;

    private float frameRate;
    private float minFrameRate;
    private float maxFrameRate;
    private float timer;
    private float minTimer;
    private float maxTimer;

    private string _applicationVersion;
    private string _unityVersion;
    private string _operatingSystem;
    private int _systemMemorySize;
    private string _processorType;
    private int _processorCount;
    private string _graphicsDeviceName;
    private string _graphicsDeviceVersion;

    private int _halfWorldSizeInVoxels;
    private int _halfWorldSizeInChunks;

    private const uint decimalPrecision = 4;

    private void Start()
    {
        minFrameRate = float.MaxValue;
        maxFrameRate = float.MinValue;
        minTimer = float.MaxValue;
        maxTimer = float.MinValue;

        _applicationVersion = Application.version;
        _unityVersion = Application.unityVersion;
        _operatingSystem = SystemInfo.operatingSystem;
        _systemMemorySize = SystemInfo.systemMemorySize;
        _processorCount = SystemInfo.processorCount;
        _processorType = SystemInfo.processorType;
        _graphicsDeviceName = SystemInfo.graphicsDeviceName;
        _graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;

        _halfWorldSizeInVoxels = Voxel.WorldSizeInVoxels / 2;
        _halfWorldSizeInChunks = Voxel.WorldSizeInChunks / 2;

        EventsManager.Instance.OnUpdatePerformanceDebugScreenUI += UpdatePerformanceGUIDebugText;
        EventsManager.Instance.OnUpdatePlayerDebugScreenUI += UpdatePlayerGUIDebugText;
        EventsManager.Instance.OnUpdateSpecsDebugScreenUI += UpdateSpecsGUIDebugText;

        EventsManager.Instance.UpdateDebugScreenUI(specsGUIArea: true);
    }

    private void Awake()
    {
        EventsManager.Instance.UpdateDebugScreenUI(performanceGUIArea: true, playerGUIArea: true, specsGUIArea: true);
    }

    private void Update()
    {
        EventsManager.Instance.UpdateDebugScreenUI(performanceGUIArea: true);

        if (timer > 1f)
        {
            frameRate = 1.0f / Time.unscaledDeltaTime;

            if (frameRate > maxFrameRate)
            {
                maxFrameRate = frameRate;
            }
            if (frameRate < minFrameRate)
            {
                minFrameRate = frameRate;
            }

            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;

            if (timer > maxTimer)
            {
                maxTimer = timer;
            }
            if (timer < minTimer)
            {
                minTimer = timer;
            }
        }
    }

    private void UpdatePerformanceGUIDebugText()
    {
        performaceGUI.text = $"Cubic {_applicationVersion} \n" +
                             $"FPS: {Round(frameRate, decimalPrecision / 2)} / {Round(minFrameRate, decimalPrecision / 2)} / {Round(maxFrameRate, decimalPrecision / 2)} \n" +
                             $"Delta: {Round(Time.deltaTime, decimalPrecision)} / {Round(minTimer, decimalPrecision)} / {Round(maxTimer, decimalPrecision)}";
    }

    private void UpdatePlayerGUIDebugText()
    {
        playerGUI.text = $"C: {GetChunksToCreateCount}/{GetEnabledActiveChunksCount} \n" +
                         "\n" +
                         $"XYZ: {GetPlayerPosition} \n" +
                         $"Block: {GetBlockPosition} \n" +
                         $"Chunk: {GetChunkCoords} \n" +
                         $"Facing: {GetFacingDirection} \n" +
                         $"{GetSelectedBlockName} \n" +
                         "\n" +
                         $"Player: {GetPlayerMovementStatus} \n" +
                         "\n" +
                         $"Light: {GetBlockLightLevel} \n" +
                         $"Biome: {GetBiomeName}";
    }

    private void UpdateSpecsGUIDebugText()
    {
        specsGUI.text = $"Unity: {_unityVersion} \n" +
                        $"OS: {_operatingSystem} \n" +
                        "\n" +
                        $"Mem: {GetCurrentMemoryUsage}/{_systemMemorySize} \n" +
                        $"CPU: {_processorCount}x {_processorType} \n" +
                        "\n" +
                        $"Display: {GetCurrentResolution} \n" +
                        $"GPU: {_graphicsDeviceName} \n" +
                        $"{_graphicsDeviceVersion} \n";
    }

    private int GetChunksToCreateCount
    {
        get
        {
            return World.Instance.GetChunksToCreateCount;
        }
    }

    private int GetEnabledActiveChunksCount
    {
        get
        {
            return World.Instance.GetEnabledActiveChunksCount;
        }
    }

    private string GetPlayerPosition
    {
        get
        {
            return $"{Round(World.Instance.GetPlayer.transform.position.x - _halfWorldSizeInVoxels, decimalPrecision)} / " +
                   $"{Round(Camera.main.transform.position.y, decimalPrecision)} / " +
                   $"{Round(World.Instance.GetPlayer.transform.position.z - _halfWorldSizeInVoxels, decimalPrecision)}";
        }
    }

    private string GetBlockPosition
    {
        get
        {
            return $"{Mathf.FloorToInt(World.Instance.GetPlayer.transform.position.x) - _halfWorldSizeInVoxels} / " +
                   $"{Mathf.FloorToInt(World.Instance.GetPlayer.transform.position.y)} / " +
                   $"{Mathf.FloorToInt(World.Instance.GetPlayer.transform.position.z) - _halfWorldSizeInVoxels}";
        }
    }

    private string GetChunkCoords
    {
        get
        {
            return $"{World.Instance.GetPlayerCurrentChunkCoords.X - _halfWorldSizeInChunks} / " +
                   $"{World.Instance.GetPlayerCurrentChunkCoords.Z - _halfWorldSizeInChunks}";
        }
    }

    private string GetFacingDirection
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

    private string GetPlayerMovementStatus
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

    private int GetBlockLightLevel
    {
        get
        {
            return 15;
        }
    }

    private string GetBiomeName
    {
        get
        {
            return World.Instance.GetBiome.biomeName;
        }
    }

    private string GetCurrentMemoryUsage
    {
        get
        {
            Process proc = Process.GetCurrentProcess();
            float memory = proc.PrivateMemorySize64 / (1024 * 1024);
            proc.Dispose();

            return memory.ToString();
        }
    }

    private string GetCurrentResolution
    {
        get
        {
            return Screen.currentResolution.ToString();
        }
    }

    private string GetSelectedBlockName
    {
        get
        {
            return World.Instance.GetPlayer.SelectedBlockName;
        }
    }

    // Helper function
    private float Round(float value, uint digits)
    {
        float mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }
}
