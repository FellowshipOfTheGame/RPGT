using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MapSave : MonoBehaviour{
    public class MapSaveData{
        public string mapName;
        public int mapRows;
        public int mapCols;

        public void Init(string mapName, int mapRows, int mapCols){
            // Armazena informações gerais do mapa
            this.mapName = mapName;
            this.mapRows = mapRows;
            this.mapCols = mapCols;
        }
    }
    public MapSaveData data = new MapSaveData();

    public string DirectoryPath(){
        return Path.Combine(Application.persistentDataPath.Replace('/', Path.DirectorySeparatorChar), "SavedMaps");
    }

    public string FilePath(){
        return Path.Combine(DirectoryPath(), data.mapName + ".bin");
    }

    public void CreateFile(string name, int mapRows, int mapCols){
        data.Init(name, mapRows, mapCols);
        Debug.Log(FilePath());
        if(!File.Exists(FilePath())){
            Directory.CreateDirectory(DirectoryPath());
            // cria um arquivo para escrever
            using(BinaryWriter writer = new BinaryWriter(File.Open(FilePath(), FileMode.Create))){
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

        else throw new ArgumentException("Arquivo " + name + ".bin já existe.");
    }

    public void ReadFile(){
        string filePath = MapList.singleton.curFilePath;
        if(File.Exists(filePath)){
            string name = Path.GetFileNameWithoutExtension(filePath);
            int mapRows;
            int mapCols;
            using(BinaryReader br = new BinaryReader(File.Open(filePath, FileMode.Open))){
                mapRows = br.ReadInt32();
                mapCols = br.ReadInt32();
            }
            data.Init(name, mapRows, mapCols);
        }
    }

    public void SaveAll(){
        if(File.Exists(FilePath())){
            using(BinaryWriter writer = new BinaryWriter(File.Open(FilePath(), FileMode.Open))){
                // Não sobrescreve atributos de tamanho do mapa
                int seekSize = 2*sizeof(int);
                writer.Seek(seekSize, SeekOrigin.Begin);

                // Escreve os dados de cada posição (mapa de voxels)            
                for(int i = 0; i < data.mapRows; i++){
                    for(int j = 0; j < data.mapCols; j++){
                        writer.Write((byte)Map.singleton.voxelMap[i,j].Item1);   // tipo do voxel
                        writer.Write((byte)Map.singleton.voxelMap[i,j].Item2);   // id do voxel
                    }
                }

                // Escreve os dados de cada posição (mapa de props)
                for(int i = 0; i < data.mapRows; i++){
                    for(int j = 0; j < data.mapCols; j++){
                        writer.Write((bool)PropMap.singleton.propMap[i,j].Item1);  // ponto de origem do prop
                        writer.Write((byte)PropMap.singleton.propMap[i,j].Item2);  // id do prop
                    }
                }
            }   
        }

        else Debug.Log("Arquivo não encontrado");
    }

    public void DeleteFile(){
        File.Delete(FilePath());
    }
}