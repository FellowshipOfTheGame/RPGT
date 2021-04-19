using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour{
    public static TileManager singleton;
    public List<GameObject> markerList = new List<GameObject>();
    public List<GameObject> pathList = new List<GameObject>();
    public enum MarkerEnum {EntityPos, CanWalkYes, Attack, AttackRange};
    public enum PathEnum {Arrow, Curve, Line};
    public Transform markerInstanceList;
    public Transform pathInstanceList;
    public Transform attackInstanceList;
    private BlockData blockData;
    private Map map;

    private void Awake() {
        // Debug.Log("TileManager:12 - Awake()");
        if (singleton != null) {
            Debug.LogWarning("Houve uma tentativa de setar 2 TileManagers");
            Destroy(this);
        }
        singleton = this;

        blockData = GameObject.FindGameObjectWithTag("DataHandler").GetComponent<BlockData>();
        map = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<Map>();
    }

    // Instancia marcador no cenário
    public void InstantiateMarkerTile(Vector2Int pos, TileManager.MarkerEnum tile, bool useAttackInstanceList = false){
        GameObject entityPosPath;
        if (useAttackInstanceList) {
            entityPosPath = Instantiate(markerList[(int)tile], new Vector3(pos.x + map.centerOffset, 1.002f, pos.y + map.centerOffset), Quaternion.identity);
            entityPosPath.transform.SetParent(attackInstanceList);
        }
        else {
            entityPosPath = Instantiate(markerList[(int)tile], new Vector3(pos.x + map.centerOffset, 1.001f, pos.y + map.centerOffset), Quaternion.identity);
            entityPosPath.transform.SetParent(markerInstanceList);
        }
        entityPosPath.GetComponent<PathCoord>().coord = pos;
        entityPosPath.name = pos.x + "," + pos.y;
        entityPosPath.SetActive(true);
    }

    // Instancia caminho no cenário
    public void InstantiatePathTile(Vector2Int pos, VoxelData.MoveDirection dir, TileManager.PathEnum tile){
        GameObject pathTile = Instantiate(pathList[(int)tile], new Vector3(pos.x + map.centerOffset, 1.003f, pos.y + map.centerOffset), Quaternion.identity);
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

    // Remove todas as instâncias de marcador do cenário
    public void ClearAttackInstances(){
        GameObject[] allChildren = new GameObject[attackInstanceList.childCount];
        int childIndex = 0;

        foreach(Transform child in attackInstanceList){
            allChildren[childIndex] = child.gameObject;
            childIndex += 1;
        }

        foreach(GameObject child in allChildren) 
            DestroyImmediate(child.gameObject);
    }
}
