using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "BiomeProperties", menuName = "Cubic/BiomeProperties")]
public class BiomeProperties : ScriptableObject
{
    public string biomeName;
    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;
    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;
    [MinMaxSlider(1, 255)]
    public Vector2Int height = new(1, 255);
    [Range(0.0f, 1.0f)]
    public float scale;
    [Range(0.0f, 1.0f)]
    public float threshold;
    public float noiseOffset;
}
