using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class Map : MonoBehaviour{
    // Variáveis para gerenciamento das informações gerais do mapa
    [HideInInspector]
    public int mapRows;
    [HideInInspector]
    public int mapCols;
    public float centerOffset;
    public bool isMapPopulated = false; 
    public (byte, byte)[,] voxelMap;
    private List<BlockType> blockList;
    private List<FluidType> fluidList;
    // Variáveis para geração dos blocos do mapa
    private int vertexIndex;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private int[] fluidVertexIndex;
    private List<Vector3>[] fluidVertices;
    private List<int>[] fluidTriangles;
    private List<Vector2>[] fluidUvs;

    // Variáveis para controle da malha do mapa
    private GameObject mapObject;
    private GameObject blockObject;
    private GameObject fluidObject;
    private GameObject[] fluidInstances;

    private MeshRenderer blockMeshRenderer;
    private MeshFilter blockMeshFilter;
    private MeshRenderer[] fluidMeshRenderer;
    private MeshFilter[] fluidMeshFilter;
    public Material material;

    public static Map singleton;

    void Awake() {
        if(singleton != null) {
            Debug.LogWarning("Houve uma tentativa de criar 2 Maps");
            Destroy(this);
        }

        singleton = this;
        blockList = GameObject.Find("DataHandler").GetComponent<BlockData>().blockList;
        fluidList = GameObject.Find("DataHandler").GetComponent<BlockData>().fluidList;
        // Instancia as listas para controle da renderização dos fluidos
        fluidInstances = new GameObject[fluidList.Count];
        fluidMeshFilter = new MeshFilter[fluidList.Count];
        fluidMeshRenderer = new MeshRenderer[fluidList.Count];
        // Instancia os arrays para controle das malhas dos fluidos
        fluidVertexIndex = new int[fluidList.Count];
        fluidVertices = new List<Vector3>[fluidList.Count];
        fluidTriangles = new List<int>[fluidList.Count];
        fluidUvs = new List<Vector2>[fluidList.Count];
        // Instancia o objeto para controle dos componentes do mapa
        mapObject = new GameObject();
        mapObject.transform.position = new Vector3(0f, 0f, 0f);
        mapObject.name = "Map";
        // Inicializa as partes do mapa
        InitBlocks();
        InitFluids();
        PopulateMap();
        // Gambiarra    
        UpdateVoxel(new Vector2Int(0,0), voxelMap[0,0].Item1, voxelMap[0,0].Item2);
        centerOffset = gameObject.transform.localScale.x/2f;
    }

    // Inicializa variáveis da malha do mapa relacionadas aos blocos
    void InitBlocks(){
        blockObject = new GameObject();
        blockMeshFilter = blockObject.AddComponent<MeshFilter>();
        blockMeshRenderer = blockObject.AddComponent<MeshRenderer>();
        blockMeshRenderer.material = material;

        blockObject.transform.position = new Vector3(0f, 0f, 0f);
        blockObject.name = "Blocks";
        blockObject.transform.SetParent(mapObject.transform);

        ClearBlockMeshData();
    }

    // Inicializa variáveis da malha do mapa relacionadas aos fluídos
    void InitFluids(){
        fluidObject = new GameObject();
        fluidObject.transform.position = new Vector3(0f, 0f, 0f);
        fluidObject.name = "Fluids";
        fluidObject.transform.SetParent(mapObject.transform);

        for(int i = 0; i < fluidList.Count; i++){
            fluidInstances[i] = new GameObject();
            fluidInstances[i].transform.position = new Vector3(0f, 0f, 0f);
            fluidInstances[i].name = fluidList[i].blockName;
            fluidInstances[i].transform.SetParent(fluidObject.transform);

            fluidMeshFilter[i] = fluidInstances[i].AddComponent<MeshFilter>();
            fluidMeshRenderer[i] = fluidInstances[i].AddComponent<MeshRenderer>();
            fluidMeshRenderer[i].material = fluidList[i].material;

            fluidVertices[i] = new List<Vector3>();
            fluidTriangles[i] = new List<int>();
            fluidUvs[i] = new List<Vector2>();          
        }
    }

    // Atribui blocos para o mapa
    void PopulateMap(){
        NetworkMap networkMap = NetworkMap.singleton;

        mapRows = networkMap.mapRows;
        mapCols = networkMap.mapCols;

        voxelMap = new (byte, byte)[mapRows, mapCols];
        for(int i = 0; i < mapRows; i++)
            for(int j = 0; j < mapCols; j++) {
                BlockContent current = networkMap.GetMapContent(i, j);
                if (current == null) {
                    Debug.Log("Current é null");
                }
                voxelMap[i, j] = ((byte) current.voxelType, (byte) current.voxelIndex);
                Debug.Log("Interpretado: " + i + " " + j + ": " + ((byte) current.voxelType) + " " + ((byte) current.voxelIndex));
                UpdateMeshData(new Vector2Int(i, j));
            }
        isMapPopulated = true;
        CreateBlockMesh();
    }

    // Atribui listas com as informações do objeto para a malha
    void CreateBlockMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        blockMeshFilter.mesh = mesh;   
    }

    void CreateFluidMesh(){
        Mesh[] mesh = new Mesh[fluidList.Count];
        for(int i = 0; i < fluidList.Count; i++){
            mesh[i] = new Mesh();
            mesh[i].vertices = fluidVertices[i].ToArray();
            mesh[i].triangles = fluidTriangles[i].ToArray();
            mesh[i].uv = fluidUvs[i].ToArray();

            mesh[i].RecalculateNormals();
            fluidMeshFilter[i].mesh = mesh[i]; 
        } 
    }

    // Atualiza informações do objeto
    void UpdateMeshData(Vector2Int pos){
        Vector3 pos3 = new Vector3(pos.x, 0, pos.y);
        int voxelType = voxelMap[pos.x, pos.y].Item1;
        int voxelID = voxelMap[pos.x, pos.y].Item2;

        for(int i = 0; i < 6; i++){
            if(CheckVoxel(pos3, pos3 + VoxelData.faceChecks[i])){
                for(int j = 0; j < 4; j++)
                    vertices.Add(pos3 + VoxelData.voxelVertices[VoxelData.voxelTriangles[i, j]]);

                if(voxelType == (byte)VoxelData.VoxelType.Block) AddTexture(blockList[voxelID].getTextureID((VoxelData.Direction)i));
                else if(voxelType == (byte)VoxelData.VoxelType.Fluid) AddTexture(fluidList[voxelID].getTextureID((VoxelData.Direction)i));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }

        // Caso seja um fluído, cria a malha da parte superior
        if(voxelType == (byte)VoxelData.VoxelType.Fluid){
            float vertice_x;
            float vertice_y;
            for(int i = 0; i < 4; i++){
                fluidVertices[voxelID].Add(pos3 + VoxelData.voxelVertices[VoxelData.voxelTriangles[(byte)VoxelData.Direction.Top, i]]);
                vertice_x = fluidVertices[voxelID][fluidVertices[voxelID].Count - 1].x;
                vertice_y = fluidVertices[voxelID][fluidVertices[voxelID].Count - 1].z;
                fluidUvs[voxelID].Add(new Vector2(vertice_x, vertice_y));
            }

            fluidTriangles[voxelID].Add(fluidVertexIndex[voxelID]);
            fluidTriangles[voxelID].Add(fluidVertexIndex[voxelID] + 1);
            fluidTriangles[voxelID].Add(fluidVertexIndex[voxelID] + 2);
            fluidTriangles[voxelID].Add(fluidVertexIndex[voxelID] + 2);
            fluidTriangles[voxelID].Add(fluidVertexIndex[voxelID] + 1);
            fluidTriangles[voxelID].Add(fluidVertexIndex[voxelID] + 3);

            fluidVertexIndex[voxelID] += 4;
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
    bool CheckVoxel(Vector3 pos, Vector3 facePos){
        int face_x = Mathf.FloorToInt(facePos.x);
        int face_y = Mathf.FloorToInt(facePos.y);
        int face_z = Mathf.FloorToInt(facePos.z);

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if(IsPositionInMap(face_x, face_z) && voxelMap[face_x,face_z].Item1 == (byte)VoxelData.VoxelType.Fluid && face_y == 1) 
            return false;
        if(voxelMap[x, z].Item1 == (byte)VoxelData.VoxelType.Block)
            return IsFaceInMapBorder(face_x, face_y, face_z) || (IsPositionInMap(face_x,face_z) && voxelMap[face_x,face_z].Item1 != (byte)VoxelData.VoxelType.Block); 
        if(voxelMap[x, z].Item1 == (byte)VoxelData.VoxelType.Fluid)
            return IsFaceInMapBorder(face_x, face_y, face_z) || (IsPositionInMap(face_x,face_z) && voxelMap[face_x,face_z].Item1 == (byte)VoxelData.VoxelType.None);
        return false;
    }

    // Checa se a face do voxel se encontra em uma das bordas do mapa
    bool IsFaceInMapBorder(int x, int y, int z){
        if(x == -1 || x == mapRows) return true;
        if(z == -1 || z == mapCols) return true;
        if(y == -1 || y == 1) return true;
        return false; 
    }

    // Remove todo o conteúdo das variáveis de geração dos blocos do mapa
    void ClearBlockMeshData(){
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    // Remove todo o conteúdo das variáveis de geração dos fluidos do mapa
    void ClearFluidMeshData(){
        for(int i = 0; i < fluidList.Count; i++){
            fluidVertexIndex[i] = 0;
            fluidVertices[i].Clear();
            fluidTriangles[i].Clear();
            fluidUvs[i].Clear();
        }
    }

    // Verifica se a posição se encontra dentro do mapa
    public bool IsPositionInMap(int x, int y){
        if(x < 0 || x >= mapRows) return false; 
        if(y < 0 || y >= mapCols) return false;
        return true;
    }

    public void UpdateVoxel(Vector2Int coord, byte voxelType, byte newBlockID = 0){
        if(!IsPositionInMap(coord.x, coord.y)) return;
        voxelMap[coord.x, coord.y].Item1 = voxelType;
        voxelMap[coord.x, coord.y].Item2 = newBlockID;
        ClearBlockMeshData();
        ClearFluidMeshData();
        for(int i = 0; i < mapRows; i++)
            for(int j = 0; j < mapCols; j++)
                if(voxelMap[i,j].Item1 != (byte)VoxelData.VoxelType.None)
                    UpdateMeshData(new Vector2Int(i,j));
        CreateBlockMesh();
        CreateFluidMesh();
    }

    public void UpdateAllVoxels(byte voxelType, byte voxelID){
        ClearBlockMeshData();
        ClearFluidMeshData();
        for(int i = 0; i < mapRows; i++){
            for(int j = 0; j < mapCols; j++){
                if(voxelMap[i,j].Item1 != (byte)VoxelData.VoxelType.None){
                    voxelMap[i, j].Item1 = voxelType;
                    voxelMap[i, j].Item2 = voxelID;
                    UpdateMeshData(new Vector2Int(i,j));
                }
            }
        }
        CreateBlockMesh();  
        CreateFluidMesh(); 
    }

    public void FloodFill(Vector2Int coord, byte targetVoxelType, byte targetVoxel, byte newVoxelType, byte newVoxel){
        UpdateVoxel(coord, newVoxelType, newVoxel);
        Vector2Int neighbor;
        for(int i = 0; i < VoxelData.movements.Length; i++){
            neighbor = new Vector2Int(coord.x + VoxelData.movements[i].x, coord.y + VoxelData.movements[i].y);
            if(IsPositionInMap(neighbor.x, neighbor.y) && voxelMap[neighbor.x, neighbor.y].Item1 == targetVoxelType && voxelMap[neighbor.x, neighbor.y].Item2 == targetVoxel)
                FloodFill(neighbor, targetVoxelType, targetVoxel, newVoxelType, newVoxel);
        }
    }
}