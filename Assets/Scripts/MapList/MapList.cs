using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class MapList : MonoBehaviour{
    public Transform list;
    private Button addMapBtn;
    private Button removeMapBtn;
    private Button selectMapBtn;
    private int curIndex;
    public string curFilePath;
    public GameObject form;
    public GameObject mapInfoPrefab;
    private List<GameObject> mapInfoInstances;
    public Color selectedColor;
    public Color notSelectedColor;
    public static MapList singleton;

    void Awake(){
        if(singleton != null){
            Debug.LogWarning("Houve uma tentativa de criar 2 MapLists");
            Destroy(this);
        }
        singleton = this;
        mapInfoInstances = new List<GameObject>();

        GameObject panel = GameObject.Find("Panel");

        addMapBtn = panel.GetComponentsInChildren<Button>()[0];
        removeMapBtn = panel.GetComponentsInChildren<Button>()[1];
        selectMapBtn = panel.GetComponentsInChildren<Button>()[2];

        addMapBtn.onClick.AddListener(() => OpenForm());
        removeMapBtn.onClick.AddListener(() => RemoveMap());
        selectMapBtn.onClick.AddListener(() => SelectMap());

        ResetData();
    }

    public void OpenForm(){
        if(mapInfoInstances.Count == 8) return;
        form.SetActive(true);
    }

    public void ResetData(){
        curIndex = -1;
        curFilePath = "";
        removeMapBtn.interactable = false;
        selectMapBtn.interactable = false;
    }

    public void RemoveMap(){
        int count = mapInfoInstances.Count;
        for(int i = curIndex+1; i < count; i++){
            int newIndex = i-1; 
            mapInfoInstances[i].GetComponent<Button>().onClick.RemoveAllListeners();
            mapInfoInstances[i].GetComponent<Button>().onClick.AddListener(() => UpdateIndex(newIndex));
        } 
        mapInfoInstances[curIndex].GetComponent<MapSave>().DeleteFile();   // exclui save do mapa
        DestroyImmediate(mapInfoInstances[curIndex]);                      // destrói o game object que contém os dados do mapa
        mapInfoInstances.RemoveAt(curIndex);                               // remove o elemento da lista
        ResetData();
    }

    public void SelectMap(){
        SceneManager.LoadScene("MapMaker");
    }

    public void UpdateIndex(int index){
        if(curIndex == index){
            ChangeInstanceColor(index, false);
            curFilePath = "";
            ResetData();
        }

        else{
            if(curIndex != -1) ChangeInstanceColor(curIndex, false);
            ChangeInstanceColor(index, true);
            curIndex = index;
            curFilePath = mapInfoInstances[curIndex].GetComponent<MapSave>().filePath;
            removeMapBtn.interactable = true;
            selectMapBtn.interactable = true;
        }
    }

    public void ChangeInstanceColor(int index, bool selected){
        if(selected){
            mapInfoInstances[index].GetComponentInChildren<Text>().color = selectedColor;
            mapInfoInstances[index].GetComponentsInChildren<Image>()[1].color = selectedColor;
        }

        else{
            mapInfoInstances[index].GetComponentInChildren<Text>().color = notSelectedColor;
            mapInfoInstances[index].GetComponentsInChildren<Image>()[1].color = notSelectedColor; 
        }
    }

    public void AddMap(string name, int mapRows, int mapCols){
        GameObject newMap = Instantiate(mapInfoPrefab);
        int count = mapInfoInstances.Count;

        newMap.name = name;
        newMap.transform.SetParent(list);
        newMap.transform.localPosition = new Vector3(0f, 0f, 0f);
        newMap.GetComponentsInChildren<Text>()[1].text = name;                       // nome do mapa
        newMap.GetComponentsInChildren<Text>()[2].text = mapRows.ToString();         // quantidade de linhas
        newMap.GetComponentsInChildren<Text>()[3].text = mapCols.ToString();         // quantidade de colunas
        newMap.GetComponent<Button>().onClick.AddListener(() => UpdateIndex(count)); // atribui identificador para o mapa
        newMap.GetComponent<MapSave>().CreateFile(name, mapRows, mapCols);           // cria arquivo para armazenar dados do mapa
        mapInfoInstances.Add(newMap);                                                // insere na lista
        ChangeInstanceColor(count, false);                                           // define cor do item como "não selecionado"
    }
}
