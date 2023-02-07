using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField]
    private Material material;

    [SerializeField]
    private BlockType[] blockTypes;
    public BlockType[] GetBlockType { get { return blockTypes; } }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
}
