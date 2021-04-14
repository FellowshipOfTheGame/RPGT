using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour{
    // Variáveis para gerenciamento das informações gerais do mapa
    public int mapRows;
    public int mapCols;
    public float centerOffset;
    public bool isMapPopulated = false; 
    public int[,] voxelMap;
    private List<BlockType> blockList;

    // Variáveis para geração dos blocos do mapa
    private int vertexIndex;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    // Variáveis para controle da malha do mapa
    private GameObject mapObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    public Material material;

    void Awake(){
        voxelMap = new int[mapRows, mapCols];
        blockList = GameObject.Find("DataHandler").GetComponent<BlockData>().blockList;
        Init();
        centerOffset = gameObject.transform.localScale.x/2f;
    }

    // Inicializa variáveis da malha do mapa
    void Init(){
        mapObject = new GameObject();
        meshFilter = mapObject.AddComponent<MeshFilter>();
        meshRenderer = mapObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        mapObject.transform.position = new Vector3(0f, 0f, 0f);
        mapObject.name = "Map";

        ClearMeshData();
        PopulateMap();
    }

    // Atribui blocos para o mapa
    void PopulateMap(){
        for(int i = 0; i < mapRows; i++){
            for(int j = 0; j < mapCols; j++){
                voxelMap[i, j] = (int)BlockData.BlockEnum.Default;
                UpdateMeshData(new Vector2Int(i, j));
            }
        }
        isMapPopulated = true;
        CreateMesh();
    }

    // Atribui listas com as informações do objeto para a malha
    void CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;   
    }

    // Atualiza informações do objeto
    void UpdateMeshData(Vector2Int pos){
        Vector3 pos3 = new Vector3(pos.x, 0, pos.y);
        for(int i = 0; i < 6; i++){
            if(CheckVoxel(pos3 + VoxelData.faceChecks[i])){
                int blockID = voxelMap[pos.x, pos.y];
                for(int j = 0; j < 4; j++)
                    vertices.Add(pos3 + VoxelData.voxelVertices[VoxelData.voxelTriangles[i, j]]);

                AddTexture(blockList[blockID].getTextureID((VoxelData.Direction)i));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }
    }

    void AddTexture(int textureID){
        float y = textureID/VoxelData.textureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.textureAtlasSizeInBlocks); 

        x *= VoxelData.normalizedBlockTextureSize;
        y *= VoxelData.normalizedBlockTextureSize;
        y = 1.0f - y - VoxelData.normalizedBlockTextureSize;

        for(int i = 0; i < 4; i++)
            uvs.Add(new Vector2(x + (VoxelData.voxelUvs[i].x * VoxelData.normalizedBlockTextureSize), y + (VoxelData.voxelUvs[i].y * VoxelData.normalizedBlockTextureSize)));
    }

    // Checa validade do voxel de acordo com sua posição  
    bool CheckVoxel(Vector3 pos){
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        return IsFaceInMapBorder(x, y, z) || (IsPositionInMap(x,z) && !blockList[voxelMap[x,z]].isSolid);
    }

    // Checa se a face do voxel se encontra em uma das bordas do mapa
    bool IsFaceInMapBorder(int x, int y, int z){
        if(x == -1 || x == mapRows) return true;
        if(z == -1 || z == mapCols) return true;
        if(y == -1 || y == 1) return true;
        return false; 
    }

    // Remove todo o conteúdo das variáveis de geração dos blocos do mapa
    void ClearMeshData(){
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    // Verifica se a posição se encontra dentro do mapa
    public bool IsPositionInMap(int x, int y){
        if(x < 0 || x >= mapRows) return false; 
        if(y < 0 || y >= mapCols) return false;
        return true;
    }

    public void UpdateVoxel(Vector2Int coord, int newBlockID){
        voxelMap[coord.x, coord.y] = newBlockID;
        ClearMeshData();
        for(int i = 0; i < mapRows; i++)
            for(int j = 0; j < mapCols; j++)
                if(blockList[voxelMap[i,j]].isSolid)
                    UpdateMeshData(new Vector2Int(i,j));
        CreateMesh();
    }

    public void UpdateAllVoxels(int newBlockID){
        ClearMeshData();
        for(int i = 0; i < mapRows; i++){
            for(int j = 0; j < mapCols; j++){
                if(blockList[voxelMap[i,j]].isSolid){
                    voxelMap[i, j] = newBlockID;
                    UpdateMeshData(new Vector2Int(i,j));
                }
            }
        }
        CreateMesh();   
    }

    public void FloodFill(Vector2Int coord, int targetBlock, int newBlock){
        UpdateVoxel(coord, newBlock);
        Vector2Int neighbor;
        for(int i = 0; i < VoxelData.movements.Length; i++){
            neighbor = new Vector2Int(coord.x + VoxelData.movements[i].x, coord.y + VoxelData.movements[i].y);
            if(IsPositionInMap(neighbor.x, neighbor.y) && voxelMap[neighbor.x, neighbor.y] == targetBlock)
                FloodFill(neighbor, targetBlock, newBlock);
        }
    }
}