using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MapSave : MonoBehaviour{
    public class MapSaveData{
        private string mapName;
        private int mapRows;
        private int mapCols;

        public void Init(string mapName, int mapRows, int mapCols){
            // Armazena informações gerais do mapa
            this.mapName = mapName;
            this.mapRows = mapRows;
            this.mapCols = mapCols;
        }
    }

    public string filePath;
    public MapSaveData data = new MapSaveData();

    public void CreateFile(string name, int mapRows, int mapCols){
        data.Init(name, mapRows, mapCols);
        filePath = Application.persistentDataPath + "/SavedMaps/" + name + ".bin";

        if(!File.Exists(filePath)){
            // cria um arquivo para escrever
            using(BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create))){
                // tamanho do mapa
                writer.Write((Int32)mapRows);             
                writer.Write((Int32)mapCols);

                // escreve os dados de cada posição (mapa de voxels)            
                for(int i = 0; i < mapRows; i++){
                    for(int j = 0; j < mapCols; j++){
                        writer.Write((byte)VoxelData.VoxelType.Block);     // tipo do voxel
                        writer.Write((byte)BlockData.BlockEnum.Default);   // id do voxel
                    }
                }

                // escreve os dados de cada posição (mapa de props)
                for(int i = 0; i < mapRows; i++){
                    for(int j = 0; j < mapCols; j++){
                    writer.Write((bool)false);                   // ponto de origem do prop
                    writer.Write((byte)PropData.PropEnum.None);  // id do prop
                    }
                } 
            }	
        }

        else Debug.Log("Arquivo " + name + ".bin já existe.");
    }

    public void DeleteFile(){
        File.Delete(filePath);
    }
}