using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "BiomeProperties", menuName = "Cubic/BiomeProperties")]
public sealed class BiomeProperties : ScriptableObject
{
    public string biomeName;
    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;
    public Vein[] veins;
}

[System.Serializable]
public class Vein
{
    public string veinName;
    public byte blockID;
    [MinMaxSlider(1, 255)]
    public Vector2Int height = new(1, 255);
    [Range(0.0f, 1.0f)]
    public float scale;
    [Range(0.0f, 1.0f)]
    public float threshold;
    public float noiseOffset;
}
