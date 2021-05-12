using System;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour{
    public static TileManager singleton;
    [HideInInspector]
    public Transform pathLayer;
    [HideInInspector]
    public List<Transform> tileLayers = new List<Transform>();
    [SerializeField]
    List<GameObject> tilePrefabs = new List<GameObject>();
    [SerializeField]
    List<GameObject> pathPrefabs = new List<GameObject>();
    public enum MarkerEnum {EntityPos, CanWalkYes, AttackRange, Attack};
    public enum PathEnum {Arrow, Curve, Line};
    private BlockData blockData;
    private Map map;

    private void Awake() {
        if (singleton != null) {
            Debug.LogWarning("Houve uma tentativa de setar 2 TileManagers");
            Destroy(this);
        }
        singleton = this;

        blockData = GameObject.FindGameObjectWithTag("DataHandler").GetComponent<BlockData>();
        map = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<Map>();

        InstantiateTilesLayers();
        NetworkSession.singleton.tileManager = this;
    }

    void InstantiateTilesLayers() {
        GameObject newObj = new GameObject("pathLayer");
        newObj.transform.SetParent(transform);
        pathLayer = newObj.transform;

        foreach (string markerName in Enum.GetNames(typeof(TileManager.MarkerEnum))) {
            newObj = new GameObject(markerName);
            newObj.transform.SetParent(transform);
            tileLayers.Add(newObj.transform);
        }
    }

    // Instancia marcador no cenário
    public void InstantiateTile(Vector2Int pos, TileManager.MarkerEnum tile){
        GameObject entityPosPath = Instantiate(tilePrefabs[(int)tile], new Vector3(pos.x + map.centerOffset, 1f + (((int) tile) + 1) / 1000f, pos.y + map.centerOffset), Quaternion.identity);
        entityPosPath.transform.SetParent(tileLayers[(int) tile]);
        entityPosPath.GetComponent<PathCoord>().coord = pos;
        entityPosPath.name = pos.x + "," + pos.y;
        entityPosPath.SetActive(true);
    }

    // Instancia caminho no cenário
    public void InstantiatePathTile(Vector2Int pos, VoxelData.MoveDirection dir, TileManager.PathEnum tile){
        GameObject pathTile = Instantiate(pathPrefabs[(int)tile], new Vector3(pos.x + map.centerOffset, 1f + (Enum.GetNames(typeof(TileManager.MarkerEnum)).Length + 1) / 1000f, pos.y + map.centerOffset), Quaternion.identity);
        pathTile.transform.SetParent(pathLayer);
        pathTile.SetActive(true);
        // Checa a direção da seta para efetuar rotação do objeto
        if(dir == VoxelData.MoveDirection.North) pathTile.transform.Rotate(90f, 0f, 90f, Space.World);
        else if(dir == VoxelData.MoveDirection.East) pathTile.transform.Rotate(90f, 0f, 180f, Space.World);
        else if(dir == VoxelData.MoveDirection.South) pathTile.transform.Rotate(90f, 0f, -90f, Space.World);
        else if(dir == VoxelData.MoveDirection.West) pathTile.transform.Rotate(90f, 0f, 0f, Space.World);
    }

    // Remove todos as instâncias de caminho do cenário
    public void ClearPathInstances(){
        foreach(Transform child in pathLayer)
            Destroy(child.gameObject);
    }

    // Remove todas as instâncias de marcador do cenário
    public void ClearInstances(){
        ClearInstances(TileManager.MarkerEnum.EntityPos);
        ClearInstances(TileManager.MarkerEnum.CanWalkYes);
        ClearInstances(TileManager.MarkerEnum.AttackRange);
        ClearInstances(TileManager.MarkerEnum.Attack);
    }

    // Remove todas as instâncias de marcador do cenário
    public void ClearInstances(TileManager.MarkerEnum tile){
        foreach(Transform child in tileLayers[(int) tile]) 
            Destroy(child.gameObject);  
    }
}
