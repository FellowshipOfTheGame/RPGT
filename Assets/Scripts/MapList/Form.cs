using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Form : MonoBehaviour{
    private InputField textName;
    private InputField textRows;
    private InputField textCols;
    public GameObject panel;

    private Button confirmButton;

    private void Start() {
        // Referencia elementos do formulário
        textName = this.GetComponentsInChildren<InputField>()[0];
        textRows = this.GetComponentsInChildren<InputField>()[1];
        textCols = this.GetComponentsInChildren<InputField>()[2];
        confirmButton = this.GetComponentsInChildren<Button>()[1];
        // Adiciona listeners para conclusão do formulário
        confirmButton.onClick.AddListener(() => Confirm());
    }

    private void Confirm(){
        // Checa se todos os campos foram preenchidos
        if(textName.text.Length == 0 || textRows.text.Length == 0 || textCols.text.Length == 0) return;
        // Recebe o conteúdo dos inputs
        string mapName = textName.text;
        int mapRows = Int32.Parse(textRows.text);
        int mapCols = Int32.Parse(textCols.text);
        // Checa valores inválidos
        if(mapRows <= 0 || mapCols <= 0) return;
        // Verifica se o nome já existe
        foreach(GameObject item in MapList.singleton.mapInfoInstances)
            if(item.name == mapName){
                return;
            }
        // Limpa conteúdo atual
        textName.text = "";
        textRows.text = "";
        textCols.text = "";
        // Desabilita painel do formulário
        panel.SetActive(false);
        // Cria instância na lista
        MapList.singleton.AddMap(mapName, mapRows, mapCols);
    }
}
