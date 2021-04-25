using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData{
    public enum Direction {Back, Front, Top, Bottom, Left, Right};
    public enum MoveDirection {North, East, West, South};
    public enum VoxelType {None, Block, Fluid};
    public static readonly int textureAtlasSizeInBlocks = 4;    
    public static float normalizedBlockTextureSize { get{return 1.0f/(float)textureAtlasSizeInBlocks;} }

        public static readonly Vector3[] voxelVertices = {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)
    };

    public static readonly Vector3[] faceChecks = {
        new Vector3( 0.0f,  0.0f, -1.0f),   // Back Face
        new Vector3( 0.0f,  0.0f,  1.0f),   // Front Face
        new Vector3( 0.0f,  1.0f,  0.0f),   // Top Face
        new Vector3( 0.0f, -1.0f,  0.0f),   // Bottom Face
        new Vector3(-1.0f,  0.0f,  0.0f),   // Left Face
        new Vector3( 1.0f,  0.0f,  0.0f)    // Right Face
    };

    public static readonly int[,] voxelTriangles = {
        {0, 3, 1, 2}, // Back Face
        {5, 6, 4, 7}, // Front Face
        {3, 7, 2, 6}, // Top Face
        {1, 5, 0, 4}, // Bottom Face
        {4, 7, 0, 3}, // Left Face
        {1, 2, 5, 6}  // Right Face
    };

    public static readonly Vector2[] voxelUvs = {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };

    public static readonly Vector2Int[] movements = {
        new Vector2Int(-1,  0), // North
        new Vector2Int( 0, -1), // East
        new Vector2Int( 0,  1), // West
        new Vector2Int( 1,  0)  // South
    };

    public static MoveDirection GetDirectionIndex(Vector2Int dir){
        if(dir.Equals(movements[(int)MoveDirection.West])) return MoveDirection.West;
        else if(dir.Equals(movements[(int)MoveDirection.East])) return MoveDirection.East;
        else if(dir.Equals(movements[(int)MoveDirection.South])) return MoveDirection.South;
        return MoveDirection.North;
    }

    public static MoveDirection GetCurveDirection(MoveDirection dir1, MoveDirection dir2){
        if((dir1 == MoveDirection.West && dir2 == MoveDirection.North) || (dir1 == MoveDirection.South && dir2 == MoveDirection.East)) 
            return MoveDirection.North;
        else if((dir1 == MoveDirection.East && dir2 == MoveDirection.North) || (dir1 == MoveDirection.South && dir2 == MoveDirection.West))
            return MoveDirection.West;
        else if((dir1 == MoveDirection.West && dir2 == MoveDirection.South) || (dir1 == MoveDirection.North && dir2 == MoveDirection.East))
            return MoveDirection.East; 
        else if((dir1 == MoveDirection.East && dir2 == MoveDirection.South) || (dir1 == MoveDirection.North && dir2 == MoveDirection.West))
            return MoveDirection.South;
        return MoveDirection.North;
    }
}
