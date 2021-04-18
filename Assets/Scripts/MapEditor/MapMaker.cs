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
    public static int curBlockID;
    private readonly int hotbarSize = 6;

    public GameObject[] hotbar;
    public GameObject[] toolButtons;

    public static List<BlockType> blockList;

    public static Map map;
    public GameObject blockEditPrefab;
    public Transform blockEditMap;
    public static GameObject highlightBlock;

    public Sprite notSelected;
    public Sprite selected;
    public Sprite selectedTool;

    public Color notSelectedColor;
    public Color selectedColor;

    private GameObject[,] blockInstances;

    void Start(){
        // Inicializa ferramenta ativada
        curTool = Tool.None;
        // Recebe conteúdo da posição inicial da hotbar
        curSlotIndex = 0;
        curBlockID = hotbar[0].GetComponent<MapMakerSlot>().blockID;
        // Referencia objetos necessários para o controle dos blocos do mapa
        map = GameObject.Find("GameHandler").GetComponent<Map>();
        highlightBlock = GameObject.Find("BlockHighlight");
        highlightBlock.SetActive(false);
        blockList = GameObject.Find("DataHandler").GetComponent<BlockData>().blockList;
        // Inicializa hotbar de edição dos blocos
        for(int i = 0; i < hotbarSize; i++){
            int pos = i;
            int slotIndex = hotbar[i].GetComponent<MapMakerSlot>().index;
            int blockID = hotbar[i].GetComponent<MapMakerSlot>().blockID;
            hotbar[pos].GetComponentsInChildren<Image>()[0].enabled = true;
            hotbar[pos].GetComponentsInChildren<Image>()[0].sprite = notSelected;
            hotbar[pos].GetComponentsInChildren<Image>()[1].sprite = blockList[blockID].icon;
            hotbar[pos].GetComponent<Button>().onClick.AddListener(() => UpdateIndex(slotIndex));
        }
        // Altera sprite da posição inicial da hotbar
        hotbar[0].GetComponent<Image>().sprite = selected;
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

    void InitBlockEditorMap(){
        blockInstances = new GameObject[map.mapRows, map.mapCols];
        for(int i = 0; i < map.mapRows; i++){
            for(int j = 0; j < map.mapCols; j++){
                blockInstances[i,j] = Instantiate(blockEditPrefab, new Vector3(i + map.centerOffset, 0f, j + map.centerOffset), Quaternion.identity);
                blockInstances[i,j].name = i + "," + j;
                blockInstances[i,j].transform.SetParent(blockEditMap);
                blockInstances[i,j].GetComponent<PathCoord>().coord = new Vector2Int(i,j);
            }
        }
    }

    private void Update(){
        UpdatedIndexByScroll();
    }

    public void UpdateIndex(int slotIndex){
        if(curSlotIndex != slotIndex){
            curBlockID = hotbar[slotIndex].GetComponent<MapMakerSlot>().blockID;
            hotbar[curSlotIndex].GetComponent<Image>().sprite = notSelected;
            hotbar[slotIndex].GetComponent<Image>().sprite = selected;
            curSlotIndex = slotIndex;
        }
    }

    public void UpdatedIndexByScroll(){
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        newSlotIndex = curSlotIndex;
        if(scroll != 0){
            if(scroll > 0) newSlotIndex++;
            else newSlotIndex--;
            if(newSlotIndex > hotbarSize - 1) newSlotIndex = 0;
            if(newSlotIndex < 0) newSlotIndex = hotbarSize - 1;
        }

        UpdateIndex(newSlotIndex);
    }

    public static void PlaceHighlight(Vector2Int coord){
        if(coord != lastCoord){
            lastCoord = coord;
            highlightBlock.SetActive(true);
            highlightBlock.transform.position = new Vector3(coord.x + map.centerOffset, map.centerOffset, coord.y + map.centerOffset);
        }
    }

    public static void UpdateVoxel(Vector2Int coord){
        if(curTool == Tool.None) map.UpdateVoxel(coord, curBlockID);
        else if(curTool == Tool.FillAll) map.UpdateAllVoxels(curBlockID);
        else if(curTool == Tool.Bucket && map.voxelMap[coord.x, coord.y] != curBlockID) map.FloodFill(coord, map.voxelMap[coord.x, coord.y], curBlockID);
        else if(curTool == Tool.Erase) map.UpdateVoxel(coord, (int)BlockData.BlockEnum.Air);
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