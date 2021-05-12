using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockData : MonoBehaviour{
    public List<BlockType> blockList = new List<BlockType>();
    public List<FluidType> fluidList = new List<FluidType>();
    public enum BlockEnum {Default, Ground, Stone, Sand, RedSand, SandStone, Snow};
    public enum FluidEnum {Water, Lava};

    public void Start() {
        BlockContent.blockList = blockList;
        BlockContent.fluidList = fluidList;
    }
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

[System.Serializable]
public class FluidType : BlockType {
    public Material material;
}

[System.Serializable]
public class BlockContent {
    public static List<BlockType> blockList = null;
    public static List<FluidType> fluidList = null;
    public Entity entity = null;
    public Effect effect = null;
    public VoxelData.VoxelType voxelType = VoxelData.VoxelType.None;
    public int voxelIndex = -1;

    public BlockContent() {}
    public BlockContent(Entity entity, VoxelData.VoxelType voxelType, int index) {
        this.entity = entity;
        this.voxelType = voxelType;
        this.voxelIndex = index;
    }

    public BlockContent with(Entity entity) {
        BlockContent newOne = (BlockContent) this.MemberwiseClone();
        newOne.entity = entity;
        return newOne;
    }
    public bool canWalk() { 
        if(entity != null) return false;
        if(voxelType == VoxelData.VoxelType.Block) return blockList[voxelIndex].canWalk;
        if(voxelType == VoxelData.VoxelType.Fluid) return fluidList[voxelIndex].canWalk;
        return false;
    }

    public override string ToString() {
        return $"{{{entity}, {effect}}}";
    }
}