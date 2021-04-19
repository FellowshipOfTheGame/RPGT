using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockData : MonoBehaviour{
    public List<BlockType> blockList = new List<BlockType>();

    public enum BlockEnum {Air, Default, Ground, Stone, Sand, RedSand};

    public void Start() {
        BlockContent.blockList = blockList;
    }
}

[System.Serializable]
public class BlockType{
    public string blockName;
    public bool canWalk;
    public bool isSolid;
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

[System.Serializable]
public class BlockContent {
    public static List<BlockType> blockList = null;
    public Entity entity = null;
    public Effect effect = null;
    public int blockTypeIndex = -1;

    public BlockContent() {}
    public BlockContent(Entity entity, int blockTypeIndex) {
        this.entity = entity;
        this.blockTypeIndex = blockTypeIndex;
    }

    public BlockContent with(Entity entity) {
        BlockContent newOne = (BlockContent) this.MemberwiseClone();
        newOne.entity = entity;
        return newOne;
    }
    public bool canWalk() { return entity == null && (blockTypeIndex != -1 || blockList[blockTypeIndex].canWalk); }

    public override string ToString()
    {
        return $"{{{entity}, {effect}}}";
    }
}