using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class PropMap : MonoBehaviour{
    public (bool, byte)[,] propMap;
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
        propInstances = new Dictionary<Vector2Int, GameObject>();
        ReadFile();
        PopulatePropMap();
    }

    void ReadFile(){
        string filePath = MapList.singleton.curFilePath;
        // verifica se o arquivo existe 
        if(File.Exists(filePath)){
            propMap = new (bool, byte)[Map.singleton.mapRows, Map.singleton.mapCols];
            bool isPropOrigin;
            byte propID;
            int seekSize = 2 * sizeof(int) + (Map.singleton.mapRows * Map.singleton.mapCols * sizeof(byte));
            // lê o trecho do arquivo que contém os dados da posição dos props
            using(BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open))){
                reader.BaseStream.Seek(seekSize, SeekOrigin.Begin);
                for(int i = 0; i < Map.singleton.mapRows; i++){
                    for(int j = 0; j < Map.singleton.mapCols; j++){
                        isPropOrigin = reader.ReadBoolean();
                        propID = reader.ReadByte();
                        propMap[i, j] = (isPropOrigin, propID);
                    }
                }
            }
        }

        else throw new Exception("Arquivo de save do mapa não foi encontrado.");
    }

    void PopulatePropMap(){
        for(int i = 0; i < Map.singleton.mapRows; i++)
            for(int j = 0; j < Map.singleton.mapCols; j++)
                if(propMap[i, j].Item1) AddProp(new Vector2Int(i, j), propMap[i, j].Item2);
    }

    public void AddProp(Vector2Int pos, byte propID){
        if(!Map.singleton.IsPositionInMap(pos.x, pos.y)) return;
        if(propMap[pos.x, pos.y].Item2 == propID) return;
        RemoveProp(pos);

        float centerOffset = Map.singleton.centerOffset;
        int size_x = PropData.singleton.propList[propID].size.x;
        int size_y = PropData.singleton.propList[propID].size.y;

        GameObject propInstance = Instantiate(PropData.singleton.propList[propID].model);
        propMap[pos.x, pos.y].Item1 = true;
        for(int x = pos.x; x < pos.x + size_x; x++){
            for(int y = pos.y; y < pos.y + size_y; y++){
                if(!Map.singleton.IsPositionInMap(x, y)) continue;
                propMap[x, y].Item2 = propID;
                propInstances[new Vector2Int(x, y)] = propInstance;
            }
        }
        propInstances[pos].transform.SetParent(this.transform);
        propInstances[pos].transform.localPosition = new Vector3(pos.x + (size_x-1)/2f + centerOffset, 2*centerOffset, pos.y + (size_y-1)/2f + centerOffset);
    }

    public void RemoveProp(Vector2Int pos){
        if(!Map.singleton.IsPositionInMap(pos.x, pos.y)) return;
        GameObject prop;
        if(propMap[pos.x, pos.y].Item2 == (byte)PropData.PropEnum.None || !propInstances.TryGetValue(pos, out prop)) return;

        int size_x = PropData.singleton.propList[propMap[pos.x,pos.y].Item2].size.x;
        int size_y = PropData.singleton.propList[propMap[pos.x,pos.y].Item2].size.y;
        
        for(int x = pos.x - (size_x - 1); x < pos.x + size_x; x++)
            for(int y = pos.y - (size_y - 1); y < pos.y + size_y; y++)
                if(Map.singleton.IsPositionInMap(x, y) && propInstances.TryGetValue(new Vector2Int(x, y), out prop))
                    if(propInstances[pos] == propInstances[new Vector2Int(x, y)])
                        propMap[x, y] = (false, (byte)PropData.PropEnum.None);

        DestroyImmediate(propInstances[pos]);
    }
}