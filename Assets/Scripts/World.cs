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

    // Block faces order
    // Back, Front, Top, Bottom, Left, Right

    public byte backFaceTextureID;
    public byte frontFaceTextureID;
    public byte topFaceTextureID;
    public byte bottomFaceTextureID;
    public byte leftFaceTextureID;
    public byte rightFaceTextureID;

    public byte GetTextureID(byte faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
            {
                return backFaceTextureID;
            }
            case 1:
            {
                return frontFaceTextureID;
            }
            case 2:
            {
                return topFaceTextureID;
            }
            case 3:
            {
                return bottomFaceTextureID;
            }
            case 4:
            {
                return leftFaceTextureID;
            }
            case 5:
            {
                return rightFaceTextureID;
            }
            default:
            {
                Debug.LogError($"Texture ID for {blockName} is out of range.");
                return 9;
            }
        }
    }
}
