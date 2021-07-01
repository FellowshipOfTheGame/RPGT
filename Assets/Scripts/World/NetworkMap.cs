using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class NetworkMap : NetworkBehaviour
{
    public static NetworkMap singleton;
    [SyncVar]
    public int mapRows;
    [SyncVar]
    public int mapCols;
    public SyncList<BlockContent> mapContent = new SyncList<BlockContent>();
    public SyncList<PropContent> propMap = new SyncList<PropContent>();
    void Awake() {
        if (singleton != null) {
            Debug.LogWarning("Houve uma tentativa de instanciar mais de um NetworkMap");
            Destroy(gameObject);
            return;
        }
        singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ReadFile(){
        string filePath = MapList.singleton.curFilePath;

        // verifica se o arquivo existe 
        if(File.Exists(filePath)){
            byte voxelType;
            byte voxelID;
            bool isPropOrigin;
            byte propID;

            // lê o trecho do arquivo que contém as dimensões do mapa e a posição dos voxels
            using(BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open))){
                mapRows = reader.ReadInt32();
                mapCols = reader.ReadInt32();
                for(int i = 0; i < mapRows; i++){
                    for(int j = 0; j < mapCols; j++){
                        voxelType = reader.ReadByte();
                        voxelID = reader.ReadByte();
                        mapContent.Add(new BlockContent(null, (VoxelData.VoxelType) voxelType, voxelID));
                    }
                }
                for(int i = 0; i < mapRows; i++){
                    for(int j = 0; j < mapCols; j++){
                        isPropOrigin = reader.ReadBoolean();
                        propID = reader.ReadByte();
                        propMap.Add(new PropContent(isPropOrigin, propID));
                    }
                }
            }
        }
        else throw new Exception("Arquivo de save do mapa não foi encontrado.");

        return;
    }

    public BlockContent GetMapContent(int i, int j) {
        return mapContent[(i * mapRows) + j];
    }

    public PropContent GetPropMap(int i, int j) {
        return propMap[(i * mapRows) + j];
    }

    [Command(requiresAuthority = false)]
    public void SetMapContent(int i, int j, BlockContent value) {
        mapContent[(i * mapRows) + j] = value;
    }

    [Command(requiresAuthority = false)]
    public void MoveEntity(Vector2Int from, Vector2Int to) {
        Entity entity = GetMapContent(from.x, from.y).entity;

        if (entity == null) {
            throw new System.Exception("Não há nenhuma entity em " + from);
        }

        SetMapContent(from.x, from.y, NetworkMap.singleton.GetMapContent(from.x, from.y).with(null));
        SetMapContent(to.x, to.y, NetworkMap.singleton.GetMapContent(to.x, to.y).with(entity));
    }

    public Vector2Int GetEmptyPosition() {
        for (int i = 0; i < mapRows; i++)
            for (int j = 0; j < mapCols; j++) 
                if (GetMapContent(i, j).canWalk())
                    return new Vector2Int(i, j);
        throw new System.Exception("Não há espaço vazio no mapa");
    }
}

public class PropContent {
    public byte index;
    public bool propOrigin;

    public PropContent() {
    }

    public PropContent(bool propOrigin, byte index) {
        this.propOrigin = propOrigin;
        this.index = index;
    }
}