using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockData : MonoBehaviour{
    public List<BlockType> blockList = new List<BlockType>();
    public List<GameObject> markerList = new List<GameObject>();
    public List<GameObject> pathList = new List<GameObject>();
    public enum BlockEnum {Ground, Stone, Air, Liquid};
    public enum MarkerEnum {EntityPos, CanWalkYes, CanWalkNo};
    public enum PathEnum {Arrow, Curve, Line};
}

[System.Serializable]
public class BlockType{
    public string blockName;
    public bool canWalk;
    public Sprite icon;
    
    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int getTextureID(VoxelData.Direction faceIndex){
        if(faceIndex == VoxelData.Direction.Back) return backFaceTexture;
        else if(faceIndex == VoxelData.Direction.Front) return frontFaceTexture;
        else if(faceIndex == VoxelData.Direction.Top) return topFaceTexture;
        else if(faceIndex == VoxelData.Direction.Bottom) return bottomFaceTexture;
        else if(faceIndex == VoxelData.Direction.Left) return leftFaceTexture;
        else return rightFaceTexture;
    }
}