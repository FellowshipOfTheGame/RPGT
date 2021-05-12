using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropMap : MonoBehaviour{
    private int[,] propMap;
    private Dictionary<Vector2Int, GameObject> propInstances;
    public static PropMap singleton;

    private void Awake() {
        if(singleton != null){
            Debug.LogWarning("Houve uma tentativa de criar 2 PropMaps");
            Destroy(this);
        }
        singleton = this;   
    }

    void Start(){
        propMap = new int[Map.singleton.mapRows, Map.singleton.mapCols];
        propInstances = new Dictionary<Vector2Int, GameObject>();

        for(int i = 0; i < Map.singleton.mapRows; i++)
            for(int j = 0; j < Map.singleton.mapCols; j++)
                propMap[i,j] = (int)PropData.PropEnum.None; 
    }

    public void AddProp(Vector2Int pos, int propID){
        if(!Map.singleton.IsPositionInMap(pos.x, pos.y)) return;
        if(propMap[pos.x, pos.y] == propID) return;
        RemoveProp(pos);

        float centerOffset = Map.singleton.centerOffset;
        int size_x = PropData.singleton.propList[propID].size.x;
        int size_y = PropData.singleton.propList[propID].size.y;

        GameObject propInstance = Instantiate(PropData.singleton.propList[propID].model);
        for(int x = pos.x; x < pos.x + size_x; x++){
            for(int y = pos.y; y < pos.y + size_y; y++){
                propMap[x, y] = propID;
                propInstances[new Vector2Int(x, y)] = propInstance;
            }
        }
        propInstances[pos].transform.SetParent(this.transform);
        propInstances[pos].transform.localPosition = new Vector3(pos.x + (size_x-1)/2f + centerOffset, 2*centerOffset, pos.y + (size_y-1)/2f + centerOffset);
    }

    public void RemoveProp(Vector2Int pos){
        if(!Map.singleton.IsPositionInMap(pos.x, pos.y)) return;
        GameObject prop;
        if(propMap[pos.x, pos.y] == (int)PropData.PropEnum.None || !propInstances.TryGetValue(pos, out prop)) return;

        int size_x = PropData.singleton.propList[propMap[pos.x,pos.y]].size.x;
        int size_y = PropData.singleton.propList[propMap[pos.x,pos.y]].size.y;
        
        for(int x = pos.x - (size_x - 1); x < pos.x + size_x; x++)
            for(int y = pos.y - (size_y - 1); y < pos.y + size_y; y++)
                if(Map.singleton.IsPositionInMap(x, y) && propInstances.TryGetValue(new Vector2Int(x, y), out prop))
                    if(propInstances[pos] == propInstances[new Vector2Int(x, y)])
                        propMap[x, y] = (int)PropData.PropEnum.None;

        DestroyImmediate(propInstances[pos]);
    }
}