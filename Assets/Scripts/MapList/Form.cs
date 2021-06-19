using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Form : MonoBehaviour{

    string _mapName = "";
    int _mapRows = -1;
    int _mapCols = -1;

    public string mapName { set { _mapName = value; } }
    public string mapRows { set { _mapRows = ValidateSize(value); } }
    public string mapCols { set { _mapCols = ValidateSize(value); } }

    public Text invalid;

    int ValidateSize(string s) {
        int value = 0;
        try {
            value = Int32.Parse(s);
        } catch (Exception) {
            invalid.gameObject.SetActive(true);
        }

        if (value <= 0) {
            invalid.gameObject.SetActive(true);
            return -1;
        }

        return value;
    }

    public void Confirm(GameObject toBeDeactivated = null){
        invalid.gameObject.SetActive(true);
        if(_mapName.Length == 0 || _mapRows <= 0 || _mapCols <= 0) return;

        // Verifica se o nome já existe
        if (MapList.singleton.mapInfoInstances.Find(gameObj => gameObject.name == _mapName) != null) return;

        // Cria instância na lista
        try {
            MapList.singleton.AddMap(_mapName, _mapRows, _mapCols, true);
            invalid.gameObject.SetActive(false);
            toBeDeactivated.SetActive(false);
        } catch (ArgumentException) { }
    }
}
