using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour{
    public Transform markerInstanceList;
    public Transform pathInstanceList;
    private BlockData blockData;

    private void Start() {
        blockData = GameObject.FindGameObjectWithTag("DataHandler").GetComponent<BlockData>();
    }

    // Instancia marcador no cenário
    public void InstantiateMarkerTile(Vector2Int pos, BlockData.MarkerEnum tile){
        GameObject entityPosPath = Instantiate(blockData.markerList[(int)tile], new Vector3(pos.x + 0.5f, 1.001f, pos.y + 0.5f), Quaternion.identity);
        entityPosPath.GetComponent<PathCoord>().coord = pos;
        entityPosPath.name = pos.x + "," + pos.y;
        entityPosPath.transform.SetParent(markerInstanceList);
        entityPosPath.SetActive(true);
    }

    // Instancia caminho no cenário
    public void InstantiatePathTile(Vector2Int pos, VoxelData.MoveDirection dir, BlockData.PathEnum tile){
        GameObject pathTile = Instantiate(blockData.pathList[(int)tile], new Vector3(pos.x + 0.5f, 1.003f, pos.y + 0.5f), Quaternion.identity);
        pathTile.transform.SetParent(pathInstanceList);
        pathTile.SetActive(true);
        // Checa a direção da seta para efetuar rotação do objeto
        if(dir == VoxelData.MoveDirection.North) pathTile.transform.Rotate(90f, 0f, 90f, Space.World);
        else if(dir == VoxelData.MoveDirection.East) pathTile.transform.Rotate(90f, 0f, 180f, Space.World);
        else if(dir == VoxelData.MoveDirection.South) pathTile.transform.Rotate(90f, 0f, -90f, Space.World);
        else if(dir == VoxelData.MoveDirection.West) pathTile.transform.Rotate(90f, 0f, 0f, Space.World);
    }

    // Remove todos as instâncias de caminho do cenário
    public void ClearPathInstances(){
        GameObject[] allChildren = new GameObject[pathInstanceList.childCount];
        int childIndex = 0;

        foreach(Transform child in pathInstanceList){
            allChildren[childIndex] = child.gameObject;
            childIndex += 1;
        }

        foreach(GameObject child in allChildren) 
            DestroyImmediate(child.gameObject);  
    }

    // Remove todas as instâncias de marcador do cenário
    public void ClearMarkerInstances(){
        GameObject[] allChildren = new GameObject[markerInstanceList.childCount];
        int childIndex = 0;

        foreach(Transform child in markerInstanceList){
            allChildren[childIndex] = child.gameObject;
            childIndex += 1;
        }

        foreach(GameObject child in allChildren) 
            DestroyImmediate(child.gameObject);
    }
}
