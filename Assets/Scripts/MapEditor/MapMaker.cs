using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour{
    public enum Tool {FillAll, Bucket, Erase, None};

    public static Vector2Int lastCoord;
    private int newSlotIndex;
    private int curSlotIndex;
    public static Tool curTool;
    public static byte curVoxelID;
    public static byte curVoxelType;
    private int hotbarSize;
    private List<GameObject> hotbar;
    public GameObject slotPrefab;
    public Transform slotList;
    public GameObject[] toolButtons;
    public List<GameObject> tabs;

    public static List<BlockType> blockList;
    public static List<FluidType> fluidList;

    public GameObject blockEditPrefab;
    public Transform blockEditMap;
    public static GameObject highlightBlock;

    public Sprite notSelected;
    public Sprite selected;
    public Sprite selectedTool;

    public Color notSelectedColor;
    public Color selectedColor;

    private GameObject[,] blockInstances;
    public static MapMaker singleton;

    void Start(){
        if(singleton != null){
            Debug.LogWarning("Houve uma tentativa de criar 2 MapMakers");
            Destroy(gameObject);
            return;
        }
        singleton = this;

        hotbar = new List<GameObject>();
        blockList = GameObject.Find("DataHandler").GetComponent<BlockData>().blockList;
        fluidList = GameObject.Find("DataHandler").GetComponent<BlockData>().fluidList;
        ChangeHotbarContent(VoxelData.VoxelType.Block);
        
        // Inicializa ferramenta ativada
        curTool = Tool.None;
        // Referencia objetos necessários para o controle dos blocos do mapa
        highlightBlock = GameObject.Find("BlockHighlight");
        highlightBlock?.SetActive(false);

        lastCoord = new Vector2Int(-1,-1);

        // Adiciona listeners para os botões das ferramentas
        for(int i = 0; i < toolButtons.Length; i++){
            int pos = i;
            toolButtons[i].GetComponent<Button>().onClick.AddListener(() => SelectTool((Tool)pos));
            toolButtons[i].GetComponentsInChildren<Image>()[1].enabled = false;
            toolButtons[i].GetComponentsInChildren<Text>()[0].enabled = false;
        }
        // Instancia blocos para permitir edição do mapa
        InitBlockEditorMap();
    }

    public void ChangeHotbarContent(VoxelData.VoxelType voxelType){
        // Atualiza cor dos ícones das abas
        foreach(GameObject tab in tabs){
            VoxelData.VoxelType tabContent = tab.GetComponent<Tab>().tabContent;
            Text tabIcon = tab.GetComponentInChildren<Text>();
            tabIcon.color = (tabContent != voxelType) ? new Color(1, 1, 1) : selectedColor;
        }

        // Limpa instancias anteriores
        foreach(GameObject item in hotbar){
            DestroyImmediate(item);
        }

        hotbar.Clear();
        hotbarSize = 0;
        curVoxelType = (byte)voxelType;

        // Recebe o tamanho da lista para instanciar os slots da toolbar
        if(voxelType == VoxelData.VoxelType.Block) hotbarSize = blockList.Count;
        else if(voxelType == VoxelData.VoxelType.Fluid) hotbarSize = fluidList.Count;
        else if(voxelType == VoxelData.VoxelType.Prop) hotbarSize = PropData.singleton.propList.Count;

        // Inicializa toolbar de edição dos blocos
        for(int i = 0; i < hotbarSize; i++){
            int pos = i;
            // Instancia slot
            hotbar.Add(Instantiate(slotPrefab));
            hotbar[i].transform.SetParent(slotList);
            hotbar[i].transform.localPosition = new Vector3(0f, 0f, 0f);
            hotbar[i].name = "Slot " + i;
            // Armazena índice do slot e ID do bloco
            hotbar[i].GetComponent<MapMakerSlot>().index = pos;
            hotbar[i].GetComponent<MapMakerSlot>().blockID = pos;
            // Armazena valores novamente, desta vez para facilitar uso 
            int slotIndex = hotbar[i].GetComponent<MapMakerSlot>().index;
            int blockID = hotbar[i].GetComponent<MapMakerSlot>().blockID;
            // Define sprite dos slots como não selecionado
            hotbar[pos].GetComponentsInChildren<Image>()[0].enabled = true;
            hotbar[pos].GetComponentsInChildren<Image>()[0].sprite = notSelected;
            if(voxelType == VoxelData.VoxelType.Block)
                hotbar[pos].GetComponentsInChildren<Image>()[1].sprite = blockList[blockID].icon;
            else if(voxelType == VoxelData.VoxelType.Fluid)
                hotbar[pos].GetComponentsInChildren<Image>()[1].sprite = fluidList[blockID].icon;
            // Adiciona listener para modificar índice ativo
            hotbar[pos].GetComponent<Button>().onClick.AddListener(() => UpdateIndex(slotIndex));
        }

        // Recebe conteúdo da posição inicial da hotbar
        curSlotIndex = 0;
        curVoxelID = (byte)hotbar[0].GetComponent<MapMakerSlot>().blockID;

        // Altera sprite da posição inicial da hotbar
        hotbar[0].GetComponent<Image>().sprite = selected;

        // Ajusta posição da lista
        slotList.transform.localPosition = new Vector3(50f, 0f, 0f);
    }

    void InitBlockEditorMap(){
        blockInstances = new GameObject[Map.singleton.mapRows, Map.singleton.mapCols];
        for(int i = 0; i < Map.singleton.mapRows; i++){
            for(int j = 0; j < Map.singleton.mapCols; j++){
                blockInstances[i,j] = Instantiate(blockEditPrefab, new Vector3(i + Map.singleton.centerOffset, 0f, j + Map.singleton.centerOffset), Quaternion.identity);
                blockInstances[i,j].name = i + "," + j;
                blockInstances[i,j].transform.SetParent(blockEditMap);
                blockInstances[i,j].GetComponent<PathCoord>().coord = new Vector2Int(i,j);
            }
        }
    }

    public void UpdateIndex(int slotIndex){
        if(curSlotIndex != slotIndex){
            curVoxelID = (byte)hotbar[slotIndex].GetComponent<MapMakerSlot>().blockID;
            hotbar[curSlotIndex].GetComponent<Image>().sprite = notSelected;
            hotbar[slotIndex].GetComponent<Image>().sprite = selected;
            curSlotIndex = slotIndex;
        }
    }
    
    public static void PlaceHighlight(Vector2Int coord){
        if(coord != lastCoord){
            lastCoord = coord;
            highlightBlock.SetActive(true);
            highlightBlock.transform.position = new Vector3(coord.x + Map.singleton.centerOffset, Map.singleton.centerOffset, coord.y + Map.singleton.centerOffset);
        }
    }

    public static void UpdateVoxel(Vector2Int coord){
        if(curVoxelType == (int)VoxelData.VoxelType.Block || curVoxelType == (int)VoxelData.VoxelType.Fluid){
            if(curTool == Tool.None) 
                Map.singleton.UpdateVoxel(coord, curVoxelType, curVoxelID);
            else if(curTool == Tool.FillAll) 
                Map.singleton.UpdateAllVoxels(curVoxelType, curVoxelID);
            else if(curTool == Tool.Bucket && (Map.singleton.voxelMap[coord.x, coord.y].Item1 != curVoxelType || Map.singleton.voxelMap[coord.x, coord.y].Item2 != curVoxelID)) 
                Map.singleton.FloodFill(coord, Map.singleton.voxelMap[coord.x, coord.y].Item1, Map.singleton.voxelMap[coord.x, coord.y].Item2, curVoxelType, curVoxelID);
            else if(curTool == Tool.Erase) 
                Map.singleton.UpdateVoxel(coord, (int)VoxelData.VoxelType.None);
        }

        else if(curVoxelType == (int)VoxelData.VoxelType.Prop){
            if(curTool == Tool.Erase) PropMap.singleton.RemoveProp(coord);
            else PropMap.singleton.AddProp(coord, curVoxelID); 
        }
    }
    
    void SelectTool(Tool tool){
        int num_tool = (int)tool;
        if((int)curTool == num_tool){
            toolButtons[(int)curTool].GetComponent<Image>().sprite = notSelected;
            toolButtons[(int)curTool].GetComponentsInChildren<Text>()[1].color = notSelectedColor;
            curTool = Tool.None;
        }

        else{
            if(curTool != Tool.None){
                toolButtons[(int)curTool].GetComponent<Image>().sprite = notSelected;
                toolButtons[(int)curTool].GetComponentsInChildren<Text>()[1].color = notSelectedColor;
            }
            toolButtons[(int)tool].GetComponent<Image>().sprite = selectedTool;
            toolButtons[(int)tool].GetComponentsInChildren<Text>()[1].color = selectedColor;
            curTool = tool;
        }
    }
}