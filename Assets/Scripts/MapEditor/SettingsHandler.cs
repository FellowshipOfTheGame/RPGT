using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsHandler : MonoBehaviour{
    private enum settingEnum{Save, ChangeSkyBox, Exit};
    private Button[] settingBtn;
    private MapSave mapSave;
    void Start(){
        mapSave = GameObject.Find("MapHandler").GetComponent<MapSave>();
        // Recupera informações básicas do mapa
        mapSave.ReadFile();
        // Referencia botões
        settingBtn = gameObject.GetComponentsInChildren<Button>();
        // Atribui listeners
        settingBtn[(int)settingEnum.Save].onClick.AddListener(() => Save());
        settingBtn[(int)settingEnum.ChangeSkyBox].onClick.AddListener(() => ChangeSkyBox());
        settingBtn[(int)settingEnum.Exit].onClick.AddListener(() => Exit());
    }

    void Save(){
        mapSave.SaveAll();        
    }

    void ChangeSkyBox(){

    }

    void Exit(){
        SceneManager.LoadScene("Room");
    }
}
